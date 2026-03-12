import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ProtectedRoute } from '../ProtectedRoute';
import { useAuth } from '../../../modules/auth';
import { MemoryRouter, Routes, Route } from 'react-router-dom';

vi.mock('../../../modules/auth', () => ({
  useAuth: vi.fn()
}));

describe('ProtectedRoute', () => {
  it('renders a spinner when auth is not ready', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: false,
      isAuthenticated: false,
      login: vi.fn(),
      logout: vi.fn()
    });

    render(
      <MemoryRouter>
        <ProtectedRoute />
      </MemoryRouter>
    );

    // Antd Spin adds an ant-spin class
    const spinner = document.querySelector('.ant-spin');
    expect(spinner).toBeInTheDocument();
  });

  it('redirects to /login when unauthenticated', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: true,
      isAuthenticated: false,
      login: vi.fn(),
      logout: vi.fn()
    });

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route path="/protected" element={<ProtectedRoute />}>
             <Route path="" element={<div data-testid="protected-content" />} />
          </Route>
          <Route path="/login" element={<div data-testid="login-page">Login Page</div>} />
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByTestId('login-page')).toBeInTheDocument();
  });

  it('renders child routes when authenticated', () => {
    vi.mocked(useAuth).mockReturnValue({
      isReady: true,
      isAuthenticated: true,
      login: vi.fn(),
      logout: vi.fn()
    });

    render(
      <MemoryRouter initialEntries={['/protected']}>
        <Routes>
          <Route path="/protected" element={<ProtectedRoute />}>
             <Route index element={<div data-testid="protected-content">Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    );

    expect(screen.getByTestId('protected-content')).toBeInTheDocument();
  });
});
