import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { AuditReadinessReportsPanel } from './AuditReadinessReportsPanel'

vi.mock('../api/client', () => ({
  getAuditReadinessReportSummary: vi.fn(),
  exportAuditReadinessReportSummaryCsv: vi.fn(),
}))

describe('AuditReadinessReportsPanel', () => {
  it('renders summary and exports csv', async () => {
    const originalCreateElement = document.createElement.bind(document)
    vi.spyOn(document, 'createElement').mockImplementation((tagName: string) => {
      if (tagName === 'a') {
        return {
          click: vi.fn(),
          href: '',
          download: '',
        } as unknown as HTMLAnchorElement
      }

      return originalCreateElement(tagName)
    })
    vi.spyOn(URL, 'createObjectURL').mockReturnValue('blob:mock')
    vi.spyOn(URL, 'revokeObjectURL').mockImplementation(() => {})

    vi.mocked(client.getAuditReadinessReportSummary).mockResolvedValue({
      totalForecasts: 1,
      scopesTracked: 1,
      readyCount: 1,
      cautionCount: 0,
      notReadyCount: 0,
      unknownCount: 0,
      readinessScore: 88,
      readinessLevel: 'ready',
      lowestReadinessScore: 88,
      averageReadinessScore: 88,
      lastForecastedAt: '2026-05-29T12:00:00Z',
      generatedAt: '2026-05-29T12:05:00Z',
      forecasts: [
        {
          forecastId: 'forecast-1',
          runId: 'run-1',
          scopeKey: 'tenant',
          rulePackId: 'pack-1',
          packKey: 'driver_qualification',
          readinessScore: 88,
          readinessLevel: 'ready',
          riskScore: 10,
          riskLevel: 'low',
          effectivenessScore: 90,
          effectivenessLevel: 'effective',
          missingEvidenceWarningCount: 0,
          highestMissingEvidenceSeverity: 'low',
          summary: 'Readiness forecast for driver_qualification: 88 (ready).',
          forecastedAt: '2026-05-29T12:00:00Z',
        },
      ],
    })
    vi.mocked(client.exportAuditReadinessReportSummaryCsv).mockResolvedValue(
      new Blob(['csv'], { type: 'text/csv' }),
    )

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <AuditReadinessReportsPanel accessToken="token" canRead canExport />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Audit readiness report')).toBeInTheDocument()
    expect(await screen.findByText('driver_qualification')).toBeInTheDocument()
    expect(screen.getByText('88', { selector: 'td' })).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Export CSV' }))
    await waitFor(() =>
      expect(vi.mocked(client.exportAuditReadinessReportSummaryCsv)).toHaveBeenCalledWith('token', {
        scopeKey: undefined,
        rulePackKey: undefined,
        readinessLevel: undefined,
      }),
    )
  })
})
