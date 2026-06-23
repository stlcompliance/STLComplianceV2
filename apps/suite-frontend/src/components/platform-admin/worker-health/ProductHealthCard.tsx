import type { PlatformWorkerHealthOrchestrationStatusResponse } from '../../../api/types'
import { healthBadgeClass } from './utils'

type Props = {
  status: PlatformWorkerHealthOrchestrationStatusResponse
}

export function ProductHealthCard({ status }: Props) {
  return (
    <div
      data-testid="platform-orchestration-product-health"
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm"
    >
      <div className="flex flex-wrap items-center gap-3">
        <h3 className="text-sm font-semibold text-[var(--color-text-primary)]">Product health</h3>
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
            className="rounded border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm text-[var(--color-text-secondary)]"
            data-testid={`platform-orchestration-health-${probe.productKey}`}
          >
            <span className="font-medium text-[var(--color-text-primary)]">{probe.productKey}</span>
            {' — '}
            <span>{probe.status}</span>
            {probe.latencyMs != null ? (
              <span className="text-xs text-[var(--color-text-muted)]"> · {Math.round(probe.latencyMs)} ms</span>
            ) : null}
            {probe.errorCode ? <p className="mt-1 text-xs text-[var(--color-warning-text)]">{probe.errorCode}</p> : null}
          </li>
        ))}
      </ul>
    </div>
  )
}
