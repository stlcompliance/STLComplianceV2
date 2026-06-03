import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { WaiverReportsPanel } from './WaiverReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getWaiverReportSummary: vi.fn(),
    exportWaiverReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <WaiverReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('WaiverReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows waiver summary and exports csv', async () => {
    vi.mocked(client.getWaiverReportSummary).mockResolvedValue({
      totalWaivers: 2,
      pendingCount: 1,
      approvedCount: 1,
      rejectedCount: 0,
      revokedCount: 0,
      expiredCount: 0,
      expiringSoonCount: 1,
      recentWaivers: [
        {
          waiverId: 'waiver-1',
          waiverKey: 'waiver-report-key',
          packKey: 'driver_qualification',
          subjectScopeKey: 'tenant',
          status: 'approved',
          reasonCode: 'operations_override',
          effectiveAt: '2026-06-01T00:00:00Z',
          expiresAt: '2026-06-10T00:00:00Z',
          updatedAt: '2026-06-02T00:00:00Z',
        },
      ],
    })
    vi.mocked(client.exportWaiverReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Waiver reports')).toBeInTheDocument()
    expect(await screen.findByText('Waivers in scope')).toBeInTheDocument()
    expect(screen.getByText('waiver-report-key')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('Waiver reports')).toBeInTheDocument()
    expect(client.exportWaiverReportSummaryCsv).toHaveBeenCalled()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getWaiverReportSummary).mockRejectedValue(new Error('waiver summary down'))
    vi.mocked(client.exportWaiverReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Waiver report unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeInTheDocument()
  })
})
