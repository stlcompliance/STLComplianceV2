import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import {
  exportWaiverReportSummaryCsv,
  getWaiverReportSummary,
} from '../api/client'

interface WaiverReportsPanelProps {
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

export function WaiverReportsPanel({ accessToken, canRead, canExport }: WaiverReportsPanelProps) {
  const [status, setStatus] = useState('all')
  const [packKey, setPackKey] = useState('')
  const [scopeKey, setScopeKey] = useState('')

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-waiver-report-summary', accessToken, status, packKey, scopeKey],
    queryFn: () =>
      getWaiverReportSummary(accessToken, {
        status: status === 'all' ? undefined : status,
        packKey: packKey.trim() || undefined,
        scopeKey: scopeKey.trim() || undefined,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportWaiverReportSummaryCsv(accessToken, {
        status: status === 'all' ? undefined : status,
        packKey: packKey.trim() || undefined,
        scopeKey: scopeKey.trim() || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-waiver-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="waiver-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Waiver reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Waiver lifecycles, approval status, and expiring approvals by pack and scope.
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
        <label htmlFor="waiver-reports-status" className="flex items-center gap-2">
          Status
          <select
            id="waiver-reports-status"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={status}
            onChange={(event) => setStatus(event.target.value)}
          >
            <option value="all">All</option>
            <option value="pending">Pending</option>
            <option value="approved">Approved</option>
            <option value="rejected">Rejected</option>
            <option value="revoked">Revoked</option>
            <option value="expired">Expired</option>
          </select>
        </label>
        <label htmlFor="waiver-reports-pack" className="flex items-center gap-2">
          Pack key
          <input
            id="waiver-reports-pack"
            type="text"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={packKey}
            onChange={(event) => setPackKey(event.target.value)}
            placeholder="driver_qualification"
          />
        </label>
        <label htmlFor="waiver-reports-scope" className="flex items-center gap-2">
          Scope key
          <input
            id="waiver-reports-scope"
            type="text"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={scopeKey}
            onChange={(event) => setScopeKey(event.target.value)}
            placeholder="tenant"
          />
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading waiver report summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Waiver report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load waiver report summary.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      )}

      {exportMutation.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export waiver report CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Waivers in scope" value={String(summaryQuery.data.totalWaivers)} />
            <MetricCard label="Pending" value={String(summaryQuery.data.pendingCount)} />
            <MetricCard label="Approved" value={String(summaryQuery.data.approvedCount)} />
            <MetricCard label="Expiring soon" value={String(summaryQuery.data.expiringSoonCount)} />
          </div>

          {summaryQuery.data.recentWaivers.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No waivers match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Waiver</th>
                    <th className="px-2 py-2">Pack</th>
                    <th className="px-2 py-2">Status</th>
                    <th className="px-2 py-2">Expires</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentWaivers.slice(0, 8).map((item) => (
                    <tr key={item.waiverId} className="border-b border-slate-800/60">
                      <td className="px-2 py-2 text-slate-100">
                        <div>{item.waiverKey}</div>
                        <div className="text-xs text-slate-500">
                          {item.reasonCode} · {item.subjectScopeKey}
                        </div>
                      </td>
                      <td className="px-2 py-2 text-slate-300">{item.packKey}</td>
                      <td className="px-2 py-2 text-slate-300">{item.status}</td>
                      <td className="px-2 py-2 text-slate-300">
                        {item.expiresAt ? new Date(item.expiresAt).toLocaleDateString() : 'No expiry'}
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
