import type { PlatformWorkerHealthOrchestrationStatusResponse } from '../../../api/types'
import { healthBadgeClass } from './utils'

type Props = {
  status: PlatformWorkerHealthOrchestrationStatusResponse
}

export function ProductHealthCard({ status }: Props) {
  return (
    <div
      data-testid="platform-orchestration-product-health"
      className="rounded-lg border border-slate-700 bg-slate-900/60 p-4"
    >
      <div className="flex flex-wrap items-center gap-3">
        <h3 className="text-sm font-semibold text-white">Product health</h3>
        <span
          className={['rounded-full px-2 py-0.5 text-xs font-medium', healthBadgeClass(status.platformHealthStatus)].join(' ')}
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
              <span className="text-xs text-[var(--color-text-muted)]"> · {Math.round(probe.latencyMs)} ms</span>
            ) : null}
            {probe.errorCode ? <p className="mt-1 text-xs text-amber-400">{probe.errorCode}</p> : null}
          </li>
        ))}
      </ul>
    </div>
  )
}
