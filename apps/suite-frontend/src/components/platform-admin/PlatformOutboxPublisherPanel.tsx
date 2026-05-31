import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'

export function PlatformOutboxPublisherPanel() {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(true)
  const [maxRetryAttempts, setMaxRetryAttempts] = useState('5')
  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState('5')

  const settingsQuery = useQuery({
    queryKey: ['platform-outbox-publisher-settings'],
    queryFn: () => nexarr.getPlatformOutboxPublisherSettings(),
  })

  const statusQuery = useQuery({
    queryKey: ['platform-outbox-publisher-status'],
    queryFn: () => nexarr.getPlatformOutboxPublisherStatus(),
    refetchInterval: 30_000,
  })

  const runsQuery = useQuery({
    queryKey: ['platform-outbox-publisher-runs'],
    queryFn: () => nexarr.getPlatformOutboxPublisherRuns(8),
  })

  const eventsQuery = useQuery({
    queryKey: ['platform-outbox-events'],
    queryFn: () => nexarr.getPlatformOutboxEvents(15),
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setMaxRetryAttempts(String(data.maxRetryAttempts))
    setRetryIntervalMinutes(String(data.retryIntervalMinutes))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertPlatformOutboxPublisherSettings({
        isEnabled,
        maxRetryAttempts: Number.parseInt(maxRetryAttempts, 10) || 5,
        retryIntervalMinutes: Number.parseInt(retryIntervalMinutes, 10) || 5,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-settings'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-status'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-lifecycle-overview'] })
    },
  })

  const triggerMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformOutboxPublisher(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-status'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-events'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-lifecycle-overview'] })
    },
  })

  const status = statusQuery.data

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
      data-testid="platform-outbox-publisher-panel"
    >
      <h2 className="text-lg font-semibold text-white">Platform event outbox</h2>
      <p className="mt-1 text-sm text-slate-400">
        Tenant and entitlement changes enqueue integration events. The dedicated{' '}
        <code className="text-xs">nexarr-worker</code> drains the outbox via NexArr internal publish
        APIs.
      </p>

      {status && (
        <dl className="mt-4 grid gap-2 text-sm text-slate-300 sm:grid-cols-3">
          <div>
            <dt className="text-slate-500">Pending</dt>
            <dd className="font-medium tabular-nums text-white">{status.pendingCount}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Dead letter</dt>
            <dd className="font-medium tabular-nums text-white">{status.deadLetterCount}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Publisher</dt>
            <dd className="font-medium text-white">{status.isEnabled ? 'Enabled' : 'Disabled'}</dd>
          </div>
        </dl>
      )}

      {settingsQuery.isError && (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(settingsQuery.error, 'Failed to load platform outbox settings.')}
          onRetry={() => void settingsQuery.refetch()}
          retryLabel="Retry settings"
        />
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="platform-outbox-enabled" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="platform-outbox-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="platform-outbox-enabled"
          />
          Enable platform outbox publishing
        </label>

        <label htmlFor="platform-outbox-max-retries" className="block text-sm text-slate-200">
          Max retry attempts
          <input
            id="platform-outbox-max-retries"
            className="mt-1 w-32 rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            type="number"
            min={1}
            max={20}
            value={maxRetryAttempts}
            onChange={(event) => setMaxRetryAttempts(event.target.value)}
            data-testid="platform-outbox-max-retries"
          />
        </label>

        <label htmlFor="platform-outbox-retry-minutes" className="block text-sm text-slate-200">
          Retry interval (minutes)
          <input
            id="platform-outbox-retry-minutes"
            className="mt-1 w-32 rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            type="number"
            min={1}
            max={1440}
            value={retryIntervalMinutes}
            onChange={(event) => setRetryIntervalMinutes(event.target.value)}
            data-testid="platform-outbox-retry-minutes"
          />
        </label>

        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            className="rounded-md bg-stl-teal px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
            disabled={saveMutation.isPending}
            onClick={() => saveMutation.mutate()}
            data-testid="platform-outbox-save"
          >
            {saveMutation.isPending ? 'Saving…' : 'Save settings'}
          </button>
          <button
            type="button"
            className="rounded-md border border-slate-600 px-4 py-2 text-sm font-medium text-slate-200 disabled:opacity-50"
            disabled={triggerMutation.isPending || !isEnabled}
            onClick={() => triggerMutation.mutate()}
            data-testid="platform-outbox-trigger"
          >
            {triggerMutation.isPending ? 'Publishing…' : 'Run publish batch now'}
          </button>
        </div>
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-white">Recent outbox events</h3>
        {eventsQuery.isLoading && <p className="mt-2 text-sm text-slate-400">Loading events…</p>}
        {eventsQuery.data?.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-400" data-testid="platform-outbox-events-empty">
            No outbox events recorded yet.
          </p>
        )}
        {eventsQuery.data && eventsQuery.data.items.length > 0 && (
          <ul className="mt-2 space-y-2" data-testid="platform-outbox-events-list">
            {eventsQuery.data.items.map((event) => (
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
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-white">Recent publish runs</h3>
        {runsQuery.isLoading && <p className="mt-2 text-sm text-slate-400">Loading runs…</p>}
        {runsQuery.data?.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-400" data-testid="platform-outbox-runs-empty">
            No publish runs recorded yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 space-y-2" data-testid="platform-outbox-runs-list">
            {runsQuery.data.items.map((run) => (
              <li
                key={run.runId}
                className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-300"
              >
                <span className="font-medium text-white">{run.outcome}</span>
                {' — '}
                published {run.publishedCount}
                {run.failedCount > 0 ? `, failed ${run.failedCount}` : ''}
                {run.deadLetterCount > 0 ? `, dead letter ${run.deadLetterCount}` : ''}
                {' · '}
                {new Date(run.processedAt).toLocaleString()}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
