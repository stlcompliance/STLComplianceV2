import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { EvidenceCompletenessReportsPanel } from './EvidenceCompletenessReportsPanel'
import {
  exportEvidenceCompletenessReportSummaryCsv,
  getEvidenceCompletenessReportSummary,
} from '../api/client'

vi.mock('../api/client', () => ({
  exportEvidenceCompletenessReportSummaryCsv: vi.fn(),
  getEvidenceCompletenessReportSummary: vi.fn(),
}))

describe('EvidenceCompletenessReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
    vi.resetAllMocks()
  })

  function renderPanel() {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <EvidenceCompletenessReportsPanel accessToken="token" canRead canExport />
      </QueryClientProvider>,
    )
  }

  it('loads evidence completeness rollups and exports csv', async () => {
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock')
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {})
    vi.mocked(getEvidenceCompletenessReportSummary).mockResolvedValue({
      tenantId: 'tenant-1',
      totalRulePacks: 2,
      completeRulePackCount: 1,
      partialRulePackCount: 1,
      incompleteRulePackCount: 0,
      totalWarnings: 3,
      criticalWarningCount: 1,
      highWarningCount: 1,
      mediumWarningCount: 1,
      lowWarningCount: 0,
      completenessScore: 78,
      generatedAt: '2026-05-27T14:00:00Z',
      rulePacks: [
        {
          rulePackId: 'rp-1',
          packKey: 'driver_qualification',
          scopeKey: 'tenant',
          totalWarnings: 2,
          criticalWarningCount: 1,
          highWarningCount: 1,
          mediumWarningCount: 0,
          lowWarningCount: 0,
          completenessScore: 55,
          completenessLevel: 'partial',
          latestWarningAt: '2026-05-27T13:00:00Z',
          summary: '2 warning(s): 1 critical, 1 high, 0 medium, 0 low.',
        },
        {
          rulePackId: 'rp-2',
          packKey: 'medical_cert',
          scopeKey: 'tenant',
          totalWarnings: 1,
          criticalWarningCount: 0,
          highWarningCount: 0,
          mediumWarningCount: 1,
          lowWarningCount: 0,
          completenessScore: 92,
          completenessLevel: 'complete',
          latestWarningAt: '2026-05-27T12:00:00Z',
          summary: '1 warning(s): 0 critical, 0 high, 1 medium, 0 low.',
        },
      ],
    })

    vi.mocked(exportEvidenceCompletenessReportSummaryCsv).mockResolvedValue(new Blob(['csv']))

    renderPanel()

    expect(await screen.findByText('Evidence completeness report')).toBeInTheDocument()
    expect(await screen.findByText('driver_qualification')).toBeInTheDocument()
    expect(screen.getByText('medical_cert')).toBeInTheDocument()
    expect(screen.getByText('78')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))

    await waitFor(() => {
      expect(exportEvidenceCompletenessReportSummaryCsv).toHaveBeenCalledWith('token', {
        scopeKey: undefined,
        severity: undefined,
        rulePackKey: undefined,
      })
    })

    fireEvent.change(screen.getByLabelText(/Severity/i), { target: { value: 'high' } })

    await waitFor(() => {
      expect(getEvidenceCompletenessReportSummary).toHaveBeenLastCalledWith('token', {
        scopeKey: undefined,
        severity: 'high',
        rulePackKey: undefined,
        limit: 10,
      })
    })
  })
})
