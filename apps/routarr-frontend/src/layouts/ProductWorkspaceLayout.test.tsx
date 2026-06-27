import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { ProductWorkspaceLayout } from './ProductWorkspaceLayout'
import * as client from '../api/client'
import * as sessionStorage from '../auth/sessionStorage'

vi.mock('@stl/shared-ui', () => ({
  ProductWorkspaceFrame: ({
    children,
    workspaceSession,
    isBootstrapping,
    bootstrapError,
  }: {
    children: ReactNode
    productKey?: string
    workspaceSession: {
      userDisplayName: string
      tenantDisplayName: string
      tenantSlug: string
      isPlatformAdmin: boolean
    } | null
    isBootstrapping?: boolean
    bootstrapError?: 'forbidden' | 'expired' | null
  }) => (
    <div>
      {bootstrapError ? <p data-testid="bootstrap-error">{bootstrapError}</p> : null}
      {isBootstrapping ? <p data-testid="bootstrapping">loading</p> : null}
      {workspaceSession ? (
        <p data-testid="session-context">
          {workspaceSession.userDisplayName} · {workspaceSession.tenantDisplayName} ·{' '}
          {workspaceSession.tenantSlug} · admin:{String(workspaceSession.isPlatformAdmin)}
        </p>
      ) : null}
      {workspaceSession ? children : null}
    </div>
  ),
  resolveProductWorkspaceBootstrapError: (error: unknown) => {
    if (typeof error === 'object' && error !== null && 'status' in error) {
      const status = (error as { status: number }).status
      if (status === 403) return 'forbidden'
      if (status === 401) return 'expired'
    }
    return null
  },
  resolveSuiteHomeUrl: (url?: string) => url ?? 'http://localhost:5174/app',
  buildProductLaunchUrlMap: () => ({}),
  getLaunchCatalog: vi.fn().mockResolvedValue({
    tenantId: 'tenant',
    tenantSlug: 'demo-stl',
    tenantDisplayName: 'STL Demo Tenant',
    currentProductKey: 'routarr',
    products: [
      {
        productKey: 'routarr',
        displayName: 'RoutArr',
        productStatus: 'available',
        launchUrl: '/launch/routarr',
        isCurrentProduct: true,
      },
    ],
    generatedAt: new Date().toISOString(),
  }),
  useProductWorkspaceLaunch: () => ({
    mutate: vi.fn(),
    isPending: false,
    isError: false,
    error: null,
  }),
  formatProductLaunchError: (error: unknown) => String(error),
}))

vi.mock('../api/client', () => ({
  getSessionBootstrap: vi.fn(),
}))

vi.mock('../auth/sessionStorage', () => ({
  clearSession: vi.fn(),
  loadSession: vi.fn(),
}))

function renderLayout(initialEntry: string | { pathname: string; search?: string } = '/') {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialEntry]}>
        <Routes>
          <Route element={<ProductWorkspaceLayout />}>
            <Route path="/" element={<p>RoutArr workspace</p>} />
          </Route>
          <Route path="/launch" element={<p>Launch page</p>} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('ProductWorkspaceLayout', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('redirects handoff query to launch route', () => {
    renderLayout({ pathname: '/', search: '?handoff=abc123' })
    expect(screen.getByText('Launch page')).toBeTruthy()
  })

  it('bootstraps session context from /api/session and preserves platform-admin state', async () => {
    vi.mocked(sessionStorage.loadSession).mockReturnValue({
      accessToken: 'token',
      accessTokenExpiresAt: new Date().toISOString(),
      userId: 'user',
      personId: 'person',
      tenantId: 'tenant',
      tenantSlug: 'demo-stl',
      tenantDisplayName: 'STL Demo Tenant',
      displayName: 'Demo Admin',
      email: 'admin@demo.stl',
      isPlatformAdmin: true,
    })
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user',
      personId: 'person',
      tenantId: 'tenant',
      sessionId: 'session-1',
      tenantRoleKey: 'routarr_admin',
      isPlatformAdmin: false,
      productKey: 'routarr',
      hasRoutArrAccess: true,
      launchableProductKeys: ['routarr'],
    })

    renderLayout('/')

    await waitFor(() => {
      expect(screen.getByText('RoutArr workspace')).toBeTruthy()
    })
    expect(screen.getByTestId('session-context').textContent).toContain('Demo Admin')
    expect(screen.getByTestId('session-context').textContent).toContain('demo-stl')
    expect(screen.getByTestId('session-context').textContent).toContain('admin:true')
  })
})
