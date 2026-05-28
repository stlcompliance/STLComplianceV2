import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { DemandProcessingPanel } from './DemandProcessingPanel'

vi.mock('../api/client', () => ({
  getDemandProcessingDashboard: vi.fn().mockResolvedValue({
    pendingCount: 1,
    stockShortCount: 0,
    stockAvailableCount: 0,
    prDraftedCount: 0,
    processedItems: [],
    pendingItems: [
      {
        processingStateId: null,
        demandRefId: 'ref-1',
        demandRefSource: 'routarr',
        sourceRefKey: 'TRIP-1',
        title: 'Trip parts',
        demandRefStatus: 'received',
        processingOutcome: null,
        recommendedAction: null,
        linesTotalCount: null,
        linesCatalogCount: null,
        linesShortCount: null,
        purchaseRequestId: null,
        lastProcessingMessage: null,
        demandReceivedAt: new Date().toISOString(),
        lastProcessedAt: null,
        sourceLink: {
          productKey: 'routarr',
          displayLabel: 'RoutArr trip TRIP-1',
          referenceKey: 'TRIP-1',
        },
      },
    ],
  }),
  getDemandProcessingDetail: vi.fn(),
  retryDemandProcessing: vi.fn(),
  createDemandProcessingPrDraft: vi.fn(),
}))

describe('DemandProcessingPanel', () => {
  afterEach(() => cleanup())

  it('renders pending queue with operator controls when canOperate', async () => {
    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={qc}>
        <DemandProcessingPanel accessToken="token" canRead={true} canOperate={true} />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('demand-processing-panel')).toBeTruthy()
    expect(await screen.findByText('Pending queue')).toBeTruthy()
    expect(await screen.findByTestId('demand-processing-row-ref-1')).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Retry processing' })).toBeTruthy()
    expect(screen.getByRole('button', { name: 'Create PR draft' })).toBeTruthy()
  })

  it('returns null when canRead is false', () => {
    const qc = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    const { container } = render(
      <QueryClientProvider client={qc}>
        <DemandProcessingPanel accessToken="token" canRead={false} />
      </QueryClientProvider>,
    )
    expect(container.firstChild).toBeNull()
  })
})
