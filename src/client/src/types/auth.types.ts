export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface UserInfo {
  id: string;
  username: string;
  fullName: string;
  fullNameEn: string | null;
  role: string;
  department: string;
  languagePreference: string;
}
