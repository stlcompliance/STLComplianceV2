import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'

import * as client from '../api/client'
import { ProcurementCoordinationPanel } from './ProcurementCoordinationPanel'

vi.mock('../api/client', () => ({
  getProcurementCoordinationDashboard: vi.fn().mockResolvedValue({
    activeCount: 1,
    terminalCount: 0,
    stageCounts: [{ coordinationStage: 'awaiting_pr_approval', count: 1 }],
    items: [
      {
        coordinationRecordId: 'rec-1',
        subjectType: 'purchase_request',
        subjectId: 'pr-1',
        documentKey: 'PR-001',
        title: 'Shop restock',
        coordinationStage: 'awaiting_pr_approval',
        nextActionRequired: 'Approve or reject purchase request',
        purchaseRequestId: 'pr-1',
        purchaseOrderId: null,
        vendorPartyId: 'vendor-1',
        vendorDisplayName: 'Acme Supply',
        documentStatus: 'submitted',
        lineCount: 1,
        quantityOrdered: 0,
        quantityReceived: 0,
        receiptProgressPercent: null,
        isTerminal: false,
        sourceUpdatedAt: new Date().toISOString(),
        computedAt: new Date().toISOString(),
        isMaterialized: true,
      },
    ],
  }),
}))

describe('ProcurementCoordinationPanel', () => {
  it('renders coordination dashboard items', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ProcurementCoordinationPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('procurement-coordination-panel')).toBeInTheDocument()
    expect(await screen.findByText(/PR-001 · Shop restock/i)).toBeInTheDocument()
    expect(await screen.findByText(/Approve or reject purchase request/i)).toBeInTheDocument()
  })

  it('returns null when user cannot read coordination', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <ProcurementCoordinationPanel accessToken="token" canRead={false} />
      </QueryClientProvider>,
    )

    expect(container).toBeEmptyDOMElement()
  })

  it('shows retryable error callout when dashboard query fails', async () => {
    vi.mocked(client.getProcurementCoordinationDashboard).mockRejectedValueOnce(
      new Error('coordination unavailable'),
    )
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <ProcurementCoordinationPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('coordination unavailable')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Retry dashboard' })).toBeInTheDocument()
  })
})
