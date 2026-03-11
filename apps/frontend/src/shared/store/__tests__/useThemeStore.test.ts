import { describe, it, expect, beforeEach } from 'vitest';
import { useThemeStore } from '../useThemeStore';

describe('useThemeStore', () => {
  beforeEach(() => {
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
