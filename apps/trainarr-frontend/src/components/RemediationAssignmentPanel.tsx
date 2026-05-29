import type { PickerOption } from '@stl/shared-ui'
import type { StaffarrIncidentRemediationResponse, TrainingDefinitionResponse } from '../api/types'
import { QualificationCheckPanel } from './QualificationCheckPanel'
import type { QualificationCheckResponse } from '../api/types'

interface RemediationAssignmentPanelProps {
  remediations: StaffarrIncidentRemediationResponse[]
  definitions: TrainingDefinitionResponse[]
  selectedRemediationId: string | null
  selectedDefinitionId: string
  onSelectRemediation: (remediationId: string) => void
  onSelectDefinition: (definitionId: string) => void
  onCreateAssignment: () => void
  isCreating: boolean
  canManage: boolean
  qualificationCheck: QualificationCheckResponse | null
  isCheckingQualification: boolean
  onRunQualificationCheck: () => void
  rulePackKey: string
  onRulePackKeyChange: (value: string) => void
  rulePackOptions: PickerOption[]
}

export function RemediationAssignmentPanel({
  remediations,
  definitions,
  selectedRemediationId,
  selectedDefinitionId,
  onSelectRemediation,
  onSelectDefinition,
  onCreateAssignment,
  isCreating,
  canManage,
  qualificationCheck,
  isCheckingQualification,
  onRunQualificationCheck,
  rulePackKey,
  onRulePackKeyChange,
  rulePackOptions,
}: RemediationAssignmentPanelProps) {
  if (!canManage) {
    return null
  }

  const pendingRemediations = remediations.filter((r) => r.status === 'intake_received')
  const selectedRemediation = pendingRemediations.find((r) => r.remediationId === selectedRemediationId)
  const selectedDefinition = definitions.find((d) => d.trainingDefinitionId === selectedDefinitionId)
  const canRunCheck = Boolean(selectedRemediation && selectedDefinition)
  const blockedByCheck = qualificationCheck?.outcome === 'block'
  const missingCheck = !qualificationCheck

  return (
    <section className="rounded-xl border border-violet-800/60 bg-violet-950/20 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-violet-300">
        Remediation → assignment
      </h2>
      <p className="mt-1 text-xs text-slate-400">
        Run a qualification authorization check before creating a training assignment from remediation intake.
      </p>

      {pendingRemediations.length === 0 ? (
        <p className="mt-3 text-sm text-slate-400">No remediations awaiting assignment.</p>
      ) : (
        <div className="mt-3 space-y-3">
          <label className="block text-xs text-slate-400">
            Remediation
            <select
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-2 text-sm text-slate-100"
              value={selectedRemediationId ?? ''}
              onChange={(e) => onSelectRemediation(e.target.value)}
            >
              <option value="">Select remediation…</option>
              {pendingRemediations.map((remediation) => (
                <option key={remediation.remediationId} value={remediation.remediationId}>
                  {remediation.reasonCategoryKey} · person {remediation.staffarrPersonId.slice(0, 8)}…
                </option>
              ))}
            </select>
          </label>

          <label className="block text-xs text-slate-400">
            Training definition
            <select
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-2 text-sm text-slate-100"
              value={selectedDefinitionId}
              onChange={(e) => onSelectDefinition(e.target.value)}
            >
              <option value="">Select definition…</option>
              {definitions.map((definition) => (
                <option key={definition.trainingDefinitionId} value={definition.trainingDefinitionId}>
                  {definition.name}
                </option>
              ))}
            </select>
          </label>

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
            className="rounded bg-violet-700 px-4 py-2 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
            disabled={
              !selectedRemediationId ||
              !selectedDefinitionId ||
              isCreating ||
              blockedByCheck ||
              missingCheck
            }
            onClick={onCreateAssignment}
          >
            {isCreating ? 'Creating assignment…' : 'Create assignment from remediation'}
          </button>
          {blockedByCheck && (
            <p className="text-xs text-rose-300">
              Assignment creation is blocked until the authorization check outcome is allow or warn.
            </p>
          )}
          {missingCheck && selectedRemediationId && selectedDefinitionId ? (
            <p className="text-xs text-amber-300">Run an authorization check before creating the assignment.</p>
          ) : null}
        </div>
      )}
    </section>
  )
}
