import { describe, it, expect, vi, beforeEach } from 'vitest';

describe('Environment Config', () => {
  beforeEach(() => {
    vi.resetModules();
  });

  it('should parse environment variables correctly', async () => {
    // Mock import.meta.env
    vi.stubEnv('MODE', 'dev');
    vi.stubEnv('VITE_APP_NAME', 'Test App');
    vi.stubEnv('VITE_API_BASE_URL', 'http://api.test');
    vi.stubEnv('VITE_AUTH_BASE_URL', 'http://auth.test');
    vi.stubEnv('VITE_KEYCLOAK_URL', 'http://keycloak.test');
    vi.stubEnv('VITE_KEYCLOAK_REALM', 'test-realm');
    vi.stubEnv('VITE_KEYCLOAK_CLIENT_ID', 'test-client');

    const { appEnv } = await import('../env');
    
    expect(appEnv.mode).toBe('dev');
    expect(appEnv.appName).toBe('Test App');
    expect(appEnv.apiBaseUrl).toBe('http://api.test');
    expect(appEnv.keycloakRealm).toBe('test-realm');
  });

  it('should normalize app-local mode correctly', async () => {
    // Override readRequiredEnv to not throw error
    vi.stubEnv('MODE', 'app-local');
    vi.stubEnv('VITE_APP_NAME', 'Test App');
    vi.stubEnv('VITE_API_BASE_URL', 'http://api.test');
    vi.stubEnv('VITE_AUTH_BASE_URL', 'http://auth.test');
    vi.stubEnv('VITE_KEYCLOAK_URL', 'http://keycloak.test');
    vi.stubEnv('VITE_KEYCLOAK_REALM', 'test-realm');
    vi.stubEnv('VITE_KEYCLOAK_CLIENT_ID', 'test-client');

    const { appEnv } = await import('../env');
    
    expect(appEnv.mode).toBe('local'); // Should normalize
  });
});
