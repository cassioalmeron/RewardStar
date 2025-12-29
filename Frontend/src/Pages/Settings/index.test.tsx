import '@testing-library/jest-dom';
import { render } from '@testing-library/react';
import Settings from './index';

// Mock toast to avoid side effects in test output
jest.mock('react-toastify', () => ({
  toast: { success: jest.fn(), error: jest.fn() },
}));

describe('Settings Page', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('renders Settings component', () => {
    const { container } = render(<Settings />);
    expect(container).toBeInTheDocument();
  });
}); 