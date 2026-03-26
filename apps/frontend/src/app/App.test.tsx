import React from 'react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render as tlRender, screen } from '@testing-library/react';

function render(ui: React.ReactElement) {
  return tlRender(ui);
}

vi.mock('../providers/AppProviders', () => ({
  AppProviders: ({ children }: { children: React.ReactNode }) => <div data-testid="mock-providers">{children}</div>,
}));

vi.mock('../routes/AppRouter', () => ({
  AppRouter: () => <div>auth.login_button</div>,
}));

describe('App - Full Smoke Test', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the application correctly via Providers into Landing Area (Integration) without crashing', async () => {
    const { App } = await import('./App');
    render(<App />);

    const loginBtn = await screen.findByText('auth.login_button');
    expect(loginBtn).toBeInTheDocument();
    expect(screen.getByTestId('mock-providers')).toBeInTheDocument();
  });
});
