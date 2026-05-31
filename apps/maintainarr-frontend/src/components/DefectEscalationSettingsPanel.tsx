import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getDefectEscalationEvents,
  getDefectEscalationRuns,
  getDefectEscalationSettings,
  getPendingDefectEscalations,
  upsertDefectEscalationSettings,
} from '../api/client'

interface DefectEscalationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function DefectEscalationSettingsPanel({
  accessToken,
  canManage,
}: DefectEscalationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [lowThresholdHours, setLowThresholdHours] = useState(168)
  const [mediumThresholdHours, setMediumThresholdHours] = useState(72)
  const [highThresholdHours, setHighThresholdHours] = useState(24)
  const [criticalThresholdHours, setCriticalThresholdHours] = useState(8)
  const [autoAcknowledge, setAutoAcknowledge] = useState(true)
  const [autoCreateWorkOrder, setAutoCreateWorkOrder] = useState(true)
  const [bumpSeverity, setBumpSeverity] = useState(true)
  const [notifyOnEscalation, setNotifyOnEscalation] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-defect-escalation-settings', accessToken],
    queryFn: () => getDefectEscalationSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['maintainarr-defect-escalation-pending', accessToken],
    queryFn: () => getPendingDefectEscalations(accessToken),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['maintainarr-defect-escalation-runs', accessToken],
    queryFn: () => getDefectEscalationRuns(accessToken, 5),
    enabled: canManage,
  })

  const eventsQuery = useQuery({
    queryKey: ['maintainarr-defect-escalation-events', accessToken],
    queryFn: () => getDefectEscalationEvents(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setLowThresholdHours(data.lowThresholdHours)
    setMediumThresholdHours(data.mediumThresholdHours)
    setHighThresholdHours(data.highThresholdHours)
    setCriticalThresholdHours(data.criticalThresholdHours)
    setAutoAcknowledge(data.autoAcknowledgeOnEscalation)
    setAutoCreateWorkOrder(data.autoCreateWorkOrderOnEscalation)
    setBumpSeverity(data.bumpSeverityOnRepeatEscalation)
    setNotifyOnEscalation(data.notifyOnEscalation)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertDefectEscalationSettings(accessToken, {
        isEnabled,
        lowThresholdHours,
        mediumThresholdHours,
        highThresholdHours,
        criticalThresholdHours,
        autoAcknowledgeOnEscalation: autoAcknowledge,
        autoCreateWorkOrderOnEscalation: autoCreateWorkOrder,
        bumpSeverityOnRepeatEscalation: bumpSeverity,
        notifyOnEscalation,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-defect-escalation-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-defect-escalation-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-defect-escalation-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-defect-escalation-events', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="defect-escalation-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">Defect escalation worker</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Escalate stagnant open defects on a schedule — acknowledge, create work orders, bump severity,
        and enqueue webhook notifications.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Escalation settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load defect escalation settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm">
          <input id="defectescalationsettings-5"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable defect escalation worker
        </label>

        <fieldset className="grid gap-3 sm:grid-cols-2 text-sm">
          <legend className="col-span-full font-medium">Stagnation thresholds (hours)</legend>
          <label className="block">
            <span>Low severity</span>
            <input id="defectescalationsettings-low-severity"
              className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2"
              type="number"
              min={1}
              max={720}
              value={lowThresholdHours}
              onChange={(event) => setLowThresholdHours(Number(event.target.value))}
            />
          </label>
          <label className="block">
            <span>Medium severity</span>
            <input id="defectescalationsettings-medium-severity"
              className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2"
              type="number"
              min={1}
              max={720}
              value={mediumThresholdHours}
              onChange={(event) => setMediumThresholdHours(Number(event.target.value))}
            />
          </label>
          <label className="block">
            <span>High severity</span>
            <input id="defectescalationsettings-high-severity"
              className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2"
              type="number"
              min={1}
              max={720}
              value={highThresholdHours}
              onChange={(event) => setHighThresholdHours(Number(event.target.value))}
            />
          </label>
          <label className="block">
            <span>Critical severity</span>
            <input id="defectescalationsettings-critical-severity"
              className="mt-1 w-full rounded-md border border-input bg-background px-3 py-2"
              type="number"
              min={1}
              max={720}
              value={criticalThresholdHours}
              onChange={(event) => setCriticalThresholdHours(Number(event.target.value))}
            />
          </label>
        </fieldset>

        <fieldset className="space-y-2 text-sm">
          <legend className="font-medium">Escalation actions</legend>
          <label className="flex items-center gap-2">
            <input id="defectescalationsettings-4" type="checkbox" checked={autoAcknowledge} onChange={(e) => setAutoAcknowledge(e.target.checked)} />
            Auto-acknowledge open defects
          </label>
          <label className="flex items-center gap-2">
            <input id="defectescalationsettings-3" type="checkbox" checked={autoCreateWorkOrder} onChange={(e) => setAutoCreateWorkOrder(e.target.checked)} />
            Auto-create work orders when missing
          </label>
          <label className="flex items-center gap-2">
            <input id="defectescalationsettings-2" type="checkbox" checked={bumpSeverity} onChange={(e) => setBumpSeverity(e.target.checked)} />
            Bump severity on repeat escalation
          </label>
          <label className="flex items-center gap-2">
            <input id="defectescalationsettings" type="checkbox" checked={notifyOnEscalation} onChange={(e) => setNotifyOnEscalation(e.target.checked)} />
            Enqueue defect-escalated notifications
          </label>
        </fieldset>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="defect-escalation-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save escalation settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save defect escalation settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Pending escalations</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="defect-escalation-pending-empty">
            No defects currently due for escalation.
          </p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="defect-escalation-pending-list"
          >
            {pendingQuery.data.items.map((item) => (
              <li key={item.defectId} className="px-3 py-2">
                <div className="font-medium">{item.title}</div>
                <div className="text-xs text-muted-foreground">
                  {item.severity} · {item.status} · {Math.round(item.stagnationHours)}h stagnant (threshold {item.thresholdHours}h)
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-sm font-semibold text-foreground">Recent runs</h3>
          {runsQuery.isLoading && (
            <p className="mt-2 text-sm text-muted-foreground">Loading worker runs…</p>
          )}
          {runsQuery.data && runsQuery.data.items.length === 0 && (
            <p className="mt-2 text-sm text-muted-foreground" data-testid="defect-escalation-runs-empty">
              No worker runs yet.
            </p>
          )}
          {runsQuery.data && runsQuery.data.items.length > 0 && (
            <ul
              className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
              data-testid="defect-escalation-runs-list"
            >
              {runsQuery.data.items.map((run) => (
                <li key={run.runId} className="px-3 py-2">
                  {run.escalatedCount} escalated / {run.candidatesFound} candidates
                  {run.skippedCount > 0 ? ` · ${run.skippedCount} skipped` : ''}
                </li>
              ))}
            </ul>
          )}
        </div>
        <div>
          <h3 className="text-sm font-semibold text-foreground">Recent events</h3>
          {eventsQuery.isLoading && (
            <p className="mt-2 text-sm text-muted-foreground">Loading escalation events…</p>
          )}
          {eventsQuery.data && eventsQuery.data.items.length === 0 && (
            <p className="mt-2 text-sm text-muted-foreground" data-testid="defect-escalation-events-empty">
              No escalation events yet.
            </p>
          )}
          {eventsQuery.data && eventsQuery.data.items.length > 0 && (
            <ul
              className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
              data-testid="defect-escalation-events-list"
            >
              {eventsQuery.data.items.map((event) => (
                <li key={event.eventId} className="px-3 py-2">
                  <span className="font-medium">{event.actionKind}</span>
                  {event.newStatus ? ` → ${event.newStatus}` : ''}
                  {event.newSeverity ? ` · ${event.newSeverity}` : ''}
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </section>
  )
}
