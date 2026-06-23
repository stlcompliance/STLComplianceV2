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
    return <span className="font-medium text-[var(--tone-danger-text)]">Late</span>
  }
  if (trip.isAtRisk) {
    return <span className="font-medium text-[var(--tone-warning-text)]">At risk</span>
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
      <div className="h-1.5 overflow-hidden rounded-full bg-[var(--color-bg-control-hover)]">
        <div
          className="h-full rounded-full bg-[var(--color-accent)]"
          style={{ width: `${trip.stopProgressPercent}%` }}
        />
      </div>
    </div>
  )
}

function TripListRow({ trip }: { trip: ActiveTripRow }) {
  const borderClass = trip.isLate
    ? 'border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)]'
    : trip.isAtRisk
      ? 'border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)]'
      : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)]'

  const driverLabel = trip.assignedDriverDisplayName
    ?? (trip.assignedDriverPersonId ? trip.assignedDriverPersonId.slice(0, 8) + '…' : null)

  return (
    <li
      className={`rounded-lg border p-3 ${borderClass}`}
      data-testid={`active-trip-row-${trip.tripId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-[var(--color-text-primary)]">{trip.title}</p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-2 text-xs">
          {statusBadge(trip)}
          {trip.openExceptionCount > 0 ? (
            <span
              className="rounded bg-[var(--tone-warning-bg)] px-1.5 py-0.5 text-[var(--tone-warning-text)]"
              data-testid={`active-trip-exceptions-${trip.tripId}`}
            >
              {trip.openExceptionCount} exception{trip.openExceptionCount === 1 ? '' : 's'}
            </span>
          ) : null}
        </div>
      </div>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">
        Start {formatTimestamp(trip.scheduledStartAt)} · End {formatTimestamp(trip.scheduledEndAt)}
      </p>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        {trip.routeCount} route(s) · {trip.pendingStopCount} pending stop(s)
        {trip.vehicleRefKey ? ` · ${trip.vehicleRefKey}` : ''}
        {driverLabel ? ` · driver ${driverLabel}` : ' · unassigned'}
      </p>
      <StopProgressBar trip={trip} />
      {trip.startedAt ? (
        <p className="mt-1 text-xs text-[var(--tone-success-text)]">Started {formatTimestamp(trip.startedAt)}</p>
      ) : trip.dispatchedAt ? (
        <p className="mt-1 text-xs text-[var(--color-link-text)]">
          Dispatched {formatTimestamp(trip.dispatchedAt)}
        </p>
      ) : null}
      <Link
        to={`/trips/${trip.tripId}`}
        className="mt-2 inline-block text-xs text-[var(--color-accent)] hover:underline"
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
      <div className="relative h-28 rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)]">
        {trips.length === 0 ? (
          <p className="absolute inset-0 flex items-center justify-center text-sm text-[var(--color-text-muted)]">
            No active trips in window
          </p>
        ) : (
          trips.map((trip) => {
            const color = trip.isLate
              ? 'bg-[var(--tone-danger-bg)] border-[var(--tone-danger-border)]'
              : trip.isAtRisk
                ? 'bg-[var(--tone-warning-bg)] border-[var(--tone-warning-border)]'
                : 'bg-[var(--color-accent-soft)] border-[var(--color-accent-border)]'
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
                <span className="block truncate text-[10px] font-medium text-[var(--color-text-primary)]">
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
    return <p className="text-sm text-[var(--color-text-muted)]">Loading active trips…</p>
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
      className="rounded-xl border border-[var(--tone-success-border)] bg-[var(--tone-success-bg)] p-5"
      data-testid="active-trips-panel"
    >
      <header className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Active trips</h2>
          <p className="mt-1 text-sm text-[var(--color-text-muted)]">
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
                  ? 'bg-[var(--color-accent)] text-[var(--color-on-accent)]'
                  : 'bg-[var(--color-bg-control-hover)] text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control)]',
              ].join(' ')}
              onClick={() => setView(mode)}
            >
              {mode}
            </button>
          ))}
        </div>
      </header>

      <div className="mt-3 flex flex-wrap items-center gap-3">
        <label className="flex items-center gap-2 text-xs text-[var(--color-text-muted)]">
          <input id="activetrips"
            type="checkbox"
            checked={attentionOnly}
            onChange={(e) => setAttentionOnly(e.target.checked)}
            data-testid="active-trips-attention-filter"
          />
          Needs attention only
        </label>
        <select id="active-trips-status-filter"
          className="rounded border border-[var(--color-border-default)] bg-[var(--color-field-bg)] px-2 py-1 text-xs text-[var(--color-text-primary)]"
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
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">Dispatched</p>
          <p className="text-xl font-semibold text-[var(--color-text-primary)]">{data.summary.dispatchedCount}</p>
        </div>
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">In progress</p>
          <p className="text-xl font-semibold text-[var(--color-text-primary)]">{data.summary.inProgressCount}</p>
        </div>
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">Needs attention</p>
          <p className="text-xl font-semibold text-[var(--tone-warning-text)]">
            {data.summary.lateCount + data.summary.atRiskCount}
          </p>
        </div>
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-3 text-center">
          <p className="text-xs text-[var(--color-text-muted)]">Unassigned active</p>
          <p className="text-xl font-semibold text-[var(--color-link-text)]">{data.summary.unassignedCount}</p>
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
