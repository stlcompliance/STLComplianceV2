import { renderToString } from 'react-dom/server'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import type { ReactNode } from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { App } from './App'

vi.mock('@tanstack/react-query', async (importOriginal) => {
  const actual = await importOriginal<typeof import('@tanstack/react-query')>()

  return {
    ...actual,
    useQuery: vi.fn((options: { queryKey?: unknown[] }) => {
      const key = Array.isArray(options.queryKey) ? options.queryKey : []
      const scope = key[0]
      const subScope = key[1]

      switch (true) {
        case scope === 'assurarr-session':
        case subScope === 'session':
          return {
            data: {
              userId: 'user-1',
              personId: 'person-1',
              tenantId: 'tenant-1',
              sessionId: 'session-1',
              tenantRoleKey: 'tenant_member',
              isPlatformAdmin: false,
              productKey: 'assurarr',
              hasAssurArrAccess: true,
              launchableProductKeys: ['assurarr'],
            },
            isError: false,
            isLoading: false,
            error: null,
            isSuccess: true,
          }
        case scope === 'assurarr-launch-catalog':
        case subScope === 'launch-catalog':
          return {
            data: {
              tenantId: 'tenant-1',
              tenantSlug: 'demo-stl',
              tenantDisplayName: 'STL Demo Tenant',
              currentProductKey: 'assurarr',
              products: [
                {
                  productKey: 'assurarr',
                  displayName: 'AssurArr',
                  productStatus: 'available',
                  launchUrl: '/launch/assurarr',
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
        case subScope === 'dashboard':
          return {
            data: {
              generatedAt: new Date().toISOString(),
              cards: [
                { key: 'critical', title: 'Critical', description: 'Critical issues', count: 2, tone: 'danger' },
                { key: 'holds', title: 'Holds', description: 'Active holds', count: 3, tone: 'warning' },
              ],
              recentEvents: [
                {
                  id: 'event-1',
                  subjectType: 'nonconformance',
                  subjectId: 'nc-1',
                  eventType: 'nonconformance.created',
                  details: 'New nonconformance',
                  occurredAt: new Date().toISOString(),
                },
              ],
            },
            isError: false,
            isLoading: false,
            error: null,
            isSuccess: true,
          }
        case scope === 'assurarr' && subScope === 'review':
          return {
            data: {
              id: 'review-1',
              number: 'RV-1001',
              title: 'Supplier evidence gate',
              status: 'in_review',
              severity: 'high',
              reviewType: 'audit_finding_review',
              sourceProduct: 'recordarr',
              sourceObjectRef: 'record-1',
              sourceReviewRef: 'review-request-1',
              reviewerPersonId: 'person-2',
              requestedAt: '2026-06-25T12:00:00.000Z',
              dueAt: '2026-06-30T12:00:00.000Z',
              decisionAt: null,
              decisionReason: null,
              notes: 'Need one more photo package.',
              requiredEvidenceRefs: ['record-1', 'record-2'],
              submittedEvidenceRefs: ['record-1'],
              affectedObjectRefs: ['asset-1'],
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
  isPlatformAdmin: boolean
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

vi.mock('./LaunchPage', () => ({
  LaunchPage: () => <p>Launch page</p>,
}))

function renderApp(initialEntry: string | { pathname: string; search?: string } = '/') {
  return renderToString(
    <MemoryRouter initialEntries={[initialEntry]}>
      <Routes>
        <Route path="/*" element={<App />} />
      </Routes>
    </MemoryRouter>,
  )
}

describe('AssurArr app', () => {
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
      isPlatformAdmin: true,
    }
  })

  afterEach(() => {
    mockSession = null
  })

  it('boots the dashboard and preserves platform-admin workspace state', () => {
    const html = renderApp('/')
    const normalized = html.replace(/<!-- -->/g, '')

    expect(normalized).toContain('Quality control center')
    expect(normalized).toContain('Demo Admin')
    expect(normalized).toContain('demo-stl')
    expect(normalized).toContain('admin:true')
  })

  it('routes launch paths to the launch page', () => {
    const html = renderApp({ pathname: '/launch' })

    expect(html).toContain('Launch page')
  })

  it('shows quality review evidence-package readiness on the detail page', () => {
    const html = renderApp({ pathname: '/reviews/review-1' })
    const normalized = html.replace(/<!-- -->/g, '')

    expect(normalized).toContain('Package readiness')
    expect(normalized).toContain('Partial package')
    expect(normalized).toContain('1 required reference still need to be attached.')
    expect(normalized).toContain('Collect the missing RecordArr evidence before final review.')
    expect(normalized).toContain('record-2')
  })
})
