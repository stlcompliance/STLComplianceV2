import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { AuditPackageExportPanel } from './AuditPackageExportPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getAuditPackageManifest: vi.fn(),
    getAuditPackageFilterOptions: vi.fn(),
    getAuditPackageExportSummary: vi.fn(),
    getAuditPackageTimeline: vi.fn(),
  }
})

function renderPanel(canExport = true) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <AuditPackageExportPanel accessToken="token" canRead canExport={canExport} />
    </QueryClientProvider>,
  )
}

describe('AuditPackageExportPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders summary and export controls when allowed', async () => {
    vi.mocked(client.getAuditPackageManifest).mockResolvedValue({
      packageVersion: '2',
      sections: [
        {
          key: 'audit_events_csv',
          fileName: 'audit_events.csv',
          label: 'Audit events (CSV)',
          description: 'CSV',
        },
      ],
    })
    vi.mocked(client.getAuditPackageFilterOptions).mockResolvedValue({
      actions: ['work_order.create'],
      results: ['success'],
      targetTypes: ['work_order'],
      actorUserIds: [],
    })
    vi.mocked(client.getAuditPackageExportSummary).mockResolvedValue({
      filters: {
        from: null,
        to: null,
        action: null,
        result: null,
        targetType: null,
        actorUserId: null,
      },
      counts: {
        auditEvents: 3,
        assets: 1,
        workOrders: 2,
        defects: 0,
        inspectionRuns: 0,
        pmSchedules: 0,
      },
      byResult: [],
      byAction: [],
      generatedAt: '2026-05-28T14:00:00Z',
    })
    vi.mocked(client.getAuditPackageTimeline).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 15,
      totalCount: 0,
      hasNextPage: false,
    })

    renderPanel(true)

    await waitFor(() => {
      expect(screen.getByTestId('maintainarr-audit-summary-counts').textContent).toContain('3 audit events')
    })
    expect(screen.getByTestId('maintainarr-audit-download-csv')).toBeTruthy()
    expect(screen.getByRole('button', { name: /Download ZIP package/i })).toBeTruthy()
  })

  it('hides export buttons when canExport is false', async () => {
    vi.mocked(client.getAuditPackageManifest).mockResolvedValue({ packageVersion: '2', sections: [] })
    vi.mocked(client.getAuditPackageFilterOptions).mockResolvedValue({
      actions: [],
      results: [],
      targetTypes: [],
      actorUserIds: [],
    })
    vi.mocked(client.getAuditPackageExportSummary).mockResolvedValue({
      filters: {
        from: null,
        to: null,
        action: null,
        result: null,
        targetType: null,
        actorUserId: null,
      },
      counts: {
        auditEvents: 0,
        assets: 0,
        workOrders: 0,
        defects: 0,
        inspectionRuns: 0,
        pmSchedules: 0,
      },
      byResult: [],
      byAction: [],
      generatedAt: '2026-05-28T14:00:00Z',
    })
    vi.mocked(client.getAuditPackageTimeline).mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 15,
      totalCount: 0,
      hasNextPage: false,
    })

    renderPanel(false)

    await waitFor(() => {
      expect(screen.getByTestId('maintainarr-audit-summary-counts')).toBeTruthy()
    })
    expect(screen.queryByRole('button', { name: /Download ZIP package/i })).toBeNull()
    expect(screen.queryByTestId('maintainarr-audit-download-csv')).toBeNull()
  })
})
