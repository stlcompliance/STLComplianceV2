import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { acknowledgeTrainingAssignment, listTrainingAcknowledgements } from '../api/client'
import type { TrainingAcknowledgementResponse } from '../api/types'

interface TrainingAcknowledgementsPanelProps {
  accessToken: string
  personId: string
  displayName: string
  highlightAcknowledgementId?: string | null
}

export function TrainingAcknowledgementsPanel({
  accessToken,
  personId,
  displayName,
  highlightAcknowledgementId,
}: TrainingAcknowledgementsPanelProps) {
  const queryClient = useQueryClient()
  const acknowledgementsQuery = useQuery({
    queryKey: ['staffarr-training-acknowledgements', accessToken, personId],
    queryFn: () => listTrainingAcknowledgements(accessToken, personId),
  })

  const acknowledgeMutation = useMutation({
    mutationFn: (acknowledgementId: string) =>
      acknowledgeTrainingAssignment(accessToken, acknowledgementId),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['staffarr-training-acknowledgements', accessToken, personId],
      })
    },
  })

  const pending = (acknowledgementsQuery.data ?? []).filter((item) => item.status === 'pending')
  const history = (acknowledgementsQuery.data ?? []).filter((item) => item.status !== 'pending')

  if (acknowledgementsQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading training acknowledgements…</p>
  }

  if (acknowledgementsQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(
          acknowledgementsQuery.error,
          `Unable to load training acknowledgements for ${displayName}.`,
        )}
        onRetry={() => void acknowledgementsQuery.refetch()}
        retryLabel="Retry acknowledgements"
      />
    )
  }

  return (
    <div className="space-y-6" data-testid="training-acknowledgements-panel">
      <p className="text-sm text-slate-400">
        TrainArr publishes assignment receipts here. Acknowledge each pending item before evidence upload in TrainArr.
      </p>

      <section className="space-y-3">
        <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">Pending</h2>
        {pending.length === 0 ? (
          <p className="text-sm text-slate-500">No pending training acknowledgements.</p>
        ) : (
          pending.map((item) => (
            <AcknowledgementCard
              key={item.acknowledgementId}
              item={item}
              highlighted={item.acknowledgementId === highlightAcknowledgementId}
              isAcknowledging={acknowledgeMutation.isPending}
              onAcknowledge={() => acknowledgeMutation.mutate(item.acknowledgementId)}
            />
          ))
        )}
      </section>

      {history.length > 0 ? (
        <section className="space-y-3">
          <h2 className="text-sm font-semibold uppercase tracking-wide text-slate-400">History</h2>
          {history.map((item) => (
            <AcknowledgementCard key={item.acknowledgementId} item={item} />
          ))}
        </section>
      ) : null}
    </div>
  )
}

function AcknowledgementCard({
  item,
  highlighted = false,
  isAcknowledging = false,
  onAcknowledge,
}: {
  item: TrainingAcknowledgementResponse
  highlighted?: boolean
  isAcknowledging?: boolean
  onAcknowledge?: () => void
}) {
  return (
    <article
      className={`rounded-xl border bg-slate-900/60 p-4 ${
        highlighted ? 'border-violet-500' : 'border-slate-700'
      }`}
      data-testid={`training-acknowledgement-${item.acknowledgementId}`}
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="font-medium text-slate-100">{item.trainingTitle}</h3>
          <p className="mt-1 text-sm text-slate-400">{item.summary}</p>
        </div>
        <span className="rounded-full border border-slate-600 px-2 py-0.5 text-xs uppercase text-slate-300">
          {item.status}
        </span>
      </div>
      <dl className="mt-3 grid gap-1 text-xs text-slate-500 sm:grid-cols-2">
        <div>Reason: {item.assignmentReason}</div>
        <div>Requested: {new Date(item.requestedAt).toLocaleString()}</div>
        {item.dueAt ? <div>Due: {new Date(item.dueAt).toLocaleString()}</div> : null}
        {item.acknowledgedAt ? (
          <div>Acknowledged: {new Date(item.acknowledgedAt).toLocaleString()}</div>
        ) : null}
      </dl>
      {item.status === 'pending' && onAcknowledge ? (
        <button
          type="button"
          className="mt-4 rounded-md bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
          disabled={isAcknowledging}
          onClick={onAcknowledge}
        >
          {isAcknowledging ? 'Acknowledging…' : 'Acknowledge assignment'}
        </button>
      ) : null}
    </article>
  )
}
