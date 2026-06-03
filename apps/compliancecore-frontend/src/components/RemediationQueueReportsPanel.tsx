import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useState } from 'react'

import {
  exportRemediationQueueReportSummaryCsv,
  getRemediationQueueReportSummary,
} from '../api/client'

interface RemediationQueueReportsPanelProps {
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

export function RemediationQueueReportsPanel({
  accessToken,
  canRead,
  canExport,
}: RemediationQueueReportsPanelProps) {
  const [queueOnly, setQueueOnly] = useState(true)
  const [severity, setSeverity] = useState('all')
  const [rulePackKey, setRulePackKey] = useState('')
  const [limit, setLimit] = useState('10')

  const summaryQuery = useQuery({
    queryKey: [
      'compliancecore-remediation-queue-report-summary',
      accessToken,
      queueOnly,
      severity,
      rulePackKey,
      limit,
    ],
    queryFn: () =>
      getRemediationQueueReportSummary(accessToken, {
        queueOnly,
        severity: severity === 'all' ? undefined : severity,
        rulePackKey: rulePackKey.trim() || undefined,
        limit: Number.parseInt(limit, 10) || 10,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportRemediationQueueReportSummaryCsv(accessToken, {
        queueOnly,
        severity: severity === 'all' ? undefined : severity,
        rulePackKey: rulePackKey.trim() || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-remediation-queue-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="remediation-queue-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Remediation queue report</h2>
          <p className="mt-1 text-sm text-slate-400">
            Open remediation items derived from missing evidence warnings and unresolved facts.
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
        <label htmlFor="remediation-queue-open-only" className="flex items-center gap-2">
          <input
            id="remediation-queue-open-only"
            type="checkbox"
            checked={queueOnly}
            onChange={(event) => setQueueOnly(event.target.checked)}
          />
          Queue-only items
        </label>
        <label htmlFor="remediation-queue-severity" className="flex items-center gap-2">
          Severity
          <select
            id="remediation-queue-severity"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={severity}
            onChange={(event) => setSeverity(event.target.value)}
          >
            <option value="all">All</option>
            <option value="critical">Critical</option>
            <option value="high">High</option>
            <option value="medium">Medium</option>
            <option value="low">Low</option>
          </select>
        </label>
        <label htmlFor="remediation-queue-rule-pack" className="flex items-center gap-2">
          Rule pack
          <input
            id="remediation-queue-rule-pack"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={rulePackKey}
            onChange={(event) => setRulePackKey(event.target.value)}
            placeholder="optional pack key"
          />
        </label>
        <label htmlFor="remediation-queue-limit" className="flex items-center gap-2">
          Limit
          <input
            id="remediation-queue-limit"
            type="number"
            min={1}
            max={100}
            value={limit}
            onChange={(event) => setLimit(event.target.value)}
            className="w-20 rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
          />
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading remediation queue…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Remediation queue unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load remediation queue report summary.',
            )}
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
            message={getErrorMessage(exportMutation.error, 'Unable to export remediation queue CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Queued items" value={String(summaryQuery.data.queuedCount)} />
            <MetricCard label="Critical" value={String(summaryQuery.data.criticalCount)} />
            <MetricCard label="High" value={String(summaryQuery.data.highCount)} />
            <MetricCard label="Total warnings" value={String(summaryQuery.data.totalWarnings)} />
          </div>

          {summaryQuery.data.queueItems.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No remediation items match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Pack / fact</th>
                    <th className="px-2 py-2">Severity</th>
                    <th className="px-2 py-2">Action</th>
                    <th className="px-2 py-2">Reason</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.queueItems.map((item) => (
                    <tr key={item.warningId} className="border-b border-slate-800/60">
                      <td className="px-2 py-2 text-slate-100">
                        <div>{item.packKey}</div>
                        <div className="text-xs text-slate-500">{item.factKey}</div>
                      </td>
                      <td className="px-2 py-2 text-slate-300">{item.severity}</td>
                      <td className="px-2 py-2 text-slate-300">{item.recommendedAction}</td>
                      <td className="px-2 py-2 text-slate-300">{item.reasonCode}</td>
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
