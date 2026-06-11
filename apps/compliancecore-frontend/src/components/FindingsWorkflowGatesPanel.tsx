import { useState } from 'react'

import type {
  ComplianceFindingResponse,
  FactDefinitionResponse,
  RulePackContentBody,
  WorkflowGateBatchCheckResponse,
  WorkflowGateCheckResponse,
  WorkflowGateDefinitionResponse,
} from '../api/types'

import { BatchWorkflowGateCheckPanel } from './BatchWorkflowGateCheckPanel'

interface FindingsWorkflowGatesPanelProps {
  factDefinitions: FactDefinitionResponse[]
  rulePackContent: RulePackContentBody | null
  findings: ComplianceFindingResponse[]
  workflowGates: WorkflowGateDefinitionResponse[]
  onCheckGate: (gateKey: string, facts: Record<string, boolean>, emitFindings: boolean) => void
  isCheckingGate: boolean
  lastGateCheck: WorkflowGateCheckResponse | null
  onCheckGateBatch: (gateKeys: string[], facts: Record<string, boolean>, emitFindings: boolean) => void
  isCheckingGateBatch: boolean
  lastGateBatch: WorkflowGateBatchCheckResponse | null
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

function severityBadgeClass(severity: string): string {
  return severity === 'warn'
    ? 'bg-amber-900/60 text-amber-200'
    : 'bg-red-900/60 text-red-200'
}

export function FindingsWorkflowGatesPanel({
  factDefinitions,
  rulePackContent,
  findings,
  workflowGates,
  onCheckGate,
  isCheckingGate,
  lastGateCheck,
  onCheckGateBatch,
  isCheckingGateBatch,
  lastGateBatch,
}: FindingsWorkflowGatesPanelProps) {
  const [selectedGateKey, setSelectedGateKey] = useState('')
  const [factInputs, setFactInputs] = useState<Record<string, boolean>>({})
  const [emitFindings, setEmitFindings] = useState(true)

  const selectedGate = workflowGates.find((gate) => gate.gateKey === selectedGateKey)
  const factKeysFromContent = (rulePackContent?.rules ?? []).map((rule) => rule.factKey)
  const factKeys =
    factKeysFromContent.length > 0
      ? factKeysFromContent
      : factDefinitions.map((fact) => fact.factKey)

  return (
    <div className="space-y-6" data-testid="findings-workflow-gates-panel">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-400">
          Review compliance findings from evaluations and run workflow gate checks tied to rule pack outcomes.
        </p>
      </div>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold text-slate-100">Workflow gate check</h2>
        <p className="mt-1 text-xs text-slate-400">
          Gates evaluate the linked rule pack and return allow, warn, or block with reasons.
        </p>
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <label htmlFor="findings-workflow-gate-select" className="block text-xs text-slate-400">
            Workflow gate to check
            <select
              id="findings-workflow-gate-select"
              data-testid="findings-workflow-gate-select"
              value={selectedGateKey}
              onChange={(event) => setSelectedGateKey(event.target.value)}
              className="mt-1 w-full rounded-md border border-slate-600 bg-slate-950 px-2 py-1.5 text-sm text-slate-100"
            >
              <option value="">Select gate…</option>
              {workflowGates.map((gate) => (
                <option key={gate.workflowGateId} value={gate.gateKey}>
                  {gate.label} ({gate.packKey})
                </option>
              ))}
            </select>
          </label>
          <label htmlFor="findings-workflow-gate-emit-findings" className="flex items-end gap-2 text-xs text-slate-300">
            <input
              id="findings-workflow-gate-emit-findings"
              type="checkbox"
              data-testid="findings-workflow-gate-emit-findings"
              checked={emitFindings}
              onChange={(event) => setEmitFindings(event.target.checked)}
              className="rounded border-slate-600"
            />
            Emit findings when gate blocks
          </label>
        </div>
        {selectedGate && (
          <p className="mt-2 text-xs text-slate-500">{selectedGate.description}</p>
        )}
        <div className="mt-4 space-y-2">
          {factKeys.map((factKey) => {
            const factInputId = `findings-workflow-gate-fact-${factKey.replace(/[^a-zA-Z0-9_-]/g, '-')}`
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
        <button
          type="button"
          data-testid="findings-workflow-gate-check"
          disabled={!selectedGateKey || isCheckingGate}
          onClick={() => onCheckGate(selectedGateKey, factInputs, emitFindings)}
          className="mt-4 rounded-md bg-violet-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
        >
          {isCheckingGate ? 'Checking…' : 'Run gate check'}
        </button>
        {lastGateCheck && (
          <div
            className="mt-4 rounded-lg border border-slate-700 bg-slate-950/80 p-3"
            data-testid="findings-workflow-gate-latest-result"
            data-outcome={lastGateCheck.outcome}
          >
            <div className="flex flex-wrap items-center gap-2">
              <span className={`rounded px-2 py-0.5 text-xs font-medium uppercase ${outcomeBadgeClass(lastGateCheck.outcome)}`}>
                {lastGateCheck.outcome}
              </span>
              <span className="text-sm text-slate-200">{lastGateCheck.gateLabel}</span>
            </div>
            <p className="mt-2 text-sm text-slate-300">{lastGateCheck.message}</p>
            {lastGateCheck.reasons.length > 0 && (
              <ul className="mt-2 list-disc space-y-1 pl-5 text-xs text-slate-400">
                {lastGateCheck.reasons.map((reason) => (
                  <li key={`${reason.code}-${reason.ruleKey ?? reason.factKey}`}>{reason.message}</li>
                ))}
              </ul>
            )}
            {lastGateCheck.findingsEmitted.length > 0 && (
              <p
                className="mt-2 text-xs text-amber-300"
                data-testid="findings-workflow-gate-emitted-notice"
              >
                {lastGateCheck.findingsEmitted.length} finding(s) emitted from this check.
              </p>
            )}
          </div>
        )}
      </section>

      <BatchWorkflowGateCheckPanel
        workflowGates={workflowGates}
        factKeys={factKeys}
        batch={lastGateBatch}
        isChecking={isCheckingGateBatch}
        onRunBatch={onCheckGateBatch}
      />

      <section
        className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
        data-testid="findings-workflow-gate-findings-section"
      >
        <h2 className="text-sm font-semibold text-slate-100">Findings ({findings.length})</h2>
        {findings.length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No findings yet. Run an evaluation or gate check with emit findings enabled.</p>
        ) : (
          <ul className="mt-3 divide-y divide-slate-800">
            {findings.map((finding) => (
              <li key={finding.findingId} className="py-3">
                <div className="flex flex-wrap items-center gap-2">
                  <span className={`rounded px-2 py-0.5 text-xs font-medium uppercase ${severityBadgeClass(finding.severity)}`}>
                    {finding.severity}
                  </span>
                  <span className="text-sm font-medium text-slate-100">{finding.title}</span>
                  <span className="text-xs text-slate-500">{finding.packKey}</span>
                </div>
                <p className="mt-1 text-sm text-slate-400">{finding.message}</p>
                <p className="mt-1 text-xs text-slate-600">
                  {finding.findingKey} · {finding.status} · {finding.reasonCode}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold text-slate-100">Workflow gates ({workflowGates.length})</h2>
        {workflowGates.length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No workflow gates defined.</p>
        ) : (
          <ul className="mt-3 space-y-2">
            {workflowGates.map((gate) => (
              <li key={gate.workflowGateId} className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2">
                <p className="text-sm font-medium text-slate-100">{gate.label}</p>
                <p className="text-xs text-slate-500">
                  {gate.gateKey} → {gate.packKey}
                </p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  )
}
