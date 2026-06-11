import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { VendorOrdersPage } from './VendorOrdersPage'

vi.mock('../../api/client', () => ({
  getVendors: vi.fn().mockResolvedValue([
    {
      partyId: 'vendor-1',
      displayName: 'Acme Aggregates',
    },
  ]),
}))

vi.mock('../../api/vendorOrderClient', () => ({
  getVendorOrders: vi.fn().mockResolvedValue([
    {
      vendorOrderId: 'vendor-order-1',
      vendorId: 'vendor-1',
      vendorNameSnapshot: 'Acme Aggregates',
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
    canReadVendorOrders: true,
    canCreateVendorOrders: true,
    canUpdateVendorOrders: true,
    canManageVendorOrderSettings: true,
  })),
}))

describe('VendorOrdersPage', () => {
  it('renders vendor orders with create and detail links', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={client}>
        <MemoryRouter initialEntries={['/purchasing/vendor-orders']}>
          <Routes>
            <Route path="/purchasing/vendor-orders" element={<VendorOrdersPage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Vendor order readiness')).toBeInTheDocument()
    expect(await screen.findByText('Crushed stone')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Create vendor order' })).toHaveAttribute(
      'href',
      '/purchasing/vendor-orders/create',
    )
    expect(screen.getByRole('link', { name: 'Open detail' })).toHaveAttribute(
      'href',
      '/purchasing/vendor-orders/vendor-order-1',
    )
  })
})
