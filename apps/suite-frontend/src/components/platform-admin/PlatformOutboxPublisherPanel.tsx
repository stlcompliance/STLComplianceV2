import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import { OutboxRecentEvents } from './outbox/OutboxRecentEvents'
import { OutboxRecentRuns } from './outbox/OutboxRecentRuns'
import { OutboxStatusSummary } from './outbox/OutboxStatusSummary'

export function PlatformOutboxPublisherPanel() {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(true)
  const [maxRetryAttempts, setMaxRetryAttempts] = useState('5')
  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState('5')
  const [actionNotice, setActionNotice] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

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
      setActionError(null)
      setActionNotice('Outbox publisher settings saved.')
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-settings'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-status'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-lifecycle-overview'] })
    },
    onError: (error: Error) => {
      setActionNotice(null)
      setActionError(error.message)
    },
  })

  const triggerMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformOutboxPublisher(),
    onSuccess: () => {
      setActionError(null)
      setActionNotice('Outbox publish batch triggered.')
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-status'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-events'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-lifecycle-overview'] })
    },
    onError: (error: Error) => {
      setActionNotice(null)
      setActionError(error.message)
    },
  })

  const status = statusQuery.data

  return (
    <section
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4"
      data-testid="platform-outbox-publisher-panel"
    >
      <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Platform event outbox</h2>
      <p className="mt-1 text-sm text-[var(--color-text-muted)]">
        Tenant and product destination status changes enqueue integration events. The dedicated{' '}
        <code className="text-xs">nexarr-worker</code> drains the outbox via NexArr internal publish
        APIs.
      </p>

      <OutboxStatusSummary status={status} />

      {settingsQuery.isError && (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(settingsQuery.error, 'Failed to load platform outbox settings.')}
          onRetry={() => void settingsQuery.refetch()}
          retryLabel="Retry settings"
        />
      )}
      {actionError ? (
        <ApiErrorCallout
          className="mt-3"
          message={actionError}
          onRetry={() => {
            setActionError(null)
            setActionNotice(null)
          }}
          retryLabel="Dismiss"
        />
      ) : null}
      {actionNotice ? (
        <p
          className="mt-3 rounded-md border border-[var(--color-success-border)] bg-[var(--color-success-bg)] px-3 py-2 text-sm text-[var(--color-success-text)]"
          data-testid="platform-outbox-action-notice"
        >
          {actionNotice}
        </p>
      ) : null}

      <div className="mt-4 space-y-3">
        <label htmlFor="platform-outbox-enabled" className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
          <input
            id="platform-outbox-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="platform-outbox-enabled"
          />
          Enable platform outbox publishing
        </label>

        <label htmlFor="platform-outbox-max-retries" className="block text-sm text-[var(--color-text-secondary)]">
          Max retry attempts
          <input
            id="platform-outbox-max-retries"
            className="mt-1 w-32 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
            type="number"
            min={1}
            max={20}
            value={maxRetryAttempts}
            onChange={(event) => setMaxRetryAttempts(event.target.value)}
            data-testid="platform-outbox-max-retries"
          />
        </label>

        <label htmlFor="platform-outbox-retry-minutes" className="block text-sm text-[var(--color-text-secondary)]">
          Retry interval (minutes)
          <input
            id="platform-outbox-retry-minutes"
            className="mt-1 w-32 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
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
            className="rounded-md bg-[var(--color-accent)] px-4 py-2 text-sm font-medium text-[var(--color-on-accent)] disabled:opacity-50"
            disabled={saveMutation.isPending}
            onClick={() => saveMutation.mutate()}
            data-testid="platform-outbox-save"
          >
            {saveMutation.isPending ? 'Saving…' : 'Save settings'}
          </button>
          <button
            type="button"
            className="rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-4 py-2 text-sm font-medium text-[var(--color-text-primary)] disabled:opacity-50"
            disabled={triggerMutation.isPending || !isEnabled}
            onClick={() => triggerMutation.mutate()}
            data-testid="platform-outbox-trigger"
          >
            {triggerMutation.isPending ? 'Publishing…' : 'Run publish batch now'}
          </button>
        </div>
      </div>

      <OutboxRecentEvents query={eventsQuery} />

      <OutboxRecentRuns query={runsQuery} />
    </section>
  )
}
