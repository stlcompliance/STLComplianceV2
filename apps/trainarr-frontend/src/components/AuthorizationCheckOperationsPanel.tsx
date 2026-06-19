import { useMemo } from 'react'
import { AdvancedReferenceField, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type {
  QualificationCheckHistoryItemResponse,
  QualificationCheckResponse,
  TrainingDefinitionResponse,
} from '../api/types'
import { QualificationCheckPanel } from './QualificationCheckPanel'

interface AuthorizationCheckOperationsPanelProps {
  definitions: TrainingDefinitionResponse[]
  history: QualificationCheckHistoryItemResponse[]
  isLoadingHistory: boolean
  check: QualificationCheckResponse | null
  isChecking: boolean
  canRun: boolean
  staffarrPersonId: string
  onStaffarrPersonIdChange: (value: string) => void
  selectedDefinitionId: string
  onSelectDefinition: (definitionId: string) => void
  rulePackKey: string
  onRulePackKeyChange: (value: string) => void
  rulePackOptions: PickerOption[]
  personPickerOptions: PickerOption[]
  onRunCheck: () => void
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

export function AuthorizationCheckOperationsPanel({
  definitions,
  history,
  isLoadingHistory,
  check,
  isChecking,
  canRun,
  staffarrPersonId,
  onStaffarrPersonIdChange,
  selectedDefinitionId,
  onSelectDefinition,
  rulePackKey,
  onRulePackKeyChange,
  rulePackOptions,
  personPickerOptions,
  onRunCheck,
}: AuthorizationCheckOperationsPanelProps) {
  const definitionOptions = useMemo<PickerOption[]>(
    () => definitions.map((definition) => ({ value: definition.trainingDefinitionId, label: `${definition.name} · ${definition.qualificationKey}` })),
    [definitions],
  )
  const selectedDefinitionOption = useMemo<PickerOption | undefined>(
    () => definitionOptions.find((option) => option.value === selectedDefinitionId),
    [definitionOptions, selectedDefinitionId],
  )

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="authorization-check-operations-panel"
    >
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
        Authorization check operations
      </h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Run ad-hoc qualification authorization checks and review recent outcomes before assignment or dispatch
        gating decisions.
      </p>

      <div className="mt-4 grid gap-3 sm:grid-cols-2">
        <StaticSearchPicker
          id="authorization-check-person-picker"
          label="StaffArr person"
          value={staffarrPersonId}
          onChange={onStaffarrPersonIdChange}
          options={personPickerOptions}
          placeholder="Search people from assignments and qualifications…"
          testId="authorization-check-person-picker"
        />
        <AdvancedReferenceField
          id="authorization-check-person-advanced"
          value={staffarrPersonId}
          onChange={onStaffarrPersonIdChange}
          label="StaffArr person (advanced)"
          testId="authorization-check-person-advanced"
        />
        <StaticSearchPicker
          id="authorization-check-definition"
          label="Training definition"
          value={selectedDefinitionId}
          onChange={onSelectDefinition}
          options={definitionOptions}
          placeholder="Search training definitions…"
          testId="authorization-check-definition-picker"
          selectedOption={selectedDefinitionOption}
        />
      </div>

      <QualificationCheckPanel
        check={check}
        isChecking={isChecking}
        onRunCheck={onRunCheck}
        canRun={canRun}
        rulePackKey={rulePackKey}
        onRulePackKeyChange={onRulePackKeyChange}
        rulePackOptions={rulePackOptions}
      />

      <div className="mt-4" data-testid="authorization-check-history">
        <p className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Recent checks</p>
        {isLoadingHistory ? (
          <p className="mt-2 text-sm text-slate-400" data-testid="authorization-check-history-loading">
            Loading check history…
          </p>
        ) : history.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400" data-testid="authorization-check-history-empty">
            No authorization checks recorded yet.
          </p>
        ) : (
          <ul className="mt-2 max-h-56 space-y-2 overflow-y-auto" data-testid="authorization-check-history-list">
            {history.map((item) => (
              <li
                key={item.checkId}
                className="rounded border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
              >
                <p className={`font-semibold uppercase ${outcomeClass(item.outcome)}`}>{item.outcome}</p>
                <p className="mt-1 font-mono text-xs text-[var(--color-text-muted)]">{item.staffarrPersonId}</p>
                <p className="mt-1 text-xs text-slate-400">
                  {item.qualificationKey} · {new Date(item.checkedAt).toLocaleString()}
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">{item.message}</p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
