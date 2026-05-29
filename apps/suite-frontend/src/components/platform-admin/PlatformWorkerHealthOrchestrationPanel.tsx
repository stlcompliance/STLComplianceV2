import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link } from 'react-router-dom'

import * as nexarr from '../../api/nexarrClient'

function formatWhen(value: string | null | undefined) {
  if (!value) {
    return 'Never'
  }
  return new Date(value).toLocaleString()
}

function healthBadgeClass(status: string) {
  if (status === 'Healthy') {
    return 'bg-emerald-100 text-emerald-800'
  }
  if (status === 'Degraded' || status === 'NotConfigured') {
    return 'bg-amber-100 text-amber-800'
  }
  return 'bg-red-100 text-red-800'
}

export function PlatformWorkerHealthOrchestrationPanel() {
  const queryClient = useQueryClient()

  const statusQuery = useQuery({
    queryKey: ['platform-worker-health-orchestration'],
    queryFn: () => nexarr.getPlatformWorkerHealthOrchestration(),
    refetchInterval: 30_000,
  })

  const tokenCleanupMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformServiceTokenCleanup(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-cleanup-runs'] })
    },
  })

  const entitlementMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformEntitlementReconciliation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-entitlement-reconciliation-runs'] })
    },
  })

  const lifecycleMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformTenantLifecycle(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-tenant-lifecycle-runs'] })
    },
  })

  const outboxMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformOutboxPublisher(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-events'] })
    },
  })

  const status = statusQuery.data
  const tokens = status?.serviceTokens
  const workers = status?.workers ?? []

  return (
    <section
      data-testid="platform-worker-health-orchestration-panel"
      className="space-y-6"
    >
      <header>
        <h2 className="text-lg font-semibold text-white">Service token & worker health</h2>
        <p className="mt-1 text-sm text-slate-400">
          Product API readiness, service token inventory, and NexArr shared-worker lifecycle jobs in
          one operational view. Manual triggers run the same batch processors as{' '}
          <code className="text-xs">shared-worker</code> internal APIs.
        </p>
      </header>

      {statusQuery.isLoading && (
        <p className="text-sm text-slate-500">Loading orchestration status…</p>
      )}
      {statusQuery.isError && (
        <p className="text-sm text-red-400" role="alert">
          Failed to load orchestration status: {(statusQuery.error as Error).message}
        </p>
      )}

      {status && (
        <>
          <p className="text-xs text-slate-500">
            Generated {new Date(status.generatedAt).toLocaleString()}
          </p>

          <div
            data-testid="platform-orchestration-product-health"
            className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
          >
            <div className="flex flex-wrap items-center gap-3">
              <h3 className="text-sm font-semibold text-white">Product health</h3>
              <span
                className={[
                  'rounded-full px-2 py-0.5 text-xs font-medium',
                  healthBadgeClass(status.platformHealthStatus),
                ].join(' ')}
                data-testid="platform-orchestration-health-status"
              >
                {status.platformHealthStatus}
              </span>
            </div>
            <ul className="mt-3 grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
              {status.productHealth.map((probe) => (
                <li
                  key={probe.productKey}
                  className="rounded border border-slate-700 px-3 py-2 text-sm text-slate-300"
                  data-testid={`platform-orchestration-health-${probe.productKey}`}
                >
                  <span className="font-medium text-white">{probe.productKey}</span>
                  {' — '}
                  <span>{probe.status}</span>
                  {probe.latencyMs != null ? (
                    <span className="text-xs text-slate-500">
                      {' '}
                      · {Math.round(probe.latencyMs)} ms
                    </span>
                  ) : null}
                  {probe.errorCode ? (
                    <p className="mt-1 text-xs text-amber-400">{probe.errorCode}</p>
                  ) : null}
                </li>
              ))}
            </ul>
          </div>

          <div
            data-testid="platform-orchestration-service-tokens"
            className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
          >
            <h3 className="text-sm font-semibold text-white">Service token inventory</h3>
            <dl className="mt-3 grid gap-2 text-sm text-slate-300 sm:grid-cols-2 lg:grid-cols-3">
              <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
                <dt>Active</dt>
                <dd className="font-medium tabular-nums text-white">{tokens?.activeCount ?? 0}</dd>
              </div>
              <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
                <dt>Expiring (24h)</dt>
                <dd className="font-medium tabular-nums text-white">
                  {tokens?.expiringWithin24HoursCount ?? 0}
                </dd>
              </div>
              <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
                <dt>Expired (retained)</dt>
                <dd className="font-medium tabular-nums text-white">
                  {tokens?.expiredRetainedCount ?? 0}
                </dd>
              </div>
              <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
                <dt>Revoked (retained)</dt>
                <dd className="font-medium tabular-nums text-white">
                  {tokens?.revokedRetainedCount ?? 0}
                </dd>
              </div>
              <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
                <dt>Pending cleanup</dt>
                <dd className="font-medium tabular-nums text-white">
                  {tokens?.pendingCleanupCount ?? 0}
                </dd>
              </div>
              <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
                <dt>Active clients</dt>
                <dd className="font-medium tabular-nums text-white">
                  {status.activeServiceClientCount}
                </dd>
              </div>
            </dl>
          </div>

          <div className="space-y-4">
            {workers.map((worker) => (
              <div
                key={worker.workerKey}
                data-testid={`platform-orchestration-worker-${worker.workerKey}`}
                className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
              >
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <h3 className="text-sm font-semibold text-white">{worker.label}</h3>
                    <p className="mt-1 text-xs text-slate-500">{worker.description}</p>
                    <p className="mt-2 font-mono text-[10px] text-slate-500">
                      {worker.serviceTokenScope}
                    </p>
                  </div>
                  <span
                    className={[
                      'rounded-full px-2 py-0.5 text-xs font-medium',
                      worker.isEnabled
                        ? 'bg-emerald-100 text-emerald-800'
                        : 'bg-slate-100 text-slate-600',
                    ].join(' ')}
                  >
                    {worker.isEnabled ? 'Enabled' : 'Disabled'}
                  </span>
                </div>

                <p className="mt-2 text-sm text-slate-400">
                  Pending (sample):{' '}
                  <span className="font-mono text-slate-200">{worker.pendingCount}</span>
                </p>
                {worker.latestRun ? (
                  <p className="mt-1 text-sm text-slate-400">
                    Last run {formatWhen(worker.latestRun.processedAt)} · {worker.latestRun.outcome}{' '}
                    · {worker.latestRun.primaryCount} {worker.latestRun.primaryCountLabel}
                  </p>
                ) : (
                  <p className="mt-1 text-sm text-slate-500">No batch runs recorded yet.</p>
                )}

                <div className="mt-3 flex flex-wrap gap-3">
                  <Link
                    to={worker.suiteAdminPath}
                    className="text-sm font-medium text-stl-teal hover:underline"
                  >
                    Open settings →
                  </Link>
                  {worker.workerKey === 'service_token_cleanup' && (
                    <button
                      type="button"
                      onClick={() => tokenCleanupMutation.mutate()}
                      disabled={tokenCleanupMutation.isPending || !worker.isEnabled}
                      data-testid="platform-orchestration-trigger-service-token-cleanup"
                      className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
                    >
                      {tokenCleanupMutation.isPending ? 'Running…' : 'Run cleanup now'}
                    </button>
                  )}
                  {worker.workerKey === 'entitlement_reconciliation' && (
                    <button
                      type="button"
                      onClick={() => entitlementMutation.mutate()}
                      disabled={entitlementMutation.isPending || !worker.isEnabled}
                      data-testid="platform-orchestration-trigger-entitlement-reconciliation"
                      className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
                    >
                      {entitlementMutation.isPending ? 'Running…' : 'Run reconciliation now'}
                    </button>
                  )}
                  {worker.workerKey === 'tenant_lifecycle' && (
                    <button
                      type="button"
                      onClick={() => lifecycleMutation.mutate()}
                      disabled={lifecycleMutation.isPending || !worker.isEnabled}
                      data-testid="platform-orchestration-trigger-tenant-lifecycle"
                      className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
                    >
                      {lifecycleMutation.isPending ? 'Running…' : 'Run lifecycle now'}
                    </button>
                  )}
                  {worker.workerKey === 'platform_outbox_publisher' && (
                    <button
                      type="button"
                      onClick={() => outboxMutation.mutate()}
                      disabled={outboxMutation.isPending || !worker.isEnabled}
                      data-testid="platform-orchestration-trigger-platform-outbox"
                      className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
                    >
                      {outboxMutation.isPending ? 'Running…' : 'Run publish now'}
                    </button>
                  )}
                </div>
              </div>
            ))}
          </div>
        </>
      )}
    </section>
  )
}
