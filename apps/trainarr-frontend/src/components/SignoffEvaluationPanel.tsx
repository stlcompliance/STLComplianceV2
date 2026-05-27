import type { TrainingAssignmentDetailResponse } from '../api/types'

interface SignoffEvaluationPanelProps {
  assignment: TrainingAssignmentDetailResponse | null
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

export function SignoffEvaluationPanel({
  assignment,
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
        <h3 className="text-xs font-semibold uppercase text-slate-500">Evaluation</h3>
        {assignment.evaluation ? (
          <div className="rounded-lg border border-slate-700 bg-slate-950/40 p-3 text-sm">
            <p className="font-medium text-slate-100">Result: {assignment.evaluation.result}</p>
            {assignment.evaluation.score != null && (
              <p className="mt-1 text-xs text-slate-400">Score: {assignment.evaluation.score}</p>
            )}
            {assignment.evaluation.notes && (
              <p className="mt-1 text-xs text-slate-300">{assignment.evaluation.notes}</p>
            )}
            <p className="mt-1 text-xs text-slate-500">
              {new Date(assignment.evaluation.evaluatedAt).toLocaleString()}
            </p>
          </div>
        ) : (
          <p className="text-sm text-slate-400">No evaluation recorded.</p>
        )}

        {canSubmitEvaluation && assignmentOpen && !assignment.evaluation && (
          <div className="space-y-2">
            <label className="block text-xs text-slate-400">
              Result
              <select
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                value={evaluationResult}
                onChange={(e) => onEvaluationResultChange(e.target.value)}
              >
                <option value="pass">pass</option>
                <option value="fail">fail</option>
                <option value="incomplete">incomplete</option>
              </select>
            </label>
            <label className="block text-xs text-slate-400">
              Score (optional)
              <input
                type="number"
                className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
                value={evaluationScore}
                onChange={(e) => onEvaluationScoreChange(e.target.value)}
              />
            </label>
            <label className="block text-xs text-slate-400">
              Notes
              <input
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
              {isSubmittingEvaluation ? 'Submitting…' : 'Submit evaluation'}
            </button>
          </div>
        )}
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
          <label className="block text-xs text-slate-400">
            Signoff notes (optional)
            <input
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
