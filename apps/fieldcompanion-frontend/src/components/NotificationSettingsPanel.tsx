import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getPushPermissionState,
  pushReadinessLabel,
  requestPushPermission,
  syncFieldCompanionPushSubscription,
} from '../lib/pushNotifications'

import {
  getFieldCompanionNotificationDispatches,
  getFieldCompanionNotificationSettings,
  upsertFieldCompanionNotificationSettings,
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
  const [notifyHandoffRedeemed, setNotifyHandoffRedeemed] = useState(true)
  const [notifyFieldInboxRefreshed, setNotifyFieldInboxRefreshed] = useState(true)
  const [pushPermission, setPushPermission] = useState(getPushPermissionState)
  const [pushSyncStatus, setPushSyncStatus] = useState<string | null>(null)

  const settingsQuery = useQuery({
    queryKey: ['fieldcompanion-notification-settings', accessToken],
    queryFn: () => getFieldCompanionNotificationSettings(accessToken),
    enabled: canManage,
  })

  const dispatchesQuery = useQuery({
    queryKey: ['fieldcompanion-notification-dispatches', accessToken],
    queryFn: () => getFieldCompanionNotificationDispatches(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setWebhookUrl(data.notificationWebhookUrl ?? '')
    setNotifyHandoffRedeemed(data.notifyOnHandoffRedeemed)
    setNotifyFieldInboxRefreshed(data.notifyOnFieldInboxRefreshed)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertFieldCompanionNotificationSettings(accessToken, {
        isEnabled,
        notificationWebhookUrl: webhookUrl.trim() || null,
        notifyOnHandoffRedeemed: notifyHandoffRedeemed,
        notifyOnFieldInboxRefreshed: notifyFieldInboxRefreshed,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-notification-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['fieldcompanion-notification-dispatches', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="fieldcompanion-notification-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Operational notifications</h2>
      <p className="mt-1 text-sm text-slate-400">
        Configure HTTPS webhooks and browser Web Push for Field Companion handoff and field inbox lifecycle events.
        Dispatch runs on the shared worker notification job.
      </p>

      {settingsQuery.isError && (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(settingsQuery.error, 'Failed to load notification settings.')}
          onRetry={() => void settingsQuery.refetch()}
          retryLabel="Retry settings"
        />
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200" htmlFor="notification-settings-enabled">
          <input
            id="notification-settings-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="notification-settings-enabled"
          />
          Enable operational notifications
        </label>

        <label className="block text-sm text-slate-200" htmlFor="notification-settings-webhook">
          <span className="font-medium">Webhook URL</span>
          <input
            id="notification-settings-webhook"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="url"
            placeholder="https://hooks.example.com/fieldcompanion"
            value={webhookUrl}
            onChange={(event) => setWebhookUrl(event.target.value)}
            data-testid="notification-settings-webhook"
          />
        </label>

        <fieldset className="space-y-2 text-sm text-slate-200">
          <legend className="font-medium text-slate-100">Notify on</legend>
          <label className="flex items-center gap-2" htmlFor="notification-handoff-redeemed">
            <input
              id="notification-handoff-redeemed"
              type="checkbox"
              checked={notifyHandoffRedeemed}
              onChange={(event) => setNotifyHandoffRedeemed(event.target.checked)}
              data-testid="notification-handoff-redeemed"
            />
            Handoff redeemed (mobile session start)
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-field-inbox-refreshed">
            <input
              id="notification-field-inbox-refreshed"
              type="checkbox"
              checked={notifyFieldInboxRefreshed}
              onChange={(event) => setNotifyFieldInboxRefreshed(event.target.checked)}
              data-testid="notification-field-inbox-refreshed"
            />
            Field inbox refreshed
          </label>
        </fieldset>

        <div className="rounded-lg border border-slate-800 bg-slate-950/60 px-4 py-3 text-sm text-slate-300">
          <p className="font-medium text-slate-100">Push notification readiness</p>
          <p className="mt-1 text-slate-400" data-testid="fieldcompanion-push-readiness-label">
            {pushReadinessLabel(pushPermission)}
          </p>
          <button
            type="button"
            className="mt-3 rounded-md border border-slate-600 px-3 py-1.5 text-sm text-slate-100 hover:border-teal-500 disabled:opacity-50"
            disabled={pushPermission === 'unsupported' || pushPermission === 'granted'}
            data-testid="fieldcompanion-request-push-permission"
            onClick={() => {
              void requestPushPermission().then(async (permission) => {
                setPushPermission(permission)
                if (permission !== 'granted') {
                  return
                }
                const result = await syncFieldCompanionPushSubscription(accessToken)
                setPushSyncStatus(
                  result === 'subscribed'
                    ? 'Push subscription registered with NexArr.'
                    : result === 'skipped'
                      ? 'Push subscription skipped (not supported or VAPID unavailable).'
                      : 'Failed to register push subscription.',
                )
              })
            }}
          >
            Request browser push permission
          </button>
          {pushSyncStatus && (
            <p className="mt-2 text-xs text-[var(--color-text-muted)]" data-testid="fieldcompanion-push-sync-status">
              {pushSyncStatus}
            </p>
          )}
        </div>

        <button
          type="button"
          className="rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          data-testid="fieldcompanion-save-notification-settings"
          onClick={() => saveMutation.mutate()}
        >
          {saveMutation.isPending ? 'Saving…' : 'Save notification settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            message={getErrorMessage(saveMutation.error, 'Failed to save notification settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent dispatches</h3>
        {dispatchesQuery.isLoading && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading dispatch history…</p>
        )}
        {dispatchesQuery.isError && (
          <ApiErrorCallout
            className="mt-2"
            message={getErrorMessage(dispatchesQuery.error, 'Failed to load dispatch history.')}
            onRetry={() => void dispatchesQuery.refetch()}
            retryLabel="Retry dispatch history"
          />
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">No notification dispatches yet.</p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {dispatchesQuery.data.items.map((item) => (
              <li key={item.notificationId} className="px-3 py-2 text-slate-300">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{item.eventKind}</span>
                  <span className="text-[var(--color-text-muted)]">{item.dispatchStatus}</span>
                </div>
                <div className="text-xs text-[var(--color-text-muted)]">
                  {item.webhookHost ?? '—'}
                  {item.httpStatusCode != null ? ` · HTTP ${item.httpStatusCode}` : ''}
                  {item.pushDeliveredCount != null && item.pushDeliveredCount > 0
                    ? ` · push ×${item.pushDeliveredCount}`
                    : ''}
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
