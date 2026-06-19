import { Link } from 'react-router-dom'
import { PmDuePanel } from '../../components/PmDuePanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

  const openWorkOrderStatuses = new Set([
  'requested',
  'triage',
  'approved',
  'planned',
  'waiting_parts',
  'waiting_labor',
  'waiting_vendor',
  'waiting_approval',
  'waiting_compliance',
  'scheduled',
  'assigned',
  'in_progress',
  'paused',
  'blocked',
  'completed_pending_review',
])

const highPriorityWorkOrderLevels = new Set(['urgent', 'high', 'critical'])

function isOpenWorkOrderStatus(status: string): boolean {
  return openWorkOrderStatuses.has(status)
}

function formatDateShort(value: string): string {
  return new Date(value).toLocaleDateString()
}

function statusTone(status: string): string {
  if (status === 'overdue') {
    return 'danger'
  }
  if (status === 'due') {
    return 'warning'
  }
  return 'info'
}

function priorityClass(priority: string): string {
  if (priority === 'critical' || priority === 'urgent') {
    return 'text-red-300'
  }
  if (priority === 'high') {
    return 'text-amber-300'
  }
  return 'text-slate-300'
}

export function OverviewSection({ state }: Props) {
  const assets = state.assetsQuery.data ?? []
  const readinessFleet = state.assetReadinessFleetQuery.data ?? []
  const duePmSchedules = state.duePmQuery.data ?? []
  const workOrders = state.workOrdersQuery.data ?? []
  const defects = state.defectsQuery.data ?? []

  const loading = state.duePmQuery.isLoading || state.assetsQuery.isLoading || state.workOrdersQuery.isLoading || state.defectsQuery.isLoading

  const openWorkOrders = workOrders.filter((item) => isOpenWorkOrderStatus(item.status))
  const waitForPartsWorkOrders = openWorkOrders.filter((item) => item.status === 'waiting_parts')
  const urgentWorkOrders = openWorkOrders.filter((item) => highPriorityWorkOrderLevels.has(item.priority))

  const openDefects = defects.filter((item) =>
    item.status === 'open' || item.status === 'acknowledged' || item.status === 'in_repair',
  )
  const criticalDefects = openDefects.filter((item) => item.severity === 'critical')
  const highDefects = openDefects.filter((item) => item.severity === 'high')

  const readinessReadyCount = readinessFleet.filter((item) => item.readinessStatus === 'ready').length
  const readinessNotReadyCount = readinessFleet.filter((item) => item.readinessStatus !== 'ready').length
  const unknownReadinessCount = Math.max(assets.length - readinessFleet.length, 0)
  const readyPercent = readinessFleet.length > 0 ? Math.round((readinessReadyCount / readinessFleet.length) * 100) : 0

  const duePmCount = duePmSchedules.filter((item) => item.dueStatus === 'due').length
  const overduePmCount = duePmSchedules.filter((item) => item.dueStatus === 'overdue').length
  const blockers = readinessFleet.filter((item) => item.readinessStatus !== 'ready' && item.blockerCount > 0)
  const blockersByAge = [...blockers].sort((a, b) => b.blockerCount - a.blockerCount)

  const executionQueue = [...openWorkOrders]
    .sort((a, b) => {
      const aPriority = highPriorityWorkOrderLevels.has(a.priority) ? 10 : 5
      const bPriority = highPriorityWorkOrderLevels.has(b.priority) ? 10 : 5
      if (aPriority !== bPriority) {
        return bPriority - aPriority
      }
      return new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime()
    })
    .slice(0, 8)

  return (
    <div className="space-y-6">
      <section className="rounded-xl border border-slate-700 bg-slate-900/70 p-5">
        <div className="mb-3 flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-white">Maintenance readiness</h2>
            <p className="mt-1 text-sm text-slate-400">Asset readiness, execution queue, PM risk, and defect pressure.</p>
          </div>
          <div className="flex flex-wrap gap-2">
            {state.canCreateWorkOrder ? <Link className="rounded-md bg-sky-700 px-3 py-2 text-sm font-medium text-white hover:bg-sky-600" to="/work-orders/create">Create work order</Link> : null}
            <Link className="rounded-md bg-slate-700 px-3 py-2 text-sm font-medium text-white hover:bg-slate-600" to="/work-orders">Open work orders</Link>
            <Link className="rounded-md bg-slate-700 px-3 py-2 text-sm font-medium text-white hover:bg-slate-600" to="/defects">Defects</Link>
          </div>
        </div>

        {loading ? (
          <p className="text-sm text-slate-400">Loading maintenance command view…</p>
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <KpiCard label="Assets tracked" value={String(assets.length)} />
            <KpiCard label="Asset readiness" value={`${readyPercent}%`} note={`${readinessReadyCount} ready, ${readinessNotReadyCount} not ready`}/>
            <KpiCard label="Open work orders" value={String(openWorkOrders.length)} note={`${waitForPartsWorkOrders.length} waiting on parts`} />
            <KpiCard
              label="Open defects"
              value={String(openDefects.length)}
              note={`Critical ${criticalDefects.length}, high ${highDefects.length}`}
            />
            <KpiCard label="PM overdue" value={String(overduePmCount)} note={`${duePmCount} due`} />
            <KpiCard label="Urgent execution" value={String(urgentWorkOrders.length)} />
          </div>
        )}
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
          <div className="mb-4 flex items-start justify-between gap-3">
            <div>
              <h2 className="text-lg font-semibold text-white">Execution queue</h2>
              <p className="mt-1 text-sm text-slate-400">Work orders needing execution, review, or supply follow-up.</p>
            </div>
            <Link to="/work-orders" className="text-sm text-sky-300 hover:text-sky-200">View all</Link>
          </div>

          {state.workOrdersQuery.isLoading ? (
            <p className="text-sm text-slate-400">Loading open work orders…</p>
          ) : executionQueue.length === 0 ? (
            <p className="text-sm text-slate-400">No open work orders at this moment.</p>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full text-left text-sm">
                <thead className="border-b border-slate-700 text-slate-400">
                  <tr>
                    <th className="px-3 py-2 font-medium">Number</th>
                    <th className="px-3 py-2 font-medium">Asset</th>
                    <th className="px-3 py-2 font-medium">Title</th>
                    <th className="px-3 py-2 font-medium">Priority</th>
                    <th className="px-3 py-2 font-medium">Status</th>
                    <th className="px-3 py-2 font-medium">Updated</th>
                    <th className="px-3 py-2 font-medium">Open</th>
                  </tr>
                </thead>
                <tbody>
                  {executionQueue.map((workOrder) => (
                    <tr key={workOrder.workOrderId} className="border-b border-slate-800 text-slate-200">
                      <td className="px-3 py-2 font-mono text-xs text-slate-300">{workOrder.workOrderNumber}</td>
                      <td className="px-3 py-2">
                        <div>{workOrder.assetTag}</div>
                        <div className="text-xs text-slate-400">{workOrder.assetName}</div>
                      </td>
                      <td className="px-3 py-2">{workOrder.title}</td>
                      <td className={`px-3 py-2 ${priorityClass(workOrder.priority)}`}>{workOrder.priority}</td>
                      <td className="px-3 py-2">{workOrder.status}</td>
                      <td className="px-3 py-2 text-slate-300">{formatDateShort(workOrder.updatedAt)}</td>
                      <td className="px-3 py-2">
                        <Link className="text-sky-300 hover:text-sky-200" to={`/work-orders/${workOrder.workOrderId}`}>
                          Open
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>

        <div className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
          <div className="mb-4 flex items-start justify-between gap-3">
            <div>
              <h2 className="text-lg font-semibold text-white">PM due now</h2>
              <p className="mt-1 text-sm text-slate-400">Schedules marked due or overdue by Preventive Maintenance scan.</p>
            </div>
            <Link to="/pm-programs" className="text-sm text-sky-300 hover:text-sky-200">PM programs</Link>
          </div>
          <PmDuePanel dueSchedules={duePmSchedules} isLoading={state.duePmQuery.isLoading} />
        </div>
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
        <div className="mb-4">
          <h2 className="text-lg font-semibold text-white">Attention and readiness</h2>
          <p className="mt-1 text-sm text-slate-400">Highest-priority readiness blockers and risk signals.</p>
        </div>

        <div className="grid gap-3 lg:grid-cols-2">
          <div className="rounded-lg border border-slate-800 bg-slate-950/40 p-3">
            <h3 className="text-sm font-medium text-slate-100">Readiness summary</h3>
            <ul className="mt-2 space-y-2 text-sm text-slate-300">
              <li>Ready assets: {readinessReadyCount}</li>
              <li>Not ready assets: {readinessNotReadyCount}</li>
              {unknownReadinessCount ? <li>Not yet computed: {unknownReadinessCount}</li> : null}
              <li>PM overdue: {overduePmCount}</li>
              <li>Work orders blocked on parts: {waitForPartsWorkOrders.length}</li>
            </ul>
          </div>

          <div className="rounded-lg border border-slate-800 bg-slate-950/40 p-3">
            <h3 className="text-sm font-medium text-slate-100">Top maintenance blockers</h3>
            {state.assetReadinessFleetQuery.isLoading ? (
              <p className="mt-2 text-sm text-slate-400">Loading readiness blockers…</p>
            ) : blockersByAge.length === 0 ? (
              <p className="mt-2 text-sm text-emerald-300">No blockers currently blocking readiness.</p>
            ) : (
              <ul className="mt-2 space-y-2 text-sm text-slate-300">
                {blockersByAge.slice(0, 6).map((asset) => (
                  <li key={asset.assetId} className="rounded border border-slate-700 bg-slate-900/60 p-2">
                    <div className="flex items-center justify-between gap-2">
                      <p className="font-medium text-slate-100">{asset.assetTag}</p>
                      <span
                        className="stl-tone-badge rounded border px-2 py-0.5 text-xs"
                        data-tone={asset.blockerCount > 1 ? 'danger' : statusTone('due')}
                      >
                        {asset.blockerCount} blocker{asset.blockerCount === 1 ? '' : 's'}
                      </span>
                    </div>
                    <p className="mt-1 text-slate-400">{asset.primaryBlockerMessage ?? 'Readiness blocker details pending.'}</p>
                    <Link className="mt-1 inline-block text-xs text-sky-300 hover:text-sky-200" to={`/assets/${asset.assetId}`}>
                      View asset
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-950/40 p-4 text-xs text-slate-400">
        <p>
          Dashboard scope: MaintainArr owns maintenance assets, defects, PMs, inspections, work orders, and labor/parts demand records.
          Readiness blockers are derived from MaintainArr readiness signals and read as execution risk for maintenance actions.
        </p>
      </section>
    </div>
  )
}

function KpiCard({ label, value, note }: { label: string; value: string; note?: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="mt-1 text-xl font-semibold text-slate-100">{value}</p>
      {note ? <p className="mt-1 text-sm text-[var(--color-text-muted)]">{note}</p> : null}
    </div>
  )
}
