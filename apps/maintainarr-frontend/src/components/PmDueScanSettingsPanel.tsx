import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getPendingPmDueScan,
  getPmDueScanRuns,
  getPmDueScanSettings,
  triggerPmDueScan,
  upsertPmDueScanSettings,
} from '../api/client'

interface PmDueScanSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function PmDueScanSettingsPanel({ accessToken, canManage }: PmDueScanSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [scanIntervalMinutes, setScanIntervalMinutes] = useState(15)
  const [batchSize, setBatchSize] = useState(100)
  const [overdueGraceDays, setOverdueGraceDays] = useState(1)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-pm-due-scan-settings', accessToken],
    queryFn: () => getPmDueScanSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['maintainarr-pm-due-scan-pending', accessToken],
    queryFn: () => getPendingPmDueScan(accessToken),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['maintainarr-pm-due-scan-runs', accessToken],
    queryFn: () => getPmDueScanRuns(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setScanIntervalMinutes(data.scanIntervalMinutes)
    setBatchSize(data.batchSize)
    setOverdueGraceDays(data.overdueGraceDays)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertPmDueScanSettings(accessToken, {
        isEnabled,
        scanIntervalMinutes,
        batchSize,
        overdueGraceDays,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due-scan-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due-scan-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due-scan-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due'] })
    },
  })

  const triggerMutation = useMutation({
    mutationFn: () => triggerPmDueScan(accessToken),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due-scan-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due-scan-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due-scan-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-pm-due'] })
    },
  })

  if (!canManage) {
    return null
  }

  const pendingCount = settingsQuery.data?.pendingPmCount ?? pendingQuery.data?.items.length ?? 0
  const lastRunLabel = settingsQuery.data?.lastRunAt
    ? new Date(settingsQuery.data.lastRunAt).toLocaleString()
    : 'Never'

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="pm-due-scan-settings-panel"
    >
      <h2 className="text-lg font-semibold text-foreground">PM due scan worker</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Mark preventive maintenance schedules due or overdue on a schedule, optionally generate
        linked work orders, and surface pending PM volume for operators.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load PM due scan settings.</p>
      )}

      <dl className="mt-4 grid gap-2 text-sm sm:grid-cols-2">
        <div>
          <dt className="text-muted-foreground">Last run</dt>
          <dd className="font-medium text-foreground" data-testid="pm-due-scan-last-run">
            {lastRunLabel}
          </dd>
        </div>
        <div>
          <dt className="text-muted-foreground">Pending PM schedules</dt>
          <dd className="font-medium text-foreground" data-testid="pm-due-scan-pending-count">
            {pendingCount}
          </dd>
        </div>
      </dl>

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable PM due scan worker
        </label>

        <label className="block text-sm">
          <span>Scan interval (minutes)</span>
          <input
            className="mt-1 w-full max-w-xs rounded-md border border-input bg-background px-3 py-2"
            type="number"
            min={1}
            max={1440}
            value={scanIntervalMinutes}
            onChange={(event) => setScanIntervalMinutes(Number(event.target.value))}
          />
        </label>

        <label className="block text-sm">
          <span>Batch size</span>
          <input
            className="mt-1 w-full max-w-xs rounded-md border border-input bg-background px-3 py-2"
            type="number"
            min={1}
            max={500}
            value={batchSize}
            onChange={(event) => setBatchSize(Number(event.target.value))}
          />
        </label>

        <label className="block text-sm">
          <span>Overdue grace (days)</span>
          <input
            className="mt-1 w-full max-w-xs rounded-md border border-input bg-background px-3 py-2"
            type="number"
            min={0}
            max={30}
            value={overdueGraceDays}
            onChange={(event) => setOverdueGraceDays(Number(event.target.value))}
          />
        </label>

        <div className="flex flex-wrap gap-2">
          <button
            type="button"
            className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
            disabled={saveMutation.isPending}
            onClick={() => saveMutation.mutate()}
            data-testid="pm-due-scan-save"
          >
            {saveMutation.isPending ? 'Saving…' : 'Save PM due scan settings'}
          </button>
          <button
            type="button"
            className="rounded-md border border-input bg-background px-4 py-2 text-sm font-medium disabled:opacity-50"
            disabled={triggerMutation.isPending}
            onClick={() => triggerMutation.mutate()}
            data-testid="pm-due-scan-trigger-button"
          >
            {triggerMutation.isPending ? 'Running…' : 'Run due scan now'}
          </button>
        </div>

        {saveMutation.isError && (
          <p className="text-sm text-destructive">Failed to save PM due scan settings.</p>
        )}
        {triggerMutation.isError && (
          <p className="text-sm text-destructive">Failed to trigger PM due scan.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Pending PM preview</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="pm-due-scan-pending-empty">
            No PM schedules currently due for scan.
          </p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="pm-due-scan-pending-list"
          >
            {pendingQuery.data.items.map((item) => (
              <li key={item.pmScheduleId} className="px-3 py-2">
                <div className="font-medium">
                  {item.assetTag} — {item.scheduleKey}
                </div>
                <p className="text-muted-foreground">
                  {item.dueStatus} · due {new Date(item.nextDueAt).toLocaleString()}
                </p>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading worker runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="pm-due-scan-runs-empty">
            No worker runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="pm-due-scan-runs-list"
          >
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2">
                {run.markedDueCount} due / {run.markedOverdueCount} overdue from {run.candidatesFound}{' '}
                candidates
                {run.workOrdersCreatedCount > 0
                  ? ` · ${run.workOrdersCreatedCount} work orders created`
                  : ''}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
