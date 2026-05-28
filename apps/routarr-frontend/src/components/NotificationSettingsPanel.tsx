import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getDispatchNotificationDispatches,
  getDispatchNotificationSettings,
  upsertDispatchNotificationSettings,
} from '../api/client'

interface NotificationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function NotificationSettingsPanel({ accessToken, canManage }: NotificationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [webhookUrl, setWebhookUrl] = useState('')
  const [notifyTripAssigned, setNotifyTripAssigned] = useState(true)
  const [notifyTripDispatched, setNotifyTripDispatched] = useState(true)
  const [notifyTripInProgress, setNotifyTripInProgress] = useState(true)
  const [notifyTripCompleted, setNotifyTripCompleted] = useState(true)
  const [notifyTripCancelled, setNotifyTripCancelled] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['routarr-notification-settings', accessToken],
    queryFn: () => getDispatchNotificationSettings(accessToken),
    enabled: canManage,
  })

  const dispatchesQuery = useQuery({
    queryKey: ['routarr-notification-dispatches', accessToken],
    queryFn: () => getDispatchNotificationDispatches(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setWebhookUrl(data.notificationWebhookUrl ?? '')
    setNotifyTripAssigned(data.notifyOnTripAssigned)
    setNotifyTripDispatched(data.notifyOnTripDispatched)
    setNotifyTripInProgress(data.notifyOnTripInProgress)
    setNotifyTripCompleted(data.notifyOnTripCompleted)
    setNotifyTripCancelled(data.notifyOnTripCancelled)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertDispatchNotificationSettings(accessToken, {
        isEnabled,
        notificationWebhookUrl: webhookUrl.trim() || null,
        notifyOnTripAssigned: notifyTripAssigned,
        notifyOnTripDispatched: notifyTripDispatched,
        notifyOnTripInProgress: notifyTripInProgress,
        notifyOnTripCompleted: notifyTripCompleted,
        notifyOnTripCancelled: notifyTripCancelled,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['routarr-notification-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-notification-dispatches', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm">
      <h2 className="text-lg font-semibold text-slate-50">Dispatch notifications</h2>
      <p className="mt-1 text-sm text-slate-400">
        Configure HTTPS webhooks for trip assignment and dispatch lifecycle events. Delivery runs on a
        scheduled worker.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load notification settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable dispatch notifications
        </label>

        <label className="block text-sm text-slate-200">
          <span className="font-medium">Webhook URL</span>
          <input
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="url"
            placeholder="https://hooks.example.com/routarr"
            value={webhookUrl}
            onChange={(event) => setWebhookUrl(event.target.value)}
          />
        </label>

        <fieldset className="space-y-2 text-sm text-slate-200">
          <legend className="font-medium">Notify on</legend>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyTripAssigned}
              onChange={(event) => setNotifyTripAssigned(event.target.checked)}
            />
            Trip assigned
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyTripDispatched}
              onChange={(event) => setNotifyTripDispatched(event.target.checked)}
            />
            Trip dispatched
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyTripInProgress}
              onChange={(event) => setNotifyTripInProgress(event.target.checked)}
            />
            Trip in progress
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyTripCompleted}
              onChange={(event) => setNotifyTripCompleted(event.target.checked)}
            />
            Trip completed
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyTripCancelled}
              onChange={(event) => setNotifyTripCancelled(event.target.checked)}
            />
            Trip cancelled
          </label>
        </fieldset>

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
        >
          {saveMutation.isPending ? 'Saving…' : 'Save notification settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-rose-400">Failed to save notification settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent dispatches</h3>
        {dispatchesQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-500">Loading dispatch history…</p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500">No notification dispatches yet.</p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {dispatchesQuery.data.items.map((item) => (
              <li key={item.notificationId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{item.eventKind}</span>
                  <span className="text-slate-500">{item.dispatchStatus}</span>
                </div>
                <div className="text-xs text-slate-500">
                  Trip {item.tripId}
                  {item.webhookHost ? ` · ${item.webhookHost}` : ''}
                  {item.httpStatusCode != null ? ` · HTTP ${item.httpStatusCode}` : ''}
                  {item.errorMessage ? ` · ${item.errorMessage}` : ''}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
