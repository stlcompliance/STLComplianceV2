import { cleanup, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'

import App from './App'

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-query')>()

  return {
    ...actual,
    useQuery: vi.fn((options: { queryKey?: unknown[] }) => {
      const key = Array.isArray(options.queryKey) ? options.queryKey : []
      const scope = key[1]

      switch (scope) {
        case 'session':
          return {
            data: {
              userId: 'user-1',
              personId: 'person-1',
              tenantId: 'tenant-1',
              sessionId: 'session-1',
              tenantRoleKey: 'tenant_member',
              isPlatformAdmin: false,
              productKey: 'ledgarr',
              hasLedgArrAccess: true,
              launchableProductKeys: ['ledgarr'],
            },
            isError: false,
            isLoading: false,
            error: null,
            isSuccess: true,
          }
        case 'launch-catalog':
          return {
            data: {
              tenantId: 'tenant-1',
              tenantSlug: 'demo-stl',
              tenantDisplayName: 'STL Demo Tenant',
              currentProductKey: 'ledgarr',
              products: [
                {
                  productKey: 'ledgarr',
                  displayName: 'LedgArr',
                  productStatus: 'available',
                  launchUrl: '/launch/ledgarr',
                  isCurrentProduct: true,
                },
              ],
              generatedAt: new Date().toISOString(),
            },
            isError: false,
            isLoading: false,
            error: null,
            isSuccess: true,
          }
        default:
          return {
            data: undefined,
            isError: false,
            isLoading: false,
            error: null,
            isSuccess: true,
          }
      }
    }),
    useMutation: vi.fn(() => ({
      mutate: vi.fn(),
      mutateAsync: vi.fn().mockResolvedValue(undefined),
      isPending: false,
      isError: false,
      error: null,
    })),
    useQueryClient: vi.fn(() => ({
      invalidateQueries: vi.fn(),
    })),
  }
})

let mockSession: {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  displayName: string
  email: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  launchableProductKeys: string[]
} | null = null

vi.mock('./auth/sessionStorage', () => ({
  clearSession: vi.fn(),
  loadSession: vi.fn(() => mockSession),
  saveSession: vi.fn(),
}))

vi.mock('@stl/shared-ui', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@stl/shared-ui')>()

  return {
    ...actual,
    ProductWorkspaceFrame: ({
      children,
      workspaceSession,
      isBootstrapping,
      bootstrapError,
    }: {
      children: ReactNode
      workspaceSession: {
        userDisplayName: string
        tenantDisplayName: string
        tenantSlug: string
        isPlatformAdmin: boolean
      } | null
      isBootstrapping?: boolean
      bootstrapError?: 'forbidden' | 'expired' | null
    }) => (
      <div data-testid="workspace-frame">
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
    buildProductLaunchUrlMap: () => ({}),
    formatProductLaunchError: (error: unknown) => String(error),
    getLaunchCatalog: vi.fn(),
    resolveProductWorkspaceBootstrapError: () => null,
    resolveSuiteHomeUrl: (url?: string) => url ?? '/',
    useProductWorkspaceLaunch: () => ({
      mutate: vi.fn(),
      isPending: false,
      isError: false,
      error: null,
    }),
  }
})

function renderApp(pathname: string) {
  window.history.pushState({}, '', pathname)
  return render(
    <MemoryRouter initialEntries={[pathname]}>
      <App />
    </MemoryRouter>,
  )
}

describe('LedgArr app', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockSession = {
      accessToken: 'token',
      accessTokenExpiresAt: new Date(Date.now() + 60_000).toISOString(),
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      tenantSlug: 'demo-stl',
      tenantDisplayName: 'STL Demo Tenant',
      displayName: 'Demo Admin',
      email: 'admin@demo.stl',
      tenantRoleKey: 'tenant_admin',
      isPlatformAdmin: true,
      launchableProductKeys: ['ledgarr'],
    }
  })

  afterEach(() => {
    cleanup()
    mockSession = null
  })

  it('boots the dashboard and preserves platform-admin workspace state', async () => {
    renderApp('/')

    await waitFor(() => {
      expect(screen.getByRole('heading', { name: 'ERP control center' })).toBeInTheDocument()
    })

    expect(screen.getByTestId('session-context')).toHaveTextContent('Demo Admin')
    expect(screen.getByTestId('session-context')).toHaveTextContent('demo-stl')
    expect(screen.getByTestId('session-context')).toHaveTextContent('admin:true')
  })

  it('routes launch paths to the launch page', async () => {
    renderApp('/launch')

    await waitFor(() => {
      expect(screen.getByText('Missing handoff code. Launch LedgArr from the suite.')).toBeInTheDocument()
    })
  })
})
