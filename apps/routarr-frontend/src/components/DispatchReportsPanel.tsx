import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportDispatchReportSummaryCsv,
  getDispatchReportExceptionDetail,
  getDispatchReportSummary,
  getDispatchReportTripDetail,
} from '../api/client'

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

export function DispatchReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [scope, setScope] = useState('daily')
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

  const summary = summaryQuery.data

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="dispatch-reports-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Dispatch reports</h2>
          <p className="mt-1 text-sm text-slate-400">Trip status and exception rollups for dispatch operations.</p>
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

      <label htmlFor="dispatch-reports-scope" className="mt-4 flex items-center gap-2 text-sm text-slate-300">
        <span>Scope</span>
        <select
          id="dispatch-reports-scope"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={scope}
          onChange={(event) => {
            setScope(event.target.value)
            setSelectedTripId(null)
            setSelectedExceptionId(null)
          }}
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </label>

      {summaryQuery.isLoading ? <p className="mt-3 text-sm text-slate-400">Loading dispatch summary...</p> : null}
      {summaryQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Dispatch report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load dispatch report summary.')}
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
            <MetricCard label="Trips" value={String(summary.totalTripCount)} />
            <MetricCard label="Late" value={String(summary.lateTripCount)} />
            <MetricCard label="At risk" value={String(summary.atRiskTripCount)} />
            <MetricCard label="Open exceptions" value={String(summary.openExceptionCount)} />
          </div>

          <div className="mt-6 grid gap-6 lg:grid-cols-2">
            <div>
              <h3 className="text-sm font-semibold text-slate-200">Trips</h3>
              <ul className="mt-2 max-h-60 space-y-1 overflow-y-auto text-sm">
                {summary.trips.map((trip) => (
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
                      {trip.tripNumber} - {trip.title}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
            <div>
              <h3 className="text-sm font-semibold text-slate-200">Recent exceptions</h3>
              <ul className="mt-2 max-h-60 space-y-1 overflow-y-auto text-sm">
                {summary.recentExceptions.map((exception) => (
                  <li key={exception.exceptionId}>
                    <button
                      type="button"
                      className={`w-full rounded px-2 py-1 text-left hover:bg-slate-800 ${
                        selectedExceptionId === exception.exceptionId ? 'bg-slate-800' : ''
                      }`}
                      onClick={() => {
                        setSelectedExceptionId(exception.exceptionId)
                        setSelectedTripId(null)
                      }}
                    >
                      {exception.title} - {exception.status}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </>
      ) : null}

      {tripDetailQuery.data ? (
        <div className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm">
          <h3 className="font-semibold text-slate-100">Trip detail</h3>
          <p className="mt-1 text-slate-300">
            {tripDetailQuery.data.tripNumber} - {tripDetailQuery.data.title}
          </p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {tripDetailQuery.data.dispatchStatus} - {tripDetailQuery.data.pendingStopCount} pending stop(s)
          </p>
        </div>
      ) : null}

      {exceptionDetailQuery.data ? (
        <div className="mt-6 rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm">
          <h3 className="font-semibold text-slate-100">Exception detail</h3>
          <p className="mt-1 text-slate-300">
            {exceptionDetailQuery.data.exceptionKey} - {exceptionDetailQuery.data.title}
          </p>
          <p className="text-xs text-[var(--color-text-muted)]">
            {exceptionDetailQuery.data.status} - {exceptionDetailQuery.data.category}
          </p>
        </div>
      ) : null}
    </section>
  )
}
