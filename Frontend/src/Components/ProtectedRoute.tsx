import React, { ReactNode } from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface ProtectedRouteProps {
  children: ReactNode;
  adminOnly?: boolean;      // Route only accessible to admins
  nonAdminOnly?: boolean;   // Route only accessible to non-admins
  redirectTo?: string;      // Custom redirect path (default: '/')
}

const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  adminOnly = false,
  nonAdminOnly = false,
  redirectTo = '/'
}) => {
  const { isAuthenticated, loading, isAdmin } = useAuth();

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner">Loading...</div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Role-based access control
  if (adminOnly && !isAdmin) {
    return <Navigate to={redirectTo} replace />;
  }

  if (nonAdminOnly && isAdmin) {
    return <Navigate to={redirectTo} replace />;
  }

  return <>{children}</>;
};

export default ProtectedRoute;
