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
export const mockChangeLanguage = vi.fn();
vi.mock('react-i18next', () => ({
  useTranslation: vi.fn(() => ({
    t: (key: string) => key,
    i18n: {
      language: 'en',
      changeLanguage: mockChangeLanguage,
    },
  })),
}));

import { useTranslation } from 'react-i18next';

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
      authState: 'anonymous',
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
      authState: 'authenticated',
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
      authState: 'loading',
      login: mockLogin,
      logout: mockLogout,
    });

    render(<AuthLandingPage />);

    const loginButton = screen.getByRole('button', { name: /login/i });
    expect(loginButton).toBeDisabled();
  });

  it('handles language changing correctly', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: true,
      isAuthenticated: false,
      authState: 'anonymous',
      login: mockLogin,
      logout: mockLogout,
    });
    
    // Override the mock locally for this test
    vi.mocked(useTranslation).mockReturnValue({
      t: (key: string) => key,
      i18n: { language: 'en', changeLanguage: mockChangeLanguage },
      ready: true,
    } as any);

    render(<AuthLandingPage />);
    
    // Ant Design's Select registers an ARIA combobox
    const comboboxes = screen.getAllByRole('combobox');
    const langSelect = comboboxes[0]; 
    
    // Simulate user toggling
    fireEvent.mouseDown(langSelect);
    
    // Find the Thai option using the mocked translation key
    const thOption = screen.getByTitle('common.language_th');
    fireEvent.click(thOption);
    
    expect(mockChangeLanguage).toHaveBeenCalledWith('th');
  });

  it('handles theme changing correctly', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: true,
      isAuthenticated: false,
      authState: 'anonymous',
      login: mockLogin,
      logout: mockLogout,
    });

    render(<AuthLandingPage />);
    
    const comboboxes = screen.getAllByRole('combobox');
    const themeSelect = comboboxes[1]; // The second select is the Theme Select
    
    fireEvent.mouseDown(themeSelect);
    const darkOption = screen.getByText('common.theme.dark');
    fireEvent.click(darkOption);
    
    expect(mockSetTheme).toHaveBeenCalledWith('dark');
  });
});
