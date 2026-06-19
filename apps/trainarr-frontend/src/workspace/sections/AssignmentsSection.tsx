import { canCompleteAssignment } from '../../auth/sessionStorage'
import { AssignmentsPanel } from '../../components/AssignmentsPanel'
import { EvaluationReviewTimelinePanel } from '../../components/EvaluationReviewTimelinePanel'
import { EvidenceCapturePanel } from '../../components/EvidenceCapturePanel'
import { ManualAssignmentPanel } from '../../components/ManualAssignmentPanel'
import { SignoffEvaluationPanel } from '../../components/SignoffEvaluationPanel'
import { useLocation } from 'react-router-dom'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }
type AssignmentsViewMode = 'manual' | 'queue' | 'evaluation' | 'instructor' | 'evaluator'

export function AssignmentsSection({ state }: Props) {
  const s = state
  const selectedAssignment = s.selectedAssignment
  const location = useLocation()
  const mode: AssignmentsViewMode = location.pathname.startsWith('/assignments/evaluation')
    ? 'evaluation'
    : location.pathname.startsWith('/evaluator')
      ? 'evaluator'
      : location.pathname.startsWith('/instructor')
        ? 'instructor'
    : location.pathname.startsWith('/assignments/queue')
      ? 'queue'
      : 'manual'

  const banner =
    mode === 'instructor'
      ? {
          title: 'Instructor Console',
          text: 'Assign learners, capture attendance, and complete classroom-style signoff flows.',
        }
      : mode === 'evaluator'
        ? {
            title: 'Evaluator Console',
            text: 'Review practical performance, record outcomes, and manage remediation.',
          }
        : mode === 'queue'
          ? {
              title: 'Course Player Queue',
              text: 'Pick up assigned learning, complete evidence, and work through signoffs.',
            }
          : {
              title: 'Course Player',
              text: 'Continue assigned learning, upload evidence, and complete the current step.',
            }

  return (
    <div className="space-y-6">
      <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-gradient-to-br from-[var(--color-bg-surface)] to-[var(--color-bg-surface-elevated)] p-5">
        <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">{banner.title}</h2>
        <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{banner.text}</p>
      </section>

      {mode === 'manual' || mode === 'instructor' ? (
        <ManualAssignmentPanel
          definitions={s.definitionsQuery.data ?? []}
          staffarrPersonId={s.manualAssignmentPersonId}
          onStaffarrPersonIdChange={(value) => {
            s.setManualAssignmentPersonId(value)
            s.setManualQualificationCheck(null)
          }}
          selectedDefinitionId={s.manualAssignmentDefinitionId}
          onSelectDefinition={(value) => {
            s.setManualAssignmentDefinitionId(value)
            s.setManualQualificationCheck(null)
          }}
          qualificationCheck={s.manualQualificationCheck}
          isCheckingQualification={s.manualQualificationCheckMutation.isPending}
          onRunQualificationCheck={() => s.manualQualificationCheckMutation.mutate()}
          rulePackKey={s.rulePackKey}
          onRulePackKeyChange={s.setRulePackKey}
          rulePackOptions={s.rulePackOptions}
          personPickerOptions={s.personPickerOptions}
          onCreateAssignment={() => s.createManualAssignmentMutation.mutate()}
          isCreating={s.createManualAssignmentMutation.isPending}
          canManage={s.canManage}
        />
      ) : null}

      {mode !== 'manual' ? (
        <EvaluationReviewTimelinePanel
          accessToken={s.accessToken}
          canReview={s.canEvaluate}
          selectedAssignmentId={s.selectedAssignmentId}
          onSelectAssignment={s.setSelectedAssignmentId}
        />
      ) : null}

      <div className="grid gap-6 lg:grid-cols-2">
      <AssignmentsPanel
        assignments={s.assignments}
        selectedAssignmentId={s.selectedAssignmentId}
        onSelectAssignment={s.setSelectedAssignmentId}
        canManage={s.canManage}
        canCompleteForAssignment={(assignment) =>
          Boolean(
            s.me &&
              canCompleteAssignment(
                s.me.tenantRoleKey,
                s.me.isPlatformAdmin,
                assignment.staffarrPersonId,
                s.me.personId,
              ) &&
              (assignment.assignmentId !== selectedAssignment?.assignmentId ||
                Boolean(selectedAssignment?.completionRequirementsMet)),
          )
        }
        onComplete={(assignmentId) => s.completeAssignmentMutation.mutate(assignmentId)}
        completingAssignmentId={
          s.completeAssignmentMutation.isPending ? s.completeAssignmentMutation.variables ?? null : null
        }
      />

      <div className="space-y-6">
        <section className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-[var(--color-text-muted)]">Assignment detail</h2>
          {!selectedAssignment ? (
            <p className="mt-3 text-sm text-[var(--color-text-muted)]">Select an assignment to view details.</p>
          ) : (
            <dl className="mt-3 space-y-2 text-sm">
              <div>
                <dt className="text-[var(--color-text-muted)]">Training</dt>
                <dd className="text-[var(--color-text-primary)]">{selectedAssignment.trainingDefinitionName}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Qualification</dt>
                <dd className="text-[var(--color-text-primary)]">{selectedAssignment.qualificationName}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Status</dt>
                <dd className="text-[var(--color-text-primary)]">{selectedAssignment.status}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Evidence on file</dt>
                <dd className="text-[var(--color-text-primary)]">{selectedAssignment.evidenceCount}</dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Completion gate</dt>
                <dd className={selectedAssignment.completionRequirementsMet ? 'text-[var(--color-success)]' : 'text-[var(--color-warning)]'}>
                  {selectedAssignment.completionRequirementsMet ? 'Ready to complete' : 'Evaluation + signoffs required'}
                </dd>
              </div>
              <div>
                <dt className="text-[var(--color-text-muted)]">Person</dt>
                <dd className="font-mono text-xs text-[var(--color-text-secondary)]">{selectedAssignment.staffarrPersonId}</dd>
              </div>
              {selectedAssignment.staffarrIncidentRemediationId ? (
                <div>
                  <dt className="text-[var(--color-text-muted)]">Remediation</dt>
                  <dd className="font-mono text-xs text-[var(--color-info)]">
                    {selectedAssignment.staffarrIncidentRemediationId}
                  </dd>
                </div>
              ) : null}
              {selectedAssignment.blockerPublicationId ? (
                <div>
                  <dt className="text-[var(--color-text-muted)]">StaffArr blocker publication</dt>
                  <dd className="font-mono text-xs text-[var(--color-text-secondary)]">{selectedAssignment.blockerPublicationId}</dd>
                </div>
              ) : null}
              {selectedAssignment.completedAt ? (
                <div>
                  <dt className="text-[var(--color-text-muted)]">Completed</dt>
                  <dd className="text-[var(--color-text-primary)]">{new Date(selectedAssignment.completedAt).toLocaleString()}</dd>
                </div>
              ) : null}
              {selectedAssignment.qualificationIssue ? (
                <div
                  className={
                    selectedAssignment.qualificationIssue.status === 'issued'
                      ? 'rounded-lg border border-[var(--tone-success-border)] bg-[var(--tone-success-bg)] p-3'
                      : selectedAssignment.qualificationIssue.status === 'suspended'
                        ? 'rounded-lg border border-[var(--tone-warning-border)] bg-[var(--tone-warning-bg)] p-3'
                        : 'rounded-lg border border-[var(--tone-danger-border)] bg-[var(--tone-danger-bg)] p-3'
                  }
                >
                  <dt className="text-xs font-semibold uppercase tracking-wide text-[var(--tone-success-text)]">
                    Qualification {selectedAssignment.qualificationIssue.status.replace('_', ' ')}
                  </dt>
                  <dd className="mt-1 text-sm text-[var(--tone-success-text)]">
                    {selectedAssignment.qualificationIssue.qualificationName} ·{' '}
                    {new Date(selectedAssignment.qualificationIssue.issuedAt).toLocaleString()}
                  </dd>
                  {selectedAssignment.qualificationIssue.statusChangedAt ? (
                    <dd className="mt-1 text-xs text-[var(--color-text-secondary)]">
                      Status changed{' '}
                      {new Date(selectedAssignment.qualificationIssue.statusChangedAt).toLocaleString()}
                    </dd>
                  ) : null}
                  {selectedAssignment.qualificationIssue.lifecycleReason ? (
                    <dd className="mt-1 text-xs text-[var(--color-text-muted)]">
                      {selectedAssignment.qualificationIssue.lifecycleReason}
                    </dd>
                  ) : null}
                  <dd className="mt-1 font-mono text-xs text-[var(--tone-success-text)]">
                    StaffArr grant publication {selectedAssignment.qualificationIssue.grantPublicationId}
                  </dd>
                  {selectedAssignment.qualificationIssue.lifecyclePublicationId ? (
                    <dd className="mt-1 font-mono text-xs text-[var(--color-info)]">
                      Lifecycle publication {selectedAssignment.qualificationIssue.lifecyclePublicationId}
                    </dd>
                  ) : null}
                  {s.canQualifications &&
                  ['issued', 'suspended'].includes(selectedAssignment.qualificationIssue.status) ? (
                    <div className="mt-3 space-y-2 border-t border-[var(--color-border-subtle)] pt-3">
                      <label className="grid gap-1 text-xs text-[var(--color-text-muted)]">
                        Lifecycle reason (optional)
                        <textarea
                          value={s.lifecycleReason}
                          onChange={(event) => s.setLifecycleReason(event.target.value)}
                          rows={2}
                          className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-2 py-1 text-sm text-[var(--color-text-primary)]"
                        />
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {selectedAssignment.qualificationIssue.status === 'issued' ? (
                          <button
                            type="button"
                            disabled={s.suspendQualificationMutation.isPending}
                            className="rounded border border-[var(--color-warning-border)] px-2 py-1 text-xs text-[var(--color-warning-text)] hover:bg-[var(--color-warning-bg)] disabled:opacity-50"
                            onClick={() =>
                              s.suspendQualificationMutation.mutate(
                                selectedAssignment.qualificationIssue!.qualificationIssueId,
                              )
                            }
                          >
                            Suspend
                          </button>
                        ) : null}
                        <button
                          type="button"
                          disabled={s.revokeQualificationMutation.isPending}
                          className="rounded border border-[var(--color-destructive-border)] px-2 py-1 text-xs text-[var(--color-destructive-text)] hover:bg-[var(--color-destructive-bg)] disabled:opacity-50"
                          onClick={() =>
                            s.revokeQualificationMutation.mutate(
                              selectedAssignment.qualificationIssue!.qualificationIssueId,
                            )
                          }
                        >
                          Revoke
                        </button>
                        <button
                          type="button"
                          disabled={s.expireQualificationMutation.isPending}
                          className="rounded border border-[var(--color-border-subtle)] px-2 py-1 text-xs text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:opacity-50"
                          onClick={() =>
                            s.expireQualificationMutation.mutate(
                              selectedAssignment.qualificationIssue!.qualificationIssueId,
                            )
                          }
                        >
                          Expire
                        </button>
                      </div>
                    </div>
                  ) : null}
                </div>
              ) : null}
            </dl>
          )}
        </section>

        {mode !== 'manual' ? (
          <EvidenceCapturePanel
            assignment={selectedAssignment ?? null}
            evidence={s.evidenceQuery.data ?? []}
            evidenceTypeKey={s.evidenceTypeKey}
            notes={s.evidenceNotes}
            selectedFileName={s.evidenceFile?.name ?? null}
            onEvidenceTypeKeyChange={s.setEvidenceTypeKey}
            onNotesChange={s.setEvidenceNotes}
            onSelectFile={s.setEvidenceFile}
            onUploadEvidence={() => s.uploadEvidenceMutation.mutate()}
            isUploading={s.uploadEvidenceMutation.isPending}
            canUpload={Boolean(s.canUploadForAssignment)}
          />
        ) : null}

        {mode !== 'queue' ? (
          <SignoffEvaluationPanel
            assignment={selectedAssignment ?? null}
            evaluationHistory={s.evaluationHistoryQuery.data?.items ?? []}
            isLoadingHistory={s.evaluationHistoryQuery.isLoading}
            evaluationResult={s.evaluationResult}
            evaluationScore={s.evaluationScore}
            evaluationNotes={s.evaluationNotes}
            signoffNotes={s.signoffNotes}
            onEvaluationResultChange={s.setEvaluationResult}
            onEvaluationScoreChange={s.setEvaluationScore}
            onEvaluationNotesChange={s.setEvaluationNotes}
            onSignoffNotesChange={s.setSignoffNotes}
            onSubmitEvaluation={() => s.submitEvaluationMutation.mutate()}
            onSubmitTraineeSignoff={() => s.submitTraineeSignoffMutation.mutate()}
            onSubmitTrainerSignoff={() => s.submitTrainerSignoffMutation.mutate()}
            isSubmittingEvaluation={s.submitEvaluationMutation.isPending}
            isSubmittingTraineeSignoff={s.submitTraineeSignoffMutation.isPending}
            isSubmittingTrainerSignoff={s.submitTrainerSignoffMutation.isPending}
            canSubmitEvaluation={s.canEvaluate}
            canSubmitTraineeSignoff={Boolean(s.canTraineeSign)}
            canSubmitTrainerSignoff={s.canTrainerSign}
          />
        ) : null}
      </div>
    </div>
    </div>
  )
}
