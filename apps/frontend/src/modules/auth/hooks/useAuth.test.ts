import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useAuth } from './useAuth';
import { AuthProvider } from '../components/AuthProvider';

// Mock the keycloakAuth service
vi.mock('../services/keycloakAuth', () => ({
  initKeycloak: vi.fn(),
  login: vi.fn(),
  logout: vi.fn(),
  bindAuthEvents: vi.fn(),
  getTokenParsed: vi.fn(() => null),
  getUserProfile: vi.fn(async () => null),
  isAuthenticated: vi.fn(() => false),
  refreshToken: vi.fn(async () => true),
}));

import { initKeycloak } from '../services/keycloakAuth';

describe('useAuth hook', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should initialize correctly when authenticated', async () => {
    vi.mocked(initKeycloak).mockResolvedValueOnce(true);

    const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });

    // Initially, it should not be ready and not authenticated
    expect(result.current.isReady).toBe(false);
    expect(result.current.isAuthenticated).toBe(false);

    // Wait for the hook to finish initializing
    await waitFor(() => {
      expect(result.current.isReady).toBe(true);
    });

    expect(result.current.isAuthenticated).toBe(true);
    expect(initKeycloak).toHaveBeenCalledTimes(1);
  });

  it('should initialize correctly when unauthenticated', async () => {
    vi.mocked(initKeycloak).mockResolvedValueOnce(false);

    const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });

    await waitFor(() => {
      expect(result.current.isReady).toBe(true);
    });

    expect(result.current.isAuthenticated).toBe(false);
  });
  
  it('should handle init errors gracefully', async () => {
    vi.mocked(initKeycloak).mockRejectedValueOnce(new Error('init failed'));

    const { result } = renderHook(() => useAuth(), { wrapper: AuthProvider });

    await waitFor(() => {
      expect(result.current.isReady).toBe(true);
    });

    expect(result.current.isAuthenticated).toBe(false);
  });
});
