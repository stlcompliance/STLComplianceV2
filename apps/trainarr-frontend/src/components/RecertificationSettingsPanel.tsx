import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  getRecertificationAssignmentRuns,
  getRecertificationSettings,
  upsertRecertificationSettings,
} from '../api/client'

interface RecertificationSettingsPanelProps {
  accessToken: string
  canManage: boolean
}

export function RecertificationSettingsPanel({ accessToken, canManage }: RecertificationSettingsPanelProps) {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [leadDays, setLeadDays] = useState('30')

  const settingsQuery = useQuery({
    queryKey: ['trainarr-recertification-settings', accessToken],
    queryFn: () => getRecertificationSettings(accessToken),
    enabled: canManage,
  })

  const runsQuery = useQuery({
    queryKey: ['trainarr-recertification-runs', accessToken],
    queryFn: () => getRecertificationAssignmentRuns(accessToken, 8),
    enabled: canManage,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setLeadDays(String(data.leadDays))
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      upsertRecertificationSettings(accessToken, {
        isEnabled,
        leadDays: Number.parseInt(leadDays, 10) || 30,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['trainarr-recertification-settings', accessToken] })
      void queryClient.invalidateQueries({ queryKey: ['trainarr-recertification-runs', accessToken] })
    },
  })

  if (!canManage) {
    return null
  }

  return (
    <section className="rounded-lg border border-border bg-card p-4 shadow-sm" data-testid="recertification-settings-panel">
      <h2 className="text-lg font-semibold text-foreground">Recertification assignments</h2>
      <p className="mt-1 text-sm text-muted-foreground">
        Automatically create training assignments when issued qualifications enter the recertification lead window.
        A scheduled worker scans expiring qualifications and assigns recertification training.
      </p>

      {settingsQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Recertification settings unavailable"
            message={getErrorMessage(settingsQuery.error, 'Failed to load recertification settings.')}
            retryLabel="Retry settings"
            onRetry={() => {
              void settingsQuery.refetch()
            }}
          />
        </div>
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="recertification-enabled" className="flex items-center gap-2 text-sm">
          <input
            id="recertification-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="recertification-enabled"
          />
          Enable automatic recertification assignments
        </label>

        <label htmlFor="recertification-lead-days" className="block text-sm">
          <span className="font-medium">Lead window (days before expiry)</span>
          <input
            id="recertification-lead-days"
            className="mt-1 w-32 rounded-md border border-input bg-background px-3 py-2 text-sm"
            type="number"
            min={1}
            max={365}
            value={leadDays}
            onChange={(event) => setLeadDays(event.target.value)}
            data-testid="recertification-lead-days"
          />
        </label>

        <button
          type="button"
          className="rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="recertification-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save recertification settings'}
        </button>

        {saveMutation.isError && (
          <ApiErrorCallout
            title="Save failed"
            message={getErrorMessage(saveMutation.error, 'Failed to save recertification settings.')}
          />
        )}
      </div>

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-foreground">Recent worker runs</h3>
        {runsQuery.isLoading && (
          <p className="mt-2 text-sm text-muted-foreground">Loading assignment runs…</p>
        )}
        {runsQuery.data && runsQuery.data.items.length === 0 && (
          <p className="mt-2 text-sm text-muted-foreground" data-testid="recertification-runs-empty">
            No recertification assignment runs yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 divide-y divide-border rounded-md border border-border text-sm" data-testid="recertification-runs-list">
            {runsQuery.data.items.map((item) => (
              <li key={item.runId} className="px-3 py-2">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{item.outcome}</span>
                  <span className="text-muted-foreground">{item.processedAt}</span>
                </div>
                <div className="text-xs text-muted-foreground">
                  Qualification {item.qualificationIssueId.slice(0, 8)}
                  {item.trainingAssignmentId ? ` · Assignment ${item.trainingAssignmentId.slice(0, 8)}` : ''}
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
