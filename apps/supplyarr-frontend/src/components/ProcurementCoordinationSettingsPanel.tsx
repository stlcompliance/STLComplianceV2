import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getPendingProcurementCoordination,
  getProcurementCoordinationRuns,
  getProcurementCoordinationSettings,
  upsertProcurementCoordinationSettings,
} from '../api/client'

interface ProcurementCoordinationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function ProcurementCoordinationSettingsPanel({
  accessToken,
  canManage,
}: ProcurementCoordinationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState(2)

  const settingsQuery = useQuery({
    queryKey: ['supplyarr-procurement-coordination-settings', accessToken],
    queryFn: () => getProcurementCoordinationSettings(accessToken),
    enabled: canManage,
  })

  const pendingQuery = useQuery({
    queryKey: ['supplyarr-procurement-coordination-pending', accessToken],
    queryFn: () => getPendingProcurementCoordination(accessToken),
    enabled: canManage && isEnabled,
  })

  const runsQuery = useQuery({
    queryKey: ['supplyarr-procurement-coordination-runs', accessToken],
    queryFn: () => getProcurementCoordinationRuns(accessToken, 5),
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
      upsertProcurementCoordinationSettings(accessToken, {
        isEnabled,
        stalenessHours,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-procurement-coordination-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-procurement-coordination-pending', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-procurement-coordination-runs', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['supplyarr-procurement-coordination', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5"
      data-testid="procurement-coordination-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Procurement coordination worker</h2>
      <p className="mt-1 text-sm text-slate-400">
        Materialize PR/PO pipeline state, next actions, and milestone events for active procurement workflows.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load procurement coordination settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
          />
          Enable automated procurement coordination refresh
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
          {saveMutation.isPending ? 'Saving…' : 'Save coordination settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-rose-400">Failed to save procurement coordination settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Pending coordination refreshes</h3>
        {pendingQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-500">Loading pending preview…</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-500">No active procurement subjects currently due for refresh.</p>
        )}
        {pendingQuery.data && pendingQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
            {pendingQuery.data.items.map((item) => (
              <li key={`${item.subjectType}-${item.subjectId}`} className="px-3 py-2 text-slate-300">
                <div className="font-medium text-slate-100">
                  {item.documentKey} · {item.title}
                </div>
                <div className="text-xs text-slate-500">
                  {item.subjectType} · {item.documentStatus}
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
