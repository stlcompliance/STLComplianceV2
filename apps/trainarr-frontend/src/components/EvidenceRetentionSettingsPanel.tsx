import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getEvidenceRetentionRuns,
  getEvidenceRetentionSettings,
  upsertEvidenceRetentionSettings,
} from '../api/client'

interface EvidenceRetentionSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) {
    return `${bytes} B`
  }
  if (bytes < 1024 * 1024) {
    return `${(bytes / 1024).toFixed(1)} KB`
  }
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export function EvidenceRetentionSettingsPanel({ accessToken, canManage }: EvidenceRetentionSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [retentionDays, setRetentionDays] = useState('365')

  const settingsQuery = useQuery({
    queryKey: ['trainarr-evidence-retention-settings', accessToken],
    queryFn: () => getEvidenceRetentionSettings(accessToken),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['trainarr-evidence-retention-runs', accessToken],
    queryFn: () => getEvidenceRetentionRuns(accessToken, 8),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setRetentionDays(String(data.retentionDaysAfterAssignmentClose))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertEvidenceRetentionSettings(accessToken, {
        isEnabled,
        retentionDaysAfterAssignmentClose: Number.parseInt(retentionDays, 10) || 365,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evidence-retention-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-evidence-retention-runs', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="evidence-retention-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">Training evidence retention</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Purge assignment evidence files after closed assignments exceed the configured retention window.
        Storage cleanup runs on a schedule via the shared worker.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-destructive">Failed to load evidence retention settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="evidence-retention-enabled"
          />
          Enable scheduled evidence retention purges
        </label>

        <label className="block text-sm">
          <span className="font-medium">Retention after assignment close (days)</span>
          <input
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={30}
            max={3650}
            value={retentionDays}
            onChange={(event) => setRetentionDays(event.target.value)}
            data-testid="evidence-retention-days"
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="evidence-retention-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save retention settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-destructive">Failed to save evidence retention settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent retention runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading retention runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="evidence-retention-runs-empty">
            No retention runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="evidence-retention-runs-list">
            {runsQuery.data.items.map((item) => (
              <li key={item.runId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.outcome}</span>
                  <span className="text-muted-foreground">{item.processedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  purged {item.evidencePurgedCount}
                  {item.bytesReclaimed > 0 ? ` · ${formatBytes(item.bytesReclaimed)} reclaimed` : ''}
                  {item.skippedCount > 0 ? ` · ${item.skippedCount} skipped` : ''}
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
