import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { VendorOrdersPage } from './VendorOrdersPage'

vi.mock('../../api/client', () => ({
  getSupplierDirectory: vi.fn().mockResolvedValue([
    {
      supplierId: 'vendor-1',
      displayName: 'North Yard Counter',
      supplierKey: 'ACME-NORTH',
      parentSupplierDisplayName: 'Acme Aggregates',
      unitKind: 'sub_unit',
      serviceTypes: ['parts', 'maintenance'],
    },
  ]),
}))

vi.mock('../../api/vendorOrderClient', () => ({
  getSupplierOrderMetadata: vi.fn().mockResolvedValue({
    filterStatusOptions: [
      { value: 'draft', label: 'Draft', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
      { value: 'partially_ready', label: 'Partially ready', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
      { value: 'completed_ready_for_dispatch', label: 'Completed ready for dispatch', owner: 'supplyarr', sourceOfTruth: 'supplyarr.vendor_order.workflow' },
    ],
    internalStatusOptions: [],
    vendorPortalStatusOptions: [],
    documentTypeOptions: [],
    brokerDecisionTypeOptions: [],
  }),
  getSupplierOrders: vi.fn().mockResolvedValue([
    {
      vendorOrderId: 'vendor-order-1',
      supplierId: 'vendor-1',
      supplierNameSnapshot: 'North Yard Counter',
      vendorId: 'vendor-1',
      vendorNameSnapshot: 'Acme Aggregates',
      parentSupplierDisplayName: 'Acme Aggregates',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts', 'maintenance'],
      itemDescription: 'Crushed stone',
      orderedQuantity: 520,
      quantityReady: 200,
      quantityRemaining: 320,
      quantityUom: 'units',
      status: 'partially_ready',
      expectedReadyAt: '2026-06-12T12:00:00Z',
      updatedAt: '2026-06-11T10:00:00Z',
      parentVendorOrderId: null,
    },
  ]),
}))

vi.mock('./useSupplyArrPageAccess', () => ({
  useSupplyArrPageAccess: vi.fn(() => ({
    session: {
      accessToken: 'token',
      tenantDisplayName: 'Acme Tenant',
    },
    meQuery: { isLoading: false },
    canReadSupplierOrders: true,
    canCreateSupplierOrders: true,
    canUpdateSupplierOrders: true,
    canManageSupplierOrderSettings: true,
  })),
}))

describe('VendorOrdersPage', () => {
  it('renders supplier orders with create and detail links', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/purchasing/supplier-orders']}>
          <Routes>
            <Route path="/purchasing/supplier-orders" element={<VendorOrdersPage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Supplier order readiness')).toBeInTheDocument()
    expect(await screen.findByText('Crushed stone')).toBeInTheDocument()
    expect(screen.getByText('Acme Aggregates · North Yard Counter')).toBeInTheDocument()
    expect(screen.getByText('Sub-unit')).toBeInTheDocument()
    expect(screen.getByText('Parts, Maintenance')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Create supplier order' })).toHaveAttribute(
      'href',
      '/purchasing/supplier-orders/create',
    )
    expect(screen.getByRole('link', { name: 'Open detail' })).toHaveAttribute(
      'href',
      '/purchasing/supplier-orders/vendor-order-1',
    )
  })
})
