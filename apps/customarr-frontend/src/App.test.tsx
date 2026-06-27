import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import App from './App'
import { clearSession, saveSession } from './auth/sessionStorage'
import * as client from './api/client'

vi.mock('@stl/shared-ui', () => ({
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
  getLaunchCatalog: vi.fn().mockResolvedValue({
    tenantId: 'tenant',
    tenantSlug: 'demo-stl',
    tenantDisplayName: 'STL Demo Tenant',
    currentProductKey: 'customarr',
    products: [
      {
        productKey: 'customarr',
        displayName: 'CustomArr',
        productStatus: 'available',
        launchUrl: '/launch/customarr',
        isCurrentProduct: true,
      },
    ],
    generatedAt: new Date().toISOString(),
  }),
  resolveProductWorkspaceBootstrapError: () => null,
  resolveSuiteHomeUrl: (url?: string) => url ?? '/',
  useProductWorkspaceLaunch: () => ({
    mutate: vi.fn(),
    isPending: false,
    isError: false,
    error: null,
  }),
  ReferenceProviderClient: class ReferenceProviderClient {
    constructor(_options: unknown) {}
  },
}))

vi.mock('./LaunchPage', () => ({
  LaunchPage: () => <p>Launch page</p>,
}))

vi.mock('./api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('./api/client')>()
  return {
    ...actual,
    getSessionBootstrap: vi.fn(),
    listCustomers: vi.fn(),
    listRequirements: vi.fn(),
    listCrmRecords: vi.fn(),
    getTenantSettings: vi.fn(),
    getDashboard: vi.fn(),
    getCrmOverview: vi.fn(),
    updateTenantSettings: vi.fn(),
  }
})

let crmRecordsByModule: Record<string, any[]> = {}

function renderApp(initialEntry: string | { pathname: string; search?: string } = '/') {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter initialEntries={[initialEntry]}>
        <Routes>
          <Route path="/*" element={<App />} />
        </Routes>
      </MemoryRouter>
    </QueryClientProvider>,
  )
}

describe('CustomArr app', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    crmRecordsByModule = {}
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
      tenantRoleKey: 'customarr_admin',
      isPlatformAdmin: true,
      launchableProductKeys: ['customarr'],
    })
    vi.mocked(client.getSessionBootstrap).mockResolvedValue({
      userId: 'user-1',
      personId: 'person-1',
      tenantId: 'tenant-1',
      sessionId: 'session-1',
      tenantRoleKey: 'customarr_admin',
      isPlatformAdmin: false,
      productKey: 'customarr',
      hasCustomArrAccess: true,
      launchableProductKeys: ['customarr'],
    })
    vi.mocked(client.listCustomers).mockResolvedValue([])
    vi.mocked(client.listRequirements).mockResolvedValue([])
    vi.mocked(client.listCrmRecords).mockImplementation(async (_accessToken, moduleKey) => crmRecordsByModule[moduleKey] ?? [])
    vi.mocked(client.getTenantSettings).mockResolvedValue({
      settings: {
        customerNumbering: {
          prefix: 'CUST',
          sequenceName: 'customer',
          paddingLength: 6,
          nextNumber: 1,
          allowManualOverride: false,
          manualOverrideRequiresPermission: true,
          displayFormat: 'CUST-{sequence}',
          uniquenessScope: 'tenant',
          preview: 'CUST-000001',
        },
      },
    } as never)
    vi.mocked(client.getDashboard).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      customerCount: 1,
      activeCustomerCount: 1,
      onboardingCustomerCount: 0,
      watchListCustomerCount: 0,
      contactCount: 2,
      siteCount: 1,
      requirementCount: 0,
      featuredCustomers: [
        {
          customerId: 'customer-1',
          customerNumber: 'CUST-000001',
          legalName: 'Acme Logistics LLC',
          displayName: 'Acme Logistics',
          dbaName: 'Acme Logistics',
          customerTypeKey: 'shipper',
          statusKey: 'active',
          tradeName: 'Acme Logistics',
          status: 'active',
          tier: 'core',
          segment: 'Logistics',
          ownerPersonId: 'person-1',
          parentCustomerId: null,
          parentCustomerName: null,
          primaryContactName: 'Alex Manager',
          primaryContactEmail: 'alex@example.com',
          siteCount: 1,
          contactCount: 2,
          requirementCount: 0,
          holdStatus: 'none',
          lastActivityAt: new Date().toISOString(),
          updatedAt: new Date().toISOString(),
        },
      ],
      recentActivity: [
        {
          activityId: 'activity-1',
          customerId: 'customer-1',
          customerNumber: 'CUST-000001',
          message: 'Customer created',
          occurredAt: new Date().toISOString(),
          kind: 'created',
        },
      ],
    })
    vi.mocked(client.getCrmOverview).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      accountCount: 1,
      leadCount: 2,
      opportunityCount: 3,
      proposalCount: 4,
      agreementCount: 5,
      openCaseCount: 6,
      openTaskCount: 7,
      blockedEligibilityCount: 8,
    })
  })

  afterEach(() => {
    cleanup()
    clearSession()
  })

  it('boots the dashboard and preserves platform-admin workspace state', async () => {
    renderApp('/')

    await waitFor(() => {
      expect(screen.getByText('Customer relationship center')).toBeInTheDocument()
    })

    expect(screen.getByTestId('session-context')).toHaveTextContent('Demo Admin')
    expect(screen.getByTestId('session-context')).toHaveTextContent('demo-stl')
    expect(screen.getByTestId('session-context')).toHaveTextContent('admin:true')
  })

  it('redirects handoff routes to launch', async () => {
    renderApp({ pathname: '/handoff', search: '?handoff=abc123' })

    await waitFor(() => {
      expect(screen.getByText('Launch page')).toBeInTheDocument()
    })
  })

  it('surfaces commercial handoff readiness for proposal and agreement records', async () => {
    crmRecordsByModule = {
      proposals: [
        {
          module: 'proposals',
          id: 'proposal-1',
          number: 'PROP-2026-0007',
          customerId: 'customer-1',
          customerName: 'Acme Logistics',
          title: 'Q3 fulfillment proposal',
          statusKey: 'accepted',
          ownerPersonId: 'person-1',
          secondaryStatusKey: 'recordarr_sent',
          value: 120000,
          dueAt: '2026-07-01T00:00:00Z',
          updatedAt: '2026-06-26T12:10:00Z',
          summary: 'Customer accepted the revised pricing.',
          sourceProductKey: 'recordarr',
          freshness: 'fresh',
        },
      ],
      agreements: [
        {
          module: 'agreements',
          id: 'agreement-1',
          number: 'AGR-2026-0007',
          customerId: 'customer-1',
          customerName: 'Acme Logistics',
          title: 'Q3 fulfillment agreement',
          statusKey: 'draft',
          ownerPersonId: 'person-1',
          secondaryStatusKey: 'signature_pending',
          value: 120000,
          dueAt: '2026-07-02T00:00:00Z',
          updatedAt: '2026-06-26T12:20:00Z',
          summary: 'Ready for agreement approval and handoff.',
          sourceProductKey: 'recordarr',
          freshness: 'fresh',
        },
      ],
    }

    renderApp({ pathname: '/commercial' })

    expect(await screen.findByRole('heading', { name: 'Proposals and agreements' })).toBeInTheDocument()
    expect(await screen.findByRole('heading', { name: 'Handoff review' })).toBeInTheDocument()
    expect(screen.getByText('Accepted proposal is waiting on agreement and OrdArr handoff.')).toBeInTheDocument()
    expect(screen.getByText('Finalize agreement metadata, then send the order handoff to OrdArr.')).toBeInTheDocument()
    expect(screen.getByText('OrdArr handoff pending')).toBeInTheDocument()
    expect(screen.getByText('Proposal Accepted')).toBeInTheDocument()
    expect(screen.getByText('Agreement Draft')).toBeInTheDocument()
  })
})
