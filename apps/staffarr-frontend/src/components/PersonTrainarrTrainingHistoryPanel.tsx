import { ApiErrorCallout } from '@stl/shared-ui'
import type { TrainarrPersonTrainingHistoryResponse } from '../api/types'

type Props = {
  personDisplayName: string
  history: TrainarrPersonTrainingHistoryResponse | null
  isLoading: boolean
  isError: boolean
  readErrorMessage?: string | null
  onRetryRead?: () => void
}

function formatTimestamp(iso: string) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

export function PersonTrainarrTrainingHistoryPanel({
  personDisplayName,
  history,
  isLoading,
  isError,
  readErrorMessage,
  onRetryRead,
}: Props) {
  return (
    <section
      className="mt-6 rounded-xl border border-slate-700 bg-slate-900/60 p-6"
      data-testid="person-trainarr-training-history-panel"
    >
      <h2 className="text-sm font-medium text-slate-300">TrainArr training history</h2>
      <p className="mt-2 text-xs text-[var(--color-text-muted)]">
        Materialized training timeline for {personDisplayName} from TrainArr (read-through).
      </p>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading TrainArr training history…</p>
      ) : null}

      {isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={readErrorMessage ?? 'Failed to load TrainArr training history.'}
          onRetry={onRetryRead}
          retryLabel="Retry history"
        />
      ) : null}

      {history ? (
        <>
          <p className="mt-3 text-xs text-[var(--color-text-muted)]">{history.sourceNote}</p>
          <p className="mt-2 text-sm text-slate-300">
            {history.totalCount} event{history.totalCount === 1 ? '' : 's'} recorded
          </p>
          {history.items.length === 0 ? (
            <p className="mt-3 text-sm text-[var(--color-text-muted)]" data-testid="person-trainarr-training-history-empty">
              No TrainArr training history entries yet.
            </p>
          ) : (
            <ul className="mt-4 space-y-2" data-testid="person-trainarr-training-history-list">
              {history.items.map((entry) => (
                <li
                  key={entry.entryId}
                  className="rounded-md border border-slate-800 bg-slate-950/50 px-3 py-2 text-sm"
                >
                  <p className="font-medium text-slate-100">{entry.summary}</p>
                  <p className="text-xs text-[var(--color-text-muted)]">
                    {entry.eventKind.replaceAll('_', ' ')} · {formatTimestamp(entry.occurredAt)}
                  </p>
                </li>
              ))}
            </ul>
          )}
        </>
      ) : null}
    </section>
  )
}
