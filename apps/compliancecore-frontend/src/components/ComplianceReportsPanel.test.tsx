import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { ComplianceReportsPanel } from './ComplianceReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getFindingsReportSummary: vi.fn(),
    exportFindingsReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ComplianceReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('ComplianceReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getFindingsReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportFindingsReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Compliance report unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeInTheDocument()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getFindingsReportSummary).mockResolvedValue({
      totalFindings: 1,
      openCount: 1,
      openBlockSeverityCount: 1,
      resolvedCount: 0,
      recentFindings: [],
    })
    vi.mocked(client.exportFindingsReportSummaryCsv).mockRejectedValue(new Error('export down'))
    renderPanel()

    await screen.findByTestId('compliance-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('CSV export failed')).toBeInTheDocument()
  })
})
