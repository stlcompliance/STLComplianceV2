import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useState } from 'react'

import {
  exportAuditReadinessReportSummaryCsv,
  getAuditReadinessReportSummary,
} from '../api/client'

interface AuditReadinessReportsPanelProps {
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

export function AuditReadinessReportsPanel({
  accessToken,
  canRead,
  canExport,
}: AuditReadinessReportsPanelProps) {
  const [scopeKey, setScopeKey] = useState('all')
  const [rulePackKey, setRulePackKey] = useState('')
  const [readinessLevel, setReadinessLevel] = useState('all')
  const [limit, setLimit] = useState('12')

  const summaryQuery = useQuery({
    queryKey: [
      'compliancecore-audit-readiness-report-summary',
      accessToken,
      scopeKey,
      rulePackKey,
      readinessLevel,
      limit,
    ],
    queryFn: () =>
      getAuditReadinessReportSummary(accessToken, {
        scopeKey: scopeKey === 'all' ? undefined : scopeKey,
        rulePackKey: rulePackKey.trim() || undefined,
        readinessLevel: readinessLevel === 'all' ? undefined : readinessLevel,
        limit: Number.parseInt(limit, 10) || 12,
      }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportAuditReadinessReportSummaryCsv(accessToken, {
        scopeKey: scopeKey === 'all' ? undefined : scopeKey,
        rulePackKey: rulePackKey.trim() || undefined,
        readinessLevel: readinessLevel === 'all' ? undefined : readinessLevel,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-audit-readiness-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="audit-readiness-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Audit readiness report</h2>
          <p className="mt-1 text-sm text-slate-400">
            Latest readiness forecast results for the tenant with summary counts, risk, and
            evidence pressure.
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
        <label htmlFor="audit-readiness-scope" className="flex items-center gap-2">
          Scope
          <select
            id="audit-readiness-scope"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={scopeKey}
            onChange={(event) => setScopeKey(event.target.value)}
          >
            <option value="all">All</option>
            <option value="tenant">Tenant</option>
          </select>
        </label>
        <label htmlFor="audit-readiness-rule-pack" className="flex items-center gap-2">
          Rule pack
          <input
            id="audit-readiness-rule-pack"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={rulePackKey}
            onChange={(event) => setRulePackKey(event.target.value)}
            placeholder="optional pack key"
          />
        </label>
        <label htmlFor="audit-readiness-level" className="flex items-center gap-2">
          Level
          <select
            id="audit-readiness-level"
            className="rounded border border-slate-700 bg-slate-950 px-2 py-1 text-slate-100"
            value={readinessLevel}
            onChange={(event) => setReadinessLevel(event.target.value)}
          >
            <option value="all">All</option>
            <option value="ready">Ready</option>
            <option value="caution">Caution</option>
            <option value="not_ready">Not ready</option>
            <option value="unknown">Unknown</option>
          </select>
        </label>
        <label htmlFor="audit-readiness-limit" className="flex items-center gap-2">
          Limit
          <input
            id="audit-readiness-limit"
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
        <p className="mt-3 text-sm text-slate-400">Loading audit readiness report…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Audit readiness report unavailable"
            message={getErrorMessage(
              summaryQuery.error,
              'Failed to load audit readiness report summary.',
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
            message={getErrorMessage(exportMutation.error, 'Unable to export audit readiness CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Forecasts" value={String(summaryQuery.data.totalForecasts)} />
            <MetricCard label="Ready" value={String(summaryQuery.data.readyCount)} />
            <MetricCard label="Caution" value={String(summaryQuery.data.cautionCount)} />
            <MetricCard label="Not ready" value={String(summaryQuery.data.notReadyCount)} />
          </div>

          <div className="mt-4 grid gap-3 sm:grid-cols-4 text-sm">
            <MetricCard label="Readiness score" value={String(summaryQuery.data.readinessScore)} />
            <MetricCard label="Level" value={summaryQuery.data.readinessLevel} />
            <MetricCard label="Weakest" value={String(summaryQuery.data.lowestReadinessScore)} />
            <MetricCard label="Average" value={String(summaryQuery.data.averageReadinessScore)} />
          </div>

          {summaryQuery.data.forecasts.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No readiness forecasts match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Pack</th>
                    <th className="px-2 py-2">Score</th>
                    <th className="px-2 py-2">Level</th>
                    <th className="px-2 py-2">Warnings</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.forecasts.map((forecast) => (
                    <tr key={forecast.forecastId} className="border-b border-slate-800/60">
                      <td className="px-2 py-2 text-slate-100">
                        <div>{forecast.packKey}</div>
                        <div className="text-xs text-slate-500">{forecast.summary}</div>
                      </td>
                      <td className="px-2 py-2 text-slate-300">{forecast.readinessScore}</td>
                      <td className="px-2 py-2 text-slate-300">{forecast.readinessLevel}</td>
                      <td className="px-2 py-2 text-slate-300">
                        {forecast.missingEvidenceWarningCount}
                        <span className="ml-2 text-xs text-slate-500">
                          risk {forecast.riskScore} / eff {forecast.effectivenessScore}
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
