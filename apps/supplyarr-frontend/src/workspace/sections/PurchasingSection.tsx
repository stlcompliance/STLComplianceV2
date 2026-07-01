import { PurchaseOrderPanel } from '../../components/PurchaseOrderPanel'
import { ProcurementCoordinationPanel } from '../../components/ProcurementCoordinationPanel'
import { ApprovalRemindersPanel } from '../../components/ApprovalRemindersPanel'
import { DemandProcessingPanel } from '../../components/DemandProcessingPanel'
import { ProcurementExceptionsPanel } from '../../components/ProcurementExceptionsPanel'
import { ProcurementApprovalAuthorityBanner } from '../../components/ProcurementApprovalAuthorityBanner'
import { PurchaseRequestPanel } from '../../components/PurchaseRequestPanel'
import { RfqPanel } from '../../components/RfqPanel'
import { EmergencyPurchasePanel } from '../../components/EmergencyPurchasePanel'
import { ContractsImportPanel } from '../../components/ContractsImportPanel'
import { SupplierEmailInboxPanel } from '../../components/SupplierEmailInboxPanel'
import { Link, useLocation } from 'react-router-dom'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'
import type { SupplierUnitPickerSource } from '../../forms/controlledFormHelpers'

type Props = { state: SupplyArrWorkspaceState }
type PurchasingViewMode = 'procurement' | 'approvals' | 'exceptions'

export function PurchasingSection({ state: s }: Props) {
  const location = useLocation()
  const mode: PurchasingViewMode = location.pathname.startsWith('/purchasing/exceptions')
    ? 'exceptions'
    : location.pathname.startsWith('/purchasing/approvals')
      ? 'approvals'
      : 'procurement'
  const suppliers: SupplierUnitPickerSource[] = s.supplierDirectory.map((supplier) => ({
    supplierId: supplier.supplierId,
    displayName: supplier.displayName,
    supplierKey: supplier.supplierKey,
    parentSupplierDisplayName: supplier.parentSupplierDisplayName,
    unitKind: supplier.unitKind,
  }))

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 className="text-lg font-semibold text-white">Supplier order readiness</h2>
            <p className="mt-1 text-sm text-slate-400">
              Manage supplier confirmations, location-aware sourcing decisions, magic links, and dispatch readiness.
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Link
              to="/purchasing/supplier-orders"
              className="rounded-lg border border-slate-600 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800"
            >
              Open supplier order portal
            </Link>
            {s.canCreatePr ? (
              <Link
                to="/purchasing/supplier-orders/create"
                className="rounded-lg bg-sky-600 px-3 py-2 text-sm font-medium text-white hover:bg-sky-500"
              >
                Create supplier order request
              </Link>
            ) : null}
          </div>
        </div>
      </section>
      {mode !== 'exceptions' ? (
        <ProcurementApprovalAuthorityBanner
          accessToken={s.accessToken}
          canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
        />
      ) : null}
      <EmergencyPurchasePanel
        accessToken={s.accessToken}
        canCreate={s.canCreateEmergencyPurchase}
        canOverrideApprove={s.canManagerOverrideEmergencyPurchase}
        parts={s.partsQuery.data ?? []}
        suppliers={suppliers}
      />
      <RfqPanel
        accessToken={s.accessToken}
        canManage={s.canCreatePr}
        canAward={s.canApprovePr}
        parts={s.partsQuery.data ?? []}
        suppliers={suppliers}
        supplierDirectory={s.supplierDirectory}
      />
      <SupplierEmailInboxPanel accessToken={s.accessToken} canManage={s.canCreatePr || s.canApprovePr || s.canCreatePo} />
      <PurchaseRequestPanel
        purchaseRequests={s.purchaseRequestsQuery.data ?? []}
        parts={s.partsQuery.data ?? []}
        suppliers={suppliers}
        canCreate={s.canCreatePr}
        canApprove={s.canApprovePr}
        isLoading={s.purchaseRequestsQuery.isLoading}
        requestKey={s.prRequestKey}
        title={s.prTitle}
        notes={s.prNotes}
        selectedSupplierUnitId={s.prSupplierUnitId}
        selectedPartId={s.prPartId}
        lineQuantity={s.prLineQty}
        lineNotes={s.prLineNotes}
        rejectionReason={s.prRejectionReason}
        selectedPurchaseRequestId={s.selectedPurchaseRequestId}
        onRequestKeyChange={s.setPrRequestKey}
        onTitleChange={s.setPrTitle}
        onNotesChange={s.setPrNotes}
        onSelectedSupplierUnitIdChange={s.setPrSupplierUnitId}
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
      {mode !== 'procurement' ? (
        <ProcurementCoordinationPanel
          accessToken={s.accessToken}
          canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
        />
      ) : null}
      {mode === 'approvals' ? (
        <ApprovalRemindersPanel
          accessToken={s.accessToken}
          canRead={s.canCreatePr || s.canApprovePr || s.canCreatePo}
        />
      ) : null}
      <DemandProcessingPanel
        accessToken={s.accessToken}
        canRead={s.canCreatePr || s.canApprovePr}
        canOperate={s.canCreatePr}
      />
      {mode !== 'approvals' ? (
        <ProcurementExceptionsPanel
          accessToken={s.accessToken}
          currentUserId={s.session?.userId ?? ''}
          canManage={s.canCreatePr}
          canApprove={s.canApprovePr}
          purchaseRequests={s.purchaseRequestsQuery.data ?? []}
          purchaseOrders={s.purchaseOrdersQuery.data ?? []}
        />
      ) : null}
      <ContractsImportPanel accessToken={s.accessToken} canManage={s.canCreatePr} />
    </div>
  )
}
