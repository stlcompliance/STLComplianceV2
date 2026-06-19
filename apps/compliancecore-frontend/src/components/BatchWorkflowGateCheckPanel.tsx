import { useState } from 'react'

import type {
  WorkflowGateBatchCheckResponse,
  WorkflowGateDefinitionResponse,
} from '../api/types'

interface BatchWorkflowGateCheckPanelProps {
  workflowGates: WorkflowGateDefinitionResponse[]
  factKeys: string[]
  batch: WorkflowGateBatchCheckResponse | null
  isChecking: boolean
  onRunBatch: (gateKeys: string[], facts: Record<string, boolean>, emitFindings: boolean) => void
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

export function BatchWorkflowGateCheckPanel({
  workflowGates,
  factKeys,
  batch,
  isChecking,
  onRunBatch,
}: BatchWorkflowGateCheckPanelProps) {
  const [selectedGateKeys, setSelectedGateKeys] = useState<string[]>([])
  const [factInputs, setFactInputs] = useState<Record<string, boolean>>({})
  const [emitFindings, setEmitFindings] = useState(false)

  const toggleGate = (gateKey: string) => {
    setSelectedGateKeys((current) =>
      current.includes(gateKey) ? current.filter((key) => key !== gateKey) : [...current, gateKey],
    )
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="batch-workflow-gate-check-panel"
    >
      <h2 className="text-sm font-semibold text-slate-100">Batch workflow gate check</h2>
      <p className="mt-1 text-xs text-slate-400">
        Evaluate multiple gates in one request with shared fact inputs applied to each selected gate.
      </p>

      {workflowGates.length === 0 ? (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Define workflow gates before running a batch check.</p>
      ) : (
        <ul className="mt-4 max-h-40 space-y-2 overflow-y-auto rounded-lg border border-slate-800 bg-slate-950/60 p-2">
          {workflowGates.map((gate) => {
            const gateInputId = `batch-workflow-gate-gate-${gate.gateKey}`
            return (
            <li key={gate.workflowGateId}>
              <label htmlFor={gateInputId} className="flex cursor-pointer items-start gap-2 text-sm text-slate-200">
                <input
                  id={gateInputId}
                  type="checkbox"
                  className="mt-1 rounded border-slate-600"
                  data-testid={`batch-workflow-gate-gate-${gate.gateKey}`}
                  checked={selectedGateKeys.includes(gate.gateKey)}
                  onChange={() => toggleGate(gate.gateKey)}
                />
                <span>
                  {gate.label}
                  <span className="mt-0.5 block text-xs text-[var(--color-text-muted)]">
                    {gate.gateKey} → {gate.packKey}
                  </span>
                </span>
              </label>
            </li>
            )
          })}
        </ul>
      )}

      <div className="mt-4 space-y-2">
        {factKeys.map((factKey) => {
          const factInputId = `batch-workflow-gate-fact-${factKey.replace(/[^a-zA-Z0-9_-]/g, '-')}`
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
            {factKey}
          </label>
          )
        })}
      </div>

      <label htmlFor="batch-workflow-gate-emit-findings" className="mt-4 flex items-center gap-2 text-xs text-slate-300">
        <input
          id="batch-workflow-gate-emit-findings"
          type="checkbox"
          data-testid="batch-workflow-gate-emit-findings"
          checked={emitFindings}
          onChange={(event) => setEmitFindings(event.target.checked)}
          className="rounded border-slate-600"
        />
        Emit findings when batch gate blocks
      </label>

      <button
        type="button"
        data-testid="batch-workflow-gate-run"
        disabled={selectedGateKeys.length === 0 || isChecking}
        onClick={() => onRunBatch(selectedGateKeys, factInputs, emitFindings)}
        className="mt-4 rounded-md bg-violet-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
      >
        {isChecking ? 'Running batch…' : `Run batch check (${selectedGateKeys.length} gate${selectedGateKeys.length === 1 ? '' : 's'})`}
      </button>

      {batch && (
        <div
          className="mt-4 space-y-3"
          data-testid="batch-workflow-gate-latest-result"
          data-allow-count={batch.summary.allowCount}
          data-block-count={batch.summary.blockCount}
          data-warn-count={batch.summary.warnCount}
        >
          <div className="flex flex-wrap gap-2 text-xs text-slate-300">
            <span className="rounded bg-slate-800 px-2 py-1">
              {batch.summary.total} total
            </span>
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
              <li key={result.checkResultId} className="px-3 py-2">
                <div className="flex flex-wrap items-center gap-2">
                  <span className={`rounded px-2 py-0.5 text-xs font-medium uppercase ${outcomeBadgeClass(result.outcome)}`}>
                    {result.outcome}
                  </span>
                  <span className="text-sm text-slate-200">{result.gateLabel}</span>
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
