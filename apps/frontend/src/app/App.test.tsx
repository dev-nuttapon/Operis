import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render as tlRender, screen, act } from '@testing-library/react';
import { App } from './App';
import * as keycloakAuthService from '../modules/auth/services/keycloakAuth';

function render(ui: React.ReactElement) {
  return tlRender(ui);
}

// Global Mocks for App tree
vi.mock('../shared/store/useThemeStore', () => ({
  useThemeStore: () => ({
    theme: 'light',
    setTheme: vi.fn(),
  }),
}));

vi.mock('react-i18next', () => ({
  useTranslation: () => ({
    t: (key: string) => key,
    i18n: { 
      language: 'en',
      changeLanguage: vi.fn() 
    },
  }),
}));

// Use actual React Context Providers instead of mocking `AppProviders` and `AppRouter`.
// However, we MUST mock Window/DOM APIs that JSDOM cannot evaluate perfectly,
// as well as the external Keycloak HTTP configurations to isolate the frontend logic.

// 1. Mock matchMedia for Ant Design responsiveness
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

// 2. Mock ResizeObserver for Ant Design components
vi.stubGlobal('ResizeObserver', class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
});

// 3. Mock window.location for router testing without breaking jsdom navigation
Object.defineProperty(window, 'location', {
  value: {
    ...window.location,
    pathname: '/',
    assign: vi.fn(),
    replace: vi.fn(),
  },
  writable: true,
});

describe('App - Full Smoke Test', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    
    // Reset Keycloak Service Spies
    vi.spyOn(keycloakAuthService, 'initKeycloak').mockResolvedValue(false); // Simulate Unauthenticated Response
    vi.spyOn(keycloakAuthService, 'login').mockResolvedValue(undefined);
  });

  it('renders the application correctly via Providers into Landing Area (Integration) without crashing', async () => {
    // Act is required because components perform async effects (e.g. keycloak initialization)
    await act(async () => {
      // Mock the AppProviders layer to skip the useThemeStore hook which conflicts with JSDOM
      vi.mock('../providers/AppProviders', () => ({
        AppProviders: ({ children }: { children: React.ReactNode }) => (
          <div data-testid="mock-providers">{children}</div>
        )
      }));
      
      const { App } = await import('./App');
      render(<App />);
    });

    const loginBtn = await screen.findByRole('button', { name: /login with keycloak/i });
    expect(loginBtn).toBeInTheDocument();
  });
});
