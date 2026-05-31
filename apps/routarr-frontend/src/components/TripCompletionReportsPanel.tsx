import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getRouteCompletions,
  getTripCompletionDetail,
  getTripCompletions,
} from '../api/client'
import type { TripCompletionSummaryResponse } from '../api/types'

type Props = {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-700 bg-slate-950/60 px-3 py-2">
      <p className="text-xs text-slate-500">{label}</p>
      <p className="text-lg font-semibold text-slate-100">{value}</p>
    </div>
  )
}

function buildTripCompletionsCsv(items: TripCompletionSummaryResponse[]): string {
  const header = [
    'tripNumber',
    'title',
    'dispatchStatus',
    'assignedDriverPersonId',
    'vehicleRefKey',
    'durationMinutes',
    'routeCount',
    'completedRouteCount',
    'stopCount',
    'completedStopCount',
    'skippedStopCount',
    'loadCount',
    'deliveredLoadCount',
    'completedAt',
    'isMaterialized',
  ]
  const rows = items.map((item) =>
    [
      item.tripNumber,
      item.title,
      item.dispatchStatus,
      item.assignedDriverPersonId ?? '',
      item.vehicleRefKey ?? '',
      item.durationMinutes ?? '',
      item.routeCount,
      item.completedRouteCount,
      item.stopCount,
      item.completedStopCount,
      item.skippedStopCount,
      item.loadCount,
      item.deliveredLoadCount,
      item.completedAt ?? item.cancelledAt ?? '',
      item.isMaterialized ? 'yes' : 'no',
    ]
      .map((value) => `"${String(value).replace(/"/g, '""')}"`)
      .join(','),
  )
  return [header.join(','), ...rows].join('\n')
}

export function TripCompletionReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [dispatchStatus, setDispatchStatus] = useState<'all' | 'completed' | 'cancelled'>('all')
  const [selectedTripId, setSelectedTripId] = useState<string | null>(null)

  const statusFilter = dispatchStatus === 'all' ? undefined : dispatchStatus

  const tripsQuery = useQuery({
    queryKey: ['routarr-trip-completions', accessToken, statusFilter],
    queryFn: () => getTripCompletions(accessToken, { dispatchStatus: statusFilter }),
    enabled: canRead,
  })

  const routesQuery = useQuery({
    queryKey: ['routarr-route-completions', accessToken],
    queryFn: () => getRouteCompletions(accessToken),
    enabled: canRead,
  })

  const detailQuery = useQuery({
    queryKey: ['routarr-trip-completion-detail', accessToken, selectedTripId],
    queryFn: () => getTripCompletionDetail(accessToken, selectedTripId!),
    enabled: canRead && Boolean(selectedTripId),
  })

  const metrics = useMemo(() => {
    const items = tripsQuery.data?.items ?? []
    const materializedCount = items.filter((item) => item.isMaterialized).length
    const completedCount = items.filter((item) => item.dispatchStatus === 'completed').length
    const cancelledCount = items.filter((item) => item.dispatchStatus === 'cancelled').length
    const totalStops = items.reduce((sum, item) => sum + item.completedStopCount, 0)
    const totalLoads = items.reduce((sum, item) => sum + item.deliveredLoadCount, 0)
    return {
      tripCount: items.length,
      materializedCount,
      completedCount,
      cancelledCount,
      totalStops,
      totalLoads,
    }
  }, [tripsQuery.data?.items])

  function handleExportCsv() {
    const items = tripsQuery.data?.items ?? []
    const csv = buildTripCompletionsCsv(items)
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' })
    const url = URL.createObjectURL(blob)
    const anchor = document.createElement('a')
    anchor.href = url
    anchor.download = `routarr-trip-completions-${new Date().toISOString().slice(0, 10)}.csv`
    anchor.click()
    URL.revokeObjectURL(url)
  }

  if (!canRead) {
    return null
  }

  return (
    <section
      className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="trip-completion-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Trip completion reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Terminal trip and route completion rollups with milestone timelines from materialized
            worker summaries or live terminal trip data.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={!tripsQuery.data || tripsQuery.isLoading}
            onClick={handleExportCsv}
          >
            Export CSV
          </button>
        ) : null}
      </div>

      <label
        className="mt-4 flex items-center gap-2 text-sm text-slate-300"
        htmlFor="tripcompletionreports-status"
      >
        Dispatch status
        <select
          id="tripcompletionreports-status"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={dispatchStatus}
          onChange={(event) => {
            setDispatchStatus(event.target.value as 'all' | 'completed' | 'cancelled')
            setSelectedTripId(null)
          }}
        >
          <option value="all">All terminal</option>
          <option value="completed">Completed</option>
          <option value="cancelled">Cancelled</option>
        </select>
      </label>

      {tripsQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-500">Loading trip completion summaries…</p>
      ) : null}

      {tripsQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Trip completion report unavailable"
            message={getErrorMessage(tripsQuery.error, 'Failed to load trip completion summaries.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void tripsQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {tripsQuery.data ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Terminal trips" value={String(metrics.tripCount)} />
            <MetricCard label="Materialized rollups" value={String(metrics.materializedCount)} />
            <MetricCard
              label="Completed / cancelled"
              value={`${metrics.completedCount} / ${metrics.cancelledCount}`}
            />
            <MetricCard
              label="Stops / loads delivered"
              value={`${metrics.totalStops} / ${metrics.totalLoads}`}
            />
          </div>

          <div className="mt-6">
            <h3 className="text-sm font-semibold text-slate-200">Trips</h3>
            {tripsQuery.data.items.length === 0 ? (
              <p className="mt-2 text-xs text-slate-500" data-testid="trip-completion-reports-empty">
                No terminal trips match this filter.
              </p>
            ) : (
              <ul className="mt-2 max-h-64 space-y-1 overflow-y-auto text-sm">
                {tripsQuery.data.items.map((trip) => (
                  <li key={trip.tripId}>
                    <button
                      type="button"
                      className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                        selectedTripId === trip.tripId ? 'bg-slate-800' : ''
                      }`}
                      onClick={() => setSelectedTripId(trip.tripId)}
                    >
                      {trip.tripNumber} — {trip.title}
                      <span className="ml-2 text-xs text-slate-500">
                        {trip.dispatchStatus.replace('_', ' ')}
                        {trip.isMaterialized ? ' · materialized' : ' · live'}
                        {trip.durationMinutes != null ? ` · ${trip.durationMinutes} min` : ''}
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </>
      ) : null}

      {detailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="trip-completion-report-detail"
        >
          <h3 className="font-semibold text-slate-100">Trip completion detail</h3>
          <p className="mt-1 text-slate-300">
            {detailQuery.data.summary.tripNumber} — {detailQuery.data.summary.title}
          </p>
          <p className="text-xs text-slate-500">
            {detailQuery.data.summary.completedRouteCount}/{detailQuery.data.summary.routeCount}{' '}
            routes · {detailQuery.data.summary.completedStopCount}/{detailQuery.data.summary.stopCount}{' '}
            stops · {detailQuery.data.summary.deliveredLoadCount}/{detailQuery.data.summary.loadCount}{' '}
            loads
          </p>
          {detailQuery.data.events.length === 0 ? (
            <p className="mt-2 text-xs text-slate-500">No milestone events recorded.</p>
          ) : (
            <ol className="mt-3 space-y-1 text-xs text-slate-400">
              {detailQuery.data.events.map((event) => (
                <li key={`${event.sequenceNumber}-${event.eventKind}`}>
                  {event.sequenceNumber}. {event.title}
                  {event.detail ? ` — ${event.detail}` : ''}
                  <span className="ml-2 text-slate-600">
                    {new Date(event.occurredAt).toLocaleString()}
                  </span>
                </li>
              ))}
            </ol>
          )}
        </div>
      ) : null}

      {routesQuery.data ? (
        <div className="mt-6">
          <h3 className="text-sm font-semibold text-slate-200">Route completions</h3>
          {routesQuery.data.items.length === 0 ? (
            <p className="mt-2 text-xs text-slate-500">No route completion rollups yet.</p>
          ) : (
            <ul className="mt-2 max-h-48 space-y-1 overflow-y-auto text-sm">
              {routesQuery.data.items.slice(0, 20).map((route) => (
                <li key={route.routeId} className="rounded px-2 py-1 text-slate-300">
                  {route.routeNumber} — {route.title}
                  <span className="ml-2 text-xs text-slate-500">
                    {route.completedStopCount}/{route.stopCount} stops
                    {route.tripNumber ? ` · trip ${route.tripNumber}` : ''}
                    {route.isMaterialized ? ' · materialized' : ' · live'}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </div>
      ) : null}
    </section>
  )
}
