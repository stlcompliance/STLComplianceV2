import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'

import { PurchasingSection } from './PurchasingSection'

vi.mock('../../components/PurchaseOrderPanel', () => ({
  PurchaseOrderPanel: () => <div data-testid="supplyarr-purchasing-po-workspace" />,
}))

vi.mock('../../components/PurchaseRequestPanel', () => ({
  PurchaseRequestPanel: () => <div data-testid="supplyarr-purchasing-pr-workspace" />,
}))

vi.mock('../../components/ProcurementApprovalAuthorityBanner', () => ({
  ProcurementApprovalAuthorityBanner: () => null,
}))

vi.mock('../../components/EmergencyPurchasePanel', () => ({
  EmergencyPurchasePanel: () => null,
}))

vi.mock('../../components/RfqPanel', () => ({
  RfqPanel: () => null,
}))

vi.mock('../../components/SupplierEmailInboxPanel', () => ({
  SupplierEmailInboxPanel: () => <div data-testid="supplyarr-supplier-email-inbox-panel" />,
}))

vi.mock('../../components/ProcurementCoordinationPanel', () => ({
  ProcurementCoordinationPanel: () => null,
}))

vi.mock('../../components/ApprovalRemindersPanel', () => ({
  ApprovalRemindersPanel: () => null,
}))

vi.mock('../../components/DemandProcessingPanel', () => ({
  DemandProcessingPanel: () => null,
}))

vi.mock('../../components/ProcurementExceptionsPanel', () => ({
  ProcurementExceptionsPanel: () => null,
}))

vi.mock('../../components/ContractsImportPanel', () => ({
  ContractsImportPanel: () => <div data-testid="supplyarr-contract-import-panel" />,
}))

const baseState = {
  accessToken: 'token',
  supplierDirectory: [],
  partsQuery: { data: [], isLoading: false },
  purchaseRequestsQuery: { data: [], isLoading: false },
  purchaseOrdersQuery: { data: [], isLoading: false },
  approvedPurchaseRequests: [],
  canCreatePr: true,
  canApprovePr: true,
  canCreatePo: true,
  canApprovePo: true,
  canCreateEmergencyPurchase: false,
  canManagerOverrideEmergencyPurchase: false,
  poOrderKey: '',
  poCancellationReason: '',
  poSourcePurchaseRequestId: '',
  selectedPurchaseOrderId: '',
  prRequestKey: '',
  prTitle: '',
  prNotes: '',
  prSupplierUnitId: '',
  prPartId: '',
  prLineQty: '',
  prLineNotes: '',
  prRejectionReason: '',
  selectedPurchaseRequestId: '',
  setPoOrderKey: () => {},
  setPoCancellationReason: () => {},
  setPoSourcePurchaseRequestId: () => {},
  setSelectedPurchaseOrderId: () => {},
  setPrRequestKey: () => {},
  setPrTitle: () => {},
  setPrNotes: () => {},
  setPrSupplierUnitId: () => {},
  setPrPartId: () => {},
  setPrLineQty: () => {},
  setPrLineNotes: () => {},
  setPrRejectionReason: () => {},
  setSelectedPurchaseRequestId: () => {},
  createPurchaseRequestMutation: { mutate: () => {}, isPending: false },
  submitPurchaseRequestMutation: { mutate: () => {}, isPending: false },
  approvePurchaseRequestMutation: { mutate: () => {}, isPending: false },
  rejectPurchaseRequestMutation: { mutate: () => {}, isPending: false },
  createPurchaseOrderMutation: { mutate: () => {}, isPending: false },
  approvePurchaseOrderMutation: { mutate: () => {}, isPending: false },
  issuePurchaseOrderMutation: { mutate: () => {}, isPending: false },
  cancelPurchaseOrderMutation: { mutate: () => {}, isPending: false },
  session: { userId: 'user-1' },
} as never

describe('PurchasingSection', () => {
  it('renders purchase order workflow panel on purchasing section', () => {
    render(
      <MemoryRouter initialEntries={['/purchasing/procurement']}>
        <PurchasingSection state={baseState} />
      </MemoryRouter>,
    )
    expect(screen.getByTestId('supplyarr-purchasing-po-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('supplyarr-purchasing-pr-workspace')).toBeInTheDocument()
    expect(screen.getByTestId('supplyarr-supplier-email-inbox-panel')).toBeInTheDocument()
    expect(screen.getByTestId('supplyarr-contract-import-panel')).toBeInTheDocument()
  })
})
