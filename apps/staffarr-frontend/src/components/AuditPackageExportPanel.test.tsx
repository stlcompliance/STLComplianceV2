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
    exportAuditPackageZip: vi.fn(),
    exportAuditPackageCsv: vi.fn(),
    exportAuditPackageJson: vi.fn(),
    createAuditPackageGenerationJob: vi.fn(),
    getAuditPackageGenerationJob: vi.fn(),
    downloadAuditPackageGenerationJob: vi.fn(),
  }
})

describe('AuditPackageExportPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders export controls with filters and summary', async () => {
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
      actions: ['people.create'],
      results: ['success'],
      targetTypes: ['person'],
      actorUserIds: ['actor-user-1'],
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
        auditEvents: 2,
        people: 1,
        permissionHistory: 0,
        personCertifications: 0,
        personnelIncidents: 0,
        readinessOverrides: 0,
        trainingBlockers: 0,
      },
      byResult: [{ key: 'success', count: 2 }],
      byAction: [],
      generatedAt: '2026-05-28T14:00:00Z',
    })
    vi.mocked(client.getAuditPackageTimeline).mockResolvedValue({
      items: [
        {
          auditEventId: 'evt-1',
          actorUserId: null,
          action: 'people.create',
          targetType: 'person',
          targetId: 'person-1',
          result: 'success',
          reasonCode: null,
          correlationId: 'corr-1',
          occurredAt: '2026-01-15T12:00:00Z',
        },
      ],
      page: 1,
      pageSize: 15,
      totalCount: 1,
      hasNextPage: false,
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <AuditPackageExportPanel accessToken="token" canExport={true} />
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getByTestId('staffarr-audit-summary-counts').textContent).toContain('2 audit events')
    })
    expect(screen.getByTestId('staffarr-audit-download-csv')).toBeTruthy()
    expect(screen.getByTestId('staffarr-audit-filter-action')).toBeTruthy()
  })

  it('hides export actions for read-only users', async () => {
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
        people: 0,
        permissionHistory: 0,
        personCertifications: 0,
        personnelIncidents: 0,
        readinessOverrides: 0,
        trainingBlockers: 0,
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

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <AuditPackageExportPanel accessToken="token" canExport={false} />
      </QueryClientProvider>,
    )

    await waitFor(() => {
      expect(screen.getByText(/requires tenant admin/i)).toBeTruthy()
    })
    expect(screen.queryByTestId('staffarr-audit-download-csv')).toBeNull()
  })
})
