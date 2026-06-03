import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportRouteReportSummaryCsv,
  getRouteReportRouteDetail,
  getRouteReportStopDetail,
  getRouteReportSummary,
} from '../api/client'

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

function formatTimestamp(iso: string) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

export function RouteReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [scope, setScope] = useState<'daily' | 'weekly'>('daily')
  const [selectedRouteId, setSelectedRouteId] = useState<string | null>(null)
  const [selectedStopId, setSelectedStopId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['routarr-route-report-summary', accessToken, scope],
    queryFn: () => getRouteReportSummary(accessToken, { scope }),
    enabled: canRead,
  })

  const routeDetailQuery = useQuery({
    queryKey: ['routarr-route-report-route', accessToken, selectedRouteId],
    queryFn: () => getRouteReportRouteDetail(accessToken, selectedRouteId!),
    enabled: canRead && Boolean(selectedRouteId),
  })

  const stopDetailQuery = useQuery({
    queryKey: ['routarr-route-report-stop', accessToken, selectedStopId],
    queryFn: () => getRouteReportStopDetail(accessToken, selectedStopId!),
    enabled: canRead && Boolean(selectedStopId),
  })

  const exportMutation = useMutation({
    mutationFn: () => exportRouteReportSummaryCsv(accessToken, { scope }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `routarr-route-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="route-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Route & stop execution reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Route and stop rollups with completion metrics from RoutArr-owned routes and stops.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <label className="mt-4 flex items-center gap-2 text-sm text-slate-300" htmlFor="routereports-scope">
          Scope
          <select id="routereports-scope"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={scope}
          onChange={(e) => {
            setScope(e.target.value as 'daily' | 'weekly')
            setSelectedRouteId(null)
            setSelectedStopId(null)
          }}
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </label>

      {summaryQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-500">Loading route report summary…</p>
      ) : null}

      {summaryQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Route report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load route report summary.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {exportMutation.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export route report CSV.')}
          />
        </div>
      ) : null}

      {summaryQuery.data ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Routes in scope" value={String(summaryQuery.data.totalRouteCount)} />
            <MetricCard label="Stops in scope" value={String(summaryQuery.data.totalStopCount)} />
            <MetricCard
              label="Completed stops"
              value={String(summaryQuery.data.completedStopCount)}
            />
            <MetricCard
              label="Pending stops"
              value={String(summaryQuery.data.pendingStopCount)}
            />
          </div>

          <div className="mt-6">
            <h3 className="text-sm font-semibold text-slate-200">Routes</h3>
            {summaryQuery.data.routes.length === 0 ? (
              <p className="mt-2 text-xs text-slate-500">No routes in this reporting window.</p>
            ) : (
              <ul className="mt-2 max-h-64 space-y-1 overflow-y-auto text-sm">
                {summaryQuery.data.routes.map((route) => (
                  <li key={route.routeId}>
                    <button
                      type="button"
                      className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                        selectedRouteId === route.routeId ? 'bg-slate-800' : ''
                      }`}
                      onClick={() => {
                        setSelectedRouteId(route.routeId)
                        setSelectedStopId(null)
                      }}
                    >
                      {route.routeNumber} — {route.title}
                      <span className="ml-2 text-xs text-slate-500">
                        {route.completionPercent}% · {route.completedStopCount}/{route.totalStopCount}{' '}
                        stops
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="mt-6">
            <h3 className="text-sm font-semibold text-slate-200">Recent stops</h3>
            {summaryQuery.data.recentStops.length === 0 ? (
              <p className="mt-2 text-xs text-slate-500">No stops in this window.</p>
            ) : (
              <ul className="mt-2 max-h-48 space-y-1 overflow-y-auto text-sm">
                {summaryQuery.data.recentStops.map((stop) => (
                  <li key={stop.stopId}>
                    <button
                      type="button"
                      className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                        selectedStopId === stop.stopId ? 'bg-slate-800' : ''
                      }`}
                      onClick={() => {
                        setSelectedStopId(stop.stopId)
                        setSelectedRouteId(null)
                      }}
                    >
                      {stop.routeNumber} · {stop.stopKey} — {stop.label}
                      <span className="ml-2 text-xs text-slate-500">
                        {stop.stopType} · {stop.stopStatus}
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </>
      ) : null}

      {routeDetailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="route-report-route-detail"
        >
          <h3 className="font-semibold text-slate-100">Route detail</h3>
          <p className="mt-1 text-slate-300">
            {routeDetailQuery.data.routeNumber} — {routeDetailQuery.data.title}
          </p>
          <p className="text-xs text-slate-500">
            {routeDetailQuery.data.completionPercent}% complete · {routeDetailQuery.data.stops.length}{' '}
            stop(s)
            {routeDetailQuery.data.tripNumber
              ? ` · trip ${routeDetailQuery.data.tripNumber}`
              : ''}
          </p>
          <div className="mt-4 rounded border border-slate-700 bg-slate-900/40 p-3">
            <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-500">Route history</h4>
            {routeDetailQuery.data.history.length === 0 ? (
              <p className="mt-2 text-xs text-slate-500">No route history events recorded yet.</p>
            ) : (
              <ul className="mt-2 space-y-2">
                {routeDetailQuery.data.history.map((entry) => (
                  <li key={`${entry.action}-${entry.occurredAt}`} className="text-xs text-slate-300">
                    <div className="font-medium text-slate-100">{entry.action}</div>
                    <div className="text-slate-400">
                      {entry.result}
                      {entry.reasonCode ? ` · ${entry.reasonCode}` : ''}
                      {entry.actorUserId ? ` · actor ${entry.actorUserId}` : ''}
                    </div>
                    <div className="text-slate-500">{formatTimestamp(entry.occurredAt)}</div>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}

      {stopDetailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="route-report-stop-detail"
        >
          <h3 className="font-semibold text-slate-100">Stop detail</h3>
          <p className="mt-1 text-slate-300">
            {stopDetailQuery.data.routeNumber} · {stopDetailQuery.data.stopKey} —{' '}
            {stopDetailQuery.data.label}
          </p>
          <p className="text-xs text-slate-500">
            {stopDetailQuery.data.stopType} · {stopDetailQuery.data.stopStatus}
          </p>
        </div>
      ) : null}
    </section>
  )
}
