import { create } from 'zustand';
import type { UserInfo } from '@/types/auth.types';

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: UserInfo | null;
  isAuthenticated: boolean;
  setAuth: (accessToken: string, refreshToken: string, user: UserInfo) => void;
  logout: () => void;
  updateLanguage: (lang: string) => void;
}

export const useAuthStore = create<AuthState>((set) => {
  const stored = localStorage.getItem('auth');
  const initial = stored ? JSON.parse(stored) : {};

  return {
    accessToken: initial.accessToken || null,
    refreshToken: initial.refreshToken || null,
    user: initial.user || null,
    isAuthenticated: !!initial.accessToken,

    setAuth: (accessToken, refreshToken, user) => {
      localStorage.setItem('auth', JSON.stringify({ accessToken, refreshToken, user }));
      set({ accessToken, refreshToken, user, isAuthenticated: true });
    },

    logout: () => {
      localStorage.removeItem('auth');
      set({ accessToken: null, refreshToken: null, user: null, isAuthenticated: false });
    },

    updateLanguage: (lang) => {
      set((state) => {
        if (!state.user) return state;
        const updated = { ...state.user, languagePreference: lang };
        const stored = localStorage.getItem('auth');
        if (stored) {
          const parsed = JSON.parse(stored);
          parsed.user = updated;
          localStorage.setItem('auth', JSON.stringify(parsed));
        }
        return { user: updated };
      });
    },
  };
});
