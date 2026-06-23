import { PlatformWorkerHealthOrchestrationPanel } from '../../components/platform-admin/PlatformWorkerHealthOrchestrationPanel'

export function PlatformWorkerHealthPage() {
  return (
    <div className="space-y-4">
      <p className="text-sm text-slate-400">
        Unified control plane for cross-product readiness probes, service token posture, and NexArr
        lifecycle workers. Use per-worker settings pages for retention and policy detail.
      </p>
      <PlatformWorkerHealthOrchestrationPanel />
    </div>
  )
}
