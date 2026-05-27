import type { PurchaseOrderResponse, PurchaseRequestResponse } from '../api/types'

interface PurchaseOrderPanelProps {
  purchaseOrders: PurchaseOrderResponse[]
  approvedPurchaseRequests: PurchaseRequestResponse[]
  canCreate: boolean
  canApprove: boolean
  isLoading: boolean
  orderKey: string
  selectedPurchaseRequestId: string
  selectedPurchaseOrderId: string
  onOrderKeyChange: (value: string) => void
  onSelectedPurchaseRequestIdChange: (value: string) => void
  onSelectedPurchaseOrderIdChange: (value: string) => void
  onCreateFromPurchaseRequest: () => void
  onApprove: () => void
  onIssue: () => void
  isCreating: boolean
  isApproving: boolean
  isIssuing: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'issued':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'approved':
      return 'bg-sky-500/20 text-sky-300 ring-sky-500/40'
    case 'draft':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'cancelled':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

export function PurchaseOrderPanel({
  purchaseOrders,
  approvedPurchaseRequests,
  canCreate,
  canApprove,
  isLoading,
  orderKey,
  selectedPurchaseRequestId,
  selectedPurchaseOrderId,
  onOrderKeyChange,
  onSelectedPurchaseRequestIdChange,
  onSelectedPurchaseOrderIdChange,
  onCreateFromPurchaseRequest,
  onApprove,
  onIssue,
  isCreating,
  isApproving,
  isIssuing,
}: PurchaseOrderPanelProps) {
  const selectedPo = purchaseOrders.find((po) => po.purchaseOrderId === selectedPurchaseOrderId)
  const selectedPr = approvedPurchaseRequests.find(
    (pr) => pr.purchaseRequestId === selectedPurchaseRequestId,
  )

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg">
      <h2 className="text-lg font-medium text-white">Purchase orders</h2>
      <p className="mt-1 text-sm text-slate-400">
        Create POs from approved purchase requests, approve, and issue to vendors.
      </p>

      {isLoading ? <p className="mt-4 text-sm text-slate-500">Loading purchase orders…</p> : null}

      <ul className="mt-4 space-y-2">
        {purchaseOrders.map((po) => (
          <li key={po.purchaseOrderId}>
            <button
              type="button"
              className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                selectedPurchaseOrderId === po.purchaseOrderId
                  ? 'border-violet-500/60 bg-violet-500/10'
                  : 'border-slate-800 bg-slate-950/40 hover:border-slate-700'
              }`}
              onClick={() => onSelectedPurchaseOrderIdChange(po.purchaseOrderId)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-slate-200">{po.orderKey}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(po.status)}`}
                >
                  {po.status}
                </span>
              </div>
              <div className="mt-1 text-slate-400">{po.title}</div>
              <div className="mt-1 text-xs text-slate-500">
                PR {po.purchaseRequestKey} · {po.vendorDisplayName} · {po.lines.length} line
                {po.lines.length === 1 ? '' : 's'}
              </div>
            </button>
          </li>
        ))}
        {purchaseOrders.length === 0 && !isLoading ? (
          <li className="text-sm text-slate-500">No purchase orders yet.</li>
        ) : null}
      </ul>

      {selectedPo ? (
        <div className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-sm font-medium text-slate-200">Order detail</h3>
          <ul className="mt-2 space-y-1 text-sm text-slate-400">
            {selectedPo.lines.map((line) => (
              <li key={line.lineId}>
                {line.partKey} — {line.quantityOrdered} {line.unitOfMeasure}
              </li>
            ))}
          </ul>
          <div className="mt-3 flex flex-wrap gap-2">
            {canApprove && selectedPo.status === 'draft' ? (
              <button
                type="button"
                className="rounded-md bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
                onClick={onApprove}
                disabled={isApproving}
              >
                {isApproving ? 'Approving…' : 'Approve PO'}
              </button>
            ) : null}
            {canCreate && selectedPo.status === 'approved' ? (
              <button
                type="button"
                className="rounded-md bg-emerald-600 px-3 py-1.5 text-sm text-white hover:bg-emerald-500 disabled:opacity-50"
                onClick={onIssue}
                disabled={isIssuing}
              >
                {isIssuing ? 'Issuing…' : 'Issue to vendor'}
              </button>
            ) : null}
          </div>
        </div>
      ) : null}

      {canCreate ? (
        <div className="mt-6 border-t border-slate-800 pt-4">
          <h3 className="text-sm font-medium text-slate-200">Create from approved PR</h3>
          <div className="mt-3 space-y-3">
            <label className="block text-xs text-slate-500">
              Approved purchase request
              <select
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
                value={selectedPurchaseRequestId}
                onChange={(e) => onSelectedPurchaseRequestIdChange(e.target.value)}
              >
                <option value="">Select…</option>
                {approvedPurchaseRequests.map((pr) => (
                  <option key={pr.purchaseRequestId} value={pr.purchaseRequestId}>
                    {pr.requestKey} — {pr.title}
                  </option>
                ))}
              </select>
            </label>
            {selectedPr ? (
              <p className="text-xs text-slate-500">
                Vendor: {selectedPr.vendorDisplayName ?? 'none'} · {selectedPr.lines.length} line(s)
              </p>
            ) : null}
            <label className="block text-xs text-slate-500">
              Order key
              <input
                className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-sm text-slate-200"
                value={orderKey}
                onChange={(e) => onOrderKeyChange(e.target.value)}
                placeholder="po-2026-001"
              />
            </label>
            <button
              type="button"
              className="rounded-md bg-violet-600 px-3 py-1.5 text-sm text-white hover:bg-violet-500 disabled:opacity-50"
              onClick={onCreateFromPurchaseRequest}
              disabled={
                isCreating || !selectedPurchaseRequestId || !orderKey.trim() || !selectedPr?.vendorPartyId
              }
            >
              {isCreating ? 'Creating…' : 'Create purchase order'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
