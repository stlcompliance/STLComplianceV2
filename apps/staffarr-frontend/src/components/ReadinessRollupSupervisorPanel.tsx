import type { ReadinessRollupSummaryResponse } from '../api/types'

interface ReadinessRollupSupervisorPanelProps {
  teamRollups: ReadinessRollupSummaryResponse[]
  siteRollups: ReadinessRollupSummaryResponse[]
  isLoading: boolean
  errorMessage: string | null
}

function readinessBarClass(readyPercent: number): string {
  if (readyPercent >= 90) {
    return 'bg-emerald-500'
  }

  if (readyPercent >= 70) {
    return 'bg-amber-400'
  }

  return 'bg-rose-500'
}

function RollupTable({
  title,
  rollups,
}: {
  title: string
  rollups: ReadinessRollupSummaryResponse[]
}) {
  if (rollups.length === 0) {
    return (
      <div>
        <h3 className="text-sm font-medium text-slate-300">{title}</h3>
        <p className="mt-2 text-sm text-slate-500">No rollups computed yet. The scheduled worker refreshes summaries periodically.</p>
      </div>
    )
  }

  return (
    <div>
      <h3 className="text-sm font-medium text-slate-300">{title}</h3>
      <div className="mt-3 overflow-x-auto">
        <table className="min-w-full text-left text-sm">
          <thead className="text-xs uppercase tracking-wide text-slate-500">
            <tr>
              <th className="pb-2 pr-4 font-medium">Unit</th>
              <th className="pb-2 pr-4 font-medium">Members</th>
              <th className="pb-2 pr-4 font-medium">Ready</th>
              <th className="pb-2 pr-4 font-medium">Not ready</th>
              <th className="pb-2 pr-4 font-medium">Overrides</th>
              <th className="pb-2 font-medium">Ready %</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800">
            {rollups.map((rollup) => (
              <tr key={`${rollup.scopeType}-${rollup.orgUnitId}`}>
                <td className="py-3 pr-4 text-white">{rollup.orgUnitName}</td>
                <td className="py-3 pr-4 text-slate-300">{rollup.totalMembers}</td>
                <td className="py-3 pr-4 text-emerald-300">{rollup.readyCount}</td>
                <td className="py-3 pr-4 text-rose-300">{rollup.notReadyCount}</td>
                <td className="py-3 pr-4 text-amber-200">{rollup.overrideCount}</td>
                <td className="py-3">
                  <div className="flex min-w-[8rem] items-center gap-2">
                    <div className="h-2 flex-1 rounded-full bg-slate-800">
                      <div
                        className={`h-2 rounded-full ${readinessBarClass(rollup.readyPercent)}`}
                        style={{ width: `${Math.min(rollup.readyPercent, 100)}%` }}
                      />
                    </div>
                    <span className="w-12 text-right text-slate-300">{rollup.readyPercent.toFixed(1)}%</span>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}

export function canViewReadinessRollups(tenantRoleKey: string, isPlatformAdmin: boolean): boolean {
  if (isPlatformAdmin) {
    return true
  }

  return ['tenant_admin', 'staffarr_admin', 'hr_admin', 'supervisor'].includes(tenantRoleKey)
}

export function ReadinessRollupSupervisorPanel({
  teamRollups,
  siteRollups,
  isLoading,
  errorMessage,
}: ReadinessRollupSupervisorPanelProps) {
  return (
    <section className="mt-8 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-medium text-slate-300">Team and site readiness rollups</h2>
          <p className="mt-1 text-sm text-slate-500">
            Materialized summaries refreshed by the readiness rollup worker for supervisor oversight.
          </p>
        </div>
      </div>

      {errorMessage ? <p className="mt-4 text-sm text-red-300">{errorMessage}</p> : null}
      {isLoading ? <p className="mt-4 text-sm text-slate-400">Loading readiness rollups…</p> : null}

      {!isLoading && !errorMessage ? (
        <div className="mt-6 grid gap-8 lg:grid-cols-2">
          <RollupTable title="Teams" rollups={teamRollups} />
          <RollupTable title="Sites" rollups={siteRollups} />
        </div>
      ) : null}
    </section>
  )
}
