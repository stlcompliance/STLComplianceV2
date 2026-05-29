import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import {
  exportDispatchReportSummaryCsv,
  getDispatchReportExceptionDetail,
  getDispatchReportSummary,
  getDispatchReportTripDetail,
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

export function DispatchReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [scope, setScope] = useState<'daily' | 'weekly'>('daily')
  const [selectedTripId, setSelectedTripId] = useState<string | null>(null)
  const [selectedExceptionId, setSelectedExceptionId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['routarr-dispatch-report-summary', accessToken, scope],
    queryFn: () => getDispatchReportSummary(accessToken, { scope }),
    enabled: canRead,
  })

  const tripDetailQuery = useQuery({
    queryKey: ['routarr-dispatch-report-trip', accessToken, selectedTripId],
    queryFn: () => getDispatchReportTripDetail(accessToken, selectedTripId!),
    enabled: canRead && Boolean(selectedTripId),
  })

  const exceptionDetailQuery = useQuery({
    queryKey: ['routarr-dispatch-report-exception', accessToken, selectedExceptionId],
    queryFn: () => getDispatchReportExceptionDetail(accessToken, selectedExceptionId!),
    enabled: canRead && Boolean(selectedExceptionId),
  })

  const exportMutation = useMutation({
    mutationFn: () => exportDispatchReportSummaryCsv(accessToken, { scope }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `routarr-dispatch-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="dispatch-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Dispatch & transportation reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Trip execution rollups, dispatch exceptions, and delay-category metrics from RoutArr-owned
            tables.
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

      <label className="mt-4 flex items-center gap-2 text-sm text-slate-300" htmlFor="dispatchreports-scope">
          Scope
          <select id="dispatchreports-scope"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={scope}
          onChange={(e) => {
            setScope(e.target.value as 'daily' | 'weekly')
            setSelectedTripId(null)
            setSelectedExceptionId(null)
          }}
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </label>

      {summaryQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-500">Loading dispatch report summary…</p>
      ) : null}

      {summaryQuery.isError ? (
        <p className="mt-3 text-sm text-rose-400">Failed to load dispatch report summary.</p>
      ) : null}

      {summaryQuery.data ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Trips in scope" value={String(summaryQuery.data.totalTripCount)} />
            <MetricCard label="Late trips" value={String(summaryQuery.data.lateTripCount)} />
            <MetricCard label="At-risk trips" value={String(summaryQuery.data.atRiskTripCount)} />
            <MetricCard
              label="Delay exceptions"
              value={String(summaryQuery.data.delayExceptionCount)}
            />
          </div>

          <div className="mt-6">
            <h3 className="text-sm font-semibold text-slate-200">Trips</h3>
            {summaryQuery.data.trips.length === 0 ? (
              <p className="mt-2 text-xs text-slate-500">No trips in this reporting window.</p>
            ) : (
              <ul className="mt-2 max-h-64 space-y-1 overflow-y-auto text-sm">
                {summaryQuery.data.trips.map((trip) => (
                  <li key={trip.tripId}>
                    <button
                      type="button"
                      className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                        selectedTripId === trip.tripId ? 'bg-slate-800' : ''
                      }`}
                      onClick={() => {
                        setSelectedTripId(trip.tripId)
                        setSelectedExceptionId(null)
                      }}
                    >
                      {trip.tripNumber} — {trip.title}
                      <span className="ml-2 text-xs text-slate-500">
                        {trip.dispatchStatus.replace('_', ' ')}
                        {trip.isLate ? ' · late' : trip.isAtRisk ? ' · at risk' : ''}
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="mt-6">
            <h3 className="text-sm font-semibold text-slate-200">Recent exceptions</h3>
            {summaryQuery.data.recentExceptions.length === 0 ? (
              <p className="mt-2 text-xs text-slate-500">No exceptions in this window.</p>
            ) : (
              <ul className="mt-2 max-h-48 space-y-1 overflow-y-auto text-sm">
                {summaryQuery.data.recentExceptions.map((ex) => (
                  <li key={ex.exceptionId}>
                    <button
                      type="button"
                      className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                        selectedExceptionId === ex.exceptionId ? 'bg-slate-800' : ''
                      }`}
                      onClick={() => {
                        setSelectedExceptionId(ex.exceptionId)
                        setSelectedTripId(null)
                      }}
                    >
                      {ex.exceptionKey} — {ex.title}
                      <span className="ml-2 text-xs text-slate-500">
                        {ex.category} · {ex.status}
                      </span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        </>
      ) : null}

      {tripDetailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="dispatch-report-trip-detail"
        >
          <h3 className="font-semibold text-slate-100">Trip detail</h3>
          <p className="mt-1 text-slate-300">
            {tripDetailQuery.data.tripNumber} — {tripDetailQuery.data.title}
          </p>
          <p className="text-xs text-slate-500">
            {tripDetailQuery.data.dispatchStatus} · {tripDetailQuery.data.routeCount} route(s) ·{' '}
            {tripDetailQuery.data.delayExceptionCount} delay exception(s)
          </p>
        </div>
      ) : null}

      {exceptionDetailQuery.data ? (
        <div
          className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
          data-testid="dispatch-report-exception-detail"
        >
          <h3 className="font-semibold text-slate-100">Exception detail</h3>
          <p className="mt-1 text-slate-300">
            {exceptionDetailQuery.data.exceptionKey} — {exceptionDetailQuery.data.title}
          </p>
          <p className="text-xs text-slate-500">
            {exceptionDetailQuery.data.category} · {exceptionDetailQuery.data.status}
            {exceptionDetailQuery.data.tripNumber
              ? ` · trip ${exceptionDetailQuery.data.tripNumber}`
              : ''}
          </p>
        </div>
      ) : null}
    </section>
  )
}
