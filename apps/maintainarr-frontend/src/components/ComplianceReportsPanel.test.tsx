import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { ComplianceReportsPanel } from './ComplianceReportsPanel'

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
})
