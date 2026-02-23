import { create } from 'zustand';

interface UiState {
  sidebarOpen: boolean;
  language: string;
  toggleSidebar: () => void;
  setLanguage: (lang: string) => void;
}

export const useUiStore = create<UiState>((set) => ({
  sidebarOpen: true,
  language: localStorage.getItem('lang') || 'ar',
  toggleSidebar: () => set((s) => ({ sidebarOpen: !s.sidebarOpen })),
  setLanguage: (lang) => {
    localStorage.setItem('lang', lang);
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.lang = lang;
    set({ language: lang });
  },
}));
