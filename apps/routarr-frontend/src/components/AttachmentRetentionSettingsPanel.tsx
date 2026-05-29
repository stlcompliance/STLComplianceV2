import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'

import {
  getAttachmentRetentionRuns,
  getAttachmentRetentionSettings,
  upsertAttachmentRetentionSettings,
} from '../api/client'

interface AttachmentRetentionSettingsPanelProps {
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

export function AttachmentRetentionSettingsPanel({
  accessToken,
  canManage,
}: AttachmentRetentionSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [retentionDays, setRetentionDays] = useState('365')

  const settingsQuery = useQuery({
    queryKey: ['routarr-attachment-retention-settings', accessToken],
    queryFn: () => getAttachmentRetentionSettings(accessToken),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['routarr-attachment-retention-runs', accessToken],
    queryFn: () => getAttachmentRetentionRuns(accessToken, 8),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setRetentionDays(String(data.retentionDaysAfterTripClose))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertAttachmentRetentionSettings(accessToken, {
        isEnabled,
        retentionDaysAfterTripClose: Number.parseInt(retentionDays, 10) || 365,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['routarr-attachment-retention-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['routarr-attachment-retention-runs', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/80 p-4 shadow-sm"
      data-testid="attachment-retention-settings-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Trip capture attachment retention</h2>
      <p className="mt-1 text-sm text-slate-400">
        Purge proof/DVIR capture attachment files after completed or cancelled trips exceed the configured retention
        window. Storage cleanup runs on a schedule via the shared worker.
      </p>

      {settingsQuery.isError && (
        <p className="mt-3 text-sm text-rose-400">Failed to load attachment retention settings.</p>
      )}

      <div className="mt-4 space-y-3">
        <label className="flex items-center gap-2 text-sm text-slate-200">
          <input id="attachmentretentionsettings"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="attachment-retention-enabled"
          />
          Enable scheduled capture attachment retention purges
        </label>

        <label className="block text-sm text-slate-200" htmlFor="attachmentretentionsettings-retention-after-trip-close-days">
          <span className="font-medium">Retention after trip close (days)</span>
          <input id="attachmentretentionsettings-retention-after-trip-close-days"
            className="mt-1 w-32 rounded border border-slate-600 bg-slate-950 px-3 py-2 text-slate-100"
            type="number"
            min={30}
            max={3650}
            value={retentionDays}
            onChange={(event) => setRetentionDays(event.target.value)}
            data-testid="attachment-retention-days"
          />
        </label>

        <button
          type="button"
          className="rounded bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="attachment-retention-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save retention settings'}
        </button>

        {saveMutation.isError && (
          <p className="text-sm text-rose-400">Failed to save attachment retention settings.</p>
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-slate-200">Recent retention runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-400">Loading retention runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-400" data-testid="attachment-retention-runs-empty">
            No retention runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul
            className="mt-2 divide-y divide-slate-700 rounded border border-slate-700 text-sm"
            data-testid="attachment-retention-runs-list"
          >
            {runsQuery.data.items.map((item) => (
              <li key={item.runId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{item.outcome}</span>
                  <span className="text-slate-400">{item.processedAt}</span>
                </div>
                <div className="text-xs text-slate-400">
                  purged {item.attachmentsPurgedCount}
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
