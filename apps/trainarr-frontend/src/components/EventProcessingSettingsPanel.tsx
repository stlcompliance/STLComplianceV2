import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getEventProcessingSettings,
  getTrainingDomainEvents,
  upsertEventProcessingSettings,
} from '../api/client'

interface EventProcessingSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function EventProcessingSettingsPanel({ accessToken, canManage }: EventProcessingSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(true)
  const [maxAttempts, setMaxAttempts] = useState('10')
  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState('5')

  const settingsQuery = useQuery({
    queryKey: ['trainarr-event-processing-settings', accessToken],
    queryFn: () => getEventProcessingSettings(accessToken),
    enabled: canManage,
  })

  const eventsQuery = useQuery({
    queryKey: ['trainarr-training-domain-events', accessToken],
    queryFn: () => getTrainingDomainEvents(accessToken, 8),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setMaxAttempts(String(data.maxAttempts))
    setRetryIntervalMinutes(String(data.retryIntervalMinutes))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertEventProcessingSettings(accessToken, {
        isEnabled,
        maxAttempts: Number.parseInt(maxAttempts, 10) || 10,
        retryIntervalMinutes: Number.parseInt(retryIntervalMinutes, 10) || 5,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-event-processing-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-training-domain-events', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="event-processing-settings-panel"
    >
      <h2 className="text-lg font-semibold text-foreground">Training event processing</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        TrainArr records training lifecycle events in an outbox and processes them asynchronously into per-person
        training history. The shared worker retries pending events on a schedule.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load event processing settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="event-processing-enabled" className="flex items-center gap-2 text-sm">
          <input
            id="event-processing-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="event-processing-enabled"
          />
          Enable training event processing
        </label>
        <label htmlFor="event-processing-max-attempts" className="block text-sm">
          Max processing attempts
          <input
            id="event-processing-max-attempts"
            className="mt-1 w-full rounded border border-border px-2 py-1"
            value={maxAttempts}
            onChange={(event) => setMaxAttempts(event.target.value)}
            data-testid="event-processing-max-attempts"
          />
        </label>
        <label htmlFor="event-processing-retry-interval" className="block text-sm">
          Retry interval (minutes)
          <input
            id="event-processing-retry-interval"
            className="mt-1 w-full rounded border border-border px-2 py-1"
            value={retryIntervalMinutes}
            onChange={(event) => setRetryIntervalMinutes(event.target.value)}
            data-testid="event-processing-retry-interval"
          />
        </label>
        <button
          type="button"
          className="rounded bg-primary px-3 py-1.5 text-sm text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="event-processing-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save settings'}
        </button>
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-medium text-foreground">Recent domain events</h3>
        {eventsQuery.isLoading && <p className="mt-2 text-sm text-muted-foreground">Loading events…</p>}
        {eventsQuery.data?.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="event-processing-events-empty">
            No training domain events recorded yet.
          </p>
        )}
        {eventsQuery.data && eventsQuery.data.items.length > 0 && (
          <ul className="mt-2 space-y-2 text-sm" data-testid="event-processing-events-list">
            {eventsQuery.data.items.map((item) => (
              <li key={item.eventId} className="rounded border border-border px-2 py-1">
                <span className="font-medium">{item.eventKind}</span> · {item.processingStatus} ·{' '}
                {item.staffarrPersonId.slice(0, 8)}…
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
