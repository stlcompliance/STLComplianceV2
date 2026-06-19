import type { MergedSubmissionChip } from '../lib/submissionState'

interface TaskSubmissionStatusBadgeProps {
  chips: MergedSubmissionChip[]
}

export function TaskSubmissionStatusBadge({ chips }: TaskSubmissionStatusBadgeProps) {
  if (chips.length === 0) {
    return null
  }

  return (
    <div
      className="mt-3 flex flex-wrap gap-2"
      data-testid="fieldcompanion-task-submission-status"
    >
      {chips.map((chip) => (
        <span
          key={chip.kind}
          className="stl-tone-badge rounded-full border px-2.5 py-1 text-xs font-medium"
          data-tone={submissionTone(chip.tone)}
          title={chip.detail}
          data-testid={`fieldcompanion-submission-chip-${chip.kind}`}
        >
          {chip.label}
        </span>
      ))}
    </div>
  )
}

function submissionTone(tone: MergedSubmissionChip['tone']): string {
  switch (tone) {
    case 'success':
      return 'success'
    case 'error':
      return 'danger'
    case 'pending':
      return 'pending'
    case 'progress':
      return 'info'
    default:
      return 'neutral'
  }
}
