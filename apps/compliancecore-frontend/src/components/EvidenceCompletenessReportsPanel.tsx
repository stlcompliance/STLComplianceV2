import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  exportEvidenceCompletenessReportSummaryCsv,
  getEvidenceCompletenessReportSummary,
} from '../api/client'

interface EvidenceCompletenessReportsPanelProps {
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

function scoreBadgeClass(level: string): string {
  switch (level) {
    case 'complete':
      return 'bg-emerald-900/50 text-emerald-300'
    case 'partial':
      return 'bg-amber-900/50 text-amber-300'
    default:
      return 'bg-rose-900/50 text-rose-300'
  }
}

export function EvidenceCompletenessReportsPanel({
  accessToken,
  canRead,
  canExport,
}: EvidenceCompletenessReportsPanelProps) {
  const [scopeKey, setScopeKey] = useState('all')
  const [severity, setSeverity] = useState('all')
  const [rulePackKey, setRulePackKey] = useState('')
  const [limit, setLimit] = useState('10')

  const summaryQuery = useQuery({
    queryKey: [
      'compliancecore-evidence-completeness-report-summary',
      accessToken,
      scopeKey,
      severity,
      rulePackKey,
      limit,
    ],
    queryFn: () =>
      getEvidenceCompletenessReportSummary(accessToken, {
        scopeKey: scopeKey === 'all' ? undefined : scopeKey,
        severity: severity === 'all' ? undefined : severity,
        rulePackKey: rulePackKey.trim() || undefined,
        limit: Number.parseInt(limit, 10) || 10,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportEvidenceCompletenessReportSummaryCsv(accessToken, {
        scopeKey: scopeKey === 'all' ? undefined : scopeKey,
        severity: severity === 'all' ? undefined : severity,
        rulePackKey: rulePackKey.trim() || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-evidence-completeness-report-${new Date()
        .toISOString()
        .slice(0, 10)}.csv`
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
      data-testid="evidence-completeness-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Evidence completeness report</h2>
          <p className="mt-1 text-sm text-slate-400">
            Missing-evidence warning rollups by rule pack and scope, with deterministic
            completeness scoring.
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
        <label htmlFor="evidence-completeness-scope" className="flex items-center gap-2">
          Scope
          <select
            id="evidence-completeness-scope"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={scopeKey}
            onChange={(event) => setScopeKey(event.target.value)}
          >
            <option value="all">All</option>
            <option value="tenant">Tenant</option>
          </select>
        </label>
        <label htmlFor="evidence-completeness-severity" className="flex items-center gap-2">
          Severity
          <select
            id="evidence-completeness-severity"
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
        <label htmlFor="evidence-completeness-rule-pack" className="flex items-center gap-2">
          Rule pack
          <input
            id="evidence-completeness-rule-pack"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={rulePackKey}
            onChange={(event) => setRulePackKey(event.target.value)}
            placeholder="optional pack key"
          />
        </label>
        <label htmlFor="evidence-completeness-limit" className="flex items-center gap-2">
          Limit
          <input
            id="evidence-completeness-limit"
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
        <p className="mt-3 text-sm text-slate-400">Loading evidence completeness report…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Evidence completeness report unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load evidence completeness report summary.',
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
            message={getErrorMessage(
              exportMutation.error,
              'Unable to export evidence completeness report CSV.',
            )}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Rule packs" value={String(summaryQuery.data.totalRulePacks)} />
            <MetricCard label="Complete" value={String(summaryQuery.data.completeRulePackCount)} />
            <MetricCard label="Partial" value={String(summaryQuery.data.partialRulePackCount)} />
            <MetricCard label="Incomplete" value={String(summaryQuery.data.incompleteRulePackCount)} />
          </div>

          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Warnings" value={String(summaryQuery.data.totalWarnings)} />
            <MetricCard label="Score" value={String(summaryQuery.data.completenessScore)} />
            <MetricCard label="High" value={String(summaryQuery.data.highWarningCount)} />
            <MetricCard label="Critical" value={String(summaryQuery.data.criticalWarningCount)} />
          </div>

          {summaryQuery.data.rulePacks.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No evidence completeness data matches this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Pack / scope</th>
                    <th className="px-2 py-2">Warnings</th>
                    <th className="px-2 py-2">Score</th>
                    <th className="px-2 py-2">Level</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.rulePacks.map((item) => (
                    <tr key={`${item.rulePackId}-${item.scopeKey}`} className="border-b border-slate-800/60">
                      <td className="px-2 py-2 text-slate-100">
                        <div>{item.packKey}</div>
                        <div className="text-xs text-slate-500">{item.scopeKey}</div>
                        <div className="text-xs text-slate-500">{item.summary}</div>
                      </td>
                      <td className="px-2 py-2 text-slate-300">
                        {item.totalWarnings}
                        <span className="ml-2 text-xs text-slate-500">
                          {item.criticalWarningCount} critical / {item.highWarningCount} high
                        </span>
                      </td>
                      <td className="px-2 py-2 text-slate-300">{item.completenessScore}</td>
                      <td className="px-2 py-2 text-slate-300">
                        <span className={`rounded px-2 py-0.5 text-xs ${scoreBadgeClass(item.completenessLevel)}`}>
                          {item.completenessLevel}
                        </span>
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
