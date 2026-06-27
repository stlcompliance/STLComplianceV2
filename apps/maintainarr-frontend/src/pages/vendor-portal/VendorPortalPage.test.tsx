import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen, waitFor, within } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

vi.mock('../../api/client', () => ({
  getMaintenanceVendorWorkPortal: vi.fn(),
  updateMaintenanceVendorWorkPortal: vi.fn(),
}))

const client = await import('../../api/client')
const { VendorPortalPage } = await import('./VendorPortalPage')

describe('VendorPortalPage', () => {
  it('renders portal details and submits vendor status updates', async () => {
    vi.mocked(client.getMaintenanceVendorWorkPortal).mockResolvedValue({
      vendorWorkId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      workOrderId: 'wo-1',
      workOrderNumber: 'WO-100',
      workOrderTitle: 'Replace drive shaft',
      workOrderPriority: 'high',
      workOrderStatus: 'open',
      assetId: 'asset-1',
      assetTag: 'TRK-100',
      assetName: 'Truck 100',
      supplierRef: 'vendor-01',
      vendorContactSnapshot: 'Vendor Ops',
      status: 'scheduled',
      workDescription: 'Replace drive shaft',
      quoteRecordRef: 'quote-1',
      approvalRef: 'approval-1',
      scheduledAt: '2026-06-06T10:00:00Z',
      completedAt: null,
      costEstimateSnapshot: '$1,200',
      invoiceRecordRef: null,
      warrantyFlag: true,
      notes: 'Bring crane',
      portalAccessExpiresAt: '2026-06-20T09:15:00Z',
      portalAccessStatus: 'opened',
      allowedActions: ['view_limited_status', 'submit_status_update', 'confirm_completion'],
      createdAt: '2026-06-06T09:00:00Z',
      updatedAt: '2026-06-06T09:30:00Z',
    })

    vi.mocked(client.updateMaintenanceVendorWorkPortal).mockResolvedValue({
      vendorWorkId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      workOrderId: 'wo-1',
      workOrderNumber: 'WO-100',
      workOrderTitle: 'Replace drive shaft',
      workOrderPriority: 'high',
      workOrderStatus: 'open',
      assetId: 'asset-1',
      assetTag: 'TRK-100',
      assetName: 'Truck 100',
      supplierRef: 'vendor-01',
      vendorContactSnapshot: 'Vendor Ops',
      status: 'in_progress',
      workDescription: 'Replace drive shaft',
      quoteRecordRef: 'quote-1',
      approvalRef: 'approval-1',
      scheduledAt: '2026-06-06T10:00:00Z',
      completedAt: null,
      costEstimateSnapshot: '$1,200',
      invoiceRecordRef: null,
      warrantyFlag: true,
      notes: 'Started work',
      portalAccessExpiresAt: '2026-06-20T09:15:00Z',
      portalAccessStatus: 'used',
      allowedActions: ['view_limited_status'],
      createdAt: '2026-06-06T09:00:00Z',
      updatedAt: '2026-06-06T10:00:00Z',
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/vendor-portal/work-orders/wo-1?accessCode=portal-abc']}>
          <Routes>
            <Route path="/vendor-portal/work-orders/:workOrderId" element={<VendorPortalPage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('WO-100')).toBeInTheDocument()
    const workOrderCard = screen.getByText('Work order').closest('div')
    expect(within(workOrderCard as HTMLElement).getByText('Replace drive shaft')).toBeInTheDocument()
    expect(screen.getByText(/Portal expires/)).toBeInTheDocument()
    expect(screen.getByText('view_limited_status, submit_status_update, confirm_completion')).toBeInTheDocument()

    fireEvent.change(screen.getByLabelText('Status'), { target: { value: 'in_progress' } })
    fireEvent.change(screen.getByLabelText('Notes'), { target: { value: 'Started work' } })
    fireEvent.click(screen.getByRole('button', { name: 'Save update' }))

    await waitFor(() =>
      expect(client.updateMaintenanceVendorWorkPortal).toHaveBeenCalledWith('wo-1', 'portal-abc', {
        status: 'in_progress',
        scheduledAt: '2026-06-06T10:00:00.000Z',
        completedAt: null,
        notes: 'Started work',
      }),
    )
  })
})
