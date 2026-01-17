import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import userService from '../../services/userService';
import { User } from '../../Types/Auth';
import UserListTable from './UserListTable';
import EditUserModal from './EditUserModal';
import './styles.css';

const UserManagement: React.FC = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [showEditModal, setShowEditModal] = useState(false);

  useEffect(() => {
    loadUsers().catch(() => {
      // Error already handled by loadUsers with toast
    });
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await userService.getAllUsers();
      setUsers(data);
    } catch (error: any) {
      const errorMessage = error.response?.data?.message || 'Failed to load users';
      setError(errorMessage);
      toast.error(errorMessage);
      throw error; // Re-throw so caller knows it failed
    } finally {
      setLoading(false);
    }
  };

  const handleEditUser = (user: User) => {
    setSelectedUser(user);
    setShowEditModal(true);
  };

  const handleToggleStatus = async (userId: number, currentStatus: boolean) => {
    try {
      await userService.updateUserStatus(userId, !currentStatus);
      toast.success(`User ${!currentStatus ? 'activated' : 'deactivated'} successfully`);
      loadUsers();
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update user status');
    }
  };

  const handleSaveUser = async () => {
    try {
      setShowEditModal(false);
      await loadUsers();
      toast.success('User updated successfully');
    } catch (error) {
      // Error already shown by loadUsers, just don't show success toast
    }
  };

  if (loading) {
    return <div className="user-management-loading">Loading users...</div>;
  }

  if (error) {
    return (
      <div className="user-management-container">
        <div className="user-management-error">
          <p>{error}</p>
          <button onClick={() => loadUsers().catch(() => {})} className="btn-retry">
            Retry
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="user-management-container">
      <div className="user-management-header">
        <h1>User Management</h1>
        <p>Manage all registered users</p>
      </div>

      {users.length === 0 ? (
        <div className="user-management-empty">
          <p>No users found</p>
        </div>
      ) : (
        <UserListTable
          users={users}
          onEdit={handleEditUser}
          onToggleStatus={handleToggleStatus}
        />
      )}

      {showEditModal && selectedUser && (
        <EditUserModal
          user={selectedUser}
          onClose={() => setShowEditModal(false)}
          onSave={handleSaveUser}
        />
      )}
    </div>
  );
};

export default UserManagement;
