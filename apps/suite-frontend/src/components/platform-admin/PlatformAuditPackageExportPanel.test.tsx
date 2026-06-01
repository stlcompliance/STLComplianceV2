import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as nexarr from '../../api/nexarrClient'
import { PlatformAuditPackageExportPanel } from './PlatformAuditPackageExportPanel'

vi.mock('../../api/nexarrClient', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../../api/nexarrClient')>()
  return {
    ...actual,
    getPlatformAuditPackageManifest: vi.fn(),
    getPlatformAuditPackageFilterOptions: vi.fn(),
    getPlatformAdminTenantOverview: vi.fn(),
    getPlatformAuditPackageExportSummary: vi.fn(),
    getPlatformAuditPackageTimeline: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <PlatformAuditPackageExportPanel />
    </QueryClientProvider>,
  )
}

describe('PlatformAuditPackageExportPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders manifest, summary, filters, and timeline', async () => {
    vi.mocked(nexarr.getPlatformAuditPackageManifest).mockResolvedValue({
      packageVersion: '2',
      sections: [
        {
          key: 'platform_audit_events_csv',
          fileName: 'platform_audit_events.csv',
          label: 'Platform audit events (CSV)',
          description: 'CSV',
        },
      ],
    })
    vi.mocked(nexarr.getPlatformAuditPackageFilterOptions).mockResolvedValue({
      actions: ['auth.login'],
      results: ['success'],
      targetTypes: ['user'],
      productKeys: ['nexarr'],
      actorUserIds: [],
    })
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 200,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAuditPackageExportSummary).mockResolvedValue({
      filters: {
        tenantId: null,
        from: null,
        to: null,
        action: null,
        result: null,
        targetType: null,
        actorUserId: null,
        productKey: null,
      },
      counts: {
        auditEvents: 3,
        tenants: 1,
        tenantEntitlements: 2,
        productCatalog: 7,
        platformUsers: 2,
        serviceClients: 1,
        serviceTokens: 0,
        launchProfiles: 7,
        callbackAllowlist: 1,
      },
      byResult: [{ key: 'success', count: 3 }],
      byAction: [{ key: 'auth.login', count: 2 }],
      generatedAt: '2026-05-28T14:00:00Z',
    })
    vi.mocked(nexarr.getPlatformAuditPackageTimeline).mockResolvedValue({
      items: [
        {
          auditEventId: '11111111-1111-1111-1111-111111111111',
          tenantId: null,
          actorUserId: null,
          action: 'auth.login',
          targetType: 'user',
          targetId: null,
          result: 'success',
          reasonCode: null,
          correlationId: '22222222-2222-2222-2222-222222222222',
          occurredAt: '2026-05-28T12:00:00Z',
        },
      ],
      page: 1,
      pageSize: 15,
      totalCount: 1,
      hasNextPage: false,
    })

    renderPanel()

    await waitFor(() => {
      expect(screen.getByTestId('platform-audit-summary-counts')).toHaveTextContent('3 audit events')
    })
    expect(screen.getByTestId('platform-audit-filter-action')).toBeInTheDocument()
    expect(screen.getByTestId('platform-audit-download-csv')).toBeInTheDocument()
    expect(screen.getByText('platform_audit_events.csv')).toBeInTheDocument()
    expect(screen.getByTestId('platform-audit-timeline-section')).toHaveTextContent('auth.login')
  })

  it('pages the audit timeline preview using backend pagination', async () => {
    vi.mocked(nexarr.getPlatformAuditPackageManifest).mockResolvedValue({
      packageVersion: '2',
      sections: [],
    })
    vi.mocked(nexarr.getPlatformAuditPackageFilterOptions).mockResolvedValue({
      actions: [],
      results: [],
      targetTypes: [],
      productKeys: [],
      actorUserIds: [],
    })
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 200,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAuditPackageExportSummary).mockResolvedValue({
      filters: {
        tenantId: null,
        from: null,
        to: null,
        action: null,
        result: null,
        targetType: null,
        actorUserId: null,
        productKey: null,
      },
      counts: {
        auditEvents: 1,
        tenants: 1,
        tenantEntitlements: 1,
        productCatalog: 1,
        platformUsers: 1,
        serviceClients: 1,
        serviceTokens: 1,
        launchProfiles: 1,
        callbackAllowlist: 1,
      },
      byResult: [],
      byAction: [],
      generatedAt: '2026-05-28T14:00:00Z',
    })
    vi.mocked(nexarr.getPlatformAuditPackageTimeline)
      .mockResolvedValueOnce({
        items: [],
        page: 1,
        pageSize: 15,
        totalCount: 20,
        hasNextPage: true,
      })
      .mockResolvedValueOnce({
        items: [
          {
            auditEventId: '11111111-1111-1111-1111-111111111111',
            tenantId: null,
            actorUserId: null,
            action: 'auth.login',
            targetType: 'user',
            targetId: null,
            result: 'success',
            reasonCode: null,
            correlationId: '22222222-2222-2222-2222-222222222222',
            occurredAt: '2026-05-28T12:00:00Z',
          },
        ],
        page: 2,
        pageSize: 15,
        totalCount: 20,
        hasNextPage: false,
      })

    renderPanel()

    await waitFor(() => {
      expect(nexarr.getPlatformAuditPackageTimeline).toHaveBeenCalled()
    })
    expect(vi.mocked(nexarr.getPlatformAuditPackageTimeline).mock.calls[0]?.[0]).toMatchObject({
      page: 1,
      pageSize: 15,
    })

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Next' })).toBeEnabled()
    })
    fireEvent.click(screen.getByRole('button', { name: 'Next' }))

    await waitFor(() => {
      const calls = vi.mocked(nexarr.getPlatformAuditPackageTimeline).mock.calls
      expect(calls.length).toBeGreaterThan(1)
      expect(calls[calls.length - 1]?.[0]).toMatchObject({
        page: 2,
        pageSize: 15,
      })
    })
  })

  it('shows retryable error states for summary and timeline queries', async () => {
    vi.mocked(nexarr.getPlatformAuditPackageManifest).mockResolvedValue({
      packageVersion: '2',
      sections: [],
    })
    vi.mocked(nexarr.getPlatformAuditPackageFilterOptions).mockResolvedValue({
      actions: [],
      results: [],
      targetTypes: [],
      productKeys: [],
      actorUserIds: [],
    })
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 200,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAuditPackageExportSummary).mockRejectedValue(
      new Error('summary unavailable'),
    )
    vi.mocked(nexarr.getPlatformAuditPackageTimeline).mockRejectedValue(
      new Error('timeline unavailable'),
    )

    renderPanel()

    expect(await screen.findByText('summary unavailable')).toBeInTheDocument()
    expect(await screen.findByText('timeline unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry timeline' })).toBeInTheDocument()
  })

  it('shows retryable filter-source callout when filter queries fail', async () => {
    vi.mocked(nexarr.getPlatformAuditPackageManifest).mockResolvedValue({
      packageVersion: '2',
      sections: [],
    })
    vi.mocked(nexarr.getPlatformAuditPackageFilterOptions).mockRejectedValue(
      new Error('filter options unavailable'),
    )
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 200,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAuditPackageExportSummary).mockResolvedValue({
      filters: {
        tenantId: null,
        from: null,
        to: null,
        action: null,
        result: null,
        targetType: null,
        actorUserId: null,
        productKey: null,
      },
      counts: {
        auditEvents: 1,
        tenants: 1,
        tenantEntitlements: 1,
        productCatalog: 1,
        platformUsers: 1,
        serviceClients: 1,
        serviceTokens: 1,
        launchProfiles: 1,
        callbackAllowlist: 1,
      },
      byResult: [],
      byAction: [],
      generatedAt: '2026-05-28T14:00:00Z',
    })
    vi.mocked(nexarr.getPlatformAuditPackageTimeline).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 15,
      totalCount: 0,
      hasNextPage: false,
    })

    renderPanel()

    expect(await screen.findByText('filter options unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry filters' })).toBeInTheDocument()
  })

  it('shows retryable manifest callout when manifest query fails', async () => {
    vi.mocked(nexarr.getPlatformAuditPackageManifest).mockRejectedValue(
      new Error('manifest unavailable'),
    )
    vi.mocked(nexarr.getPlatformAuditPackageFilterOptions).mockResolvedValue({
      actions: [],
      results: [],
      targetTypes: [],
      productKeys: [],
      actorUserIds: [],
    })
    vi.mocked(nexarr.getPlatformAdminTenantOverview).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 200,
      totalCount: 0,
      hasNextPage: false,
    })
    vi.mocked(nexarr.getPlatformAuditPackageExportSummary).mockResolvedValue({
      filters: {
        tenantId: null,
        from: null,
        to: null,
        action: null,
        result: null,
        targetType: null,
        actorUserId: null,
        productKey: null,
      },
      counts: {
        auditEvents: 1,
        tenants: 1,
        tenantEntitlements: 1,
        productCatalog: 1,
        platformUsers: 1,
        serviceClients: 1,
        serviceTokens: 1,
        launchProfiles: 1,
        callbackAllowlist: 1,
      },
      byResult: [],
      byAction: [],
      generatedAt: '2026-05-28T14:00:00Z',
    })
    vi.mocked(nexarr.getPlatformAuditPackageTimeline).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 15,
      totalCount: 0,
      hasNextPage: false,
    })

    renderPanel()

    expect(await screen.findByText('manifest unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry manifest' })).toBeInTheDocument()
  })
})
