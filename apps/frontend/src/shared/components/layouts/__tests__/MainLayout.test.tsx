import { describe, it, expect, vi, beforeEach } from 'vitest';
import { act, render, screen, fireEvent, waitFor } from '@testing-library/react';
import { MainLayout } from '../MainLayout';
import { MemoryRouter } from 'react-router-dom';
import { useAuth } from '../../../../modules/auth';
import { useThemeStore } from '../../../store/useThemeStore';
import { useI18nLanguage } from '../../../i18n/hooks/useI18nLanguage';

// Mocks
vi.mock('../../../../modules/auth', () => ({
  useAuth: vi.fn(),
}));

vi.mock('../../../store/useThemeStore', () => ({
  useThemeStore: vi.fn(),
}));

vi.mock('../../../i18n/config', () => ({
  default: {
    t: (key: string, options?: { lng?: string }) => {
      if (key === 'common.application_name') return 'OPERIS';
      if (key === 'common.documents') return 'Documents';
      if (key === 'auth.logout_button') return 'Logout';
      if (key === 'common.user_fallback') return 'User';
      if (key === 'common.version') return 'Version';
      return options?.lng ? `${key}:${options.lng}` : key;
    },
    language: 'en',
    on: vi.fn(),
    off: vi.fn(),
    changeLanguage: vi.fn(),
  },
}));

vi.mock('../../../i18n/hooks/useI18nLanguage', () => ({
  useI18nLanguage: vi.fn(),
}));

vi.mock('react-i18next', () => ({
  useTranslation: vi.fn(() => ({
    t: (key: string) => key,
    i18n: {
      language: 'en',
      changeLanguage: vi.fn(),
    },
  })),
}));

// Mock Resize Observer for Ant Design layout rendering
vi.stubGlobal('ResizeObserver', class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
});

describe('MainLayout', () => {
  let mockLogout: any;
  let mockSetTheme: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    vi.clearAllMocks();
    mockLogout = vi.fn().mockResolvedValue(undefined);
    mockSetTheme = vi.fn();

    vi.mocked(useAuth).mockReturnValue({
      isReady: true,
      isAuthenticated: true,
      user: { name: 'Test User', email: 'user@example.com', roles: [] },
      login: vi.fn(),
      logout: mockLogout,
    });

    vi.mocked(useThemeStore).mockReturnValue({
      theme: 'light',
      setTheme: mockSetTheme,
    });
    vi.mocked(useI18nLanguage).mockReturnValue('en');
  });

  it('renders correctly with sidebar and header elements', () => {
    render(
      <MemoryRouter>
        <MainLayout />
      </MemoryRouter>
    );

    // Verify logo
    expect(screen.getByText('OPERIS')).toBeInTheDocument();
    
    // Verify menu items
    expect(screen.getByText('Documents')).toBeInTheDocument();
    
    expect(screen.getByText('Test User')).toBeInTheDocument();
  });

  it('handles sidebar toggling correctly', () => {
    render(
      <MemoryRouter>
        <MainLayout />
      </MemoryRouter>
    );

    // Get the toggle button (MenuFoldOutlined is rendered by default when not collapsed)
    // The closest role is button, and it has no text, just an icon.
    // It's the first button in the header structure usually.
    // We can query by class or assume it's the first button since logout is named.
    const toggleBtn = screen.getByRole('button', { name: 'left' });
    
    expect(screen.getByText('OPERIS')).toBeInTheDocument();
    
    act(() => {
      fireEvent.click(toggleBtn);
    });
    
    expect(screen.queryByText('OPERIS')).not.toBeInTheDocument();
    
    act(() => {
      fireEvent.click(screen.getByRole('button', { name: 'right' }));
    });

    expect(screen.getByText('OPERIS')).toBeInTheDocument();
  });

  it('calls logout when logout button is clicked', async () => {
    render(
      <MemoryRouter>
        <MainLayout />
      </MemoryRouter>
    );

    act(() => {
      fireEvent.click(screen.getByText('Test User'));
    });

    const logoutBtn = await screen.findByText('Logout');
    act(() => {
      fireEvent.click(logoutBtn);
    });

    await waitFor(() => {
      expect(mockLogout).toHaveBeenCalled();
    });
  });
});
