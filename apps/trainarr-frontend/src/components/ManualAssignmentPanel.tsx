import { AdvancedReferenceField, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useMemo } from 'react'

import type { QualificationCheckResponse, TrainingDefinitionResponse } from '../api/types'
import { QualificationCheckPanel } from './QualificationCheckPanel'

interface ManualAssignmentPanelProps {
  definitions: TrainingDefinitionResponse[]
  staffarrPersonId: string
  onStaffarrPersonIdChange: (value: string) => void
  selectedDefinitionId: string
  onSelectDefinition: (definitionId: string) => void
  qualificationCheck: QualificationCheckResponse | null
  isCheckingQualification: boolean
  onRunQualificationCheck: () => void
  rulePackKey: string
  onRulePackKeyChange: (value: string) => void
  rulePackOptions: PickerOption[]
  personPickerOptions: PickerOption[]
  onCreateAssignment: () => void
  isCreating: boolean
  canManage: boolean
}

export function ManualAssignmentPanel({
  definitions,
  staffarrPersonId,
  onStaffarrPersonIdChange,
  selectedDefinitionId,
  onSelectDefinition,
  qualificationCheck,
  isCheckingQualification,
  onRunQualificationCheck,
  rulePackKey,
  onRulePackKeyChange,
  rulePackOptions,
  personPickerOptions,
  onCreateAssignment,
  isCreating,
  canManage,
}: ManualAssignmentPanelProps) {
  if (!canManage) {
    return null
  }

  const selectedDefinition = definitions.find((d) => d.trainingDefinitionId === selectedDefinitionId)
  const definitionOptions = useMemo<PickerOption[]>(
    () => definitions.map((definition) => ({ value: definition.trainingDefinitionId, label: definition.name })),
    [definitions],
  )
  const selectedDefinitionOption = useMemo<PickerOption | undefined>(
    () => definitionOptions.find((option) => option.value === selectedDefinitionId),
    [definitionOptions, selectedDefinitionId],
  )
  const canRunCheck = Boolean(staffarrPersonId.trim() && selectedDefinition)
  const blockedByCheck = qualificationCheck?.outcome === 'block'
  const missingCheck = !qualificationCheck

  return (
    <section className="rounded-xl border border-sky-800/60 bg-sky-950/20 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-sky-300">Manual assignment</h2>
      <p className="mt-1 text-xs text-slate-400">
        Create a direct training assignment after a qualification authorization check passes or warns.
      </p>

      <div className="mt-3 space-y-3">
        <StaticSearchPicker
          id="manual-assignment-person-picker"
          label="StaffArr person"
          value={staffarrPersonId}
          onChange={onStaffarrPersonIdChange}
          options={personPickerOptions}
          placeholder="Search people from assignments and qualifications…"
          testId="manual-assignment-person-picker"
        />
        <AdvancedReferenceField
          id="manual-assignment-person-advanced"
          value={staffarrPersonId}
          onChange={onStaffarrPersonIdChange}
          label="StaffArr person (advanced)"
          testId="manual-assignment-person-advanced"
        />

        <StaticSearchPicker
          id="manual-assignment-definition"
          label="Training definition"
          value={selectedDefinitionId}
          onChange={onSelectDefinition}
          options={definitionOptions}
          placeholder="Search training definitions…"
          testId="manual-assignment-definition-picker"
          selectedOption={selectedDefinitionOption}
        />

        <QualificationCheckPanel
          check={qualificationCheck}
          isChecking={isCheckingQualification}
          onRunCheck={onRunQualificationCheck}
          canRun={canRunCheck}
          rulePackKey={rulePackKey}
          onRulePackKeyChange={onRulePackKeyChange}
          rulePackOptions={rulePackOptions}
        />

        <button
          type="button"
          className="rounded bg-sky-700 px-4 py-2 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
          disabled={
            !staffarrPersonId.trim() ||
            !selectedDefinitionId ||
            isCreating ||
            blockedByCheck ||
            missingCheck
          }
          onClick={onCreateAssignment}
        >
          {isCreating ? 'Creating assignment…' : 'Create manual assignment'}
        </button>
        {blockedByCheck ? (
          <p className="text-xs text-rose-300">
            Assignment creation is blocked until the authorization check outcome is allow or warn.
          </p>
        ) : null}
        {missingCheck && staffarrPersonId.trim() && selectedDefinitionId ? (
          <p className="text-xs text-amber-300">Run an authorization check before creating the assignment.</p>
        ) : null}
      </div>
    </section>
  )
}
