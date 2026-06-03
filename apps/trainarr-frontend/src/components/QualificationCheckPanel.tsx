import { useMemo } from 'react'
import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

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
  const selectedRulePackOption = useMemo<PickerOption | undefined>(
    () =>
      rulePackOptions.find((option) => option.value === rulePackKey) ??
      (rulePackKey ? { value: rulePackKey, label: rulePackKey } : undefined),
    [rulePackKey, rulePackOptions],
  )

  return (
    <div className="mt-3 space-y-3 rounded-lg border border-slate-700 bg-slate-950/50 p-3">
      <p className="text-xs font-medium uppercase tracking-wide text-slate-400">Authorization check</p>
      <StaticSearchPicker
        id="qualification-check-rule-pack"
        label="Compliance Core rule pack key"
        value={rulePackKey}
        onChange={onRulePackKeyChange}
        options={rulePackOptions}
        selectedOption={selectedRulePackOption}
        placeholder="Search rule packs…"
        testId="qualification-check-rule-pack"
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
          {check.qualificationCatalog && (
            <p className="mt-2 text-xs opacity-90">
              Catalog: {check.qualificationCatalog.labelSnapshot} · {check.qualificationCatalog.statusSnapshot}
            </p>
          )}
          {check.localQualification && (
            <p className="mt-2 text-xs opacity-90">
              TrainArr qualification:{' '}
              {(check.localQualification.qualificationName ?? check.qualificationKey).replace('_', ' ')} ·{' '}
              {check.localQualification.status.replace('_', ' ')}
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
