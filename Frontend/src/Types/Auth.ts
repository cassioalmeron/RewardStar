// Request DTOs
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface GoogleAuthRequest {
  idToken: string;
}

// Response DTOs (matching backend AuthenticationDTOs.cs)
export interface AuthResponse {
  userId: number;
  name: string;
  email: string;
  token: string;
  active: boolean;
  googleAuthId?: string;
}

export interface User {
  id: number;
  name: string;
  email: string;
  active: boolean;
  createdAt: string;
  lastLoginAt?: string;
  isAdmin: boolean;
  isGoogleAccount?: boolean;
}

// Helper function to check if user is admin
export const isAdmin = (user: User | null): boolean => {
  return user?.isAdmin === true;
};

// User Management DTOs
export interface UpdateUserRequest {
  name: string;
  email: string;
  newPassword?: string;
  currentPassword?: string;
  active?: boolean;
}

export interface UpdateStatusRequest {
  active: boolean;
}

// Context Types
export interface AuthContextType {
  user: User | null;
  loading: boolean;
  error: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (name: string, email: string, password: string) => Promise<void>;
  loginWithGoogle: (idToken: string) => Promise<void>;
  registerWithGoogle: (idToken: string) => Promise<void>;
  logout: () => void;
  isAuthenticated: boolean;
  isAdmin: boolean;
}
