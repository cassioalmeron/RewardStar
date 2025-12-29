import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { HomeIcon, GameIcon, ActivityIcon, SettingsIcon } from '../../icons';
import './styles.css';

const Sidebar: React.FC = () => {
  const location = useLocation();

  const navItems = [
    { path: '/', label: 'Home', Icon: HomeIcon },
    { path: '/game', label: 'Game', Icon: GameIcon },
    { path: '/activity', label: 'Activity', Icon: ActivityIcon },
    { path: '/settings', label: 'Settings', Icon: SettingsIcon },
  ];

  const isActive = (path: string) => location.pathname === path;

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
    </nav>
  );
};

export default Sidebar;
