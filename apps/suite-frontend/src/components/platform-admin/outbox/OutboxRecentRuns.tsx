import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PlatformOutboxPublisherRunsResponse } from '../../../api/types'

type Props = {
  query: {
    isLoading: boolean
    isError: boolean
    error: unknown
    data: PlatformOutboxPublisherRunsResponse | undefined
    refetch: () => Promise<unknown>
  }
}

export function OutboxRecentRuns({ query }: Props) {
  return (
    <div className="mt-6">
      <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Recent publish runs</h3>
      {query.isLoading ? <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading runs…</p> : null}
      {query.isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(query.error, 'Failed to load publish runs.')}
          onRetry={() => void query.refetch()}
          retryLabel="Retry runs"
        />
      ) : null}
      {query.data?.items.length === 0 ? (
        <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="platform-outbox-runs-empty">
          No publish runs recorded yet.
        </p>
      ) : null}
      {query.data && query.data.items.length > 0 ? (
        <ul className="mt-2 space-y-2" data-testid="platform-outbox-runs-list">
          {query.data.items.map((run) => (
            <li key={run.runId} className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-secondary)]">
              <span className="font-medium text-[var(--color-text-primary)]">{run.outcome}</span>
              {' — '}
              published {run.publishedCount}
              {run.failedCount > 0 ? `, failed ${run.failedCount}` : ''}
              {run.deadLetterCount > 0 ? `, dead letter ${run.deadLetterCount}` : ''}
              {' · '}
              {new Date(run.processedAt).toLocaleString()}
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
