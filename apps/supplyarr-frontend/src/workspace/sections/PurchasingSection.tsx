import { PurchaseOrderPanel } from '../../components/PurchaseOrderPanel'
import { ProcurementCoordinationPanel } from '../../components/ProcurementCoordinationPanel'
import { ApprovalRemindersPanel } from '../../components/ApprovalRemindersPanel'
import { DemandProcessingPanel } from '../../components/DemandProcessingPanel'
import { ProcurementExceptionsPanel } from '../../components/ProcurementExceptionsPanel'
import { ProcurementApprovalAuthorityBanner } from '../../components/ProcurementApprovalAuthorityBanner'
import { PurchaseRequestPanel } from '../../components/PurchaseRequestPanel'
import { RfqPanel } from '../../components/RfqPanel'
import { EmergencyPurchasePanel } from '../../components/EmergencyPurchasePanel'
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
      <ProcurementApprovalAuthorityBanner
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <EmergencyPurchasePanel
        accessToken={s.accessToken}
        canCreate={s.canCreateEmergencyPurchase}
        canOverrideApprove={s.canManagerOverrideEmergencyPurchase}
        parts={s.partsQuery.data ?? []}
        vendors={vendors}
      />
      <RfqPanel
        accessToken={s.accessToken}
        canManage={s.canCreatePr}
        canAward={s.canApprovePr}
        parts={s.partsQuery.data ?? []}
        vendors={vendors}
      />
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
        cancellationReason={s.poCancellationReason}
        selectedPurchaseRequestId={s.poSourcePurchaseRequestId}
        selectedPurchaseOrderId={s.selectedPurchaseOrderId}
        onOrderKeyChange={s.setPoOrderKey}
        onCancellationReasonChange={s.setPoCancellationReason}
        onSelectedPurchaseRequestIdChange={s.setPoSourcePurchaseRequestId}
        onSelectedPurchaseOrderIdChange={s.setSelectedPurchaseOrderId}
        onCreateFromPurchaseRequest={() => s.createPurchaseOrderMutation.mutate()}
        onApprove={() => s.approvePurchaseOrderMutation.mutate()}
        onIssue={() => s.issuePurchaseOrderMutation.mutate()}
        onCancel={() => s.cancelPurchaseOrderMutation.mutate()}
        isCreating={s.createPurchaseOrderMutation.isPending}
        isApproving={s.approvePurchaseOrderMutation.isPending}
        isIssuing={s.issuePurchaseOrderMutation.isPending}
        isCancelling={s.cancelPurchaseOrderMutation.isPending}
      />
      <ProcurementCoordinationPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <ApprovalRemindersPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
      />
      <DemandProcessingPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr}
        canOperate={s.canCreatePr}
      />
      <ProcurementExceptionsPanel
        accessToken={s.accessToken}
        currentUserId={s.session?.userId ?? ''}
        canManage={s.canCreatePr}
        canApprove={s.canApprovePr}
        purchaseRequests={s.purchaseRequestsQuery.data ?? []}
        purchaseOrders={s.purchaseOrdersQuery.data ?? []}
      />
    </div>
  )
}
