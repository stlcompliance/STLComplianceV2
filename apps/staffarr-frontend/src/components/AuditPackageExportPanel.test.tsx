import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { AuditPackageExportPanel } from './AuditPackageExportPanel'

vi.mock('../api/client', () => ({
  getAuditPackageTimeline: vi.fn().mockResolvedValue({
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
    hasMore: false,
  }),
  getAuditPackageManifest: vi.fn().mockResolvedValue({
    packageVersion: '1',
    sections: [
      {
        key: 'audit_events',
        fileName: 'audit_events.json',
        label: 'Audit events',
        description: 'Tenant audit trail.',
      },
    ],
  }),
  exportAuditPackageZip: vi.fn(),
  exportAuditPackageJson: vi.fn(),
}))

describe('AuditPackageExportPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders export controls for authorized users', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditPackageExportPanel accessToken="token" canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Audit package export/)).toBeTruthy()
    expect(await screen.findByText(/people.create/)).toBeTruthy()
    expect(screen.getByRole('button', { name: /Download ZIP package/i })).toBeTruthy()
    expect(screen.getByRole('button', { name: /Preview JSON export/i })).toBeTruthy()
  })

  it('hides export actions for read-only users', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditPackageExportPanel accessToken="token" canExport={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/requires tenant admin/i)).toBeTruthy()
    expect(screen.queryByRole('button', { name: /Download ZIP package/i })).toBeNull()
  })
})
