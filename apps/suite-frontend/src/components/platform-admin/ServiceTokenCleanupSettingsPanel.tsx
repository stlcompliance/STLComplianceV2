import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import * as nexarr from '../../api/nexarrClient'

export function ServiceTokenCleanupSettingsPanel() {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [retentionDaysAfterExpiry, setRetentionDaysAfterExpiry] = useState('7')
  const [retentionDaysAfterRevoke, setRetentionDaysAfterRevoke] = useState('30')

  const settingsQuery = useQuery({
    queryKey: ['platform-service-token-cleanup-settings'],
    queryFn: () => nexarr.getServiceTokenCleanupSettings(),
  })

  const runsQuery = useQuery({
    queryKey: ['platform-service-token-cleanup-runs'],
    queryFn: () => nexarr.getServiceTokenCleanupRuns(8),
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setRetentionDaysAfterExpiry(String(data.retentionDaysAfterExpiry))
    setRetentionDaysAfterRevoke(String(data.retentionDaysAfterRevoke))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertServiceTokenCleanupSettings({
        isEnabled,
        retentionDaysAfterExpiry: Number.parseInt(retentionDaysAfterExpiry, 10) || 7,
        retentionDaysAfterRevoke: Number.parseInt(retentionDaysAfterRevoke, 10) || 30,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-cleanup-settings'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-cleanup-runs'] })
    },
  })

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
      data-testid="service-token-cleanup-settings-panel"
    >
      <h2 className="text-lg font-semibold text-white">Service token cleanup</h2>
      <p className="mt-1 text-sm text-slate-400">
        Purge expired and revoked service token records after configured grace periods. The shared worker
        calls NexArr internal cleanup APIs on a schedule.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-red-400">Failed to load service token cleanup settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="service-token-cleanup-enabled" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="service-token-cleanup-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="service-token-cleanup-enabled"
          />
          Enable scheduled service token cleanup
        </label>

        <label htmlFor="service-token-cleanup-expiry-days" className="block text-sm text-slate-200">
          Grace after token expiry (days)
          <input
            id="service-token-cleanup-expiry-days"
            className="mt-1 w-32 rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            type="number"
            min={0}
            max={365}
            value={retentionDaysAfterExpiry}
            onChange={(event) => setRetentionDaysAfterExpiry(event.target.value)}
            data-testid="service-token-cleanup-expiry-days"
          />
        </label>

        <label htmlFor="service-token-cleanup-revoke-days" className="block text-sm text-slate-200">
          Grace after token revoke (days)
          <input
            id="service-token-cleanup-revoke-days"
            className="mt-1 w-32 rounded-md border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
            type="number"
            min={0}
            max={365}
            value={retentionDaysAfterRevoke}
            onChange={(event) => setRetentionDaysAfterRevoke(event.target.value)}
            data-testid="service-token-cleanup-revoke-days"
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-stl-teal px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="service-token-cleanup-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save settings'}
        </button>
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-white">Recent cleanup runs</h3>
        {runsQuery.isLoading && <p className="mt-2 text-sm text-slate-400">Loading runs…</p>}
        {runsQuery.isError && (
          <p className="mt-2 text-sm text-red-400">Failed to load cleanup run history.</p>
        )}
        {runsQuery.data?.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-400" data-testid="service-token-cleanup-runs-empty">
            No cleanup runs recorded yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 space-y-2" data-testid="service-token-cleanup-runs-list">
            {runsQuery.data.items.map((run) => (
              <li
                key={run.runId}
                className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-300"
              >
                <span className="font-medium text-white">{run.outcome}</span>
                {' — '}
                purged {run.purgedCount} (expired {run.expiredPurgeCount}, revoked {run.revokedPurgeCount})
                {run.skippedCount > 0 ? `, skipped ${run.skippedCount}` : ''}
                {' · '}
                {new Date(run.processedAt).toLocaleString()}
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
