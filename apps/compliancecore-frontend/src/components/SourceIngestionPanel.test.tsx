import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { SourceIngestionPanel } from './SourceIngestionPanel'

vi.mock('../api/client', () => ({
  listSourceIngestionBatches: vi.fn().mockResolvedValue([]),
  validateFactSourceIngestion: vi.fn(),
  commitFactSourceIngestion: vi.fn(),
}))

describe('SourceIngestionPanel', () => {
  afterEach(() => {
    cleanup()
  })

  it('renders ingestion title and validate control for managers', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SourceIngestionPanel accessToken="token" canManage={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText(/Source ingestion/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Validate batch/i })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /Commit batch/i })).toBeInTheDocument()
  })

  it('shows read-only notice for non-managers', () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SourceIngestionPanel accessToken="token" canManage={false} />
      </QueryClientProvider>,
    )

    expect(screen.getByText(/requires compliance admin/i)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /Validate batch/i })).not.toBeInTheDocument()
  })
})
