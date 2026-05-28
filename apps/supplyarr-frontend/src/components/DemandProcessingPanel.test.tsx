import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { DemandProcessingPanel } from './DemandProcessingPanel'

vi.mock('../api/client', () => ({
  getDemandProcessingDashboard: vi.fn().mockResolvedValue({
    pendingCount: 1,
    stockShortCount: 1,
    stockAvailableCount: 0,
    prDraftedCount: 0,
    items: [
      {
        processingStateId: 'state-1',
        demandRefId: 'ref-1',
        maintainarrWorkOrderNumber: 'WO-DP-100',
        title: 'Brake pads',
        demandRefStatus: 'received',
        processingOutcome: 'stock_short',
        recommendedAction: 'create_purchase_request',
        linesTotalCount: 1,
        linesCatalogCount: 1,
        linesShortCount: 1,
        purchaseRequestId: null,
        lastProcessingMessage: '1 of 1 catalog-linked lines are short on stock.',
        demandReceivedAt: '2026-05-28T00:00:00Z',
        lastProcessedAt: '2026-05-28T01:00:00Z',
      },
    ],
  }),
}))

describe('DemandProcessingPanel', () => {
  it('renders dashboard when user can read', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <DemandProcessingPanel accessToken="token" canRead={true} />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('demand-processing-panel')).toBeInTheDocument()
    expect(screen.getByText('MaintainArr demand processing')).toBeInTheDocument()
    expect(await screen.findByText(/WO WO-DP-100/)).toBeInTheDocument()
  })

  it('returns null when user cannot read', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <DemandProcessingPanel accessToken="token" canRead={false} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
