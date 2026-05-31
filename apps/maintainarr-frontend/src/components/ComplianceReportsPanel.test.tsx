import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi, afterEach } from 'vitest'
import { cleanup, fireEvent, waitFor } from '@testing-library/react'

import { ComplianceReportsPanel } from './ComplianceReportsPanel'
import * as client from '../api/client'

vi.mock('../api/client', () => ({
  getComplianceReportSummary: vi.fn().mockResolvedValue({
    generatedAt: new Date().toISOString(),
    inspectionTotals: {
      totalRuns: 12,
      completedRuns: 10,
      passedRuns: 8,
      failedRuns: 2,
      inProgressRuns: 2,
      failedChecklistAnswers: 3,
      passRatePercent: 80,
    },
    defectTotals: {
      openDefectCount: 4,
      openCriticalCount: 1,
      openHighCount: 2,
      inspectionSourcedOpenCount: 2,
      manualSourcedOpenCount: 2,
    },
    pmAdherenceTotals: {
      activeScheduleCount: 6,
      overdueCount: 1,
      dueCount: 2,
      scheduledCount: 3,
      adherencePercent: 83.3,
    },
    regulatoryKeyMirrorCount: 2,
    regulatoryKeyGroups: [
      {
        complianceKey: 'dot.annual',
        materialKey: 'vehicle.fleet',
        linkedSubjectCount: 1,
        inspectionTemplateCount: 1,
        openComplianceIssueCount: 1,
      },
    ],
    templateSummaries: [],
    attentionItems: [
      {
        assetId: 'asset-1',
        assetTag: 'TRK-01',
        assetName: 'Truck 01',
        siteRef: 'yard-a',
        issueType: 'failed_inspection',
        message: 'Failed inspection',
      },
    ],
    defectSeverityCounts: [{ key: 'high', count: 2 }],
  }),
  exportComplianceReportSummaryCsv: vi.fn(),
}))

describe('ComplianceReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('renders compliance report metrics', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ComplianceReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('compliance-reports-panel')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: /Compliance reports/i })).toBeInTheDocument()
    expect(await screen.findByText(/dot\.annual/i)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Export CSV/i })).toBeInTheDocument()
  })

  it('returns null when user cannot read compliance reports', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <ComplianceReportsPanel accessToken="token" canRead={false} canExport={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getComplianceReportSummary).mockResolvedValue({
      generatedAt: new Date().toISOString(),
      inspectionTotals: {
        totalRuns: 1,
        completedRuns: 1,
        passedRuns: 1,
        failedRuns: 0,
        inProgressRuns: 0,
        failedChecklistAnswers: 0,
        passRatePercent: 100,
      },
      defectTotals: {
        openDefectCount: 0,
        openCriticalCount: 0,
        openHighCount: 0,
        inspectionSourcedOpenCount: 0,
        manualSourcedOpenCount: 0,
      },
      pmAdherenceTotals: {
        activeScheduleCount: 1,
        overdueCount: 0,
        dueCount: 0,
        scheduledCount: 1,
        adherencePercent: 100,
      },
      regulatoryKeyMirrorCount: 0,
      regulatoryKeyGroups: [],
      templateSummaries: [],
      attentionItems: [],
      defectSeverityCounts: [],
    })
    vi.mocked(client.exportComplianceReportSummaryCsv).mockRejectedValue(new Error('export down'))

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <ComplianceReportsPanel accessToken="token" canRead={true} canExport={true} />
      </QueryClientProvider>,
    )

    await screen.findByTestId('compliance-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))

    expect(await screen.findByText('CSV export failed')).toBeInTheDocument()
    await waitFor(() => {
      expect(client.exportComplianceReportSummaryCsv).toHaveBeenCalledTimes(1)
    })
  })
})
