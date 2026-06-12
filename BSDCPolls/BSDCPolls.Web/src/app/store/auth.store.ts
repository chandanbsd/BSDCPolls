import { inject } from '@angular/core';
import { signalStore, withState, withMethods, patchState } from '@ngrx/signals';
import { AuthService, LoginRequest, RegisterRequest } from '../core/auth/auth.service';

const SESSION_KEY = 'bsdcpolls_auth';

interface AuthState {
  userUid: string | null;
  username: string | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface PersistedAuth {
  userUid: string;
  username: string;
  accessToken: string;
}

function loadFromSession(): Partial<AuthState> {
  try {
    const raw = sessionStorage.getItem(SESSION_KEY);
    if (!raw) return {};
    const stored: PersistedAuth = JSON.parse(raw);
    return {
      userUid: stored.userUid,
      username: stored.username,
      accessToken: stored.accessToken,
      isAuthenticated: true,
    };
  } catch {
    return {};
  }
}

function saveToSession(userUid: string, username: string, accessToken: string): void {
  const data: PersistedAuth = { userUid, username, accessToken };
  sessionStorage.setItem(SESSION_KEY, JSON.stringify(data));
}

function clearSession(): void {
  sessionStorage.removeItem(SESSION_KEY);
}

const initialState: AuthState = {
  userUid: null,
  username: null,
  accessToken: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,
  ...loadFromSession(),
};

export const AuthStore = signalStore(
  { providedIn: 'root' },
  withState(initialState),
  withMethods((store) => {
    const authService = inject(AuthService);
    return {
      async login(request: LoginRequest): Promise<void> {
        patchState(store, { isLoading: true, error: null });
        try {
          const response = await authService.login(request);
          saveToSession(response.userUid, response.username, response.accessToken);
          patchState(store, {
            userUid: response.userUid,
            username: response.username,
            accessToken: response.accessToken,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
        } catch (err: unknown) {
          patchState(store, {
            isLoading: false,
            error: err instanceof Error ? err.message : 'Login failed.',
          });
          throw err;
        }
      },

      async register(request: RegisterRequest): Promise<string> {
        patchState(store, { isLoading: true, error: null });
        try {
          const registerResponse = await authService.register(request);
          // Auto-login after registration
          const loginResponse = await authService.login({
            username: registerResponse.username,
            password: request.password,
          });
          saveToSession(loginResponse.userUid, loginResponse.username, loginResponse.accessToken);
          patchState(store, {
            userUid: loginResponse.userUid,
            username: loginResponse.username,
            accessToken: loginResponse.accessToken,
            isAuthenticated: true,
            isLoading: false,
            error: null,
          });
          return registerResponse.username;
        } catch (err: unknown) {
          patchState(store, {
            isLoading: false,
            error: err instanceof Error ? err.message : 'Registration failed.',
          });
          throw err;
        }
      },

      logout(): void {
        clearSession();
        patchState(store, {
          userUid: null,
          username: null,
          accessToken: null,
          isAuthenticated: false,
          isLoading: false,
          error: null,
        });
      },
    };
  }),
);

export type AuthStore = InstanceType<typeof AuthStore>;
