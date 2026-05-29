import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import {
  exportFindingsReportSummaryCsv,
  getFindingsReportSummary,
} from '../api/client'

interface ComplianceReportsPanelProps {
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

export function ComplianceReportsPanel({
  accessToken,
  canRead,
  canExport,
}: ComplianceReportsPanelProps) {
  const [severity, setSeverity] = useState('all')
  const [openOnly, setOpenOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-findings-report-summary', accessToken, severity, openOnly],
    queryFn: () =>
      getFindingsReportSummary(accessToken, {
        severity: severity === 'all' ? undefined : severity,
        openOnly,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportFindingsReportSummaryCsv(accessToken, {
        severity: severity === 'all' ? undefined : severity,
        openOnly,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-findings-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="compliance-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Compliance reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Findings rollups by severity and status from Compliance Core-owned tables.
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
        <label htmlFor="compliance-reports-severity" className="flex items-center gap-2">
          Finding severity filter
          <select
            id="compliance-reports-severity"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={severity}
            onChange={(event) => setSeverity(event.target.value)}
          >
            <option value="all">All</option>
            <option value="block">Block</option>
            <option value="warn">Warn</option>
          </select>
        </label>
        <label htmlFor="compliance-reports-open-only" className="flex items-center gap-2">
          <input
            id="compliance-reports-open-only"
            type="checkbox"
            checked={openOnly}
            onChange={(event) => setOpenOnly(event.target.checked)}
          />
          Open findings only
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading compliance report summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-red-300">Failed to load compliance report summary.</p>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Findings in scope" value={String(summaryQuery.data.totalFindings)} />
            <MetricCard label="Open" value={String(summaryQuery.data.openCount)} />
            <MetricCard label="Open (block)" value={String(summaryQuery.data.openBlockSeverityCount)} />
            <MetricCard label="Resolved" value={String(summaryQuery.data.resolvedCount)} />
          </div>

          {summaryQuery.data.recentFindings.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No findings match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Title</th>
                    <th className="px-2 py-2">Severity</th>
                    <th className="px-2 py-2">Status</th>
                    <th className="px-2 py-2">Rule pack</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentFindings.slice(0, 8).map((item) => (
                    <tr key={item.findingId} className="border-b border-slate-800/60">
                      <td className="px-2 py-2 text-slate-100">{item.title}</td>
                      <td className="px-2 py-2 text-slate-300">{item.severity}</td>
                      <td className="px-2 py-2 text-slate-300">{item.status}</td>
                      <td className="px-2 py-2 text-slate-300">{item.packKey}</td>
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
