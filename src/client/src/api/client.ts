import axios from 'axios';

const apiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

apiClient.interceptors.request.use((config) => {
  const stored = localStorage.getItem('auth');
  if (stored) {
    const { accessToken } = JSON.parse(stored);
    if (accessToken) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const stored = localStorage.getItem('auth');
        if (stored) {
          const { refreshToken, user } = JSON.parse(stored);
          const res = await axios.post('/api/auth/refresh', {
            userId: user.id,
            refreshToken,
          });
          const newAuth = res.data;
          localStorage.setItem('auth', JSON.stringify({
            accessToken: newAuth.accessToken,
            refreshToken: newAuth.refreshToken,
            user: newAuth.user,
          }));
          originalRequest.headers.Authorization = `Bearer ${newAuth.accessToken}`;
          return apiClient(originalRequest);
        }
      } catch {
        localStorage.removeItem('auth');
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default apiClient;
