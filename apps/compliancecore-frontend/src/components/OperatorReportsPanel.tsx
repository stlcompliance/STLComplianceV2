import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import {
  exportOperatorReportSummaryCsv,
  getOperatorReportSummary,
} from '../api/client'

interface OperatorReportsPanelProps {
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

export function OperatorReportsPanel({
  accessToken,
  canRead,
  canExport,
}: OperatorReportsPanelProps) {
  const [attentionOnly, setAttentionOnly] = useState(false)

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-operator-report-summary', accessToken, attentionOnly],
    queryFn: () => getOperatorReportSummary(accessToken, { attentionOnly }),
    enabled: canRead,
  })

  const exportMutation = useMutation({
    mutationFn: () => exportOperatorReportSummaryCsv(accessToken, { attentionOnly }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `compliancecore-operator-report-${new Date().toISOString().slice(0, 10)}.csv`
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
      data-testid="operator-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Operator reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Rule evaluation runs, workflow gate outcomes, and rule pack lifecycle rollups.
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
        <label htmlFor="operator-reports-attention-only" className="flex items-center gap-2">
          <input
            id="operator-reports-attention-only"
            type="checkbox"
            checked={attentionOnly}
            onChange={(event) => setAttentionOnly(event.target.checked)}
          />
          Show attention-only metrics
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-400">Loading operator report summary…</p>
      )}

      {summaryQuery.isError && (
        <p className="mt-3 text-sm text-red-300">Failed to load operator report summary.</p>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
            <MetricCard label="Evaluations" value={String(summaryQuery.data.evaluationTotalCount)} />
            <MetricCard label="Failed evaluations" value={String(summaryQuery.data.evaluationFailCount)} />
            <MetricCard label="Gate blocks" value={String(summaryQuery.data.workflowGateBlockCount)} />
            <MetricCard label="Published rule packs" value={String(summaryQuery.data.rulePackPublishedCount)} />
          </div>

          {summaryQuery.data.recentEvaluations.length === 0 ? (
            <p className="mt-4 text-sm text-slate-400">No evaluations match this filter.</p>
          ) : (
            <div className="mt-4 overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-slate-700 text-left text-slate-400">
                    <th className="px-2 py-2">Rule pack</th>
                    <th className="px-2 py-2">Result</th>
                    <th className="px-2 py-2">Evaluated</th>
                  </tr>
                </thead>
                <tbody>
                  {summaryQuery.data.recentEvaluations.slice(0, 8).map((item) => (
                    <tr key={item.evaluationRunId} className="border-b border-slate-800/60">
                      <td className="px-2 py-2 text-slate-100">{item.rulePackLabel}</td>
                      <td className="px-2 py-2 text-slate-300">{item.overallResult}</td>
                      <td className="px-2 py-2 text-slate-300">
                        {new Date(item.createdAt).toLocaleDateString()}
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
