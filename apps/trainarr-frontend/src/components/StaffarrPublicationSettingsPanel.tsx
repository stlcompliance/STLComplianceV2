import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getStaffarrPublicationDeliveries,
  getStaffarrPublicationSettings,
  upsertStaffarrPublicationSettings,
} from '../api/client'

interface StaffarrPublicationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function StaffarrPublicationSettingsPanel({ accessToken, canManage }: StaffarrPublicationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(true)
  const [maxAttempts, setMaxAttempts] = useState('10')
  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState('5')

  const settingsQuery = useQuery({
    queryKey: ['trainarr-staffarr-publication-settings', accessToken],
    queryFn: () => getStaffarrPublicationSettings(accessToken),
    enabled: canManage,
  })

  const deliveriesQuery = useQuery({
    queryKey: ['trainarr-staffarr-publication-deliveries', accessToken],
    queryFn: () => getStaffarrPublicationDeliveries(accessToken, 8),
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
      upsertStaffarrPublicationSettings(accessToken, {
        isEnabled,
        maxAttempts: Number.parseInt(maxAttempts, 10) || 10,
        retryIntervalMinutes: Number.parseInt(retryIntervalMinutes, 10) || 5,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-staffarr-publication-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-staffarr-publication-deliveries', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="staffarr-publication-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">StaffArr publication retry</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        TrainArr records certification and training blocker publications locally, then delivers them to StaffArr with
        automatic retries when ingestion is temporarily unavailable.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load StaffArr publication settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="staffarr-publication-retry-enabled"
          />
          Enable scheduled StaffArr publication retries
        </label>

        <label className="block text-sm">
          <span className="font-medium">Max delivery attempts</span>
          <input
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={1}
            max={50}
            value={maxAttempts}
            onChange={(event) => setMaxAttempts(event.target.value)}
            data-testid="staffarr-publication-max-attempts"
          />
        </label>

        <label className="block text-sm">
          <span className="font-medium">Retry interval (minutes)</span>
          <input
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={1}
            max={1440}
            value={retryIntervalMinutes}
            onChange={(event) => setRetryIntervalMinutes(event.target.value)}
            data-testid="staffarr-publication-retry-interval"
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="staffarr-publication-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save settings'}
        </button>
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent deliveries</h3>
        {deliveriesQuery.isLoading && <p className="mt-2 text-sm text-muted-foreground">Loading deliveries…</p>}
        {deliveriesQuery.isError && (
          <p className="mt-2 text-sm text-destructive">Failed to load recent StaffArr publication deliveries.</p>
        )}
        {deliveriesQuery.data && deliveriesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="staffarr-publication-deliveries-empty">
            No StaffArr publication deliveries recorded yet.
          </p>
        )}
        {deliveriesQuery.data && deliveriesQuery.data.items.length > 0 && (
          <ul className="mt-2 space-y-2 text-sm" data-testid="staffarr-publication-deliveries-list">
            {deliveriesQuery.data.items.map((item) => (
              <li key={item.deliveryId} className="rounded border border-border px-3 py-2">
                <div className="font-medium">
                  {item.operationKind} · {item.deliveryStatus}
                </div>
                <div className="text-muted-foreground">
                  Attempts: {item.attemptCount}
                  {item.httpStatusCode != null ? ` · HTTP ${item.httpStatusCode}` : ''}
                </div>
                {item.errorMessage && <div className="text-destructive">{item.errorMessage}</div>}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
