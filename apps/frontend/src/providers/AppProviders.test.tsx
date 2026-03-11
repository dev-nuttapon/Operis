import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppProviders } from './AppProviders';
import { useThemeStore } from '../shared/store/useThemeStore';

// Mock matchMedia specifically for this test
const matchMediaMock = vi.fn().mockImplementation((query) => ({
  matches: false,
  media: query,
  onchange: null,
  addListener: vi.fn(),
  removeListener: vi.fn(),
  addEventListener: vi.fn(),
  removeEventListener: vi.fn(),
  dispatchEvent: vi.fn(),
}));

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: matchMediaMock,
});

vi.mock('../shared/store/useThemeStore', () => ({
  useThemeStore: vi.fn(),
}));

// Mock i18n
vi.mock('../shared/i18n/config', () => ({}));

describe('AppProviders', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders children correctly', () => {
    vi.mocked(useThemeStore).mockReturnValue({ theme: 'light', setTheme: vi.fn() });
    
    render(
      <AppProviders>
        <div data-testid="child">Test Child</div>
      </AppProviders>
    );

    expect(screen.getByTestId('child')).toBeInTheDocument();
  });

  it('listens to system theme changes when theme is system', () => {
    const addEventListenerMock = vi.fn();
    const removeEventListenerMock = vi.fn();

    matchMediaMock.mockImplementationOnce((query) => ({
      matches: true, // System is dark
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: addEventListenerMock,
      removeEventListener: removeEventListenerMock,
      dispatchEvent: vi.fn(),
    }));

    vi.mocked(useThemeStore).mockReturnValue({ theme: 'system', setTheme: vi.fn() });

    const { unmount } = render(
      <AppProviders>
        <div>Content</div>
      </AppProviders>
    );

    // Verify it added the listener for system theme changes
    expect(addEventListenerMock).toHaveBeenCalledWith('change', expect.any(Function));

    // Verify cleanup
    unmount();
    expect(removeEventListenerMock).toHaveBeenCalledWith('change', expect.any(Function));
  });
});
