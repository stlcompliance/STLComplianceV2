import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import { evaluateRiskScores, getRiskScoreSummary, listRiskScores } from '../api/client'
import type { EvaluateRiskScoresResponse } from '../api/types'

interface RiskScoringPanelProps {
  accessToken: string
  canEvaluate: boolean
}

export function RiskScoringPanel({ accessToken, canEvaluate }: RiskScoringPanelProps) {
  const queryClient = useQueryClient()
  const [scopeKey, setScopeKey] = useState('tenant')
  const [rulePackKey, setRulePackKey] = useState('')
  const [purchaseRequestId, setPurchaseRequestId] = useState('')
  const [lastResult, setLastResult] = useState<EvaluateRiskScoresResponse | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-risk-score-summary', accessToken],
    queryFn: () => getRiskScoreSummary(accessToken),
  })

  const scoresQuery = useQuery({
    queryKey: ['compliancecore-risk-scores', accessToken, scopeKey, rulePackKey],
    queryFn: () =>
      listRiskScores(accessToken, {
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
      return evaluateRiskScores(accessToken, {
        scopeKey: scopeKey.trim() || undefined,
        rulePackKey: rulePackKey.trim() || undefined,
        context: Object.keys(context).length > 0 ? context : undefined,
      })
    },
    onSuccess: (result) => {
      setLastResult(result)
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-risk-score-summary'] })
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-risk-scores'] })
    },
  })

  const summary = summaryQuery.data

  return (
    <section
      data-testid="risk-scoring-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Risk scoring</h2>
        <p className="mt-1 text-sm text-slate-400">
          Scores published rule packs using product fact mirrors and internal rule evaluation for a tenant
          scope (0–100, low through critical).
        </p>
      </header>

      {summary && summary.totalScores > 0 && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Highest</p>
            <p className="mt-1 text-2xl font-semibold text-rose-300">{summary.highestRiskScore}</p>
            <p className="text-xs text-slate-500">{summary.highestRiskLevel}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Critical</p>
            <p className="mt-1 text-2xl font-semibold text-rose-400">{summary.criticalCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">High</p>
            <p className="mt-1 text-2xl font-semibold text-amber-300">{summary.highCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Medium</p>
            <p className="mt-1 text-2xl font-semibold text-yellow-200">{summary.mediumCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Low</p>
            <p className="mt-1 text-2xl font-semibold text-emerald-300">{summary.lowCount}</p>
          </div>
        </div>
      )}

      {canEvaluate ? (
        <div className="space-y-3 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <div className="flex flex-wrap gap-3">
            <label className="flex flex-col gap-1 text-sm text-slate-400">
              Scope key
              <input
                type="text"
                value={scopeKey}
                onChange={(event) => setScopeKey(event.target.value)}
                placeholder="tenant"
                className="rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 font-mono text-sm text-slate-200"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-400">
              Rule pack key (optional)
              <input
                type="text"
                value={rulePackKey}
                onChange={(event) => setRulePackKey(event.target.value)}
                placeholder="All published packs"
                className="rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 font-mono text-sm text-slate-200"
              />
            </label>
            <label className="flex flex-col gap-1 text-sm text-slate-400">
              Purchase request ID (context)
              <input
                type="text"
                value={purchaseRequestId}
                onChange={(event) => setPurchaseRequestId(event.target.value)}
                placeholder="Optional GUID for scope resolution"
                className="rounded-md border border-slate-700 bg-slate-950 px-2 py-1.5 font-mono text-sm text-slate-200"
              />
            </label>
          </div>
          <button
            type="button"
            onClick={() => evaluateMutation.mutate()}
            disabled={evaluateMutation.isPending}
            data-testid="risk-scoring-evaluate"
            className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {evaluateMutation.isPending ? 'Scoring…' : 'Evaluate risk scores'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-400">
          Risk score evaluation requires compliance admin, compliance reviewer, or tenant admin.
        </p>
      )}

      {lastResult && (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Run <span className="font-mono text-violet-300">{lastResult.runId}</span> — scope{' '}
            <span className="font-mono">{lastResult.scopeKey}</span> — highest{' '}
            {lastResult.highestRiskScore} ({lastResult.highestRiskLevel}), {lastResult.mirrorFactCount}{' '}
            mirror fact(s)
          </p>
        </div>
      )}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Latest scores</h3>
        {(scoresQuery.data ?? []).length === 0 ? (
          <p className="mt-2 text-sm text-slate-500" data-testid="risk-scoring-list-empty">
            No risk scores yet. Run an evaluation to populate scores.
          </p>
        ) : (
          <ul className="mt-3 max-h-80 space-y-2 overflow-y-auto" data-testid="risk-scoring-list">
            {(scoresQuery.data ?? []).map((score) => (
              <li
                key={score.riskScoreId}
                className="rounded border border-slate-800 px-3 py-2 text-sm text-slate-300"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <span className="text-lg font-semibold text-rose-300">{score.riskScore}</span>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase">{score.riskLevel}</span>
                  <span className="font-mono text-xs text-sky-300">{score.packKey}</span>
                  <span className="font-mono text-xs text-slate-500">{score.scopeKey}</span>
                </div>
                <p className="mt-1 text-xs text-slate-500">{score.summary}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
