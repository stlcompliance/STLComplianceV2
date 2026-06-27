import { renderToString } from 'react-dom/server'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import type { ReactNode } from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { App } from './App'
import { clearSession, saveSession } from './auth/sessionStorage'

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
              productKey: 'reportarr',
              hasReportArrAccess: true,
              launchableProductKeys: ['reportarr'],
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
              currentProductKey: 'reportarr',
              products: [
                {
                  productKey: 'reportarr',
                  displayName: 'ReportArr',
                  productStatus: 'available',
                  launchUrl: '/launch/reportarr',
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
        case 'summary':
          return {
            data: {
              generatedAt: new Date().toISOString(),
              freshnessStatus: 'fresh',
              datasetCount: 1,
              dashboardCount: 2,
              reportDefinitionCount: 3,
              reportRunCount: 4,
              kpiCount: 5,
              alertCount: 6,
              auditPackageCount: 7,
              recentDatasets: [],
              recentDashboards: [],
              recentReports: [],
              recentAlerts: [],
              recentAuditPackages: [],
            },
            isError: false,
            isLoading: false,
            error: null,
            isSuccess: true,
          }
        case 'me':
          return {
            data: {
              userId: 'user-1',
              personId: 'person-1',
              email: 'admin@demo.stl',
              displayName: 'Demo Admin',
              tenantId: 'tenant-1',
              tenantRoleKey: 'tenant_member',
              isPlatformAdmin: false,
              productKey: 'reportarr',
              hasReportArrAccess: true,
              launchableProductKeys: ['reportarr'],
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

vi.mock('./LaunchPage', () => ({
  LaunchPage: () => <p>Launch page</p>,
}))

vi.mock('./api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('./api/client')>()
  return {
    ...actual,
    getSessionBootstrap: vi.fn(),
    getMe: vi.fn(),
    getWorkspaceSummary: vi.fn(),
  }
})

function createSessionStorageMock() {
  const store = new Map<string, string>()
  return {
    getItem: vi.fn((key: string) => store.get(key) ?? null),
    setItem: vi.fn((key: string, value: string) => {
      store.set(key, value)
    }),
    removeItem: vi.fn((key: string) => {
      store.delete(key)
    }),
  }
}

function renderApp(initialEntry: string | { pathname: string; search?: string } = '/') {
  return renderToString(
    <MemoryRouter initialEntries={[initialEntry]}>
      <Routes>
        <Route path="/*" element={<App />} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('ReportArr app', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    vi.stubGlobal('sessionStorage', createSessionStorageMock())
    saveSession({
      accessToken: 'token',
      accessTokenExpiresAt: new Date(Date.now() + 60_000).toISOString(),
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      tenantSlug: 'demo-stl',
      tenantDisplayName: 'STL Demo Tenant',
      displayName: 'Demo Admin',
      email: 'admin@demo.stl',
      isPlatformAdmin: true,
    })
  })

  afterEach(() => {
    clearSession()
    vi.unstubAllGlobals()
  })

  it('boots the dashboard and preserves platform-admin workspace state', () => {
    const html = renderApp('/')
    const normalized = html.replace(/<!-- -->/g, '')

    expect(normalized).toContain('Reporting command center')
    expect(normalized).toContain('Demo Admin')
    expect(normalized).toContain('demo-stl')
    expect(normalized).toContain('admin:true')
  })

  it('routes launch paths to the launch page', () => {
    const html = renderApp({ pathname: '/launch' })

    expect(html).toContain('Launch page')
  })
})
