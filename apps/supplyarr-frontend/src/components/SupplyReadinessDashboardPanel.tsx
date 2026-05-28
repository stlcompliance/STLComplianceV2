import { useQuery } from '@tanstack/react-query'

import { getSupplyReadinessDashboard } from '../api/client'

interface SupplyReadinessDashboardPanelProps {
  accessToken: string
  canRead: boolean
}

function categoryBadgeClass(category: string): string {
  switch (category) {
    case 'stock':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'backorder':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    case 'compliance':
      return 'bg-sky-500/20 text-sky-300 ring-sky-500/40'
    case 'procurement':
      return 'bg-violet-500/20 text-violet-300 ring-violet-500/40'
    case 'exception':
    case 'restriction':
      return 'bg-orange-500/20 text-orange-300 ring-orange-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

export function SupplyReadinessDashboardPanel({
  accessToken,
  canRead,
}: SupplyReadinessDashboardPanelProps) {
  const dashboardQuery = useQuery({
    queryKey: ['supplyarr-supply-readiness-dashboard', accessToken],
    queryFn: () => getSupplyReadinessDashboard(accessToken),
    enabled: canRead,
  })

  if (!canRead) {
    return null
  }

  const totals = dashboardQuery.data?.totals

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="supply-readiness-dashboard-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Supply readiness</h2>
      <p className="mt-1 text-sm text-slate-400">
        Tenant-scoped snapshot of stock position, open procurement, cross-product demand, and
        compliance attention.
      </p>

      {dashboardQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading supply readiness dashboard…</p>
      )}

      {dashboardQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load supply readiness dashboard.</p>
      )}

      {totals && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <MetricCard label="Available qty" value={totals.totalQuantityAvailable.toString()} />
            <MetricCard label="Below reorder" value={totals.partsBelowReorderCount.toString()} />
            <MetricCard label="Open backorders" value={totals.openBackorderCount.toString()} />
            <MetricCard
              label="Open PR / PO"
              value={`${totals.openPurchaseRequestCount} / ${totals.openPurchaseOrderCount}`}
            />
            <MetricCard label="Issued POs" value={totals.issuedPurchaseOrderCount.toString()} />
            <MetricCard label="Open demand refs" value={totals.openDemandRefCount.toString()} />
            <MetricCard
              label="Compliance attention"
              value={totals.complianceAttentionCount.toString()}
            />
            <MetricCard
              label="Restrictions / exceptions"
              value={`${totals.activeVendorRestrictionCount} / ${totals.activeProcurementExceptionCount}`}
            />
          </div>

          {dashboardQuery.data!.demandRefsBySource.length > 0 && (
            <div className="mt-4 flex flex-wrap gap-2 text-sm">
              {dashboardQuery.data!.demandRefsBySource.map((row) => (
                <span
                  key={row.source}
                  className="rounded-md bg-slate-800 px-3 py-1 text-slate-200"
                >
                  {row.source}: {row.openCount}
                </span>
              ))}
            </div>
          )}

          {dashboardQuery.data!.attentionItems.length === 0 ? (
            <p className="mt-4 text-sm text-slate-500">No attention items right now.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
              {dashboardQuery.data!.attentionItems.map((item, index) => (
                <li key={`${item.category}-${item.title}-${index}`} className="px-3 py-3">
                  <div className="flex flex-wrap items-center gap-2">
                    <span
                      className={`rounded px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${categoryBadgeClass(item.category)}`}
                    >
                      {item.category}
                    </span>
                    <span className="font-medium text-slate-100">{item.title}</span>
                  </div>
                  <p className="mt-1 text-slate-400">{item.detail}</p>
                </li>
              ))}
            </ul>
          )}
        </>
      )}
    </section>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2">
      <p className="text-xs uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-lg font-semibold text-slate-100">{value}</p>
    </div>
  )
}
