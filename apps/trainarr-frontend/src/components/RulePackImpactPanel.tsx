import { useMemo } from 'react'
import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type { RulePackImpactAssessmentResponse } from '../api/types'

interface RulePackImpactPanelProps {
  rulePackKeyInput: string
  rulePackOptions: PickerOption[]
  onRulePackKeyChange: (value: string) => void
  onAssess: () => void
  isAssessing: boolean
  canAssess: boolean
  assessment: RulePackImpactAssessmentResponse | null
}

function triggerLabel(trigger: string): string {
  switch (trigger) {
    case 'version_drift':
      return 'Version drift'
    case 'status_change':
      return 'Status change'
    case 'pack_inactive':
      return 'Pack inactive'
    case 'pack_not_found':
      return 'Pack not found'
    case 'manual_assessment':
      return 'Manual assessment'
    default:
      return trigger.replace(/_/g, ' ')
  }
}

function priorityClass(priority: string): string {
  switch (priority) {
    case 'high':
      return 'border-red-800/60 bg-red-950/30 text-red-100'
    case 'medium':
      return 'border-amber-800/60 bg-amber-950/30 text-amber-100'
    default:
      return 'border-slate-700 bg-slate-950/40 text-slate-200'
  }
}

export function RulePackImpactPanel({
  rulePackKeyInput,
  rulePackOptions,
  onRulePackKeyChange,
  onAssess,
  isAssessing,
  canAssess,
  assessment,
}: RulePackImpactPanelProps) {
  const selectedRulePackOption = useMemo<PickerOption | undefined>(
    () =>
      rulePackOptions.find((option) => option.value === rulePackKeyInput) ??
      (rulePackKeyInput ? { value: rulePackKeyInput, label: rulePackKeyInput } : undefined),
    [rulePackKeyInput, rulePackOptions],
  )

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Rule pack change impact</h2>
      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
        Assess training impact when Compliance Core rule pack content or status changes. TrainArr orchestrates impact on
        the training domain; Compliance Core owns rule content.
      </p>

      {canAssess ? (
        <div className="mt-4 flex flex-wrap items-end gap-3">
          <div className="min-w-[16rem] flex-1">
            <StaticSearchPicker
              id="rule-pack-impact-key"
              label="Rule pack"
              value={rulePackKeyInput}
              onChange={onRulePackKeyChange}
              options={rulePackOptions}
              selectedOption={selectedRulePackOption}
              placeholder="Search rule packs…"
              testId="rule-pack-impact-key"
            />
          </div>
          <button
            type="button"
            className="rounded bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
            disabled={isAssessing || !rulePackKeyInput.trim()}
            onClick={onAssess}
          >
            {isAssessing ? 'Assessing…' : 'Run impact assessment'}
          </button>
        </div>
      ) : (
        <p className="mt-3 text-sm text-slate-400">Impact assessment requires trainarr admin access.</p>
      )}

      {assessment ? (
        <div className="mt-5 space-y-4">
          <div className="rounded-lg border border-slate-700 bg-slate-950/40 p-3">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <p className="text-sm font-medium text-slate-100">{assessment.currentState?.label ?? 'Rule pack'}</p>
              <span
                className={
                  assessment.summary.requiresAttention
                    ? 'rounded-full border border-amber-700 px-2 py-0.5 text-xs text-amber-200'
                    : 'rounded-full border border-emerald-700 px-2 py-0.5 text-xs text-emerald-200'
                }
              >
                {assessment.summary.requiresAttention ? 'Attention required' : 'Reviewed'}
              </span>
            </div>
            {assessment.currentState ? (
              <p className="mt-2 text-sm text-slate-300">
                {assessment.currentState.label} · v{assessment.currentState.versionNumber} ·{' '}
                {assessment.currentState.status}
              </p>
            ) : (
              <p className="mt-2 text-sm text-red-300">Rule pack not found in Compliance Core.</p>
            )}
            {assessment.triggers.length > 0 ? (
              <div className="mt-3 flex flex-wrap gap-2">
                {assessment.triggers.map((trigger) => (
                  <span
                    key={trigger}
                    className="rounded border border-slate-600 px-2 py-0.5 text-xs text-slate-300"
                  >
                    {triggerLabel(trigger)}
                  </span>
                ))}
              </div>
            ) : null}
            {assessment.drift?.hasVersionDrift || assessment.drift?.hasStatusDrift ? (
              <p className="mt-3 text-xs text-amber-200">
                Drift detected
                {assessment.drift.baselineVersionNumber != null && assessment.drift.currentVersionNumber != null
                  ? `: v${assessment.drift.baselineVersionNumber} → v${assessment.drift.currentVersionNumber}`
                  : ''}
                {assessment.drift.baselineStatus && assessment.drift.currentStatus
                  ? ` · ${assessment.drift.baselineStatus} → ${assessment.drift.currentStatus}`
                  : ''}
              </p>
            ) : null}
          </div>

          <dl className="grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-5">
            <div className="rounded border border-slate-700 p-2">
              <dt className="text-xs text-[var(--color-text-muted)]">Requirements</dt>
              <dd className="text-lg font-semibold text-slate-100">{assessment.summary.requirementCount}</dd>
            </div>
            <div className="rounded border border-slate-700 p-2">
              <dt className="text-xs text-[var(--color-text-muted)]">Definitions</dt>
              <dd className="text-lg font-semibold text-slate-100">{assessment.summary.definitionCount}</dd>
            </div>
            <div className="rounded border border-slate-700 p-2">
              <dt className="text-xs text-[var(--color-text-muted)]">Programs</dt>
              <dd className="text-lg font-semibold text-slate-100">{assessment.summary.programCount}</dd>
            </div>
            <div className="rounded border border-slate-700 p-2">
              <dt className="text-xs text-[var(--color-text-muted)]">Active assignments</dt>
              <dd className="text-lg font-semibold text-slate-100">{assessment.summary.activeAssignmentCount}</dd>
            </div>
            <div className="rounded border border-slate-700 p-2">
              <dt className="text-xs text-[var(--color-text-muted)]">Qualifications</dt>
              <dd className="text-lg font-semibold text-slate-100">{assessment.summary.activeQualificationCount}</dd>
            </div>
          </dl>

          {assessment.recommendedActions.length > 0 ? (
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Recommended actions</h3>
              <ul className="mt-2 space-y-2">
                {assessment.recommendedActions.map((action, index) => (
                  <li
                    key={`${action.actionType}-${index}`}
                    className={`rounded-lg border p-3 text-sm ${priorityClass(action.priority)}`}
                  >
                    <p className="font-medium capitalize">{action.actionType.replace(/_/g, ' ')}</p>
                    <p className="mt-1 text-xs opacity-90">{action.message}</p>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

              {assessment.affectedAssignments.length > 0 ? (
            <div>
              <h3 className="text-xs font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Affected assignments</h3>
              <ul className="mt-2 space-y-1 text-sm text-slate-300">
                {assessment.affectedAssignments.slice(0, 5).map((assignment) => (
                  <li key={assignment.assignmentId} className="text-xs">
                    {assignment.trainingDefinitionName} · {assignment.status}
                  </li>
                ))}
                {assessment.affectedAssignments.length > 5 ? (
                  <li className="text-xs text-[var(--color-text-muted)]">
                    +{assessment.affectedAssignments.length - 5} more assignment(s)
                  </li>
                ) : null}
              </ul>
            </div>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}
