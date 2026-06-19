import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getActiveTrips } from '../api/client'
import type { ActiveTripRow } from '../api/types'

type Props = {
  accessToken: string
  scope: 'daily' | 'weekly'
}

type StatusFilter = 'all' | 'dispatched' | 'in_progress'

function formatTimestamp(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function statusBadge(trip: ActiveTripRow) {
  if (trip.isLate) {
    return <span className="font-medium text-red-300">Late</span>
  }
  if (trip.isAtRisk) {
    return <span className="font-medium text-amber-300">At risk</span>
  }
  return <span className="text-[var(--color-text-muted)]">On track</span>
}

function StopProgressBar({ trip }: { trip: ActiveTripRow }) {
  if (trip.totalStopCount <= 0) {
    return <p className="mt-1 text-xs text-[var(--color-text-muted)]">No stops on route</p>
  }

  return (
    <div className="mt-2 space-y-1" data-testid={`active-trip-progress-${trip.tripId}`}>
      <div className="flex justify-between text-[10px] text-[var(--color-text-muted)]">
        <span>Stop progress</span>
        <span>
          {trip.completedStopCount}/{trip.totalStopCount} ({trip.stopProgressPercent}%)
        </span>
      </div>
      <div className="h-1.5 overflow-hidden rounded-full bg-slate-800">
        <div
          className="h-full rounded-full bg-emerald-500/80"
          style={{ width: `${trip.stopProgressPercent}%` }}
        />
      </div>
    </div>
  )
}

function TripListRow({ trip }: { trip: ActiveTripRow }) {
  const borderClass = trip.isLate
    ? 'border-red-500/60 bg-red-950/30'
    : trip.isAtRisk
      ? 'border-amber-500/60 bg-amber-950/20'
      : 'border-slate-700 bg-slate-900/40'

  const driverLabel = trip.assignedDriverDisplayName
    ?? (trip.assignedDriverPersonId ? trip.assignedDriverPersonId.slice(0, 8) + '…' : null)

  return (
    <li
      className={`rounded-lg border p-3 ${borderClass}`}
      data-testid={`active-trip-row-${trip.tripId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-100">{trip.title}</p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2 text-xs">
          {statusBadge(trip)}
          {trip.openExceptionCount > 0 ? (
            <span
              className="rounded bg-orange-900/60 px-1.5 py-0.5 text-orange-200"
              data-testid={`active-trip-exceptions-${trip.tripId}`}
            >
              {trip.openExceptionCount} exception{trip.openExceptionCount === 1 ? '' : 's'}
            </span>
          ) : null}
        </div>
      </div>
      <p className="mt-2 text-xs text-slate-400">
        Start {formatTimestamp(trip.scheduledStartAt)} · End {formatTimestamp(trip.scheduledEndAt)}
      </p>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        {trip.routeCount} route(s) · {trip.pendingStopCount} pending stop(s)
        {trip.vehicleRefKey ? ` · ${trip.vehicleRefKey}` : ''}
        {driverLabel ? ` · driver ${driverLabel}` : ' · unassigned'}
      </p>
      <StopProgressBar trip={trip} />
      {trip.startedAt ? (
        <p className="mt-1 text-xs text-emerald-400/80">Started {formatTimestamp(trip.startedAt)}</p>
      ) : trip.dispatchedAt ? (
        <p className="mt-1 text-xs text-sky-400/80">
          Dispatched {formatTimestamp(trip.dispatchedAt)}
        </p>
      ) : null}
      <Link
        to={`/trips/${trip.tripId}`}
        className="mt-2 inline-block text-xs text-teal-300 hover:text-teal-200"
        data-testid={`active-trip-workspace-link-${trip.tripId}`}
      >
        Open execution workspace →
      </Link>
    </li>
  )
}

function TripMapStrip({ trips, windowStart, windowEnd }: {
  trips: ActiveTripRow[]
  windowStart: string
  windowEnd: string
}) {
  return (
    <div className="space-y-3" data-testid="active-trips-map">
      <p className="text-xs text-[var(--color-text-muted)]">
        Timeline {formatTimestamp(windowStart)} → {formatTimestamp(windowEnd)}
      </p>
      <div className="relative h-28 rounded-lg border border-slate-700 bg-slate-950/60">
        {trips.length === 0 ? (
          <p className="absolute inset-0 flex items-center justify-center text-sm text-[var(--color-text-muted)]">
            No active trips in window
          </p>
        ) : (
          trips.map((trip) => {
            const color = trip.isLate
              ? 'bg-red-600/80 border-red-400'
              : trip.isAtRisk
                ? 'bg-amber-600/80 border-amber-400'
                : 'bg-sky-600/80 border-sky-400'
            return (
              <div
                key={trip.tripId}
                className={`absolute top-2 bottom-2 rounded border px-1 ${color}`}
                style={{
                  left: `${trip.timelineOffsetPercent}%`,
                  width: `${trip.timelineWidthPercent}%`,
                  minWidth: '2.5rem',
                }}
                title={`${trip.tripNumber}: ${trip.title}`}
                data-testid={`active-trip-map-${trip.tripId}`}
              >
                <span className="block truncate text-[10px] font-medium text-white">
                  {trip.tripNumber}
                </span>
              </div>
            )
          })
        )}
      </div>
      <ul className="flex flex-wrap gap-2 text-[10px] text-[var(--color-text-muted)]">
        {trips.map((trip) => (
          <li key={`legend-${trip.tripId}`}>
            {trip.tripNumber}
            {trip.isLate ? ' (late)' : trip.isAtRisk ? ' (at risk)' : ''}
          </li>
        ))}
      </ul>
    </div>
  )
}

export function ActiveTripsPanel({ accessToken, scope }: Props) {
  const [view, setView] = useState<'list' | 'map'>('list')
  const [attentionOnly, setAttentionOnly] = useState(false)
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all')

  const query = useQuery({
    queryKey: ['routarr-active-trips', accessToken, scope, attentionOnly, statusFilter],
    queryFn: () =>
      getActiveTrips(accessToken, scope, {
        attentionOnly,
        statusFilter,
      }),
  })

  if (query.isLoading) {
    return <p className="text-sm text-slate-400">Loading active trips…</p>
  }

  if (query.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(query.error, 'Failed to load active trips.')}
        onRetry={() => void query.refetch()}
        retryLabel="Retry active trips"
      />
    )
  }

  const data = query.data!

  return (
    <section
      className="rounded-xl border border-emerald-800/40 bg-emerald-950/15 p-5"
      data-testid="active-trips-panel"
    >
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Active trips</h2>
          <p className="mt-1 text-sm text-slate-400">
            {data.summary.totalCount} dispatched/in-progress · {data.summary.lateCount} late ·{' '}
            {data.summary.atRiskCount} at risk
            {data.summary.openExceptionCount > 0
              ? ` · ${data.summary.openExceptionCount} open exception(s)`
              : ''}
          </p>
        </div>
        <div className="flex gap-2">
          {(['list', 'map'] as const).map((mode) => (
            <button
              key={mode}
              type="button"
              className={[
                'rounded-md px-3 py-1 text-sm capitalize',
                view === mode
                  ? 'bg-emerald-700 text-white'
                  : 'bg-slate-800 text-slate-300 hover:bg-slate-700',
              ].join(' ')}
              onClick={() => setView(mode)}
            >
              {mode}
            </button>
          ))}
        </div>
      </header>

      <div className="mt-3 flex flex-wrap items-center gap-3">
        <label className="flex items-center gap-2 text-xs text-slate-400">
          <input id="activetrips"
            type="checkbox"
            checked={attentionOnly}
            onChange={(e) => setAttentionOnly(e.target.checked)}
            data-testid="active-trips-attention-filter"
          />
          Needs attention only
        </label>
        <select id="active-trips-status-filter"
          className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-xs text-slate-200"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value as StatusFilter)}
          aria-label="Active trip status filter"
          data-testid="active-trips-status-filter"
        >
          <option value="all">All statuses</option>
          <option value="dispatched">Dispatched</option>
          <option value="in_progress">In progress</option>
        </select>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-4">
        <div className="rounded-lg border border-slate-700 bg-slate-900/50 p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">Dispatched</p>
          <p className="text-xl font-semibold text-slate-100">{data.summary.dispatchedCount}</p>
        </div>
        <div className="rounded-lg border border-slate-700 bg-slate-900/50 p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">In progress</p>
          <p className="text-xl font-semibold text-slate-100">{data.summary.inProgressCount}</p>
        </div>
        <div className="rounded-lg border border-slate-700 bg-slate-900/50 p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">Needs attention</p>
          <p className="text-xl font-semibold text-amber-200">
            {data.summary.lateCount + data.summary.atRiskCount}
          </p>
        </div>
        <div className="rounded-lg border border-slate-700 bg-slate-900/50 p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">Unassigned active</p>
          <p className="text-xl font-semibold text-violet-200">{data.summary.unassignedCount}</p>
        </div>
      </div>

      <div className="mt-4">
        {view === 'list' ? (
          <ul className="max-h-80 space-y-2 overflow-y-auto">
            {data.items.length === 0 ? (
              <li className="text-sm text-[var(--color-text-muted)]">No dispatched or in-progress trips match filters.</li>
            ) : (
              data.items.map((trip) => <TripListRow key={trip.tripId} trip={trip} />)
            )}
          </ul>
        ) : (
          <TripMapStrip
            trips={data.items}
            windowStart={data.windowStart}
            windowEnd={data.windowEnd}
          />
        )}
      </div>
    </section>
  )
}

