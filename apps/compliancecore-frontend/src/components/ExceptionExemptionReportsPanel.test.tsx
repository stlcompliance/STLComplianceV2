import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { ExceptionExemptionReportsPanel } from './ExceptionExemptionReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getExceptionExemptionReportSummary: vi.fn(),
    exportExceptionExemptionReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ExceptionExemptionReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('ExceptionExemptionReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows exception exemption summary and exports csv', async () => {
    vi.mocked(client.getExceptionExemptionReportSummary).mockResolvedValue({
      totalExceptionExemptions: 1,
      activeCount: 1,
      inactiveCount: 0,
      waiverTypeCount: 1,
      varianceTypeCount: 0,
      specialPermitTypeCount: 0,
      expiringSoonCount: 1,
      recentExceptionExemptions: [
        {
          exceptionExemptionId: 'ex-1',
          key: 'variance-one',
          label: 'Variance one',
          type: 'variance',
          effectType: 'authorizes_otherwise_blocked_action',
          packKey: 'driver_qualification',
          citationKey: 'cfr_391_11',
          activeState: 'active',
          effectiveAt: '2026-06-01T00:00:00Z',
          expiresAt: '2026-06-10T00:00:00Z',
          updatedAt: '2026-06-02T00:00:00Z',
        },
      ],
    })
    vi.mocked(client.exportExceptionExemptionReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Exception exemption reports')).toBeInTheDocument()
    expect(await screen.findByText('Exemptions in scope')).toBeInTheDocument()
    expect(screen.getByText('Variance one')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    await waitFor(() => {
      expect(client.exportExceptionExemptionReportSummaryCsv).toHaveBeenCalled()
    })
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getExceptionExemptionReportSummary).mockRejectedValue(
      new Error('exception summary down'),
    )
    vi.mocked(client.exportExceptionExemptionReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Exception exemption report unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeInTheDocument()
  })
})
