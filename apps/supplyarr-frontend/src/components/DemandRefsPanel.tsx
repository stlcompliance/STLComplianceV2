import { Link } from 'react-router-dom'

import type { MaintainArrDemandRefResponse, PartResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface DemandRefsPanelProps {
  demandRefs: MaintainArrDemandRefResponse[]
  parts: PartResponse[]
  canCreatePurchaseRequest: boolean
  isLoading: boolean
  selectedDemandRefId: string
  prRequestKey: string
  prTitle: string
  prNotes: string
  onSelectedDemandRefIdChange: (value: string) => void
  onPrRequestKeyChange: (value: string) => void
  onPrTitleChange: (value: string) => void
  onPrNotesChange: (value: string) => void
  onCreatePurchaseRequest: () => void
  isCreatingPurchaseRequest: boolean
}

const PROCUREMENT_JOURNEY_STEPS = [
  'received',
  'pr_drafted',
  'pr_submitted',
  'pr_approved',
  'po_created',
  'po_issued',
  'partially_received',
  'received_complete',
] as const

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'pr_drafted':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'received':
      return 'bg-sky-500/20 text-sky-300 ring-sky-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

function procurementBadgeClass(status: string): string {
  switch (status) {
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

function procurementStepIndex(status: string): number {
  const normalized = status === 'pr_rejected' ? 'pr_drafted' : status
  const index = PROCUREMENT_JOURNEY_STEPS.indexOf(normalized as (typeof PROCUREMENT_JOURNEY_STEPS)[number])
  return index >= 0 ? index : 0
}

function formatStepLabel(step: string): string {
  return step.replaceAll('_', ' ')
}

export function DemandRefsPanel({
  demandRefs,
  parts,
  canCreatePurchaseRequest,
  isLoading,
  selectedDemandRefId,
  prRequestKey,
  prTitle,
  prNotes,
  onSelectedDemandRefIdChange,
  onPrRequestKeyChange,
  onPrTitleChange,
  onPrNotesChange,
  onCreatePurchaseRequest,
  isCreatingPurchaseRequest,
}: DemandRefsPanelProps) {
  const selected = demandRefs.find((ref) => ref.demandRefId === selectedDemandRefId) ?? null
  const activeStepIndex = selected ? procurementStepIndex(selected.procurementStatus) : -1
  const prRequestKeySource =
    prTitle.trim() ||
    (selected
      ? `${selected.maintainarrWorkOrderNumber} ${selected.title} purchase request`
      : '')

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/40 p-5" data-testid="demand-refs-panel">
      <h2 className="text-lg font-semibold text-white">Work-order demand intake</h2>
      <p className="mt-1 text-sm text-slate-400">
        Work-order parts demand comes in here. Track the procurement journey from intake through receiving.
      </p>

      {isLoading ? <p className="mt-4 text-sm text-slate-400">Loading demand references…</p> : null}

      {!isLoading && demandRefs.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No demand references yet.</p>
      ) : null}

      {demandRefs.length > 0 ? (
        <div className="mt-4 overflow-x-auto">
          <table className="min-w-full text-left text-sm text-slate-300">
            <thead className="text-xs uppercase text-[var(--color-text-muted)]">
              <tr>
                <th className="px-2 py-2">Work order</th>
                <th className="px-2 py-2">Title</th>
                <th className="px-2 py-2">Status</th>
                <th className="px-2 py-2">Procurement</th>
                <th className="px-2 py-2">Lines</th>
                <th className="px-2 py-2">PR</th>
              </tr>
            </thead>
            <tbody>
              {demandRefs.map((ref) => (
                <tr
                  key={ref.demandRefId}
                  className={`cursor-pointer border-t border-slate-800 ${
                    selectedDemandRefId === ref.demandRefId ? 'bg-slate-800/60' : ''
                  }`}
                  onClick={() => onSelectedDemandRefIdChange(ref.demandRefId)}
                >
                  <td className="px-2 py-2 font-mono text-white">{ref.maintainarrWorkOrderNumber}</td>
                  <td className="px-2 py-2">{ref.title}</td>
                  <td className="px-2 py-2">
                    <span className={`rounded px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(ref.status)}`}>
                      {ref.status}
                    </span>
                  </td>
                  <td className="px-2 py-2">
                    <span
                      className={`rounded px-2 py-0.5 text-xs ring-1 ${procurementBadgeClass(ref.procurementStatus)}`}
                    >
                      {ref.procurementStatus}
                    </span>
                  </td>
                  <td className="px-2 py-2">{ref.lines.length}</td>
                  <td className="px-2 py-2">{ref.purchaseRequestId ? 'Yes' : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}

      {selected ? (
        <div className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <h3 className="text-sm font-semibold text-white">Selected demand reference</h3>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Work order {selected.maintainarrWorkOrderNumber} · asset {selected.maintainarrAssetId.slice(0, 8)}…
          </p>

          <div className="mt-3" data-testid="demand-ref-procurement-journey">
            <h4 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">
              Procurement journey
            </h4>
            <ol className="mt-2 flex flex-wrap gap-2">
              {PROCUREMENT_JOURNEY_STEPS.map((step, index) => {
                const reached = index <= activeStepIndex
                const current = index === activeStepIndex
                return (
                  <li
                    key={step}
                    className={`rounded px-2 py-1 text-xs ring-1 ${
                      current
                        ? procurementBadgeClass(selected.procurementStatus)
                        : reached
                          ? 'bg-slate-700/40 text-slate-300 ring-slate-600/40'
                          : 'bg-slate-900/40 text-[var(--color-text-muted)] ring-slate-800'
                    }`}
                  >
                    {formatStepLabel(step)}
                  </li>
                )
              })}
            </ol>
            <div className="mt-2 flex flex-wrap gap-3 text-xs text-[var(--color-text-muted)]">
              <span>Received {new Date(selected.receivedAt).toLocaleString()}</span>
              {selected.lastStatusCallbackAt ? (
                <span>Last callback {new Date(selected.lastStatusCallbackAt).toLocaleString()}</span>
              ) : null}
            </div>
            <div className="mt-2 flex flex-wrap gap-3 text-xs">
              {selected.purchaseRequestId ? (
                <Link
                  to="/purchasing"
                  className="text-violet-300 underline-offset-2 hover:underline"
                  data-testid="demand-ref-open-pr"
                >
                  Open purchase request {selected.purchaseRequestId.slice(0, 8)}…
                </Link>
              ) : null}
              {selected.purchaseOrderId ? (
                <Link
                  to="/purchasing"
                  className="text-violet-300 underline-offset-2 hover:underline"
                  data-testid="demand-ref-open-po"
                >
                  Linked PO {selected.purchaseOrderId.slice(0, 8)}…
                </Link>
              ) : null}
            </div>
          </div>

          <ul className="mt-3 space-y-2 text-sm">
            {selected.lines.map((line) => {
              const part = parts.find((p) => p.partId === line.partId)
              return (
                <li key={line.lineId} className="text-slate-300">
                  {line.partNumber} · qty {line.quantityRequested} {line.unitOfMeasure}
                  {part ? ` · ${part.displayName}` : ''}
                </li>
              )
            })}
          </ul>

          {canCreatePurchaseRequest && !selected.purchaseRequestId ? (
            <div className="mt-4 grid gap-3 md:grid-cols-2">
              <GeneratedKeyFieldGroup
                sourceLabel={prRequestKeySource}
                existingKeys={[]}
                onKeyChange={onPrRequestKeyChange}
                domain="purchase"
                kind="request"
                maxLength={128}
                label="PR request key"
                disabled={isCreatingPurchaseRequest}
              />
              <label htmlFor="demand-ref-pr-title" className="block text-xs text-slate-400">
                PR title
                <input
                  id="demand-ref-pr-title"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
                  value={prTitle}
                  onChange={(event) => onPrTitleChange(event.target.value)}
                />
              </label>
              <label htmlFor="demand-ref-pr-notes" className="md:col-span-2 block text-xs text-slate-400">
                PR notes
                <input
                  id="demand-ref-pr-notes"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-sm text-white"
                  value={prNotes}
                  onChange={(event) => onPrNotesChange(event.target.value)}
                />
              </label>
              <button
                type="button"
                className="rounded bg-emerald-600 px-3 py-1.5 text-sm text-white disabled:opacity-50 md:col-span-2 md:w-fit"
                disabled={isCreatingPurchaseRequest || !prRequestKey || !prTitle}
                onClick={onCreatePurchaseRequest}
              >
                Create purchase request draft
              </button>
            </div>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}
