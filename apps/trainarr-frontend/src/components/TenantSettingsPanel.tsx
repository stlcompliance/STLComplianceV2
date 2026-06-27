import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getTrainArrTenantSettings,
  getTrainArrTenantSettingsDefaults,
  putTrainArrTenantSettings,
} from '../api/client'
import type {
  TrainArrTenantSettingsPayload,
  TrainArrTenantSettingsResponse,
} from '../api/types'

type Props = {
  accessToken: string
  canRead: boolean
  canManage: boolean
}

type GroupKey = keyof TrainArrTenantSettingsPayload

const priorityOptions = ['low', 'normal', 'high', 'critical']
const versionPolicies = [
  'none',
  'new_assignments_only',
  'incomplete_assignments_only',
  'expired_or_incomplete',
  'all_active_assignments',
]
const completionModes = ['self', 'trainer', 'manager', 'evaluator', 'blended']
const completionEditPolicies = [
  'locked',
  'admin_correction_only',
  'trainer_correction_allowed',
  'manager_correction_allowed',
]
const evidenceTypeOptions = ['pdf', 'image', 'video', 'external_url', 'signature', 'form']
const workBlockModes = ['none', 'warn', 'manager_override_required', 'hard_block']
const confidenceOptions = ['low', 'medium', 'high', 'verified']
const conflictPolicies = ['allow', 'warn', 'block', 'admin_override']
const rosterSources = ['staffarr_role', 'trainarr_qualification', 'both']
const citationModes = ['hidden', 'admin_only', 'trainer_and_admin', 'all_users']

export function TenantSettingsPanel({ accessToken, canRead, canManage }: Props) {
  const queryClient = useQueryClient()
  const settingsQuery = useQuery({
    queryKey: ['trainarr-tenant-settings', accessToken],
    queryFn: () => getTrainArrTenantSettings(accessToken),
    enabled: canRead,
  })
  const defaultsQuery = useQuery({
    queryKey: ['trainarr-tenant-settings-defaults', accessToken],
    queryFn: () => getTrainArrTenantSettingsDefaults(accessToken),
    enabled: canRead,
  })

  const [draft, setDraft] = useState<TrainArrTenantSettingsPayload | null>(null)
  const [baseline, setBaseline] = useState<TrainArrTenantSettingsPayload | null>(null)
  const [rowVersion, setRowVersion] = useState<number | null>(null)
  const [lastUpdated, setLastUpdated] = useState<Pick<
    TrainArrTenantSettingsResponse,
    'updatedAt' | 'updatedByDisplayName'
  > | null>(null)

  useEffect(() => {
    if (!settingsQuery.data || draft) {
      return
    }
    setDraft(settingsQuery.data.settings)
    setBaseline(settingsQuery.data.settings)
    setRowVersion(settingsQuery.data.rowVersion)
    setLastUpdated({
      updatedAt: settingsQuery.data.updatedAt,
      updatedByDisplayName: settingsQuery.data.updatedByDisplayName,
    })
  }, [draft, settingsQuery.data])

  const hasChanges = useMemo(
    () => Boolean(draft && baseline && JSON.stringify(draft) !== JSON.stringify(baseline)),
    [baseline, draft],
  )

  const saveMutation = useMutation({
    mutationFn: async () => {
      if (!canManage) {
        throw new Error('Only TrainArr tenant administrators can change tenant settings.')
      }
      if (!draft) {
        throw new Error('Settings are not loaded yet.')
      }
      return putTrainArrTenantSettings(accessToken, {
        settings: draft,
        rowVersion,
      })
    },
    onSuccess: (saved) => {
      setDraft(saved.settings)
      setBaseline(saved.settings)
      setRowVersion(saved.rowVersion)
      setLastUpdated({
        updatedAt: saved.updatedAt,
        updatedByDisplayName: saved.updatedByDisplayName,
      })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-tenant-settings', accessToken] })
    },
  })

  if (!canRead) {
    return null
  }

  if (settingsQuery.isError) {
    return (
      <ApiErrorCallout
        title="Tenant settings unavailable"
        message={getErrorMessage(settingsQuery.error, 'Failed to load TrainArr tenant settings.')}
        retryLabel="Retry settings"
        onRetry={() => {
          void settingsQuery.refetch()
        }}
      />
    )
  }

  if (settingsQuery.isLoading || !draft) {
    return (
      <section className="rounded-lg border border-border bg-card p-4 shadow-sm">
        <p className="text-sm text-muted-foreground">Loading TrainArr tenant settings…</p>
      </section>
    )
  }

  function updateGroup<K extends GroupKey>(
    group: K,
    patch: Partial<TrainArrTenantSettingsPayload[K]>,
  ) {
    setDraft((current) => current && {
      ...current,
      [group]: {
        ...current[group],
        ...patch,
      },
    })
  }

  function resetToDefaults() {
    if (defaultsQuery.data) {
      setDraft(defaultsQuery.data.settings)
    }
  }

  function cancelChanges() {
    if (baseline) {
      setDraft(baseline)
    }
  }

  return (
    <section className="space-y-5" data-testid="trainarr-tenant-settings-panel">
      <div className="rounded-lg border border-border bg-card p-4 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-foreground">Tenant settings</h2>
            <p className="mt-1 max-w-3xl text-sm text-muted-foreground">
              Tenant-scoped defaults for TrainArr assignment, certification, completion, evidence,
              remediation, notification, enforcement, integration, and audit behavior.
            </p>
            <p className="mt-2 text-xs text-muted-foreground" data-testid="tenant-settings-last-updated">
              {lastUpdated
                ? `Last updated ${new Date(lastUpdated.updatedAt).toLocaleString()}${lastUpdated.updatedByDisplayName ? ` by ${lastUpdated.updatedByDisplayName}` : ''}`
                : 'Using canonical defaults until the first tenant update.'}
            </p>
          </div>
          <span
            className={`rounded-full px-3 py-1 text-xs font-semibold ${
              hasChanges
                ? 'bg-amber-500/15 text-amber-300'
                : 'bg-emerald-500/15 text-emerald-300'
            }`}
            data-testid="tenant-settings-unsaved-state"
          >
            {hasChanges ? 'Unsaved changes' : 'Saved'}
          </span>
        </div>

        <div className="mt-4 flex flex-wrap gap-2">
          <button
            type="button"
            className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
            disabled={!canManage || !hasChanges || saveMutation.isPending}
            onClick={() => saveMutation.mutate()}
            data-testid="tenant-settings-save"
          >
            {saveMutation.isPending ? 'Saving…' : 'Save'}
          </button>
          <button
            type="button"
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground disabled:opacity-50"
            disabled={!canManage || !hasChanges || saveMutation.isPending}
            onClick={cancelChanges}
            data-testid="tenant-settings-cancel"
          >
            Cancel
          </button>
          <button
            type="button"
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground disabled:opacity-50"
            disabled={!canManage || !defaultsQuery.data || saveMutation.isPending}
            onClick={resetToDefaults}
            data-testid="tenant-settings-reset"
          >
            Reset to defaults
          </button>
        </div>
        {!canManage ? (
          <p className="mt-3 text-xs text-muted-foreground">
            You can review TrainArr tenant settings, but only TrainArr tenant administrators can change them.
          </p>
        ) : null}

        {saveMutation.isError && (
          <div className="mt-3">
            <ApiErrorCallout
              title="Save failed"
              message={getErrorMessage(saveMutation.error, 'Failed to save tenant settings.')}
            />
          </div>
        )}
      </div>

      <fieldset className="space-y-5" disabled={!canManage}>
        <SettingsCard
          title="Assignment"
        description="Default assignment behavior when a person or organization changes."
        >
          <ToggleField label="Auto-assign on hire" checked={draft.assignment.autoAssignOnHire} onChange={(value) => updateGroup('assignment', { autoAssignOnHire: value })} />
          <ToggleField label="Auto-assign on position change" checked={draft.assignment.autoAssignOnPositionChange} onChange={(value) => updateGroup('assignment', { autoAssignOnPositionChange: value })} />
          <ToggleField label="Auto-assign on site change" checked={draft.assignment.autoAssignOnSiteChange} onChange={(value) => updateGroup('assignment', { autoAssignOnSiteChange: value })} />
          <ToggleField label="Auto-assign on department change" checked={draft.assignment.autoAssignOnDepartmentChange} onChange={(value) => updateGroup('assignment', { autoAssignOnDepartmentChange: value })} />
          <ToggleField label="Allow manager assignment" checked={draft.assignment.allowManagerAssignment} onChange={(value) => updateGroup('assignment', { allowManagerAssignment: value })} />
          <ToggleField label="Allow self-enrollment" checked={draft.assignment.allowSelfEnrollment} onChange={(value) => updateGroup('assignment', { allowSelfEnrollment: value })} />
          <ToggleField label="Optional enrollment requires approval" checked={draft.assignment.optionalEnrollmentRequiresApproval} onChange={(value) => updateGroup('assignment', { optionalEnrollmentRequiresApproval: value })} />
          <NumberField label="Default due days" value={draft.assignment.defaultAssignmentDueDays} min={0} onChange={(value) => updateGroup('assignment', { defaultAssignmentDueDays: value })} />
          <NumberField label="Grace period days" value={draft.assignment.assignmentGracePeriodDays} min={0} onChange={(value) => updateGroup('assignment', { assignmentGracePeriodDays: value })} />
          <SelectField label="Default priority" value={draft.assignment.assignmentPriorityDefault} options={priorityOptions} onChange={(value) => updateGroup('assignment', { assignmentPriorityDefault: value })} />
        </SettingsCard>

      <SettingsCard
        title="Program Versioning"
        description="Default behavior when published training programs are revised."
        warning="Revision policies can trigger reassignment for active workers."
      >
        <SelectField label="Change policy" value={draft.programVersioning.programVersionChangePolicy} options={versionPolicies} onChange={(value) => updateGroup('programVersioning', { programVersionChangePolicy: value })} />
        <ToggleField label="Reassign on major version" checked={draft.programVersioning.reassignOnMajorVersion} onChange={(value) => updateGroup('programVersioning', { reassignOnMajorVersion: value })} />
        <ToggleField label="Reassign on minor version" checked={draft.programVersioning.reassignOnMinorVersion} onChange={(value) => updateGroup('programVersioning', { reassignOnMinorVersion: value })} />
        <ToggleField label="Allow in-progress version completion" checked={draft.programVersioning.allowInProgressVersionCompletion} onChange={(value) => updateGroup('programVersioning', { allowInProgressVersionCompletion: value })} />
        <ToggleField label="Require publish reason" checked={draft.programVersioning.requireReasonForProgramPublish} onChange={(value) => updateGroup('programVersioning', { requireReasonForProgramPublish: value })} />
        <ToggleField label="Archive superseded programs" checked={draft.programVersioning.archiveSupersededPrograms} onChange={(value) => updateGroup('programVersioning', { archiveSupersededPrograms: value })} />
      </SettingsCard>

      <SettingsCard
        title="Certifications"
        description="Default certificate validity, renewal, warning, and generated-certificate behavior."
        warning="Expiration and revocation settings can affect work eligibility."
      >
        <NumberField label="Default validity days" value={draft.certifications.defaultCertificateValidityDays ?? 0} min={0} onChange={(value) => updateGroup('certifications', { defaultCertificateValidityDays: value || null })} />
        <NumberField label="Renewal window days" value={draft.certifications.defaultRenewalWindowDays} min={0} onChange={(value) => updateGroup('certifications', { defaultRenewalWindowDays: value })} />
        <NumberChipField label="Expiration warning days" values={draft.certifications.defaultExpirationWarningDays} onChange={(value) => updateGroup('certifications', { defaultExpirationWarningDays: value })} />
        <ToggleField label="Allow early renewal" checked={draft.certifications.allowEarlyRenewal} onChange={(value) => updateGroup('certifications', { allowEarlyRenewal: value })} />
        <ToggleField label="Allow expired renewal" checked={draft.certifications.allowExpiredRenewal} onChange={(value) => updateGroup('certifications', { allowExpiredRenewal: value })} />
        <ToggleField label="Expired qualification blocks work" checked={draft.certifications.expiredQualificationBlocksWork} onChange={(value) => updateGroup('certifications', { expiredQualificationBlocksWork: value })} />
        <TextField label="Certificate number format" value={draft.certifications.certificateNumberFormat} onChange={(value) => updateGroup('certifications', { certificateNumberFormat: value })} hint="Must include {sequence}." />
        <ToggleField label="Require certificate PDF" checked={draft.certifications.requireCertificatePdf} onChange={(value) => updateGroup('certifications', { requireCertificatePdf: value })} />
        <TextField label="Display name format" value={draft.certifications.certificateDisplayNameFormat ?? ''} onChange={(value) => updateGroup('certifications', { certificateDisplayNameFormat: value.trim() || null })} />
      </SettingsCard>

      <SettingsCard title="Completion And Signoff" description="Defaults for how completions are captured, corrected, and approved.">
        <SelectField label="Completion mode" value={draft.completionSignoff.defaultCompletionMode} options={completionModes} onChange={(value) => updateGroup('completionSignoff', { defaultCompletionMode: value })} />
        <ToggleField label="Require trainer signoff" checked={draft.completionSignoff.requireTrainerSignoff} onChange={(value) => updateGroup('completionSignoff', { requireTrainerSignoff: value })} />
        <ToggleField label="Require trainee acknowledgement" checked={draft.completionSignoff.requireTraineeAcknowledgement} onChange={(value) => updateGroup('completionSignoff', { requireTraineeAcknowledgement: value })} />
        <ToggleField label="Require manager approval" checked={draft.completionSignoff.requireManagerApproval} onChange={(value) => updateGroup('completionSignoff', { requireManagerApproval: value })} />
        <ToggleField label="Allow bulk completion" checked={draft.completionSignoff.allowBulkCompletion} onChange={(value) => updateGroup('completionSignoff', { allowBulkCompletion: value })} />
        <ToggleField label="Bulk completion requires reason" checked={draft.completionSignoff.bulkCompletionRequiresReason} onChange={(value) => updateGroup('completionSignoff', { bulkCompletionRequiresReason: value })} />
        <ToggleField label="Allow backdated completion" checked={draft.completionSignoff.allowBackdatedCompletion} onChange={(value) => updateGroup('completionSignoff', { allowBackdatedCompletion: value })} />
        <NumberField label="Backdate max days" value={draft.completionSignoff.backdatedCompletionMaxDays} min={0} onChange={(value) => updateGroup('completionSignoff', { backdatedCompletionMaxDays: value })} />
        <ToggleField label="Backdating requires reason" checked={draft.completionSignoff.requireReasonForBackdating} onChange={(value) => updateGroup('completionSignoff', { requireReasonForBackdating: value })} />
        <SelectField label="Edit policy" value={draft.completionSignoff.completionEditPolicy} options={completionEditPolicies} onChange={(value) => updateGroup('completionSignoff', { completionEditPolicy: value })} />
      </SettingsCard>

      <SettingsCard title="Evaluations" description="Quiz, test, observation, and practical evaluation defaults.">
        <NumberField label="Passing score percent" value={draft.evaluations.defaultPassingScorePercent} min={0} max={100} onChange={(value) => updateGroup('evaluations', { defaultPassingScorePercent: value })} />
        <ToggleField label="Allow retakes" checked={draft.evaluations.allowRetakes} onChange={(value) => updateGroup('evaluations', { allowRetakes: value })} />
        <NumberField label="Max retake attempts" value={draft.evaluations.maxRetakeAttempts} min={0} onChange={(value) => updateGroup('evaluations', { maxRetakeAttempts: value })} />
        <NumberField label="Retake cooldown hours" value={draft.evaluations.retakeCooldownHours} min={0} onChange={(value) => updateGroup('evaluations', { retakeCooldownHours: value })} />
        <ToggleField label="Randomize question order" checked={draft.evaluations.randomizeQuestionOrder} onChange={(value) => updateGroup('evaluations', { randomizeQuestionOrder: value })} />
        <ToggleField label="Randomize answer order" checked={draft.evaluations.randomizeAnswerOrder} onChange={(value) => updateGroup('evaluations', { randomizeAnswerOrder: value })} />
        <ToggleField label="Show correct answers after attempt" checked={draft.evaluations.showCorrectAnswersAfterAttempt} onChange={(value) => updateGroup('evaluations', { showCorrectAnswersAfterAttempt: value })} />
        <ToggleField label="Require comment on fail" checked={draft.evaluations.requireEvaluatorCommentOnFail} onChange={(value) => updateGroup('evaluations', { requireEvaluatorCommentOnFail: value })} />
        <ToggleField label="Require comment on override" checked={draft.evaluations.requireEvaluatorCommentOnOverride} onChange={(value) => updateGroup('evaluations', { requireEvaluatorCommentOnOverride: value })} />
      </SettingsCard>

      <SettingsCard
        title="Remediation"
        description="Incident-driven retraining intake, review, assignment, and escalation posture."
        warning="Remediation can suspend qualification status when configured."
      >
        <ToggleField label="Accept incident retraining requests" checked={draft.remediation.acceptIncidentRetrainingRequests} onChange={(value) => updateGroup('remediation', { acceptIncidentRetrainingRequests: value })} />
        <NumberField label="Incident due days" value={draft.remediation.incidentRetrainingDefaultDueDays} min={0} onChange={(value) => updateGroup('remediation', { incidentRetrainingDefaultDueDays: value })} />
        <ToggleField label="Incident retraining requires review" checked={draft.remediation.incidentRetrainingRequiresReview} onChange={(value) => updateGroup('remediation', { incidentRetrainingRequiresReview: value })} />
        <ToggleField label="Auto-assign remediation on incident" checked={draft.remediation.autoAssignRemediationOnIncident} onChange={(value) => updateGroup('remediation', { autoAssignRemediationOnIncident: value })} />
        <NumberField label="Repeat incident threshold" value={draft.remediation.repeatIncidentEscalationThreshold} min={1} onChange={(value) => updateGroup('remediation', { repeatIncidentEscalationThreshold: value })} />
        <NumberField label="Repeat lookback days" value={draft.remediation.repeatIncidentLookbackDays} min={1} onChange={(value) => updateGroup('remediation', { repeatIncidentLookbackDays: value })} />
        <ToggleField label="Notify manager on remediation" checked={draft.remediation.notifyManagerOnRemediation} onChange={(value) => updateGroup('remediation', { notifyManagerOnRemediation: value })} />
        <ToggleField label="Block qualification during remediation" checked={draft.remediation.blockQualificationDuringRemediation} onChange={(value) => updateGroup('remediation', { blockQualificationDuringRemediation: value })} />
      </SettingsCard>

      <SettingsCard
        title="Evidence And Records"
        description="Completion evidence requirements and finalized record handoff posture."
        warning="Record handoff skips gracefully when the records workspace is unavailable or not configured."
      >
        <ToggleField label="Require evidence for completion" checked={draft.evidenceRecords.requireEvidenceForCompletion} onChange={(value) => updateGroup('evidenceRecords', { requireEvidenceForCompletion: value })} />
        <MultiCheckField label="Allowed evidence types" values={draft.evidenceRecords.allowedEvidenceTypes} options={evidenceTypeOptions} onChange={(value) => updateGroup('evidenceRecords', { allowedEvidenceTypes: value })} />
        <NumberField label="Max file size MB" value={draft.evidenceRecords.maxEvidenceFileSizeMb} min={1} onChange={(value) => updateGroup('evidenceRecords', { maxEvidenceFileSizeMb: value })} />
        <NumberField label="Retention years" value={draft.evidenceRecords.evidenceRetentionYears} min={0} onChange={(value) => updateGroup('evidenceRecords', { evidenceRetentionYears: value })} />
        <ToggleField label="Allow external evidence URL" checked={draft.evidenceRecords.allowExternalEvidenceUrl} onChange={(value) => updateGroup('evidenceRecords', { allowExternalEvidenceUrl: value })} />
        <ToggleField label="Require evidence review" checked={draft.evidenceRecords.requireEvidenceReview} onChange={(value) => updateGroup('evidenceRecords', { requireEvidenceReview: value })} />
        <ToggleField label="Allow trainee upload" checked={draft.evidenceRecords.allowTraineeEvidenceUpload} onChange={(value) => updateGroup('evidenceRecords', { allowTraineeEvidenceUpload: value })} />
        <ToggleField label="Allow trainer upload" checked={draft.evidenceRecords.allowTrainerEvidenceUpload} onChange={(value) => updateGroup('evidenceRecords', { allowTrainerEvidenceUpload: value })} />
        <ToggleField label="Send final records" checked={draft.evidenceRecords.sendFinalRecordsToRecordArr} onChange={(value) => updateGroup('evidenceRecords', { sendFinalRecordsToRecordArr: value })} />
      </SettingsCard>

      <SettingsCard title="Notifications" description="Assignment, overdue, escalation, issuance, and expiration notification posture.">
        <ToggleField label="Notify on assignment created" checked={draft.notifications.notifyOnAssignmentCreated} onChange={(value) => updateGroup('notifications', { notifyOnAssignmentCreated: value })} />
        <ToggleField label="Notify due soon" checked={draft.notifications.notifyOnDueSoon} onChange={(value) => updateGroup('notifications', { notifyOnDueSoon: value })} />
        <NumberChipField label="Due soon reminder days" values={draft.notifications.dueSoonReminderDays} onChange={(value) => updateGroup('notifications', { dueSoonReminderDays: value })} />
        <ToggleField label="Notify overdue" checked={draft.notifications.notifyOnOverdue} onChange={(value) => updateGroup('notifications', { notifyOnOverdue: value })} />
        <NumberField label="Overdue cadence days" value={draft.notifications.overdueReminderCadenceDays} min={1} onChange={(value) => updateGroup('notifications', { overdueReminderCadenceDays: value })} />
        <ToggleField label="Notify manager on overdue" checked={draft.notifications.notifyManagerOnOverdue} onChange={(value) => updateGroup('notifications', { notifyManagerOnOverdue: value })} />
        <ToggleField label="Notify admin on critical overdue" checked={draft.notifications.notifyAdminOnCriticalOverdue} onChange={(value) => updateGroup('notifications', { notifyAdminOnCriticalOverdue: value })} />
        <ToggleField label="Notify on certificate issued" checked={draft.notifications.notifyOnCertificateIssued} onChange={(value) => updateGroup('notifications', { notifyOnCertificateIssued: value })} />
        <ToggleField label="Notify on certificate expiring" checked={draft.notifications.notifyOnCertificateExpiring} onChange={(value) => updateGroup('notifications', { notifyOnCertificateExpiring: value })} />
        <NumberChipField label="Certificate warning days" values={draft.notifications.certificateExpirationWarningDays} onChange={(value) => updateGroup('notifications', { certificateExpirationWarningDays: value })} />
      </SettingsCard>

      <SettingsCard
        title="Enforcement"
        description="How qualification status is exposed to the rest of the suite."
        warning="Work block settings can prevent related workflows from releasing work."
      >
        <ToggleField label="Expose qualification status to products" checked={draft.enforcement.exposeQualificationStatusToProducts} onChange={(value) => updateGroup('enforcement', { exposeQualificationStatusToProducts: value })} />
        <ToggleField label="Allow products to block work" checked={draft.enforcement.allowProductsToBlockWork} onChange={(value) => updateGroup('enforcement', { allowProductsToBlockWork: value })} />
        <SelectField label="Default work block mode" value={draft.enforcement.defaultWorkBlockMode} options={workBlockModes} onChange={(value) => updateGroup('enforcement', { defaultWorkBlockMode: value })} />
        <ToggleField label="Allow manager override" checked={draft.enforcement.allowManagerOverrideOfBlock} onChange={(value) => updateGroup('enforcement', { allowManagerOverrideOfBlock: value })} />
        <ToggleField label="Override requires reason" checked={draft.enforcement.overrideRequiresReason} onChange={(value) => updateGroup('enforcement', { overrideRequiresReason: value })} />
        <NumberField label="Override duration hours" value={draft.enforcement.overrideDurationHours} min={1} onChange={(value) => updateGroup('enforcement', { overrideDurationHours: value })} />
        <ToggleField label="Publish qualification events" checked={draft.enforcement.publishQualificationEvents} onChange={(value) => updateGroup('enforcement', { publishQualificationEvents: value })} />
      </SettingsCard>

      <SettingsCard title="External Training" description="Outside training provider import and manual-entry posture.">
        <ToggleField label="Allow external training provider" checked={draft.externalTraining.allowExternalTrainingProvider} onChange={(value) => updateGroup('externalTraining', { allowExternalTrainingProvider: value })} />
        <ToggleField label="External completion requires review" checked={draft.externalTraining.externalCompletionRequiresReview} onChange={(value) => updateGroup('externalTraining', { externalCompletionRequiresReview: value })} />
        <ToggleField label="External certificate requires evidence" checked={draft.externalTraining.externalCertificateRequiresEvidence} onChange={(value) => updateGroup('externalTraining', { externalCertificateRequiresEvidence: value })} />
        <ProviderPicker providerCount={draft.externalTraining.trustedProviderIds.length} />
        <ToggleField label="Allow manual external completion entry" checked={draft.externalTraining.allowManualExternalCompletionEntry} onChange={(value) => updateGroup('externalTraining', { allowManualExternalCompletionEntry: value })} />
        <SelectField label="Default confidence" value={draft.externalTraining.externalRecordConfidenceDefault} options={confidenceOptions} onChange={(value) => updateGroup('externalTraining', { externalRecordConfidenceDefault: value })} />
      </SettingsCard>

      <SettingsCard title="Trainers And Evaluators" description="Who may train, evaluate, and sign off within TrainArr.">
        <ToggleField label="Trainer must be qualified" checked={draft.trainersEvaluators.trainerMustBeQualified} onChange={(value) => updateGroup('trainersEvaluators', { trainerMustBeQualified: value })} />
        <NumberField label="Trainer qualification required days" value={draft.trainersEvaluators.trainerQualificationRequiredDays} min={0} onChange={(value) => updateGroup('trainersEvaluators', { trainerQualificationRequiredDays: value })} />
        <ToggleField label="Allow trainer self-signoff" checked={draft.trainersEvaluators.allowTrainerSelfSignoff} onChange={(value) => updateGroup('trainersEvaluators', { allowTrainerSelfSignoff: value })} />
        <ToggleField label="Allow manager as evaluator" checked={draft.trainersEvaluators.allowManagerAsEvaluator} onChange={(value) => updateGroup('trainersEvaluators', { allowManagerAsEvaluator: value })} />
        <ToggleField label="Require different trainer and evaluator" checked={draft.trainersEvaluators.requireDifferentTrainerAndEvaluator} onChange={(value) => updateGroup('trainersEvaluators', { requireDifferentTrainerAndEvaluator: value })} />
        <SelectField label="Evaluator conflict policy" value={draft.trainersEvaluators.evaluatorConflictPolicy} options={conflictPolicies} onChange={(value) => updateGroup('trainersEvaluators', { evaluatorConflictPolicy: value })} />
        <SelectField label="Trainer roster source" value={draft.trainersEvaluators.trainerRosterSource} options={rosterSources} onChange={(value) => updateGroup('trainersEvaluators', { trainerRosterSource: value })} />
      </SettingsCard>

      <SettingsCard title="Compliance Core" description="How TrainArr maps programs and certificates to Compliance Core without owning regulatory truth.">
        <ToggleField label="Compliance Core enabled" checked={draft.complianceCore.complianceCoreEnabled} onChange={(value) => updateGroup('complianceCore', { complianceCoreEnabled: value })} />
        <ToggleField label="Require program mapping" checked={draft.complianceCore.requireComplianceCoreProgramMapping} onChange={(value) => updateGroup('complianceCore', { requireComplianceCoreProgramMapping: value })} />
        <ToggleField label="Allow unmapped internal programs" checked={draft.complianceCore.allowUnmappedInternalPrograms} onChange={(value) => updateGroup('complianceCore', { allowUnmappedInternalPrograms: value })} />
        <SelectField label="Citation display mode" value={draft.complianceCore.citationDisplayMode} options={citationModes} onChange={(value) => updateGroup('complianceCore', { citationDisplayMode: value })} />
        <ToggleField label="Regulatory change review required" checked={draft.complianceCore.regulatoryChangeReviewRequired} onChange={(value) => updateGroup('complianceCore', { regulatoryChangeReviewRequired: value })} />
        <ToggleField label="Auto-create review tasks from rule changes" checked={draft.complianceCore.autoCreateReviewTasksFromRuleChanges} onChange={(value) => updateGroup('complianceCore', { autoCreateReviewTasksFromRuleChanges: value })} />
      </SettingsCard>

      <SettingsCard
        title="Audit And Retention"
        description="Correction discipline, certificate revocation, voided record posture, and audit retention."
        warning="Audit and revocation settings affect compliance history."
      >
        <ToggleField label="Require correction reason" checked={draft.auditCorrection.requireCorrectionReason} onChange={(value) => updateGroup('auditCorrection', { requireCorrectionReason: value })} />
        <ToggleField label="Require admin reason for deletion" checked={draft.auditCorrection.requireAdminReasonForDeletion} onChange={(value) => updateGroup('auditCorrection', { requireAdminReasonForDeletion: value })} />
        <ToggleField label="Allow certificate revocation" checked={draft.auditCorrection.allowCertificateRevocation} onChange={(value) => updateGroup('auditCorrection', { allowCertificateRevocation: value })} />
        <ToggleField label="Revocation requires reason" checked={draft.auditCorrection.revocationRequiresReason} onChange={(value) => updateGroup('auditCorrection', { revocationRequiresReason: value })} />
        <ToggleField label="Retain voided records" checked={draft.auditCorrection.retainVoidedRecords} onChange={(value) => updateGroup('auditCorrection', { retainVoidedRecords: value })} />
        <NumberField label="Audit retention years" value={draft.auditCorrection.auditEventRetentionYears} min={0} onChange={(value) => updateGroup('auditCorrection', { auditEventRetentionYears: value })} />
      </SettingsCard>
      </fieldset>
    </section>
  )
}

function SettingsCard({
  title,
  description,
  warning,
  children,
}: {
  title: string
  description: string
  warning?: string
  children: ReactNode
}) {
  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid={`tenant-settings-section-${title.toLowerCase().replaceAll(' ', '-')}`}
    >
      <h3 className="text-base font-semibold text-foreground">{title}</h3>
      <p className="mt-1 text-sm text-muted-foreground">{description}</p>
      {warning ? (
        <p className="mt-3 rounded-md border border-amber-500/40 bg-amber-500/10 px-3 py-2 text-sm text-amber-200">
          {warning}
        </p>
      ) : null}
      <div className="mt-4 grid gap-4 md:grid-cols-2 xl:grid-cols-3">{children}</div>
    </section>
  )
}

function ToggleField({
  label,
  checked,
  onChange,
}: {
  label: string
  checked: boolean
  onChange: (value: boolean) => void
}) {
  return (
    <label className="flex items-start gap-3 rounded-md border border-border/70 px-3 py-2 text-sm">
      <input
        type="checkbox"
        className="mt-1"
        checked={checked}
        onChange={(event) => onChange(event.target.checked)}
      />
      <span className="font-medium text-foreground">{label}</span>
    </label>
  )
}

function NumberField({
  label,
  value,
  min,
  max,
  onChange,
}: {
  label: string
  value: number
  min?: number
  max?: number
  onChange: (value: number) => void
}) {
  return (
    <label className="block text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <input
        className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
        type="number"
        min={min}
        max={max}
        value={value}
        onChange={(event) => onChange(Number.parseInt(event.target.value, 10) || 0)}
      />
      <span className="mt-1 block text-xs text-muted-foreground">
        {min === 1 ? 'Must be greater than 0.' : 'Must be zero or greater.'}
      </span>
    </label>
  )
}

function TextField({
  label,
  value,
  hint,
  onChange,
}: {
  label: string
  value: string
  hint?: string
  onChange: (value: string) => void
}) {
  return (
    <label className="block text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <input
        className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
        type="text"
        value={value}
        onChange={(event) => onChange(event.target.value)}
      />
      {hint ? <span className="mt-1 block text-xs text-muted-foreground">{hint}</span> : null}
    </label>
  )
}

function SelectField({
  label,
  value,
  options,
  onChange,
}: {
  label: string
  value: string
  options: string[]
  onChange: (value: string) => void
}) {
  return (
    <label className="block text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <select
        className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        {options.map((option) => (
          <option key={option} value={option}>
            {formatLabel(option)}
          </option>
        ))}
      </select>
    </label>
  )
}

function NumberChipField({
  label,
  values,
  onChange,
}: {
  label: string
  values: number[]
  onChange: (value: number[]) => void
}) {
  const [input, setInput] = useState('')
  const normalized = [...new Set(values)].sort((left, right) => right - left)

  function addValue() {
    const parsed = Number.parseInt(input, 10)
    if (!Number.isFinite(parsed) || parsed <= 0) {
      return
    }
    onChange([...new Set([...normalized, parsed])].sort((left, right) => right - left))
    setInput('')
  }

  return (
    <div className="text-sm">
      <span className="font-medium text-foreground">{label}</span>
      <div className="mt-2 flex flex-wrap gap-2">
        {normalized.map((value) => (
          <button
            key={value}
            type="button"
            className="rounded-full border border-border px-3 py-1 text-xs text-foreground"
            onClick={() => onChange(normalized.filter((item) => item !== value))}
          >
            {value} days
          </button>
        ))}
      </div>
      <div className="mt-2 flex gap-2">
        <input
          className="w-28 rounded-md border border-input bg-background px-3 py-2 text-sm"
          type="number"
          min={1}
          value={input}
          onChange={(event) => setInput(event.target.value)}
        />
        <button
          type="button"
          className="rounded-md border border-border px-3 py-2 text-sm font-medium text-foreground"
          onClick={addValue}
        >
          Add
        </button>
      </div>
      <span className="mt-1 block text-xs text-muted-foreground">
        Positive unique days are saved in descending order.
      </span>
    </div>
  )
}

function MultiCheckField({
  label,
  values,
  options,
  onChange,
}: {
  label: string
  values: string[]
  options: string[]
  onChange: (value: string[]) => void
}) {
  const selected = new Set(values)
  return (
    <fieldset className="text-sm">
      <legend className="font-medium text-foreground">{label}</legend>
      <div className="mt-2 flex flex-wrap gap-2">
        {options.map((option) => (
          <label key={option} className="flex items-center gap-2 rounded-md border border-border px-3 py-2">
            <input
              type="checkbox"
              checked={selected.has(option)}
              onChange={(event) => {
                if (event.target.checked) {
                  onChange([...values, option])
                  return
                }
                onChange(values.filter((value) => value !== option))
              }}
            />
            <span>{formatLabel(option)}</span>
          </label>
        ))}
      </div>
    </fieldset>
  )
}

function ProviderPicker({ providerCount }: { providerCount: number }) {
  return (
    <label className="block text-sm">
      <span className="font-medium text-foreground">Trusted providers</span>
      <select
        className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
        value=""
        disabled
      >
        <option value="">
          {providerCount > 0 ? `${providerCount} provider record(s) selected` : 'No provider records available'}
        </option>
      </select>
      <span className="mt-1 block text-xs text-muted-foreground">
        Providers must come from controlled provider records.
      </span>
    </label>
  )
}

function formatLabel(value: string): string {
  return value
    .split('_')
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join(' ')
}
