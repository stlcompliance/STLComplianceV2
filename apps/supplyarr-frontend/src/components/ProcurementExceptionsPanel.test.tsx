import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

vi.mock('@stl/shared-ui', async () => {
  const actual = await vi.importActual<typeof import('@stl/shared-ui')>('@stl/shared-ui')

  return {
    ...actual,
    StaticSearchPicker: ({
      label,
      value,
      onChange,
      options,
      placeholder,
      testId,
    }: {
      label?: string
      value: string
      onChange: (value: string) => void
      options: Array<{ value: string; label: string }>
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

import { ProcurementExceptionsPanel } from './ProcurementExceptionsPanel'

vi.mock('../api/client', () => ({
  listProcurementExceptionResolutionTemplates: vi.fn().mockResolvedValue([
    {
      templateKey: 'pr_resubmit',
      label: 'PR resubmit',
      defaultResolutionNotes: 'Correct and resubmit.',
    },
  ]),
  listProcurementExceptions: vi.fn().mockResolvedValue([]),
  listSubjectProcurementExceptions: vi.fn().mockResolvedValue([
    {
      exceptionId: 'ex-1',
      exceptionKey: 'PEX-001',
      status: 'open',
      exceptionCategory: 'policy_violation',
      title: 'Test exception',
      slaDueAt: null,
      isSlaBreached: false,
      assignedToUserId: null,
      linkedPurchaseRequestId: null,
      linkedPurchaseOrderId: null,
      linkedPurchaseRequestKey: null,
      linkedPurchaseOrderKey: null,
    },
    {
      exceptionId: 'ex-2',
      exceptionKey: 'PEX-002',
      status: 'investigating',
      exceptionCategory: 'policy_violation',
      title: 'Investigating exception',
      slaDueAt: null,
      isSlaBreached: false,
      assignedToUserId: 'user-1',
      linkedPurchaseRequestId: null,
      linkedPurchaseOrderId: null,
      linkedPurchaseRequestKey: null,
      linkedPurchaseOrderKey: null,
    },
    {
      exceptionId: 'ex-3',
      exceptionKey: 'PEX-003',
      status: 'waive_pending',
      exceptionCategory: 'policy_violation',
      title: 'Waive pending exception',
      slaDueAt: null,
      isSlaBreached: false,
      assignedToUserId: 'user-1',
      linkedPurchaseRequestId: null,
      linkedPurchaseOrderId: null,
      linkedPurchaseRequestKey: null,
      linkedPurchaseOrderKey: null,
    },
    {
      exceptionId: 'ex-5',
      exceptionKey: 'PEX-005',
      status: 'cancelled',
      exceptionCategory: 'policy_violation',
      title: 'Cancelled exception',
      slaDueAt: null,
      isSlaBreached: false,
      assignedToUserId: null,
      linkedPurchaseRequestId: null,
      linkedPurchaseOrderId: null,
      linkedPurchaseRequestKey: null,
      linkedPurchaseOrderKey: null,
    },
    {
      exceptionId: 'ex-4',
      exceptionKey: 'PEX-004',
      status: 'waived',
      exceptionCategory: 'policy_violation',
      title: 'Waived exception',
      slaDueAt: null,
      isSlaBreached: false,
      assignedToUserId: 'user-1',
      linkedPurchaseRequestId: null,
      linkedPurchaseOrderId: null,
      linkedPurchaseRequestKey: null,
      linkedPurchaseOrderKey: null,
    },
  ]),
  getRfqs: vi.fn().mockResolvedValue([]),
  createSubjectProcurementException: vi.fn(),
  assignProcurementException: vi.fn(),
  linkProcurementExceptionActions: vi.fn(),
  startProcurementExceptionInvestigation: vi.fn(),
  resolveProcurementException: vi.fn(),
  requestProcurementExceptionWaive: vi.fn(),
  approveProcurementExceptionWaive: vi.fn(),
  rejectProcurementExceptionWaive: vi.fn(),
  closeProcurementException: vi.fn(),
  cancelProcurementException: vi.fn(),
  reopenProcurementException: vi.fn(),
}))

describe('ProcurementExceptionsPanel', () => {
  it('renders when user can manage', async () => {
    const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={client}>
        <ProcurementExceptionsPanel
          accessToken="token"
          currentUserId="user-1"
          canManage={true}
          canApprove={true}
          purchaseRequests={[
            {
              purchaseRequestId: 'pr-1',
              requestKey: 'PR-1',
              title: 'Test PR',
              notes: '',
              status: 'draft',
              supplierId: null,
              supplierKey: null,
              supplierDisplayName: null,
              parentSupplierId: null,
              parentSupplierDisplayName: null,
              supplierUnitKind: null,
              supplierServiceTypes: [],
              vendorPartyId: null,
              vendorPartyKey: null,
              vendorDisplayName: null,
              requestedByUserId: 'u1',
              submittedAt: null,
              submittedByUserId: null,
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
              lines: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
          purchaseOrders={[
            {
              purchaseOrderId: 'po-1',
              orderKey: 'PO-1',
              title: 'Test PO',
              notes: '',
              status: 'issued',
              purchaseRequestId: 'pr-1',
              purchaseRequestKey: 'PR-1',
              supplierId: 'vendor-1',
              supplierKey: 'vendor-1',
              supplierDisplayName: 'Acme Supply',
              parentSupplierId: null,
              parentSupplierDisplayName: null,
              supplierUnitKind: 'identity',
              supplierServiceTypes: ['parts'],
              vendorPartyId: 'vendor-1',
              vendorPartyKey: 'vendor-1',
              vendorDisplayName: 'Acme Supply',
              createdByUserId: 'u1',
              approvedAt: null,
              approvedByUserId: null,
              issuedAt: null,
              issuedByUserId: null,
              cancelledAt: null,
              cancelledByUserId: null,
              cancellationReason: '',
              lines: [],
              createdAt: '',
              updatedAt: '',
            },
          ]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('procurement-exceptions-panel')).toBeInTheDocument()
    expect(screen.getByText('Procurement exceptions')).toBeInTheDocument()

    fireEvent.change(screen.getByTestId('procurement-exception-subject-record'), {
      target: { value: 'pr-1' },
    })
    expect(screen.getByTestId('procurement-exception-subject-record')).toHaveTextContent('PR-1 — Test PR')
    expect(screen.getByTestId('procurement-exception-waive-justification')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-cancel-reason')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-reopen-reason')).toBeInTheDocument()
  })
})
