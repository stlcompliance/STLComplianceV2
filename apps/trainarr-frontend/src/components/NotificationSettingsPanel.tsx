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

  const [notifyAssignmentCompleted, setNotifyAssignmentCompleted] = useState(true)

  const [notifyQualificationExpiring, setNotifyQualificationExpiring] = useState(true)

  const [notifyQualificationIssued, setNotifyQualificationIssued] = useState(true)

  const [notifyQualificationSuspended, setNotifyQualificationSuspended] = useState(true)

  const [notifyQualificationRevoked, setNotifyQualificationRevoked] = useState(true)

  const [notifyQualificationExpired, setNotifyQualificationExpired] = useState(true)

  const [notifyAssignmentDueReminder, setNotifyAssignmentDueReminder] = useState(true)

  const [notifyAssignmentOverdueEscalation, setNotifyAssignmentOverdueEscalation] = useState(true)

  const [expiringLeadDays, setExpiringLeadDays] = useState('30')

  const [maxAttempts, setMaxAttempts] = useState('10')

  const [retryIntervalMinutes, setRetryIntervalMinutes] = useState('5')



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

    setNotifyAssignmentCompleted(data.notifyOnAssignmentCompleted)

    setNotifyQualificationExpiring(data.notifyOnQualificationExpiring)

    setNotifyQualificationIssued(data.notifyOnQualificationIssued)

    setNotifyQualificationSuspended(data.notifyOnQualificationSuspended)

    setNotifyQualificationRevoked(data.notifyOnQualificationRevoked)

    setNotifyQualificationExpired(data.notifyOnQualificationExpired)

    setNotifyAssignmentDueReminder(data.notifyOnAssignmentDueReminder)

    setNotifyAssignmentOverdueEscalation(data.notifyOnAssignmentOverdueEscalation)

    setExpiringLeadDays(String(data.expiringLeadDays))

    setMaxAttempts(String(data.maxAttempts))

    setRetryIntervalMinutes(String(data.retryIntervalMinutes))

    setInitialized(true)

  }, [initialized, settingsQuery.data, settingsQuery.isLoading])



  const saveMutation = useMutation({

    mutationFn: () =>

      upsertTrainingNotificationSettings(accessToken, {

        isEnabled,

        notificationWebhookUrl: webhookUrl.trim() || null,

        notifyOnAssignmentCreated: notifyAssignmentCreated,

        notifyOnAssignmentCompleted: notifyAssignmentCompleted,

        notifyOnQualificationExpiring: notifyQualificationExpiring,

        notifyOnQualificationIssued: notifyQualificationIssued,

        notifyOnQualificationSuspended: notifyQualificationSuspended,

        notifyOnQualificationRevoked: notifyQualificationRevoked,

        notifyOnQualificationExpired: notifyQualificationExpired,

        notifyOnAssignmentDueReminder: notifyAssignmentDueReminder,

        notifyOnAssignmentOverdueEscalation: notifyAssignmentOverdueEscalation,

        expiringLeadDays: Number.parseInt(expiringLeadDays, 10) || 30,

        maxAttempts: Number.parseInt(maxAttempts, 10) || 10,

        retryIntervalMinutes: Number.parseInt(retryIntervalMinutes, 10) || 5,

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

    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="notification-settings-panel"
    >

      <h2 className="text-lg font-semibold text-foreground">Training notifications</h2>

      <p className="mt-1 text-sm text-muted-foreground">

        Configure HTTPS webhooks for training lifecycle events. Failed deliveries retry on a schedule until

        abandoned.

      </p>



      {settingsQuery.isError && (

        <p className="mt-3 text-sm text-destructive">Failed to load notification settings.</p>

      )}



      <div className="mt-4 space-y-3">
        <label htmlFor="notification-settings-enabled" className="flex items-center gap-2 text-sm">
          <input
            id="notification-settings-enabled"
            type="checkbox"
            data-testid="notification-settings-enabled"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable training notifications
        </label>

        <label htmlFor="notification-settings-webhook-url" className="block text-sm">
          <span className="font-medium">Webhook URL</span>
          <input
            id="notification-settings-webhook-url"
            className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="url"
            value={webhookUrl}
            onChange={(event) => setWebhookUrl(event.target.value)}
          />
        </label>

        <fieldset className="space-y-2 text-sm">
          <legend className="font-medium">Notify on</legend>
          <label htmlFor="notification-settings-notify-assignment-created" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-assignment-created"
              type="checkbox"
              data-testid="notification-settings-notify-assignment-created"
              checked={notifyAssignmentCreated}
              onChange={(event) => setNotifyAssignmentCreated(event.target.checked)}
            />
            Training assignment created
          </label>
          <label htmlFor="notification-settings-notify-assignment-completed" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-assignment-completed"
              type="checkbox"
              data-testid="notification-settings-notify-assignment-completed"
              checked={notifyAssignmentCompleted}
              onChange={(event) => setNotifyAssignmentCompleted(event.target.checked)}
            />
            Training assignment completed
          </label>
          <label htmlFor="notification-settings-notify-qualification-expiring" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-qualification-expiring"
              type="checkbox"
              data-testid="notification-settings-notify-qualification-expiring"
              checked={notifyQualificationExpiring}
              onChange={(event) => setNotifyQualificationExpiring(event.target.checked)}
            />
            Qualification expiring soon
          </label>
          <label htmlFor="notification-settings-notify-qualification-issued" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-qualification-issued"
              type="checkbox"
              data-testid="notification-settings-notify-qualification-issued"
              checked={notifyQualificationIssued}
              onChange={(event) => setNotifyQualificationIssued(event.target.checked)}
            />
            Qualification issued
          </label>
          <label htmlFor="notification-settings-notify-qualification-suspended" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-qualification-suspended"
              type="checkbox"
              data-testid="notification-settings-notify-qualification-suspended"
              checked={notifyQualificationSuspended}
              onChange={(event) => setNotifyQualificationSuspended(event.target.checked)}
            />
            Qualification suspended
          </label>
          <label htmlFor="notification-settings-notify-qualification-revoked" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-qualification-revoked"
              type="checkbox"
              data-testid="notification-settings-notify-qualification-revoked"
              checked={notifyQualificationRevoked}
              onChange={(event) => setNotifyQualificationRevoked(event.target.checked)}
            />
            Qualification revoked
          </label>
          <label htmlFor="notification-settings-notify-qualification-expired" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-qualification-expired"
              type="checkbox"
              data-testid="notification-settings-notify-qualification-expired"
              checked={notifyQualificationExpired}
              onChange={(event) => setNotifyQualificationExpired(event.target.checked)}
            />
            Qualification expired
          </label>
          <label htmlFor="notification-settings-notify-assignment-due-reminder" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-assignment-due-reminder"
              type="checkbox"
              data-testid="notification-settings-notify-assignment-due-reminder"
              checked={notifyAssignmentDueReminder}
              onChange={(event) => setNotifyAssignmentDueReminder(event.target.checked)}
            />
            Assignment due reminder (scheduled worker)
          </label>
          <label htmlFor="notification-settings-notify-assignment-overdue-escalation" className="flex items-center gap-2">
            <input
              id="notification-settings-notify-assignment-overdue-escalation"
              type="checkbox"
              data-testid="notification-settings-notify-assignment-overdue-escalation"
              checked={notifyAssignmentOverdueEscalation}
              onChange={(event) => setNotifyAssignmentOverdueEscalation(event.target.checked)}
            />
            Assignment overdue escalation (scheduled worker)
          </label>
        </fieldset>

        <div className="flex flex-wrap gap-4">
          <label htmlFor="notification-settings-expiring-lead-days" className="block text-sm">
            <span className="font-medium">Expiring lead (days)</span>
            <input
              id="notification-settings-expiring-lead-days"
              className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
              type="number"
              min={1}
              max={365}
              value={expiringLeadDays}
              onChange={(event) => setExpiringLeadDays(event.target.value)}
            />
          </label>
          <label htmlFor="notification-settings-max-attempts" className="block text-sm">
            <span className="font-medium">Max attempts</span>
            <input
              id="notification-settings-max-attempts"
              className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
              type="number"
              min={1}
              max={50}
              value={maxAttempts}
              onChange={(event) => setMaxAttempts(event.target.value)}
            />
          </label>
          <label htmlFor="notification-settings-retry-interval" className="block text-sm">
            <span className="font-medium">Retry interval (minutes)</span>
            <input
              id="notification-settings-retry-interval"
              className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
              type="number"
              min={1}
              max={1440}
              value={retryIntervalMinutes}
              onChange={(event) => setRetryIntervalMinutes(event.target.value)}
            />
          </label>
        </div>



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

                  attempts {item.attemptCount}

                  {item.nextRetryAt ? ` · retry ${item.nextRetryAt}` : ''}

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



