import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import { AuditHistoryPanel } from './AuditHistoryPanel'

vi.mock('../api/client', () => ({
  listAuditHistory: vi.fn().mockResolvedValue({
    items: [
      {
        id: 'audit-1',
        actorUserId: 'user-1',
        action: 'supplyarr.parties.create',
        targetType: 'external_party',
        targetId: 'party-1',
        result: 'success',
        reasonCode: null,
        correlationId: 'corr-1',
        occurredAt: new Date().toISOString(),
      },
    ],
    nextCursor: null,
    hasMore: false,
  }),
}))

describe('AuditHistoryPanel', () => {
  it('renders audit history rows', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditHistoryPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('audit-history-panel')).toBeInTheDocument()
    expect(await screen.findByText('supplyarr.parties.create')).toBeInTheDocument()
  })

  it('returns null when user cannot read audit history', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <AuditHistoryPanel accessToken="token" canRead={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('shows retry callout when audit history fails', async () => {
    const { listAuditHistory } = await import('../api/client')
    vi.mocked(listAuditHistory).mockRejectedValueOnce(new Error('audit down'))
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <AuditHistoryPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Audit history unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry history' })).toBeInTheDocument()
  })
})
