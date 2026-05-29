import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getAssetStatusRollupRuns,
  getAssetStatusRollupSettings,
  getFleetAssetStatusRollup,
  getPendingAssetStatusRollups,
  upsertAssetStatusRollupSettings,
} from '../api/client'

interface AssetStatusRollupSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function AssetStatusRollupSettingsPanel({
  accessToken,
  canManage,
}: AssetStatusRollupSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState(1)

  const settingsQuery = useQuery({
    queryKey: ['maintainarr-asset-status-rollup-settings', accessToken],
    queryFn: () => getAssetStatusRollupSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['maintainarr-asset-status-rollup-pending', accessToken],
    queryFn: () => getPendingAssetStatusRollups(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['maintainarr-asset-status-rollup-runs', accessToken],
    queryFn: () => getAssetStatusRollupRuns(accessToken, 5),
    enabled: canManage,
  })

  const fleetQuery = useQuery({
    queryKey: ['maintainarr-fleet-asset-status-rollup', accessToken],
    queryFn: () => getFleetAssetStatusRollup(accessToken),
    enabled: canManage,
    retry: false,
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
      upsertAssetStatusRollupSettings(accessToken, {
        isEnabled,
        stalenessHours,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-status-rollup-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-status-rollup-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-status-rollup-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-fleet-asset-status-rollup', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['maintainarr-asset-readiness-fleet'] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-border bg-card p-4 shadow-sm"
      data-testid="asset-status-rollup-settings-panel"
    >
      <h2 className="text-lg font-semibold text-foreground">Asset status rollup worker</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Materialize per-asset readiness and fleet/type/class/site rollups on a schedule so asset
        readiness reads stay fast and consistent.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load asset status rollup settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable asset status rollup worker
        </label>

        <label className="block text-sm">
          <span>Staleness window (hours)</span>
          <input
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
          data-testid="asset-status-rollup-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save rollup settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-destructive">Failed to save asset status rollup settings.</p>
        )}
      </div>

      {fleetQuery.data && (
        <div className="mt-6 rounded-md border border-border p-3 text-sm">
          <h3 className="font-semibold text-foreground">Fleet rollup snapshot</h3>
          <p className="mt-1 text-muted-foreground">
            {fleetQuery.data.readyCount} ready / {fleetQuery.data.notReadyCount} not ready (
            {fleetQuery.data.readyPercent}% ready across {fleetQuery.data.totalAssets} assets)
          </p>
        </div>
      )}

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Pending asset refreshes</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="asset-status-rollup-pending-empty">
            No assets currently due for rollup refresh.
          </p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="asset-status-rollup-pending-list"
          >
            {pendingQuery.data.items.map((item) => (
              <li key={item.assetId} className="px-3 py-2">
                <div className="font-medium">{item.assetTag} — {item.assetName}</div>
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
          <p className="mt-2 text-sm text-muted-foreground" data-testid="asset-status-rollup-runs-empty">
            No worker runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-border rounded-md border border-border text-sm"
            data-testid="asset-status-rollup-runs-list"
          >
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2">
                {run.refreshedCount} refreshed / {run.candidatesFound} candidates
                {run.scopeRollupsRefreshed > 0 ? ` · ${run.scopeRollupsRefreshed} scope rollups` : ''}
                {run.skippedCount > 0 ? ` · ${run.skippedCount} skipped` : ''}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
