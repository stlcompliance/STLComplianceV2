import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { ComplianceReportsPanel } from './ComplianceReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getComplianceReportSummary: vi.fn(),
    exportComplianceReportSummaryCsv: vi.fn(),
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

  it('shows export failure callout', async () => {
    vi.mocked(client.getComplianceReportSummary).mockResolvedValue({
      citationAttachmentCount: 1,
      rulePackRequirementCount: 2,
      openRemediationCount: 3,
      totalRemediationCount: 3,
      attentionItemCount: 4,
      recentRemediations: [],
    })
    vi.mocked(client.exportComplianceReportSummaryCsv).mockRejectedValue(new Error('export down'))

    renderPanel()

    await screen.findByText('Compliance reports')
    fireEvent.click(screen.getByRole('button', { name: 'Export CSV' }))

    expect(await screen.findByText('CSV export failed')).toBeInTheDocument()
    await waitFor(() => {
      expect(client.exportComplianceReportSummaryCsv).toHaveBeenCalledTimes(1)
    })
  })
})
