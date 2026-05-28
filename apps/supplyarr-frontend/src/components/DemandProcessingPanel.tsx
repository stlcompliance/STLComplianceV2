import { useQuery } from '@tanstack/react-query'

import { getDemandProcessingDashboard } from '../api/client'

interface DemandProcessingPanelProps {
  accessToken: string
  canRead: boolean
}

function formatOutcome(outcome: string): string {
  return outcome.replaceAll('_', ' ')
}

function outcomeBadgeClass(outcome: string): string {
  switch (outcome) {
    case 'stock_available':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'stock_short':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'pr_drafted':
      return 'bg-violet-500/20 text-violet-300 ring-violet-500/40'
    case 'no_catalog_parts':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

export function DemandProcessingPanel({ accessToken, canRead }: DemandProcessingPanelProps) {
  const dashboardQuery = useQuery({
    queryKey: ['supplyarr-demand-processing', accessToken],
    queryFn: () => getDemandProcessingDashboard(accessToken),
    enabled: canRead,
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="demand-processing-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">MaintainArr demand processing</h2>
      <p className="mt-1 text-sm text-slate-400">
        Stock availability evaluation and procurement recommendations for work-order demand references.
      </p>

      {dashboardQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading demand processing dashboard…</p>
      )}

      {dashboardQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load demand processing dashboard.</p>
      )}

      {dashboardQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-3 text-sm">
            <span className="rounded-md bg-slate-800 px-3 py-1 text-slate-200">
              Pending: {dashboardQuery.data.pendingCount}
            </span>
            <span className="rounded-md bg-amber-950 px-3 py-1 text-amber-200">
              Stock short: {dashboardQuery.data.stockShortCount}
            </span>
            <span className="rounded-md bg-emerald-950 px-3 py-1 text-emerald-200">
              In stock: {dashboardQuery.data.stockAvailableCount}
            </span>
            <span className="rounded-md bg-violet-950 px-3 py-1 text-violet-200">
              PR drafted: {dashboardQuery.data.prDraftedCount}
            </span>
          </div>

          {dashboardQuery.data.items.length === 0 ? (
            <p className="mt-4 text-sm text-slate-500">No processed demand references yet.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
              {dashboardQuery.data.items.map((item) => (
                <li key={item.processingStateId} className="px-3 py-3">
                  <div className="flex flex-wrap items-start justify-between gap-2">
                    <div>
                      <div className="font-medium text-slate-100">
                        WO {item.maintainarrWorkOrderNumber} · {item.title}
                      </div>
                      <div className="text-xs text-slate-500">
                        {item.linesShortCount} short of {item.linesCatalogCount} catalog lines
                      </div>
                    </div>
                    <span
                      className={`rounded px-2 py-0.5 text-xs ring-1 ${outcomeBadgeClass(item.processingOutcome)}`}
                    >
                      {formatOutcome(item.processingOutcome)}
                    </span>
                  </div>
                  {item.lastProcessingMessage && (
                    <p className="mt-2 text-slate-300">{item.lastProcessingMessage}</p>
                  )}
                </li>
              ))}
            </ul>
          )}
        </>
      )}
    </section>
  )
}
