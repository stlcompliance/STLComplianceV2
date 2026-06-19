import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

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
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading supply readiness dashboard…</p>
      )}

      {dashboardQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Supply readiness unavailable"
            message={getErrorMessage(dashboardQuery.error, 'Failed to load supply readiness dashboard.')}
            retryLabel="Retry dashboard"
            onRetry={() => {
              void dashboardQuery.refetch()
            }}
          />
        </div>
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
            <p className="mt-4 text-sm text-[var(--color-text-muted)]">No attention items right now.</p>
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

          {dashboardQuery.data!.predictiveStockoutItems.length > 0 ? (
            <div className="mt-6 rounded-lg border border-rose-900/60 bg-rose-950/20 p-4">
              <div className="flex flex-wrap items-start justify-between gap-3">
                <div>
                  <h3 className="text-sm font-semibold uppercase tracking-wide text-rose-200">
                    Predictive stockout risk
                  </h3>
                  <p className="mt-1 text-sm text-slate-300">
                    Projected using open demand refs, open backorders, and current inventory.
                  </p>
                </div>
                <span className="rounded-full bg-rose-500/20 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-rose-200">
                  {dashboardQuery.data!.predictiveStockoutItems.length} at risk
                </span>
              </div>

              <div className="mt-4 grid gap-3 sm:grid-cols-2">
                {dashboardQuery.data!.predictiveStockoutItems.map((item) => (
                  <div key={item.partId} className="rounded-md border border-rose-900/50 bg-slate-950/60 p-3">
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">
                          {item.partKey} · {item.displayName}
                        </div>
                        <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {item.riskLevel} · reorder {item.reorderPoint ?? 'n/a'}
                        </div>
                      </div>
                      <span className="rounded bg-rose-500/20 px-2 py-0.5 text-xs font-semibold uppercase tracking-wide text-rose-200">
                        {item.shortageQuantity > 0 ? 'Shortage' : 'Watch'}
                      </span>
                    </div>
                    <p className="mt-2 text-sm text-slate-300">{item.reason}</p>
                    <div className="mt-3 grid gap-2 text-xs text-slate-400 sm:grid-cols-2">
                      <MetricChip label="Available" value={item.quantityAvailable.toString()} />
                      <MetricChip label="Open demand" value={item.openDemandQuantity.toString()} />
                      <MetricChip label="Open backorders" value={item.openBackorderQuantity.toString()} />
                      <MetricChip label="Projected" value={item.projectedQuantity.toString()} />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <p className="mt-6 text-sm text-emerald-300">No predictive stockout risks detected.</p>
          )}
        </>
      )}
    </section>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-1 text-lg font-semibold text-slate-100">{value}</p>
    </div>
  )
}

function MetricChip({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-800 bg-slate-900/80 px-2 py-1">
      <div className="uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
      <div className="mt-0.5 font-semibold text-slate-100">{value}</div>
    </div>
  )
}
