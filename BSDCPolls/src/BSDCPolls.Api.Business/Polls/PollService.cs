using BSDCPolls.Api.Data.Entities;
using BSDCPolls.Api.Data.Repositories;
using BSDCPolls.Contracts.Enums;
using BSDCPolls.Contracts.Requests.Polls;
using BSDCPolls.Contracts.Responses.Polls;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.Api.Business.Polls;

/// <summary>Domain service for poll lifecycle, question management, and voting.</summary>
public sealed class PollService : IPollService
{
    private readonly IPollRepository _pollRepository;
    private readonly IPollSubmissionRepository _submissionRepository;
    private readonly ILogger<PollService> _logger;

    /// <summary>Initialises the service with required repositories and logger.</summary>
    public PollService(
        IPollRepository pollRepository,
        IPollSubmissionRepository submissionRepository,
        ILogger<PollService> logger)
    {
        _pollRepository = pollRepository;
        _submissionRepository = submissionRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PollDetailResponse> CreateAsync(CreatePollRequest request, int creatorId, CancellationToken ct = default)
    {
        var poll = Poll.Create(request.Title, request.IsPublic, creatorId);
        await _pollRepository.CreateAsync(poll, ct);

        _logger.LogInformation("Poll {PollUid} created by user {UserId}", poll.Uid, creatorId);

        return MapToDetailResponse(poll, isCreator: true);
    }

    /// <inheritdoc />
    public async Task<PollDetailResponse> GetByUidAsync(Guid uid, int requestingUserId, CancellationToken ct = default)
    {
        var poll = await _pollRepository.GetByUidAsync(uid, requestingUserId, ct)
            ?? throw new KeyNotFoundException($"Poll {uid} not found.");

        var isCreator = poll.CreatorId == requestingUserId;
        if (!isCreator && !poll.IsPublic)
        {
            throw new UnauthorizedAccessException($"User {requestingUserId} is not authorized to view poll {uid}.");
        }

        return MapToDetailResponse(poll, isCreator);
    }

    /// <inheritdoc />
    public async Task<PollFeedResponse> GetFeedAsync(
        int userId,
        bool showPublic,
        PollStatus? status,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _pollRepository.GetFeedAsync(userId, showPublic, status, page, pageSize, ct);

        var feedItems = items.Select(p => new PollFeedItem(
            p.Uid,
            p.Title,
            p.IsPublic,
            p.Status,
            p.Creator.Username,
            p.Questions.Count,
            p.CreatedOn,
            null)).ToList();

        return new PollFeedResponse(feedItems, totalCount, page, pageSize);
    }

    /// <inheritdoc />
    public async Task<PollQuestionResponse> AddQuestionAsync(
        Guid pollUid,
        AddPollQuestionRequest request,
        int creatorId,
        CancellationToken ct = default)
    {
        var poll = await _pollRepository.GetByUidAsync(pollUid, creatorId, ct)
            ?? throw new KeyNotFoundException($"Poll {pollUid} not found.");

        if (poll.CreatorId != creatorId)
        {
            throw new UnauthorizedAccessException($"User {creatorId} is not the creator of poll {pollUid}.");
        }

        var orderIndex = poll.Questions.Count;
        var question = PollQuestion.Create(poll.Id, request.Text, orderIndex);

        foreach (var opt in request.Options)
        {
            var option = PollAnswerOption.Create(0, opt.Text, opt.OrderIndex);
            question.AnswerOptions.Add(option);
        }

        if (request.PushImmediately)
        {
            question.MarkPushed(DateTime.UtcNow);
        }

        poll.Questions.Add(question);
        await _pollRepository.UpdateAsync(poll, ct);

        _logger.LogInformation(
            "Question {QuestionUid} added to poll {PollUid} by user {UserId} (pushed={Pushed})",
            question.Uid,
            pollUid,
            creatorId,
            request.PushImmediately);

        return MapToQuestionResponse(question);
    }

    /// <inheritdoc />
    public async Task<PollQuestionResponse> PushQuestionAsync(Guid pollUid, Guid questionUid, int creatorId, CancellationToken ct = default)
    {
        var poll = await _pollRepository.GetByUidAsync(pollUid, creatorId, ct)
            ?? throw new KeyNotFoundException($"Poll {pollUid} not found.");

        if (poll.CreatorId != creatorId)
        {
            throw new UnauthorizedAccessException($"User {creatorId} is not the creator of poll {pollUid}.");
        }

        if (poll.Status != PollStatus.Active)
        {
            throw new InvalidOperationException($"Poll {pollUid} must be Active to push questions (current status: {poll.Status}).");
        }

        var question = poll.Questions.FirstOrDefault(q => q.Uid == questionUid)
            ?? throw new KeyNotFoundException($"Question {questionUid} not found in poll {pollUid}.");

        question.MarkPushed(DateTime.UtcNow);
        await _pollRepository.UpdateAsync(poll, ct);

        _logger.LogInformation("Question {QuestionUid} pushed on poll {PollUid} by user {UserId}", questionUid, pollUid, creatorId);

        return MapToQuestionResponse(question);
    }

    /// <inheritdoc />
    public async Task<PollDetailResponse> ChangeStatusAsync(Guid pollUid, PollStatus newStatus, int creatorId, CancellationToken ct = default)
    {
        var poll = await _pollRepository.GetByUidAsync(pollUid, creatorId, ct)
            ?? throw new KeyNotFoundException($"Poll {pollUid} not found.");

        if (poll.CreatorId != creatorId)
        {
            throw new UnauthorizedAccessException($"User {creatorId} is not the creator of poll {pollUid}.");
        }

        if (newStatus == PollStatus.Active)
        {
            poll.Activate();
        }
        else if (newStatus == PollStatus.Closed)
        {
            poll.Close();
        }

        await _pollRepository.UpdateAsync(poll, ct);

        _logger.LogInformation("Poll {PollUid} status changed to {Status} by user {UserId}", pollUid, newStatus, creatorId);

        return MapToDetailResponse(poll, isCreator: true);
    }

    /// <inheritdoc />
    public async Task<PollSubmissionResponse> SubmitVoteAsync(Guid pollUid, SubmitPollVoteRequest request, int respondentId, CancellationToken ct = default)
    {
        var poll = await _pollRepository.GetByUidAsync(pollUid, respondentId, ct)
            ?? throw new KeyNotFoundException($"Poll {pollUid} not found.");

        if (poll.Status != PollStatus.Active)
        {
            throw new InvalidOperationException($"Poll {pollUid} is not active.");
        }

        var question = poll.Questions.FirstOrDefault(q => q.Uid == request.QuestionUid)
            ?? throw new KeyNotFoundException($"Question {request.QuestionUid} not found.");

        if (!question.PushedAt.HasValue)
        {
            throw new InvalidOperationException($"Question {request.QuestionUid} has not been pushed yet.");
        }

        var option = question.AnswerOptions.FirstOrDefault(o => o.Uid == request.SelectedOptionUid)
            ?? throw new KeyNotFoundException($"Option {request.SelectedOptionUid} not found.");

        var alreadySubmitted = await _submissionRepository.HasUserSubmittedAsync(question.Id, respondentId, ct);
        if (alreadySubmitted)
        {
            throw new InvalidOperationException($"User {respondentId} has already voted on question {request.QuestionUid}.");
        }

        var submission = PollSubmission.Create(question.Id, option.Id, respondentId);
        await _submissionRepository.CreateAsync(submission, ct);

        _logger.LogInformation(
            "Vote submitted by user {UserId} on question {QuestionUid} in poll {PollUid}",
            respondentId,
            request.QuestionUid,
            pollUid);

        return new PollSubmissionResponse(submission.Uid, question.Uid, option.Uid, submission.CreatedOn);
    }

    /// <inheritdoc />
    public async Task<PollResultsResponse> GetResultsAsync(Guid pollUid, int requestingUserId, CancellationToken ct = default)
    {
        var poll = await _pollRepository.GetWithSubmissionsAsync(pollUid, requestingUserId, ct)
            ?? throw new UnauthorizedAccessException($"Poll {pollUid} not found or user {requestingUserId} is not the creator.");

        var questionResults = new List<PollResultsQuestionResponse>();

        foreach (var question in poll.Questions.OrderBy(q => q.OrderIndex))
        {
            var voteCounts = await _submissionRepository.GetVoteCountsAsync(question.Id, ct);
            var totalVotes = voteCounts.Values.Sum();

            var optionResults = question.AnswerOptions
                .OrderBy(o => o.OrderIndex)
                .Select(o =>
                {
                    var count = voteCounts.TryGetValue(o.Uid, out var c) ? c : 0;
                    var pct = totalVotes > 0 ? Math.Round((decimal)count / totalVotes * 100, 2) : 0m;
                    return new PollResultsOptionResponse(o.Uid, o.Text, count, pct);
                })
                .ToList();

            questionResults.Add(new PollResultsQuestionResponse(
                question.Uid,
                question.Text,
                question.PushedAt,
                totalVotes,
                optionResults));
        }

        return new PollResultsResponse(poll.Uid, poll.Title, poll.Status, questionResults);
    }

    private static PollDetailResponse MapToDetailResponse(Poll poll, bool isCreator) =>
        new(
            poll.Uid,
            poll.Title,
            poll.IsPublic,
            poll.Status,
            poll.CreatedOn,
            isCreator,
            poll.Questions.OrderBy(q => q.OrderIndex).Select(MapToQuestionResponse).ToList());

    private static PollQuestionResponse MapToQuestionResponse(PollQuestion q) =>
        new(
            q.Uid,
            q.Text,
            q.OrderIndex,
            q.PushedAt,
            q.AnswerOptions.OrderBy(o => o.OrderIndex)
                .Select(o => new PollAnswerOptionResponse(o.Uid, o.Text, o.OrderIndex))
                .ToList());
}
