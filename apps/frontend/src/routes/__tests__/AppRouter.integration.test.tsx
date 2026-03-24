import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render as tlRender, screen, act } from '@testing-library/react';
import { AppRouter } from '../AppRouter';
import { AppProviders } from '../../providers/AppProviders';
import * as keycloakAuthService from '../../modules/auth/services/keycloakAuth';
import { useThemeStore } from '../../shared/store/useThemeStore';

function render(ui: React.ReactElement) {
  return tlRender(ui);
}

// Mock Keycloak Service
vi.mock('../../modules/auth/services/keycloakAuth', () => ({
  initKeycloak: vi.fn(),
  bindAuthEvents: vi.fn(),
  isAuthenticated: vi.fn(() => false),
  refreshToken: vi.fn(async () => true),
  getUserProfile: vi.fn(async () => null),
  getTokenParsed: vi.fn(() => null),
  login: vi.fn(async () => undefined),
  logout: vi.fn(async () => undefined),
  getAccessToken: vi.fn(),
}));

// Mock Zustand
vi.mock('../../shared/store/useThemeStore', () => ({
  useThemeStore: vi.fn(),
}));

// Mock react-i18next to prevent Context issues
vi.mock('react-i18next', async (importOriginal) => {
  const actual = await importOriginal<typeof import('react-i18next')>();
  return {
    ...actual,
    useTranslation: () => ({
      t: (key: string) => key,
      i18n: { 
        language: 'en',
        changeLanguage: vi.fn() 
      },
    }),
  };
});

// Mock matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation((query) => ({
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

// Mock ResizeObserver
vi.stubGlobal('ResizeObserver', class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
});

function renderWithProviders(ui: React.ReactElement) {
  return render(<AppProviders>{ui}</AppProviders>);
}

describe('AppRouter Integration Tests', () => {
  beforeEach(() => {
    vi.clearAllMocks();

    vi.mocked(useThemeStore).mockReturnValue({
      theme: 'light',
      setTheme: vi.fn(),
    });
  });

  it('redirects to /login when attempting to access /app/documents without auth', async () => {
    // Navigate to protected route
    window.history.pushState({}, '', '/app/documents');

    vi.spyOn(keycloakAuthService, 'initKeycloak').mockResolvedValue(false); // Unauthenticated

    await act(async () => {
      renderWithProviders(<AppRouter />);
    });

    expect(await screen.findByText('auth.checking_login')).toBeInTheDocument();
    expect(window.location.pathname).toBe('/login');
  });

  it('renders the MainLayout and DocumentDashboardPage when authenticated and visiting /app/documents', async () => {
    window.history.pushState({}, '', '/app/documents');

    vi.spyOn(keycloakAuthService, 'initKeycloak').mockResolvedValue(true); // Authenticated

    await act(async () => {
      renderWithProviders(<AppRouter />);
    });

    expect(await screen.findByText('Document Dashboard')).toBeInTheDocument();
    expect(window.location.pathname).toBe('/app/documents');
  });

  it('renders the WorkflowDefinitionsPage when authenticated and visiting /app/workflows', async () => {
    window.history.pushState({}, '', '/app/workflows');

    vi.spyOn(keycloakAuthService, 'initKeycloak').mockResolvedValue(true);

    await act(async () => {
      renderWithProviders(<AppRouter />);
    });

    expect(await screen.findByText('workflow_definitions.page_title')).toBeInTheDocument();
    expect(window.location.pathname).toBe('/app/workflows');
  });

  it('handles unknown routes by redirecting to /login fallback (`*`)', async () => {
    window.history.pushState({}, '', '/unknown-random-route');

    vi.spyOn(keycloakAuthService, 'initKeycloak').mockResolvedValue(false);

    await act(async () => {
      renderWithProviders(<AppRouter />);
    });

    expect(await screen.findByText('auth.checking_login')).toBeInTheDocument();
    expect(window.location.pathname).toBe('/login');
  });
});
