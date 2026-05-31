import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getPendingProcurementExceptionEscalations,
  getProcurementExceptionEscalationEvents,
  getProcurementExceptionEscalationRuns,
  getProcurementExceptionEscalationSettings,
  upsertProcurementExceptionEscalationSettings,
} from '../api/client'

interface ProcurementExceptionEscalationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function ProcurementExceptionEscalationSettingsPanel({
  accessToken,
  canManage,
}: ProcurementExceptionEscalationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [escalationCooldownHours, setEscalationCooldownHours] = useState(24)
  const [maxEscalationsPerException, setMaxEscalationsPerException] = useState(5)
  const [notifyOnProcurementExceptionSlaEscalation, setNotifyOnProcurementExceptionSlaEscalation] =
    useState(true)

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-procurement-exception-escalation-settings', accessToken],
    queryFn: () => getProcurementExceptionEscalationSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-procurement-exception-escalation-pending', accessToken],
    queryFn: () => getPendingProcurementExceptionEscalations(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['supplyarr-procurement-exception-escalation-runs', accessToken],
    queryFn: () => getProcurementExceptionEscalationRuns(accessToken, 5),
    enabled: canManage,
  })

  const eventsQuery = useQuery({
    queryKey: ['supplyarr-procurement-exception-escalation-events', accessToken],
    queryFn: () => getProcurementExceptionEscalationEvents(accessToken, 10),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setEscalationCooldownHours(data.escalationCooldownHours)
    setMaxEscalationsPerException(data.maxEscalationsPerException)
    setNotifyOnProcurementExceptionSlaEscalation(data.notifyOnProcurementExceptionSlaEscalation)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertProcurementExceptionEscalationSettings(accessToken, {
        isEnabled,
        escalationCooldownHours,
        maxEscalationsPerException,
        notifyOnProcurementExceptionSlaEscalation,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-procurement-exception-escalation-settings', accessToken],
      })
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-procurement-exception-escalation-pending', accessToken],
      })
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-procurement-exception-escalation-runs', accessToken],
      })
      void queryClient.invalidateQueries({
        queryKey: ['supplyarr-procurement-exception-escalation-events', accessToken],
      })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="procurement-exception-escalation-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Procurement exception SLA escalation</h2>
      <p className="mt-1 text-sm text-slate-400">
        Escalate overdue procurement exceptions on SLA breach and enqueue webhook notifications.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Escalation settings unavailable"
            message={getErrorMessage(
              settingsQuery.error,
              'Failed to load procurement exception escalation settings.',
            )}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="procurement-exception-escalation-enabled" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="procurement-exception-escalation-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="procurement-exception-escalation-enabled"
          />
          Enable automated SLA escalation worker
        </label>

        <label htmlFor="procurement-exception-escalation-cooldown-hours" className="block text-sm text-slate-200">
          <span className="font-medium">Escalation cooldown (hours)</span>
          <input
            id="procurement-exception-escalation-cooldown-hours"
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="number"
            min={1}
            max={168}
            value={escalationCooldownHours}
            onChange={(event) => setEscalationCooldownHours(Number(event.target.value))}
            data-testid="procurement-exception-escalation-cooldown-hours"
          />
        </label>

        <label htmlFor="procurement-exception-escalation-max-escalations" className="block text-sm text-slate-200">
          <span className="font-medium">Max escalations per exception</span>
          <input
            id="procurement-exception-escalation-max-escalations"
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="number"
            min={1}
            max={50}
            value={maxEscalationsPerException}
            onChange={(event) => setMaxEscalationsPerException(Number(event.target.value))}
            data-testid="procurement-exception-escalation-max-escalations"
          />
        </label>

        <label htmlFor="procurement-exception-escalation-notify" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="procurement-exception-escalation-notify"
            type="checkbox"
            checked={notifyOnProcurementExceptionSlaEscalation}
            onChange={(event) => setNotifyOnProcurementExceptionSlaEscalation(event.target.checked)}
            data-testid="procurement-exception-escalation-notify"
          />
          Notify on SLA escalation (requires notification webhook)
        </label>

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="procurement-exception-escalation-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save escalation settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save escalation settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Due for escalation</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-500">Loading pending preview…</p>
        )}
        {pendingQuery.isError && (
          <ApiErrorCallout
            className="mt-2"
            title="Pending preview unavailable"
            message={getErrorMessage(
              pendingQuery.error,
              'Failed to load pending escalation preview.',
            )}
            retryLabel="Retry pending preview"
            onRetry={() => {
              void pendingQuery.refetch()
            }}
          />
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p
            className="mt-2 text-sm text-slate-500"
            data-testid="procurement-exception-escalation-pending-empty"
          >
            No exceptions currently due for SLA escalation.
          </p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="procurement-exception-escalation-pending-list"
          >
            {pendingQuery.data.items.map((item) => (
              <li
                key={item.procurementExceptionId}
                className="px-3 py-2 text-slate-300"
                data-testid={`procurement-exception-escalation-pending-${item.exceptionKey}`}
              >
                <div className="font-medium text-slate-100">
                  {item.exceptionKey} · {item.title}
                </div>
                <div className="text-xs text-slate-500">
                  {Math.round(item.hoursOverdue)}h overdue · {item.escalationCount} escalations sent
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent runs</h3>
        {runsQuery.isError && (
          <ApiErrorCallout
            className="mt-2"
            title="Run history unavailable"
            message={getErrorMessage(
              runsQuery.error,
              'Failed to load escalation worker run history.',
            )}
            retryLabel="Retry run history"
            onRetry={() => {
              void runsQuery.refetch()
            }}
          />
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500" data-testid="procurement-exception-escalation-runs-empty">
            No worker runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="procurement-exception-escalation-runs-list"
          >
            {runsQuery.data.items.map((run) => (
              <li
                key={run.runId}
                className="px-3 py-2 text-slate-300"
                data-testid={`procurement-exception-escalation-run-${run.runId}`}
              >
                <span data-testid="procurement-exception-escalation-run-summary">
                  {run.escalatedCount} escalated / {run.candidatesFound} candidates
                  {run.skippedCount > 0 ? ` · ${run.skippedCount} skipped` : ''}
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent escalation events</h3>
        {eventsQuery.isError && (
          <ApiErrorCallout
            className="mt-2"
            title="Escalation events unavailable"
            message={getErrorMessage(
              eventsQuery.error,
              'Failed to load recent escalation events.',
            )}
            retryLabel="Retry escalation events"
            onRetry={() => {
              void eventsQuery.refetch()
            }}
          />
        )}
        {eventsQuery.data && eventsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500" data-testid="procurement-exception-escalation-events-empty">
            No escalation events yet.
          </p>
        )}
        {eventsQuery.data && eventsQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm"
            data-testid="procurement-exception-escalation-events-list"
          >
            {eventsQuery.data.items.map((event) => (
              <li
                key={event.eventId}
                className="px-3 py-2 text-slate-300"
                data-testid={`procurement-exception-escalation-event-${event.exceptionKey}`}
              >
                <div className="font-medium text-slate-100">{event.exceptionKey}</div>
                <div className="text-xs text-slate-500">
                  Level {event.escalationLevel} · {event.actionKind}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
