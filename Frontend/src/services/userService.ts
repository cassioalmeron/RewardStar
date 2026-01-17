import { api } from './api';
import type { User, UpdateUserRequest, UpdateStatusRequest } from '../Types/Auth';

/**
 * Get all users (admin only)
 */
export const getAllUsers = async (): Promise<User[]> => {
  const response = await api.get<User[]>('/Users');
  return response.data;
};

/**
 * Get user by ID (admin or self only)
 */
export const getUserById = async (id: number): Promise<User> => {
  const response = await api.get<User>(`/Users/${id}`);
  return response.data;
};

/**
 * Update user profile (admin can edit any, users can edit self only)
 */
export const updateUser = async (id: number, data: UpdateUserRequest): Promise<User> => {
  const response = await api.put<User>(`/Users/${id}`, data);
  return response.data;
};

/**
 * Update user active status (admin only)
 */
export const updateUserStatus = async (id: number, active: boolean): Promise<void> => {
  const request: UpdateStatusRequest = { active };
  await api.patch(`/Users/${id}/status`, request);
};

const userService = {
  getAllUsers,
  getUserById,
  updateUser,
  updateUserStatus
};

export default userService;
