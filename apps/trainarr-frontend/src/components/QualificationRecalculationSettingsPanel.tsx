import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getQualificationRecalculationRuns,
  getQualificationRecalculationSettings,
  getQualificationRecalculationStates,
  upsertQualificationRecalculationSettings,
} from '../api/client'

interface QualificationRecalculationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function QualificationRecalculationSettingsPanel({ accessToken, canManage }: QualificationRecalculationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState('24')
  const [autoSuspendOnBlock, setAutoSuspendOnBlock] = useState(false)

  const settingsQuery = useQuery({
    queryKey: ['trainarr-qualification-recalculation-settings', accessToken],
    queryFn: () => getQualificationRecalculationSettings(accessToken),
    enabled: canManage,
  })

  const statesQuery = useQuery({
    queryKey: ['trainarr-qualification-recalculation-states', accessToken],
    queryFn: () => getQualificationRecalculationStates(accessToken, 8),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['trainarr-qualification-recalculation-runs', accessToken],
    queryFn: () => getQualificationRecalculationRuns(accessToken, 8),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setStalenessHours(String(data.stalenessHours))
    setAutoSuspendOnBlock(data.autoSuspendOnBlock)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertQualificationRecalculationSettings(accessToken, {
        isEnabled,
        stalenessHours: Number.parseInt(stalenessHours, 10) || 24,
        autoSuspendOnBlock,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-recalculation-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-recalculation-states', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-qualification-recalculation-runs', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="qualification-recalculation-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">Qualification recalculation</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Periodically re-evaluate active qualifications against TrainArr local state and linked Compliance Core rule packs.
        Materialized outcomes power admin visibility and optional auto-suspend when compliance blocks.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load qualification recalculation settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="qualification-recalculation-enabled" className="flex items-center gap-2 text-sm">
          <input
            id="qualification-recalculation-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="qualification-recalculation-enabled"
          />
          Enable scheduled qualification recalculation
        </label>

        <label htmlFor="qualification-recalculation-staleness-hours" className="block text-sm">
          <span className="font-medium">Staleness window (hours)</span>
          <input
            id="qualification-recalculation-staleness-hours"
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={1}
            max={168}
            value={stalenessHours}
            onChange={(event) => setStalenessHours(event.target.value)}
            data-testid="qualification-recalculation-staleness-hours"
          />
        </label>

        <label htmlFor="qualification-recalculation-auto-suspend" className="flex items-center gap-2 text-sm">
          <input
            id="qualification-recalculation-auto-suspend"
            type="checkbox"
            checked={autoSuspendOnBlock}
            onChange={(event) => setAutoSuspendOnBlock(event.target.checked)}
            data-testid="qualification-recalculation-auto-suspend"
          />
          Auto-suspend issued qualifications when compliance recheck blocks
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="qualification-recalculation-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save recalculation settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-destructive">Failed to save qualification recalculation settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent materialized states</h3>
        {statesQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading recalculation states…</p>
        )}
        {statesQuery.data && statesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="qualification-recalculation-states-empty">
            No recalculation states yet.
          </p>
        )}
        {statesQuery.data && statesQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="qualification-recalculation-states-list">
            {statesQuery.data.items.map((item) => (
              <li key={`${item.qualificationIssueId}-${item.computedAt}`} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.outcome}</span>
                  <span className="text-muted-foreground">{item.computedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  Qualification {item.qualificationIssueId.slice(0, 8)} · {item.reasonCode}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent worker runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading recalculation runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="qualification-recalculation-runs-empty">
            No recalculation runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="qualification-recalculation-runs-list">
            {runsQuery.data.items.map((item) => (
              <li key={item.runId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.outcome}</span>
                  <span className="text-muted-foreground">{item.processedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  Qualification {item.qualificationIssueId.slice(0, 8)}
                  {item.checkOutcome ? ` · check ${item.checkOutcome}` : ''}
                  {item.skipReason ? ` · ${item.skipReason}` : ''}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
