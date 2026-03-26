import { describe, it, expect, beforeEach, vi } from 'vitest';

const storage = {
  getItem: vi.fn(() => null),
  setItem: vi.fn(),
  removeItem: vi.fn(),
  clear: vi.fn(),
};

vi.stubGlobal('localStorage', storage);

import { useThemeStore } from './useThemeStore';

describe('useThemeStore', () => {
  beforeEach(() => {
    storage.clear();
    // Reset state before each test
    useThemeStore.setState({ theme: 'system' });
  });

  it('should have initial state theme as system', () => {
    const theme = useThemeStore.getState().theme;
    expect(theme).toBe('system');
  });

  it('should update theme to dark', () => {
    useThemeStore.getState().setTheme('dark');
    const theme = useThemeStore.getState().theme;
    expect(theme).toBe('dark');
  });

  it('should update theme to light', () => {
    useThemeStore.getState().setTheme('light');
    const theme = useThemeStore.getState().theme;
    expect(theme).toBe('light');
  });
});
