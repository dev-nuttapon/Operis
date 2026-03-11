import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { MainLayout } from '../MainLayout';
import { MemoryRouter } from 'react-router-dom';
import { useAuth } from '../../../../modules/auth/hooks/useAuth';
import { useThemeStore } from '../../../store/useThemeStore';

// Mocks
vi.mock('../../../../modules/auth/hooks/useAuth', () => ({
  useAuth: vi.fn(),
}));

vi.mock('../../../store/useThemeStore', () => ({
  useThemeStore: vi.fn(),
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
      login: vi.fn(),
      logout: mockLogout,
    });

    vi.mocked(useThemeStore).mockReturnValue({
      theme: 'light',
      setTheme: mockSetTheme,
    });
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
    
    // Verify Logout button exists exactly once (assuming standard layout rendering)
    const logoutBtn = screen.getByRole('button', { name: /logout/i });
    expect(logoutBtn).toBeInTheDocument();
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
    const buttons = screen.getAllByRole('button');
    const toggleBtn = buttons[0]; // Assuming it's the first one in Header
    
    // Initially not collapsed (Logo reads "OPERIS")
    expect(screen.getByText('OPERIS')).toBeInTheDocument();
    
    // Click to collapse
    fireEvent.click(toggleBtn);
    
    // Logo should change to "OP"
    expect(screen.getByText('OP')).toBeInTheDocument();
    
    // Click to uncollapse
    fireEvent.click(toggleBtn);
    expect(screen.getByText('OPERIS')).toBeInTheDocument();
  });

  it('calls logout when logout button is clicked', () => {
    render(
      <MemoryRouter>
        <MainLayout />
      </MemoryRouter>
    );

    const logoutBtn = screen.getByRole('button', { name: /logout/i });
    fireEvent.click(logoutBtn);

    expect(mockLogout).toHaveBeenCalled();
  });
});
