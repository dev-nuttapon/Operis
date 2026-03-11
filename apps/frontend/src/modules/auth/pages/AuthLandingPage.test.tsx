import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { AuthLandingPage } from './AuthLandingPage';
import { useAuth } from '../hooks/useAuth';
import { useThemeStore } from '../../../shared/store/useThemeStore';

// Mock matchMedia for window (required by useThemeStore interaction)
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// Mock modules
vi.mock('../hooks/useAuth', () => ({
  useAuth: vi.fn(),
}));

vi.mock('../../../shared/store/useThemeStore', () => ({
  useThemeStore: vi.fn(),
}));

// Mock react-i18next
vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: {
      language: 'en',
      changeLanguage: vi.fn(),
    },
  }),
}));

describe('AuthLandingPage', () => {
  let mockLogin: ReturnType<typeof vi.fn> | any;
  let mockLogout: ReturnType<typeof vi.fn> | any;
  let mockSetTheme: ReturnType<typeof vi.fn> | any;

  beforeEach(() => {
    vi.clearAllMocks();
    mockLogin = vi.fn().mockResolvedValue(undefined);
    mockLogout = vi.fn().mockResolvedValue(undefined);
    mockSetTheme = vi.fn();

    vi.mocked(useThemeStore).mockReturnValue({
      theme: 'system',
      setTheme: mockSetTheme,
    });
  });

  it('renders login button when unauthenticated and ready', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: true,
      isAuthenticated: false,
      login: mockLogin,
      logout: mockLogout,
    });

    render(<AuthLandingPage />);

    const titleEl = screen.getByText('auth.welcome_title');
    expect(titleEl).toBeInTheDocument();

    const tagEl = screen.getByText('auth.status_anonymous');
    expect(tagEl).toBeInTheDocument();

    const loginButton = screen.getByRole('button', { name: /login/i });
    expect(loginButton).toBeInTheDocument();
    
    // Simulate click
    fireEvent.click(loginButton);
    expect(mockLogin).toHaveBeenCalled();
  });

  it('renders logout button when authenticated and ready', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: true,
      isAuthenticated: true,
      login: mockLogin,
      logout: mockLogout,
    });

    render(<AuthLandingPage />);

    const tagEl = screen.getByText('auth.status_authenticated');
    expect(tagEl).toBeInTheDocument();

    const logoutButton = screen.getByRole('button', { name: /logout/i });
    expect(logoutButton).toBeInTheDocument();

    fireEvent.click(logoutButton);
    expect(mockLogout).toHaveBeenCalled();
  });

  it('renders disabled buttons when not ready', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: false,
      isAuthenticated: false,
      login: mockLogin,
      logout: mockLogout,
    });

    render(<AuthLandingPage />);

    const loginButton = screen.getByRole('button', { name: /login/i });
    expect(loginButton).toBeDisabled();
  });
});
