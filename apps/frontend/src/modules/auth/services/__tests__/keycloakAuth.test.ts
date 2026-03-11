import { describe, it, expect, vi, beforeEach } from 'vitest';
import { initKeycloak, login, logout, getAccessToken } from '../keycloakAuth';

// Use vi.mock to mock the default export of keycloak-js
const mockKeycloakInit = vi.fn();
const mockKeycloakLogin = vi.fn();
const mockKeycloakLogout = vi.fn();

vi.mock('keycloak-js', () => {
  return {
    default: vi.fn().mockImplementation(() => ({
      init: mockKeycloakInit,
      login: mockKeycloakLogin,
      logout: mockKeycloakLogout,
      authenticated: false,
      token: 'mock-token-123',
    })),
  };
});

// Reset internal variable "initialized" before each test 
// Since it's module-scoped and not exported, we need to reset module 
// for strict isolation
describe('keycloakAuth service', () => {

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('initKeycloak', () => {
    // Note: Due to vite module caching, the initialized flag persists across tests in this suite
    // In a real sophisticated setup we would use vi.resetModules() but it conflicts with vi.mock sometimes
    
    it('should call keycloak.init with correct options', async () => {
      mockKeycloakInit.mockResolvedValueOnce(true);
      
      const authenticated = await initKeycloak();
      
      expect(mockKeycloakInit).toHaveBeenCalledWith({
        onLoad: "check-sso",
        checkLoginIframe: false,
        pkceMethod: "S256",
      });
      expect(authenticated).toBe(true);
    });

  });

  describe('login', () => {
    it('should call keycloak.login with redirectUri', async () => {
      await login();
      expect(mockKeycloakLogin).toHaveBeenCalledWith({
        redirectUri: window.location.origin,
      });
    });
  });

  describe('logout', () => {
    it('should call keycloak.logout with redirectUri', async () => {
      await logout();
      expect(mockKeycloakLogout).toHaveBeenCalledWith({
        redirectUri: window.location.origin,
      });
    });
  });

  describe('getAccessToken', () => {
    it('should return the token from keycloak instance', () => {
      const token = getAccessToken();
      expect(token).toBe('mock-token-123');
    });
  });
});
