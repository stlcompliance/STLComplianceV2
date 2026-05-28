import { PlatformLifecycleOverviewPanel } from '../../components/platform-admin/PlatformLifecycleOverviewPanel'

export function PlatformLifecyclePage() {
  return (
    <div className="space-y-3">
      <div>
        <h4 className="text-lg font-semibold text-stl-navy">Platform lifecycle workers</h4>
        <p className="mt-1 text-sm text-slate-600">
          Service-token cleanup, entitlement reconciliation, and tenant lifecycle — scheduled via
          shared-worker with platform-admin visibility here.
        </p>
      </div>
      <PlatformLifecycleOverviewPanel />
    </div>
  )
}
