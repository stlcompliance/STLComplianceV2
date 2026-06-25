import type { PlatformWorkerHealthOrchestrationStatusResponse } from '../../../api/types'

type Props = {
  status: PlatformWorkerHealthOrchestrationStatusResponse
}

export function ServiceTokenInventoryCard({ status }: Props) {
  const tokens = status.serviceTokens

  return (
    <div
      data-testid="platform-orchestration-service-tokens"
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm"
    >
      <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Service token inventory</h3>
      <dl className="mt-3 grid gap-2 text-sm text-[var(--color-text-secondary)] sm:grid-cols-2 lg:grid-cols-3">
        <div className="flex justify-between gap-2 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2">
          <dt>Enabled</dt>
          <dd className="font-medium tabular-nums text-[var(--color-text-primary)]">{tokens?.activeCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2">
          <dt>Expiring (24h)</dt>
          <dd className="font-medium tabular-nums text-[var(--color-text-primary)]">{tokens?.expiringWithin24HoursCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2">
          <dt>Expired (retained)</dt>
          <dd className="font-medium tabular-nums text-[var(--color-text-primary)]">{tokens?.expiredRetainedCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2">
          <dt>Revoked (retained)</dt>
          <dd className="font-medium tabular-nums text-[var(--color-text-primary)]">{tokens?.revokedRetainedCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2">
          <dt>Pending cleanup</dt>
          <dd className="font-medium tabular-nums text-[var(--color-text-primary)]">{tokens?.pendingCleanupCount ?? 0}</dd>
        </div>
        <div className="flex justify-between gap-2 rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2">
          <dt>Enabled clients</dt>
          <dd className="font-medium tabular-nums text-[var(--color-text-primary)]">{status.activeServiceClientCount}</dd>
        </div>
      </dl>
    </div>
  )
}
