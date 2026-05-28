import { useMutation, useQuery } from '@tanstack/react-query'
import { exportExecutiveReportSummaryCsv, getExecutiveReportSummary } from '../api/client'

interface ExecutiveReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

export function ExecutiveReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ExecutiveReportsPanelProps) {
  const summaryQuery = useQuery({
    queryKey: ['maintainarr-executive-report-summary', accessToken],
    queryFn: () => getExecutiveReportSummary(accessToken),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () => exportExecutiveReportSummaryCsv(accessToken),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `maintainarr-executive-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  const fleet = summaryQuery.data?.fleetReadiness
  const ops = summaryQuery.data?.operationalTotals
  const supply = summaryQuery.data?.supplyDemand

  return (
    <section
      className="mt-6 rounded-xl border border-violet-800/40 bg-violet-950/20 p-5"
      data-testid="executive-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Executive summary</h2>
          <p className="mt-1 text-sm text-slate-400">
            Fleet readiness, operational KPIs, and SupplyArr parts-demand rollups (read-only local
            refs).
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading executive summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load executive summary.</p>
      )}

      {summaryQuery.data && fleet && ops && supply && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Fleet ready %" value={`${fleet.readyPercent.toFixed(1)}%`} />
            <MetricCard
              label="Ready / not ready"
              value={`${fleet.readyCount} / ${fleet.notReadyCount}`}
            />
            <MetricCard label="Open work orders" value={String(ops.openWorkOrderCount)} />
            <MetricCard
              label="Critical / high defects"
              value={`${ops.openCriticalDefectCount} / ${ops.openHighDefectCount}`}
            />
            <MetricCard label="Overdue PM" value={String(ops.overduePmScheduleCount)} />
            <MetricCard label="Labor hours (30d)" value={ops.laborHoursLast30Days.toFixed(1)} />
            <MetricCard
              label="WO completed (30d)"
              value={String(ops.workOrdersCompletedLast30Days)}
            />
            <MetricCard
              label="SupplyArr open procurement"
              value={String(supply.openProcurementLines)}
            />
          </div>

          <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/50 p-3 text-sm text-slate-300">
            <p>
              <span className="text-slate-500">SupplyArr demand lines:</span>{' '}
              {supply.publishedDemandLines} published of {supply.totalDemandLines} total ·{' '}
              {supply.fulfilledLines} fulfilled
            </p>
            {fleet.computedAt ? (
              <p className="mt-1 text-xs text-slate-500">
                Fleet rollup computed {new Date(fleet.computedAt).toLocaleString()}
                {fleet.fromScopeRollup ? ' (scope worker)' : ' (asset rollups)'}
              </p>
            ) : null}
          </div>

          {summaryQuery.data.scopeReadiness.length > 0 ? (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead className="text-xs uppercase text-slate-500">
                  <tr>
                    <th className="px-2 py-2">Scope</th>
                    <th className="px-2 py-2">Assets</th>
                    <th className="px-2 py-2">Ready %</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.scopeReadiness.map((scope) => (
                    <tr key={`${scope.scopeType}-${scope.scopeEntityId}`} className="border-t border-slate-800">
                      <td className="px-2 py-2 text-slate-100">
                        {scope.scopeType}: {scope.scopeLabel}
                      </td>
                      <td className="px-2 py-2 text-slate-300">{scope.totalAssets}</td>
                      <td className="px-2 py-2 text-slate-300">{scope.readyPercent.toFixed(1)}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}
        </>
      )}
    </section>
  )
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/50 px-3 py-2">
      <p className="text-xs uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-lg font-semibold text-slate-100">{value}</p>
    </div>
  )
}
