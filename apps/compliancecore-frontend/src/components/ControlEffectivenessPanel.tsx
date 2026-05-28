import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  evaluateControlEffectiveness,
  getControlEffectivenessSummary,
  listControlEffectivenessRecords,
} from '../api/client'
import type { EvaluateControlEffectivenessResponse } from '../api/types'

interface ControlEffectivenessPanelProps {
  accessToken: string
  canEvaluate: boolean
}

export function ControlEffectivenessPanel({
  accessToken,
  canEvaluate,
}: ControlEffectivenessPanelProps) {
  const queryClient = useQueryClient()
  const [scopeKey, setScopeKey] = useState('tenant')
  const [rulePackKey, setRulePackKey] = useState('')
  const [purchaseRequestId, setPurchaseRequestId] = useState('')
  const [lastResult, setLastResult] = useState<EvaluateControlEffectivenessResponse | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-control-effectiveness-summary', accessToken],
    queryFn: () => getControlEffectivenessSummary(accessToken),
  })

  const recordsQuery = useQuery({
    queryKey: ['compliancecore-control-effectiveness-records', accessToken, scopeKey, rulePackKey],
    queryFn: () =>
      listControlEffectivenessRecords(accessToken, {
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
      return evaluateControlEffectiveness(accessToken, {
        scopeKey: scopeKey.trim() || undefined,
        rulePackKey: rulePackKey.trim() || undefined,
        context: Object.keys(context).length > 0 ? context : undefined,
      })
    },
    onSuccess: (result) => {
      setLastResult(result)
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-control-effectiveness-summary'] })
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-control-effectiveness-records'] })
    },
  })

  const summary = summaryQuery.data

  return (
    <section className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Control effectiveness</h2>
        <p className="mt-1 text-sm text-slate-400">
          Tracks how effectively published rule packs (compliance controls) perform at a scope using
          real rule evaluation outcomes (0–100, effective through unknown).
        </p>
      </header>

      {summary && summary.totalControls > 0 && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Average</p>
            <p className="mt-1 text-2xl font-semibold text-emerald-300">
              {summary.averageEffectivenessScore}
            </p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Effective</p>
            <p className="mt-1 text-2xl font-semibold text-emerald-300">{summary.effectiveCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Partial</p>
            <p className="mt-1 text-2xl font-semibold text-yellow-200">
              {summary.partiallyEffectiveCount}
            </p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Ineffective</p>
            <p className="mt-1 text-2xl font-semibold text-rose-300">{summary.ineffectiveCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-slate-500">Weakest</p>
            <p className="mt-1 text-2xl font-semibold text-amber-300">
              {summary.lowestEffectivenessScore}
            </p>
            <p className="text-xs text-slate-500">{summary.lowestEffectivenessLevel}</p>
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
            className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {evaluateMutation.isPending ? 'Evaluating…' : 'Evaluate control effectiveness'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-400">
          Control effectiveness evaluation requires compliance admin, compliance reviewer, or tenant
          admin.
        </p>
      )}

      {lastResult && (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Run <span className="font-mono text-violet-300">{lastResult.runId}</span> — scope{' '}
            <span className="font-mono">{lastResult.scopeKey}</span> — average{' '}
            {lastResult.averageEffectivenessScore}, weakest {lastResult.lowestEffectivenessScore} (
            {lastResult.lowestEffectivenessLevel})
          </p>
        </div>
      )}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Latest control records</h3>
        {(recordsQuery.data ?? []).length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">
            No effectiveness records yet. Run an evaluation to track controls at scope.
          </p>
        ) : (
          <ul className="mt-3 max-h-80 space-y-2 overflow-y-auto">
            {(recordsQuery.data ?? []).map((record) => (
              <li
                key={record.recordId}
                className="rounded border border-slate-800 px-3 py-2 text-sm text-slate-300"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <span className="text-lg font-semibold text-emerald-300">
                    {record.effectivenessScore}
                  </span>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase">
                    {record.effectivenessLevel}
                  </span>
                  <span className="rounded bg-slate-900 px-2 py-0.5 text-xs text-sky-300">
                    {record.controlStatus}
                  </span>
                  <span className="font-mono text-xs text-sky-300">{record.packKey}</span>
                </div>
                <p className="mt-1 text-xs text-slate-500">{record.summary}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
