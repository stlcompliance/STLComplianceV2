import type { PlatformWorkerHealthOrchestrationStatusResponse } from '../../../api/types'

type Props = {
  status: PlatformWorkerHealthOrchestrationStatusResponse
}

export function ServiceTokenInventoryCard({ status }: Props) {
  const tokens = status.serviceTokens

  return (
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
          <dd className="font-medium tabular-nums text-white">{tokens?.expiringWithin24HoursCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
          <dt>Expired (retained)</dt>
          <dd className="font-medium tabular-nums text-white">{tokens?.expiredRetainedCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
          <dt>Revoked (retained)</dt>
          <dd className="font-medium tabular-nums text-white">{tokens?.revokedRetainedCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
          <dt>Pending cleanup</dt>
          <dd className="font-medium tabular-nums text-white">{tokens?.pendingCleanupCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-slate-700 px-3 py-2">
          <dt>Active clients</dt>
          <dd className="font-medium tabular-nums text-white">{status.activeServiceClientCount}</dd>
        </div>
      </dl>
    </div>
  )
}
