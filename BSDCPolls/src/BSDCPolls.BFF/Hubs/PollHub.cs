using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSDCPolls.Contracts.Responses.Polls;
using BSDCPolls.Contracts.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BSDCPolls.BFF.Hubs;

/// <summary>
/// SignalR hub for real-time poll sessions. Participants join the poll group;
/// the creator receives live vote counts; all group members receive status changes.
/// </summary>
[Authorize]
public sealed class PollHub : Hub
{
    private const string VoteDuplicate = "VOTE_DUPLICATE";
    private const string VoteNotAuthorized = "VOTE_NOT_AUTHORIZED";
    private const string PollNotActive = "POLL_NOT_ACTIVE";
    private const string InternalError = "INTERNAL_ERROR";

    private readonly IPollSessionTracker _sessionTracker;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PollHub> _logger;

    /// <summary>Initialises the hub with session tracker, HTTP client factory, and logger.</summary>
    public PollHub(
        IPollSessionTracker sessionTracker,
        IHttpClientFactory httpClientFactory,
        ILogger<PollHub> logger)
    {
        _sessionTracker = sessionTracker;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        var pollUidStr = Context.GetHttpContext()?.Request.Query["pollUid"].ToString();

        if (!Guid.TryParse(pollUidStr, out var pollUid))
        {
            throw new HubException("Invalid or missing pollUid query parameter.");
        }

        var bearerToken = GetBearerToken();
        if (string.IsNullOrEmpty(bearerToken))
        {
            throw new HubException(VoteNotAuthorized);
        }

        var pollDetail = await GetPollDetailAsync(pollUid, bearerToken);
        if (pollDetail is null)
        {
            throw new HubException("Poll not found or access denied.");
        }

        Context.Items["pollUid"] = pollUid;
        Context.Items["isCreator"] = pollDetail.IsCreator;

        await Groups.AddToGroupAsync(Context.ConnectionId, pollUid.ToString());

        if (pollDetail.IsCreator)
        {
            _sessionTracker.RegisterCreator(pollUid, Context.ConnectionId);
        }

        _logger.LogInformation(
            "Client {ConnectionId} joined poll {PollUid} (isCreator={IsCreator})",
            Context.ConnectionId,
            pollUid,
            pollDetail.IsCreator);

        await base.OnConnectedAsync();
    }

    /// <inheritdoc />
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items.TryGetValue("pollUid", out var uid) && uid is Guid pollUid)
        {
            _sessionTracker.UnregisterCreator(pollUid, Context.ConnectionId);
        }

        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Submits a vote for <paramref name="questionUid"/> selecting <paramref name="selectedOptionUid"/>.
    /// On success broadcasts updated vote counts to the creator connection only.
    /// Throws a typed <see cref="HubException"/> for client-distinguishable errors.
    /// </summary>
    public async Task SubmitVote(Guid questionUid, Guid selectedOptionUid)
    {
        if (!Context.Items.TryGetValue("pollUid", out var uid) || uid is not Guid pollUid)
        {
            throw new HubException(InternalError);
        }

        var bearerToken = GetBearerToken();
        if (string.IsNullOrEmpty(bearerToken))
        {
            throw new HubException(VoteNotAuthorized);
        }

        var client = _httpClientFactory.CreateClient("InternalApi");
        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/internal/polls/{pollUid}/submissions");

        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        requestMessage.Content = JsonContent.Create(new { questionUid, selectedOptionUid });

        HttpResponseMessage response;
        try
        {
            response = await client.SendAsync(requestMessage);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Vote submission HTTP error for poll {PollUid}", pollUid);
            throw new HubException(InternalError);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new HubException(VoteDuplicate);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new HubException(VoteNotAuthorized);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            throw new HubException(PollNotActive);
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Vote submission returned {Status} for poll {PollUid}", response.StatusCode, pollUid);
            throw new HubException(InternalError);
        }

        // Broadcast updated counts to creator only
        var creatorConnectionId = _sessionTracker.GetCreatorConnectionId(pollUid);
        if (!string.IsNullOrEmpty(creatorConnectionId))
        {
            var results = await GetPollResultsAsync(pollUid, bearerToken);
            if (results is not null)
            {
                var questionResult = results.Questions.FirstOrDefault(q => q.QuestionUid == questionUid);
                if (questionResult is not null)
                {
                    var payload = new PollVoteCountUpdatedPayload(pollUid, questionUid, questionResult.Options);
                    await Clients.Client(creatorConnectionId).SendAsync("VoteCountUpdated", payload);
                }
            }
        }
    }

    private string? GetBearerToken() =>
        Context.GetHttpContext()?.Request.Query["access_token"].ToString()
            is { Length: > 0 } token ? token : null;

    private async Task<PollDetailResponse?> GetPollDetailAsync(Guid pollUid, string bearerToken)
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/internal/polls/{pollUid}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PollDetailResponse>();
    }

    private async Task<PollResultsResponse?> GetPollResultsAsync(Guid pollUid, string bearerToken)
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/internal/polls/{pollUid}/results");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<PollResultsResponse>();
    }
}
