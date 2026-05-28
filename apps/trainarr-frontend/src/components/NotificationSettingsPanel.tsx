import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getTrainingNotificationDispatches,
  getTrainingNotificationSettings,
  upsertTrainingNotificationSettings,
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
  const [notifyAssignmentCreated, setNotifyAssignmentCreated] = useState(true)
  const [notifyQualificationExpiring, setNotifyQualificationExpiring] = useState(true)
  const [notifyQualificationExpired, setNotifyQualificationExpired] = useState(true)
  const [expiringLeadDays, setExpiringLeadDays] = useState('30')

  const settingsQuery = useQuery({
    queryKey: ['trainarr-notification-settings', accessToken],
    queryFn: () => getTrainingNotificationSettings(accessToken),
    enabled: canManage,
  })

  const dispatchesQuery = useQuery({
    queryKey: ['trainarr-notification-dispatches', accessToken],
    queryFn: () => getTrainingNotificationDispatches(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setWebhookUrl(data.notificationWebhookUrl ?? '')
    setNotifyAssignmentCreated(data.notifyOnAssignmentCreated)
    setNotifyQualificationExpiring(data.notifyOnQualificationExpiring)
    setNotifyQualificationExpired(data.notifyOnQualificationExpired)
    setExpiringLeadDays(String(data.expiringLeadDays))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertTrainingNotificationSettings(accessToken, {
        isEnabled,
        notificationWebhookUrl: webhookUrl.trim() || null,
        notifyOnAssignmentCreated: notifyAssignmentCreated,
        notifyOnQualificationExpiring: notifyQualificationExpiring,
        notifyOnQualificationExpired: notifyQualificationExpired,
        expiringLeadDays: Number.parseInt(expiringLeadDays, 10) || 30,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-notification-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-notification-dispatches', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm">
      <h2 className="text-lg font-semibold text-foreground">Training notifications</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Configure HTTPS webhooks for assignment and qualification lifecycle events. Dispatch runs on a
        scheduled worker.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load notification settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable training notifications
        </label>

        <label className="block text-sm">
          <span className="font-medium">Webhook URL</span>
          <input
            className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="url"
            placeholder="https://hooks.example.com/trainarr"
            value={webhookUrl}
            onChange={(event) => setWebhookUrl(event.target.value)}
          />
        </label>

        <fieldset className="space-y-2 text-sm">
          <legend className="font-medium">Notify on</legend>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyAssignmentCreated}
              onChange={(event) => setNotifyAssignmentCreated(event.target.checked)}
            />
            Training assignment created
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyQualificationExpiring}
              onChange={(event) => setNotifyQualificationExpiring(event.target.checked)}
            />
            Qualification expiring soon
          </label>
          <label className="flex items-center gap-2">
            <input
              type="checkbox"
              checked={notifyQualificationExpired}
              onChange={(event) => setNotifyQualificationExpired(event.target.checked)}
            />
            Qualification expired
          </label>
        </fieldset>

        <label className="block text-sm">
          <span className="font-medium">Expiring lead (days)</span>
          <input
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={1}
            max={365}
            value={expiringLeadDays}
            onChange={(event) => setExpiringLeadDays(event.target.value)}
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
        >
          {saveMutation.isPending ? 'Saving…' : 'Save notification settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-destructive">Failed to save notification settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent dispatches</h3>
        {dispatchesQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading dispatch history…</p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground">No notification dispatches yet.</p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm">
            {dispatchesQuery.data.items.map((item) => (
              <li key={item.notificationId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.eventKind}</span>
                  <span className="text-muted-foreground">{item.dispatchStatus}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  {item.webhookHost ?? '—'}
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
