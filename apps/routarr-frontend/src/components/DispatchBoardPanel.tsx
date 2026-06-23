import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getDispatchBoard } from '../api/client'
import type { DispatchBoardResponse, DispatchBoardTripRow } from '../api/types'

type DispatchBoardPanelProps = {
  accessToken: string
  scope: 'daily' | 'weekly'
  onScopeChange: (scope: 'daily' | 'weekly') => void
}

function SummaryCard({ label, value, hint }: { label: string; value: number; hint?: string }) {
  return (
    <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-[var(--color-text-primary)]">{value}</p>
      {hint ? <p className="mt-1 text-xs text-[var(--color-text-muted)]">{hint}</p> : null}
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
    ? 'border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)]'
    : trip.isAtRisk
      ? 'border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)]'
      : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)]'

  return (
    <li className={`rounded-lg border p-3 ${highlightClass}`}>
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-[var(--color-text-primary)]">{trip.title}</p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
          </p>
        </div>
        <div className="text-right text-xs">
          {trip.isLate ? <span className="font-medium text-[var(--tone-danger-text)]">Late</span> : null}
          {!trip.isLate && trip.isAtRisk ? (
            <span className="font-medium text-[var(--tone-warning-text)]">At risk</span>
          ) : null}
          {!trip.isLate && !trip.isAtRisk ? (
            <span className="text-[var(--color-text-muted)]">On track</span>
          ) : null}
        </div>
      </div>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">
        Start {formatTimestamp(trip.scheduledStartAt)} · End {formatTimestamp(trip.scheduledEndAt)}
      </p>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        {trip.routeCount} route(s) · {trip.pendingStopCount} pending stop(s)
        {trip.missingRequiredProofCount > 0
          ? ` · ${trip.missingRequiredProofCount} missing proof`
          : ''}
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
    return <p className="text-sm text-[var(--color-text-muted)]">Loading dispatch board…</p>
  }

  if (boardQuery.isError) {
    return (
      <ApiErrorCallout
        title="Dispatch board unavailable"
        message={getErrorMessage(boardQuery.error, 'Failed to load dispatch board.')}
        retryLabel="Retry board"
        onRetry={() => {
          void boardQuery.refetch()
        }}
      />
    )
  }

  const board = boardQuery.data as DispatchBoardResponse

  return (
    <section className="space-y-6" aria-label="Dispatch board">
      <div className="flex flex-wrap items-center justify-between gap-4 rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
        <div>
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Dispatch board</h2>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
            {scope === 'daily' ? 'Daily' : 'Weekly'} view · {formatTimestamp(board.windowStart)} –{' '}
            {formatTimestamp(board.windowEnd)} · updated {formatTimestamp(board.generatedAt)}
          </p>
        </div>
        <div className="flex gap-2">
          <button
            type="button"
            className={`rounded px-3 py-1.5 text-sm ${
              scope === 'daily'
                ? 'bg-[var(--color-accent)] text-[var(--color-on-accent)]'
                : 'border border-[var(--color-border-default)] text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]'
            }`}
            onClick={() => onScopeChange('daily')}
          >
            Daily
          </button>
          <button
            type="button"
            className={`rounded px-3 py-1.5 text-sm ${
              scope === 'weekly'
                ? 'bg-[var(--color-accent)] text-[var(--color-on-accent)]'
                : 'border border-[var(--color-border-default)] text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)]'
            }`}
            onClick={() => onScopeChange('weekly')}
          >
            Weekly
          </button>
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-[var(--color-text-secondary)]">Work queue</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
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
          <SummaryCard
            label="Missing proof"
            value={board.workQueue.missingProofTripCount}
            hint="Trips missing required proof"
          />
        </div>
      </div>

      <div>
        <h3 className="mb-3 text-sm font-medium text-[var(--color-text-secondary)]">Trips by status</h3>
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
        <h3 className="mb-3 text-sm font-medium text-[var(--color-text-secondary)]">Routes & stops</h3>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <SummaryCard label="Route draft" value={board.routes.draftCount} />
          <SummaryCard label="Route planned" value={board.routes.plannedCount} />
          <SummaryCard label="Route active" value={board.routes.activeCount} />
          <SummaryCard label="Stop pending" value={board.stops.pendingCount} />
          <SummaryCard label="Stop completed" value={board.stops.completedCount} />
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
          <h3 className="text-sm font-medium text-[var(--color-text-secondary)]">Active trips</h3>
          {board.activeTrips.length === 0 ? (
            <p className="mt-3 text-sm text-[var(--color-text-muted)]">No dispatched or in-progress trips in this window.</p>
          ) : (
            <ul className="mt-3 space-y-2">
              {board.activeTrips.map((trip) => (
                <TripRow key={trip.tripId} trip={trip} />
              ))}
            </ul>
          )}
        </div>

        <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
          <h3 className="text-sm font-medium text-[var(--color-text-secondary)]">Assigned trips</h3>
          {board.assignedTrips.length === 0 ? (
            <p className="mt-3 text-sm text-[var(--color-text-muted)]">No assigned trips in this window.</p>
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
