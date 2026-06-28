import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'

export function TenantLifecycleSettingsPanel() {
  const queryClient = useQueryClient()
  const [initialized, setInitialized] = useState(false)
  const [isEnabled, setIsEnabled] = useState(false)
  const [autoSuspendWhenNoValidLicense, setAutoSuspendWhenNoValidLicense] = useState(false)
  const [suspendGraceDays, setSuspendGraceDays] = useState('7')
  const [autoReactivateWhenValidLicense, setAutoReactivateWhenValidLicense] = useState(false)
  const [revokeSessionsOnSuspend, setRevokeSessionsOnSuspend] = useState(true)

  const settingsQuery = useQuery({
    queryKey: ['platform-tenant-lifecycle-settings'],
    queryFn: () => nexarr.getTenantLifecycleSettings(),
  })

  const runsQuery = useQuery({
    queryKey: ['platform-tenant-lifecycle-runs'],
    queryFn: () => nexarr.getTenantLifecycleRuns(8),
  })

  const pendingQuery = useQuery({
    queryKey: ['platform-tenant-lifecycle-pending'],
    queryFn: () => nexarr.getTenantLifecyclePending(10),
    enabled: settingsQuery.data?.isEnabled === true,
  })

  useEffect(() => {
    if (initialized || settingsQuery.isLoading || !settingsQuery.data) {
      return
    }
    const data = settingsQuery.data
    setIsEnabled(data.isEnabled)
    setAutoSuspendWhenNoValidLicense(data.autoSuspendWhenNoValidLicense)
    setSuspendGraceDays(String(data.suspendGraceDaysAfterLastLicenseExpiry))
    setAutoReactivateWhenValidLicense(data.autoReactivateWhenValidLicense)
    setRevokeSessionsOnSuspend(data.revokeSessionsOnSuspend)
    setInitialized(true)
  }, [initialized, settingsQuery.data, settingsQuery.isLoading])

  const saveMutation = useMutation({
    mutationFn: () =>
      nexarr.upsertTenantLifecycleSettings({
        isEnabled,
        autoSuspendWhenNoValidLicense,
        suspendGraceDaysAfterLastLicenseExpiry: Number.parseInt(suspendGraceDays, 10) || 7,
        autoReactivateWhenValidLicense,
        revokeSessionsOnSuspend,
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-tenant-lifecycle-settings'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-tenant-lifecycle-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-tenant-lifecycle-pending'] })
    },
  })

  return (
    <section
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm"
      data-testid="tenant-lifecycle-settings-panel"
    >
      <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Tenant lifecycle</h2>
      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
        License-based tenant suspension is retired under the fixed-suite launch model. These legacy
        settings remain visible for audit review, but ordinary product availability no longer turns
        on product-license coverage.
      </p>
      <p className="mt-2 text-sm text-[var(--color-text-secondary)]">
        Use tenant status controls and product-local permissions for live access management. Any
        historical lifecycle runs remain listed below.
      </p>

      {settingsQuery.isError && (
        <ApiErrorCallout
          className="mt-3"
          message={getErrorMessage(settingsQuery.error, 'Failed to load tenant lifecycle settings.')}
          onRetry={() => void settingsQuery.refetch()}
          retryLabel="Retry settings"
        />
      )}

      <div className="mt-4 space-y-3">
        <label htmlFor="tenant-lifecycle-enabled" className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
          <input
            id="tenant-lifecycle-enabled"
            type="checkbox"
            checked={isEnabled}
            onChange={(event) => setIsEnabled(event.target.checked)}
            data-testid="tenant-lifecycle-enabled"
          />
          Enable scheduled tenant lifecycle processing
        </label>

        <label htmlFor="tenant-lifecycle-auto-suspend" className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
          <input
            id="tenant-lifecycle-auto-suspend"
            type="checkbox"
            checked={autoSuspendWhenNoValidLicense}
            onChange={(event) => setAutoSuspendWhenNoValidLicense(event.target.checked)}
            disabled
            data-testid="tenant-lifecycle-auto-suspend"
          />
          Legacy auto-suspend setting (retired)
        </label>

        <label htmlFor="tenant-lifecycle-grace-days" className="block text-sm text-[var(--color-text-secondary)]">
          Legacy grace days after license coverage ends
          <input
            id="tenant-lifecycle-grace-days"
            type="number"
            min={0}
            max={365}
            value={suspendGraceDays}
            onChange={(event) => setSuspendGraceDays(event.target.value)}
            disabled
            className="mt-1 block w-32 rounded border border-[var(--color-border-default)] bg-[var(--color-bg-control)] px-2 py-1 text-[var(--color-text-primary)]"
            data-testid="tenant-lifecycle-grace-days"
          />
        </label>

        <label htmlFor="tenant-lifecycle-auto-reactivate" className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
          <input
            id="tenant-lifecycle-auto-reactivate"
            type="checkbox"
            checked={autoReactivateWhenValidLicense}
            onChange={(event) => setAutoReactivateWhenValidLicense(event.target.checked)}
            disabled
            data-testid="tenant-lifecycle-auto-reactivate"
          />
          Legacy auto-reactivate setting (retired)
        </label>

        <label htmlFor="tenant-lifecycle-revoke-sessions" className="flex items-center gap-2 text-sm text-[var(--color-text-secondary)]">
          <input
            id="tenant-lifecycle-revoke-sessions"
            type="checkbox"
            checked={revokeSessionsOnSuspend}
            onChange={(event) => setRevokeSessionsOnSuspend(event.target.checked)}
            data-testid="tenant-lifecycle-revoke-sessions"
          />
          Revoke active user sessions when suspending
        </label>
      </div>

      <div className="mt-4 flex items-center gap-3">
        <button
          type="button"
          onClick={() => saveMutation.mutate()}
          disabled={saveMutation.isPending}
          className="rounded bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-50"
          data-testid="tenant-lifecycle-save"
        >
          {saveMutation.isPending ? 'Saving…' : 'Save settings'}
        </button>
        {saveMutation.isError && (
          <ApiErrorCallout
            message={getErrorMessage(saveMutation.error, 'Failed to save tenant lifecycle settings.')}
          />
        )}
        {saveMutation.isSuccess && (
          <span className="text-sm text-[var(--color-success-text)]">Saved.</span>
        )}
      </div>

      {isEnabled && (
        <div className="mt-6">
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Pending lifecycle actions</h3>
          {pendingQuery.isLoading && <p className="mt-2 text-sm text-[var(--color-text-secondary)]">Loading pending actions…</p>}
          {pendingQuery.data?.items.length === 0 && (
            <p className="mt-2 text-sm text-[var(--color-text-secondary)]" data-testid="tenant-lifecycle-pending-empty">
              No pending tenant lifecycle actions.
            </p>
          )}
          {pendingQuery.data && pendingQuery.data.items.length > 0 && (
            <ul className="mt-2 space-y-1 text-sm text-[var(--color-text-secondary)]" data-testid="tenant-lifecycle-pending-list">
              {pendingQuery.data.items.map((item) => (
                <li key={`${item.tenantId}-${item.actionKind}`}>
                  {item.tenantDisplayName} ({item.tenantSlug}) — {item.actionKind} from{' '}
                  {item.currentStatus}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      <div className="mt-6">
        <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Recent lifecycle runs</h3>
        {runsQuery.isLoading && <p className="mt-2 text-sm text-[var(--color-text-secondary)]">Loading runs…</p>}
        {runsQuery.data?.items.length === 0 && (
          <p className="mt-2 text-sm text-[var(--color-text-secondary)]" data-testid="tenant-lifecycle-runs-empty">
            No lifecycle runs recorded yet.
          </p>
        )}
        {runsQuery.data && runsQuery.data.items.length > 0 && (
          <ul className="mt-2 space-y-1 text-sm text-[var(--color-text-secondary)]" data-testid="tenant-lifecycle-runs-list">
            {runsQuery.data.items.map((run) => (
              <li key={run.runId}>
                {run.outcome}: suspended {run.suspendedCount}, reactivated {run.reactivatedCount},
                sessions revoked {run.sessionsRevokedCount} ({new Date(run.processedAt).toLocaleString()})
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
