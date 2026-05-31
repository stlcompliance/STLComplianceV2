import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'

export function EntitlementReconciliationSettingsPanel() {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [autoGrantFromLicense, setAutoGrantFromLicense] = useState(true)
  const [autoRevokeStaleEntitlements, setAutoRevokeStaleEntitlements] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['platform-entitlement-reconciliation-settings'],
    queryFn: () => nexarr.getEntitlementReconciliationSettings(),
  })

  const runsQuery = useQuery({
    queryKey: ['platform-entitlement-reconciliation-runs'],
    queryFn: () => nexarr.getEntitlementReconciliationRuns(8),
  })

  const pendingQuery = useQuery({
    queryKey: ['platform-entitlement-reconciliation-pending'],
    queryFn: () => nexarr.getEntitlementReconciliationPending(10),
    enabled: settingsQuery.data?.isEnabled === true,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setAutoGrantFromLicense(data.autoGrantFromLicense)
    setAutoRevokeStaleEntitlements(data.autoRevokeStaleEntitlements)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertEntitlementReconciliationSettings({
        isEnabled,
        autoGrantFromLicense,
        autoRevokeStaleEntitlements,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-entitlement-reconciliation-settings'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-entitlement-reconciliation-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-entitlement-reconciliation-pending'] })
    },
  })

  return (
    <section
      className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
      data-testid="entitlement-reconciliation-settings-panel"
    >
      <h2 className="text-lg font-semibold text-white">Entitlement reconciliation</h2>
      <p className="mt-1 text-sm text-slate-400">
        Align tenant product entitlements with subscription/licensing records. The shared worker
        scans for drift and applies grants or revokes based on these settings.
      </p>

      {settingsQuery.isError && (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(
            settingsQuery.error,
            'Failed to load entitlement reconciliation settings.',
          )}
          onRetry={() => void settingsQuery.refetch()}
          retryLabel="Retry settings"
        />
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="entitlement-reconciliation-enabled" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="entitlement-reconciliation-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="entitlement-reconciliation-enabled"
          />
          Enable scheduled entitlement reconciliation
        </label>

        <label htmlFor="entitlement-reconciliation-auto-grant" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="entitlement-reconciliation-auto-grant"
            type="checkbox"
            checked={autoGrantFromLicense}
            onChange={(event) => setAutoGrantFromLicense(event.target.checked)}
            data-testid="entitlement-reconciliation-auto-grant"
          />
          Auto-grant entitlements when a valid license exists
        </label>

        <label htmlFor="entitlement-reconciliation-auto-revoke" className="flex items-center gap-2 text-sm text-slate-200">
          <input
            id="entitlement-reconciliation-auto-revoke"
            type="checkbox"
            checked={autoRevokeStaleEntitlements}
            onChange={(event) => setAutoRevokeStaleEntitlements(event.target.checked)}
            data-testid="entitlement-reconciliation-auto-revoke"
          />
          Auto-revoke entitlements when license is missing, expired, or tenant is suspended
        </label>

        <button
          type="button"
          className="rounded-md bg-stl-teal px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
          disabled={saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          data-testid="entitlement-reconciliation-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save settings'}
        </button>
        {saveMutation.isError && (
          <ApiErrorCallout
            message={getErrorMessage(
              saveMutation.error,
              'Failed to save entitlement reconciliation settings.',
            )}
          />
        )}
      </div>

      {isEnabled && (
        <div className="mt-6">
          <h3 className="text-sm font-semibold text-white">Pending drift (preview)</h3>
          {pendingQuery.isLoading && <p className="mt-2 text-sm text-slate-400">Loading pending drift…</p>}
          {pendingQuery.isError && (
            <ApiErrorCallout
              className="mt-2"
              message={getErrorMessage(pendingQuery.error, 'Failed to load pending entitlement drift.')}
              onRetry={() => void pendingQuery.refetch()}
              retryLabel="Retry pending drift"
            />
          )}
          {pendingQuery.data?.items.length === 0 && (
            <p className="mt-2 text-sm text-slate-400" data-testid="entitlement-reconciliation-pending-empty">
              No entitlement drift detected.
            </p>
          )}
          {pendingQuery.data && pendingQuery.data.items.length > 0 && (
            <ul className="mt-2 space-y-2" data-testid="entitlement-reconciliation-pending-list">
              {pendingQuery.data.items.map((item) => (
                <li
                  key={`${item.tenantId}-${item.productKey}`}
                  className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-300"
                >
                  <span className="font-medium text-white">{item.tenantDisplayName}</span>
                  {' · '}
                  {item.productDisplayName}
                  {' — '}
                  {item.driftKind.replace(/_/g, ' ')}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-white">Recent reconciliation runs</h3>
        {runsQuery.isLoading && <p className="mt-2 text-sm text-slate-400">Loading runs…</p>}
        {runsQuery.isError && (
          <ApiErrorCallout
            className="mt-2"
            message={getErrorMessage(runsQuery.error, 'Failed to load reconciliation run history.')}
            onRetry={() => void runsQuery.refetch()}
            retryLabel="Retry runs"
          />
        )}
        {runsQuery.data?.items.length === 0 && (
          <p className="mt-2 text-sm text-slate-400" data-testid="entitlement-reconciliation-runs-empty">
            No reconciliation runs recorded yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 space-y-2" data-testid="entitlement-reconciliation-runs-list">
            {runsQuery.data.items.map((run) => (
              <li
                key={run.runId}
                className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-300"
              >
                <span className="font-medium text-white">{run.outcome}</span>
                {' — '}
                drift {run.driftFoundCount}, granted {run.grantedCount}, revoked {run.revokedCount}
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
