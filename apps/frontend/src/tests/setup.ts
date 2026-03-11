import '@testing-library/jest-dom';
import { vi } from 'vitest';

// Mock matchMedia for jsdom
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});

// Mock Global Environment variables for tests
vi.stubEnv('VITE_APP_NAME', 'Test App');
vi.stubEnv('VITE_API_BASE_URL', 'http://api.test');
vi.stubEnv('VITE_AUTH_BASE_URL', 'http://auth.test');
vi.stubEnv('VITE_KEYCLOAK_URL', 'http://keycloak.test');
vi.stubEnv('VITE_KEYCLOAK_REALM', 'test-realm');
vi.stubEnv('VITE_KEYCLOAK_CLIENT_ID', 'test-client');
// Mock ResizeObserver for Ant Design components
vi.stubGlobal('ResizeObserver', class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
});

