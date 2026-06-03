import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { CertificationReportsPanel } from './CertificationReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getCertificationReportSummary: vi.fn(),
    exportCertificationReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <CertificationReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('CertificationReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getCertificationReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportCertificationReportSummaryCsv).mockResolvedValue(new Blob(['x']))
    renderPanel()

    expect(await screen.findByText('Certification report unavailable')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeTruthy()
  })

  it('shows export failure callout', async () => {
    vi.mocked(client.getCertificationReportSummary).mockResolvedValue({
      totalPeople: 1,
      activeCertificationCount: 1,
      expiringSoonCount: 0,
      expiredCertificationCount: 0,
      missingCertificationCount: 0,
      recentCertifications: [],
    })
    vi.mocked(client.exportCertificationReportSummaryCsv).mockRejectedValue(new Error('export down'))
    renderPanel()

    await screen.findByTestId('certification-reports-panel')
    fireEvent.click(screen.getByRole('button', { name: /Export CSV/i }))
    expect(await screen.findByText('CSV export failed')).toBeTruthy()
  })
})
