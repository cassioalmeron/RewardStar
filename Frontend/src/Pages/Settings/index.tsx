import React, { useState, useEffect } from 'react';
import { toast } from 'react-toastify';
import { useAuth } from '../../contexts/AuthContext';
import userService from '../../services/userService';
import { UpdateUserRequest } from '../../Types/Auth';
import GmailLinkedBadge from '../../Components/ui/GmailLinkedBadge';
import './styles.css';

const Settings: React.FC = () => {
  const { user } = useAuth();
  const [formData, setFormData] = useState({
    name: user?.name || '',
    email: user?.email || '',
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (user) {
      setFormData(prev => ({
        ...prev,
        name: user.name,
        email: user.email
      }));
    }
  }, [user]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.newPassword && formData.newPassword !== formData.confirmPassword) {
      toast.error('New passwords do not match');
      return;
    }

    if (formData.newPassword && formData.newPassword.length < 8) {
      toast.error('Password must be at least 8 characters');
      return;
    }

    if (formData.newPassword && !/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$/.test(formData.newPassword)) {
      toast.error('Password must contain uppercase, lowercase, and number');
      return;
    }

    setLoading(true);

    try {
      const updateData: UpdateUserRequest = {
        name: formData.name,
        email: formData.email
      };

      if (formData.newPassword) {
        updateData.newPassword = formData.newPassword;
        updateData.currentPassword = formData.currentPassword;
      }

      await userService.updateUser(user!.id, updateData);
      toast.success('Profile updated successfully');

      // Clear password fields
      setFormData(prev => ({
        ...prev,
        currentPassword: '',
        newPassword: '',
        confirmPassword: ''
      }));
    } catch (error: any) {
      toast.error(error.response?.data?.message || 'Failed to update profile');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="settings-container">
      <div className="settings-header">
        <h1>Profile Settings</h1>
        <p>Update your personal information and password</p>
      </div>

      <form onSubmit={handleSubmit} className="settings-form">
        <div className="form-section">
          <h2>Personal Information</h2>

          <div className="form-group">
            <label htmlFor="name">Name</label>
            <input
              id="name"
              type="text"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
              maxLength={200}
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              required
              maxLength={320}
            />
          </div>
        </div>

        {user?.isGoogleAccount ? (
          <div className="form-section">
            <GmailLinkedBadge showLabel={true} />
          </div>
        ) : (
          <div className="form-section">
            <h2>Change Password</h2>
            <p className="section-description">Leave blank if you don't want to change your password</p>

            <div className="form-group">
              <label htmlFor="currentPassword">Current Password</label>
              <input
                id="currentPassword"
                type="password"
                value={formData.currentPassword}
                onChange={(e) => setFormData({ ...formData, currentPassword: e.target.value })}
                placeholder="Enter current password..."
              />
            </div>

            <div className="form-group">
              <label htmlFor="newPassword">New Password</label>
              <input
                id="newPassword"
                type="password"
                value={formData.newPassword}
                onChange={(e) => setFormData({ ...formData, newPassword: e.target.value })}
                minLength={8}
                placeholder="Enter new password..."
              />
              <small className="form-hint">
                At least 8 characters with uppercase, lowercase, and number
              </small>
            </div>

            <div className="form-group">
              <label htmlFor="confirmPassword">Confirm New Password</label>
              <input
                id="confirmPassword"
                type="password"
                value={formData.confirmPassword}
                onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
                minLength={8}
                placeholder="Confirm new password..."
              />
            </div>
          </div>
        )}

        <div className="form-actions">
          <button type="submit" disabled={loading} className="btn-save">
            {loading ? 'Saving...' : 'Save Changes'}
          </button>
        </div>
      </form>
    </div>
  );
};

export default Settings;