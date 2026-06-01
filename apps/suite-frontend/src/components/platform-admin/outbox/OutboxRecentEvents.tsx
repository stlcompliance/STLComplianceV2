import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PlatformOutboxEventsListResponse } from '../../../api/types'

type Props = {
  query: {
    isLoading: boolean
    isError: boolean
    error: unknown
    data: PlatformOutboxEventsListResponse | undefined
    refetch: () => Promise<unknown>
  }
}

export function OutboxRecentEvents({ query }: Props) {
  return (
    <div className="mt-6">
      <h3 className="text-sm font-semibold text-white">Recent outbox events</h3>
      {query.isLoading ? <p className="mt-2 text-sm text-slate-400">Loading events…</p> : null}
      {query.isError ? (
        <ApiErrorCallout
          className="mt-2"
          message={getErrorMessage(query.error, 'Failed to load outbox events.')}
          onRetry={() => void query.refetch()}
          retryLabel="Retry events"
        />
      ) : null}
      {query.data?.items.length === 0 ? (
        <p className="mt-2 text-sm text-slate-400" data-testid="platform-outbox-events-empty">
          No outbox events recorded yet.
        </p>
      ) : null}
      {query.data && query.data.items.length > 0 ? (
        <ul className="mt-2 space-y-2" data-testid="platform-outbox-events-list">
          {query.data.items.map((event) => (
            <li
              key={event.eventId}
              className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-300"
            >
              <span className="font-medium text-white">{event.eventType}</span>
              {' — '}
              {event.processingStatus}
              {event.tenantId ? ` · tenant ${event.tenantId.slice(0, 8)}…` : ''}
              {' · '}
              {new Date(event.occurredAt).toLocaleString()}
            </li>
          ))}
        </ul>
      ) : null}
    </div>
  )
}
