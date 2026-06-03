import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { ProductOverviewPage } from './ProductOverviewPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminProductOverview: vi.fn(),
  getPlatformAdminProductManifests: vi.fn(),
  listServiceClients: vi.fn(),
  getPlatformAdminLaunchAttempts: vi.fn(),
}))

vi.mock('../../components/platform-admin/ProductCatalogAdminPanel', () => ({
  ProductCatalogAdminPanel: () => <div>Product catalog admin</div>,
}))

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <ProductOverviewPage />
    </QueryClientProvider>,
  )
}

describe('ProductOverviewPage', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retryable error callout when products query fails', async () => {
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockRejectedValueOnce(
      new Error('products unavailable'),
    )

    renderPage()

    expect(await screen.findByText('products unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry products' })).toBeTruthy()
  })

  it('shows product manifest details and allowlist metadata', async () => {
    vi.mocked(nexarr.getPlatformAdminProductOverview).mockResolvedValue([
      {
        productKey: 'staffarr',
        displayName: 'StaffArr',
        isActive: true,
        activeEntitlementCount: 3,
        hasLaunchProfile: true,
        launchProfileActive: true,
        baseUrl: 'https://staffarr.example.com',
      },
    ])
    vi.mocked(nexarr.getPlatformAdminProductManifests).mockResolvedValue({
      items: [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          productCategory: 'operations',
          productOwner: 'STL Compliance',
          productStatus: 'available',
          isActive: true,
          environmentKey: 'production',
          canonicalCallbackPath: '/auth/nexarr/callback',
          launchBaseUrl: 'https://staffarr.example.com',
          launchPath: '/launch',
          launchUrl: 'https://nexarr.example.com/launch/staffarr',
          apiBaseUrl: 'https://api.staffarr.example.com',
          healthUrl: 'https://api.staffarr.example.com/health',
          serviceAudience: 'stl:staffarr:api',
          marketingUrl: 'https://example.com/staffarr',
          documentationUrl: 'https://docs.example.com/staffarr',
          supportUrl: 'https://support.example.com',
          entitlementDependencyRules: 'tenant-product-entitlement-required',
          productDependencyMetadata: 'requires:nexarr',
          launchProfileModifiedAt: '2026-06-03T00:00:00Z',
          callbackAllowlist: [
            {
              entryId: 'entry-1',
              tenantId: 'tenant-1',
              urlPattern: 'https://suite.example.com/auth/nexarr/callback',
              patternType: 'prefix',
              isActive: true,
            },
          ],
          dataPlaneProfiles: [
            {
              profileId: 'profile-1',
              tenantId: 'tenant-1',
              deploymentMode: 'hosted',
              trustStatus: 'trusted',
              dataEndpointUrl: 'https://staffarr.example.com/api',
            },
          ],
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 1,
      hasNextPage: false,
    })
    vi.mocked(nexarr.listServiceClients).mockResolvedValue({
      items: [
        {
          serviceClientId: 'svc-1',
          clientKey: 'staffarr-api',
          displayName: 'StaffArr API Client',
          sourceProductKey: 'staffarr',
          allowedProductKeys: ['staffarr'],
          allowedTenantIds: ['tenant-1'],
          isActive: true,
          createdAt: '2026-05-01T00:00:00Z',
          lastUsedAt: '2026-05-02T00:00:00Z',
          failedAuthenticationAttempts: 0,
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
          tenantSlug: 'tenant-main',
          tenantDisplayName: 'Tenant Main',
          actorUserId: 'user-1',
          actorEmail: 'driver@example.com',
          actorDisplayName: 'Driver One',
          productKey: 'staffarr',
          productDisplayName: 'StaffArr',
          action: 'launch.redeem',
          result: 'Success',
          reasonCode: null,
          targetType: 'launch',
          targetId: 'launch-1',
          correlationId: 'corr-launch-1',
          occurredAt: '2026-05-02T00:00:00Z',
          remediationHint: null,
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      hasNextPage: false,
    })

    renderPage()

    expect(await screen.findByText('Product manifest explorer')).toBeTruthy()
    expect(screen.getAllByText('https://staffarr.example.com')).toHaveLength(2)
    expect(screen.getByText('https://nexarr.example.com/launch/staffarr')).toBeTruthy()
    expect(screen.getByText('https://suite.example.com/auth/nexarr/callback [prefix] · tenant tenant-1')).toBeTruthy()
    expect(screen.getByText('tenant-1 · hosted · trusted · https://staffarr.example.com/api')).toBeTruthy()
    expect(screen.getByText('Service clients')).toBeTruthy()
    expect(screen.getByText('StaffArr API Client')).toBeTruthy()
    expect(screen.getByText('staffarr-api')).toBeTruthy()
    fireEvent.change(screen.getByLabelText('Filter by product key'), {
      target: { value: 'staffarr' },
    })
    expect(await screen.findByText('Launch activity')).toBeTruthy()
    expect(screen.getByText('Tenant Main')).toBeTruthy()
    expect(screen.getByText('launch.redeem · Success · Driver One')).toBeTruthy()
    expect(nexarr.getPlatformAdminProductManifests).toHaveBeenCalledWith({
      productKey: undefined,
      tenantId: undefined,
      page: 1,
      pageSize: 20,
    })
    expect(nexarr.listServiceClients).toHaveBeenCalledWith(1, 100)
    expect(nexarr.getPlatformAdminLaunchAttempts).toHaveBeenCalledWith({
      productKey: 'staffarr',
      page: 1,
      pageSize: 10,
    })
  })
})
