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
    metadata: {
      filterStatusOptions: [],
      internalStatusOptions: [],
      vendorPortalStatusOptions: [
        { value: 'acknowledged', label: 'Acknowledged', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'in_progress', label: 'In progress', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'partially_ready', label: 'Partially ready', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'completed_ready_for_dispatch', label: 'Completed ready for dispatch', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'unable_to_fulfill', label: 'Unable to fulfill', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
      ],
      documentTypeOptions: [
        { value: 'photo', label: 'Photo', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'packing_slip', label: 'Packing slip', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'scale_ticket', label: 'Scale ticket', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'proof_of_readiness', label: 'Proof of readiness', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'other', label: 'Other', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
      ],
      brokerDecisionTypeOptions: [],
    },
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
    metadata: {
      filterStatusOptions: [],
      internalStatusOptions: [],
      vendorPortalStatusOptions: [
        { value: 'acknowledged', label: 'Acknowledged', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'in_progress', label: 'In progress', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'partially_ready', label: 'Partially ready', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'completed_ready_for_dispatch', label: 'Completed ready for dispatch', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'unable_to_fulfill', label: 'Unable to fulfill', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
      ],
      documentTypeOptions: [
        { value: 'photo', label: 'Photo', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'packing_slip', label: 'Packing slip', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'scale_ticket', label: 'Scale ticket', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'proof_of_readiness', label: 'Proof of readiness', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'other', label: 'Other', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
      ],
      brokerDecisionTypeOptions: [],
    },
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
    metadata: {
      filterStatusOptions: [],
      internalStatusOptions: [],
      vendorPortalStatusOptions: [
        { value: 'acknowledged', label: 'Acknowledged', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'in_progress', label: 'In progress', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'partially_ready', label: 'Partially ready', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'completed_ready_for_dispatch', label: 'Completed ready for dispatch', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
        { value: 'unable_to_fulfill', label: 'Unable to fulfill', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
      ],
      documentTypeOptions: [
        { value: 'photo', label: 'Photo', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'packing_slip', label: 'Packing slip', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'scale_ticket', label: 'Scale ticket', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'proof_of_readiness', label: 'Proof of readiness', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
        { value: 'other', label: 'Other', owner: 'recordarr', sourceOfTruth: 'recordarr.document_type_catalog.mapped_to_supplyarr' },
      ],
      brokerDecisionTypeOptions: [],
    },
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
