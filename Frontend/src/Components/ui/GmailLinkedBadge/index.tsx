import React from 'react';
import { GoogleIcon } from '../../icons';
import './styles.css';

interface GmailLinkedBadgeProps {
  showLabel?: boolean;
}

const GmailLinkedBadge: React.FC<GmailLinkedBadgeProps> = ({ showLabel = true }) => {
  return (
    <div className="google-linked-badge">
      <GoogleIcon size={18} />
      {showLabel && <span className="badge-text">Gmail Linked Account</span>}
    </div>
  );
};

export default GmailLinkedBadge;
