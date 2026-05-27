import type { PartResponse, PurchaseRequestResponse } from '../api/types'

interface PurchaseRequestPanelProps {
  purchaseRequests: PurchaseRequestResponse[]
  parts: PartResponse[]
  vendors: { partyId: string; displayName: string; partyKey: string }[]
  canCreate: boolean
  canApprove: boolean
  isLoading: boolean
  requestKey: string
  title: string
  notes: string
  selectedVendorId: string
  selectedPartId: string
  lineQuantity: string
  lineNotes: string
  rejectionReason: string
  selectedPurchaseRequestId: string
  onRequestKeyChange: (value: string) => void
  onTitleChange: (value: string) => void
  onNotesChange: (value: string) => void
  onSelectedVendorIdChange: (value: string) => void
  onSelectedPartIdChange: (value: string) => void
  onLineQuantityChange: (value: string) => void
  onLineNotesChange: (value: string) => void
  onRejectionReasonChange: (value: string) => void
  onSelectedPurchaseRequestIdChange: (value: string) => void
  onCreate: () => void
  onSubmit: () => void
  onApprove: () => void
  onReject: () => void
  isCreating: boolean
  isSubmitting: boolean
  isApproving: boolean
  isRejecting: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'submitted':
      return 'bg-sky-500/20 text-sky-300 ring-sky-500/40'
    case 'rejected':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    case 'draft':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

export function PurchaseRequestPanel({
  purchaseRequests,
  parts,
  vendors,
  canCreate,
  canApprove,
  isLoading,
  requestKey,
  title,
  notes,
  selectedVendorId,
  selectedPartId,
  lineQuantity,
  lineNotes,
  rejectionReason,
  selectedPurchaseRequestId,
  onRequestKeyChange,
  onTitleChange,
  onNotesChange,
  onSelectedVendorIdChange,
  onSelectedPartIdChange,
  onLineQuantityChange,
  onLineNotesChange,
  onRejectionReasonChange,
  onSelectedPurchaseRequestIdChange,
  onCreate,
  onSubmit,
  onApprove,
  onReject,
  isCreating,
  isSubmitting,
  isApproving,
  isRejecting,
}: PurchaseRequestPanelProps) {
  const selected = purchaseRequests.find((pr) => pr.purchaseRequestId === selectedPurchaseRequestId)

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg">
      <h2 className="text-lg font-medium text-white">Purchase requests</h2>
      <p className="mt-1 text-sm text-slate-400">Draft, submit, and approve procurement requests.</p>

      {isLoading ? <p className="mt-4 text-sm text-slate-500">Loading purchase requests…</p> : null}

      <ul className="mt-4 space-y-2">
        {purchaseRequests.map((pr) => (
          <li key={pr.purchaseRequestId}>
            <button
              type="button"
              className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                selectedPurchaseRequestId === pr.purchaseRequestId
                  ? 'border-sky-500/60 bg-sky-500/10'
                  : 'border-slate-800 bg-slate-950/40 hover:border-slate-700'
              }`}
              onClick={() => onSelectedPurchaseRequestIdChange(pr.purchaseRequestId)}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="font-medium text-slate-200">{pr.requestKey}</span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs ring-1 ring-inset ${statusBadgeClass(pr.status)}`}
                >
                  {pr.status}
                </span>
              </div>
              <div className="mt-1 text-slate-400">{pr.title}</div>
              <div className="mt-1 text-xs text-slate-500">
                {pr.lines.length} line{pr.lines.length === 1 ? '' : 's'}
                {pr.vendorDisplayName ? ` · ${pr.vendorDisplayName}` : ''}
              </div>
            </button>
          </li>
        ))}
        {!isLoading && purchaseRequests.length === 0 ? (
          <li className="text-sm text-slate-500">No purchase requests yet.</li>
        ) : null}
      </ul>

      {selected ? (
        <div className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-3 text-sm">
          <div className="font-medium text-slate-200">{selected.title}</div>
          {selected.notes ? <p className="mt-1 text-slate-400">{selected.notes}</p> : null}
          <ul className="mt-2 space-y-1 text-slate-400">
            {selected.lines.map((line) => (
              <li key={line.lineId}>
                #{line.lineNumber} {line.partDisplayName} ({line.partKey}) — {line.quantityRequested}{' '}
                {line.unitOfMeasure}
              </li>
            ))}
          </ul>
          {selected.status === 'rejected' && selected.rejectionReason ? (
            <p className="mt-2 text-rose-300">Rejected: {selected.rejectionReason}</p>
          ) : null}
          <div className="mt-3 flex flex-wrap gap-2">
            {canCreate && selected.status === 'draft' ? (
              <button
                type="button"
                className="rounded-md bg-sky-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-sky-500 disabled:opacity-50"
                disabled={isSubmitting}
                onClick={onSubmit}
              >
                {isSubmitting ? 'Submitting…' : 'Submit'}
              </button>
            ) : null}
            {canApprove && selected.status === 'submitted' ? (
              <>
                <button
                  type="button"
                  className="rounded-md bg-emerald-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
                  disabled={isApproving}
                  onClick={onApprove}
                >
                  {isApproving ? 'Approving…' : 'Approve'}
                </button>
                <input
                  className="min-w-[10rem] flex-1 rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 text-xs text-slate-200"
                  placeholder="Rejection reason"
                  value={rejectionReason}
                  onChange={(e) => onRejectionReasonChange(e.target.value)}
                />
                <button
                  type="button"
                  className="rounded-md bg-rose-700 px-3 py-1.5 text-xs font-medium text-white hover:bg-rose-600 disabled:opacity-50"
                  disabled={isRejecting || !rejectionReason.trim()}
                  onClick={onReject}
                >
                  {isRejecting ? 'Rejecting…' : 'Reject'}
                </button>
              </>
            ) : null}
          </div>
        </div>
      ) : null}

      {canCreate ? (
        <div className="mt-6 space-y-3 border-t border-slate-800 pt-4">
          <h3 className="text-sm font-medium text-slate-300">New purchase request</h3>
          <div className="grid gap-2 sm:grid-cols-2">
            <input
              className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
              placeholder="Request key"
              value={requestKey}
              onChange={(e) => onRequestKeyChange(e.target.value)}
            />
            <input
              className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
              placeholder="Title"
              value={title}
              onChange={(e) => onTitleChange(e.target.value)}
            />
          </div>
          <textarea
            className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
            placeholder="Notes"
            rows={2}
            value={notes}
            onChange={(e) => onNotesChange(e.target.value)}
          />
          <select
            className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
            value={selectedVendorId}
            onChange={(e) => onSelectedVendorIdChange(e.target.value)}
          >
            <option value="">Vendor (optional)</option>
            {vendors.map((v) => (
              <option key={v.partyId} value={v.partyId}>
                {v.displayName} ({v.partyKey})
              </option>
            ))}
          </select>
          <div className="grid gap-2 sm:grid-cols-3">
            <select
              className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200 sm:col-span-2"
              value={selectedPartId}
              onChange={(e) => onSelectedPartIdChange(e.target.value)}
            >
              <option value="">Part for first line</option>
              {parts.map((p) => (
                <option key={p.partId} value={p.partId}>
                  {p.displayName} ({p.partKey})
                </option>
              ))}
            </select>
            <input
              className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
              placeholder="Qty"
              type="number"
              min="0"
              step="any"
              value={lineQuantity}
              onChange={(e) => onLineQuantityChange(e.target.value)}
            />
          </div>
          <input
            className="w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-200"
            placeholder="Line notes"
            value={lineNotes}
            onChange={(e) => onLineNotesChange(e.target.value)}
          />
          <button
            type="button"
            className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
            disabled={isCreating || !requestKey.trim() || !title.trim() || !selectedPartId || !lineQuantity}
            onClick={onCreate}
          >
            {isCreating ? 'Creating…' : 'Create draft'}
          </button>
        </div>
      ) : null}
    </section>
  )
}
