import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, within } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

vi.mock('../api/client', () => ({
  getMaintenanceVendorWork: vi.fn(),
  issueMaintenanceVendorWorkPortalAccess: vi.fn(),
  revokeMaintenanceVendorWorkPortalAccess: vi.fn(),
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
          portalAccessCode: 'portal-abc',
          portalAccessCodeIssuedAt: '2026-06-06T09:15:00Z',
          portalAccessExpiresAt: '2026-06-20T09:15:00Z',
          portalAccessOpenedAt: '2026-06-06T09:30:00Z',
          portalAccessRevokedAt: null,
          portalAccessStatus: 'opened',
          portalAccessUrl: '/vendor-portal/work-orders/wo-1?accessCode=portal-abc',
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
    expect(screen.getByText('Portal access')).toBeInTheDocument()
    expect(screen.getByDisplayValue('portal-abc')).toBeInTheDocument()
    expect(
      screen.getByDisplayValue(/\/vendor-portal\/work-orders\/wo-1\?accessCode=portal-abc$/),
    ).toBeInTheDocument()

    const summary = screen.getByTestId('vendor-work-summary')
    expect(summary).toHaveTextContent('Vendor work scheduled')
    expect(summary).toHaveTextContent('Track vendor updates and evidence until the work is complete.')
    expect(within(summary).getByText('Scheduled')).toBeInTheDocument()

    const vendorCard = screen.getByText('Replace drive shaft').closest('button')
    expect(vendorCard).not.toBeNull()
    expect(within(vendorCard as HTMLElement).getByText('quote-1')).toBeInTheDocument()
    expect(within(vendorCard as HTMLElement).getByText('approval-1')).toBeInTheDocument()
  })
})
