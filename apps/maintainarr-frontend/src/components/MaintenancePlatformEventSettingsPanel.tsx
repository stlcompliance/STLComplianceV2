import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import {
  getMaintenancePlatformEventProcessingRuns,
  getMaintenancePlatformEventSettings,
  getMaintenancePlatformOutboxEvents,
  upsertMaintenancePlatformEventSettings,
} from '../api/client'

interface MaintenancePlatformEventSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function MaintenancePlatformEventSettingsPanel({
  accessToken,
  canManage,
}: MaintenancePlatformEventSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(true)
  const [maxAttempts, setMaxAttempts] = useState(5)
  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState(15)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-platform-event-settings', accessToken],
    queryFn: () => getMaintenancePlatformEventSettings(accessToken),
    enabled: canManage,
  })

  const outboxQuery = useQuery({
    queryKey: ['maintainarr-platform-outbox-events', accessToken],
    queryFn: () => getMaintenancePlatformOutboxEvents(accessToken, 8),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['maintainarr-platform-event-runs', accessToken],
    queryFn: () => getMaintenancePlatformEventProcessingRuns(accessToken, 5),
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
      upsertMaintenancePlatformEventSettings(accessToken, {
        isEnabled,
        maxAttempts,
        retryIntervalMinutes,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-platform-event-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-platform-outbox-events', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-platform-event-runs', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-800 bg-slate-900/60 p-6"
      data-testid="maintainarr-platform-event-settings-panel"
    >
      <div className="mb-4">
        <h2 className="text-lg font-semibold text-slate-100">Platform event emission</h2>
        <p className="text-sm text-slate-400">
          Emit readiness updates when status rollups refresh.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <label className="flex items-center gap-2 text-sm text-slate-300">
          <input type="checkbox" checked={isEnabled} onChange={(event) => setIsEnabled(event.target.checked)} />
          Enable platform event outbox
        </label>
        <label className="grid gap-1 text-sm text-slate-300">
          Max processing attempts
          <input
            type="number"
            min={1}
            max={20}
            className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
            value={maxAttempts}
            onChange={(event) => setMaxAttempts(Number(event.target.value))}
          />
        </label>
        <label className="grid gap-1 text-sm text-slate-300">
          Retry interval (minutes)
          <input
            type="number"
            min={1}
            max={1440}
            className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
            value={retryIntervalMinutes}
            onChange={(event) => setRetryIntervalMinutes(Number(event.target.value))}
          />
        </label>
      </div>

      <button
        type="button"
        className="mt-4 rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        onClick={() => saveMutation.mutate()}
        disabled={saveMutation.isPending}
      >
        Save platform event settings
      </button>

      {isEnabled && outboxQuery.data && outboxQuery.data.items.length > 0 ? (
        <div className="mt-4 overflow-x-auto">
          <p className="mb-2 text-sm font-medium text-slate-300">Recent outbox events</p>
          <table className="min-w-full text-left text-xs text-slate-400">
            <thead>
              <tr>
                <th className="pr-4 pb-2">Event</th>
                <th className="pr-4 pb-2">Status</th>
                <th className="pb-2">Created</th>
              </tr>
            </thead>
            <tbody>
              {outboxQuery.data.items.map((item) => (
                <tr key={item.id}>
                  <td className="pr-4 py-1 font-mono">{item.eventKind}</td>
                  <td className="pr-4 py-1">{item.processingStatus}</td>
                  <td className="py-1">{new Date(item.createdAt).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}

      {runsQuery.data && runsQuery.data.items.length > 0 ? (
        <p className="mt-4 text-sm text-slate-400">
          Latest processing run: processed {runsQuery.data.items[0].processedCount}, pending{' '}
          {runsQuery.data.items[0].pendingFound}, skipped {runsQuery.data.items[0].skippedCount}
        </p>
      ) : null}
    </section>
  )
}
