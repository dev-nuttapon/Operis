import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppRouter } from './AppRouter';

// Mock dependencies
vi.mock('../modules/auth/pages/LoginPage', () => ({
  LoginPage: () => <div data-testid="mock-login-page">Login Page</div>
}));

describe('AppRouter', () => {
  it('redirects root traffic to the login page', async () => {
    window.history.pushState({}, '', '/');

    render(<AppRouter />);

    expect(await screen.findByTestId('mock-login-page')).toBeInTheDocument();
  });
});
