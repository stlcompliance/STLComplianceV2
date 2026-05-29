import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import {
  getDowntimeSyncRuns,
  getDowntimeTrackingSettings,
  getPendingDowntimeSync,
  upsertDowntimeTrackingSettings,
} from '../api/client'

interface DowntimeTrackingSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function DowntimeTrackingSettingsPanel({
  accessToken,
  canManage,
}: DowntimeTrackingSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [autoTrackOutOfService, setAutoTrackOutOfService] = useState(true)
  const [autoTrackNotReady, setAutoTrackNotReady] = useState(true)
  const [availabilityPeriodDays, setAvailabilityPeriodDays] = useState(30)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-downtime-tracking-settings', accessToken],
    queryFn: () => getDowntimeTrackingSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['maintainarr-downtime-sync-pending', accessToken],
    queryFn: () => getPendingDowntimeSync(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['maintainarr-downtime-sync-runs', accessToken],
    queryFn: () => getDowntimeSyncRuns(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setAutoTrackOutOfService(data.autoTrackOutOfService)
    setAutoTrackNotReady(data.autoTrackNotReady)
    setAvailabilityPeriodDays(data.availabilityPeriodDays)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertDowntimeTrackingSettings(accessToken, {
        isEnabled,
        autoTrackOutOfService,
        autoTrackNotReady,
        availabilityPeriodDays,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-downtime-tracking-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-downtime-sync-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-downtime-sync-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-fleet-availability'] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-6" data-testid="maintainarr-downtime-settings-panel">
      <div className="mb-4">
        <h2 className="text-lg font-semibold text-slate-100">Downtime tracking automation</h2>
        <p className="text-sm text-slate-400">
          Open and close automatic downtime events when assets are out of service or not ready.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <label className="flex items-center gap-2 text-sm text-slate-300">
          <input type="checkbox" checked={isEnabled} onChange={(event) => setIsEnabled(event.target.checked)} />
          Enable downtime sync worker
        </label>
        <label className="flex items-center gap-2 text-sm text-slate-300">
          <input
            type="checkbox"
            checked={autoTrackOutOfService}
            onChange={(event) => setAutoTrackOutOfService(event.target.checked)}
          />
          Auto-track out-of-service lifecycle
        </label>
        <label className="flex items-center gap-2 text-sm text-slate-300">
          <input
            type="checkbox"
            checked={autoTrackNotReady}
            onChange={(event) => setAutoTrackNotReady(event.target.checked)}
          />
          Auto-track not-ready readiness
        </label>
        <label className="grid gap-1 text-sm text-slate-300">
          Availability period (days)
          <input
            type="number"
            min={1}
            max={365}
            className="rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
            value={availabilityPeriodDays}
            onChange={(event) => setAvailabilityPeriodDays(Number(event.target.value))}
          />
        </label>
      </div>

      <button
        type="button"
        className="mt-4 rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
        onClick={() => saveMutation.mutate()}
        disabled={saveMutation.isPending}
      >
        Save downtime settings
      </button>

      {isEnabled && pendingQuery.data ? (
        <p className="mt-4 text-sm text-slate-400">
          Pending sync candidates: {pendingQuery.data.items.length}
        </p>
      ) : null}

      {runsQuery.data && runsQuery.data.items.length > 0 ? (
        <div className="mt-4 text-sm text-slate-400">
          Latest run: scanned {runsQuery.data.items[0].assetsScanned}, opened{' '}
          {runsQuery.data.items[0].eventsOpened}, closed {runsQuery.data.items[0].eventsClosed}
        </div>
      ) : null}
    </section>
  )
}
