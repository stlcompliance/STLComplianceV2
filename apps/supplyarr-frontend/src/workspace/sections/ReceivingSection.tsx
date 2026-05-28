import { BackordersPanel } from '../../components/BackordersPanel'
import { ReceivingPanel } from '../../components/ReceivingPanel'
import { ReturnsPanel } from '../../components/ReturnsPanel'
import { WarrantyClaimsPanel } from '../../components/WarrantyClaimsPanel'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }

export function ReceivingSection({ state: s }: Props) {
  return (
    <div className="grid gap-6 lg:grid-cols-2">
      <ReceivingPanel
        receivingReceipts={s.receivingReceiptsQuery.data ?? []}
        issuedPurchaseOrders={s.issuedPurchaseOrders}
        bins={s.binsQuery.data ?? []}
        canPerform={s.canReceive}
        isLoading={s.receivingReceiptsQuery.isLoading}
        receiptKey={s.receiptKey}
        selectedPurchaseOrderId={s.receiveSourcePurchaseOrderId}
        selectedReceivingReceiptId={s.selectedReceivingReceiptId}
        selectedBinId={s.receiveBinId}
        selectedLineId={s.selectedReceiveLineId}
        lineQuantityReceived={s.lineQuantityReceived}
        exceptionType={s.exceptionType}
        exceptionQuantity={s.exceptionQuantity}
        exceptionNotes={s.exceptionNotes}
        onReceiptKeyChange={s.setReceiptKey}
        onSelectedPurchaseOrderIdChange={s.setReceiveSourcePurchaseOrderId}
        onSelectedReceivingReceiptIdChange={s.setSelectedReceivingReceiptId}
        onSelectedBinIdChange={s.setReceiveBinId}
        onSelectedLineIdChange={s.setSelectedReceiveLineId}
        onLineQuantityReceivedChange={s.setLineQuantityReceived}
        onExceptionTypeChange={s.setExceptionType}
        onExceptionQuantityChange={s.setExceptionQuantity}
        onExceptionNotesChange={s.setExceptionNotes}
        onCreateFromPurchaseOrder={() => s.createReceivingReceiptMutation.mutate()}
        onUpdateLineQuantity={() => s.updateReceivingLineMutation.mutate()}
        onCreateException={() => s.createReceivingExceptionMutation.mutate()}
        onResolveException={(id) => s.resolveReceivingExceptionMutation.mutate(id)}
        onPost={() => s.postReceivingReceiptMutation.mutate()}
        isCreating={s.createReceivingReceiptMutation.isPending}
        isUpdatingLine={s.updateReceivingLineMutation.isPending}
        isCreatingException={s.createReceivingExceptionMutation.isPending}
        isPosting={s.postReceivingReceiptMutation.isPending}
      />
      <BackordersPanel
        backorders={s.backordersQuery.data ?? []}
        issuedPurchaseOrders={s.issuedPurchaseOrders}
        canManage={s.canReceive}
        isLoading={s.backordersQuery.isLoading}
        backorderKey={s.backorderKey}
        selectedBackorderId={s.selectedBackorderId}
        selectedPurchaseOrderLineId={s.selectedBackorderPoLineId}
        backorderQuantity={s.backorderQuantity}
        backorderNotes={s.backorderNotes}
        cancelReason={s.backorderCancelReason}
        statusFilter={s.backorderStatusFilter}
        onBackorderKeyChange={s.setBackorderKey}
        onSelectedBackorderIdChange={s.setSelectedBackorderId}
        onSelectedPurchaseOrderLineIdChange={s.setSelectedBackorderPoLineId}
        onBackorderQuantityChange={s.setBackorderQuantity}
        onBackorderNotesChange={s.setBackorderNotes}
        onCancelReasonChange={s.setBackorderCancelReason}
        onStatusFilterChange={s.setBackorderStatusFilter}
        onCreateFromPurchaseOrderLine={() => s.createBackorderMutation.mutate()}
        onFulfill={() => s.fulfillBackorderMutation.mutate()}
        onCancel={() => s.cancelBackorderMutation.mutate()}
        isCreating={s.createBackorderMutation.isPending}
        isFulfilling={s.fulfillBackorderMutation.isPending}
        isCancelling={s.cancelBackorderMutation.isPending}
      />
      <ReturnsPanel
        returns={s.vendorReturnsQuery.data ?? []}
        vendors={s.vendors}
        parts={s.partsQuery.data ?? []}
        issuedPurchaseOrders={s.issuedPurchaseOrdersWithReceived}
        inventoryBins={s.returnInventoryBins}
        canManage={s.canReceive}
        isLoading={s.vendorReturnsQuery.isLoading}
        returnKey={s.returnKey}
        selectedReturnId={s.selectedReturnId}
        selectedVendorPartyId={s.selectedReturnVendorId}
        selectedInventoryBinId={s.selectedReturnBinId}
        selectedReturnPoLineId={s.selectedReturnPoLineId}
        selectedReturnPartId={s.selectedReturnPartId}
        returnQuantity={s.returnQuantity}
        rmaNumber={s.rmaNumber}
        returnNotes={s.returnNotes}
        cancelReason={s.returnCancelReason}
        statusFilter={s.returnStatusFilter}
        returnSource={s.returnSource}
        onReturnKeyChange={s.setReturnKey}
        onSelectedReturnIdChange={s.setSelectedReturnId}
        onSelectedVendorPartyIdChange={s.setSelectedReturnVendorId}
        onSelectedInventoryBinIdChange={s.setSelectedReturnBinId}
        onSelectedReturnPoLineIdChange={s.setSelectedReturnPoLineId}
        onSelectedReturnPartIdChange={s.setSelectedReturnPartId}
        onReturnQuantityChange={s.setReturnQuantity}
        onRmaNumberChange={s.setRmaNumber}
        onReturnNotesChange={s.setReturnNotes}
        onCancelReasonChange={s.setReturnCancelReason}
        onStatusFilterChange={s.setReturnStatusFilter}
        onReturnSourceChange={s.setReturnSource}
        onCreate={() => s.createReturnMutation.mutate()}
        onPost={() => s.postReturnMutation.mutate()}
        onCancel={() => s.cancelReturnMutation.mutate()}
        isCreating={s.createReturnMutation.isPending}
        isPosting={s.postReturnMutation.isPending}
        isCancelling={s.cancelReturnMutation.isPending}
      />
      <WarrantyClaimsPanel
        accessToken={s.accessToken}
        canManage={s.canReceive}
        vendors={s.vendors}
        parts={s.partsQuery.data ?? []}
        issuedPurchaseOrders={s.issuedPurchaseOrdersWithReceived}
      />
    </div>
  )
}
