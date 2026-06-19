import { Link, useLocation } from 'react-router-dom'
import { EvaluationHistoryExplorerPanel } from '../../components/EvaluationHistoryExplorerPanel'
import { RuleEvaluationPanel } from '../../components/RuleEvaluationPanel'
import { SituationEvaluatorPanel } from '../../components/SituationEvaluatorPanel'
import type { RuleEvaluationRunResponse } from '../../api/types'
import type { ComplianceCoreWorkspaceState } from '../useComplianceCoreWorkspaceState'

type Props = { state: ComplianceCoreWorkspaceState }

type EvaluationView = 'overview' | 'recent' | 'tester' | 'traces'

const evaluationViews: Array<{ key: EvaluationView; label: string; to: string }> = [
  { key: 'overview', label: 'Overview', to: '/evaluation' },
  { key: 'recent', label: 'Recent Runs', to: '/evaluation/recent' },
  { key: 'tester', label: 'Situation Tester', to: '/evaluation/tester' },
  { key: 'traces', label: 'Calculation Traces', to: '/evaluation/traces' },
]

function evaluationViewFromPath(pathname: string): EvaluationView {
  const segment = pathname.split('/').filter(Boolean)[1]
  if (segment && evaluationViews.some((view) => view.key === segment)) return segment as EvaluationView
  return 'overview'
}

function resultTone(result: string) {
  const normalized = result.toLowerCase()
  if (['pass', 'compliant', 'allow', 'allowed'].includes(normalized)) return 'bg-emerald-950 text-emerald-200'
  if (['warn', 'warning', 'needs_review', 'review'].includes(normalized)) return 'bg-amber-950 text-amber-200'
  return 'bg-red-950 text-red-200'
}

function EvaluationTabs({ activeView }: { activeView: EvaluationView }) {
  return (
    <nav className="flex flex-wrap gap-2" aria-label="Evaluation views">
      {evaluationViews.map((view) => (
        <Link
          key={view.key}
          to={view.to}
          className={`rounded-md border px-3 py-2 text-sm ${
            activeView === view.key
              ? 'border-sky-500 bg-sky-950/50 text-sky-100'
              : 'border-slate-800 bg-slate-950 text-slate-300 hover:border-slate-600'
          }`}
        >
          {view.label}
        </Link>
      ))}
    </nav>
  )
}

function ExplainResultPanel({ latestRun }: { latestRun: RuleEvaluationRunResponse | null }) {
  return (
    <section className="rounded-lg border border-slate-800 bg-slate-950/70 p-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h2 className="text-lg font-semibold text-white">Explain Result</h2>
          <p className="mt-2 max-w-3xl text-sm text-slate-300">
            Every compliance badge, warning, blocker, or recommendation should be explainable by
            rulepack, requirement, applicability, facts, evidence, calculation, and product signal.
          </p>
        </div>
        <Link to="/evaluation/traces" className="rounded-md border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800">
          Open traces
        </Link>
      </div>

      {!latestRun ? (
        <p className="mt-4 rounded-lg border border-slate-800 bg-slate-900/70 p-4 text-sm text-slate-400">
          No evaluation run is available yet. Run an evaluation or use the Situation Tester to create
          a traceable result.
        </p>
      ) : (
        <div className="mt-5 grid gap-4 xl:grid-cols-[0.9fr_1.1fr]">
          <div className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
            <div className="flex flex-wrap items-center gap-2">
              <span className={`rounded-md px-2 py-1 text-xs font-semibold uppercase ${resultTone(latestRun.overallResult)}`}>
                {latestRun.overallResult}
              </span>
              <span className="text-sm text-slate-300">{latestRun.packLabel}</span>
            </div>
            <dl className="mt-4 space-y-3 text-sm">
              <div>
                <dt className="text-slate-400">Rulepack</dt>
                <dd className="mt-1 text-slate-100">{latestRun.packLabel}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Evaluation time</dt>
                <dd className="mt-1 text-slate-100">{new Date(latestRun.createdAt).toLocaleString()}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Product signal</dt>
                <dd className="mt-1 text-slate-100">
                  {latestRun.findingsEmitted?.length
                    ? `${latestRun.findingsEmitted.length} review item${latestRun.findingsEmitted.length === 1 ? '' : 's'} emitted`
                    : 'No finding emitted by this run'}
                </dd>
              </div>
            </dl>
          </div>

          <div className="space-y-3">
            <div className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
              <h3 className="font-semibold text-white">Facts used</h3>
              {Object.keys(latestRun.factInputs).length === 0 ? (
                <p className="mt-2 text-sm text-slate-400">No fact inputs were recorded on this run.</p>
              ) : (
                <ul className="mt-3 grid gap-2 md:grid-cols-2">
                  {Object.entries(latestRun.factInputs).map(([factKey, value]) => (
                    <li key={factKey} className="rounded-md border border-slate-800 bg-slate-950 px-3 py-2 text-sm">
                      <span className="font-mono text-xs text-sky-300">{factKey}</span>
                      <span className="ml-2 text-slate-300">{value ? 'true' : 'false'}</span>
                    </li>
                  ))}
                </ul>
              )}
            </div>
            <div className="rounded-lg border border-slate-800 bg-slate-900/70 p-4">
              <h3 className="font-semibold text-white">Calculations run</h3>
              <ul className="mt-3 space-y-2">
                {latestRun.ruleResults.map((result) => (
                  <li key={result.ruleKey} className="rounded-md border border-slate-800 bg-slate-950 px-3 py-2 text-sm">
                    <div className="flex flex-wrap items-center justify-between gap-2">
                      <span className="font-medium text-slate-100">{result.label}</span>
                      <span className={`rounded-md px-2 py-1 text-xs font-semibold uppercase ${resultTone(result.result)}`}>
                        {result.result}
                      </span>
                    </div>
                    <p className="mt-1 text-xs text-slate-400">{result.message}</p>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      )}
    </section>
  )
}

export function EvaluationSection({ state }: Props) {
  const s = state
  const activeView = evaluationViewFromPath(useLocation().pathname)
  const latestRun =
    s.lastEvaluation ??
    s.allRuleEvaluationsQuery.data?.[0] ??
    s.ruleEvaluationsQuery.data?.[0] ??
    null
  return (
    <div className="space-y-8">
      <section className="rounded-lg border border-slate-800 bg-slate-950/70 p-5">
        <h2 className="text-lg font-semibold text-white">Evaluation chain</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-300">
          Evaluations show what Compliance Core actually calculated, for whom or what, and why.
          Testing logic here does not mutate source product workflows.
        </p>
        <div className="mt-4 grid gap-3 lg:grid-cols-7">
          {['Applicability', 'Facts used', 'Facts missing', 'Evidence', 'Calculation', 'Result', 'Output signal'].map((step, index) => (
            <div key={step} className="rounded-lg border border-slate-800 bg-slate-900/70 p-3">
              <span className="text-xs font-semibold text-sky-300">{index + 1}</span>
              <p className="mt-1 text-sm font-medium text-slate-100">{step}</p>
            </div>
          ))}
        </div>
      </section>
      <EvaluationTabs activeView={activeView} />
      <ExplainResultPanel latestRun={latestRun} />
      {(activeView === 'overview' || activeView === 'tester') ? (
        <SituationEvaluatorPanel
          accessToken={s.accessToken}
          canEvaluate={s.canManage || s.canEvaluateRisk}
          factRequirements={s.factRequirementsQuery.data ?? []}
        />
      ) : null}
      {(activeView === 'overview' || activeView === 'traces') ? (
        <RuleEvaluationPanel
          rulePacks={s.rulePacksQuery.data ?? []}
          factDefinitions={s.factDefinitionsQuery.data ?? []}
          selectedRulePackId={s.selectedRulePackId}
          onSelectRulePack={s.setSelectedRulePackId}
          content={s.rulePackContentQuery.data?.content ?? null}
          hasContent={s.rulePackContentQuery.data?.hasContent ?? false}
          evaluationRuns={s.ruleEvaluationsQuery.data ?? []}
          canManage={s.canManage}
          onSaveContent={(content) => s.saveRuleContentMutation.mutate(content)}
          isSavingContent={s.saveRuleContentMutation.isPending}
          onEvaluate={(facts) => s.evaluateRulePackMutation.mutate(facts)}
          isEvaluating={s.evaluateRulePackMutation.isPending}
          lastEvaluation={s.lastEvaluation}
          onEvaluateBatch={(rulePackKeys, facts, emitFindings) =>
            s.evaluateRulePackBatchMutation.mutate({ rulePackKeys, facts, emitFindings })
          }
          isEvaluatingBatch={s.evaluateRulePackBatchMutation.isPending}
          lastBatchEvaluation={s.lastBatchEvaluation}
        />
      ) : null}
      {(activeView === 'overview' || activeView === 'recent' || activeView === 'traces') ? (
        <EvaluationHistoryExplorerPanel
          accessToken={s.accessToken}
          rulePacks={s.rulePacksQuery.data ?? []}
          evaluationRuns={s.allRuleEvaluationsQuery.data ?? []}
          canExportAudit={s.canExportAudit}
          onFocusRulePack={s.setSelectedRulePackId}
        />
      ) : null}
    </div>
  )
}
