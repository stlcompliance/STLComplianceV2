import type { BackorderResponse, PurchaseOrderResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface BackordersPanelProps {
  backorders: BackorderResponse[]
  issuedPurchaseOrders: PurchaseOrderResponse[]
  canManage: boolean
  isLoading: boolean
  backorderKey: string
  selectedBackorderId: string
  selectedPurchaseOrderLineId: string
  backorderQuantity: string
  backorderNotes: string
  cancelReason: string
  statusFilter: string
  onBackorderKeyChange: (value: string) => void
  onSelectedBackorderIdChange: (value: string) => void
  onSelectedPurchaseOrderLineIdChange: (value: string) => void
  onBackorderQuantityChange: (value: string) => void
  onBackorderNotesChange: (value: string) => void
  onCancelReasonChange: (value: string) => void
  onStatusFilterChange: (value: string) => void
  onCreateFromPurchaseOrderLine: () => void
  onFulfill: () => void
  onCancel: () => void
  isCreating: boolean
  isFulfilling: boolean
  isCancelling: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'fulfilled':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'open':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'cancelled':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

function sourceLabel(sourceType: string): string {
  switch (sourceType) {
    case 'receipt_post':
      return 'Receipt post'
    case 'purchase_order_line':
      return 'PO line'
    default:
      return sourceType
  }
}

export function BackordersPanel({
  backorders,
  issuedPurchaseOrders,
  canManage,
  isLoading,
  backorderKey,
  selectedBackorderId,
  selectedPurchaseOrderLineId,
  backorderQuantity,
  backorderNotes,
  cancelReason,
  statusFilter,
  onBackorderKeyChange,
  onSelectedBackorderIdChange,
  onSelectedPurchaseOrderLineIdChange,
  onBackorderQuantityChange,
  onBackorderNotesChange,
  onCancelReasonChange,
  onStatusFilterChange,
  onCreateFromPurchaseOrderLine,
  onFulfill,
  onCancel,
  isCreating,
  isFulfilling,
  isCancelling,
}: BackordersPanelProps) {
  const selected = backorders.find((bo) => bo.backorderId === selectedBackorderId)
  const poLines = issuedPurchaseOrders.flatMap((po) =>
    po.lines
      .filter((line) => line.quantityRemaining > 0)
      .map((line) => ({
        purchaseOrderLineId: line.lineId,
        label: `${po.orderKey} · line ${line.lineNumber} · ${line.partKey} (${line.quantityRemaining} remaining)`,
      })),
  )
  const selectedPoLineLabel =
    poLines.find((line) => line.purchaseOrderLineId === selectedPurchaseOrderLineId)?.label ?? ''
  const backorderKeySource = selectedPoLineLabel ? `${selectedPoLineLabel} backorder` : ''
  const existingBackorderKeys = backorders.map((backorder) => backorder.backorderKey)

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg lg:col-span-2">
      <h2 className="text-lg font-medium text-white">Backorders</h2>
      <p className="mt-1 text-sm text-slate-400">
        Track short shipments and open PO quantities linked to purchase requests and orders.
      </p>

      <div className="mt-4 flex flex-wrap items-end gap-3">
        <label htmlFor="backorder-status-filter" className="block text-sm text-slate-400">
          Backorder status filter
          <select
            id="backorder-status-filter"
            className="mt-1 block w-full min-w-[8rem] rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
            value={statusFilter}
            onChange={(e) => onStatusFilterChange(e.target.value)}
          >
            <option value="">All</option>
            <option value="open">Open</option>
            <option value="fulfilled">Fulfilled</option>
            <option value="cancelled">Cancelled</option>
          </select>
        </label>
      </div>

      {isLoading ? <p className="mt-4 text-sm text-slate-500">Loading backorders…</p> : null}

      <ul className="mt-4 space-y-2">
        {backorders.map((bo) => (
          <li key={bo.backorderId}>
            <button
              type="button"
              className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                selectedBackorderId === bo.backorderId
                  ? 'border-violet-500/60 bg-violet-500/10'
                  : 'border-slate-800 bg-slate-950/40 hover:border-slate-700'
              }`}
              onClick={() => onSelectedBackorderIdChange(bo.backorderId)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-slate-200">{bo.backorderKey}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(bo.status)}`}
                >
                  {bo.status}
                </span>
              </div>
              <div className="mt-1 text-slate-400">
                {bo.partKey} · {bo.quantityOpen} open of {bo.quantityBackordered}
              </div>
              <div className="mt-1 text-xs text-slate-500">
                PO {bo.purchaseOrderKey}
                {bo.purchaseRequestKey ? ` · PR ${bo.purchaseRequestKey}` : ''} ·{' '}
                {sourceLabel(bo.sourceType)}
              </div>
            </button>
          </li>
        ))}
        {backorders.length === 0 && !isLoading ? (
          <li className="text-sm text-slate-500">No backorders match this filter.</li>
        ) : null}
      </ul>

      {canManage ? (
        <div className="mt-6 grid gap-4 border-t border-slate-800 pt-4 md:grid-cols-2">
          <div className="space-y-3">
            <h3 className="text-sm font-medium text-slate-300">Record from PO line</h3>
            <label htmlFor="backorder-po-line" className="block text-sm text-slate-400">
              Backorder PO line
              <select
                id="backorder-po-line"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={selectedPurchaseOrderLineId}
                onChange={(e) => onSelectedPurchaseOrderLineIdChange(e.target.value)}
              >
                <option value="">Select line…</option>
                {poLines.map((line) => (
                  <option key={line.purchaseOrderLineId} value={line.purchaseOrderLineId}>
                    {line.label}
                  </option>
                ))}
              </select>
            </label>
            <GeneratedKeyFieldGroup
              sourceLabel={backorderKeySource}
              existingKeys={existingBackorderKeys}
              onKeyChange={onBackorderKeyChange}
              domain="purchase"
              kind="backorder"
              maxLength={128}
              label="Backorder key"
              disabled={isCreating}
            />
            <label htmlFor="backorder-quantity" className="block text-sm text-slate-400">
              Backorder quantity (optional)
              <input
                id="backorder-quantity"
                type="number"
                min="0"
                step="any"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={backorderQuantity}
                onChange={(e) => onBackorderQuantityChange(e.target.value)}
              />
            </label>
            <label htmlFor="backorder-notes" className="block text-sm text-slate-400">
              Backorder notes
              <input
                id="backorder-notes"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={backorderNotes}
                onChange={(e) => onBackorderNotesChange(e.target.value)}
              />
            </label>
            <button
              type="button"
              className="rounded-lg bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
              disabled={!selectedPurchaseOrderLineId || !backorderKey || isCreating}
              onClick={onCreateFromPurchaseOrderLine}
            >
              {isCreating ? 'Creating…' : 'Create backorder'}
            </button>
          </div>

          <div className="space-y-3">
            <h3 className="text-sm font-medium text-slate-300">Selected backorder</h3>
            {selected ? (
              <>
                <p className="text-sm text-slate-400">
                  Line {selected.purchaseOrderLineNumber} on PO {selected.purchaseOrderKey}
                  {selected.purchaseRequestKey
                    ? ` (PR ${selected.purchaseRequestKey})`
                    : ''}
                </p>
                {selected.status === 'open' ? (
                  <>
                    <button
                      type="button"
                      className="mr-2 rounded-lg border border-emerald-600/50 px-4 py-2 text-sm text-emerald-300 hover:bg-emerald-500/10 disabled:opacity-50"
                      disabled={isFulfilling}
                      onClick={onFulfill}
                    >
                      {isFulfilling ? 'Fulfilling…' : 'Mark fulfilled'}
                    </button>
                    <label htmlFor="backorder-cancel-reason" className="mt-3 block text-sm text-slate-400">
                      Backorder cancel reason
                      <input
                        id="backorder-cancel-reason"
                        className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                        value={cancelReason}
                        onChange={(e) => onCancelReasonChange(e.target.value)}
                      />
                    </label>
                    <button
                      type="button"
                      className="mt-2 rounded-lg border border-rose-600/50 px-4 py-2 text-sm text-rose-300 hover:bg-rose-500/10 disabled:opacity-50"
                      disabled={!cancelReason.trim() || isCancelling}
                      onClick={onCancel}
                    >
                      {isCancelling ? 'Cancelling…' : 'Cancel backorder'}
                    </button>
                  </>
                ) : (
                  <p className="text-sm text-slate-500">Only open backorders can be fulfilled or cancelled.</p>
                )}
              </>
            ) : (
              <p className="text-sm text-slate-500">Select a backorder to manage it.</p>
            )}
          </div>
        </div>
      ) : null}
    </section>
  )
}
