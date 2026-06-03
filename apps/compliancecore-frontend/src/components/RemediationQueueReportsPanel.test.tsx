import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { RemediationQueueReportsPanel } from './RemediationQueueReportsPanel'

vi.mock('../api/client', () => ({
  getRemediationQueueReportSummary: vi.fn(),
  exportRemediationQueueReportSummaryCsv: vi.fn(),
}))

describe('RemediationQueueReportsPanel', () => {
  it('renders remediation queue summary and exports csv', async () => {
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

    vi.mocked(client.getRemediationQueueReportSummary).mockResolvedValue({
      totalWarnings: 1,
      queuedCount: 1,
      criticalCount: 0,
      highCount: 1,
      mediumCount: 0,
      lowCount: 0,
      lastEvaluatedAt: '2026-05-29T12:00:00Z',
      generatedAt: '2026-05-29T12:05:00Z',
      queueItems: [
        {
          warningId: 'warning-1',
          runId: 'run-1',
          rulePackId: 'pack-1',
          packKey: 'driver_qualification',
          factKey: 'driver_license_valid',
          warningType: 'rule_pack_fact',
          severity: 'high',
          reasonCode: 'missing_mirror',
          queueState: 'open',
          recommendedAction: 'Provision the required mirror or product source at this scope.',
          hasMirrorAtScope: false,
          isRequiredInRule: true,
          isRequiredInCatalog: true,
          summary: 'Predicted missing evidence for driver_qualification/driver_license_valid: missing_mirror (high) — no mirror at scope.',
          evaluatedAt: '2026-05-29T12:00:00Z',
        },
      ],
    })
    vi.mocked(client.exportRemediationQueueReportSummaryCsv).mockResolvedValue(
      new Blob(['csv'], { type: 'text/csv' }),
    )

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <RemediationQueueReportsPanel accessToken="token" canRead canExport />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Remediation queue report')).toBeInTheDocument()
    expect(await screen.findByText('driver_qualification')).toBeInTheDocument()
    expect(await screen.findByText('Provision the required mirror or product source at this scope.')).toBeInTheDocument()

    fireEvent.click(screen.getByRole('button', { name: 'Export CSV' }))
    await waitFor(() =>
      expect(vi.mocked(client.exportRemediationQueueReportSummaryCsv)).toHaveBeenCalledWith('token', {
        queueOnly: true,
        severity: undefined,
        rulePackKey: undefined,
      }),
    )
  })
})
