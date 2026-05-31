import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { AssignmentReportsPanel } from './AssignmentReportsPanel'

vi.mock('../api/client', async (importOriginal) => {
  const actual = await importOriginal<typeof import('../api/client')>()
  return {
    ...actual,
    getAssignmentReportSummary: vi.fn(),
    exportAssignmentReportSummaryCsv: vi.fn(),
  }
})

function renderPanel() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  return render(
    <QueryClientProvider client={queryClient}>
      <AssignmentReportsPanel accessToken="token" canRead canExport />
    </QueryClientProvider>,
  )
}

describe('AssignmentReportsPanel', () => {
  afterEach(() => {
    cleanup()
    vi.clearAllMocks()
  })

  it('shows retry callout when summary fails', async () => {
    vi.mocked(client.getAssignmentReportSummary).mockRejectedValue(new Error('summary down'))
    vi.mocked(client.exportAssignmentReportSummaryCsv).mockResolvedValue(new Blob(['x']))

    renderPanel()

    expect(await screen.findByText('Assignment report unavailable')).toBeInTheDocument()
    fireEvent.click(screen.getByRole('button', { name: 'Retry summary' }))

    await waitFor(() => {
      expect(client.getAssignmentReportSummary).toHaveBeenCalledTimes(2)
    })
  })
})
