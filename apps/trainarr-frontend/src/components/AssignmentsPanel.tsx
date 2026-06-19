import type { TrainingAssignmentSummaryResponse } from '../api/types'

interface AssignmentsPanelProps {
  assignments: TrainingAssignmentSummaryResponse[]
  selectedAssignmentId: string | null
  onSelectAssignment: (assignmentId: string) => void
  canManage: boolean
  canCompleteForAssignment?: (assignment: TrainingAssignmentSummaryResponse) => boolean
  onComplete?: (assignmentId: string) => void
  completingAssignmentId?: string | null
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'completed':
      return 'bg-emerald-900/60 text-emerald-200'
    case 'assigned':
    case 'in_progress':
      return 'bg-amber-900/60 text-amber-200'
    default:
      return 'bg-slate-700 text-slate-200'
  }
}

export function AssignmentsPanel({
  assignments,
  selectedAssignmentId,
  onSelectAssignment,
  canManage,
  canCompleteForAssignment,
  onComplete,
  completingAssignmentId,
}: AssignmentsPanelProps) {
  if (assignments.length === 0) {
    return (
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Course assignments</h2>
        <p className="mt-3 text-sm text-slate-400">No course assignments yet.</p>
      </section>
    )
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Course assignments</h2>
      <ul className="mt-3 space-y-2">
        {assignments.map((assignment) => {
          const isSelected = assignment.assignmentId === selectedAssignmentId
          const canComplete =
            (assignment.status === 'assigned' || assignment.status === 'in_progress') &&
            Boolean(onComplete) &&
            (canCompleteForAssignment?.(assignment) ?? true)
          return (
            <li
              key={assignment.assignmentId}
              className={`rounded-lg border p-3 ${isSelected ? 'border-sky-500 bg-sky-950/30' : 'border-slate-700 bg-slate-950/40'}`}
            >
              <button
                type="button"
                className="w-full text-left"
                onClick={() => onSelectAssignment(assignment.assignmentId)}
              >
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <p className="font-medium text-slate-100">{assignment.trainingDefinitionName}</p>
                    <p className="mt-1 text-xs text-slate-400">
                      Person {assignment.staffarrPersonId.slice(0, 8)}… · {assignment.assignmentReason.replace('_', ' ')}
                    </p>
                  </div>
                  <span className={`rounded px-2 py-0.5 text-xs font-medium ${statusBadgeClass(assignment.status)}`}>
                    {assignment.status.replace('_', ' ')}
                  </span>
                </div>
              </button>
              {canComplete && (
                  <button
                  type="button"
                  className="mt-2 rounded bg-emerald-700 px-3 py-1 text-xs font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
                  disabled={completingAssignmentId === assignment.assignmentId}
                  onClick={() => onComplete?.(assignment.assignmentId)}
                >
                  {completingAssignmentId === assignment.assignmentId ? 'Completing…' : 'Complete lesson'}
                </button>
              )}
              {canManage && assignment.staffarrIncidentRemediationId && (
                <p className="mt-2 text-xs text-violet-300">
                  Linked remediation {assignment.staffarrIncidentRemediationId.slice(0, 8)}…
                </p>
              )}
            </li>
          )
        })}
      </ul>
    </section>
  )
}
