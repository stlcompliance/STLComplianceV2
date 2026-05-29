import type { WorkOrderSupplyReadinessResponse } from '../api/types'

interface WorkOrderSupplyReadinessPanelProps {
  readiness: WorkOrderSupplyReadinessResponse | null
  isLoading: boolean
}

function overallBadgeClass(status: string): string {
  switch (status) {
    case 'ready':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'not_ready':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    case 'no_demand':
    case 'unknown':
      return 'bg-slate-500/20 text-slate-300 ring-slate-700'
    default:
      return 'bg-amber-500/20 text-amber-200 ring-amber-500/40'
  }
}

function overallLabel(status: string): string {
  switch (status) {
    case 'ready':
      return 'Supply ready'
    case 'not_ready':
      return 'Supply blocked'
    case 'no_demand':
      return 'No parts demand'
    case 'unknown':
      return 'Readiness unknown'
    default:
      return status.replace(/_/g, ' ')
  }
}

function lineBadgeClass(status: string | null): string {
  if (status === 'ready') {
    return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
  }
  if (status === 'not_ready') {
    return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
  }
  return 'bg-slate-500/20 text-slate-300 ring-slate-700'
}

export function WorkOrderSupplyReadinessPanel({
  readiness,
  isLoading,
}: WorkOrderSupplyReadinessPanelProps) {
  return (
    <section
      className="mt-4 border-t border-slate-800 pt-4"
      data-testid="work-order-supply-readiness-panel"
    >
      <h5 className="text-xs font-semibold uppercase tracking-wide text-slate-500">
        Supply readiness (SupplyArr)
      </h5>
      <p className="mt-1 text-xs text-slate-500">
        Part availability and procurement blockers for demand lines with linked SupplyArr part IDs.
      </p>

      {isLoading ? (
        <p className="mt-3 text-sm text-slate-400" data-testid="work-order-supply-readiness-loading">
          Loading supply readiness…
        </p>
      ) : !readiness ? (
        <p className="mt-3 text-sm text-slate-400">Select a work order to review supply readiness.</p>
      ) : (
        <div className="mt-3 space-y-3">
          <div className="flex flex-wrap items-center gap-2">
            <span
              className={`rounded px-2 py-0.5 text-xs ring-1 ${overallBadgeClass(readiness.overallReadinessStatus)}`}
              data-testid="work-order-supply-readiness-overall"
            >
              {overallLabel(readiness.overallReadinessStatus)}
            </span>
            <span className="text-xs text-slate-500">
              {readiness.linesReady} ready · {readiness.linesBlocked} blocked · {readiness.linesSkipped}{' '}
              skipped
            </span>
          </div>

          {readiness.lines.length === 0 ? (
            <p className="text-sm text-slate-400">Add parts demand lines to evaluate supply readiness.</p>
          ) : (
            <ul className="space-y-2 text-sm">
              {readiness.lines.map((line) => (
                <li
                  key={line.demandLineId}
                  className="rounded border border-slate-800 bg-slate-900/40 px-3 py-2"
                  data-testid={`work-order-supply-readiness-line-${line.demandLineId}`}
                >
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <span className="font-medium text-white">
                      #{line.lineNumber} {line.partNumber}
                    </span>
                    {line.readinessStatus ? (
                      <span
                        className={`rounded px-2 py-0.5 text-xs ring-1 ${lineBadgeClass(line.readinessStatus)}`}
                      >
                        {line.readinessStatus}
                      </span>
                    ) : (
                      <span className="rounded px-2 py-0.5 text-xs ring-1 ring-slate-700 text-slate-400">
                        skipped
                      </span>
                    )}
                  </div>
                  <div className="mt-1 text-xs text-slate-500">
                    Qty {line.quantityRequested}
                    {line.quantityAvailable != null ? ` · available ${line.quantityAvailable}` : ''}
                    {line.skipReason ? ` · ${line.skipReason.replace(/_/g, ' ')}` : ''}
                  </div>
                  {line.blockers.length > 0 ? (
                    <ul className="mt-2 space-y-1 text-xs text-rose-200/90">
                      {line.blockers.map((blocker) => (
                        <li key={`${line.demandLineId}-${blocker.reasonCode}-${blocker.sourceEntityId}`}>
                          <span className="font-mono text-rose-300/80">{blocker.reasonCode}</span>
                          {' — '}
                          {blocker.message}
                        </li>
                      ))}
                    </ul>
                  ) : null}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </section>
  )
}
