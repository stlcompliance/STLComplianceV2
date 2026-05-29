import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { PurchaseRequestPanel } from './PurchaseRequestPanel'

afterEach(() => {
  cleanup()
})

const baseProps = {
  purchaseRequests: [
    {
      purchaseRequestId: 'pr-1',
      requestKey: 'pr-2026-001',
      title: 'Shop restock',
      notes: 'Weekly filters',
      status: 'submitted',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Parts',
      requestedByUserId: 'user-1',
      submittedAt: '2026-01-02T00:00:00Z',
      submittedByUserId: 'user-1',
      approvedAt: null,
      approvedByUserId: null,
      rejectedAt: null,
      rejectedByUserId: null,
      rejectionReason: '',
      isEmergency: false,
      emergencyReason: '',
      emergencyExpeditedAt: null,
      managerOverrideApproved: false,
      managerOverrideJustification: '',
      managerOverrideApprovedAt: null,
      lines: [
        {
          lineId: 'line-1',
          lineNumber: 1,
          partId: 'part-1',
          partKey: 'filter-001',
          partDisplayName: 'Oil Filter',
          quantityRequested: 6,
          unitOfMeasure: 'each',
          notes: '',
          createdAt: '2026-01-01T00:00:00Z',
          updatedAt: '2026-01-01T00:00:00Z',
        },
      ],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-02T00:00:00Z',
    },
    {
      purchaseRequestId: 'pr-2',
      requestKey: 'pr-2026-002',
      title: 'Rejected request',
      notes: '',
      status: 'rejected',
      vendorPartyId: 'vendor-1',
      vendorPartyKey: 'acme',
      vendorDisplayName: 'Acme Parts',
      requestedByUserId: 'user-1',
      submittedAt: '2026-01-01T00:00:00Z',
      submittedByUserId: 'user-1',
      approvedAt: null,
      approvedByUserId: null,
      rejectedAt: '2026-01-03T00:00:00Z',
      rejectedByUserId: 'user-2',
      rejectionReason: 'Budget exceeded',
      isEmergency: false,
      emergencyReason: '',
      emergencyExpeditedAt: null,
      managerOverrideApproved: false,
      managerOverrideJustification: '',
      managerOverrideApprovedAt: null,
      lines: [],
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-03T00:00:00Z',
    },
  ],
  parts: [],
  vendors: [],
  canCreate: true,
  canApprove: true,
  isLoading: false,
  requestKey: '',
  title: '',
  notes: '',
  selectedVendorId: '',
  selectedPartId: '',
  lineQuantity: '',
  lineNotes: '',
  rejectionReason: '',
  selectedPurchaseRequestId: 'pr-1',
  onRequestKeyChange: vi.fn(),
  onTitleChange: vi.fn(),
  onNotesChange: vi.fn(),
  onSelectedVendorIdChange: vi.fn(),
  onSelectedPartIdChange: vi.fn(),
  onLineQuantityChange: vi.fn(),
  onLineNotesChange: vi.fn(),
  onRejectionReasonChange: vi.fn(),
  onSelectedPurchaseRequestIdChange: vi.fn(),
  onCreate: vi.fn(),
  onSubmit: vi.fn(),
  onApprove: vi.fn(),
  onReject: vi.fn(),
  isCreating: false,
  isSubmitting: false,
  isApproving: false,
  isRejecting: false,
}

describe('PurchaseRequestPanel', () => {
  it('renders purchase request list and workflow actions', () => {
    render(<PurchaseRequestPanel {...baseProps} />)

    expect(screen.getByTestId('supplyarr-purchasing-pr-workspace')).toBeInTheDocument()
    expect(screen.getByText('pr-2026-001')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Approve' })).toBeInTheDocument()
    expect(screen.getByTestId('purchase-request-line-line-1')).toHaveTextContent('6 each requested')
    expect(screen.getByTestId('purchase-request-create-form')).toBeInTheDocument()
  })

  it('shows reject controls for submitted purchase requests', () => {
    render(<PurchaseRequestPanel {...baseProps} />)

    const detail = screen.getByTestId('purchase-request-detail')
    expect(within(detail).getByTestId('purchase-request-reject-button')).toBeInTheDocument()
    expect(within(detail).getByTestId('purchase-request-rejection-reason-input')).toBeInTheDocument()
    expect(within(detail).getByTestId('purchase-request-workflow-timeline')).toBeInTheDocument()
  })

  it('shows rejection reason for rejected purchase requests', () => {
    render(
      <PurchaseRequestPanel
        {...baseProps}
        purchaseRequests={[baseProps.purchaseRequests[1]!]}
        selectedPurchaseRequestId="pr-2"
        canApprove={false}
        canCreate={false}
      />,
    )

    expect(screen.getByTestId('purchase-request-rejection-reason-display')).toHaveTextContent(
      'Budget exceeded',
    )
    expect(screen.queryByTestId('purchase-request-approve-button')).not.toBeInTheDocument()
  })
})
