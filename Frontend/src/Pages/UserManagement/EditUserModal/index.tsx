import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import userService from '../../../services/userService';
import { User, UpdateUserRequest } from '../../../Types/Auth';
import { CloseIcon } from '../../../Components/icons';
import './styles.css';

interface EditUserModalProps {
  user: User;
  onClose: () => void;
  onSave: () => void;
}

const EditUserModal: React.FC<EditUserModalProps> = ({ user, onClose, onSave }) => {
  const [formData, setFormData] = useState({
    name: user.name,
    email: user.email,
    newPassword: '',
    active: user.active
  });
  const [loading, setLoading] = useState(false);
  const [passwordError, setPasswordError] = useState<string>('');

  // Handle ESC key and prevent body scroll
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };

    document.addEventListener('keydown', handleEscape);
    document.body.style.overflow = 'hidden';

    return () => {
      document.removeEventListener('keydown', handleEscape);
      document.body.style.overflow = 'unset';
    };
  }, [onClose]);

  const validatePassword = (password: string): string => {
    if (!password) return ''; // Empty is valid (optional field)

    if (password.length < 8) {
      return 'Password must be at least 8 characters';
    }
    if (!/[A-Z]/.test(password)) {
      return 'Password must contain at least one uppercase letter';
    }
    if (!/[a-z]/.test(password)) {
      return 'Password must contain at least one lowercase letter';
    }
    if (!/[0-9]/.test(password)) {
      return 'Password must contain at least one number';
    }
    return '';
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newPassword = e.target.value;
    setFormData({ ...formData, newPassword });
    setPasswordError(validatePassword(newPassword));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Validate password if it's being changed
    if (formData.newPassword) {
      const pwdError = validatePassword(formData.newPassword);
      if (pwdError) {
        setPasswordError(pwdError);
        return;
      }
    }

    setLoading(true);

    try {
      const updateData: UpdateUserRequest = {
        name: formData.name,
        email: formData.email,
        active: formData.active
      };

      if (formData.newPassword) {
        updateData.newPassword = formData.newPassword;
      }

      await userService.updateUser(user.id, updateData);
      await onSave(); // Await parent callback
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update user');
    } finally {
      setLoading(false); // Always reset loading state
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>Edit User {user.isAdmin && '(Admin)'}</h2>
          <button onClick={onClose} className="modal-close" aria-label="Close">
            <CloseIcon size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit} className="modal-form">
          <div className="form-group">
            <label>Name</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
              maxLength={200}
            />
          </div>

          <div className="form-group">
            <label>Email</label>
            <input
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              required
              maxLength={320}
            />
          </div>

          <div className="form-group">
            <label>New Password (leave blank to keep current)</label>
            <input
              type="password"
              value={formData.newPassword}
              onChange={handlePasswordChange}
              placeholder="Enter new password..."
              className={passwordError ? 'input-error' : ''}
            />
            {passwordError ? (
              <small className="form-error">{passwordError}</small>
            ) : (
              <small className="form-hint">
                Password must be at least 8 characters with uppercase, lowercase, and number
              </small>
            )}
          </div>

          {!user.isAdmin && (
            <div className="form-group">
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={formData.active}
                  onChange={(e) => setFormData({ ...formData, active: e.target.checked })}
                />
                <span>Active</span>
              </label>
            </div>
          )}

          <div className="modal-actions">
            <button type="button" onClick={onClose} className="btn-cancel" disabled={loading}>
              Cancel
            </button>
            <button type="submit" disabled={loading} className="btn-save">
              {loading ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default EditUserModal;
