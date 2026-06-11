import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { VendorOrderPortalPage } from './VendorOrderPortalPage'

vi.mock('../../api/vendorOrderClient', () => ({
  getVendorAccessOrder: vi.fn().mockResolvedValue({
    vendorOrderId: 'vendor-order-1',
    status: 'in_progress',
    linkExpiresAt: '2026-06-20T12:00:00Z',
    vendorNameSnapshot: 'Acme Aggregates',
    itemDescription: 'Crushed stone',
    orderedQuantity: 520,
    quantityReady: 200,
    quantityRemaining: 320,
    quantityUom: 'units',
    expectedReadyAt: '2026-06-12T12:00:00Z',
    confirmedReadyAt: null,
    pickupWindowStart: '2026-06-12T14:00:00Z',
    pickupWindowEnd: '2026-06-12T18:00:00Z',
    pickupLocationNameSnapshot: 'North Quarry',
    pickupAddressSnapshot: '100 Quarry Road',
    deliveryLocationNameSnapshot: null,
    deliveryAddressSnapshot: null,
    pickupInstructions: 'Call dock on arrival',
    statusHistory: [
      {
        statusUpdateId: 'status-1',
        previousStatus: 'acknowledged',
        newStatus: 'in_progress',
        quantityReady: 200,
        quantityRemaining: 320,
        orderedQuantitySnapshot: 520,
        note: 'Loading crew on site',
        createdAt: '2026-06-11T10:00:00Z',
      },
    ],
    documents: [],
  }),
  submitVendorAccessOrderStatus: vi.fn().mockResolvedValue({
    vendorOrderId: 'vendor-order-1',
    status: 'completed_ready_for_dispatch',
    linkExpiresAt: '2026-06-20T12:00:00Z',
    vendorNameSnapshot: 'Acme Aggregates',
    itemDescription: 'Crushed stone',
    orderedQuantity: 520,
    quantityReady: 520,
    quantityRemaining: 0,
    quantityUom: 'units',
    expectedReadyAt: '2026-06-12T12:00:00Z',
    confirmedReadyAt: '2026-06-12T13:00:00Z',
    pickupWindowStart: '2026-06-12T14:00:00Z',
    pickupWindowEnd: '2026-06-12T18:00:00Z',
    pickupLocationNameSnapshot: 'North Quarry',
    pickupAddressSnapshot: '100 Quarry Road',
    deliveryLocationNameSnapshot: null,
    deliveryAddressSnapshot: null,
    pickupInstructions: 'Call dock on arrival',
    statusHistory: [
      {
        statusUpdateId: 'status-2',
        previousStatus: 'in_progress',
        newStatus: 'completed_ready_for_dispatch',
        quantityReady: 520,
        quantityRemaining: 0,
        orderedQuantitySnapshot: 520,
        note: 'Ready for pickup',
        createdAt: '2026-06-11T11:00:00Z',
      },
    ],
    documents: [],
  }),
  registerVendorAccessOrderDocument: vi.fn().mockResolvedValue({
    vendorOrderId: 'vendor-order-1',
    status: 'in_progress',
    linkExpiresAt: '2026-06-20T12:00:00Z',
    vendorNameSnapshot: 'Acme Aggregates',
    itemDescription: 'Crushed stone',
    orderedQuantity: 520,
    quantityReady: 200,
    quantityRemaining: 320,
    quantityUom: 'units',
    expectedReadyAt: '2026-06-12T12:00:00Z',
    confirmedReadyAt: null,
    pickupWindowStart: '2026-06-12T14:00:00Z',
    pickupWindowEnd: '2026-06-12T18:00:00Z',
    pickupLocationNameSnapshot: 'North Quarry',
    pickupAddressSnapshot: '100 Quarry Road',
    deliveryLocationNameSnapshot: null,
    deliveryAddressSnapshot: null,
    pickupInstructions: 'Call dock on arrival',
    statusHistory: [],
    documents: [],
  }),
}))

describe('VendorOrderPortalPage', () => {
  it('requires readiness confirmation before allowing a ready-for-pickup update', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/vendor-portal/orders/token-1']}>
          <Routes>
            <Route path="/vendor-portal/orders/:token" element={<VendorOrderPortalPage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Order readiness confirmation')).toBeInTheDocument()
    expect(screen.getByText('Acme Aggregates')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Status'), {
      target: { value: 'completed_ready_for_dispatch' },
    })

    expect(
      screen.getByText(/readiness confirmation checkbox is required/i),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Submit readiness update' })).toBeDisabled()

    fireEvent.click(
      screen.getByLabelText(/I confirm this order is complete, staged, and ready for pickup/i),
    )
    fireEvent.click(screen.getByRole('button', { name: 'Submit readiness update' }))

    const { submitVendorAccessOrderStatus } = await import('../../api/vendorOrderClient')
    await waitFor(() => {
      expect(submitVendorAccessOrderStatus).toHaveBeenCalledWith(
        'token-1',
        expect.objectContaining({
          newStatus: 'completed_ready_for_dispatch',
          readyForPickupConfirmed: true,
        }),
      )
    })
  })
})
