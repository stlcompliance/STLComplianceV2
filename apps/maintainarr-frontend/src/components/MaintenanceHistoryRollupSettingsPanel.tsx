import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getMaintenanceHistoryRollupRuns,
  getMaintenanceHistoryRollupSettings,
  getPendingMaintenanceHistoryRollups,
  upsertMaintenanceHistoryRollupSettings,
} from '../api/client'

interface MaintenanceHistoryRollupSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function MaintenanceHistoryRollupSettingsPanel({
  accessToken,
  canManage,
}: MaintenanceHistoryRollupSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState(1)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-maintenance-history-rollup-settings', accessToken],
    queryFn: () => getMaintenanceHistoryRollupSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['maintainarr-maintenance-history-rollup-pending', accessToken],
    queryFn: () => getPendingMaintenanceHistoryRollups(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['maintainarr-maintenance-history-rollup-runs', accessToken],
    queryFn: () => getMaintenanceHistoryRollupRuns(accessToken, 5),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setStalenessHours(data.stalenessHours)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertMaintenanceHistoryRollupSettings(accessToken, {
        isEnabled,
        stalenessHours,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({
        queryKey: ['maintainarr-maintenance-history-rollup-settings', accessToken],
      })
      void queryClient.invalidateQueries({
        queryKey: ['maintainarr-maintenance-history-rollup-pending', accessToken],
      })
      void queryClient.invalidateQueries({
        queryKey: ['maintainarr-maintenance-history-rollup-runs', accessToken],
      })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-maintenance-history-summary'] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-maintenance-history'] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="maintenance-history-rollup-settings-panel"
    >
      <h2 className="text-lg font-semibold text-foreground">Maintenance history rollup worker</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Materialize per-asset maintenance timelines from inspections, defects, work orders, and PM
        events so history reads stay fast and consistent.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load maintenance history rollup settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm">
          <input id="maintenancehistoryrollupsettings"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable maintenance history rollup worker
        </label>

        <label className="block text-sm" htmlFor="maintenancehistoryrollupsettings-staleness-window-hours">
          <span>Staleness window (hours)</span>
          <input id="maintenancehistoryrollupsettings-staleness-window-hours"
            className="mt-1 w-full max-w-xs rounded-md border border-input bg-background px-3 py-2"
            type="number"
            min={1}
            max={168}
            value={stalenessHours}
            onChange={(event) => setStalenessHours(Number(event.target.value))}
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="maintenance-history-rollup-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save rollup settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-destructive">Failed to save maintenance history rollup settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Pending asset refreshes</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p
            className="mt-2 text-sm text-muted-foreground"
            data-testid="maintenance-history-rollup-pending-empty"
          >
            No assets currently due for history rollup refresh.
          </p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="maintenance-history-rollup-pending-list"
          >
            {pendingQuery.data.items.map((item) => (
              <li key={item.assetId} className="px-3 py-2">
                <div className="font-medium">
                  {item.assetTag} — {item.assetName}
                </div>
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
          <p
            className="mt-2 text-sm text-muted-foreground"
            data-testid="maintenance-history-rollup-runs-empty"
          >
            No worker runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="maintenance-history-rollup-runs-list"
          >
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2">
                {run.refreshedCount} refreshed / {run.candidatesFound} candidates
                {run.skippedCount > 0 ? ` · ${run.skippedCount} skipped` : ''}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
