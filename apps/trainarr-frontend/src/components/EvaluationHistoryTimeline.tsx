import type { TrainingEvaluationHistoryItem } from '../api/types'

interface EvaluationHistoryTimelineProps {
  items: TrainingEvaluationHistoryItem[]
  emptyMessage?: string
}

function resultBadgeClass(result: string): string {
  switch (result) {
    case 'pass':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'fail':
      return 'bg-rose-500/20 text-rose-300 ring-rose-500/40'
    default:
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
  }
}

export function EvaluationHistoryTimeline({
  items,
  emptyMessage = 'No evaluation history yet.',
}: EvaluationHistoryTimelineProps) {
  if (items.length === 0) {
    return <p className="text-sm text-slate-400">{emptyMessage}</p>
  }

  return (
    <ol className="space-y-2" data-testid="evaluation-history-timeline">
      {items.map((item) => (
        <li
          key={item.entryId}
          className="rounded-lg border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
        >
          <div className="flex flex-wrap items-center gap-2">
            <span
              className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${resultBadgeClass(item.result)}`}
            >
              {item.result}
            </span>
            {item.isCurrent ? (
              <span className="text-xs font-medium uppercase tracking-wide text-violet-300">Current</span>
            ) : (
              <span className="text-xs text-[var(--color-text-muted)]">Superseded</span>
            )}
            {item.score != null ? (
              <span className="text-xs text-slate-400">Score {item.score}</span>
            ) : null}
          </div>
          {item.notes ? <p className="mt-1 text-xs text-slate-300">{item.notes}</p> : null}
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Evaluated {new Date(item.evaluatedAt).toLocaleString()}
            {item.supersededAt
              ? ` · superseded ${new Date(item.supersededAt).toLocaleString()}`
              : ''}
          </p>
        </li>
      ))}
    </ol>
  )
}
