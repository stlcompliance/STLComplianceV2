import { CheckboxMultiSelect, ControlledSelect, type PickerOption } from '@stl/shared-ui'

import type { BatchQualificationCheckResponse } from '../api/types'

interface BatchQualificationCheckPanelProps {
  batch: BatchQualificationCheckResponse | null
  isChecking: boolean
  onRunBatch: () => void
  canRun: boolean
  qualificationKey: string
  onQualificationKeyChange: (value: string) => void
  rulePackKey: string
  onRulePackKeyChange: (value: string) => void
  rulePackOptions: PickerOption[]
  selectedPersonIds: string[]
  onSelectedPersonIdsChange: (values: string[]) => void
  personPickerOptions: PickerOption[]
  selectedRemediationPersonIds: string[]
  onToggleRemediationPerson: (personId: string) => void
  remediationPersonOptions: { remediationId: string; label: string; staffarrPersonId: string }[]
}

function outcomeClass(outcome: string): string {
  switch (outcome) {
    case 'allow':
      return 'text-emerald-300'
    case 'warn':
      return 'text-amber-300'
    case 'block':
      return 'text-rose-300'
    default:
      return 'text-slate-300'
  }
}

export function BatchQualificationCheckPanel({
  batch,
  isChecking,
  onRunBatch,
  canRun,
  qualificationKey,
  onQualificationKeyChange,
  rulePackKey,
  onRulePackKeyChange,
  rulePackOptions,
  selectedPersonIds,
  onSelectedPersonIdsChange,
  personPickerOptions,
  selectedRemediationPersonIds,
  onToggleRemediationPerson,
  remediationPersonOptions,
}: BatchQualificationCheckPanelProps) {
  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Batch authorization check</h2>
      <p className="mt-1 text-xs text-slate-500">
        Run qualification checks for multiple StaffArr people in one request (supervisors and trainers).
      </p>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <label className="block text-xs text-slate-400">
          Qualification key
          <input
            type="text"
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-2 text-sm text-slate-100"
            value={qualificationKey}
            onChange={(e) => onQualificationKeyChange(e.target.value)}
            placeholder="hazmat_endorsement"
          />
        </label>
        <ControlledSelect
          label="Compliance Core rule pack key"
          value={rulePackKey}
          onChange={onRulePackKeyChange}
          options={rulePackOptions}
          emptyLabel="Select rule pack…"
          testId="batch-qualification-rule-pack"
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-2 text-sm text-slate-100"
        />
      </div>

      {remediationPersonOptions.length > 0 && (
        <div className="mt-4">
          <p className="text-xs font-medium uppercase tracking-wide text-slate-500">From incident remediations</p>
          <ul className="mt-2 max-h-36 space-y-1 overflow-y-auto rounded border border-slate-700 bg-slate-950/60 p-2">
            {remediationPersonOptions.map((option) => (
              <li key={option.remediationId}>
                <label className="flex cursor-pointer items-start gap-2 text-sm text-slate-200">
                  <input
                    type="checkbox"
                    className="mt-1"
                    checked={selectedRemediationPersonIds.includes(option.staffarrPersonId)}
                    onChange={() => onToggleRemediationPerson(option.staffarrPersonId)}
                  />
                  <span>
                    {option.label}
                    <span className="mt-0.5 block font-mono text-xs text-slate-500">{option.staffarrPersonId}</span>
                  </span>
                </label>
              </li>
            ))}
          </ul>
        </div>
      )}

      {personPickerOptions.length > 0 ? (
        <div className="mt-4">
          <CheckboxMultiSelect
            label="StaffArr people"
            values={selectedPersonIds}
            onChange={onSelectedPersonIdsChange}
            options={personPickerOptions}
            testId="batch-qualification-person-picker"
          />
        </div>
      ) : (
        <p className="mt-4 text-xs text-slate-500">
          No people directory refs loaded. Select people from remediations above or add qualification issues first.
        </p>
      )}

      <button
        type="button"
        className="mt-3 rounded bg-violet-700 px-3 py-2 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
        disabled={!canRun || isChecking}
        onClick={onRunBatch}
      >
        {isChecking ? 'Running batch check…' : 'Run batch qualification check'}
      </button>

      {batch && (
        <div className="mt-4 space-y-3">
          <div className="rounded-lg border border-slate-600 bg-slate-950/50 p-3 text-sm text-slate-200">
            <p className="font-medium">Batch summary</p>
            <p className="mt-1 text-xs text-slate-400">
              {batch.summary.total} checked ·{' '}
              <span className="text-emerald-300">{batch.summary.allowCount} allow</span> ·{' '}
              <span className="text-amber-300">{batch.summary.warnCount} warn</span> ·{' '}
              <span className="text-rose-300">{batch.summary.blockCount} block</span>
            </p>
          </div>
          <ul className="max-h-64 space-y-2 overflow-y-auto">
            {batch.results.map((result) => (
              <li
                key={result.checkId}
                className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
              >
                <p className={`font-semibold uppercase ${outcomeClass(result.outcome)}`}>{result.outcome}</p>
                <p className="mt-1 font-mono text-xs text-slate-500">{result.staffarrPersonId}</p>
                <p className="mt-1 text-xs text-slate-400">{result.message}</p>
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}
