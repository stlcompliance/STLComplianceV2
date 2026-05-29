import type {
  ExternalPartyResponse,
  PartResponse,
  PurchaseOrderResponse,
  VendorReturnResponse,
} from '../api/types'

interface ReturnsPanelProps {
  returns: VendorReturnResponse[]
  vendors: ExternalPartyResponse[]
  parts: PartResponse[]
  issuedPurchaseOrders: PurchaseOrderResponse[]
  inventoryBins: { binId: string; binKey: string; name: string; label: string }[]
  canManage: boolean
  isLoading: boolean
  returnKey: string
  selectedReturnId: string
  selectedVendorPartyId: string
  selectedInventoryBinId: string
  selectedReturnPoLineId: string
  selectedReturnPartId: string
  returnQuantity: string
  rmaNumber: string
  returnNotes: string
  cancelReason: string
  statusFilter: string
  returnSource: 'stock' | 'purchase_order_line'
  onReturnKeyChange: (value: string) => void
  onSelectedReturnIdChange: (value: string) => void
  onSelectedVendorPartyIdChange: (value: string) => void
  onSelectedInventoryBinIdChange: (value: string) => void
  onSelectedReturnPoLineIdChange: (value: string) => void
  onSelectedReturnPartIdChange: (value: string) => void
  onReturnQuantityChange: (value: string) => void
  onRmaNumberChange: (value: string) => void
  onReturnNotesChange: (value: string) => void
  onCancelReasonChange: (value: string) => void
  onStatusFilterChange: (value: string) => void
  onReturnSourceChange: (value: 'stock' | 'purchase_order_line') => void
  onCreate: () => void
  onPost: () => void
  onCancel: () => void
  isCreating: boolean
  isPosting: boolean
  isCancelling: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'posted':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'draft':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'cancelled':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

function sourceLabel(sourceType: string): string {
  switch (sourceType) {
    case 'stock':
      return 'Stock'
    case 'purchase_order_line':
      return 'PO line'
    default:
      return sourceType
  }
}

export function ReturnsPanel({
  returns,
  vendors,
  parts,
  issuedPurchaseOrders,
  inventoryBins,
  canManage,
  isLoading,
  returnKey,
  selectedReturnId,
  selectedVendorPartyId,
  selectedInventoryBinId,
  selectedReturnPoLineId,
  selectedReturnPartId,
  returnQuantity,
  rmaNumber,
  returnNotes,
  cancelReason,
  statusFilter,
  returnSource,
  onReturnKeyChange,
  onSelectedReturnIdChange,
  onSelectedVendorPartyIdChange,
  onSelectedInventoryBinIdChange,
  onSelectedReturnPoLineIdChange,
  onSelectedReturnPartIdChange,
  onReturnQuantityChange,
  onRmaNumberChange,
  onReturnNotesChange,
  onCancelReasonChange,
  onStatusFilterChange,
  onReturnSourceChange,
  onCreate,
  onPost,
  onCancel,
  isCreating,
  isPosting,
  isCancelling,
}: ReturnsPanelProps) {
  const selected = returns.find((item) => item.returnId === selectedReturnId)
  const poLines = issuedPurchaseOrders.flatMap((po) =>
    po.lines
      .filter((line) => line.quantityReceived > 0)
      .map((line) => ({
        purchaseOrderLineId: line.lineId,
        label: `${po.orderKey} · line ${line.lineNumber} · ${line.partKey} (${line.quantityReceived} received)`,
      })),
  )

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg lg:col-span-2">
      <h2 className="text-lg font-medium text-white">Vendor returns</h2>
      <p className="mt-1 text-sm text-slate-400">
        Return parts to vendors from stock or against purchase orders with RMA tracking and stock
        decrement on post.
      </p>

      <div className="mt-4 flex flex-wrap items-end gap-3">
        <label htmlFor="vendor-return-status-filter" className="block text-sm text-slate-400">
          Return status filter
          <select
            id="vendor-return-status-filter"
            className="mt-1 block w-full min-w-[8rem] rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
            value={statusFilter}
            onChange={(e) => onStatusFilterChange(e.target.value)}
          >
            <option value="">All</option>
            <option value="draft">Draft</option>
            <option value="posted">Posted</option>
            <option value="cancelled">Cancelled</option>
          </select>
        </label>
      </div>

      {isLoading ? <p className="mt-4 text-sm text-slate-500">Loading vendor returns…</p> : null}

      <ul className="mt-4 space-y-2">
        {returns.map((item) => (
          <li key={item.returnId}>
            <button
              type="button"
              className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                selectedReturnId === item.returnId
                  ? 'border-violet-500/60 bg-violet-500/10'
                  : 'border-slate-800 bg-slate-950/40 hover:border-slate-700'
              }`}
              onClick={() => onSelectedReturnIdChange(item.returnId)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-slate-200">{item.returnKey}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(item.status)}`}
                >
                  {item.status}
                </span>
              </div>
              <div className="mt-1 text-slate-400">
                {item.vendorDisplayName}
                {item.rmaNumber ? ` · RMA ${item.rmaNumber}` : ''}
              </div>
              <div className="mt-1 text-xs text-slate-500">
                {item.lines.length} line(s) · {sourceLabel(item.sourceType)}
                {item.purchaseOrderKey ? ` · PO ${item.purchaseOrderKey}` : ''}
                {item.purchaseRequestKey ? ` · PR ${item.purchaseRequestKey}` : ''}
              </div>
            </button>
          </li>
        ))}
        {returns.length === 0 && !isLoading ? (
          <li className="text-sm text-slate-500">No vendor returns match this filter.</li>
        ) : null}
      </ul>

      {canManage ? (
        <div className="mt-6 grid gap-4 border-t border-slate-800 pt-4 md:grid-cols-2">
          <div className="space-y-3">
            <h3 className="text-sm font-medium text-slate-300">Create return</h3>
            <label htmlFor="vendor-return-source" className="block text-sm text-slate-400">
              Return source
              <select
                id="vendor-return-source"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={returnSource}
                onChange={(e) =>
                  onReturnSourceChange(e.target.value as 'stock' | 'purchase_order_line')
                }
              >
                <option value="stock">From stock</option>
                <option value="purchase_order_line">From PO line</option>
              </select>
            </label>
            <label htmlFor="vendor-return-key" className="block text-sm text-slate-400">
              Return key
              <input
                id="vendor-return-key"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={returnKey}
                onChange={(e) => onReturnKeyChange(e.target.value)}
              />
            </label>
            <label htmlFor="vendor-return-inventory-bin" className="block text-sm text-slate-400">
              Inventory bin
              <select
                id="vendor-return-inventory-bin"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={selectedInventoryBinId}
                onChange={(e) => onSelectedInventoryBinIdChange(e.target.value)}
              >
                <option value="">Select bin…</option>
                {inventoryBins.map((bin) => (
                  <option key={bin.binId} value={bin.binId}>
                    {bin.label}
                  </option>
                ))}
              </select>
            </label>
            {returnSource === 'stock' ? (
              <>
                <label htmlFor="vendor-return-vendor" className="block text-sm text-slate-400">
                  Return vendor
                  <select
                    id="vendor-return-vendor"
                    className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                    value={selectedVendorPartyId}
                    onChange={(e) => onSelectedVendorPartyIdChange(e.target.value)}
                  >
                    <option value="">Select vendor…</option>
                    {vendors.map((vendor) => (
                      <option key={vendor.partyId} value={vendor.partyId}>
                        {vendor.displayName} ({vendor.partyKey})
                      </option>
                    ))}
                  </select>
                </label>
                <label htmlFor="vendor-return-part" className="block text-sm text-slate-400">
                  Return part
                  <select
                    id="vendor-return-part"
                    className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                    value={selectedReturnPartId}
                    onChange={(e) => onSelectedReturnPartIdChange(e.target.value)}
                  >
                    <option value="">Select part…</option>
                    {parts.map((part) => (
                      <option key={part.partId} value={part.partId}>
                        {part.displayName} ({part.partKey})
                      </option>
                    ))}
                  </select>
                </label>
              </>
            ) : (
              <label htmlFor="vendor-return-po-line" className="block text-sm text-slate-400">
                Return PO line
                <select
                  id="vendor-return-po-line"
                  className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                  value={selectedReturnPoLineId}
                  onChange={(e) => onSelectedReturnPoLineIdChange(e.target.value)}
                >
                  <option value="">Select line…</option>
                  {poLines.map((line) => (
                    <option key={line.purchaseOrderLineId} value={line.purchaseOrderLineId}>
                      {line.label}
                    </option>
                  ))}
                </select>
              </label>
            )}
            <label htmlFor="vendor-return-quantity" className="block text-sm text-slate-400">
              Return quantity
              <input
                id="vendor-return-quantity"
                type="number"
                min="0"
                step="any"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={returnQuantity}
                onChange={(e) => onReturnQuantityChange(e.target.value)}
              />
            </label>
            <label htmlFor="vendor-return-rma-number" className="block text-sm text-slate-400">
              RMA number
              <input
                id="vendor-return-rma-number"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={rmaNumber}
                onChange={(e) => onRmaNumberChange(e.target.value)}
              />
            </label>
            <label htmlFor="vendor-return-notes" className="block text-sm text-slate-400">
              Return notes
              <input
                id="vendor-return-notes"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={returnNotes}
                onChange={(e) => onReturnNotesChange(e.target.value)}
              />
            </label>
            <button
              type="button"
              className="rounded-lg bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
              disabled={
                !returnKey ||
                !selectedInventoryBinId ||
                !returnQuantity ||
                isCreating ||
                (returnSource === 'stock'
                  ? !selectedVendorPartyId || !selectedReturnPartId
                  : !selectedReturnPoLineId)
              }
              onClick={onCreate}
            >
              {isCreating ? 'Creating…' : 'Create return'}
            </button>
          </div>

          <div className="space-y-3">
            <h3 className="text-sm font-medium text-slate-300">Selected return</h3>
            {selected ? (
              <>
                <p className="text-sm text-slate-400">
                  {selected.vendorDisplayName} · bin {selected.inventoryBinKey}
                  {selected.rmaNumber ? ` · RMA ${selected.rmaNumber}` : ''}
                </p>
                <ul className="text-sm text-slate-500">
                  {selected.lines.map((line) => (
                    <li key={line.lineId}>
                      Line {line.lineNumber}: {line.partKey} × {line.quantity}
                    </li>
                  ))}
                </ul>
                {selected.status === 'draft' ? (
                  <>
                    <button
                      type="button"
                      className="mr-2 rounded-lg border border-emerald-600/50 px-4 py-2 text-sm text-emerald-300 hover:bg-emerald-500/10 disabled:opacity-50"
                      disabled={isPosting}
                      onClick={onPost}
                    >
                      {isPosting ? 'Posting…' : 'Post return'}
                    </button>
                    <label htmlFor="vendor-return-cancel-reason" className="mt-3 block text-sm text-slate-400">
                      Return cancel reason
                      <input
                        id="vendor-return-cancel-reason"
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
                      {isCancelling ? 'Cancelling…' : 'Cancel return'}
                    </button>
                  </>
                ) : (
                  <p className="text-sm text-slate-500">
                    Only draft returns can be posted or cancelled.
                  </p>
                )}
              </>
            ) : (
              <p className="text-sm text-slate-500">Select a return to manage it.</p>
            )}
          </div>
        </div>
      ) : null}
    </section>
  )
}
