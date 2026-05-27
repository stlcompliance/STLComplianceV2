import { useQuery } from '@tanstack/react-query'

import { getDispatchBoard } from '../api/client'
import type { DispatchBoardResponse, DispatchBoardTripRow } from '../api/types'

type DispatchBoardPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  onScopeChange: (scope: 'daily' | 'weekly') => void
}

function SummaryCard({ label, value, hint }: { label: string; value: number; hint?: string }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-900/60 p-4">
      <p className="text-xs uppercase tracking-wide text-slate-500">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-slate-50">{value}</p>
      {hint ? <p className="mt-1 text-xs text-slate-400">{hint}</p> : null}
    </div>
  )
}

function formatTimestamp(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function TripRow({ trip }: { trip: DispatchBoardTripRow }) {
  const highlightClass = trip.isLate
    ? 'border-red-500/60 bg-red-950/30'
    : trip.isAtRisk
      ? 'border-amber-500/60 bg-amber-950/20'
      : 'border-slate-700 bg-slate-900/40'

  return (
    <li className={`rounded-lg border p-3 ${highlightClass}`}>
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-100">{trip.title}</p>
          <p className="text-xs text-slate-500">
            {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
          </p>
        </div>
        <div className="text-right text-xs">
          {trip.isLate ? <span className="font-medium text-red-300">Late</span> : null}
          {!trip.isLate && trip.isAtRisk ? (
            <span className="font-medium text-amber-300">At risk</span>
          ) : null}
          {!trip.isLate && !trip.isAtRisk ? (
            <span className="text-slate-500">On track</span>
          ) : null}
        </div>
      </div>
      <p className="mt-2 text-xs text-slate-400">
        Start {formatTimestamp(trip.scheduledStartAt)} · End {formatTimestamp(trip.scheduledEndAt)}
      </p>
      <p className="mt-1 text-xs text-slate-500">
        {trip.routeCount} route(s) · {trip.pendingStopCount} pending stop(s)
        {trip.assignedDriverPersonId ? ` · driver ${trip.assignedDriverPersonId.slice(0, 8)}…` : ' · unassigned'}
      </p>
    </li>
  )
}

export function DispatchBoardPanel({ accessToken, scope, onScopeChange }: DispatchBoardPanelProps) {
  const boardQuery = useQuery({
    queryKey: ['routarr-dispatch-board', accessToken, scope],
    queryFn: () => getDispatchBoard(accessToken, scope),
  })

  if (boardQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading dispatch board…</p>
  }

  if (boardQuery.isError) {
    return (
      <p className="text-sm text-red-300">
        Failed to load dispatch board: {(boardQuery.error as Error).message}
      </p>
    )
  }

  const board = boardQuery.data as DispatchBoardResponse

  return (
    <section className="space-y-6" aria-label="Dispatch board">
      <div className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Dispatch board</h2>
          <p className="mt-1 text-sm text-slate-400">
            {scope === 'daily' ? 'Daily' : 'Weekly'} view · {formatTimestamp(board.windowStart)} –{' '}
            {formatTimestamp(board.windowEnd)} · updated {formatTimestamp(board.generatedAt)}
          </p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            className={`rounded px-3 py-1.5 text-sm ${
              scope === 'daily'
                ? 'bg-sky-600 text-white'
                : 'border border-slate-600 text-slate-300 hover:bg-slate-800'
            }`}
            onClick={() => onScopeChange('daily')}
          >
            Daily
          </button>
          <button
            type="button"
            className={`rounded px-3 py-1.5 text-sm ${
              scope === 'weekly'
                ? 'bg-sky-600 text-white'
                : 'border border-slate-600 text-slate-300 hover:bg-slate-800'
            }`}
            onClick={() => onScopeChange('weekly')}
          >
            Weekly
          </button>
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Work queue</h3>
        <div className="grid gap-3 sm:grid-cols-3">
          <SummaryCard
            label="Unassigned trips"
            value={board.workQueue.unassignedDriverTripCount}
            hint="Active trips without a driver"
          />
          <SummaryCard
            label="Unlinked routes"
            value={board.workQueue.unlinkedRouteCount}
            hint="Editable routes not linked to a trip"
          />
          <SummaryCard
            label="Pending stops"
            value={board.workQueue.pendingStopCount}
            hint="Stops awaiting arrival"
          />
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Trips by status</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <SummaryCard label="Planned" value={board.trips.plannedCount} />
          <SummaryCard label="Assigned" value={board.trips.assignedCount} />
          <SummaryCard label="Dispatched" value={board.trips.dispatchedCount} />
          <SummaryCard label="In progress" value={board.trips.inProgressCount} />
          <SummaryCard label="Completed" value={board.trips.completedCount} />
          <SummaryCard label="Cancelled" value={board.trips.cancelledCount} />
          <SummaryCard label="Late" value={board.trips.lateCount} hint="Past schedule, not completed" />
          <SummaryCard label="At risk" value={board.trips.atRiskCount} hint="Due within 2 hours" />
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-slate-300">Routes & stops</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <SummaryCard label="Route draft" value={board.routes.draftCount} />
          <SummaryCard label="Route planned" value={board.routes.plannedCount} />
          <SummaryCard label="Route active" value={board.routes.activeCount} />
          <SummaryCard label="Stop pending" value={board.stops.pendingCount} />
          <SummaryCard label="Stop completed" value={board.stops.completedCount} />
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
          <h3 className="text-sm font-medium text-slate-300">Active trips</h3>
          {board.activeTrips.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">No dispatched or in-progress trips in this window.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {board.activeTrips.map((trip) => (
                <TripRow key={trip.tripId} trip={trip} />
              ))}
            </ul>
          )}
        </div>

        <div className="rounded-xl border border-slate-700 bg-slate-900/80 p-5">
          <h3 className="text-sm font-medium text-slate-300">Assigned trips</h3>
          {board.assignedTrips.length === 0 ? (
            <p className="mt-3 text-sm text-slate-500">No assigned trips in this window.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {board.assignedTrips.map((trip) => (
                <TripRow key={`assigned-${trip.tripId}`} trip={trip} />
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  )
}
