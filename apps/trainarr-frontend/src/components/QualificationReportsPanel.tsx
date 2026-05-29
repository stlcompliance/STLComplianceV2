import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import {
  exportQualificationReportSummaryCsv,
  getQualificationReportSummary,
} from '../api/client'

interface QualificationReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-border bg-card px-3 py-2">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="text-lg font-semibold text-foreground">{value}</p>
    </div>
  )
}

export function QualificationReportsPanel({
  accessToken,
  canRead,
  canExport,
}: QualificationReportsPanelProps) {
  const [status, setStatus] = useState('all')

  const summaryQuery = useQuery({
    queryKey: ['trainarr-qualification-report-summary', accessToken, status],
    queryFn: () =>
      getQualificationReportSummary(accessToken, {
        status: status === 'all' ? undefined : status,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportQualificationReportSummaryCsv(accessToken, {
        status: status === 'all' ? undefined : status,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `trainarr-qualification-report-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="mt-6 rounded-xl border border-border bg-card p-5"
      data-testid="qualification-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-foreground">Qualification reports</h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Issued qualification lifecycle metrics, expiring credentials, and status rollups.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:opacity-90 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-4 text-sm text-foreground">
        <label className="flex items-center gap-2">
          Status
          <select
            className="rounded border border-border bg-background px-2 py-1"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
          >
            <option value="all">All</option>
            <option value="issued">Issued</option>
            <option value="expired">Expired</option>
            <option value="suspended">Suspended</option>
            <option value="revoked">Revoked</option>
          </select>
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-muted-foreground">Loading qualification report summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load qualification report summary.</p>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Qualifications in scope" value={String(summaryQuery.data.totalQualifications)} />
            <MetricCard label="Issued" value={String(summaryQuery.data.issuedCount)} />
            <MetricCard label="Expired" value={String(summaryQuery.data.expiredCount)} />
            <MetricCard label="Expiring within 30 days" value={String(summaryQuery.data.expiringWithin30Days)} />
          </div>

          {summaryQuery.data.recentQualifications.length === 0 ? (
            <p className="mt-4 text-sm text-muted-foreground">No qualifications match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-border text-left text-muted-foreground">
                    <th className="px-2 py-2">Qualification</th>
                    <th className="px-2 py-2">Status</th>
                    <th className="px-2 py-2">Expires</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentQualifications.slice(0, 8).map((item) => (
                    <tr key={item.qualificationIssueId} className="border-b border-border/60">
                      <td className="px-2 py-2">{item.qualificationName}</td>
                      <td className="px-2 py-2">{item.status}</td>
                      <td className="px-2 py-2">
                        {item.expiresAt ? new Date(item.expiresAt).toLocaleDateString() : '—'}
                      </td>
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
