import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getApprovalReminderRuns,
  getApprovalReminderSettings,
  getPendingApprovalReminders,
  upsertApprovalReminderSettings,
} from '../api/client'

interface ApprovalReminderSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function ApprovalReminderSettingsPanel({
  accessToken,
  canManage,
}: ApprovalReminderSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [prReminderAfterHours, setPrReminderAfterHours] = useState(24)
  const [poReminderAfterHours, setPoReminderAfterHours] = useState(24)
  const [reminderCooldownHours, setReminderCooldownHours] = useState(24)
  const [maxRemindersPerSubject, setMaxRemindersPerSubject] = useState(10)
  const [notifyOnPrApprovalReminder, setNotifyOnPrApprovalReminder] = useState(true)
  const [notifyOnPoApprovalReminder, setNotifyOnPoApprovalReminder] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-approval-reminder-settings', accessToken],
    queryFn: () => getApprovalReminderSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-approval-reminder-pending', accessToken],
    queryFn: () => getPendingApprovalReminders(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['supplyarr-approval-reminder-runs', accessToken],
    queryFn: () => getApprovalReminderRuns(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setPrReminderAfterHours(data.prReminderAfterHours)
    setPoReminderAfterHours(data.poReminderAfterHours)
    setReminderCooldownHours(data.reminderCooldownHours)
    setMaxRemindersPerSubject(data.maxRemindersPerSubject)
    setNotifyOnPrApprovalReminder(data.notifyOnPrApprovalReminder)
    setNotifyOnPoApprovalReminder(data.notifyOnPoApprovalReminder)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertApprovalReminderSettings(accessToken, {
        isEnabled,
        prReminderAfterHours,
        poReminderAfterHours,
        reminderCooldownHours,
        maxRemindersPerSubject,
        notifyOnPrApprovalReminder,
        notifyOnPoApprovalReminder,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-approval-reminder-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-approval-reminder-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-approval-reminder-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-approval-reminders', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="approval-reminder-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Approval reminder worker</h2>
      <p className="mt-1 text-sm text-slate-400">
        Send periodic reminders for purchase requests and purchase orders awaiting approval.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Approval reminder settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load approval reminder settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="approval-reminder-enabled" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="approval-reminder-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable automated approval reminders
        </label>

        <label htmlFor="approval-reminder-pr-hours" className="block text-sm text-slate-200">
          <span className="font-medium">PR reminder after (hours)</span>
          <input
            id="approval-reminder-pr-hours"
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="number"
            min={1}
            max={720}
            value={prReminderAfterHours}
            onChange={(event) => setPrReminderAfterHours(Number(event.target.value))}
          />
        </label>

        <label htmlFor="approval-reminder-po-hours" className="block text-sm text-slate-200">
          <span className="font-medium">PO reminder after (hours)</span>
          <input
            id="approval-reminder-po-hours"
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="number"
            min={1}
            max={720}
            value={poReminderAfterHours}
            onChange={(event) => setPoReminderAfterHours(Number(event.target.value))}
          />
        </label>

        <label htmlFor="approval-reminder-cooldown-hours" className="block text-sm text-slate-200">
          <span className="font-medium">Reminder cooldown (hours)</span>
          <input
            id="approval-reminder-cooldown-hours"
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="number"
            min={1}
            max={168}
            value={reminderCooldownHours}
            onChange={(event) => setReminderCooldownHours(Number(event.target.value))}
          />
        </label>

        <label htmlFor="approval-reminder-max-per-subject" className="block text-sm text-slate-200">
          <span className="font-medium">Max reminders per subject</span>
          <input
            id="approval-reminder-max-per-subject"
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="number"
            min={1}
            max={100}
            value={maxRemindersPerSubject}
            onChange={(event) => setMaxRemindersPerSubject(Number(event.target.value))}
          />
        </label>

        <label htmlFor="approval-reminder-notify-pr" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="approval-reminder-notify-pr"
            type="checkbox"
            checked={notifyOnPrApprovalReminder}
            onChange={(event) => setNotifyOnPrApprovalReminder(event.target.checked)}
          />
          Notify on PR approval reminders (requires notification webhook)
        </label>

        <label htmlFor="approval-reminder-notify-po" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="approval-reminder-notify-po"
            type="checkbox"
            checked={notifyOnPoApprovalReminder}
            onChange={(event) => setNotifyOnPoApprovalReminder(event.target.checked)}
          />
          Notify on PO approval reminders (requires notification webhook)
        </label>

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
        >
          {saveMutation.isPending ? 'Saving…' : 'Save reminder settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save approval reminder settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Due for reminder</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">No approvals currently due for reminder.</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {pendingQuery.data.items.map((item) => (
              <li key={`${item.subjectType}-${item.subjectId}`} className="px-3 py-2 text-slate-300">
                <div className="font-medium text-slate-100">
                  {item.documentKey} · {item.title}
                </div>
                <div className="text-xs text-[var(--color-text-muted)]">
                  {item.subjectType} · {Math.round(item.hoursPending)}h pending · {item.reminderCount} sent
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent runs</h3>
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">No worker runs yet.</p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2 text-slate-300">
                {run.remindersSentCount} sent / {run.candidatesFound} candidates
                {run.skippedCount > 0 ? ` · ${run.skippedCount} skipped` : ''}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
