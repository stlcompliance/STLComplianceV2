import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getDispatchNotificationDispatches,
  getDispatchNotificationSettings,
  RoutArrApiError,
  upsertDispatchNotificationSettings,
} from '../api/client'

function resolveDispatchNotificationWebhookError(
  isEnabled: boolean,
  webhookUrl: string,
): string | null {
  const trimmed = webhookUrl.trim()
  if (isEnabled && !trimmed) {
    return 'Webhook URL is required when dispatch notifications are enabled.'
  }

  if (!trimmed) {
    return null
  }

  try {
    const url = new URL(trimmed)
    if (url.protocol !== 'http:' && url.protocol !== 'https:') {
      return 'Webhook URL must be an absolute URL.'
    }
  } catch {
    return 'Webhook URL must be an absolute URL.'
  }

  return null
}

function resolveSaveErrorMessage(error: unknown): string {
  if (error instanceof RoutArrApiError) {
    try {
      const payload = JSON.parse(error.body) as { message?: string }
      if (payload.message) {
        return payload.message
      }
    } catch {
      if (error.message && !error.message.startsWith('{')) {
        return error.message
      }
    }
  }

  return 'Failed to save notification settings.'
}

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
  const [clearWebhookOnDisable, setClearWebhookOnDisable] = useState(false)
  const [webhookError, setWebhookError] = useState<string | null>(null)
  const [saveError, setSaveError] = useState<string | null>(null)

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
        ...(!isEnabled && clearWebhookOnDisable
          ? { clearNotificationWebhookOnDisable: true }
          : {}),
      }),
    onSuccess: () => {
      setWebhookError(null)
      setSaveError(null)
      if (!isEnabled && clearWebhookOnDisable) {
        setWebhookUrl('')
        setClearWebhookOnDisable(false)
      }
      void queryClient.invalidateQueries({ queryKey: ['routarr-notification-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-notification-dispatches', accessToken] })
    },
    onError: (error) => {
      setSaveError(resolveSaveErrorMessage(error))
    },
  })

  const handleSave = () => {
    const validationError = resolveDispatchNotificationWebhookError(isEnabled, webhookUrl)
    if (validationError) {
      setWebhookError(validationError)
      setSaveError(null)
      return
    }

    setWebhookError(null)
    setSaveError(null)
    saveMutation.mutate()
  }

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm"
      data-testid="notification-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Dispatch notifications</h2>
      <p className="mt-1 text-sm text-slate-400">
        Configure HTTPS webhooks for trip assignment and dispatch lifecycle events. Delivery runs on a
        scheduled worker.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load notification settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200" htmlFor="notification-settings-enabled">
          <input
            id="notification-settings-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => {
              const nextEnabled = event.target.checked
              setIsEnabled(nextEnabled)
              setWebhookError(null)
              setSaveError(null)
              setClearWebhookOnDisable(false)
              if (nextEnabled && settingsQuery.data?.notificationWebhookUrl) {
                setWebhookUrl(settingsQuery.data.notificationWebhookUrl)
              }
            }}
            data-testid="notification-settings-enabled"
          />
          Enable dispatch notifications
        </label>

        <label className="block text-sm text-slate-200" htmlFor="notification-settings-webhook">
          <span className="font-medium">Webhook URL</span>
          <input
            id="notification-settings-webhook"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="url"
            placeholder="https://hooks.example.com/routarr"
            value={webhookUrl}
            onChange={(event) => {
              setWebhookUrl(event.target.value)
              setWebhookError(null)
              setSaveError(null)
            }}
            data-testid="notification-settings-webhook"
            aria-invalid={webhookError ? true : undefined}
            aria-describedby={webhookError ? 'notification-settings-webhook-error' : undefined}
          />
          {webhookError && (
            <p
              id="notification-settings-webhook-error"
              className="mt-1 text-sm text-rose-400"
              data-testid="notification-settings-webhook-error"
            >
              {webhookError}
            </p>
          )}
        </label>

        {!isEnabled && (
          <label className="flex items-center gap-2 text-sm text-slate-200" htmlFor="notification-settings-clear-webhook-on-disable">
            <input
              id="notification-settings-clear-webhook-on-disable"
              type="checkbox"
              checked={clearWebhookOnDisable}
              onChange={(event) => {
                const nextClear = event.target.checked
                setClearWebhookOnDisable(nextClear)
                setSaveError(null)
                if (nextClear) {
                  setWebhookUrl('')
                } else if (settingsQuery.data?.notificationWebhookUrl) {
                  setWebhookUrl(settingsQuery.data.notificationWebhookUrl)
                }
              }}
              data-testid="notification-settings-clear-webhook-on-disable"
            />
            Clear saved webhook URL when saving disabled settings
          </label>
        )}

        <fieldset className="space-y-2 text-sm text-slate-200">
          <legend className="font-medium">Notify on</legend>
          <label className="flex items-center gap-2" htmlFor="notification-trip-assigned">
            <input
              id="notification-trip-assigned"
              type="checkbox"
              checked={notifyTripAssigned}
              onChange={(event) => setNotifyTripAssigned(event.target.checked)}
              data-testid="notification-trip-assigned"
            />
            Trip assigned
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-trip-dispatched">
            <input
              id="notification-trip-dispatched"
              type="checkbox"
              checked={notifyTripDispatched}
              onChange={(event) => setNotifyTripDispatched(event.target.checked)}
              data-testid="notification-trip-dispatched"
            />
            Trip dispatched
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-trip-in-progress">
            <input
              id="notification-trip-in-progress"
              type="checkbox"
              checked={notifyTripInProgress}
              onChange={(event) => setNotifyTripInProgress(event.target.checked)}
              data-testid="notification-trip-in-progress"
            />
            Trip in progress
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-trip-completed">
            <input
              id="notification-trip-completed"
              type="checkbox"
              checked={notifyTripCompleted}
              onChange={(event) => setNotifyTripCompleted(event.target.checked)}
              data-testid="notification-trip-completed"
            />
            Trip completed
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-trip-cancelled">
            <input
              id="notification-trip-cancelled"
              type="checkbox"
              checked={notifyTripCancelled}
              onChange={(event) => setNotifyTripCancelled(event.target.checked)}
              data-testid="notification-trip-cancelled"
            />
            Trip cancelled
          </label>
        </fieldset>

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={handleSave}
          data-testid="notification-settings-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save notification settings'}
        </button>

        {saveError && (
          <p className="text-sm text-rose-400" data-testid="notification-settings-save-error">
            {saveError}
          </p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent dispatches</h3>
        {dispatchesQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-500">Loading dispatch history…</p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500" data-testid="notification-dispatches-empty">
            No notification dispatches yet.
          </p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="notification-dispatches-list"
          >
            {dispatchesQuery.data.items.map((item) => (
              <li
                key={item.notificationId}
                className="px-3 py-2"
                data-testid={`notification-dispatch-row-${item.tripId}`}
              >
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
