import React from 'react';
import { GoogleLogin } from '@react-oauth/google';
import { toast } from 'react-toastify';

interface GoogleSignInButtonProps {
  onSuccess: (credentialResponse: any) => void;
  onError?: () => void;
  text?: 'signin' | 'signup';
}

const GoogleSignInButton: React.FC<GoogleSignInButtonProps> = ({
  onSuccess,
  onError,
  text = 'signin'
}) => {
  return (
    <div className="google-signin-wrapper">
      <GoogleLogin
        onSuccess={onSuccess}
        onError={() => {
          toast.error('Google authentication failed');
          onError?.();
        }}
        text={text === 'signin' ? 'signin_with' : 'signup_with'}
        theme="filled_blue"
        size="large"
        width="100%"
      />
    </div>
  );
};

export default GoogleSignInButton;
