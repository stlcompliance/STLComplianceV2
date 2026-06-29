import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getAvailabilitySnapshotRuns,
  getAvailabilitySnapshotSettings,
  getPendingAvailabilitySnapshotCaptures,
  upsertAvailabilitySnapshotSettings,
} from '../api/client'

interface AvailabilitySnapshotSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function AvailabilitySnapshotSettingsPanel({
  accessToken,
  canManage,
}: AvailabilitySnapshotSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState(24)

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-availability-snapshot-settings', accessToken],
    queryFn: () => getAvailabilitySnapshotSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-availability-snapshot-pending', accessToken],
    queryFn: () => getPendingAvailabilitySnapshotCaptures(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['supplyarr-availability-snapshot-runs', accessToken],
    queryFn: () => getAvailabilitySnapshotRuns(accessToken, 5),
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
      upsertAvailabilitySnapshotSettings(accessToken, {
        isEnabled,
        stalenessHours,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-availability-snapshot-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-availability-snapshot-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-availability-snapshot-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-availability-snapshots', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="availability-snapshot-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Availability snapshot worker</h2>
      <p className="mt-1 text-sm text-slate-400">
        Automatically capture supplier catalog availability into availability snapshot history when catalog quantity or
        status drifts from the current effective snapshot.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Availability snapshot settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load availability snapshot settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="availability-snapshot-settings-enabled" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="availability-snapshot-settings-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable automated availability snapshot capture
        </label>

        <label htmlFor="availability-snapshot-staleness-hours" className="block text-sm text-slate-200">
          <span className="font-medium">Staleness window (hours)</span>
          <input
            id="availability-snapshot-staleness-hours"
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
          {saveMutation.isPending ? 'Saving…' : 'Save availability snapshot settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save availability snapshot settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Pending catalog captures</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">No supplier links currently due for availability snapshot capture.</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {pendingQuery.data.items.map((item) => (
              <li key={item.partVendorLinkId} className="px-3 py-2 text-slate-300">
                <div className="font-medium text-slate-100">
                  {item.partKey} · {item.vendorDisplayName}
                </div>
                <div className="text-xs text-[var(--color-text-muted)]">
                  Catalog{' '}
                  {item.catalogQuantityAvailable != null ? `${item.catalogQuantityAvailable} qty` : ''}
                  {item.catalogQuantityAvailable != null && item.catalogAvailabilityStatus ? ' · ' : ''}
                  {item.catalogAvailabilityStatus ?? ''}
                  {item.currentAvailabilityStatus != null
                    ? ` · current ${item.currentAvailabilityStatus}`
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
          <p className="mt-2 text-sm text-[var(--color-text-muted)]">No worker runs yet.</p>
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
