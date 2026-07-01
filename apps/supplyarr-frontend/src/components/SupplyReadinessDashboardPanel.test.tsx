import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SupplyReadinessDashboardPanel } from './SupplyReadinessDashboardPanel'

vi.mock('../api/client', () => ({
  getSupplyReadinessDashboard: vi.fn().mockResolvedValue({
    generatedAt: '2026-05-28T12:00:00Z',
    totals: {
      activePartsCount: 10,
      partsBelowReorderCount: 2,
      stockLineCount: 15,
      totalQuantityOnHand: 100,
      totalQuantityReserved: 20,
      totalQuantityAvailable: 80,
      openBackorderCount: 1,
      openPurchaseRequestCount: 3,
      openPurchaseOrderCount: 2,
      issuedPurchaseOrderCount: 4,
      openDemandRefCount: 5,
      complianceAttentionCount: 1,
      activeSupplierRestrictionCount: 0,
      activeProcurementExceptionCount: 1,
    },
    demandRefsBySource: [{ source: 'maintainarr', openCount: 2 }],
    attentionItems: [
      {
        category: 'stock',
        title: 'PART-1 below reorder',
        detail: 'Available 1 · reorder point 5',
        status: 'below_reorder',
        occurredAt: '2026-05-28T12:00:00Z',
        relatedEntityType: 'part',
        relatedEntityId: '11111111-1111-1111-1111-111111111111',
      },
    ],
    predictiveStockoutItems: [
      {
        partId: '11111111-1111-1111-1111-111111111111',
        partKey: 'PART-1',
        displayName: 'Filter',
        quantityAvailable: 2,
        openDemandQuantity: 5,
        openBackorderQuantity: 3,
        projectedQuantity: -6,
        shortageQuantity: 6,
        reorderPoint: 5,
        riskLevel: 'critical',
        reason: 'Projected shortage after open demand and backorders',
        sourceTimestamp: '2026-05-28T12:00:00Z',
      },
    ],
  }),
}))

describe('SupplyReadinessDashboardPanel', () => {
  it('renders dashboard when user can read', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <SupplyReadinessDashboardPanel accessToken="token" canRead />
      </QueryClientProvider>,
    )

    expect(await screen.findByTestId('supply-readiness-dashboard-panel')).toBeInTheDocument()
    expect(screen.getByText('Supply readiness')).toBeInTheDocument()
    expect(await screen.findByText('PART-1 below reorder')).toBeInTheDocument()
    expect(screen.getByText('maintainarr: 2')).toBeInTheDocument()
    expect(screen.getByText(/Predictive stockout risk/i)).toBeInTheDocument()
    expect(screen.getByText(/PART-1 · Filter/i)).toBeInTheDocument()
    expect(screen.getByText(/Projected shortage after open demand and backorders/i)).toBeInTheDocument()
  })

  it('returns null when user cannot read', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <SupplyReadinessDashboardPanel accessToken="token" canRead={false} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
