import '@testing-library/jest-dom';
import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import Parameters from './index';

// Mock toast to avoid side effects in test output
jest.mock('react-toastify', () => ({
  toast: { success: jest.fn(), error: jest.fn() },
}));

describe('Parameters Page', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('renders API URL input and Save button', () => {
    render(<Parameters />);
    expect(screen.getByLabelText(/API URL/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /save/i })).toBeInTheDocument();
  });

  it('enables Save button when input is not empty', () => {
    render(<Parameters />);
    const input = screen.getByLabelText(/API URL/i);
    const button = screen.getByRole('button', { name: /save/i });
    expect(button).toBeDisabled();
    fireEvent.change(input, { target: { value: 'https://api.example.com' } });
    expect(button).not.toBeDisabled();
  });
}); 