import { canCompleteAssignment } from '../../auth/sessionStorage'
import { AssignmentsPanel } from '../../components/AssignmentsPanel'
import { EvaluationReviewTimelinePanel } from '../../components/EvaluationReviewTimelinePanel'
import { EvidenceCapturePanel } from '../../components/EvidenceCapturePanel'
import { ManualAssignmentPanel } from '../../components/ManualAssignmentPanel'
import { SignoffEvaluationPanel } from '../../components/SignoffEvaluationPanel'
import type { TrainArrWorkspaceState } from '../useTrainArrWorkspaceState'

type Props = { state: TrainArrWorkspaceState }

export function AssignmentsSection({ state }: Props) {
  const s = state
  const selectedAssignment = s.selectedAssignment

  return (
    <div className="space-y-6">
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

      <EvaluationReviewTimelinePanel
        accessToken={s.accessToken}
        canReview={s.canEvaluate}
        selectedAssignmentId={s.selectedAssignmentId}
        onSelectAssignment={s.setSelectedAssignmentId}
      />

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
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Assignment detail</h2>
          {!selectedAssignment ? (
            <p className="mt-3 text-sm text-slate-400">Select an assignment to view details.</p>
          ) : (
            <dl className="mt-3 space-y-2 text-sm">
              <div>
                <dt className="text-slate-500">Training</dt>
                <dd className="text-slate-100">{selectedAssignment.trainingDefinitionName}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Qualification</dt>
                <dd className="text-slate-100">{selectedAssignment.qualificationName}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Status</dt>
                <dd className="text-slate-100">{selectedAssignment.status}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Evidence on file</dt>
                <dd className="text-slate-100">{selectedAssignment.evidenceCount}</dd>
              </div>
              <div>
                <dt className="text-slate-500">Completion gate</dt>
                <dd className={selectedAssignment.completionRequirementsMet ? 'text-emerald-300' : 'text-amber-300'}>
                  {selectedAssignment.completionRequirementsMet ? 'Ready to complete' : 'Evaluation + signoffs required'}
                </dd>
              </div>
              <div>
                <dt className="text-slate-500">Person</dt>
                <dd className="font-mono text-xs text-slate-300">{selectedAssignment.staffarrPersonId}</dd>
              </div>
              {selectedAssignment.staffarrIncidentRemediationId ? (
                <div>
                  <dt className="text-slate-500">Remediation</dt>
                  <dd className="font-mono text-xs text-violet-300">
                    {selectedAssignment.staffarrIncidentRemediationId}
                  </dd>
                </div>
              ) : null}
              {selectedAssignment.blockerPublicationId ? (
                <div>
                  <dt className="text-slate-500">StaffArr blocker publication</dt>
                  <dd className="font-mono text-xs text-slate-300">{selectedAssignment.blockerPublicationId}</dd>
                </div>
              ) : null}
              {selectedAssignment.completedAt ? (
                <div>
                  <dt className="text-slate-500">Completed</dt>
                  <dd className="text-slate-100">{new Date(selectedAssignment.completedAt).toLocaleString()}</dd>
                </div>
              ) : null}
              {selectedAssignment.qualificationIssue ? (
                <div
                  className={
                    selectedAssignment.qualificationIssue.status === 'issued'
                      ? 'rounded-lg border border-emerald-800/60 bg-emerald-950/30 p-3'
                      : selectedAssignment.qualificationIssue.status === 'suspended'
                        ? 'rounded-lg border border-amber-800/60 bg-amber-950/30 p-3'
                        : 'rounded-lg border border-red-800/60 bg-red-950/30 p-3'
                  }
                >
                  <dt className="text-xs font-semibold uppercase tracking-wide text-emerald-400">
                    Qualification {selectedAssignment.qualificationIssue.status.replace('_', ' ')}
                  </dt>
                  <dd className="mt-1 text-sm text-emerald-100">
                    {selectedAssignment.qualificationIssue.qualificationName} ·{' '}
                    {new Date(selectedAssignment.qualificationIssue.issuedAt).toLocaleString()}
                  </dd>
                  {selectedAssignment.qualificationIssue.statusChangedAt ? (
                    <dd className="mt-1 text-xs text-slate-300">
                      Status changed{' '}
                      {new Date(selectedAssignment.qualificationIssue.statusChangedAt).toLocaleString()}
                    </dd>
                  ) : null}
                  {selectedAssignment.qualificationIssue.lifecycleReason ? (
                    <dd className="mt-1 text-xs text-slate-400">
                      {selectedAssignment.qualificationIssue.lifecycleReason}
                    </dd>
                  ) : null}
                  <dd className="mt-1 font-mono text-xs text-emerald-300/80">
                    StaffArr grant publication {selectedAssignment.qualificationIssue.grantPublicationId}
                  </dd>
                  {selectedAssignment.qualificationIssue.lifecyclePublicationId ? (
                    <dd className="mt-1 font-mono text-xs text-violet-300/80">
                      Lifecycle publication {selectedAssignment.qualificationIssue.lifecyclePublicationId}
                    </dd>
                  ) : null}
                  {s.canQualifications &&
                  ['issued', 'suspended'].includes(selectedAssignment.qualificationIssue.status) ? (
                    <div className="mt-3 space-y-2 border-t border-slate-700/60 pt-3">
                      <label className="grid gap-1 text-xs text-slate-400">
                        Lifecycle reason (optional)
                        <textarea
                          value={s.lifecycleReason}
                          onChange={(event) => s.setLifecycleReason(event.target.value)}
                          rows={2}
                          className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-white"
                        />
                      </label>
                      <div className="flex flex-wrap gap-2">
                        {selectedAssignment.qualificationIssue.status === 'issued' ? (
                          <button
                            type="button"
                            disabled={s.suspendQualificationMutation.isPending}
                            className="rounded border border-amber-700 px-2 py-1 text-xs text-amber-100 hover:bg-amber-950/40 disabled:opacity-50"
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
                          className="rounded border border-red-700 px-2 py-1 text-xs text-red-100 hover:bg-red-950/40 disabled:opacity-50"
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
                          className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 hover:bg-slate-800 disabled:opacity-50"
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
      </div>
    </div>
    </div>
  )
}
