import { signalStore, withState } from '@ngrx/signals';

export interface AuthState {
  accessToken: string | null;
  userId: string | null;
  username: string | null;
}

const initialState: AuthState = {
  accessToken: null,
  userId: null,
  username: null,
};

export const AuthStore = signalStore({ providedIn: 'root' }, withState(initialState));

export type AuthStore = InstanceType<typeof AuthStore>;
