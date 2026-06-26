import { Link } from 'react-router-dom'
import type { PlatformWorkerOrchestrationWorkerStatus } from '../../../api/types'
import { formatWhen } from './utils'

type Props = {
  worker: PlatformWorkerOrchestrationWorkerStatus
  tokenCleanupPending: boolean
  lifecyclePending: boolean
  outboxPending: boolean
  onTriggerTokenCleanup: () => void
  onTriggerLifecycle: () => void
  onTriggerOutbox: () => void
}

export function WorkerStatusCard({
  worker,
  tokenCleanupPending,
  lifecyclePending,
  outboxPending,
  onTriggerTokenCleanup,
  onTriggerLifecycle,
  onTriggerOutbox,
}: Props) {
  const isLaunchDestinationReconciliationWorker =
    worker.workerKey === 'launch_destination_reconciliation'
  return (
    <div
      key={worker.workerKey}
      data-testid={`platform-orchestration-worker-${worker.workerKey}`}
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm"
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">{worker.label}</h3>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">{worker.description}</p>
          <p className="mt-2 font-mono text-[10px] text-[var(--color-text-muted)]">{worker.serviceTokenScope}</p>
        </div>
        <span
          className={[
            'rounded-full px-2 py-0.5 text-xs font-medium',
            worker.isEnabled
              ? 'bg-[var(--tone-success-bg)] text-[var(--tone-success-text)]'
              : 'bg-[var(--color-bg-control-hover)] text-[var(--color-text-muted)]',
          ].join(' ')}
        >
          {worker.isEnabled ? 'Enabled' : 'Disabled'}
        </span>
      </div>

      <p className="mt-2 text-sm text-[var(--color-text-secondary)]">
        Pending: <span className="font-mono text-[var(--color-text-primary)]">{worker.pendingCount}</span>
      </p>
      {worker.latestRun ? (
        <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
          Last run {formatWhen(worker.latestRun.processedAt)} · {worker.latestRun.outcome} ·{' '}
          {worker.latestRun.primaryCount} {worker.latestRun.primaryCountLabel}
        </p>
      ) : (
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">No batch runs recorded yet.</p>
      )}

      <div className="mt-3 flex flex-wrap gap-3">
        {isLaunchDestinationReconciliationWorker ? (
          <span className="text-sm font-medium text-[var(--color-text-muted)]">No direct settings page</span>
        ) : (
          <Link to={worker.suiteAdminPath} className="text-sm font-medium text-[var(--color-accent)] hover:underline">
            Open settings →
          </Link>
        )}
        {worker.workerKey === 'service_token_cleanup' && (
          <button
            type="button"
            onClick={onTriggerTokenCleanup}
            disabled={tokenCleanupPending || !worker.isEnabled}
            data-testid="platform-orchestration-trigger-service-token-cleanup"
            className="rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] disabled:opacity-50"
          >
            {tokenCleanupPending ? 'Running…' : 'Run cleanup now'}
          </button>
        )}
        {worker.workerKey === 'tenant_lifecycle' && (
          <button
            type="button"
            onClick={onTriggerLifecycle}
            disabled={lifecyclePending || !worker.isEnabled}
            data-testid="platform-orchestration-trigger-tenant-lifecycle"
            className="rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] disabled:opacity-50"
          >
            {lifecyclePending ? 'Running…' : 'Run lifecycle now'}
          </button>
        )}
        {worker.workerKey === 'platform_outbox_publisher' && (
          <button
            type="button"
            onClick={onTriggerOutbox}
            disabled={outboxPending || !worker.isEnabled}
            data-testid="platform-orchestration-trigger-platform-outbox"
            className="rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-sm font-medium text-[var(--color-on-accent)] disabled:opacity-50"
          >
            {outboxPending ? 'Running…' : 'Run publish now'}
          </button>
        )}
      </div>
    </div>
  )
}
