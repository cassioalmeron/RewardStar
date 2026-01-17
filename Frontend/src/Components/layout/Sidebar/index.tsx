import React from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useAuth } from '../../../contexts/AuthContext';
import { HomeIcon, GameIcon, ActivityIcon, SettingsIcon, LogoutIcon, UsersIcon } from '../../icons';
import './styles.css';

const Sidebar: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout, isAdmin } = useAuth();

  const navItems = [
    { path: '/', label: 'Home', Icon: HomeIcon },
    ...(!isAdmin ? [{ path: '/game', label: 'Game', Icon: GameIcon }] : []),
    ...(!isAdmin ? [{ path: '/activity', label: 'Activity', Icon: ActivityIcon }] : []),
    ...(isAdmin ? [{ path: '/users', label: 'User Management', Icon: UsersIcon }] : []),
    { path: '/settings', label: 'Profile', Icon: SettingsIcon },
  ];

  const isActive = (path: string) => location.pathname === path;

  const handleLogout = () => {
    logout();
    toast.info('Logged out successfully');
    navigate('/login');
  };

  return (
    <nav className="sidebar">
      <div className="sidebar-header">
        <span className="sidebar-star">⭐</span>
        <span className="sidebar-title">Reward Star</span>
      </div>
      <ul className="sidebar-nav">
        {navItems.map(({ path, label, Icon }) => (
          <li key={path}>
            <Link
              to={path}
              className={`sidebar-link ${isActive(path) ? 'active' : ''}`}
            >
              <Icon className="sidebar-icon" />
              <span className="sidebar-text">{label}</span>
            </Link>
          </li>
        ))}
      </ul>

      {/* User section at bottom */}
      <div className="sidebar-footer">
        <div className="sidebar-user">
          <div className="user-avatar">
            {user?.name.substring(0, 2).toUpperCase()}
          </div>
          <div className="user-info">
            <span className="user-name">{user?.name}</span>
            <span className="user-email">{user?.email}</span>
          </div>
        </div>
        <button onClick={handleLogout} className="logout-btn">
          <LogoutIcon className="icon" size={18} />
          Logout
        </button>
      </div>
    </nav>
  );
};

export default Sidebar;
