import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

vi.mock('../api/client', () => ({
  getMaintenanceVendorWork: vi.fn(),
  upsertMaintenanceVendorWork: vi.fn(),
}))

vi.mock('@stl/shared-ui', () => ({
  ReferenceProviderClient: vi.fn(),
  ReferencePicker: ({ value }: { value: { displayLabelSnapshot?: string } | null }) => (
    <div data-testid="supplier-reference-picker">{value?.displayLabelSnapshot ?? 'Select supplier'}</div>
  ),
}))

const client = await import('../api/client')
const { WorkOrderVendorWorkPanel } = await import('./WorkOrderVendorWorkPanel')

describe('WorkOrderVendorWorkPanel', () => {
  it('renders vendor work records for a work order', async () => {
    vi.mocked(client.getMaintenanceVendorWork).mockResolvedValue({
      items: [
        {
          vendorWorkId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
          workOrderId: 'wo-1',
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
          createdAt: '2026-06-06T09:00:00Z',
          updatedAt: '2026-06-06T09:30:00Z',
          duplicate: false,
        },
      ],
    })

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <WorkOrderVendorWorkPanel
          workOrder={{
            workOrderId: 'wo-1',
          } as any}
          accessToken="token"
          canPerform
        />
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Vendor coordination')).toBeInTheDocument()
    expect(await screen.findByText('vendor-01')).toBeInTheDocument()
    expect(screen.getByText('Replace drive shaft')).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'scheduled' })).toBeInTheDocument()
  })
})
