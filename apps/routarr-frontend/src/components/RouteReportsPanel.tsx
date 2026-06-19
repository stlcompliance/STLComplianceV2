import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { exportRouteReportSummaryCsv, getRouteReportRouteDetail, getRouteReportStopDetail, getRouteReportSummary } from '../api/client'

interface Props {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="text-lg font-semibold text-slate-50">{value}</p>
    </div>
  )
}

export function RouteReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [scope, setScope] = useState('daily')
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

  if (!canRead) return null

  const summary = summaryQuery.data

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="route-reports-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Route reports</h2>
          <p className="mt-1 text-sm text-slate-400">Route and stop completion rollups for dispatch operations.</p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting...' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <label htmlFor="route-reports-scope" className="mt-4 flex items-center gap-2 text-sm text-slate-300">
        <span>Scope</span>
        <select
          id="route-reports-scope"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={scope}
          onChange={(event) => {
            setScope(event.target.value)
            setSelectedRouteId(null)
            setSelectedStopId(null)
          }}
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </label>

      {summaryQuery.isLoading ? <p className="mt-3 text-sm text-slate-400">Loading route summary...</p> : null}
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

      {summary ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Routes" value={String(summary.totalRouteCount)} />
            <MetricCard label="Stops" value={String(summary.totalStopCount)} />
            <MetricCard label="Pending stops" value={String(summary.pendingStopCount)} />
            <MetricCard label="Completed stops" value={String(summary.completedStopCount)} />
          </div>

          <div className="mt-6 grid gap-6 lg:grid-cols-2">
            <div>
              <h3 className="text-sm font-semibold text-slate-200">Routes</h3>
              <ul className="mt-2 max-h-60 space-y-1 overflow-y-auto text-sm">
                {summary.routes.map((route) => (
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
                      {route.routeNumber} - {route.title}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
            <div>
              <h3 className="text-sm font-semibold text-slate-200">Recent stops</h3>
              <ul className="mt-2 max-h-60 space-y-1 overflow-y-auto text-sm">
                {summary.recentStops.map((stop) => (
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
                      {stop.routeNumber} - {stop.label}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </>
      ) : null}

      {routeDetailQuery.data ? (
        <div className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm">
          <h3 className="font-semibold text-slate-100">Route detail</h3>
          <p className="mt-1 text-slate-300">
            {routeDetailQuery.data.routeNumber} - {routeDetailQuery.data.title}
          </p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {routeDetailQuery.data.completedStopCount} of {routeDetailQuery.data.totalStopCount} stops complete
          </p>
        </div>
      ) : null}

      {stopDetailQuery.data ? (
        <div className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm">
          <h3 className="font-semibold text-slate-100">Stop detail</h3>
          <p className="mt-1 text-slate-300">
            {stopDetailQuery.data.routeNumber} - {stopDetailQuery.data.label}
          </p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {stopDetailQuery.data.stopStatus} - {stopDetailQuery.data.stopType}
          </p>
        </div>
      ) : null}
    </section>
  )
}
