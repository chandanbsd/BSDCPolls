using System.Net.Http.Headers;
using System.Net.Http.Json;
using BSDCPolls.Contracts.Responses.Notifications;
using Microsoft.Extensions.Logging;

namespace BSDCPolls.BFF.Business.Notifications;

/// <summary>Forwards notification requests from the BFF to the internal API.</summary>
public sealed class BffNotificationService : IBffNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<BffNotificationService> _logger;

    /// <summary>Initialises the service with the HTTP client factory and logger.</summary>
    public BffNotificationService(IHttpClientFactory httpClientFactory, ILogger<BffNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<NotificationListResponse> GetNotificationsAsync(
        bool unreadOnly,
        int page,
        int pageSize,
        string bearerToken,
        CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var query = $"/api/internal/notifications?unreadOnly={unreadOnly}&page={page}&pageSize={pageSize}";
        var response = await client.GetAsync(query, ct);
        await EnsureSuccessAsync(response, "Get notifications", ct);
        return (await response.Content.ReadFromJsonAsync<NotificationListResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task<NotificationReadResponse> MarkReadAsync(Guid notificationUid, string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PatchAsync($"/api/internal/notifications/{notificationUid}/read", null, ct);
        await EnsureSuccessAsync(response, "Mark notification read", ct);
        return (await response.Content.ReadFromJsonAsync<NotificationReadResponse>(ct))!;
    }

    /// <inheritdoc />
    public async Task MarkAllReadAsync(string bearerToken, CancellationToken ct = default)
    {
        var client = CreateAuthenticatedClient(bearerToken);
        var response = await client.PatchAsync("/api/internal/notifications/read-all", null, ct);
        await EnsureSuccessAsync(response, "Mark all notifications read", ct);
    }

    /// <inheritdoc />
    public async Task<int> GetUnreadCountAsync(string bearerToken, CancellationToken ct = default)
    {
        var list = await GetNotificationsAsync(unreadOnly: true, page: 1, pageSize: 1, bearerToken, ct);
        return list.UnreadCount;
    }

    private HttpClient CreateAuthenticatedClient(string bearerToken)
    {
        var client = _httpClientFactory.CreateClient("InternalApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return client;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, string operation, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        _logger.LogWarning("{Operation} failed ({Status}): {Body}", operation, (int)response.StatusCode, body);
        response.EnsureSuccessStatusCode();
    }
}
