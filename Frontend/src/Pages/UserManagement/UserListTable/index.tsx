import React from 'react';
import { User } from '../../../Types/Auth';
import './styles.css';

interface UserListTableProps {
  users: User[];
  onEdit: (user: User) => void;
  onToggleStatus: (userId: number, currentStatus: boolean) => void;
}

const UserListTable: React.FC<UserListTableProps> = ({ users, onEdit, onToggleStatus }) => {
  const formatDate = (dateString?: string) => {
    if (!dateString) return 'Never';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="user-table-container">
      <table className="user-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Email</th>
            <th>Status</th>
            <th>Created At</th>
            <th>Last Login</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>{user.id}</td>
              <td>
                {user.name}
                {user.isAdmin && <span className="admin-badge">Admin</span>}
              </td>
              <td>{user.email}</td>
              <td>
                <span className={`status-badge ${user.active ? 'active' : 'inactive'}`}>
                  {user.active ? 'Active' : 'Inactive'}
                </span>
              </td>
              <td>{formatDate(user.createdAt)}</td>
              <td>{formatDate(user.lastLoginAt)}</td>
              <td className="action-buttons">
                <button onClick={() => onEdit(user)} className="btn-edit">
                  Edit
                </button>
                {!user.isAdmin && (
                  <button
                    onClick={() => onToggleStatus(user.id, user.active)}
                    className={`btn-toggle ${user.active ? 'btn-deactivate' : 'btn-activate'}`}
                  >
                    {user.active ? 'Deactivate' : 'Activate'}
                  </button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default UserListTable;
