import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportDispatchOverrideReportSummaryCsv,
  getDispatchOverrideReportSummary,
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

export function DispatchOverrideReportsPanel({ accessToken, canRead, canExport }: Props) {
  const [scope, setScope] = useState<'daily' | 'weekly'>('daily')

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

  if (!canRead) {
    return null
  }

  const summary = summaryQuery.data

  return (
    <section
      className="mt-8 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="dispatch-override-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Dispatch gate override audit</h2>
          <p className="mt-1 text-sm text-slate-400">
            Driver and vehicle assignments where availability, eligibility, dispatchability, or
            workflow gates were explicitly overridden.
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

      <label
        className="mt-4 flex items-center gap-2 text-sm text-slate-300"
        htmlFor="dispatchoverridereports-scope"
      >
        Scope
        <select
          id="dispatchoverridereports-scope"
          className="rounded-md border border-slate-600 bg-slate-950 px-2 py-1 text-slate-100"
          value={scope}
          onChange={(event) => setScope(event.target.value as 'daily' | 'weekly')}
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </label>

      {summaryQuery.isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading dispatch override audit summary…</p>
      ) : null}

      {summaryQuery.isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Override report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load dispatch override report summary.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      ) : null}

      {exportMutation.isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export dispatch override report CSV.')}
          />
        </div>
      ) : null}

      {summary ? (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <MetricCard label="Overrides in scope" value={String(summary.totalOverrideCount)} />
            <MetricCard
              label="Driver assignment overrides"
              value={String(summary.driverAssignmentOverrideCount)}
            />
            <MetricCard
              label="Vehicle assignment overrides"
              value={String(summary.vehicleAssignmentOverrideCount)}
            />
            <MetricCard
              label="Reporting window"
              value={`${formatTimestamp(summary.windowStart)} – ${formatTimestamp(summary.windowEnd)}`}
            />
          </div>

          {summary.overrideKindCounts.length > 0 ? (
            <div className="mt-4">
              <h3 className="text-sm font-medium text-slate-200">Override kinds</h3>
              <ul className="mt-2 flex flex-wrap gap-2">
                {summary.overrideKindCounts.map((item) => (
                  <li
                    key={item.key}
                    className="rounded-full border border-amber-800/60 bg-amber-950/40 px-3 py-1 text-xs text-amber-100"
                  >
                    {item.key}: {item.count}
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

          {summary.recentOverrides.length === 0 ? (
            <p
              className="mt-4 text-sm text-slate-400"
              data-testid="dispatch-override-reports-empty"
            >
              No dispatch assignment overrides in this reporting window.
            </p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-left text-sm text-slate-200">
                <thead className="text-xs uppercase text-slate-500">
                  <tr>
                    <th className="px-2 py-2">When</th>
                    <th className="px-2 py-2">Action</th>
                    <th className="px-2 py-2">Trip</th>
                    <th className="px-2 py-2">Override kinds</th>
                    <th className="px-2 py-2">Audit result</th>
                  </tr>
                </thead>
                <tbody>
                  {summary.recentOverrides.map((entry) => (
                    <tr key={entry.auditEventId} className="border-t border-slate-800">
                      <td className="px-2 py-2 whitespace-nowrap">
                        {formatTimestamp(entry.occurredAt)}
                      </td>
                      <td className="px-2 py-2">{entry.action.replace('trip.', '')}</td>
                      <td className="px-2 py-2 font-mono text-xs text-slate-400">
                        {entry.targetId ?? '—'}
                      </td>
                      <td className="px-2 py-2">{entry.overrideKinds.join(', ') || '—'}</td>
                      <td className="px-2 py-2 max-w-md truncate" title={entry.result}>
                        {entry.result}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      ) : null}
    </section>
  )
}
