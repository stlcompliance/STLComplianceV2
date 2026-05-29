import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

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
          purchaseOrders={[]}
        />
      </QueryClientProvider>,
    )
    expect(await screen.findByTestId('procurement-exceptions-panel')).toBeInTheDocument()
    expect(screen.getByText('Procurement exceptions')).toBeInTheDocument()

    fireEvent.change(screen.getByTestId('procurement-exception-subject-record'), {
      target: { value: 'pr-1' },
    })
    expect(await screen.findByTestId('procurement-exception-status-ex-1')).toHaveTextContent('open')
    expect(screen.getByTestId('procurement-exception-waive-justification')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-cancel-reason')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-request-waive-ex-2')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-cancel-ex-2')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-approve-waive-ex-3')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-reject-waive-ex-3')).toBeInTheDocument()
    expect(screen.getByTestId('procurement-exception-close-ex-4')).toBeInTheDocument()
  })
})
