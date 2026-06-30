import { cleanup, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')
  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      options,
      onChange,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      options: Array<{ value: string; label: string }>
      onChange: (value: string) => void
      placeholder?: string
      testId?: string
    }) => (
      <label>
        {label ? <span>{label}</span> : null}
        <select
          aria-label={label ?? placeholder ?? 'Static search picker'}
          data-testid={testId}
          value={value}
          onChange={(event) => onChange(event.target.value)}
        >
          <option value="">{placeholder ?? 'Select…'}</option>
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </label>
    ),
  }
})

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
      supplierId: 'vendor-1',
      supplierKey: 'acme',
      supplierDisplayName: 'North Yard Counter',
      parentSupplierId: 'supplier-1',
      parentSupplierDisplayName: 'Acme Parts',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts', 'maintenance'],
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
      supplierId: 'vendor-1',
      supplierKey: 'acme',
      supplierDisplayName: 'North Yard Counter',
      parentSupplierId: 'supplier-1',
      parentSupplierDisplayName: 'Acme Parts',
      supplierUnitKind: 'sub_unit',
      supplierServiceTypes: ['parts'],
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
  suppliers: [
    {
      supplierId: 'vendor-1',
      partyId: 'vendor-1',
      displayName: 'North Yard Counter',
      supplierKey: 'acme-north-yard',
      partyKey: 'acme-north-yard',
      parentSupplierDisplayName: 'Acme Parts',
      unitKind: 'sub_unit',
    },
  ],
  canCreate: true,
  canApprove: true,
  isLoading: false,
  requestKey: '',
  title: '',
  notes: '',
  selectedSupplierUnitId: '',
  selectedPartId: '',
  lineQuantity: '',
  lineNotes: '',
  rejectionReason: '',
  selectedPurchaseRequestId: 'pr-1',
  onRequestKeyChange: vi.fn(),
  onTitleChange: vi.fn(),
  onNotesChange: vi.fn(),
  onSelectedSupplierUnitIdChange: vi.fn(),
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
    expect(within(screen.getByTestId('purchase-request-detail')).getByText(/Acme Parts · North Yard Counter/)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Approve' })).toBeInTheDocument()
    expect(screen.getByTestId('purchase-request-line-line-1')).toHaveTextContent('6 each requested')
    expect(screen.getByTestId('purchase-request-create-form')).toBeInTheDocument()
    expect(screen.getByLabelText('Supplier identity or sub-unit (optional)')).toBeInTheDocument()
    expect(screen.getByLabelText('Part for first line')).toBeInTheDocument()
    expect(screen.getByText('Sub-unit · Parts, Maintenance')).toBeInTheDocument()
  })

  it('shows reject controls for submitted purchase requests', () => {
    render(<PurchaseRequestPanel {...baseProps} />)

    const detail = screen.getByTestId('purchase-request-detail')
    expect(within(detail).getByTestId('purchase-request-reject-button')).toBeInTheDocument()
    expect(within(detail).getByLabelText('Rejection reason code')).toBeInTheDocument()
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
