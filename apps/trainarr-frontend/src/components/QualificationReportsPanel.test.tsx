import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { QualificationReportsPanel } from './QualificationReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getQualificationReportSummary: vi.fn(),
    exportQualificationReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <QualificationReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('QualificationReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getQualificationReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportQualificationReportSummaryCsv).mockResolvedValue(new Blob(['x']))

    renderPanel()

    expect(await screen.findByText('Qualification report unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry summary' })).toBeInTheDocument()
  })
})
