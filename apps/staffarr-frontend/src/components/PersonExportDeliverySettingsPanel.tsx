import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getPersonExportDeliveryNotifications,
  getPersonExportDeliveryPending,
  getPersonExportDeliveryRuns,
  getPersonExportSchedule,
  upsertPersonExportSchedule,
} from '../api/client'

interface PersonExportDeliverySettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function PersonExportDeliverySettingsPanel({
  accessToken,
  canManage,
}: PersonExportDeliverySettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [scheduleEnabled, setScheduleEnabled] = useState(false)
  const [scheduleIntervalHours, setScheduleIntervalHours] = useState('24')
  const [notificationWebhookUrl, setNotificationWebhookUrl] = useState('')
  const [notifyOnSuccess, setNotifyOnSuccess] = useState(true)
  const [notifyOnFailure, setNotifyOnFailure] = useState(true)

  const scheduleQuery = useQuery({
    queryKey: ['staffarr-people-export-schedule', accessToken],
    queryFn: () => getPersonExportSchedule(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['staffarr-people-export-delivery-pending', accessToken],
    queryFn: () => getPersonExportDeliveryPending(accessToken),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['staffarr-people-export-delivery-runs', accessToken],
    queryFn: () => getPersonExportDeliveryRuns(accessToken, 5),
    enabled: canManage,
  })

  const notificationsQuery = useQuery({
    queryKey: ['staffarr-people-export-delivery-notifications', accessToken],
    queryFn: () => getPersonExportDeliveryNotifications(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || scheduleQuery.isLoading || !scheduleQuery.data) {
      return
    }
    setScheduleEnabled(scheduleQuery.data.isEnabled)
    setScheduleIntervalHours(String(scheduleQuery.data.intervalHours))
    setNotificationWebhookUrl(scheduleQuery.data.notificationWebhookUrl ?? '')
    setNotifyOnSuccess(scheduleQuery.data.notifyOnSuccess)
    setNotifyOnFailure(scheduleQuery.data.notifyOnFailure)
    setInitialized(true)
  }, [initialized, scheduleQuery.data, scheduleQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertPersonExportSchedule(accessToken, {
        isEnabled: scheduleEnabled,
        intervalHours: Number.parseInt(scheduleIntervalHours, 10) || 24,
        notificationWebhookUrl: notificationWebhookUrl.trim() || null,
        notifyOnSuccess,
        notifyOnFailure,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['staffarr-people-export-schedule', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['staffarr-people-export-delivery-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['staffarr-people-export-delivery-runs', accessToken] })
      void queryClient.invalidateQueries({
        queryKey: ['staffarr-people-export-delivery-notifications', accessToken],
      })
    },
  })

  if (!canManage) {
    return null
  }

  const lastDeliveredLabel = scheduleQuery.data?.lastDeliveredAt
    ? new Date(scheduleQuery.data.lastDeliveredAt).toLocaleString()
    : 'Never'

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm"
      data-testid="person-export-delivery-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Person export scheduled delivery</h2>
      <p className="mt-1 text-sm text-slate-400">
        Runs workforce exports on an interval using tenant default filters. Delivery runs and webhook notifications
        are recorded for operators.
      </p>

      <dl className="mt-4 grid gap-2 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-slate-500">Last delivered</dt>
          <dd className="font-medium text-slate-100">{lastDeliveredLabel}</dd>
        </div>
        <div>
          <dt className="text-slate-500">Pending deliveries</dt>
          <dd className="font-medium text-slate-100">{pendingQuery.data?.items.length ?? 0}</dd>
        </div>
      </dl>

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input
            type="checkbox"
            checked={scheduleEnabled}
            onChange={(event) => setScheduleEnabled(event.target.checked)}
          />
          Enable scheduled delivery
        </label>

        <label className="block text-sm text-slate-200">
          Delivery interval (hours)
          <input
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
            type="number"
            min={1}
            max={720}
            value={scheduleIntervalHours}
            onChange={(event) => setScheduleIntervalHours(event.target.value)}
          />
        </label>

        <label className="block text-sm text-slate-200">
          Notification webhook URL (optional)
          <input
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2"
            type="url"
            value={notificationWebhookUrl}
            onChange={(event) => setNotificationWebhookUrl(event.target.value)}
            placeholder="https://hooks.example.com/staffarr-export"
          />
        </label>

        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input
            type="checkbox"
            checked={notifyOnSuccess}
            onChange={(event) => setNotifyOnSuccess(event.target.checked)}
          />
          Notify on successful delivery
        </label>

        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input
            type="checkbox"
            checked={notifyOnFailure}
            onChange={(event) => setNotifyOnFailure(event.target.checked)}
          />
          Notify on failed delivery
        </label>

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="person-export-delivery-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save export delivery settings'}
        </button>
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Pending delivery preview</h3>
        {pendingQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-400">Loading pending preview…</p>
        ) : null}
        {pendingQuery.data && pendingQuery.data.items.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400" data-testid="person-export-delivery-pending-empty">
            No scheduled deliveries are currently due.
          </p>
        ) : null}
        {pendingQuery.data && pendingQuery.data.items.length > 0 ? (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="person-export-delivery-pending-list"
          >
            {pendingQuery.data.items.map((item) => (
              <li key={`${item.tenantId}-${item.intervalHours}`} className="px-3 py-2 text-slate-200">
                Every {item.intervalHours}h
                {item.lastDeliveredAt
                  ? ` · last delivered ${new Date(item.lastDeliveredAt).toLocaleString()}`
                  : ' · never delivered'}
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent delivery runs</h3>
        {runsQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-400">Loading delivery runs…</p>
        ) : null}
        {runsQuery.data && runsQuery.data.items.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400" data-testid="person-export-delivery-runs-empty">
            No delivery runs yet.
          </p>
        ) : null}
        {runsQuery.data && runsQuery.data.items.length > 0 ? (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="person-export-delivery-runs-list"
          >
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2 text-slate-200">
                {run.status} · {run.personCount} people · {new Date(run.completedAt).toLocaleString()}
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent delivery notifications</h3>
        {notificationsQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-400">Loading delivery notifications…</p>
        ) : null}
        {notificationsQuery.data && notificationsQuery.data.items.length === 0 ? (
          <p className="mt-2 text-sm text-slate-400" data-testid="person-export-delivery-notifications-empty">
            No delivery notifications yet.
          </p>
        ) : null}
        {notificationsQuery.data && notificationsQuery.data.items.length > 0 ? (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="person-export-delivery-notifications-list"
          >
            {notificationsQuery.data.items.map((item) => (
              <li key={item.notificationId} className="px-3 py-2 text-slate-200">
                {item.eventKind} · {item.deliveryStatus}
                {item.webhookHost ? ` → ${item.webhookHost}` : ''}
              </li>
            ))}
          </ul>
        ) : null}
      </div>
    </section>
  )
}
