import type { TrainingAssignmentDetailResponse, TrainingEvaluationHistoryItem } from '../api/types'
import { EvaluationHistoryTimeline } from './EvaluationHistoryTimeline'

interface SignoffEvaluationPanelProps {
  assignment: TrainingAssignmentDetailResponse | null
  evaluationHistory: TrainingEvaluationHistoryItem[]
  isLoadingHistory: boolean
  evaluationResult: string
  evaluationScore: string
  evaluationNotes: string
  signoffNotes: string
  onEvaluationResultChange: (value: string) => void
  onEvaluationScoreChange: (value: string) => void
  onEvaluationNotesChange: (value: string) => void
  onSignoffNotesChange: (value: string) => void
  onSubmitEvaluation: () => void
  onSubmitTraineeSignoff: () => void
  onSubmitTrainerSignoff: () => void
  isSubmittingEvaluation: boolean
  isSubmittingTraineeSignoff: boolean
  isSubmittingTrainerSignoff: boolean
  canSubmitEvaluation: boolean
  canSubmitTraineeSignoff: boolean
  canSubmitTrainerSignoff: boolean
}

function hasSignoff(assignment: TrainingAssignmentDetailResponse, role: string): boolean {
  return assignment.signoffs.some((s) => s.signoffRole === role)
}

function EvaluationSubmitForm({
  evaluationResult,
  evaluationScore,
  evaluationNotes,
  isSubmittingEvaluation,
  onEvaluationResultChange,
  onEvaluationScoreChange,
  onEvaluationNotesChange,
  onSubmitEvaluation,
  submitLabel,
}: {
  evaluationResult: string
  evaluationScore: string
  evaluationNotes: string
  isSubmittingEvaluation: boolean
  onEvaluationResultChange: (value: string) => void
  onEvaluationScoreChange: (value: string) => void
  onEvaluationNotesChange: (value: string) => void
  onSubmitEvaluation: () => void
  submitLabel: string
}) {
  return (
    <div className="space-y-2">
      <label htmlFor="evaluation-submit-result" className="block text-xs text-slate-400">
        Result
        <select
          id="evaluation-submit-result"
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          value={evaluationResult}
          onChange={(e) => onEvaluationResultChange(e.target.value)}
        >
          <option value="pass">pass</option>
          <option value="fail">fail</option>
          <option value="incomplete">incomplete</option>
        </select>
      </label>
      <label htmlFor="evaluation-submit-score" className="block text-xs text-slate-400">
        Score (optional)
        <input
          id="evaluation-submit-score"
          type="number"
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          value={evaluationScore}
          onChange={(e) => onEvaluationScoreChange(e.target.value)}
        />
      </label>
      <label htmlFor="evaluation-submit-notes" className="block text-xs text-slate-400">
        Notes
        <input
          id="evaluation-submit-notes"
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          value={evaluationNotes}
          onChange={(e) => onEvaluationNotesChange(e.target.value)}
        />
      </label>
      <button
        type="button"
        className="rounded bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
        disabled={isSubmittingEvaluation}
        onClick={onSubmitEvaluation}
      >
        {isSubmittingEvaluation ? 'Submitting…' : submitLabel}
      </button>
    </div>
  )
}

export function SignoffEvaluationPanel({
  assignment,
  evaluationHistory,
  isLoadingHistory,
  evaluationResult,
  evaluationScore,
  evaluationNotes,
  signoffNotes,
  onEvaluationResultChange,
  onEvaluationScoreChange,
  onEvaluationNotesChange,
  onSignoffNotesChange,
  onSubmitEvaluation,
  onSubmitTraineeSignoff,
  onSubmitTrainerSignoff,
  isSubmittingEvaluation,
  isSubmittingTraineeSignoff,
  isSubmittingTrainerSignoff,
  canSubmitEvaluation,
  canSubmitTraineeSignoff,
  canSubmitTrainerSignoff,
}: SignoffEvaluationPanelProps) {
  if (!assignment) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Evaluation & signoffs</h2>
        <p className="mt-3 text-sm text-slate-400">Select an assignment to record evaluation and signoffs.</p>
      </section>
    )
  }

  const assignmentOpen = assignment.status === 'assigned' || assignment.status === 'in_progress'
  const traineeSigned = hasSignoff(assignment, 'trainee')
  const trainerSigned = hasSignoff(assignment, 'trainer')
  const hasEvaluation = Boolean(assignment.evaluation)
  const showInitialSubmit = canSubmitEvaluation && assignmentOpen && !hasEvaluation
  const showReviseSubmit = canSubmitEvaluation && assignmentOpen && hasEvaluation

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Evaluation & signoffs</h2>
      <p className="mt-1 text-xs text-slate-500">
        Completion gate:{' '}
        {assignment.completionRequirementsMet ? (
          <span className="text-emerald-400">requirements met</span>
        ) : (
          <span className="text-amber-300">passing evaluation + trainee + trainer signoffs required</span>
        )}
      </p>

      <div className="mt-4 space-y-3 border-t border-slate-700 pt-4">
        <h3 className="text-xs font-semibold uppercase text-slate-500">Evaluation history</h3>
        {isLoadingHistory ? (
          <p className="text-sm text-slate-400">Loading evaluation history…</p>
        ) : (
          <EvaluationHistoryTimeline items={evaluationHistory} />
        )}

        {showInitialSubmit ? (
          <EvaluationSubmitForm
            evaluationResult={evaluationResult}
            evaluationScore={evaluationScore}
            evaluationNotes={evaluationNotes}
            isSubmittingEvaluation={isSubmittingEvaluation}
            onEvaluationResultChange={onEvaluationResultChange}
            onEvaluationScoreChange={onEvaluationScoreChange}
            onEvaluationNotesChange={onEvaluationNotesChange}
            onSubmitEvaluation={onSubmitEvaluation}
            submitLabel="Submit evaluation"
          />
        ) : null}

        {showReviseSubmit ? (
          <div className="space-y-2 border-t border-slate-800 pt-3">
            <p className="text-xs text-slate-500">Revise evaluation (prior result moves to history).</p>
            <EvaluationSubmitForm
              evaluationResult={evaluationResult}
              evaluationScore={evaluationScore}
              evaluationNotes={evaluationNotes}
              isSubmittingEvaluation={isSubmittingEvaluation}
              onEvaluationResultChange={onEvaluationResultChange}
              onEvaluationScoreChange={onEvaluationScoreChange}
              onEvaluationNotesChange={onEvaluationNotesChange}
              onSubmitEvaluation={onSubmitEvaluation}
              submitLabel="Submit revised evaluation"
            />
          </div>
        ) : null}
      </div>

      <div className="mt-4 space-y-3 border-t border-slate-700 pt-4">
        <h3 className="text-xs font-semibold uppercase text-slate-500">Signoffs</h3>
        <ul className="space-y-2 text-sm">
          <li className="rounded-lg border border-slate-700 bg-slate-950/40 p-3">
            <span className="font-medium text-slate-100">Trainee</span>
            <span className="ml-2 text-xs text-slate-400">{traineeSigned ? 'signed' : 'pending'}</span>
          </li>
          <li className="rounded-lg border border-slate-700 bg-slate-950/40 p-3">
            <span className="font-medium text-slate-100">Trainer</span>
            <span className="ml-2 text-xs text-slate-400">{trainerSigned ? 'signed' : 'pending'}</span>
          </li>
        </ul>

        {assignmentOpen && (canSubmitTraineeSignoff || canSubmitTrainerSignoff) && (
          <label htmlFor="signoff-notes" className="block text-xs text-slate-400">
            Signoff notes (optional)
            <input
              id="signoff-notes"
              className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
              value={signoffNotes}
              onChange={(e) => onSignoffNotesChange(e.target.value)}
            />
          </label>
        )}

        {canSubmitTraineeSignoff && assignmentOpen && !traineeSigned && (
          <button
            type="button"
            className="mr-2 rounded bg-slate-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-slate-600 disabled:opacity-50"
            disabled={isSubmittingTraineeSignoff}
            onClick={onSubmitTraineeSignoff}
          >
            {isSubmittingTraineeSignoff ? 'Signing…' : 'Trainee signoff'}
          </button>
        )}

        {canSubmitTrainerSignoff && assignmentOpen && !trainerSigned && (
          <button
            type="button"
            className="rounded bg-violet-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-violet-600 disabled:opacity-50"
            disabled={isSubmittingTrainerSignoff}
            onClick={onSubmitTrainerSignoff}
          >
            {isSubmittingTrainerSignoff ? 'Signing…' : 'Trainer signoff'}
          </button>
        )}
      </div>
    </section>
  )
}
