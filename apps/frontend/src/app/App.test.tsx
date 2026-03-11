import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { App } from './App';

// Mock dependencies
vi.mock('../providers/AppProviders', () => ({
  AppProviders: ({ children }: { children: React.ReactNode }) => (
    <div data-testid="mock-providers">{children}</div>
  ),
}));

vi.mock('../routes/AppRouter', () => ({
  AppRouter: () => <div data-testid="mock-router">Router Content</div>,
}));

describe('App', () => {
  it('should render AppRouter inside AppProviders', () => {
    render(<App />);

    const providersEl = screen.getByTestId('mock-providers');
    const routerEl = screen.getByTestId('mock-router');

    expect(providersEl).toBeInTheDocument();
    expect(providersEl).toContainElement(routerEl);
  });
});
