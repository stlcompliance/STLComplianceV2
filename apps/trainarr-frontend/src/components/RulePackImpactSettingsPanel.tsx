import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getRulePackImpactRuns,
  getRulePackImpactSettings,
  getRulePackImpactStates,
  upsertRulePackImpactSettings,
} from '../api/client'

interface RulePackImpactSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function RulePackImpactSettingsPanel({ accessToken, canManage }: RulePackImpactSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState('24')
  const [autoUpdateRequirementBaselines, setAutoUpdateRequirementBaselines] = useState(false)

  const settingsQuery = useQuery({
    queryKey: ['trainarr-rule-pack-impact-settings', accessToken],
    queryFn: () => getRulePackImpactSettings(accessToken),
    enabled: canManage,
  })

  const statesQuery = useQuery({
    queryKey: ['trainarr-rule-pack-impact-states', accessToken],
    queryFn: () => getRulePackImpactStates(accessToken, 8),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['trainarr-rule-pack-impact-runs', accessToken],
    queryFn: () => getRulePackImpactRuns(accessToken, 8),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setStalenessHours(String(data.stalenessHours))
    setAutoUpdateRequirementBaselines(data.autoUpdateRequirementBaselines)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertRulePackImpactSettings(accessToken, {
        isEnabled,
        stalenessHours: Number.parseInt(stalenessHours, 10) || 24,
        autoUpdateRequirementBaselines,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-rule-pack-impact-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-rule-pack-impact-states', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-rule-pack-impact-runs', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="rule-pack-impact-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">Rule pack impact scanning</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Periodically assess linked Compliance Core rule packs for version or status drift against training requirement baselines.
        Materialized outcomes surface affected definitions, assignments, and qualifications for admin review.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load rule pack impact settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="rule-pack-impact-enabled" className="flex items-center gap-2 text-sm">
          <input
            id="rule-pack-impact-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="rule-pack-impact-enabled"
          />
          Enable scheduled rule pack impact scans
        </label>

        <label htmlFor="rule-pack-impact-staleness-hours" className="block text-sm">
          <span className="font-medium">Staleness window (hours)</span>
          <input
            id="rule-pack-impact-staleness-hours"
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={1}
            max={168}
            value={stalenessHours}
            onChange={(event) => setStalenessHours(event.target.value)}
            data-testid="rule-pack-impact-staleness-hours"
          />
        </label>

        <label htmlFor="rule-pack-impact-auto-update-baselines" className="flex items-center gap-2 text-sm">
          <input
            id="rule-pack-impact-auto-update-baselines"
            type="checkbox"
            checked={autoUpdateRequirementBaselines}
            onChange={(event) => setAutoUpdateRequirementBaselines(event.target.checked)}
            data-testid="rule-pack-impact-auto-update-baselines"
          />
          Auto-update requirement baselines when scans find no attention required
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="rule-pack-impact-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save impact scan settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-destructive">Failed to save rule pack impact settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent materialized states</h3>
        {statesQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading impact states…</p>
        )}
        {statesQuery.data && statesQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="rule-pack-impact-states-empty">
            No impact states yet.
          </p>
        )}
        {statesQuery.data && statesQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="rule-pack-impact-states-list">
            {statesQuery.data.items.map((item) => (
              <li key={`${item.rulePackKey}-${item.computedAt}`} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.rulePackKey}</span>
                  <span className="text-muted-foreground">{item.computedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  {item.requiresAttention ? 'attention required' : 'reviewed'}
                  {item.hasDrift ? ' · drift detected' : ''}
                  {item.triggers.length > 0 ? ` · ${item.triggers.join(', ')}` : ''}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent worker runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading impact runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="rule-pack-impact-runs-empty">
            No impact runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="rule-pack-impact-runs-list">
            {runsQuery.data.items.map((item) => (
              <li key={item.runId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.outcome}</span>
                  <span className="text-muted-foreground">{item.processedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  {item.rulePackKey}
                  {item.requiresAttention ? ' · attention required' : ''}
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
