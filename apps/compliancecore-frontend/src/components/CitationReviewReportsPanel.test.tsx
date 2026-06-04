import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { CitationReviewReportsPanel } from './CitationReviewReportsPanel'
import {
  exportCitationReviewReportSummaryCsv,
  getCitationReviewReportSummary,
} from '../api/client'

vi.mock('../api/client', () => ({
  exportCitationReviewReportSummaryCsv: vi.fn(),
  getCitationReviewReportSummary: vi.fn(),
}))

describe('CitationReviewReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.restoreAllMocks()
    vi.resetAllMocks()
  })

  function renderPanel() {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <CitationReviewReportsPanel accessToken="token" canRead canExport />
      </QueryClientProvider>,
    )
  }

  it('loads citation review rollups and exports csv', async () => {
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock')
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})
    vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => {})
    vi.mocked(getCitationReviewReportSummary).mockResolvedValue({
      tenantId: 'tenant-1',
      totalCitations: 3,
      activeCitationCount: 2,
      reviewedCitationCount: 1,
      needsReviewCitationCount: 1,
      inactiveCitationCount: 1,
      supersededCitationCount: 1,
      linkedRulePackCount: 2,
      totalFactRequirementCount: 2,
      totalMappingCount: 3,
      generatedAt: '2026-05-27T14:00:00Z',
      citations: [
        {
          citationId: 'c-1',
          citationKey: 'cfr_172_101',
          sourceReference: '49 CFR 172.101',
          programKey: 'phmsa_hmr',
          programLabel: 'PHMSA HMR',
          citationLabel: 'Hazardous materials table',
          versionNumber: 2,
          reviewState: 'reviewed',
          isActive: true,
          hasRulePack: true,
          rulePackKey: 'phmsa_hmr_operational',
          rulePackLabel: 'PHMSA HMR Operational Pack',
          factRequirementCount: 1,
          mappingCount: 1,
          supersededByCount: 0,
          supersedesCitationKey: 'cfr_172_101_v1',
          updatedAt: '2026-05-27T13:00:00Z',
          summary: 'Active citation linked to 1 fact requirement(s) and 1 mapping(s).',
        },
        {
          citationId: 'c-2',
          citationKey: 'cfr_172_102',
          sourceReference: '49 CFR 172.102',
          programKey: 'phmsa_hmr',
          programLabel: 'PHMSA HMR',
          citationLabel: 'Special provisions',
          versionNumber: 1,
          reviewState: 'needs_review',
          isActive: true,
          hasRulePack: false,
          rulePackKey: null,
          rulePackLabel: null,
          factRequirementCount: 0,
          mappingCount: 0,
          supersededByCount: 0,
          supersedesCitationKey: null,
          updatedAt: '2026-05-27T12:00:00Z',
          summary: 'Active citation needs rule-pack assignment and downstream review.',
        },
      ],
    })

    vi.mocked(exportCitationReviewReportSummaryCsv).mockResolvedValue(new Blob(['csv']))

    renderPanel()

    expect(await screen.findByText('Citation review report')).toBeInTheDocument()
    expect(await screen.findByText('cfr_172_101')).toBeInTheDocument()
    expect(screen.getByText('reviewed')).toBeInTheDocument()
    expect(screen.getByText('needs_review')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))

    await waitFor(() => {
      expect(exportCitationReviewReportSummaryCsv).toHaveBeenCalledWith('token', {
        reviewState: undefined,
        programKey: undefined,
        rulePackKey: undefined,
      })
    })

    fireEvent.change(screen.getByLabelText(/Review state/i), { target: { value: 'needs_review' } })

    await waitFor(() => {
      expect(getCitationReviewReportSummary).toHaveBeenLastCalledWith('token', {
        reviewState: 'needs_review',
        programKey: undefined,
        rulePackKey: undefined,
        limit: 10,
      })
    })
  })
})
