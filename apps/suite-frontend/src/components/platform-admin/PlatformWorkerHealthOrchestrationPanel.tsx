import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import { useState } from 'react'
import { ProductHealthCard } from './worker-health/ProductHealthCard'
import { ServiceTokenInventoryCard } from './worker-health/ServiceTokenInventoryCard'
import { WorkerStatusCard } from './worker-health/WorkerStatusCard'

export function PlatformWorkerHealthOrchestrationPanel() {
  const queryClient = useQueryClient()
  const [actionNotice, setActionNotice] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  const statusQuery = useQuery({
    queryKey: ['platform-worker-health-orchestration'],
    queryFn: () => nexarr.getPlatformWorkerHealthOrchestration(),
    refetchInterval: 30_000,
  })

  const tokenCleanupMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformServiceTokenCleanup(),
    onSuccess: () => {
      setActionError(null)
      setActionNotice('Service token cleanup triggered.')
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-service-token-cleanup-runs'] })
    },
    onError: (error: Error) => {
      setActionNotice(null)
      setActionError(error.message)
    },
  })

  const entitlementMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformEntitlementReconciliation(),
    onSuccess: () => {
      setActionError(null)
      setActionNotice('Entitlement reconciliation triggered.')
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-entitlement-reconciliation-runs'] })
    },
    onError: (error: Error) => {
      setActionNotice(null)
      setActionError(error.message)
    },
  })

  const lifecycleMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformTenantLifecycle(),
    onSuccess: () => {
      setActionError(null)
      setActionNotice('Tenant lifecycle orchestration triggered.')
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-tenant-lifecycle-runs'] })
    },
    onError: (error: Error) => {
      setActionNotice(null)
      setActionError(error.message)
    },
  })

  const outboxMutation = useMutation({
    mutationFn: () => nexarr.triggerPlatformOutboxPublisher(),
    onSuccess: () => {
      setActionError(null)
      setActionNotice('Platform outbox publish triggered.')
      void queryClient.invalidateQueries({ queryKey: ['platform-worker-health-orchestration'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-publisher-runs'] })
      void queryClient.invalidateQueries({ queryKey: ['platform-outbox-events'] })
    },
    onError: (error: Error) => {
      setActionNotice(null)
      setActionError(error.message)
    },
  })

  const status = statusQuery.data
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
        <p className="text-sm text-[var(--color-text-muted)]">Loading orchestration status…</p>
      )}
      {statusQuery.isError && (
        <ApiErrorCallout
          message={getErrorMessage(statusQuery.error, 'Failed to load orchestration status.')}
          onRetry={() => void statusQuery.refetch()}
          retryLabel="Retry orchestration"
        />
      )}
      {actionError ? (
        <ApiErrorCallout
          message={actionError}
          onRetry={() => {
            setActionError(null)
            setActionNotice(null)
          }}
          retryLabel="Dismiss"
        />
      ) : null}
      {actionNotice ? (
        <p
          className="rounded-md border border-emerald-700/40 bg-emerald-950/20 px-3 py-2 text-sm text-emerald-300"
          data-testid="platform-orchestration-action-notice"
        >
          {actionNotice}
        </p>
      ) : null}

      {status && (
        <>
          <p className="text-xs text-[var(--color-text-muted)]">
            Generated {new Date(status.generatedAt).toLocaleString()}
          </p>

          <ProductHealthCard status={status} />
          <ServiceTokenInventoryCard status={status} />

          <div className="space-y-4">
            {workers.map((worker) => (
              <WorkerStatusCard
                key={worker.workerKey}
                worker={worker}
                tokenCleanupPending={tokenCleanupMutation.isPending}
                entitlementPending={entitlementMutation.isPending}
                lifecyclePending={lifecycleMutation.isPending}
                outboxPending={outboxMutation.isPending}
                onTriggerTokenCleanup={() => tokenCleanupMutation.mutate()}
                onTriggerEntitlement={() => entitlementMutation.mutate()}
                onTriggerLifecycle={() => lifecycleMutation.mutate()}
                onTriggerOutbox={() => outboxMutation.mutate()}
              />
            ))}
          </div>
        </>
      )}
    </section>
  )
}
