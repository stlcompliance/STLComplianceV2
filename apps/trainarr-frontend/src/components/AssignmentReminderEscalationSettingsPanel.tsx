import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getAssignmentDueReminderRuns,
  getAssignmentDueReminderSettings,
  getAssignmentEscalationEvents,
  getAssignmentEscalationRuns,
  getAssignmentEscalationSettings,
  getPendingAssignmentDueReminders,
  getPendingAssignmentEscalations,
  upsertAssignmentDueReminderSettings,
  upsertAssignmentEscalationSettings,
} from '../api/client'

interface AssignmentReminderEscalationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function AssignmentReminderEscalationSettingsPanel({
  accessToken,
  canManage,
}: AssignmentReminderEscalationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [reminderInitialized, setReminderInitialized] = useState(false)
  const [escalationInitialized, setEscalationInitialized] = useState(false)
  const [reminderEnabled, setReminderEnabled] = useState(false)
  const [dueSoonLeadDays, setDueSoonLeadDays] = useState('7')
  const [reminderCooldownHours, setReminderCooldownHours] = useState('24')
  const [maxReminders, setMaxReminders] = useState('5')
  const [escalationEnabled, setEscalationEnabled] = useState(false)
  const [overdueHours, setOverdueHours] = useState('24')
  const [escalationCooldownHours, setEscalationCooldownHours] = useState('48')
  const [maxEscalations, setMaxEscalations] = useState('10')

  const reminderSettingsQuery = useQuery({
    queryKey: ['trainarr-due-reminder-settings', accessToken],
    queryFn: () => getAssignmentDueReminderSettings(accessToken),
    enabled: canManage,
  })

  const escalationSettingsQuery = useQuery({
    queryKey: ['trainarr-assignment-escalation-settings', accessToken],
    queryFn: () => getAssignmentEscalationSettings(accessToken),
    enabled: canManage,
  })

  const pendingRemindersQuery = useQuery({
    queryKey: ['trainarr-pending-due-reminders', accessToken],
    queryFn: () => getPendingAssignmentDueReminders(accessToken),
    enabled: canManage && reminderEnabled,
  })

  const pendingEscalationsQuery = useQuery({
    queryKey: ['trainarr-pending-assignment-escalations', accessToken],
    queryFn: () => getPendingAssignmentEscalations(accessToken),
    enabled: canManage && escalationEnabled,
  })

  const reminderRunsQuery = useQuery({
    queryKey: ['trainarr-due-reminder-runs', accessToken],
    queryFn: () => getAssignmentDueReminderRuns(accessToken, 5),
    enabled: canManage,
  })

  const escalationRunsQuery = useQuery({
    queryKey: ['trainarr-assignment-escalation-runs', accessToken],
    queryFn: () => getAssignmentEscalationRuns(accessToken, 5),
    enabled: canManage,
  })

  const escalationEventsQuery = useQuery({
    queryKey: ['trainarr-assignment-escalation-events', accessToken],
    queryFn: () => getAssignmentEscalationEvents(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (reminderInitialized || reminderSettingsQuery.isLoading || !reminderSettingsQuery.data) {
      return
    }
    const data = reminderSettingsQuery.data
    setReminderEnabled(data.isEnabled)
    setDueSoonLeadDays(String(data.dueSoonLeadDays))
    setReminderCooldownHours(String(data.reminderCooldownHours))
    setMaxReminders(String(data.maxRemindersPerAssignment))
    setReminderInitialized(true)
  }, [reminderInitialized, reminderSettingsQuery.data, reminderSettingsQuery.isLoading])

  useEffect(() => {
    if (escalationInitialized || escalationSettingsQuery.isLoading || !escalationSettingsQuery.data) {
      return
    }
    const data = escalationSettingsQuery.data
    setEscalationEnabled(data.isEnabled)
    setOverdueHours(String(data.overdueEscalationAfterHours))
    setEscalationCooldownHours(String(data.escalationCooldownHours))
    setMaxEscalations(String(data.maxEscalationsPerAssignment))
    setEscalationInitialized(true)
  }, [escalationInitialized, escalationSettingsQuery.data, escalationSettingsQuery.isLoading])

  const saveReminderMutation = useMutation({
    mutationFn: () =>
      upsertAssignmentDueReminderSettings(accessToken, {
        isEnabled: reminderEnabled,
        dueSoonLeadDays: Number.parseInt(dueSoonLeadDays, 10) || 7,
        reminderCooldownHours: Number.parseInt(reminderCooldownHours, 10) || 24,
        maxRemindersPerAssignment: Number.parseInt(maxReminders, 10) || 5,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-due-reminder-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-pending-due-reminders', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-due-reminder-runs', accessToken] })
    },
  })

  const saveEscalationMutation = useMutation({
    mutationFn: () =>
      upsertAssignmentEscalationSettings(accessToken, {
        isEnabled: escalationEnabled,
        overdueEscalationAfterHours: Number.parseInt(overdueHours, 10) || 24,
        escalationCooldownHours: Number.parseInt(escalationCooldownHours, 10) || 48,
        maxEscalationsPerAssignment: Number.parseInt(maxEscalations, 10) || 10,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment-escalation-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-pending-assignment-escalations', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment-escalation-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-assignment-escalation-events', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="assignment-reminder-escalation-settings-panel"
    >
      <h2 className="text-lg font-semibold text-foreground">Assignment due reminders & escalations</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Scheduled workers scan open training assignments. Due reminders fire before the due date; escalations fire
        after overdue thresholds. Webhook delivery uses Training notifications toggles above.
      </p>

      <div className="mt-6 space-y-6">
        <div data-testid="due-reminder-settings-section">
          <h3 className="text-sm font-semibold text-foreground">Due reminders</h3>
          <div className="mt-3 space-y-3">
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={reminderEnabled}
                onChange={(event) => setReminderEnabled(event.target.checked)}
                data-testid="due-reminder-enabled"
              />
              Enable assignment due reminders
            </label>
            <div className="flex flex-wrap gap-4">
              <label className="block text-sm">
                <span className="font-medium">Due soon lead (days)</span>
                <input
                  className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
                  type="number"
                  min={1}
                  max={90}
                  value={dueSoonLeadDays}
                  onChange={(event) => setDueSoonLeadDays(event.target.value)}
                />
              </label>
              <label className="block text-sm">
                <span className="font-medium">Cooldown (hours)</span>
                <input
                  className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
                  type="number"
                  min={1}
                  max={168}
                  value={reminderCooldownHours}
                  onChange={(event) => setReminderCooldownHours(event.target.value)}
                />
              </label>
              <label className="block text-sm">
                <span className="font-medium">Max reminders</span>
                <input
                  className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
                  type="number"
                  min={1}
                  max={50}
                  value={maxReminders}
                  onChange={(event) => setMaxReminders(event.target.value)}
                />
              </label>
            </div>
            <button
              type="button"
              className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
              disabled={saveReminderMutation.isPending}
              onClick={() => saveReminderMutation.mutate()}
              data-testid="due-reminder-save"
            >
              {saveReminderMutation.isPending ? 'Saving…' : 'Save due reminder settings'}
            </button>
          </div>
          {reminderEnabled && pendingRemindersQuery.data && (
            <p className="mt-2 text-xs text-muted-foreground" data-testid="due-reminder-pending-count">
              {pendingRemindersQuery.data.items.length} assignment(s) due for reminder now
            </p>
          )}
          {reminderRunsQuery.data && reminderRunsQuery.data.items.length > 0 && (
            <ul className="mt-2 divide-y divide-border rounded-md border border-border text-xs">
              {reminderRunsQuery.data.items.map((run) => (
                <li key={run.runId} className="px-3 py-2">
                  sent {run.remindersSentCount} / {run.candidatesFound} candidates · skipped {run.skippedCount}
                </li>
              ))}
            </ul>
          )}
        </div>

        <div data-testid="assignment-escalation-settings-section">
          <h3 className="text-sm font-semibold text-foreground">Overdue escalations</h3>
          <div className="mt-3 space-y-3">
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={escalationEnabled}
                onChange={(event) => setEscalationEnabled(event.target.checked)}
                data-testid="assignment-escalation-enabled"
              />
              Enable assignment overdue escalations
            </label>
            <div className="flex flex-wrap gap-4">
              <label className="block text-sm">
                <span className="font-medium">Overdue after (hours)</span>
                <input
                  className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
                  type="number"
                  min={1}
                  max={720}
                  value={overdueHours}
                  onChange={(event) => setOverdueHours(event.target.value)}
                />
              </label>
              <label className="block text-sm">
                <span className="font-medium">Cooldown (hours)</span>
                <input
                  className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
                  type="number"
                  min={1}
                  max={168}
                  value={escalationCooldownHours}
                  onChange={(event) => setEscalationCooldownHours(event.target.value)}
                />
              </label>
              <label className="block text-sm">
                <span className="font-medium">Max escalations</span>
                <input
                  className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
                  type="number"
                  min={1}
                  max={50}
                  value={maxEscalations}
                  onChange={(event) => setMaxEscalations(event.target.value)}
                />
              </label>
            </div>
            <button
              type="button"
              className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
              disabled={saveEscalationMutation.isPending}
              onClick={() => saveEscalationMutation.mutate()}
              data-testid="assignment-escalation-save"
            >
              {saveEscalationMutation.isPending ? 'Saving…' : 'Save escalation settings'}
            </button>
          </div>
          {escalationEnabled && pendingEscalationsQuery.data && (
            <p className="mt-2 text-xs text-muted-foreground" data-testid="assignment-escalation-pending-count">
              {pendingEscalationsQuery.data.items.length} assignment(s) due for escalation now
            </p>
          )}
          {escalationRunsQuery.data && escalationRunsQuery.data.items.length > 0 && (
            <ul className="mt-2 divide-y divide-border rounded-md border border-border text-xs">
              {escalationRunsQuery.data.items.map((run) => (
                <li key={run.runId} className="px-3 py-2">
                  escalated {run.escalatedCount} / {run.candidatesFound} candidates · skipped {run.skippedCount}
                </li>
              ))}
            </ul>
          )}
          {escalationEventsQuery.data && escalationEventsQuery.data.items.length > 0 && (
            <ul className="mt-2 divide-y divide-border rounded-md border border-border text-xs">
              {escalationEventsQuery.data.items.map((evt) => (
                <li key={evt.eventId} className="px-3 py-2">
                  assignment {evt.trainingAssignmentId.slice(0, 8)}… · escalation #{evt.escalationCount}
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  )
}
