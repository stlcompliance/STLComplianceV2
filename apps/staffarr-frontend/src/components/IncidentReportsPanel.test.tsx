import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { IncidentReportsPanel } from './IncidentReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getIncidentReportSummary: vi.fn(),
    exportIncidentReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <IncidentReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('IncidentReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getIncidentReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportIncidentReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Incident report unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeTruthy()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getIncidentReportSummary).mockResolvedValue({
      totalIncidents: 1,
      openCount: 1,
      closedCount: 0,
      highSeverityOpenCount: 1,
      recentIncidents: [],
    })
    vi.mocked(client.exportIncidentReportSummaryCsv).mockRejectedValue(new Error('export down'))
    renderPanel()

    await screen.findByTestId('incident-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('CSV export failed')).toBeTruthy()
  })
})
