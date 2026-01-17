import { api } from './api';
import type { AuthResponse, User } from '../Types/Auth';

const TOKEN_KEY = 'auth_token';

// Token Management
export const getToken = (): string | null => {
  return localStorage.getItem(TOKEN_KEY);
};

export const setToken = (token: string): void => {
  localStorage.setItem(TOKEN_KEY, token);
};

export const removeToken = (): void => {
  localStorage.removeItem(TOKEN_KEY);
};

export const isAuthenticated = (): boolean => {
  return !!getToken();
};

// API Calls
export const register = async (name: string, email: string, password: string): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>('/Auth/Register', { name, email, password });
  setToken(response.data.token);
  return response.data;
};

export const login = async (email: string, password: string): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>('/Auth/Login', { email, password });
  setToken(response.data.token);
  return response.data;
};

export const registerWithGoogle = async (idToken: string): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>('/Auth/Register/Google', { idToken });
  setToken(response.data.token);
  return response.data;
};

export const loginWithGoogle = async (idToken: string): Promise<AuthResponse> => {
  const response = await api.post<AuthResponse>('/Auth/Login/Google', { idToken });
  setToken(response.data.token);
  return response.data;
};

export const getCurrentUser = async (): Promise<User> => {
  const response = await api.get<User>('/Users/me');
  return response.data;
};

export const logout = (): void => {
  removeToken();
};

const authService = {
  getToken,
  setToken,
  removeToken,
  isAuthenticated,
  register,
  login,
  registerWithGoogle,
  loginWithGoogle,
  getCurrentUser,
  logout
};

export default authService;
