import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  evaluateReadinessForecast,
  getReadinessForecastSummary,
  listReadinessForecasts,
} from '../api/client'
import type { EvaluateReadinessForecastResponse } from '../api/types'
import { M12AnalyticsScopeFilters } from './M12AnalyticsScopeFilters'

interface ReadinessForecastPanelProps {
  accessToken: string
  canEvaluate: boolean
}

export function ReadinessForecastPanel({ accessToken, canEvaluate }: ReadinessForecastPanelProps) {
  const queryClient = useQueryClient()
  const [scopeKey, setScopeKey] = useState('tenant')
  const [rulePackKey, setRulePackKey] = useState('')
  const [purchaseRequestId, setPurchaseRequestId] = useState('')
  const [lastResult, setLastResult] = useState<EvaluateReadinessForecastResponse | null>(null)

  const summaryQuery = useQuery({
    queryKey: ['compliancecore-readiness-forecast-summary', accessToken],
    queryFn: () => getReadinessForecastSummary(accessToken),
  })

  const forecastsQuery = useQuery({
    queryKey: ['compliancecore-readiness-forecasts', accessToken, scopeKey, rulePackKey],
    queryFn: () =>
      listReadinessForecasts(accessToken, {
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
      return evaluateReadinessForecast(accessToken, {
        scopeKey: scopeKey.trim() || undefined,
        rulePackKey: rulePackKey.trim() || undefined,
        context: Object.keys(context).length > 0 ? context : undefined,
      })
    },
    onSuccess: (result) => {
      setLastResult(result)
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-readiness-forecast-summary'] })
      void queryClient.invalidateQueries({ queryKey: ['compliancecore-readiness-forecasts'] })
    },
  })

  const summary = summaryQuery.data

  return (
    <section
      data-testid="readiness-forecast-panel"
      className="space-y-4 rounded-xl border border-slate-700 bg-slate-900/80 p-5"
    >
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Readiness forecasting</h2>
        <p className="mt-1 text-sm text-slate-400">
          Forecasts operational readiness at a scope by combining risk scores, missing-evidence
          warnings, and control effectiveness (ready, caution, not ready, unknown).
        </p>
      </header>

      {summary && summary.totalForecasts > 0 && (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Forecast</p>
            <p className="mt-1 text-2xl font-semibold text-emerald-300">{summary.readinessScore}</p>
            <p className="text-xs text-[var(--color-text-muted)]">{summary.readinessLevel}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Ready</p>
            <p className="mt-1 text-2xl font-semibold text-emerald-300">{summary.readyCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Caution</p>
            <p className="mt-1 text-2xl font-semibold text-yellow-200">{summary.cautionCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Not ready</p>
            <p className="mt-1 text-2xl font-semibold text-rose-300">{summary.notReadyCount}</p>
          </div>
          <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-3">
            <p className="text-xs uppercase text-[var(--color-text-muted)]">Weakest</p>
            <p className="mt-1 text-2xl font-semibold text-amber-300">
              {summary.lowestReadinessScore}
            </p>
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
            data-testid="readiness-forecast-evaluate"
            className="rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {evaluateMutation.isPending ? 'Forecasting…' : 'Forecast readiness'}
          </button>
        </div>
      ) : (
        <p className="text-sm text-slate-400">
          Readiness forecasting requires compliance admin, compliance reviewer, or tenant admin.
        </p>
      )}

      {lastResult && (
        <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4 text-sm text-slate-300">
          <p>
            Run <span className="font-mono text-violet-300">{lastResult.runId}</span> — scope{' '}
            <span className="font-mono">{lastResult.scopeKey}</span> — forecast{' '}
            {lastResult.readinessScore} ({lastResult.readinessLevel}), risk peak{' '}
            {lastResult.highestRiskScore}, {lastResult.missingEvidenceWarningCount} warning(s)
          </p>
        </div>
      )}

      <div className="rounded-lg border border-slate-800 bg-slate-950/50 p-4">
        <h3 className="text-sm font-medium text-slate-200">Latest forecasts</h3>
        {(forecastsQuery.data ?? []).length === 0 ? (
          <p
            className="mt-2 text-sm text-[var(--color-text-muted)]"
            data-testid="readiness-forecast-list-empty"
          >
            No forecasts yet. Run a forecast to combine risk, evidence, and control signals.
          </p>
        ) : (
          <ul
            className="mt-3 max-h-80 space-y-2 overflow-y-auto"
            data-testid="readiness-forecast-list"
          >
            {(forecastsQuery.data ?? []).map((forecast) => (
              <li
                key={forecast.forecastId}
                className="rounded border border-slate-800 px-3 py-2 text-sm text-slate-300"
              >
                <div className="flex flex-wrap items-center gap-2">
                  <span className="text-lg font-semibold text-emerald-300">
                    {forecast.readinessScore}
                  </span>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase">
                    {forecast.readinessLevel}
                  </span>
                  <span className="font-mono text-xs text-sky-300">{forecast.packKey}</span>
                  <span className="text-xs text-rose-300">risk {forecast.riskScore}</span>
                  <span className="text-xs text-amber-200">
                    eff {forecast.effectivenessScore}
                  </span>
                </div>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">{forecast.summary}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
