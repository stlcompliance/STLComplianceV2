import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { exportDispatchOverrideReportSummaryCsv, getDispatchOverrideReportSummary } from '../api/client'

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

export function DispatchOverrideReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [scope, setScope] = useState('daily')

  const summaryQuery = useQuery({
    queryKey: ['routarr-dispatch-override-report-summary', accessToken, scope],
    queryFn: () => getDispatchOverrideReportSummary(accessToken, { scope }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () => exportDispatchOverrideReportSummaryCsv(accessToken, { scope }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `routarr-dispatch-override-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) return null

  const summary = summaryQuery.data

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="dispatch-override-reports-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Dispatch override reports</h2>
          <p className="mt-1 text-sm text-slate-400">Audit summaries for dispatch assignment overrides.</p>
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

      <label htmlFor="dispatch-override-reports-scope" className="mt-4 flex items-center gap-2 text-sm text-slate-300">
        <span>Scope</span>
        <select
          id="dispatch-override-reports-scope"
          className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          value={scope}
          onChange={(event) => setScope(event.target.value)}
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </label>

      {summaryQuery.isLoading ? <p className="mt-3 text-sm text-slate-400">Loading override summary...</p> : null}
      {summaryQuery.isError ? (
        <div className="mt-3">
          <ApiErrorCallout
            title="Dispatch override report unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load dispatch override report summary.',
            )}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {summary ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3 text-sm">
            <MetricCard label="Overrides" value={String(summary.totalOverrideCount)} />
            <MetricCard label="Driver overrides" value={String(summary.driverAssignmentOverrideCount)} />
            <MetricCard label="Vehicle overrides" value={String(summary.vehicleAssignmentOverrideCount)} />
          </div>
          <div className="mt-4">
            <h3 className="text-sm font-semibold text-slate-200">Recent overrides</h3>
            <ul className="mt-2 space-y-1 text-sm">
              {summary.recentOverrides.map((override) => (
                <li key={override.auditEventId} className="rounded border border-slate-800 bg-slate-950/40 px-3 py-2">
                  <p className="font-medium text-slate-100">{override.action}</p>
                  <p className="text-xs text-slate-500">{override.targetType} - {override.result}</p>
                </li>
              ))}
            </ul>
          </div>
        </>
      ) : null}
    </section>
  )
}
