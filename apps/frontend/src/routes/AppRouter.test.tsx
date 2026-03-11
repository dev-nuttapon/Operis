import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { AppRouter } from './AppRouter';

// Mock dependencies
vi.mock('../modules/auth', () => ({
  AuthLandingPage: () => <div data-testid="mock-auth-landing-page">Auth Landing Page</div>
}));

describe('AppRouter', () => {
  it('should render AuthLandingPage correctly', () => {
    render(<AppRouter />);
    
    // Check if the mock landing page is rendered
    expect(screen.getByTestId('mock-auth-landing-page')).toBeInTheDocument();
  });
});
