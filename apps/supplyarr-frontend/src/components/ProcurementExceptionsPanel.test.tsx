import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import { ProcurementExceptionsPanel } from './ProcurementExceptionsPanel'

vi.mock('../api/client', () => ({
  listProcurementExceptions: vi.fn().mockResolvedValue([]),
  listSubjectProcurementExceptions: vi.fn().mockResolvedValue([]),
  getRfqs: vi.fn().mockResolvedValue([]),
  createSubjectProcurementException: vi.fn(),
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
  })
})
