import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import { getActiveTrips } from '../api/client'
import type { ActiveTripRow } from '../api/types'

type Props = {
  accessToken: string
  scope: 'daily' | 'weekly'
}

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
  return <span className="text-slate-500">On track</span>
}

function TripListRow({ trip }: { trip: ActiveTripRow }) {
  const borderClass = trip.isLate
    ? 'border-red-500/60 bg-red-950/30'
    : trip.isAtRisk
      ? 'border-amber-500/60 bg-amber-950/20'
      : 'border-slate-700 bg-slate-900/40'

  return (
    <li
      className={`rounded-lg border p-3 ${borderClass}`}
      data-testid={`active-trip-row-${trip.tripId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-100">{trip.title}</p>
          <p className="text-xs text-slate-500">
            {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
          </p>
        </div>
        <div className="text-xs">{statusBadge(trip)}</div>
      </div>
      <p className="mt-2 text-xs text-slate-400">
        Start {formatTimestamp(trip.scheduledStartAt)} · End {formatTimestamp(trip.scheduledEndAt)}
      </p>
      <p className="mt-1 text-xs text-slate-500">
        {trip.routeCount} route(s) · {trip.pendingStopCount} pending stop(s)
        {trip.vehicleRefKey ? ` · ${trip.vehicleRefKey}` : ''}
        {trip.assignedDriverPersonId
          ? ` · driver ${trip.assignedDriverPersonId.slice(0, 8)}…`
          : ' · unassigned'}
      </p>
      {trip.startedAt ? (
        <p className="mt-1 text-xs text-emerald-400/80">Started {formatTimestamp(trip.startedAt)}</p>
      ) : trip.dispatchedAt ? (
        <p className="mt-1 text-xs text-sky-400/80">
          Dispatched {formatTimestamp(trip.dispatchedAt)}
        </p>
      ) : null}
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
      <p className="text-xs text-slate-500">
        Timeline {formatTimestamp(windowStart)} → {formatTimestamp(windowEnd)}
      </p>
      <div className="relative h-28 rounded-lg border border-slate-700 bg-slate-950/60">
        {trips.length === 0 ? (
          <p className="absolute inset-0 flex items-center justify-center text-sm text-slate-500">
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
      <ul className="flex flex-wrap gap-2 text-[10px] text-slate-500">
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

  const query = useQuery({
    queryKey: ['routarr-active-trips', accessToken, scope],
    queryFn: () => getActiveTrips(accessToken, scope),
  })

  if (query.isLoading) {
    return <p className="text-sm text-slate-400">Loading active trips…</p>
  }

  if (query.isError) {
    return <p className="text-sm text-red-300">{(query.error as Error).message}</p>
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

      <div className="mt-4 grid gap-3 sm:grid-cols-3">
        <div className="rounded-lg border border-slate-700 bg-slate-900/50 p-3 text-center">
          <p className="text-xs text-slate-500">Dispatched</p>
          <p className="text-xl font-semibold text-slate-100">{data.summary.dispatchedCount}</p>
        </div>
        <div className="rounded-lg border border-slate-700 bg-slate-900/50 p-3 text-center">
          <p className="text-xs text-slate-500">In progress</p>
          <p className="text-xl font-semibold text-slate-100">{data.summary.inProgressCount}</p>
        </div>
        <div className="rounded-lg border border-slate-700 bg-slate-900/50 p-3 text-center">
          <p className="text-xs text-slate-500">Needs attention</p>
          <p className="text-xl font-semibold text-amber-200">
            {data.summary.lateCount + data.summary.atRiskCount}
          </p>
        </div>
      </div>

      <div className="mt-4">
        {view === 'list' ? (
          <ul className="max-h-80 space-y-2 overflow-y-auto">
            {data.items.length === 0 ? (
              <li className="text-sm text-slate-500">No dispatched or in-progress trips in this window.</li>
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
