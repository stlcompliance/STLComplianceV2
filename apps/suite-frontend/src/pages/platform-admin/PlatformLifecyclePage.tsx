import { PlatformLifecycleOverviewPanel } from '../../components/platform-admin/PlatformLifecycleOverviewPanel'

export function PlatformLifecyclePage() {
  return (
    <div className="space-y-3">
      <div>
        <h4 className="text-lg font-semibold text-[var(--color-text-primary)]">Platform lifecycle workers</h4>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Service-token cleanup, entitlement reconciliation, tenant lifecycle, and platform event
          outbox — scheduled via shared-worker and nexarr-worker with platform-admin visibility here.
        </p>
      </div>
      <PlatformLifecycleOverviewPanel />
    </div>
  )
}
