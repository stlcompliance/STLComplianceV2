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
      data-testid="companion-task-submission-status"
    >
      {chips.map((chip) => (
        <span
          key={chip.kind}
          className={`rounded-full px-2.5 py-1 text-xs font-medium ${toneClass(chip.tone)}`}
          title={chip.detail}
          data-testid={`companion-submission-chip-${chip.kind}`}
        >
          {chip.label}
        </span>
      ))}
    </div>
  )
}

function toneClass(tone: MergedSubmissionChip['tone']): string {
  switch (tone) {
    case 'success':
      return 'bg-emerald-900/60 text-emerald-200'
    case 'error':
      return 'bg-rose-900/60 text-rose-200'
    case 'pending':
      return 'bg-amber-900/60 text-amber-200'
    case 'progress':
      return 'bg-sky-900/60 text-sky-200'
    default:
      return 'bg-slate-800 text-slate-200'
  }
}
