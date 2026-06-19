import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getProcurementNotificationDispatches,
  getProcurementNotificationSettings,
  upsertProcurementNotificationSettings,
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
  const [notifyPurchaseRequestSubmitted, setNotifyPurchaseRequestSubmitted] = useState(true)
  const [notifyPurchaseRequestApproved, setNotifyPurchaseRequestApproved] = useState(true)
  const [notifyPurchaseOrderIssued, setNotifyPurchaseOrderIssued] = useState(true)
  const [notifyReceivingReceiptPosted, setNotifyReceivingReceiptPosted] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-notification-settings', accessToken],
    queryFn: () => getProcurementNotificationSettings(accessToken),
    enabled: canManage,
  })

  const dispatchesQuery = useQuery({
    queryKey: ['supplyarr-notification-dispatches', accessToken],
    queryFn: () => getProcurementNotificationDispatches(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setWebhookUrl(data.notificationWebhookUrl ?? '')
    setNotifyPurchaseRequestSubmitted(data.notifyOnPurchaseRequestSubmitted)
    setNotifyPurchaseRequestApproved(data.notifyOnPurchaseRequestApproved)
    setNotifyPurchaseOrderIssued(data.notifyOnPurchaseOrderIssued)
    setNotifyReceivingReceiptPosted(data.notifyOnReceivingReceiptPosted)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertProcurementNotificationSettings(accessToken, {
        isEnabled,
        notificationWebhookUrl: webhookUrl.trim() || null,
        notifyOnPurchaseRequestSubmitted: notifyPurchaseRequestSubmitted,
        notifyOnPurchaseRequestApproved: notifyPurchaseRequestApproved,
        notifyOnPurchaseOrderIssued: notifyPurchaseOrderIssued,
        notifyOnReceivingReceiptPosted: notifyReceivingReceiptPosted,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-notification-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-notification-dispatches', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="notification-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Procurement notifications</h2>
      <p className="mt-1 text-sm text-slate-400">
        Configure HTTPS webhooks for purchase request, purchase order, and LoadArr receipt-posted
        events mirrored into procurement coordination. Dispatch runs on a scheduled worker.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Notification settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load notification settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="notification-settings-enabled" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="notification-settings-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="notification-settings-enabled"
          />
          Enable procurement notifications
        </label>

        <label htmlFor="notification-settings-webhook" className="block text-sm text-slate-200">
          <span className="font-medium">Webhook URL</span>
          <input
            id="notification-settings-webhook"
            className="mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="url"
            placeholder="https://hooks.example.com/supplyarr"
            value={webhookUrl}
            onChange={(event) => setWebhookUrl(event.target.value)}
            data-testid="notification-settings-webhook"
          />
        </label>

        <fieldset className="space-y-2 text-sm text-slate-200">
          <legend className="font-medium text-slate-100">Notify on</legend>
          <label htmlFor="notification-settings-pr-submitted" className="flex items-center gap-2">
            <input
              id="notification-settings-pr-submitted"
              type="checkbox"
              checked={notifyPurchaseRequestSubmitted}
              onChange={(event) => setNotifyPurchaseRequestSubmitted(event.target.checked)}
            />
            Purchase request submitted
          </label>
          <label htmlFor="notification-settings-pr-approved" className="flex items-center gap-2">
            <input
              id="notification-settings-pr-approved"
              type="checkbox"
              checked={notifyPurchaseRequestApproved}
              onChange={(event) => setNotifyPurchaseRequestApproved(event.target.checked)}
            />
            Purchase request approved
          </label>
          <label htmlFor="notification-settings-po-issued" className="flex items-center gap-2">
            <input
              id="notification-settings-po-issued"
              type="checkbox"
              checked={notifyPurchaseOrderIssued}
              onChange={(event) => setNotifyPurchaseOrderIssued(event.target.checked)}
            />
            Purchase order issued
          </label>
          <label htmlFor="notification-settings-receiving-posted" className="flex items-center gap-2">
            <input
              id="notification-settings-receiving-posted"
              type="checkbox"
              checked={notifyReceivingReceiptPosted}
              onChange={(event) => setNotifyReceivingReceiptPosted(event.target.checked)}
            />
            LoadArr receipt posted
          </label>
        </fieldset>

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="notification-settings-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save notification settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save notification settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent dispatches</h3>
        {dispatchesQuery.isLoading && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading dispatch history…</p>
        )}
        {dispatchesQuery.data && dispatchesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]" data-testid="notification-dispatches-empty">
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
                className="px-3 py-2 text-slate-300"
                data-testid={`notification-dispatch-row-${item.relatedEntityId}`}
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{item.eventKind}</span>
                  <span className="text-[var(--color-text-muted)]">{item.dispatchStatus}</span>
                </div>
                <div className="text-xs text-[var(--color-text-muted)]">
                  {item.relatedEntityType} {item.relatedEntityId}
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
