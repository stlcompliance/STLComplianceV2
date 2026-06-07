import { useMutation, useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { Download } from 'lucide-react'
import { useState } from 'react'

import { exportRuleChangeImpactReportSummaryCsv, getRuleChangeImpactReportSummary } from '../api/client'

interface RuleChangeImpactReportPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

function MetricCard({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="rounded-lg border border-slate-700 bg-slate-950/50 p-3">
      <p className="text-xs uppercase text-slate-500">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-slate-50">{value}</p>
    </div>
  )
}

export function RuleChangeImpactReportPanel({
  accessToken,
  canRead,
  canExport,
}: RuleChangeImpactReportPanelProps) {
  const [packKey, setPackKey] = useState('')

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-rule-change-impact-report-summary', accessToken, packKey],
    queryFn: () => getRuleChangeImpactReportSummary(accessToken, { packKey: packKey.trim() || undefined }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () =>
      exportRuleChangeImpactReportSummaryCsv(accessToken, { packKey: packKey.trim() || undefined }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-rule-change-impact-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) return null

  return (
    <section
      data-testid="rule-change-impact-report-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Rule change impact report</h2>
          <p className="mt-1 text-sm text-slate-400">
            Summarizes which rule packs were impacted by changes, and how many evaluations, findings,
            and waivers were involved.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="inline-flex items-center gap-2 rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            <Download className="h-4 w-4" />
            {exportMutation.isPending ? 'Exporting…' : 'Export CSV'}
          </button>
        ) : null}
      </div>

      <label className="block text-sm text-slate-300">
        Rule pack key filter
        <input
          value={packKey}
          onChange={(event) => setPackKey(event.target.value)}
          placeholder="optional pack key"
          className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
        />
      </label>

      {summaryQuery.isLoading ? <p className="text-sm text-slate-400">Loading impact report…</p> : null}

      {summaryQuery.isError ? (
        <ApiErrorCallout
          title="Rule change impact report unavailable"
          message={getErrorMessage(
            summaryQuery.error,
            'Failed to load rule change impact report summary.',
          )}
          retryLabel="Retry summary"
          onRetry={() => {
            void summaryQuery.refetch()
          }}
        />
      ) : null}

      {exportMutation.isError ? (
        <ApiErrorCallout
          title="CSV export failed"
          message={getErrorMessage(
            exportMutation.error,
            'Unable to export rule change impact report CSV.',
          )}
        />
      ) : null}

      {summaryQuery.data ? (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
            <MetricCard label="Impacted packs" value={summaryQuery.data.totalImpactedRulePacks} />
            <MetricCard label="Change events" value={summaryQuery.data.totalChangeEvents} />
            <MetricCard label="Evaluations" value={summaryQuery.data.totalEvaluationRuns} />
            <MetricCard label="Findings" value={summaryQuery.data.totalFindings} />
            <MetricCard label="Waivers" value={summaryQuery.data.totalWaivers} />
          </div>

          {summaryQuery.data.rulePacks.length === 0 ? (
            <p className="text-sm text-slate-400">No impacted rule packs match the current filter.</p>
          ) : (
            <div className="space-y-2">
              {summaryQuery.data.rulePacks.map((item) => (
                <div
                  key={item.rulePackId}
                  className="rounded-lg border border-slate-700 bg-slate-950/60 p-4"
                >
                  <div className="flex flex-wrap items-center justify-between gap-2">
                    <div>
                      <p className="font-semibold text-slate-50">{item.packKey}</p>
                      <p className="text-xs text-slate-500">{item.programKey}</p>
                    </div>
                    <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase text-slate-200">
                      {item.latestChangeType}
                    </span>
                  </div>
                  <p className="mt-2 text-sm text-slate-300">{item.latestSummary}</p>
                  <div className="mt-3 grid gap-2 text-xs text-slate-500 sm:grid-cols-2 lg:grid-cols-4">
                    <p>Change events {item.changeEventCount}</p>
                    <p>Evaluations {item.evaluationRunCount}</p>
                    <p>Findings {item.findingCount}</p>
                    <p>Waivers {item.waiverCount}</p>
                  </div>
                  <p className="mt-2 text-xs text-slate-600">
                    Latest change {new Date(item.latestChangedAt).toLocaleString()}
                  </p>
                </div>
              ))}
            </div>
          )}
        </>
      ) : null}
    </section>
  )
}
