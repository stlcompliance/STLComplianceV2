import { ControlledSelect, type PickerOption } from '@stl/shared-ui'

import type { QualificationCheckResponse } from '../api/types'

interface QualificationCheckPanelProps {
  check: QualificationCheckResponse | null
  isChecking: boolean
  onRunCheck: () => void
  canRun: boolean
  rulePackKey: string
  onRulePackKeyChange: (value: string) => void
  rulePackOptions: PickerOption[]
}

function outcomeClass(outcome: string): string {
  switch (outcome) {
    case 'allow':
      return 'border-emerald-700 bg-emerald-950/40 text-emerald-100'
    case 'warn':
      return 'border-amber-700 bg-amber-950/40 text-amber-100'
    case 'block':
      return 'border-rose-700 bg-rose-950/40 text-rose-100'
    default:
      return 'border-slate-600 bg-slate-950/40 text-slate-200'
  }
}

export function QualificationCheckPanel({
  check,
  isChecking,
  onRunCheck,
  canRun,
  rulePackKey,
  onRulePackKeyChange,
  rulePackOptions,
}: QualificationCheckPanelProps) {
  return (
    <div className="mt-3 space-y-3 rounded-lg border border-slate-700 bg-slate-950/50 p-3">
      <p className="text-xs font-medium uppercase tracking-wide text-slate-400">Authorization check</p>
      <ControlledSelect
        label="Compliance Core rule pack key"
        value={rulePackKey}
        onChange={onRulePackKeyChange}
        options={rulePackOptions}
        emptyLabel="Select rule pack…"
        testId="qualification-check-rule-pack"
        className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-2 text-sm text-slate-100"
      />
      <button
        type="button"
        className="rounded bg-slate-700 px-3 py-2 text-sm font-medium text-white hover:bg-slate-600 disabled:opacity-50"
        disabled={!canRun || isChecking}
        onClick={onRunCheck}
      >
        {isChecking ? 'Checking authorization…' : 'Run qualification check'}
      </button>
      {check && (
        <div
          className={`rounded-lg border p-3 text-sm ${outcomeClass(check.outcome)}`}
          data-testid="qualification-check-latest-result"
          data-outcome={check.outcome}
        >
          <p className="font-semibold uppercase tracking-wide">{check.outcome}</p>
          <p className="mt-2">{check.message}</p>
          {check.localQualification && (
            <p className="mt-2 text-xs opacity-90">
              TrainArr qualification: {check.localQualification.status.replace('_', ' ')}
            </p>
          )}
          {check.complianceCore && (
            <p className="mt-1 text-xs opacity-90">
              Compliance Core ({check.complianceCore.rulePackKey}): {check.complianceCore.outcome} ·{' '}
              {check.complianceCore.evaluationResult}
            </p>
          )}
        </div>
      )}
    </div>
  )
}
