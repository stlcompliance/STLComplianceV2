import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import type { PlatformAuditPackageExportSummary } from '../../../api/types'

type Props = {
  isLoading: boolean
  isError: boolean
  error: unknown
  summary: PlatformAuditPackageExportSummary | undefined
  onRetry: () => void
}

export function AuditExportSummaryCard({ isLoading, isError, error, summary, onRetry }: Props) {
  const availabilityRecordCount = summary?.counts.tenantEntitlements ?? 0

  return (
    <div
      data-testid="platform-audit-summary-section"
      className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4 shadow-sm"
    >
      <h3 className="text-sm font-medium text-[var(--color-text-primary)]">Export summary</h3>
      {isLoading ? (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Calculating scoped counts…</p>
      ) : isError ? (
        <ApiErrorCallout
          message={getErrorMessage(error, 'Failed to load export summary.')}
          onRetry={onRetry}
          retryLabel="Retry summary"
        />
      ) : summary ? (
        <div className="mt-3 space-y-3 text-sm text-[var(--color-text-secondary)]">
          <p data-testid="platform-audit-summary-counts">
            {summary.counts.auditEvents} audit events · {summary.counts.tenants} tenants ·{' '}
            {summary.counts.serviceClients} service clients · {availabilityRecordCount}{' '}
            launch availability records
          </p>
          {summary.byResult.length > 0 ? (
            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">By result</p>
              <ul className="mt-1 flex flex-wrap gap-2">
                {summary.byResult.map((item) => (
                  <li
                    key={item.key}
                    className="rounded-md bg-[var(--color-bg-surface-elevated)] px-2 py-1 font-mono text-xs text-[var(--color-text-primary)]"
                  >
                    {item.key}: {item.count}
                  </li>
                ))}
              </ul>
            </div>
          ) : null}
          {summary.byAction.length > 0 ? (
            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Top actions</p>
              <ul className="mt-1 flex flex-wrap gap-2">
                {summary.byAction.map((item) => (
                  <li
                    key={item.key}
                    className="rounded-md bg-[var(--color-bg-surface-elevated)] px-2 py-1 font-mono text-xs text-[var(--color-accent)]"
                  >
                    {item.key}: {item.count}
                  </li>
                ))}
              </ul>
            </div>
          ) : null}
        </div>
      ) : (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">Summary unavailable.</p>
      )}
    </div>
  )
}
