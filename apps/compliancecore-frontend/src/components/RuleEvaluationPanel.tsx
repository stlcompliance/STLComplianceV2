import { useEffect, useState } from 'react'

import type {
  EvaluateRulePackBatchResponse,
  FactDefinitionResponse,
  RuleEvaluationRunResponse,
  RulePackContentBody,
  RulePackResponse,
} from '../api/types'

import { BatchRuleEvaluationPanel } from './BatchRuleEvaluationPanel'

interface RuleEvaluationPanelProps {
  rulePacks: RulePackResponse[]
  factDefinitions: FactDefinitionResponse[]
  selectedRulePackId: string
  onSelectRulePack: (rulePackId: string) => void
  content: RulePackContentBody | null
  hasContent: boolean
  evaluationRuns: RuleEvaluationRunResponse[]
  canManage: boolean
  onSaveContent: (content: RulePackContentBody) => void
  isSavingContent: boolean
  onSeedContent: () => void
  isSeedingContent: boolean
  onEvaluate: (facts: Record<string, boolean>) => void
  isEvaluating: boolean
  lastEvaluation: RuleEvaluationRunResponse | null
  onEvaluateBatch: (
    rulePackKeys: string[],
    facts: Record<string, boolean>,
    emitFindings: boolean,
  ) => void
  isEvaluatingBatch: boolean
  lastBatchEvaluation: EvaluateRulePackBatchResponse | null
}

function resultBadgeClass(result: string): string {
  return result === 'pass'
    ? 'bg-emerald-900/60 text-emerald-200'
    : 'bg-red-900/60 text-red-200'
}

export function RuleEvaluationPanel({
  rulePacks,
  factDefinitions,
  selectedRulePackId,
  onSelectRulePack,
  content,
  hasContent,
  evaluationRuns,
  canManage,
  onSaveContent,
  isSavingContent,
  onSeedContent,
  isSeedingContent,
  onEvaluate,
  isEvaluating,
  lastEvaluation,
  onEvaluateBatch,
  isEvaluatingBatch,
  lastBatchEvaluation,
}: RuleEvaluationPanelProps) {
  const [logic, setLogic] = useState(content?.logic ?? 'all')
  const [rulesJson, setRulesJson] = useState(
    JSON.stringify(content?.rules ?? [], null, 2),
  )
  const [factInputs, setFactInputs] = useState<Record<string, boolean>>({})

  useEffect(() => {
    setLogic(content?.logic ?? 'all')
    setRulesJson(JSON.stringify(content?.rules ?? [], null, 2))
    setFactInputs({})
  }, [selectedRulePackId, content])

  const selectedPack = rulePacks.find((pack) => pack.rulePackId === selectedRulePackId)
  const factKeysFromContent = (content?.rules ?? []).map((rule) => rule.factKey)
  const batchFactKeys =
    factDefinitions.length > 0
      ? factDefinitions.map((fact) => fact.factKey)
      : factKeysFromContent

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-400">
          Attach structured rule content to rule packs and run synchronous fact-based evaluations.
        </p>
        {canManage && (
          <button
            type="button"
            onClick={onSeedContent}
            disabled={isSeedingContent || rulePacks.length === 0}
            className="rounded-md bg-violet-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {isSeedingContent ? 'Seeding…' : 'Seed sample rule content'}
          </button>
        )}
      </div>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Rule pack</h2>
        {rulePacks.length === 0 ? (
          <p className="mt-3 text-sm text-slate-400">Create a rule pack on the Regulatory tab first.</p>
        ) : (
          <select
            value={selectedRulePackId}
            onChange={(event) => onSelectRulePack(event.target.value)}
            className="mt-3 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
          >
            {rulePacks.map((pack) => (
              <option key={pack.rulePackId} value={pack.rulePackId}>
                {pack.label} (v{pack.versionNumber}, {pack.status})
              </option>
            ))}
          </select>
        )}
        {selectedPack && (
          <p className="mt-2 text-xs text-slate-500">
            {selectedPack.packKey} · {hasContent ? 'content attached' : 'no content yet'}
          </p>
        )}
      </section>

      {canManage && selectedPack && (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Rule content editor</h2>
          <div className="mt-3 space-y-3">
            <label className="block text-xs text-slate-400">
              Logic
              <select
                value={logic}
                onChange={(event) => setLogic(event.target.value)}
                className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-slate-100"
              >
                <option value="all">All rules must pass</option>
                <option value="any">Any rule may pass</option>
              </select>
            </label>
            <label className="block text-xs text-slate-400">
              Rules JSON (fact_boolean rules)
              <textarea
                value={rulesJson}
                onChange={(event) => setRulesJson(event.target.value)}
                rows={8}
                className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
              />
            </label>
            <button
              type="button"
              disabled={isSavingContent}
              onClick={() => {
                try {
                  const parsedRules = JSON.parse(rulesJson) as RulePackContentBody['rules']
                  onSaveContent({
                    schemaVersion: 1,
                    logic,
                    rules: parsedRules,
                  })
                } catch {
                  window.alert('Rules JSON is invalid.')
                }
              }}
              className="rounded-md bg-violet-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-violet-500 disabled:opacity-50"
            >
              {isSavingContent ? 'Saving…' : 'Save rule content'}
            </button>
          </div>
        </section>
      )}

      {selectedPack && hasContent && (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Run evaluation</h2>
          <div className="mt-3 space-y-3">
            {(factKeysFromContent.length > 0 ? factKeysFromContent : factDefinitions.map((f) => f.factKey)).map(
              (factKey) => (
                <label key={factKey} className="flex items-center gap-2 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    checked={factInputs[factKey] ?? false}
                    onChange={(event) =>
                      setFactInputs((current) => ({
                        ...current,
                        [factKey]: event.target.checked,
                      }))
                    }
                    className="rounded border-slate-600 bg-slate-950"
                  />
                  <span className="font-mono text-xs text-sky-300">{factKey}</span>
                </label>
              ),
            )}
            <button
              type="button"
              disabled={isEvaluating}
              onClick={() => onEvaluate(factInputs)}
              className="rounded-md bg-emerald-700 px-3 py-1.5 text-xs font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
            >
              {isEvaluating ? 'Evaluating…' : 'Evaluate rule pack'}
            </button>
          </div>

          {lastEvaluation && (
            <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/60 p-3">
              <div className="flex items-center justify-between gap-2">
                <p className="text-sm font-medium text-slate-100">Latest result</p>
                <span className={`rounded px-2 py-0.5 text-xs uppercase ${resultBadgeClass(lastEvaluation.overallResult)}`}>
                  {lastEvaluation.overallResult}
                </span>
              </div>
              <ul className="mt-2 space-y-1">
                {lastEvaluation.ruleResults.map((item) => (
                  <li key={item.ruleKey} className="text-xs text-slate-400">
                    <span className="font-mono text-slate-300">{item.ruleKey}</span> — {item.result}: {item.message}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>
      )}

      <BatchRuleEvaluationPanel
        rulePacks={rulePacks}
        factKeys={batchFactKeys}
        batch={lastBatchEvaluation}
        isEvaluating={isEvaluatingBatch}
        onRunBatch={onEvaluateBatch}
      />

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Evaluation history</h2>
        {evaluationRuns.length === 0 ? (
          <p className="mt-3 text-sm text-slate-400">No evaluation runs yet.</p>
        ) : (
          <ul className="mt-3 space-y-2">
            {evaluationRuns.map((run) => (
              <li key={run.evaluationRunId} className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                <div className="flex items-center justify-between gap-2">
                  <p className="text-sm text-slate-100">{run.packLabel}</p>
                  <span className={`rounded px-2 py-0.5 text-xs uppercase ${resultBadgeClass(run.overallResult)}`}>
                    {run.overallResult}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-500">
                  v{run.versionNumber} · {new Date(run.createdAt).toLocaleString()}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
