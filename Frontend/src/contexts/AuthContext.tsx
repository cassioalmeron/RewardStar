import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import authService from '../services/authService';
import type { User, AuthContextType } from '../Types/Auth';
import { isAdmin } from '../Types/Auth';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Initialize authentication on mount
  useEffect(() => {
    const initAuth = async () => {
      const token = authService.getToken();
      if (token) {
        try {
          const currentUser = await authService.getCurrentUser();
          setUser(currentUser);
        } catch (err) {
          // Token invalid or expired
          authService.removeToken();
        }
      }
      setLoading(false);
    };
    initAuth();
  }, []);

  const login = async (email: string, password: string): Promise<void> => {
    setError(null);
    try {
      await authService.login(email, password);
      const currentUser = await authService.getCurrentUser();
      setUser(currentUser);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Login failed');
      throw err;
    }
  };

  const register = async (name: string, email: string, password: string): Promise<void> => {
    setError(null);
    try {
      await authService.register(name, email, password);
      // Don't set user - redirect to login page after registration
    } catch (err: any) {
      setError(err.response?.data?.message || 'Registration failed');
      throw err;
    }
  };

  const loginWithGoogle = async (idToken: string): Promise<void> => {
    setError(null);
    try {
      await authService.loginWithGoogle(idToken);
      const currentUser = await authService.getCurrentUser();
      setUser(currentUser);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Google login failed');
      throw err;
    }
  };

  const registerWithGoogle = async (idToken: string): Promise<void> => {
    setError(null);
    try {
      await authService.registerWithGoogle(idToken);
      const currentUser = await authService.getCurrentUser();
      setUser(currentUser);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Google registration failed');
      throw err;
    }
  };

  const logout = (): void => {
    authService.logout();
    setUser(null);
  };

  const value: AuthContextType = {
    user,
    loading,
    error,
    login,
    register,
    loginWithGoogle,
    registerWithGoogle,
    logout,
    isAuthenticated: !!user,
    isAdmin: isAdmin(user)
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
