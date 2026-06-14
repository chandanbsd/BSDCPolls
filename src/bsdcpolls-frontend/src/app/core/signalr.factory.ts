import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { AuthStore } from '../store/auth.store';

/**
 * Creates and configures a SignalR hub connection.
 *
 * The access token is sourced from the {@link AuthStore} so that the JWT is
 * always the current (non-expired) token at connection time.
 * The token is also passed as `?access_token=` in the query string so that
 * the BFF's JwtBearer middleware can authenticate WebSocket upgrade requests.
 *
 * @param hubUrl Relative URL of the SignalR hub endpoint (e.g. '/hubs/poll').
 * @param authStore The NgRx Signal Store slice holding the current auth state.
 * @returns A configured but not-yet-started {@link HubConnection}.
 */
export function createHubConnection(hubUrl: string, authStore: AuthStore): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(hubUrl, {
      accessTokenFactory: () => authStore.accessToken() ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
