import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import {
  exportIncidentReportSummaryCsv,
  getIncidentReportSummary,
} from '../api/client'

interface IncidentReportsPanelProps {
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

export function IncidentReportsPanel({
  accessToken,
  canRead,
  canExport,
}: IncidentReportsPanelProps) {
  const [status, setStatus] = useState('all')
  const [openOnly, setOpenOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['staffarr-incident-report-summary', accessToken, status, openOnly],
    queryFn: () =>
      getIncidentReportSummary(accessToken, {
        status: status === 'all' ? undefined : status,
        openOnly,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportIncidentReportSummaryCsv(accessToken, {
        status: status === 'all' ? undefined : status,
        openOnly,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `staffarr-incident-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="incident-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Incident reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Personnel incident rollups by status and severity from StaffArr-owned tables.
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

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-slate-300">
        <label className="flex items-center gap-2">
          Status
          <select
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
          >
            <option value="all">All</option>
            <option value="open">Open</option>
            <option value="closed">Closed</option>
          </select>
        </label>
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={openOnly}
            onChange={(event) => setOpenOnly(event.target.checked)}
          />
          Open only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading incident report summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-red-300">Failed to load incident report summary.</p>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Incidents in scope" value={String(summaryQuery.data.totalIncidents)} />
            <MetricCard label="Open" value={String(summaryQuery.data.openCount)} />
            <MetricCard label="Closed" value={String(summaryQuery.data.closedCount)} />
            <MetricCard
              label="High severity open"
              value={String(summaryQuery.data.highSeverityOpenCount)}
            />
          </div>

          {summaryQuery.data.recentIncidents.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No incidents match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Title</th>
                    <th className="px-2 py-2">Severity</th>
                    <th className="px-2 py-2">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentIncidents.slice(0, 8).map((item) => (
                    <tr key={item.incidentId} className="border-b border-slate-800">
                      <td className="px-2 py-2 text-slate-100">{item.title}</td>
                      <td className="px-2 py-2 text-slate-300">{item.severity}</td>
                      <td className="px-2 py-2 text-slate-300">{item.status}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </section>
  )
}
