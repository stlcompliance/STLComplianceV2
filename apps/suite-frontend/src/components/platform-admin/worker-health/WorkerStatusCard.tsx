import { Link } from 'react-router-dom'
import type { PlatformWorkerOrchestrationWorkerStatus } from '../../../api/types'
import { formatWhen } from './utils'

type Props = {
  worker: PlatformWorkerOrchestrationWorkerStatus
  tokenCleanupPending: boolean
  entitlementPending: boolean
  lifecyclePending: boolean
  outboxPending: boolean
  onTriggerTokenCleanup: () => void
  onTriggerEntitlement: () => void
  onTriggerLifecycle: () => void
  onTriggerOutbox: () => void
}

export function WorkerStatusCard({
  worker,
  tokenCleanupPending,
  entitlementPending,
  lifecyclePending,
  outboxPending,
  onTriggerTokenCleanup,
  onTriggerEntitlement,
  onTriggerLifecycle,
  onTriggerOutbox,
}: Props) {
  return (
    <div
      key={worker.workerKey}
      data-testid={`platform-orchestration-worker-${worker.workerKey}`}
      className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
    >
      <div className="flex flex-wrap items-start justify-between gap-2">
        <div>
          <h3 className="text-sm font-semibold text-white">{worker.label}</h3>
          <p className="mt-1 text-xs text-slate-500">{worker.description}</p>
          <p className="mt-2 font-mono text-[10px] text-slate-500">{worker.serviceTokenScope}</p>
        </div>
        <span
          className={[
            'rounded-full px-2 py-0.5 text-xs font-medium',
            worker.isEnabled ? 'bg-emerald-100 text-emerald-800' : 'bg-slate-100 text-slate-600',
          ].join(' ')}
        >
          {worker.isEnabled ? 'Enabled' : 'Disabled'}
        </span>
      </div>

      <p className="mt-2 text-sm text-slate-400">
        Pending: <span className="font-mono text-slate-200">{worker.pendingCount}</span>
      </p>
      {worker.latestRun ? (
        <p className="mt-1 text-sm text-slate-400">
          Last run {formatWhen(worker.latestRun.processedAt)} · {worker.latestRun.outcome} ·{' '}
          {worker.latestRun.primaryCount} {worker.latestRun.primaryCountLabel}
        </p>
      ) : (
        <p className="mt-1 text-sm text-slate-500">No batch runs recorded yet.</p>
      )}

      <div className="mt-3 flex flex-wrap gap-3">
        <Link to={worker.suiteAdminPath} className="text-sm font-medium text-stl-teal hover:underline">
          Open settings →
        </Link>
        {worker.workerKey === 'service_token_cleanup' && (
          <button
            type="button"
            onClick={onTriggerTokenCleanup}
            disabled={tokenCleanupPending || !worker.isEnabled}
            data-testid="platform-orchestration-trigger-service-token-cleanup"
            className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
          >
            {tokenCleanupPending ? 'Running…' : 'Run cleanup now'}
          </button>
        )}
        {worker.workerKey === 'entitlement_reconciliation' && (
          <button
            type="button"
            onClick={onTriggerEntitlement}
            disabled={entitlementPending || !worker.isEnabled}
            data-testid="platform-orchestration-trigger-entitlement-reconciliation"
            className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
          >
            {entitlementPending ? 'Running…' : 'Run reconciliation now'}
          </button>
        )}
        {worker.workerKey === 'tenant_lifecycle' && (
          <button
            type="button"
            onClick={onTriggerLifecycle}
            disabled={lifecyclePending || !worker.isEnabled}
            data-testid="platform-orchestration-trigger-tenant-lifecycle"
            className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
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
            className="rounded-md bg-stl-teal px-3 py-1.5 text-sm font-medium text-white disabled:opacity-50"
          >
            {outboxPending ? 'Running…' : 'Run publish now'}
          </button>
        )}
      </div>
    </div>
  )
}
