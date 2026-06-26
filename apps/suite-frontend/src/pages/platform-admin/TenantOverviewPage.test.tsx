import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { TenantOverviewPage } from './TenantOverviewPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminTenantOverview: vi.fn(),
  getTenant: vi.fn(),
  getTenantMembers: vi.fn(),
  listProducts: vi.fn(),
  listServiceClients: vi.fn(),
  getPlatformAdminLaunchAttempts: vi.fn(),
  getTenantAuditEvents: vi.fn(),
}))

vi.mock('../../components/platform-admin/TenantCatalogAdminPanel', () => ({
  TenantCatalogAdminPanel: () => <div>Tenant catalog admin</div>,
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <TenantOverviewPage />
    </QueryClientProvider>,
  )
}

describe('TenantOverviewPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable error callout when tenants query fails', async () => {
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockRejectedValueOnce(
      new Error('tenants unavailable'),
    )

    renderPage()

    expect(await screen.findByText('tenants unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry tenants' })).toBeTruthy()
  })

  it('shows tenant audit history for the selected tenant', async () => {
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [
        {
          tenantId: 'tenant-1',
          slug: 'main',
          displayName: 'Main Tenant',
          status: 'active',
          launchableDestinationCount: 3,
          membershipCount: 12,
          createdAt: '2026-05-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getTenant).mockResolvedValue({
      tenantId: 'tenant-1',
      slug: 'main',
      displayName: 'Main Tenant',
      status: 'active',
      subscriptionTier: 'standard',
      billingCustomerId: 'cus_123',
      billingSubscriptionId: 'sub_123',
      billingGraceDays: 7,
      isTrial: true,
      isInternalTenant: false,
      createdAt: '2026-05-01T00:00:00Z',
      modifiedAt: '2026-05-03T00:00:00Z',
    })
    vi.mocked(nexarr.getTenantMembers).mockResolvedValue({
      tenantId: 'tenant-1',
      members: [
        {
          membershipId: 'membership-1',
          userId: 'user-1',
          email: 'driver@example.com',
          displayName: 'Driver One',
          roleKey: 'tenant_admin',
          isActive: true,
          createdAt: '2026-05-01T00:00:00Z',
        },
      ],
    })
    vi.mocked(nexarr.listProducts).mockResolvedValue({
      items: [
        {
          productKey: 'maintainarr',
          displayName: 'MaintainArr',
          sortOrder: 1,
          isActive: true,
          productCategory: 'operations',
          productOwner: 'Product',
          productStatus: 'available',
          canonicalCallbackPath: '/launch/maintainarr/callback',
          apiBaseUrl: 'https://api.example.test',
          healthUrl: 'https://api.example.test/health',
          serviceAudience: 'maintainarr-api',
          marketingUrl: '',
          documentationUrl: '',
          supportUrl: '',
          environmentKey: 'development',
          launchDependencyRules: '',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listServiceClients).mockResolvedValue({
      items: [
        {
          serviceClientId: 'svc-1',
          clientKey: 'maintainarr-api',
          displayName: 'MaintainArr API Client',
          sourceProductKey: 'maintainarr',
          allowedProductKeys: ['maintainarr', 'staffarr'],
          allowedTenantIds: ['tenant-1'],
          isActive: true,
          createdAt: '2026-05-01T00:00:00Z',
          lastUsedAt: '2026-05-06T00:00:00Z',
          failedAuthenticationAttempts: 2,
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAdminLaunchAttempts).mockResolvedValue({
      items: [
        {
          auditEventId: 'launch-1',
          tenantId: 'tenant-1',
          tenantSlug: 'main',
          tenantDisplayName: 'Main Tenant',
          actorUserId: 'user-1',
          actorEmail: 'driver@example.com',
          actorDisplayName: 'Driver One',
          productKey: 'maintainarr',
          productDisplayName: 'MaintainArr',
          action: 'launch.redeem',
          result: 'Success',
          reasonCode: null,
          targetType: 'launch',
          targetId: 'launch-1',
          correlationId: 'corr-launch-1',
          occurredAt: '2026-05-06T00:00:00Z',
          remediationHint: null,
        },
        {
          auditEventId: 'launch-2',
          tenantId: 'tenant-1',
          tenantSlug: 'main',
          tenantDisplayName: 'Main Tenant',
          actorUserId: 'user-1',
          actorEmail: 'driver@example.com',
          actorDisplayName: 'Driver One',
          productKey: 'staffarr',
          productDisplayName: 'StaffArr',
          action: 'launch.redeem',
          result: 'Denied',
          reasonCode: 'not_available',
          targetType: 'launch',
          targetId: 'launch-2',
          correlationId: 'corr-launch-2',
          occurredAt: '2026-05-05T00:00:00Z',
          remediationHint: 'Activate or reactivate the tenant launch availability for the requested product.',
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 2,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getTenantAuditEvents).mockResolvedValue({
      items: [
        {
          auditEventId: 'audit-1',
          tenantId: 'tenant-1',
          actorUserId: 'actor-1',
          action: 'tenant.updated',
          targetType: 'tenant',
          targetId: 'tenant-1',
          result: 'Success',
          reasonCode: null,
          correlationId: 'corr-1',
          occurredAt: '2026-05-02T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      hasNextPage: false,
    })

    renderPage()

    expect(await screen.findByText('Main Tenant')).toBeTruthy()
    expect(await screen.findByText('Tenant detail')).toBeTruthy()
    expect(screen.getByText('tenant-1')).toBeTruthy()
    expect(screen.getByText('Modified')).toBeTruthy()
    expect(await screen.findByText('Tenant members and product destinations')).toBeTruthy()
    expect(screen.getByText('Current tenant members and product destinations for Main Tenant')).toBeTruthy()
    expect(await screen.findByText('Driver One')).toBeTruthy()
    expect(screen.getByText('Tenant Admin · Enabled')).toBeTruthy()
    const membersSection =
      screen.getByText('Tenant members and product destinations').closest('section')?.textContent ?? ''
    expect(membersSection).toContain('MaintainArr')
    expect(membersSection).toContain('Active tenant members can launch this destination. Product-local permissions are checked after launch.')
    expect(await screen.findByRole('heading', { name: 'Service clients' })).toBeTruthy()
    expect(await screen.findByText('MaintainArr API Client')).toBeTruthy()
    expect(screen.getByText('maintainarr-api')).toBeTruthy()
    expect(await screen.findByText('Launch history')).toBeTruthy()
    const launchSection = screen.getByText('Launch history').closest('section')?.textContent ?? ''
    expect(launchSection).toContain('MaintainArr')
    expect(launchSection).toContain('StaffArr')
    expect(launchSection).toContain('Product unavailable')
    expect(launchSection).toContain('product_unavailable')
    expect(launchSection).not.toContain('raw not_available')
    expect(launchSection).toContain('Confirm the tenant is active, then review the destination product status and local permissions.')
    expect(await screen.findByText('Tenant audit history')).toBeTruthy()
    expect(await screen.findByText('tenant.updated')).toBeTruthy()
    expect(nexarr.getTenant).toHaveBeenCalledWith('tenant-1')
    expect(nexarr.getTenantMembers).toHaveBeenCalledWith('tenant-1')
    expect(nexarr.listProducts).toHaveBeenCalledWith(1, 100)
    expect(nexarr.listServiceClients).toHaveBeenCalledWith(1, 100)
    expect(nexarr.getPlatformAdminLaunchAttempts).toHaveBeenCalledWith({
      tenantId: 'tenant-1',
      page: 1,
      pageSize: 10,
    })
    expect(nexarr.getTenantAuditEvents).toHaveBeenCalledWith('tenant-1', { page: 1, pageSize: 10 })
  })
})
