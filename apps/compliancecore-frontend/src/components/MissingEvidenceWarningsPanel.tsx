import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  evaluateMissingEvidenceWarnings,
  getMissingEvidenceWarningSummary,
  listMissingEvidenceWarnings,
} from '../api/client'
import type { EvaluateMissingEvidenceWarningsResponse } from '../api/types'
import { M12AnalyticsScopeFilters } from './M12AnalyticsScopeFilters'

interface MissingEvidenceWarningsPanelProps {
  accessToken: string
  canEvaluate: boolean
}

export function MissingEvidenceWarningsPanel({
  accessToken,
  canEvaluate,
}: MissingEvidenceWarningsPanelProps) {
  const queryClient = useQueryClient()
  const [scopeKey, setScopeKey] = useState('tenant')
  const [rulePackKey, setRulePackKey] = useState('')
  const [purchaseRequestId, setPurchaseRequestId] = useState('')
  const [lastResult, setLastResult] = useState<EvaluateMissingEvidenceWarningsResponse | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-missing-evidence-summary', accessToken],
    queryFn: () => getMissingEvidenceWarningSummary(accessToken),
  })

  const warningsQuery = useQuery({
    queryKey: ['compliancecore-missing-evidence-warnings', accessToken, scopeKey, rulePackKey],
    queryFn: () =>
      listMissingEvidenceWarnings(accessToken, {
        scopeKey: scopeKey.trim() || undefined,
        rulePackKey: rulePackKey.trim() || undefined,
        limit: 25,
      }),
  })

  const evaluateMutation = useMutation({
    mutationFn: () => {
      const context: Record<string, string> = {}
      if (purchaseRequestId.trim()) {
        context.purchase_request_id = purchaseRequestId.trim()
      }
      return evaluateMissingEvidenceWarnings(accessToken, {
        scopeKey: scopeKey.trim() || undefined,
        rulePackKey: rulePackKey.trim() || undefined,
        context: Object.keys(context).length > 0 ? context : undefined,
      })
    },
    onSuccess: (result) => {
      setLastResult(result)
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-missing-evidence-summary'] })
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-missing-evidence-warnings'] })
    },
  })

  const summary = summaryQuery.data

  return (
    <section
      data-testid="missing-evidence-warnings-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Missing evidence warnings</h2>
        <p className="mt-1 text-sm text-slate-400">
          Predicts likely missing evidence at a scope by comparing published rule pack facts, catalog
          requirements, and product fact mirrors before evaluation fails.
        </p>
      </header>

      {summary && summary.totalWarnings > 0 && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Highest</p>
            <p className="mt-1 text-2xl font-semibold text-rose-300">{summary.highestSeverity}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Critical</p>
            <p className="mt-1 text-2xl font-semibold text-rose-400">{summary.criticalCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">High</p>
            <p className="mt-1 text-2xl font-semibold text-amber-300">{summary.highCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Medium</p>
            <p className="mt-1 text-2xl font-semibold text-yellow-200">{summary.mediumCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Low</p>
            <p className="mt-1 text-2xl font-semibold text-emerald-300">{summary.lowCount}</p>
          </div>
        </div>
      )}

      {canEvaluate ? (
        <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <M12AnalyticsScopeFilters
            accessToken={accessToken}
            scopeKey={scopeKey}
            onScopeKeyChange={setScopeKey}
            rulePackKey={rulePackKey}
            onRulePackKeyChange={setRulePackKey}
            purchaseRequestId={purchaseRequestId}
            onPurchaseRequestIdChange={setPurchaseRequestId}
          />
          <button
            type="button"
            onClick={() => evaluateMutation.mutate()}
            disabled={evaluateMutation.isPending}
            data-testid="missing-evidence-evaluate"
            className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {evaluateMutation.isPending ? 'Analyzing…' : 'Evaluate missing evidence'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-400">
          Missing evidence evaluation requires compliance admin, compliance reviewer, or tenant admin.
        </p>
      )}

      {lastResult && (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Run <span className="font-mono text-violet-300">{lastResult.runId}</span> — scope{' '}
            <span className="font-mono">{lastResult.scopeKey}</span> — {lastResult.warningsEmittedCount}{' '}
            warning(s), highest {lastResult.highestSeverity}, {lastResult.mirrorFactCount} mirror fact(s)
          </p>
        </div>
      )}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Latest warnings</h3>
        {(warningsQuery.data ?? []).length === 0 ? (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="missing-evidence-list-empty">
            No warnings yet. Run an evaluation to predict missing evidence at scope.
          </p>
        ) : (
          <ul
            className="mt-3 max-h-80 space-y-2 overflow-y-auto"
            data-testid="missing-evidence-list"
          >
            {(warningsQuery.data ?? []).map((warning) => (
              <li
                key={warning.warningId}
                className="rounded border border-slate-800 px-3 py-2 text-sm text-slate-300"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase text-rose-200">
                    {warning.severity}
                  </span>
                  <span className="font-mono text-xs text-sky-300">{warning.packKey}</span>
                  <span className="font-mono text-xs text-amber-200">{warning.factKey}</span>
                  <span className="text-xs text-[var(--color-text-muted)]">{warning.reasonCode}</span>
                </div>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">{warning.summary}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
