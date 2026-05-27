import type {
  WorkOrderDetailResponse,
  WorkOrderPartsDemandLineResponse,
} from '../api/types'

interface WorkOrderPartsDemandPanelProps {
  workOrder: WorkOrderDetailResponse | null
  demandLines: WorkOrderPartsDemandLineResponse[]
  canPerform: boolean
  partNumber: string
  supplyarrPartId: string
  quantityRequested: string
  unitOfMeasure: string
  notes: string
  createPurchaseRequestDraft: boolean
  onPartNumberChange: (value: string) => void
  onSupplyarrPartIdChange: (value: string) => void
  onQuantityRequestedChange: (value: string) => void
  onUnitOfMeasureChange: (value: string) => void
  onNotesChange: (value: string) => void
  onCreatePurchaseRequestDraftChange: (value: boolean) => void
  onAddDemandLine: () => void
  onPublishDemand: () => void
  isAdding: boolean
  isPublishing: boolean
}

function workOrderEditable(status: string): boolean {
  return status === 'open' || status === 'in_progress'
}

function procurementBadgeClass(status: string): string {
  switch (status) {
    case 'pr_drafted':
    case 'pr_submitted':
    case 'pr_approved':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'po_created':
    case 'po_issued':
      return 'bg-violet-500/20 text-violet-300 ring-violet-500/40'
    case 'partially_received':
    case 'received_complete':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'pr_rejected':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

export function WorkOrderPartsDemandPanel({
  workOrder,
  demandLines,
  canPerform,
  partNumber,
  supplyarrPartId,
  quantityRequested,
  unitOfMeasure,
  notes,
  createPurchaseRequestDraft,
  onPartNumberChange,
  onSupplyarrPartIdChange,
  onQuantityRequestedChange,
  onUnitOfMeasureChange,
  onNotesChange,
  onCreatePurchaseRequestDraftChange,
  onAddDemandLine,
  onPublishDemand,
  isAdding,
  isPublishing,
}: WorkOrderPartsDemandPanelProps) {
  if (!workOrder) {
    return null
  }

  const editable = workOrderEditable(workOrder.status)
  const pendingCount = demandLines.filter((line) => line.status === 'pending').length

  return (
    <section className="mt-6 border-t border-slate-800 pt-4">
      <h4 className="text-sm font-semibold text-white">Parts demand (SupplyArr)</h4>
      <p className="mt-1 text-xs text-slate-500">
        Request parts for this work order. Publish sends demand to SupplyArr with opaque work order
        references.
      </p>

      {demandLines.length === 0 ? (
        <p className="mt-3 text-sm text-slate-400">No parts demand lines yet.</p>
      ) : (
        <ul className="mt-3 space-y-2 text-sm text-slate-300">
          {demandLines.map((line) => (
            <li
              key={line.demandLineId}
              className="rounded border border-slate-800 bg-slate-900/40 px-3 py-2"
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <span className="font-medium text-white">
                  #{line.lineNumber} {line.partNumber}
                </span>
                <span className="rounded px-2 py-0.5 text-xs ring-1 ring-slate-700">{line.status}</span>
                {line.status === 'published' ? (
                  <span
                    className={`rounded px-2 py-0.5 text-xs ring-1 ${procurementBadgeClass(line.procurementStatus)}`}
                  >
                    {line.procurementStatus}
                  </span>
                ) : null}
              </div>
              <div className="mt-1 text-xs text-slate-500">
                Qty {line.quantityRequested} {line.unitOfMeasure}
                {line.quantityReceived > 0 ? ` · received ${line.quantityReceived}` : ''}
                {line.supplyarrDemandRefId ? ` · SupplyArr ref ${line.supplyarrDemandRefId.slice(0, 8)}…` : ''}
                {line.procurementStatusMessage ? ` · ${line.procurementStatusMessage}` : ''}
              </div>
            </li>
          ))}
        </ul>
      )}

      {canPerform && editable ? (
        <div className="mt-4 grid gap-3 md:grid-cols-2">
          <label className="block text-xs text-slate-400">
            SupplyArr part id (optional)
            <input
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
              value={supplyarrPartId}
              onChange={(event) => onSupplyarrPartIdChange(event.target.value)}
            />
          </label>
          <label className="block text-xs text-slate-400">
            Part number
            <input
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
              value={partNumber}
              onChange={(event) => onPartNumberChange(event.target.value)}
            />
          </label>
          <label className="block text-xs text-slate-400">
            Quantity
            <input
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
              value={quantityRequested}
              onChange={(event) => onQuantityRequestedChange(event.target.value)}
            />
          </label>
          <label className="block text-xs text-slate-400">
            Unit of measure
            <input
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
              value={unitOfMeasure}
              onChange={(event) => onUnitOfMeasureChange(event.target.value)}
            />
          </label>
          <label className="md:col-span-2 block text-xs text-slate-400">
            Notes
            <input
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
              value={notes}
              onChange={(event) => onNotesChange(event.target.value)}
            />
          </label>
          <div className="flex flex-wrap gap-2 md:col-span-2">
            <button
              type="button"
              className="rounded bg-sky-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
              disabled={isAdding}
              onClick={onAddDemandLine}
            >
              Add demand line
            </button>
          </div>
        </div>
      ) : null}

      {canPerform && pendingCount > 0 ? (
        <div className="mt-4 flex flex-wrap items-center gap-3">
          <label className="flex items-center gap-2 text-xs text-slate-400">
            <input
              type="checkbox"
              checked={createPurchaseRequestDraft}
              onChange={(event) => onCreatePurchaseRequestDraftChange(event.target.checked)}
            />
            Create SupplyArr purchase request draft
          </label>
          <button
            type="button"
            className="rounded bg-emerald-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            disabled={isPublishing}
            onClick={onPublishDemand}
          >
            Publish {pendingCount} line(s) to SupplyArr
          </button>
        </div>
      ) : null}
    </section>
  )
}
