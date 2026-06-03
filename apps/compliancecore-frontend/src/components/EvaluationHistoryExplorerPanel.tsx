import { useMutation } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getRuleEvaluationAuditExport } from '../api/client'
import type {
  RuleEvaluationAuditExportResponse,
  RuleEvaluationRunResponse,
  RulePackResponse,
} from '../api/types'

interface EvaluationHistoryExplorerPanelProps {
  accessToken: string
  rulePacks: RulePackResponse[]
  evaluationRuns: RuleEvaluationRunResponse[]
  canExportAudit: boolean
  onFocusRulePack: (rulePackId: string) => void
}

function resultBadgeClass(result: string): string {
  const normalized = result.toLowerCase()
  if (normalized === 'pass' || normalized === 'allow') {
    return 'bg-emerald-900/60 text-emerald-200'
  }
  if (normalized === 'warn' || normalized === 'review') {
    return 'bg-amber-900/60 text-amber-100'
  }
  return 'bg-red-900/60 text-red-200'
}

function MetricCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
      <p className="text-xs text-slate-400">{label}</p>
      <p className="text-lg font-semibold text-slate-50">{value}</p>
    </div>
  )
}

function formatAuditExportSummary(exportResponse: RuleEvaluationAuditExportResponse) {
  return [
    `${exportResponse.workflowGateChecks.length} gate checks`,
    `${exportResponse.findings.length} findings`,
    `${exportResponse.waivers.length} waivers`,
  ].join(' · ')
}

export function EvaluationHistoryExplorerPanel({
  accessToken,
  rulePacks,
  evaluationRuns,
  canExportAudit,
  onFocusRulePack,
}: EvaluationHistoryExplorerPanelProps) {
  const [searchTerm, setSearchTerm] = useState('')
  const [rulePackFilter, setRulePackFilter] = useState('all')
  const [resultFilter, setResultFilter] = useState('all')
  const [selectedRunId, setSelectedRunId] = useState('')
  const [lastAuditExport, setLastAuditExport] = useState<RuleEvaluationAuditExportResponse | null>(null)

  const packOptions = useMemo(() => {
    const options = new Map<string, string>()
    for (const pack of rulePacks) {
      options.set(pack.rulePackId, `${pack.label} (${pack.packKey})`)
    }
    for (const run of evaluationRuns) {
      if (!options.has(run.rulePackId)) {
        options.set(run.rulePackId, `${run.packLabel} (${run.packKey})`)
      }
    }
    return Array.from(options.entries()).sort((left, right) => left[1].localeCompare(right[1]))
  }, [evaluationRuns, rulePacks])

  const filteredRuns = useMemo(() => {
    const normalizedSearch = searchTerm.trim().toLowerCase()
    return [...evaluationRuns]
      .sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt))
      .filter((run) => {
        if (rulePackFilter !== 'all' && run.rulePackId !== rulePackFilter) {
          return false
        }
        if (resultFilter !== 'all' && run.overallResult.toLowerCase() !== resultFilter.toLowerCase()) {
          return false
        }
        if (!normalizedSearch) {
          return true
        }
        const searchableText = [
          run.packLabel,
          run.packKey,
          run.evaluationRunId,
          run.overallResult,
          run.status,
          JSON.stringify(run.factInputs),
          run.ruleResults.map((item) => `${item.ruleKey} ${item.label} ${item.message}`).join(' '),
        ]
          .join(' ')
          .toLowerCase()
        return searchableText.includes(normalizedSearch)
      })
  }, [evaluationRuns, resultFilter, rulePackFilter, searchTerm])

  useEffect(() => {
    if (filteredRuns.length === 0) {
      if (selectedRunId) {
        setSelectedRunId('')
      }
      return
    }

    if (!filteredRuns.some((run) => run.evaluationRunId === selectedRunId)) {
      setSelectedRunId(filteredRuns[0].evaluationRunId)
    }
  }, [filteredRuns, selectedRunId])

  useEffect(() => {
    setLastAuditExport(null)
  }, [selectedRunId])

  const selectedRun = filteredRuns.find((run) => run.evaluationRunId === selectedRunId) ?? null

  const auditExportMutation = useMutation({
    mutationFn: async () => {
      if (!selectedRun) {
        return null
      }
      return getRuleEvaluationAuditExport(accessToken, selectedRun.evaluationRunId)
    },
    onSuccess: (result) => {
      if (result) {
        setLastAuditExport(result)
      }
    },
  })

  const totalRuns = filteredRuns.length
  const passRuns = filteredRuns.filter((run) => run.overallResult.toLowerCase() === 'pass').length
  const warnRuns = filteredRuns.filter((run) => {
    const normalized = run.overallResult.toLowerCase()
    return normalized === 'warn' || normalized === 'review'
  }).length
  const blockRuns = filteredRuns.filter((run) => {
    const normalized = run.overallResult.toLowerCase()
    return normalized === 'block' || normalized === 'fail'
  }).length

  const latestRun = filteredRuns[0] ?? null

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4" data-testid="evaluation-history-explorer-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Evaluation history explorer</h2>
          <p className="mt-1 text-sm text-slate-400">
            Search all evaluation runs, inspect the evidence trail, and jump back to the associated rule pack.
          </p>
        </div>
        {canExportAudit ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-xs font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={!selectedRun || auditExportMutation.isPending}
            onClick={() => auditExportMutation.mutate()}
          >
            {auditExportMutation.isPending ? 'Loading export…' : 'Load audit export snapshot'}
          </button>
        ) : (
          <p className="rounded-md border border-slate-700 bg-slate-950 px-3 py-1.5 text-xs text-slate-400">
            Audit export permissions are required to load the snapshot.
          </p>
        )}
      </div>

      <div className="mt-4 grid gap-3 lg:grid-cols-4">
        <label htmlFor="evaluation-history-search" className="text-xs text-slate-400">
          Search
          <input
            id="evaluation-history-search"
            value={searchTerm}
            onChange={(event) => setSearchTerm(event.target.value)}
            placeholder="Rule pack, fact, result, or run id"
            className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100 placeholder:text-slate-600"
          />
        </label>
        <label htmlFor="evaluation-history-pack-filter" className="text-xs text-slate-400">
          Rule pack
          <select
            id="evaluation-history-pack-filter"
            value={rulePackFilter}
            onChange={(event) => setRulePackFilter(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          >
            <option value="all">All packs</option>
            {packOptions.map(([rulePackId, label]) => (
              <option key={rulePackId} value={rulePackId}>
                {label}
              </option>
            ))}
          </select>
        </label>
        <label htmlFor="evaluation-history-result-filter" className="text-xs text-slate-400">
          Result
          <select
            id="evaluation-history-result-filter"
            value={resultFilter}
            onChange={(event) => setResultFilter(event.target.value)}
            className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          >
            <option value="all">All results</option>
            <option value="pass">Pass</option>
            <option value="warn">Warn</option>
            <option value="block">Block</option>
          </select>
        </label>
        <div className="grid gap-3 sm:grid-cols-3 lg:grid-cols-1">
          <MetricCard label="Matching runs" value={String(totalRuns)} />
          <MetricCard label="Passing" value={String(passRuns)} />
          <MetricCard label="Blocking" value={String(blockRuns)} />
        </div>
      </div>

      <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Warnings" value={String(warnRuns)} />
        <MetricCard label="Latest run" value={latestRun ? new Date(latestRun.createdAt).toLocaleString() : 'None'} />
        <MetricCard label="Selected run" value={selectedRun ? selectedRun.packLabel : 'None'} />
        <MetricCard label="Selected result" value={selectedRun ? selectedRun.overallResult : 'None'} />
      </div>

      {filteredRuns.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">No evaluation runs match the current filters.</p>
      ) : (
        <div className="mt-4 grid gap-4 xl:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
          <div className="space-y-2">
            {filteredRuns.map((run) => {
              const isSelected = run.evaluationRunId === selectedRunId
              return (
                <button
                  key={run.evaluationRunId}
                  type="button"
                  onClick={() => setSelectedRunId(run.evaluationRunId)}
                  className={`w-full rounded-lg border px-3 py-3 text-left transition ${
                    isSelected
                      ? 'border-sky-500 bg-sky-950/40'
                      : 'border-slate-700 bg-slate-950/60 hover:border-slate-500'
                  }`}
                >
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <p className="text-sm font-medium text-slate-100">{run.packLabel}</p>
                      <p className="mt-1 text-xs text-slate-500">
                        {run.packKey} · v{run.versionNumber} · {new Date(run.createdAt).toLocaleString()}
                      </p>
                    </div>
                    <span className={`rounded px-2 py-0.5 text-xs uppercase ${resultBadgeClass(run.overallResult)}`}>
                      {run.overallResult}
                    </span>
                  </div>
                  <p className="mt-2 line-clamp-2 text-xs text-slate-400">
                    {Object.entries(run.factInputs)
                      .map(([factKey, value]) => `${factKey}=${value ? 'true' : 'false'}`)
                      .join(' · ')}
                  </p>
                </button>
              )
            })}
          </div>

          <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-4">
            {selectedRun ? (
              <div className="space-y-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <h3 className="text-base font-semibold text-slate-50">{selectedRun.packLabel}</h3>
                    <p className="mt-1 text-xs text-slate-500">
                      {selectedRun.packKey} · v{selectedRun.versionNumber} · {new Date(selectedRun.createdAt).toLocaleString()}
                    </p>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <span className={`rounded px-2 py-0.5 text-xs uppercase ${resultBadgeClass(selectedRun.overallResult)}`}>
                      {selectedRun.overallResult}
                    </span>
                    <button
                      type="button"
                      onClick={() => onFocusRulePack(selectedRun.rulePackId)}
                      className="rounded-md border border-slate-600 px-2 py-1 text-xs text-slate-200 hover:border-slate-400"
                    >
                      Focus pack
                    </button>
                  </div>
                </div>

                <div>
                  <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-400">Fact inputs</h4>
                  <ul className="mt-2 flex flex-wrap gap-2">
                    {Object.entries(selectedRun.factInputs).map(([factKey, value]) => (
                      <li key={factKey} className="rounded-md border border-slate-700 bg-slate-900 px-2 py-1 text-xs text-slate-200">
                        <span className="font-mono text-sky-300">{factKey}</span>={value ? 'true' : 'false'}
                      </li>
                    ))}
                  </ul>
                </div>

                <div>
                  <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-400">Rule results</h4>
                  <ul className="mt-2 space-y-2">
                    {selectedRun.ruleResults.map((item) => (
                      <li key={item.ruleKey} className="rounded-md border border-slate-700 bg-slate-900/80 px-3 py-2 text-sm">
                        <div className="flex items-center justify-between gap-2">
                          <span className="font-mono text-xs text-sky-300">{item.ruleKey}</span>
                          <span className={`rounded px-2 py-0.5 text-xs uppercase ${resultBadgeClass(item.result)}`}>
                            {item.result}
                          </span>
                        </div>
                        <p className="mt-1 text-slate-100">{item.label}</p>
                        <p className="mt-1 text-xs text-slate-400">{item.message}</p>
                      </li>
                    ))}
                  </ul>
                </div>

                <div className="grid gap-3 sm:grid-cols-3">
                  <MetricCard label="Facts tracked" value={String(Object.keys(selectedRun.factInputs).length)} />
                  <MetricCard label="Rule results" value={String(selectedRun.ruleResults.length)} />
                  <MetricCard label="Findings emitted" value={String(selectedRun.findingsEmitted?.length ?? 0)} />
                </div>

                {auditExportMutation.isError ? (
                  <ApiErrorCallout
                    title="Audit export snapshot unavailable"
                    message={getErrorMessage(
                      auditExportMutation.error,
                      'Failed to load the rule evaluation audit export snapshot.',
                    )}
                  />
                ) : null}

                {lastAuditExport ? (
                  <div className="rounded-lg border border-slate-700 bg-slate-900/70 p-3">
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <h4 className="text-sm font-semibold text-slate-50">Audit export snapshot</h4>
                        <p className="mt-1 text-xs text-slate-500">
                          Export {lastAuditExport.exportId} · {new Date(lastAuditExport.generatedAt).toLocaleString()}
                        </p>
                      </div>
                      <p className="text-xs text-slate-400">{formatAuditExportSummary(lastAuditExport)}</p>
                    </div>
                    <div className="mt-3 grid gap-3 sm:grid-cols-2">
                      <MetricCard label="Exported run" value={lastAuditExport.evaluationRun.packKey} />
                      <MetricCard label="Gate checks" value={String(lastAuditExport.workflowGateChecks.length)} />
                    </div>
                    <p className="mt-3 text-xs text-slate-400">
                      Audit export contains the exact evaluation run record plus workflow gate checks, findings, and waivers
                      used for downstream evidence review.
                    </p>
                  </div>
                ) : null}

                {canExportAudit && (
                  <button
                    type="button"
                    onClick={() => auditExportMutation.mutate()}
                    disabled={auditExportMutation.isPending}
                    className="rounded-md bg-sky-700 px-3 py-1.5 text-xs font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                  >
                    {auditExportMutation.isPending ? 'Loading export…' : 'Refresh audit export snapshot'}
                  </button>
                )}
              </div>
            ) : (
              <p className="text-sm text-slate-400">Select a run to inspect its rule results and export snapshot.</p>
            )}
          </div>
        </div>
      )}
    </section>
  )
}
