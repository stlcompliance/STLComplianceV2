import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { getDispatchCommandCenter } from '../../api/client'
import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canAssignDrivers } from '../../auth/sessionStorage'
import { ActiveTripsPanel } from '../../components/ActiveTripsPanel'
import { DispatchBoardPanel } from '../../components/DispatchBoardPanel'
import { DispatchCommandCenterPanel } from '../../components/DispatchCommandCenterPanel'
import { DispatchExceptionQueuePanel } from '../../components/DispatchExceptionQueuePanel'
import { RouteCalendarPanel } from '../../components/RouteCalendarPanel'
import { UnassignedWorkQueuePanel } from '../../components/UnassignedWorkQueuePanel'

type Props = { state: RoutArrWorkspaceState }

function KpiCard({ label, value, note }: { label: string; value: string; note: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/60 px-4 py-3">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-slate-100">{value}</p>
      <p className="mt-1 text-xs text-slate-400">{note}</p>
    </div>
  )
}

export function DashboardSection({ state }: Props) {
  const { session, boardScope, setBoardScope, roleKey, isPlatformAdmin } = state
  const canAssign = canAssignDrivers(roleKey, isPlatformAdmin)
  const commandCenterQuery = useQuery({
    queryKey: ['routarr-command-center-summary', session.accessToken, boardScope],
    queryFn: () => getDispatchCommandCenter(session.accessToken, boardScope),
  })
  const commandCenter = commandCenterQuery.data ?? null
  const board = commandCenter?.board ?? null
  const generatedAt = commandCenter?.generatedAt ?? null

  return (
    <div className="space-y-6">
      <section className="rounded-xl border border-slate-700 bg-slate-900/70 p-5 shadow-sm shadow-black/20">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-sky-300">
              Dispatch dashboard
            </p>
            <h2 className="mt-1 text-xl font-semibold text-slate-50">Current operating picture</h2>
            <p className="mt-1 max-w-3xl text-sm text-slate-400">
              Live dispatch health, blockers, execution queues, and route timing for the selected
              {boardScope === 'daily' ? ' day' : ' week'}.
            </p>
          </div>
          <div className="rounded-md border border-slate-700 bg-slate-950/60 px-3 py-2 text-xs text-slate-300">
            <p className="font-medium text-slate-100">Scope</p>
            <p>{boardScope === 'daily' ? 'Daily dispatch window' : 'Weekly dispatch window'}</p>
            <p className="mt-1 text-[var(--color-text-muted)]">
              {generatedAt ? `Refreshed ${new Date(generatedAt).toLocaleString()}` : 'Refreshing live'}
            </p>
          </div>
        </div>

        {commandCenterQuery.isError ? (
          <div className="mt-4">
            <ApiErrorCallout
              title="Dispatch summary unavailable"
              message={getErrorMessage(
                commandCenterQuery.error,
                'Unable to load the dashboard summary.',
              )}
              retryLabel="Retry summary"
              onRetry={() => {
                void commandCenterQuery.refetch()
              }}
            />
          </div>
        ) : null}

        {board ? (
          <div className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-5">
            <KpiCard
              label="Active trips"
              value={String(board.trips.totalCount)}
              note={`${board.trips.dispatchedCount + board.trips.inProgressCount} executing now`}
            />
            <KpiCard
              label="Late trips"
              value={String(board.trips.lateCount)}
              note="Need immediate attention"
            />
            <KpiCard
              label="At risk"
              value={String(board.trips.atRiskCount)}
              note="Window or proof risk"
            />
            <KpiCard
              label="Unassigned"
              value={String(board.workQueue.unassignedDriverTripCount)}
              note="Driver assignment needed"
            />
            <KpiCard
              label="Missing proof"
              value={String(board.workQueue.missingProofTripCount)}
              note="Closeout blocker"
            />
          </div>
        ) : (
          <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading dispatch summary…</p>
        )}

        {board ? (
          <div className="mt-5 grid gap-3 lg:grid-cols-3">
            <div className="rounded-lg border border-amber-800/40 bg-amber-950/20 p-4">
              <h3 className="text-sm font-semibold text-amber-200">Attention</h3>
              <ul className="mt-2 space-y-2 text-sm text-slate-200">
                {board.trips.lateCount > 0 ? (
                  <li>{board.trips.lateCount} late trip(s) require review.</li>
                ) : null}
                {board.workQueue.unassignedDriverTripCount > 0 ? (
                  <li>{board.workQueue.unassignedDriverTripCount} trip(s) still need drivers.</li>
                ) : null}
                {board.workQueue.missingProofTripCount > 0 ? (
                  <li>{board.workQueue.missingProofTripCount} trip(s) are missing proof.</li>
                ) : null}
                {board.workQueue.unlinkedRouteCount > 0 ? (
                  <li>{board.workQueue.unlinkedRouteCount} route(s) are not linked to trips.</li>
                ) : null}
                {board.trips.lateCount === 0 &&
                board.workQueue.unassignedDriverTripCount === 0 &&
                board.workQueue.missingProofTripCount === 0 &&
                board.workQueue.unlinkedRouteCount === 0 ? (
                  <li>No immediate dispatch blockers.</li>
                ) : null}
              </ul>
            </div>
            <div className="rounded-lg border border-sky-800/40 bg-sky-950/20 p-4">
              <h3 className="text-sm font-semibold text-sky-200">Primary view</h3>
              <p className="mt-2 text-sm text-slate-200">
                Command center, active trips, unassigned work, exceptions, board, and route calendar
                are stacked below for drilling into the live operating picture.
              </p>
            </div>
            <div className="rounded-lg border border-emerald-800/40 bg-emerald-950/20 p-4">
              <h3 className="text-sm font-semibold text-emerald-200">Dashboard scope</h3>
              <p className="mt-2 text-sm text-slate-200">
                RoutArr owns dispatch, route planning, trip execution, assignment, stop management,
                ETA, and exceptions. Readiness signals from MaintainArr and StaffArr are surfaced as
                references only.
              </p>
            </div>
          </div>
        ) : null}
      </section>

      <DispatchCommandCenterPanel
        accessToken={session.accessToken}
        scope={boardScope}
        onScopeChange={setBoardScope}
        canAssign={canAssign}
      />

      <div className="mt-8">
        <ActiveTripsPanel accessToken={session.accessToken} scope={boardScope} />
      </div>

      <div className="mt-8">
        <UnassignedWorkQueuePanel
          accessToken={session.accessToken}
          scope={boardScope}
          canAssign={canAssign}
        />
      </div>

      <div className="mt-8">
        <DispatchExceptionQueuePanel
          accessToken={session.accessToken}
          userId={session.userId}
          canTriage={canAssign}
        />
      </div>

      <div className="mt-8">
        <DispatchBoardPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
        />
      </div>

      <div className="mt-8">
        <RouteCalendarPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
        />
      </div>
    </div>
  )
}
