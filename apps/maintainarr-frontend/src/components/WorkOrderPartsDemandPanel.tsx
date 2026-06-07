import { AdvancedReferenceField, ControlledSelect } from '@stl/shared-ui'

import type {
  WorkOrderDetailResponse,
  WorkOrderPartsDemandLineResponse,
  WorkOrderPartsDemandStatusEventResponse,
} from '../api/types'
import { PARTS_DEMAND_UOM_OPTIONS } from './formOptions'

interface WorkOrderPartsDemandPanelProps {
  workOrder: WorkOrderDetailResponse | null
  demandLines: WorkOrderPartsDemandLineResponse[]
  statusEvents: WorkOrderPartsDemandStatusEventResponse[]
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
  return status === 'draft' || status === 'open' || status === 'in_progress'
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
    case 'fulfilled':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'pr_rejected':
    case 'cancelled':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

export function WorkOrderPartsDemandPanel({
  workOrder,
  demandLines,
  statusEvents,
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
    <section
      className="mt-6 border-t border-slate-800 pt-4"
      data-testid="work-order-parts-demand-panel"
    >
      <h4 className="text-sm font-semibold text-white">Parts demand (SupplyArr)</h4>
      <p className="mt-1 text-xs text-slate-500">
        Request parts for this work order. Publish sends demand to SupplyArr with opaque work order
        references; procurement status updates arrive via SupplyArr callbacks.
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
                    data-testid={`procurement-status-${line.demandLineId}`}
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
                {line.lastProcurementStatusAt
                  ? ` · updated ${new Date(line.lastProcurementStatusAt).toLocaleString()}`
                  : ''}
              </div>
            </li>
          ))}
        </ul>
      )}

      {statusEvents.length > 0 ? (
        <div className="mt-4 border-t border-slate-800 pt-3" data-testid="parts-demand-status-timeline">
          <h5 className="text-xs font-semibold uppercase tracking-wide text-slate-500">
            Procurement status timeline
          </h5>
          <ol className="mt-2 space-y-2 text-xs text-slate-400">
            {statusEvents.map((event) => (
              <li
                key={event.statusEventId}
                className="rounded border border-slate-800/80 bg-slate-950/30 px-2 py-1.5"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <span
                    className={`rounded px-1.5 py-0.5 ring-1 ${procurementBadgeClass(event.procurementStatus)}`}
                  >
                    {event.procurementStatus}
                  </span>
                  <span className="text-slate-500">{event.eventType}</span>
                  <span className="text-slate-600">
                    {new Date(event.occurredAt).toLocaleString()}
                  </span>
                </div>
                {event.message ? <p className="mt-1 text-slate-500">{event.message}</p> : null}
              </li>
            ))}
          </ol>
        </div>
      ) : null}

      {canPerform && editable ? (
        <div className="mt-4 grid gap-3 md:grid-cols-2">
          <div className="block text-xs text-slate-400">
            SupplyArr part (optional)
            <AdvancedReferenceField
              value={supplyarrPartId}
              onChange={onSupplyarrPartIdChange}
              label="SupplyArr part ID"
              followUpId="maintainarr-supplyarr-part-picker"
              testId="work-order-parts-demand-supplyarr-part"
            />
          </div>
          <label className="block text-xs text-slate-400" htmlFor="workorderpartsdemand-part-number">
          Part number
          <input id="workorderpartsdemand-part-number"
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
              value={partNumber}
              onChange={(event) => onPartNumberChange(event.target.value)}
            />
          </label>
          <label className="block text-xs text-slate-400" htmlFor="workorderpartsdemand-quantity">
          Quantity
          <input id="workorderpartsdemand-quantity"
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
              value={quantityRequested}
              onChange={(event) => onQuantityRequestedChange(event.target.value)}
            />
          </label>
          <ControlledSelect
            label="Unit of measure"
            value={unitOfMeasure}
            onChange={onUnitOfMeasureChange}
            options={PARTS_DEMAND_UOM_OPTIONS}
            emptyLabel="Select unit…"
            testId="work-order-parts-demand-uom"
            className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
          />
          <label className="md:col-span-2 block text-xs text-slate-400" htmlFor="workorderpartsdemand-notes">
          Notes
          <input id="workorderpartsdemand-notes"
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
            <input id="workorderpartsdemand"
              type="checkbox"
              checked={createPurchaseRequestDraft}
              onChange={(event) => onCreatePurchaseRequestDraftChange(event.target.checked)}
            />
            Create SupplyArr purchase request draft
          </label>
          <button
            type="button"
            className="rounded bg-emerald-600 px-3 py-1.5 text-sm text-white disabled:opacity-50"
            data-testid="work-order-parts-demand-publish"
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
