using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Documents;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Surveys;
using BSDCPolls.Contracts.Responses.Surveys;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.Api.Business.Surveys;

/// <summary>Domain service for survey lifecycle, question tree management, and response collection.</summary>
public sealed class SurveyService : ISurveyService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private readonly ISurveyRepository _surveyRepository;
    private readonly ISurveyResponseRepository _responseRepository;
    private readonly ISurveyDocumentRepository _documentRepository;
    private readonly ILogger<SurveyService> _logger;

    /// <summary>Initialises the service with required repositories and logger.</summary>
    public SurveyService(
        ISurveyRepository surveyRepository,
        ISurveyResponseRepository responseRepository,
        ISurveyDocumentRepository documentRepository,
        ILogger<SurveyService> logger)
    {
        _surveyRepository = surveyRepository;
        _responseRepository = responseRepository;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> CreateAsync(CreateSurveyRequest request, int creatorId, CancellationToken ct = default)
    {
        var survey = Survey.Create(request.Title, request.IsPublic, creatorId, request.QuestionTree);
        await _surveyRepository.CreateAsync(survey, ct);

        _logger.LogInformation("Survey {SurveyUid} created by user {UserId}", survey.Uid, creatorId);

        return MapToDetailResponse(survey, isCreator: true);
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> GetByUidAsync(Guid uid, int requestingUserId, CancellationToken ct = default)
    {
        var survey = await _surveyRepository.GetByUidAsync(uid, requestingUserId, ct)
            ?? throw new KeyNotFoundException($"Survey {uid} not found.");

        var isCreator = survey.CreatorId == requestingUserId;
        if (!isCreator && !survey.IsPublic)
        {
            throw new UnauthorizedAccessException($"User {requestingUserId} is not authorized to view survey {uid}.");
        }

        return MapToDetailResponse(survey, isCreator);
    }

    /// <inheritdoc />
    public async Task<SurveyFeedResponse> GetFeedAsync(
        int userId,
        bool showPublic,
        SurveyStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _surveyRepository.GetFeedAsync(userId, showPublic, status, page, pageSize, ct);

        var feedItems = items.Select(s => new SurveyFeedItem(
            s.Uid,
            s.Title,
            s.IsPublic,
            s.Status,
            s.Creator.Username,
            s.QuestionTree.Questions.Count,
            s.CreatedOn,
            null)).ToList();

        return new SurveyFeedResponse(feedItems, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> ChangeStatusAsync(Guid surveyUid, SurveyStatus newStatus, int creatorId, CancellationToken ct = default)
    {
        var survey = await _surveyRepository.GetByUidAsync(surveyUid, creatorId, ct)
            ?? throw new KeyNotFoundException($"Survey {surveyUid} not found.");

        if (survey.CreatorId != creatorId)
        {
            throw new UnauthorizedAccessException($"User {creatorId} is not the creator of survey {surveyUid}.");
        }

        if (newStatus == SurveyStatus.Published)
        {
            survey.Publish();
        }
        else if (newStatus == SurveyStatus.Closed)
        {
            survey.Close();
        }

        await _surveyRepository.UpdateAsync(survey, ct);

        _logger.LogInformation(
            "Survey {SurveyUid} status changed to {Status} by user {UserId}",
            surveyUid,
            newStatus,
            creatorId);

        return MapToDetailResponse(survey, isCreator: true);
    }

    /// <inheritdoc />
    public async Task<SurveyDetailResponse> UpdateQuestionsAsync(Guid surveyUid, UpdateSurveyQuestionsRequest request, int creatorId, CancellationToken ct = default)
    {
        var survey = await _surveyRepository.GetByUidAsync(surveyUid, creatorId, ct)
            ?? throw new KeyNotFoundException($"Survey {surveyUid} not found.");

        if (survey.CreatorId != creatorId)
        {
            throw new UnauthorizedAccessException($"User {creatorId} is not the creator of survey {surveyUid}.");
        }

        survey.UpdateQuestionTree(request.QuestionTree);
        await _surveyRepository.UpdateAsync(survey, ct);

        _logger.LogInformation("Survey {SurveyUid} question tree updated by user {UserId}", surveyUid, creatorId);

        return MapToDetailResponse(survey, isCreator: true);
    }

    /// <inheritdoc />
    public async Task<SurveyResponseStatusResponse> SaveResponseAsync(
        Guid surveyUid,
        SaveSurveyResponseRequest request,
        int respondentId,
        CancellationToken ct = default)
    {
        var survey = await _surveyRepository.GetByUidAsync(surveyUid, respondentId, ct)
            ?? throw new KeyNotFoundException($"Survey {surveyUid} not found.");

        if (survey.Status != SurveyStatus.Published)
        {
            throw new InvalidOperationException($"Survey {surveyUid} is not open for responses.");
        }

        var answersDoc = new SurveyAnswersDocument(request.Answers);

        var response = await _responseRepository.GetByRespondentAsync(surveyUid, respondentId, ct);
        if (response == null)
        {
            response = SurveyResponse.Create(survey.Id, respondentId);
            if (request.IsSubmitting)
            {
                response.Submit(answersDoc, DateTime.UtcNow);
            }
            else
            {
                response.SaveProgress(answersDoc);
            }

            await _responseRepository.CreateAsync(response, ct);
        }
        else
        {
            if (response.IsComplete)
            {
                throw new InvalidOperationException($"Survey response for survey {surveyUid} has already been submitted.");
            }

            if (request.IsSubmitting)
            {
                response.Submit(answersDoc, DateTime.UtcNow);
            }
            else
            {
                response.SaveProgress(answersDoc);
            }

            await _responseRepository.UpdateAsync(response, ct);
        }

        _logger.LogInformation(
            "Survey response {ResponseUid} saved for survey {SurveyUid} by user {UserId} (submitted={Submitted})",
            response.Uid,
            surveyUid,
            respondentId,
            request.IsSubmitting);

        return new SurveyResponseStatusResponse(response.Uid, response.IsComplete, response.SubmittedAt);
    }

    /// <inheritdoc />
    public async Task<SurveyDocumentResponse> UploadDocumentAsync(
        Guid surveyUid,
        Guid responseUid,
        Stream pdfStream,
        string fileName,
        long fileSize,
        Guid questionUid,
        int respondentId,
        CancellationToken ct = default)
    {
        if (fileSize > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size {fileSize} bytes exceeds the 10 MB limit.");
        }

        var response = await _responseRepository.GetByRespondentAsync(surveyUid, respondentId, ct)
            ?? throw new KeyNotFoundException($"Survey response not found for survey {surveyUid}.");

        if (response.Uid != responseUid)
        {
            throw new UnauthorizedAccessException($"User {respondentId} does not own response {responseUid}.");
        }

        using var ms = new MemoryStream();
        await pdfStream.CopyToAsync(ms, ct);
        var fileData = ms.ToArray();

        var document = SurveyDocument.Create(response.Id, questionUid, fileName, fileSize, fileData);
        await _documentRepository.CreateAsync(document, ct);

        _logger.LogInformation(
            "Document {DocumentUid} uploaded for question {QuestionUid} in survey {SurveyUid} by user {UserId}",
            document.Uid,
            questionUid,
            surveyUid,
            respondentId);

        return new SurveyDocumentResponse(document.Uid, document.FileName, document.FileSizeBytes);
    }

    /// <inheritdoc />
    public async Task<SurveyResultsResponse> GetResultsAsync(Guid surveyUid, int requestingUserId, CancellationToken ct = default)
    {
        var survey = await _surveyRepository.GetResultsAsync(surveyUid, requestingUserId, ct)
            ?? throw new UnauthorizedAccessException($"Survey {surveyUid} not found or user {requestingUserId} is not the creator.");

        var allResponses = survey.Responses.ToList();
        var totalResponses = allResponses.Count;
        var completeResponses = allResponses.Count(r => r.IsComplete);

        var summaries = BuildQuestionSummaries(survey.QuestionTree.Questions, allResponses);

        return new SurveyResultsResponse(
            survey.Uid,
            survey.Title,
            survey.Status,
            totalResponses,
            completeResponses,
            summaries);
    }

    private static IReadOnlyList<SurveyQuestionSummary> BuildQuestionSummaries(
        IReadOnlyList<SurveyQuestionNode> questions,
        IReadOnlyList<Data.Entities.SurveyResponse> responses)
    {
        var summaries = new List<SurveyQuestionSummary>();

        foreach (var node in questions)
        {
            var allAnswers = responses
                .SelectMany(r => r.AnswersJson.Answers)
                .Where(a => a.QuestionUid == node.Uid)
                .ToList();

            var choiceTallies = new List<SurveyChoiceTally>();
            var textAnswers = new List<string>();
            var documentCount = 0;

            if (node.AnswerType == SurveyAnswerType.MultipleChoice && node.Choices != null)
            {
                foreach (var choice in node.Choices)
                {
                    var count = allAnswers.Count(a => a.SelectedChoiceUid == choice.Uid);
                    choiceTallies.Add(new SurveyChoiceTally(choice.Uid, choice.Text, count));
                }
            }
            else if (node.AnswerType == SurveyAnswerType.ShortText || node.AnswerType == SurveyAnswerType.LongText)
            {
                textAnswers.AddRange(allAnswers
                    .Where(a => !string.IsNullOrWhiteSpace(a.TextValue))
                    .Select(a => a.TextValue!));
            }
            else if (node.AnswerType == SurveyAnswerType.DocumentUpload)
            {
                documentCount = responses
                    .SelectMany(r => r.Documents)
                    .Count(d => d.QuestionUid == node.Uid);
            }

            summaries.Add(new SurveyQuestionSummary(
                node.Uid,
                node.Text,
                node.AnswerType,
                allAnswers.Count,
                choiceTallies,
                textAnswers,
                documentCount));

            if (node.Branches != null)
            {
                foreach (var branch in node.Branches)
                {
                    summaries.AddRange(BuildQuestionSummaries(branch.Questions, responses));
                }
            }
        }

        return summaries;
    }

    private static SurveyDetailResponse MapToDetailResponse(Survey survey, bool isCreator) =>
        new(
            survey.Uid,
            survey.Title,
            survey.IsPublic,
            survey.Status,
            survey.QuestionTree,
            survey.CreatedOn,
            isCreator);
}
