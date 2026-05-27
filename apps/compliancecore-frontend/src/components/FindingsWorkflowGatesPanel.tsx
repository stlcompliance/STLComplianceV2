import { useState } from 'react'

import type {
  ComplianceFindingResponse,
  FactDefinitionResponse,
  RulePackContentBody,
  RulePackResponse,
  WorkflowGateBatchCheckResponse,
  WorkflowGateCheckResponse,
  WorkflowGateDefinitionResponse,
} from '../api/types'

import { BatchWorkflowGateCheckPanel } from './BatchWorkflowGateCheckPanel'

interface FindingsWorkflowGatesPanelProps {
  rulePacks: RulePackResponse[]
  factDefinitions: FactDefinitionResponse[]
  rulePackContent: RulePackContentBody | null
  findings: ComplianceFindingResponse[]
  workflowGates: WorkflowGateDefinitionResponse[]
  canManage: boolean
  onSeedGate: () => void
  isSeedingGate: boolean
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
  rulePacks,
  factDefinitions,
  rulePackContent,
  findings,
  workflowGates,
  canManage,
  onSeedGate,
  isSeedingGate,
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
    <div className="space-y-6">
      <div className="flex items-center justify-between gap-3">
        <p className="text-sm text-slate-400">
          Review compliance findings from evaluations and run workflow gate checks tied to rule pack outcomes.
        </p>
        {canManage && (
          <button
            type="button"
            onClick={onSeedGate}
            disabled={isSeedingGate || rulePacks.length === 0}
            className="rounded-md bg-violet-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          >
            {isSeedingGate ? 'Seeding…' : 'Seed sample workflow gate'}
          </button>
        )}
      </div>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold text-slate-100">Workflow gate check</h2>
        <p className="mt-1 text-xs text-slate-400">
          Gates evaluate the linked rule pack and return allow, warn, or block with reasons.
        </p>
        <div className="mt-4 grid gap-3 sm:grid-cols-2">
          <label className="block text-xs text-slate-400">
            Gate
            <select
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
          <label className="flex items-end gap-2 text-xs text-slate-300">
            <input
              type="checkbox"
              checked={emitFindings}
              onChange={(event) => setEmitFindings(event.target.checked)}
              className="rounded border-slate-600"
            />
            Emit findings when blocked
          </label>
        </div>
        {selectedGate && (
          <p className="mt-2 text-xs text-slate-500">{selectedGate.description}</p>
        )}
        <div className="mt-4 space-y-2">
          {factKeys.map((factKey) => (
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
                className="rounded border-slate-600"
              />
              {factKey}
            </label>
          ))}
        </div>
        <button
          type="button"
          disabled={!selectedGateKey || isCheckingGate}
          onClick={() => onCheckGate(selectedGateKey, factInputs, emitFindings)}
          className="mt-4 rounded-md bg-violet-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
        >
          {isCheckingGate ? 'Checking…' : 'Run gate check'}
        </button>
        {lastGateCheck && (
          <div className="mt-4 rounded-lg border border-slate-700 bg-slate-950/80 p-3">
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
              <p className="mt-2 text-xs text-amber-300">
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

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
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
