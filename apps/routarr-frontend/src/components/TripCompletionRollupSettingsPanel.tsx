import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getPendingTripCompletionRollups,
  getTripCompletionRollupRuns,
  getTripCompletionRollupSettings,
  upsertTripCompletionRollupSettings,
} from '../api/client'

interface TripCompletionRollupSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function TripCompletionRollupSettingsPanel({
  accessToken,
  canManage,
}: TripCompletionRollupSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState(1)

  const settingsQuery = useQuery({
    queryKey: ['routarr-trip-completion-rollup-settings', accessToken],
    queryFn: () => getTripCompletionRollupSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['routarr-trip-completion-rollup-pending', accessToken],
    queryFn: () => getPendingTripCompletionRollups(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['routarr-trip-completion-rollup-runs', accessToken],
    queryFn: () => getTripCompletionRollupRuns(accessToken, 5),
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
      upsertTripCompletionRollupSettings(accessToken, {
        isEnabled,
        stalenessHours,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['routarr-trip-completion-rollup-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-trip-completion-rollup-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-trip-completion-rollup-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-trip-completions', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm mt-6"
      data-testid="trip-completion-rollup-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Trip completion rollup worker</h2>
      <p className="mt-1 text-sm text-slate-400">
        Materialize trip and route completion summaries for completed or cancelled trips. The shared worker refreshes
        stale rollups on a schedule.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load trip completion rollup settings.</p>
      )}

      <div className="mt-4 space-y-4">
        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="trip-completion-rollup-enabled"
          />
          Enable scheduled trip completion rollups
        </label>

        <label className="block text-sm text-slate-200">
          Staleness window (hours)
          <input
            type="number"
            min={1}
            max={168}
            value={stalenessHours}
            onChange={(event) => setStalenessHours(Number(event.target.value))}
            className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
            data-testid="trip-completion-rollup-staleness"
          />
        </label>

        <button
          type="button"
          onClick={() => saveMutation.mutate()}
          disabled={saveMutation.isPending}
          className="rounded bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          data-testid="trip-completion-rollup-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save settings'}
        </button>
      </div>

      {isEnabled && pendingQuery.data && (
        <div className="mt-4 rounded border border-slate-700 bg-slate-950/60 p-3" data-testid="trip-completion-rollup-pending">
          <p className="text-sm font-medium text-slate-200">
            Pending rollups: {pendingQuery.data.items.length}
          </p>
          {pendingQuery.data.items.length > 0 && (
            <ul className="mt-2 space-y-1 text-xs text-slate-400">
              {pendingQuery.data.items.slice(0, 5).map((item) => (
                <li key={item.tripId}>
                  {item.tripNumber} — {item.title} ({item.dispatchStatus})
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent worker runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-400">Loading worker runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-400" data-testid="trip-completion-rollup-runs-empty">
            No worker runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-slate-700 rounded border border-slate-700 text-sm"
            data-testid="trip-completion-rollup-runs-list"
          >
            {runsQuery.data.items.map((run) => (
              <li key={run.runId} className="px-3 py-2 text-xs text-slate-400">
                {run.refreshedCount}/{run.candidatesFound} refreshed, {run.skippedCount} skipped
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
