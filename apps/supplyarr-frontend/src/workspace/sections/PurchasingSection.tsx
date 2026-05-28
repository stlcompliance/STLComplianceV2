import { PurchaseOrderPanel } from '../../components/PurchaseOrderPanel'
import { ProcurementCoordinationPanel } from '../../components/ProcurementCoordinationPanel'
import { ApprovalRemindersPanel } from '../../components/ApprovalRemindersPanel'
import { PurchaseRequestPanel } from '../../components/PurchaseRequestPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function PurchasingSection({ state: s }: Props) {
  const vendors = s.vendors.map((v) => ({
    partyId: v.partyId,
    displayName: v.displayName,
    partyKey: v.partyKey,
  }))

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <PurchaseRequestPanel
        purchaseRequests={s.purchaseRequestsQuery.data ?? []}
        parts={s.partsQuery.data ?? []}
        vendors={vendors}
        canCreate={s.canCreatePr}
        canApprove={s.canApprovePr}
        isLoading={s.purchaseRequestsQuery.isLoading}
        requestKey={s.prRequestKey}
        title={s.prTitle}
        notes={s.prNotes}
        selectedVendorId={s.prVendorId}
        selectedPartId={s.prPartId}
        lineQuantity={s.prLineQty}
        lineNotes={s.prLineNotes}
        rejectionReason={s.prRejectionReason}
        selectedPurchaseRequestId={s.selectedPurchaseRequestId}
        onRequestKeyChange={s.setPrRequestKey}
        onTitleChange={s.setPrTitle}
        onNotesChange={s.setPrNotes}
        onSelectedVendorIdChange={s.setPrVendorId}
        onSelectedPartIdChange={s.setPrPartId}
        onLineQuantityChange={s.setPrLineQty}
        onLineNotesChange={s.setPrLineNotes}
        onRejectionReasonChange={s.setPrRejectionReason}
        onSelectedPurchaseRequestIdChange={s.setSelectedPurchaseRequestId}
        onCreate={() => s.createPurchaseRequestMutation.mutate()}
        onSubmit={() => s.submitPurchaseRequestMutation.mutate()}
        onApprove={() => s.approvePurchaseRequestMutation.mutate()}
        onReject={() => s.rejectPurchaseRequestMutation.mutate()}
        isCreating={s.createPurchaseRequestMutation.isPending}
        isSubmitting={s.submitPurchaseRequestMutation.isPending}
        isApproving={s.approvePurchaseRequestMutation.isPending}
        isRejecting={s.rejectPurchaseRequestMutation.isPending}
      />
      <PurchaseOrderPanel
        purchaseOrders={s.purchaseOrdersQuery.data ?? []}
        approvedPurchaseRequests={s.approvedPurchaseRequests}
        canCreate={s.canCreatePo}
        canApprove={s.canApprovePo}
        isLoading={s.purchaseOrdersQuery.isLoading}
        orderKey={s.poOrderKey}
        selectedPurchaseRequestId={s.poSourcePurchaseRequestId}
        selectedPurchaseOrderId={s.selectedPurchaseOrderId}
        onOrderKeyChange={s.setPoOrderKey}
        onSelectedPurchaseRequestIdChange={s.setPoSourcePurchaseRequestId}
        onSelectedPurchaseOrderIdChange={s.setSelectedPurchaseOrderId}
        onCreateFromPurchaseRequest={() => s.createPurchaseOrderMutation.mutate()}
        onApprove={() => s.approvePurchaseOrderMutation.mutate()}
        onIssue={() => s.issuePurchaseOrderMutation.mutate()}
        isCreating={s.createPurchaseOrderMutation.isPending}
        isApproving={s.approvePurchaseOrderMutation.isPending}
        isIssuing={s.issuePurchaseOrderMutation.isPending}
      />
      <ProcurementCoordinationPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <ApprovalRemindersPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
    </div>
  )
}
