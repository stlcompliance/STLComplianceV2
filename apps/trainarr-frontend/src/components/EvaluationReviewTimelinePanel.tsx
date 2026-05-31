import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { getTrainingEvaluationReviewTimeline } from '../api/client'
import type { TrainingEvaluationReviewItem } from '../api/types'

interface EvaluationReviewTimelinePanelProps {
  accessToken: string
  canReview: boolean
  selectedAssignmentId: string | null
  onSelectAssignment: (assignmentId: string) => void
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

function ReviewRow({
  item,
  isSelected,
  onSelect,
}: {
  item: TrainingEvaluationReviewItem
  isSelected: boolean
  onSelect: () => void
}) {
  return (
    <li
      className={`rounded-lg border px-3 py-2 text-sm ${
        isSelected
          ? 'border-violet-500/60 bg-violet-950/30'
          : 'border-slate-700 bg-slate-950/40 hover:border-slate-600'
      }`}
    >
      <button type="button" className="w-full text-left" onClick={onSelect}>
        <div className="flex flex-wrap items-center gap-2">
          <span
            className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ring-1 ring-inset ${resultBadgeClass(item.result)}`}
          >
            {item.result}
          </span>
          <span className="font-medium text-slate-100">{item.trainingDefinitionName}</span>
          <span className="text-xs text-slate-500">{item.assignmentStatus}</span>
        </div>
        <p className="mt-1 text-xs text-slate-400">
          {item.qualificationName} · {new Date(item.evaluatedAt).toLocaleString()}
        </p>
        {item.notes ? <p className="mt-1 text-xs text-slate-300">{item.notes}</p> : null}
        <p className="mt-1 font-mono text-[11px] text-slate-500">{item.staffarrPersonId}</p>
      </button>
      <Link
        to={`/assignments/${item.trainingAssignmentId}`}
        className="mt-2 inline-block text-xs text-violet-300 hover:text-violet-200"
      >
        Open assignment workspace →
      </Link>
    </li>
  )
}

export function EvaluationReviewTimelinePanel({
  accessToken,
  canReview,
  selectedAssignmentId,
  onSelectAssignment,
}: EvaluationReviewTimelinePanelProps) {
  const [resultFilter, setResultFilter] = useState('')

  const timelineQuery = useQuery({
    queryKey: ['trainarr-evaluation-review-timeline', accessToken, resultFilter],
    queryFn: () =>
      getTrainingEvaluationReviewTimeline(accessToken, {
        result: resultFilter || undefined,
        limit: 25,
      }),
    enabled: canReview,
  })

  if (!canReview) {
    return null
  }

  const items = timelineQuery.data?.items ?? []

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
      data-testid="evaluation-review-timeline"
    >
      <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">
        Trainer evaluation review
      </h2>
      <p className="mt-1 text-xs text-slate-500">
        Recent evaluations across assignments. Select a row to focus the assignment list, or open the
        workspace for full history and re-evaluation.
      </p>

      <label htmlFor="evaluation-review-result-filter" className="mt-3 block text-xs text-slate-400">
        Result filter
        <select
          id="evaluation-review-result-filter"
          className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
          value={resultFilter}
          onChange={(event) => setResultFilter(event.target.value)}
        >
          <option value="">All results</option>
          <option value="pass">pass</option>
          <option value="fail">fail</option>
          <option value="incomplete">incomplete</option>
        </select>
      </label>

      {timelineQuery.isLoading ? (
        <p className="mt-3 text-sm text-slate-400">Loading evaluation timeline…</p>
      ) : timelineQuery.isError ? (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(timelineQuery.error, 'Unable to load evaluation review timeline.')}
          onRetry={() => void timelineQuery.refetch()}
          retryLabel="Retry evaluation timeline"
        />
      ) : items.length === 0 ? (
        <p className="mt-3 text-sm text-slate-400">No evaluations recorded yet.</p>
      ) : (
        <ul className="mt-3 space-y-2">
          {items.map((item) => (
            <ReviewRow
              key={item.evaluationId}
              item={item}
              isSelected={selectedAssignmentId === item.trainingAssignmentId}
              onSelect={() => onSelectAssignment(item.trainingAssignmentId)}
            />
          ))}
        </ul>
      )}
    </section>
  )
}
