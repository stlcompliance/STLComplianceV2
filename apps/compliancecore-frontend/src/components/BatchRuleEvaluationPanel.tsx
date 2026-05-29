import { useState } from 'react'

import type {
  EvaluateRulePackBatchResponse,
  RulePackResponse,
} from '../api/types'

interface BatchRuleEvaluationPanelProps {
  rulePacks: RulePackResponse[]
  factKeys: string[]
  batch: EvaluateRulePackBatchResponse | null
  isEvaluating: boolean
  onRunBatch: (
    rulePackKeys: string[],
    facts: Record<string, boolean>,
    emitFindings: boolean,
  ) => void
}

function outcomeBadgeClass(outcome: string): string {
  if (outcome === 'allow') {
    return 'bg-emerald-900/60 text-emerald-200'
  }
  if (outcome === 'warn') {
    return 'bg-amber-900/60 text-amber-200'
  }
  return 'bg-red-900/60 text-red-200'
}

export function BatchRuleEvaluationPanel({
  rulePacks,
  factKeys,
  batch,
  isEvaluating,
  onRunBatch,
}: BatchRuleEvaluationPanelProps) {
  const [selectedPackKeys, setSelectedPackKeys] = useState<string[]>([])
  const [factInputs, setFactInputs] = useState<Record<string, boolean>>({})
  const [emitFindings, setEmitFindings] = useState(false)

  const packsWithContent = rulePacks.filter((pack) => pack.isActive)

  const togglePack = (packKey: string) => {
    setSelectedPackKeys((current) =>
      current.includes(packKey) ? current.filter((key) => key !== packKey) : [...current, packKey],
    )
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="batch-rule-evaluation-panel"
    >
      <h2 className="text-sm font-semibold text-slate-100">Batch rule evaluation</h2>
      <p className="mt-1 text-xs text-slate-400">
        Evaluate multiple rule packs in one request with shared fact inputs applied to each selected pack.
      </p>

      {packsWithContent.length === 0 ? (
        <p className="mt-3 text-sm text-slate-500">Create rule packs with content before running a batch evaluation.</p>
      ) : (
        <ul className="mt-4 max-h-40 space-y-2 overflow-y-auto rounded-lg border border-slate-800 bg-slate-950/60 p-2">
          {packsWithContent.map((pack) => {
            const packInputId = `batch-rule-evaluation-pack-${pack.packKey}`
            return (
            <li key={pack.rulePackId}>
              <label htmlFor={packInputId} className="flex cursor-pointer items-start gap-2 text-sm text-slate-200">
                <input
                  id={packInputId}
                  type="checkbox"
                  className="mt-1 rounded border-slate-600"
                  data-testid={`batch-rule-evaluation-pack-${pack.packKey}`}
                  checked={selectedPackKeys.includes(pack.packKey)}
                  onChange={() => togglePack(pack.packKey)}
                />
                <span>
                  {pack.label}
                  <span className="mt-0.5 block text-xs text-slate-500">
                    {pack.packKey} · v{pack.versionNumber} ({pack.status})
                  </span>
                </span>
              </label>
            </li>
            )
          })}
        </ul>
      )}

      <div className="mt-4 space-y-2">
        {factKeys.length === 0 ? (
          <p className="text-xs text-slate-500">Add rule content with fact keys to configure batch fact inputs.</p>
        ) : (
          factKeys.map((factKey) => {
            const factInputId = `batch-rule-evaluation-fact-${factKey.replace(/[^a-zA-Z0-9_-]/g, '-')}`
            return (
            <label key={factKey} htmlFor={factInputId} className="flex items-center gap-2 text-sm text-slate-200">
              <input
                id={factInputId}
                type="checkbox"
                checked={factInputs[factKey] ?? false}
                onChange={(event) =>
                  setFactInputs((current) => ({
                    ...current,
                    [factKey]: event.target.checked,
                  }))
                }
                className="rounded border-slate-600"
              />
              <span className="font-mono text-xs text-sky-300">{factKey}</span>
            </label>
            )
          })
        )}
      </div>

      <label htmlFor="batch-rule-evaluation-emit-findings" className="mt-4 flex items-center gap-2 text-xs text-slate-300">
        <input
          id="batch-rule-evaluation-emit-findings"
          type="checkbox"
          data-testid="batch-rule-evaluation-emit-findings"
          checked={emitFindings}
          onChange={(event) => setEmitFindings(event.target.checked)}
          className="rounded border-slate-600"
        />
        Emit findings when batch evaluation fails
      </label>

      <button
        type="button"
        data-testid="batch-rule-evaluation-run"
        disabled={selectedPackKeys.length === 0 || isEvaluating}
        onClick={() => onRunBatch(selectedPackKeys, factInputs, emitFindings)}
        className="mt-4 rounded-md bg-violet-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
      >
        {isEvaluating
          ? 'Running batch…'
          : `Run batch evaluation (${selectedPackKeys.length} pack${selectedPackKeys.length === 1 ? '' : 's'})`}
      </button>

      {batch && (
        <div
          className="mt-4 space-y-3"
          data-testid="batch-rule-evaluation-latest-result"
          data-allow-count={batch.summary.allowCount}
          data-block-count={batch.summary.blockCount}
          data-warn-count={batch.summary.warnCount}
        >
          <div className="flex flex-wrap gap-2 text-xs text-slate-300">
            <span className="rounded bg-slate-800 px-2 py-1">{batch.summary.total} total</span>
            <span className="rounded bg-emerald-900/50 px-2 py-1 text-emerald-200">
              {batch.summary.allowCount} allow
            </span>
            <span className="rounded bg-amber-900/50 px-2 py-1 text-amber-200">
              {batch.summary.warnCount} warn
            </span>
            <span className="rounded bg-red-900/50 px-2 py-1 text-red-200">
              {batch.summary.blockCount} block
            </span>
          </div>
          <ul className="divide-y divide-slate-800 rounded-lg border border-slate-800">
            {batch.results.map((result) => (
              <li key={`${result.rulePackKey}-${result.evaluationRunId ?? result.outcome}`} className="px-3 py-2">
                <div className="flex flex-wrap items-center gap-2">
                  <span
                    className={`rounded px-2 py-0.5 text-xs font-medium uppercase ${outcomeBadgeClass(result.outcome)}`}
                  >
                    {result.outcome}
                  </span>
                  <span className="text-sm text-slate-200">{result.packLabel}</span>
                  <span className="text-xs text-slate-500">{result.rulePackKey}</span>
                </div>
                <p className="mt-1 text-xs text-slate-400">{result.message}</p>
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}
