import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { SupplyReadinessCheckPanel } from './SupplyReadinessCheckPanel'

const { partId } = vi.hoisted(() => ({
  partId: '11111111-1111-1111-1111-111111111111',
}))

vi.mock('../api/client', () => ({
  getPartSupplyReadiness: vi.fn().mockResolvedValue({
    partId,
    partKey: 'PART-1',
    displayName: 'Test part',
    status: 'active',
    readinessStatus: 'not_ready',
    readinessBasis: 'supply_blockers',
    calculatedAt: '2026-05-28T12:00:00Z',
    blockers: [
      {
        reasonCode: 'part_stockout',
        message: 'No available stock for this part.',
        sourceEntityType: 'part',
        sourceEntityId: partId,
        relatedEntityId: null,
      },
    ],
    availability: {
      quantityOnHand: 0,
      quantityReserved: 0,
      quantityAvailable: 0,
      reorderPoint: 5,
      activeReservationCount: 0,
      openBackorderCount: 0,
    },
  }),
  getVendorSupplyReadiness: vi.fn(),
  getProcurementPathReadiness: vi.fn(),
}))

describe('SupplyReadinessCheckPanel', () => {
  it('renders part readiness blockers when a part is selected', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <SupplyReadinessCheckPanel
          accessToken="token"
          canRead
          parts={[
            {
              partId,
              partKey: 'PART-1',
              displayName: 'Test part',
              status: 'active',
            } as never,
          ]}
          vendors={[]}
        />
      </QueryClientProvider>,
    )

    fireEvent.change(screen.getByLabelText('Part'), { target: { value: partId } })

    expect(await screen.findByText('part_stockout')).toBeInTheDocument()
    expect(screen.getByText('not_ready')).toBeInTheDocument()
  })

  it('returns null when user cannot read', () => {
    const client = new QueryClient()
    const { container } = render(
      <QueryClientProvider client={client}>
        <SupplyReadinessCheckPanel accessToken="token" canRead={false} parts={[]} vendors={[]} />
      </QueryClientProvider>,
    )
    expect(container).toBeEmptyDOMElement()
  })
})
