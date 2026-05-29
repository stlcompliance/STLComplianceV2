import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getIntegrationEventInbox,
  getIntegrationEventOutbox,
  getIntegrationEventSettings,
  upsertIntegrationEventSettings,
} from '../api/client'

interface IntegrationEventSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function IntegrationEventSettingsPanel({
  accessToken,
  canManage,
}: IntegrationEventSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(true)
  const [maxAttempts, setMaxAttempts] = useState(5)
  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState(15)

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-integration-event-settings', accessToken],
    queryFn: () => getIntegrationEventSettings(accessToken),
    enabled: canManage,
  })

  const outboxQuery = useQuery({
    queryKey: ['supplyarr-integration-event-outbox', accessToken],
    queryFn: () => getIntegrationEventOutbox(accessToken, 10),
    enabled: canManage,
  })

  const inboxQuery = useQuery({
    queryKey: ['supplyarr-integration-event-inbox', accessToken],
    queryFn: () => getIntegrationEventInbox(accessToken, 10),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setMaxAttempts(data.maxAttempts)
    setRetryIntervalMinutes(data.retryIntervalMinutes)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertIntegrationEventSettings(accessToken, {
        isEnabled,
        maxAttempts,
        retryIntervalMinutes,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-integration-event-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-integration-event-outbox', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-integration-event-inbox', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="integration-event-settings-panel"
    >
      <h3 className="text-sm font-semibold text-foreground">Integration event outbox / inbox</h3>
      <p className="mt-1 text-xs text-muted-foreground">
        Async cross-product integration queue processed by the shared worker.
      </p>

      <div className="mt-4 grid gap-3 sm:grid-cols-3">
        <label htmlFor="integration-event-enabled" className="flex items-center gap-2 text-sm">
          <input
            id="integration-event-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(e) => setIsEnabled(e.target.checked)}
          />
          Enable integration event worker
        </label>
        <label htmlFor="integration-event-max-attempts" className="text-sm">
          Max retry attempts
          <input
            id="integration-event-max-attempts"
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            type="number"
            min={1}
            max={20}
            value={maxAttempts}
            onChange={(e) => setMaxAttempts(Number(e.target.value))}
          />
        </label>
        <label htmlFor="integration-event-retry-interval" className="text-sm">
          Retry interval (minutes)
          <input
            id="integration-event-retry-interval"
            className="mt-1 w-full rounded border border-input bg-background px-2 py-1"
            type="number"
            min={1}
            value={retryIntervalMinutes}
            onChange={(e) => setRetryIntervalMinutes(Number(e.target.value))}
          />
        </label>
      </div>

      <button
        type="button"
        className="mt-4 rounded bg-primary px-3 py-1.5 text-sm text-primary-foreground disabled:opacity-50"
        disabled={saveMutation.isPending}
        onClick={() => saveMutation.mutate()}
      >
        {saveMutation.isPending ? 'Saving…' : 'Save settings'}
      </button>

      <div className="mt-6 grid gap-4 md:grid-cols-2">
        <div>
          <h4 className="text-xs font-medium uppercase text-muted-foreground">Recent outbox</h4>
          <ul className="mt-2 space-y-1 text-xs">
            {(outboxQuery.data?.items ?? []).map((item) => (
              <li key={item.eventId} className="rounded border border-border px-2 py-1">
                {item.eventKind} — {item.processingStatus}
              </li>
            ))}
            {(outboxQuery.data?.items.length ?? 0) === 0 && (
              <li className="text-muted-foreground">No outbox events yet.</li>
            )}
          </ul>
        </div>
        <div>
          <h4 className="text-xs font-medium uppercase text-muted-foreground">Recent inbox</h4>
          <ul className="mt-2 space-y-1 text-xs">
            {(inboxQuery.data?.items ?? []).map((item) => (
              <li key={item.eventId} className="rounded border border-border px-2 py-1">
                {item.sourceProduct}/{item.eventKind} — {item.processingStatus}
              </li>
            ))}
            {(inboxQuery.data?.items.length ?? 0) === 0 && (
              <li className="text-muted-foreground">No inbox events yet.</li>
            )}
          </ul>
        </div>
      </div>
    </section>
  )
}
