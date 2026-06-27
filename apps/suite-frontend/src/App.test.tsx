import { cleanup, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { Outlet } from 'react-router-dom'
import { afterEach, describe, expect, it, vi } from 'vitest'

import App from './App'

vi.mock('./auth/AuthProvider', () => ({
  AuthProvider: ({ children }: { children: ReactNode }) => children,
}))

vi.mock('./feedback', () => ({
  ToastProvider: ({ children }: { children: ReactNode }) => children,
}))

vi.mock('./components/RequireAuth', () => ({
  RequireAuth: () => <Outlet />,
}))

vi.mock('./components/RequirePlatformAdmin', () => ({
  RequirePlatformAdmin: () => <Outlet />,
}))

vi.mock('./layouts/AppShellLayout', () => ({
  AppShellLayout: () => <Outlet />,
}))

vi.mock('./layouts/ProductShellLayout', () => ({
  ProductShellLayout: () => <Outlet />,
}))

vi.mock('./layouts/PlatformAdminLayout', () => ({
  PlatformAdminLayout: () => <Outlet />,
}))

vi.mock('./pages/HomePage', () => ({
  HomePage: () => <p>Suite home route</p>,
}))

vi.mock('./pages/ProductSurfacePage', () => ({
  ProductSurfacePage: () => <p>Suite product surface route</p>,
}))

vi.mock('./pages/platform-admin/PlatformAdminDashboardPage', () => ({
  PlatformAdminDashboardPage: () => <p>Suite platform admin dashboard route</p>,
}))

describe('Suite app routes', () => {
  afterEach(() => {
    cleanup()
  })

  it('redirects the workspace root to /app', async () => {
    window.history.pushState({}, '', '/')

    render(<App />)

    await waitFor(() => expect(screen.getByText('Suite home route')).toBeInTheDocument())
  })

  it('renders the platform-admin dashboard route', async () => {
    window.history.pushState({}, '', '/app/platform-admin')

    render(<App />)

    await waitFor(() =>
      expect(screen.getByText('Suite platform admin dashboard route')).toBeInTheDocument(),
    )
  })

  it('renders a product surface route', async () => {
    window.history.pushState({}, '', '/app/customarr')

    render(<App />)

    await waitFor(() => expect(screen.getByText('Suite product surface route')).toBeInTheDocument())
  })
})
