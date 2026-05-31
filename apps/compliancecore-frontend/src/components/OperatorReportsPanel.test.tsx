import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { OperatorReportsPanel } from './OperatorReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getOperatorReportSummary: vi.fn(),
    exportOperatorReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <OperatorReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('OperatorReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getOperatorReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportOperatorReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Operator report unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeInTheDocument()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getOperatorReportSummary).mockResolvedValue({
      evaluationTotalCount: 3,
      evaluationFailCount: 1,
      workflowGateBlockCount: 2,
      rulePackPublishedCount: 4,
      recentEvaluations: [],
    })
    vi.mocked(client.exportOperatorReportSummaryCsv).mockRejectedValue(new Error('export down'))
    renderPanel()

    await screen.findByTestId('operator-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('CSV export failed')).toBeInTheDocument()
  })
})
