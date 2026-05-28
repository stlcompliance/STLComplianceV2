import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getPendingPriceSnapshotCaptures,
  getPriceSnapshotRuns,
  getPriceSnapshotSettings,
  upsertPriceSnapshotSettings,
} from '../api/client'

interface PriceSnapshotSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function PriceSnapshotSettingsPanel({ accessToken, canManage }: PriceSnapshotSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState(24)

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-price-snapshot-settings', accessToken],
    queryFn: () => getPriceSnapshotSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-price-snapshot-pending', accessToken],
    queryFn: () => getPendingPriceSnapshotCaptures(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['supplyarr-price-snapshot-runs', accessToken],
    queryFn: () => getPriceSnapshotRuns(accessToken, 5),
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
      upsertPriceSnapshotSettings(accessToken, {
        isEnabled,
        stalenessHours,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-price-snapshot-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-price-snapshot-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-price-snapshot-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-pricing-snapshots', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="price-snapshot-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Price snapshot worker</h2>
      <p className="mt-1 text-sm text-slate-400">
        Automatically capture vendor catalog prices into pricing snapshot history when catalog prices drift from the
        current effective snapshot.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load price snapshot settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable automated price snapshot capture
        </label>

        <label className="block text-sm text-slate-200">
          <span className="font-medium">Staleness window (hours)</span>
          <input
            className="mt-1 w-full max-w-xs rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
            type="number"
            min={1}
            max={168}
            value={stalenessHours}
            onChange={(event) => setStalenessHours(Number(event.target.value))}
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
        >
          {saveMutation.isPending ? 'Saving…' : 'Save price snapshot settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-rose-400">Failed to save price snapshot settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Pending catalog captures</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-500">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500">No vendor links currently due for price snapshot capture.</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {pendingQuery.data.items.map((item) => (
              <li key={item.partVendorLinkId} className="px-3 py-2 text-slate-300">
                <div className="font-medium text-slate-100">
                  {item.partKey} · {item.vendorDisplayName}
                </div>
                <div className="text-xs text-slate-500">
                  Catalog {item.catalogUnitPrice} {item.catalogCurrencyCode}
                  {item.currentUnitPrice != null
                    ? ` · current ${item.currentUnitPrice} ${item.currentCurrencyCode ?? ''}`
                    : ' · no current snapshot'}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent runs</h3>
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500">No worker runs yet.</p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2 text-slate-300">
                {run.capturedCount} captured / {run.candidatesFound} candidates
                {run.skippedCount > 0 ? ` · ${run.skippedCount} skipped` : ''}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
