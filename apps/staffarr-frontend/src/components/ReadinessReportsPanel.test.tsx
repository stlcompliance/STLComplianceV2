import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { ReadinessReportsPanel } from './ReadinessReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getReadinessReportSummary: vi.fn(),
    exportReadinessReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <ReadinessReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('ReadinessReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getReadinessReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportReadinessReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Readiness report unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeTruthy()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getReadinessReportSummary).mockResolvedValue({
      totalRollups: 1,
      totalMembers: 10,
      readyCount: 8,
      notReadyCount: 2,
      overrideCount: 0,
      readyPercent: 80,
      recentRollups: [],
    })
    vi.mocked(client.exportReadinessReportSummaryCsv).mockRejectedValue(new Error('export down'))
    renderPanel()

    await screen.findByTestId('readiness-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('CSV export failed')).toBeTruthy()
  })
})
