import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { TenantOverviewPage } from './TenantOverviewPage'

vi.mock('../../api/nexarrClient', () => ({
  getPlatformAdminTenantOverview: vi.fn(),
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
          activeEntitlementCount: 3,
          membershipCount: 12,
          createdAt: '2026-05-01T00:00:00Z',
        },
      ],
      page: 1,
      pageSize: 100,
      totalCount: 1,
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
    expect(await screen.findByText('Tenant audit history')).toBeTruthy()
    expect(await screen.findByText('tenant.updated')).toBeTruthy()
    expect(nexarr.getTenantAuditEvents).toHaveBeenCalledWith('tenant-1', { page: 1, pageSize: 10 })
  })
})
