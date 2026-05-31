import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getOrphanReferenceFindings,
  getOrphanReferenceRuns,
  getOrphanReferenceSettings,
  upsertOrphanReferenceSettings,
} from '../api/client'

interface OrphanReferenceSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

function formatReferenceKind(kind: string): string {
  switch (kind) {
    case 'staffarr_person':
      return 'StaffArr person'
    case 'compliancecore_citation':
      return 'Compliance Core citation'
    case 'compliancecore_rule_pack':
      return 'Compliance Core rule pack'
    default:
      return kind
  }
}

export function OrphanReferenceSettingsPanel({ accessToken, canManage }: OrphanReferenceSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [stalenessHours, setStalenessHours] = useState('24')

  const settingsQuery = useQuery({
    queryKey: ['trainarr-orphan-reference-settings', accessToken],
    queryFn: () => getOrphanReferenceSettings(accessToken),
    enabled: canManage,
  })

  const findingsQuery = useQuery({
    queryKey: ['trainarr-orphan-reference-findings', accessToken],
    queryFn: () => getOrphanReferenceFindings(accessToken, 8),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['trainarr-orphan-reference-runs', accessToken],
    queryFn: () => getOrphanReferenceRuns(accessToken, 8),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setStalenessHours(String(data.scanStalenessHours))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertOrphanReferenceSettings(accessToken, {
        isEnabled,
        scanStalenessHours: Number.parseInt(stalenessHours, 10) || 24,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-orphan-reference-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-orphan-reference-findings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-orphan-reference-runs', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="orphan-reference-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">Cross-product orphan references</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Detect TrainArr records that reference missing StaffArr people or Compliance Core citations and rule packs.
        Scans run on a schedule via the shared worker and surface active findings for admin review.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Orphan reference settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load orphan reference settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="orphan-reference-enabled" className="flex items-center gap-2 text-sm">
          <input
            id="orphan-reference-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="orphan-reference-enabled"
          />
          Enable scheduled orphan reference scans
        </label>

        <label htmlFor="orphan-reference-staleness-hours" className="block text-sm">
          <span className="font-medium">Rescan interval (hours)</span>
          <input
            id="orphan-reference-staleness-hours"
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={1}
            max={168}
            value={stalenessHours}
            onChange={(event) => setStalenessHours(event.target.value)}
            data-testid="orphan-reference-staleness-hours"
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="orphan-reference-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save orphan reference settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save orphan reference settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Active orphan findings</h3>
        {findingsQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading orphan findings…</p>
        )}
        {findingsQuery.data && findingsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="orphan-reference-findings-empty">
            No active orphan references detected.
          </p>
        )}
        {findingsQuery.data && findingsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="orphan-reference-findings-list">
            {findingsQuery.data.items.map((item) => (
              <li key={item.findingId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{formatReferenceKind(item.referenceKind)}</span>
                  <span className="text-muted-foreground">{item.lastDetectedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  key {item.referenceKey}
                  {` · ${item.affectedSourceCount} source${item.affectedSourceCount === 1 ? '' : 's'}`}
                  {` · sample ${item.sampleSourceEntityType}`}
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent scan runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading scan runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="orphan-reference-runs-empty">
            No orphan reference scans yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="orphan-reference-runs-list">
            {runsQuery.data.items.map((item) => (
              <li key={item.runId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.outcome}</span>
                  <span className="text-muted-foreground">{item.processedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  checked {item.referencesCheckedCount}
                  {` · findings ${item.findingsDetectedCount}`}
                  {item.findingsResolvedCount > 0 ? ` · resolved ${item.findingsResolvedCount}` : ''}
                  {item.skippedCount > 0 ? ` · skipped ${item.skippedCount}` : ''}
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

