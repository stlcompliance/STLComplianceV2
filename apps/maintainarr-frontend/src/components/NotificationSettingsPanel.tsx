import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getMaintenanceNotificationDispatches,
  getMaintenanceNotificationSettings,
  upsertMaintenanceNotificationSettings,
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
  const [notifyWorkOrderCreated, setNotifyWorkOrderCreated] = useState(true)
  const [notifyPmScheduleDue, setNotifyPmScheduleDue] = useState(true)
  const [notifyPmScheduleOverdue, setNotifyPmScheduleOverdue] = useState(true)
  const [notifyDefectEscalated, setNotifyDefectEscalated] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-notification-settings', accessToken],
    queryFn: () => getMaintenanceNotificationSettings(accessToken),
    enabled: canManage,
  })

  const dispatchesQuery = useQuery({
    queryKey: ['maintainarr-notification-dispatches', accessToken],
    queryFn: () => getMaintenanceNotificationDispatches(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setWebhookUrl(data.notificationWebhookUrl ?? '')
    setNotifyWorkOrderCreated(data.notifyOnWorkOrderCreated)
    setNotifyPmScheduleDue(data.notifyOnPmScheduleDue)
    setNotifyPmScheduleOverdue(data.notifyOnPmScheduleOverdue)
    setNotifyDefectEscalated(data.notifyOnDefectEscalated)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertMaintenanceNotificationSettings(accessToken, {
        isEnabled,
        notificationWebhookUrl: webhookUrl.trim() || null,
        notifyOnWorkOrderCreated: notifyWorkOrderCreated,
        notifyOnPmScheduleDue: notifyPmScheduleDue,
        notifyOnPmScheduleOverdue: notifyPmScheduleOverdue,
        notifyOnDefectEscalated: notifyDefectEscalated,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-notification-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-notification-dispatches', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="notification-settings-panel"
    >
      <h2 className="text-lg font-semibold text-foreground">Maintenance notifications</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Configure HTTPS webhooks for work order and PM schedule lifecycle events. Dispatch runs on a
        scheduled worker.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load notification settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm" htmlFor="notification-settings-enabled">
          <input
            id="notification-settings-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="notification-settings-enabled"
          />
          Enable maintenance notifications
        </label>

        <label className="block text-sm" htmlFor="notification-settings-webhook">
          <span className="font-medium">Webhook URL</span>
          <input
            id="notification-settings-webhook"
            className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="url"
            placeholder="https://hooks.example.com/maintainarr"
            value={webhookUrl}
            onChange={(event) => setWebhookUrl(event.target.value)}
            data-testid="notification-settings-webhook"
          />
        </label>

        <fieldset className="space-y-2 text-sm">
          <legend className="font-medium">Notify on</legend>
          <label className="flex items-center gap-2" htmlFor="notification-notify-work-order-created">
            <input
              id="notification-notify-work-order-created"
              type="checkbox"
              checked={notifyWorkOrderCreated}
              onChange={(event) => setNotifyWorkOrderCreated(event.target.checked)}
              data-testid="notification-notify-work-order-created"
            />
            Work order created
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-notify-pm-schedule-due">
            <input
              id="notification-notify-pm-schedule-due"
              type="checkbox"
              checked={notifyPmScheduleDue}
              onChange={(event) => setNotifyPmScheduleDue(event.target.checked)}
              data-testid="notification-notify-pm-schedule-due"
            />
            PM schedule due
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-notify-pm-schedule-overdue">
            <input
              id="notification-notify-pm-schedule-overdue"
              type="checkbox"
              checked={notifyPmScheduleOverdue}
              onChange={(event) => setNotifyPmScheduleOverdue(event.target.checked)}
              data-testid="notification-notify-pm-schedule-overdue"
            />
            PM schedule overdue
          </label>
          <label className="flex items-center gap-2" htmlFor="notification-notify-defect-escalated">
            <input
              id="notification-notify-defect-escalated"
              type="checkbox"
              checked={notifyDefectEscalated}
              onChange={(event) => setNotifyDefectEscalated(event.target.checked)}
              data-testid="notification-notify-defect-escalated"
            />
            Defect escalated
          </label>
        </fieldset>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="notification-settings-save"
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
          <p className="mt-2 text-sm text-muted-foreground" data-testid="notification-dispatches-empty">
            No notification dispatches yet.
          </p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="notification-dispatches-list"
          >
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
