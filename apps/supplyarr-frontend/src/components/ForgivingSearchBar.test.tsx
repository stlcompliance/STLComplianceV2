import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { ForgivingSearchBar } from './ForgivingSearchBar'

vi.mock('../api/client', () => ({
  forgivingSearch: vi.fn().mockResolvedValue({
    query: 'acme',
    normalizedQuery: 'acme',
    totalCount: 1,
    results: [
      {
        entityType: 'supplier',
        entityId: 'supplier-1',
        primaryKey: 'ACME',
        title: 'Acme Supply',
        subtitle: 'supplier identity · approved',
        deepLinkPath: '/suppliers',
        matchScore: 85,
      },
    ],
  }),
}))

describe('ForgivingSearchBar', () => {
  it('runs search and shows results', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <ForgivingSearchBar accessToken="token" canSearch={true} />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    const input = screen.getByPlaceholderText(/Search suppliers, parts, SKUs/i)
    fireEvent.change(input, { target: { value: 'acme' } })

    await waitFor(() => {
      expect(screen.getByText(/ACME · Acme Supply/i)).toBeInTheDocument()
    })
  })

  it('returns null when search is not permitted', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <MemoryRouter>
          <ForgivingSearchBar accessToken="token" canSearch={false} />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })
})
