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
  return (
    <div
      data-testid="platform-audit-summary-section"
      className="rounded-lg border border-slate-800 bg-slate-950/50 p-4"
    >
      <h3 className="text-sm font-medium text-slate-200">Export summary</h3>
      {isLoading ? (
        <p className="mt-3 text-sm text-slate-500">Calculating scoped counts…</p>
      ) : isError ? (
        <ApiErrorCallout
          message={getErrorMessage(error, 'Failed to load export summary.')}
          onRetry={onRetry}
          retryLabel="Retry summary"
        />
      ) : summary ? (
        <div className="mt-3 space-y-3 text-sm text-slate-300">
          <p data-testid="platform-audit-summary-counts">
            {summary.counts.auditEvents} audit events · {summary.counts.tenants} tenants ·{' '}
            {summary.counts.serviceClients} service clients · {summary.counts.tenantEntitlements}{' '}
            entitlements
          </p>
          {summary.byResult.length > 0 ? (
            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-slate-500">By result</p>
              <ul className="mt-1 flex flex-wrap gap-2">
                {summary.byResult.map((item) => (
                  <li
                    key={item.key}
                    className="rounded-md bg-slate-800 px-2 py-1 font-mono text-xs text-slate-200"
                  >
                    {item.key}: {item.count}
                  </li>
                ))}
              </ul>
            </div>
          ) : null}
          {summary.byAction.length > 0 ? (
            <div>
              <p className="text-xs font-medium uppercase tracking-wide text-slate-500">Top actions</p>
              <ul className="mt-1 flex flex-wrap gap-2">
                {summary.byAction.map((item) => (
                  <li
                    key={item.key}
                    className="rounded-md bg-slate-800 px-2 py-1 font-mono text-xs text-teal-200"
                  >
                    {item.key}: {item.count}
                  </li>
                ))}
              </ul>
            </div>
          ) : null}
        </div>
      ) : (
        <p className="mt-3 text-sm text-slate-500">Summary unavailable.</p>
      )}
    </div>
  )
}
