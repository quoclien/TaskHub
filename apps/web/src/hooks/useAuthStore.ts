import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { getUserIdFromToken, getUserRoleFromToken, isTokenExpired } from '@/lib/jwt';

interface AuthState {
  token: string | null;
  userId: string | null;
  userRole: string | null;
  setToken: (token: string | null) => void;
  clearToken: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      token: null,
      userId: null,
      userRole: null,
      setToken: (token: string | null) => {
        if (token) {
          const userId = getUserIdFromToken(token);
          const userRole = getUserRoleFromToken(token);
          set({ token, userId, userRole });
        } else {
          set({ token: null, userId: null, userRole: null });
        }
      },
      clearToken: () => {
        set({ token: null, userId: null, userRole: null });
      },
      isAuthenticated: () => {
        const state = get();
        return !!(state.token && state.userId && !isTokenExpired(state.token));
      },
    }),
    {
      name: 'auth-storage', // name of the item in the storage (must be unique)
    }
  )
);
