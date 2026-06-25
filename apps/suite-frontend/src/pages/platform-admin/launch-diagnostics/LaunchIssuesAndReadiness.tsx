import type { LaunchDiagnosticsResponse } from '../../../api/types'
import { readinessClass } from './utils'

type Props = {
  diagnostics: LaunchDiagnosticsResponse
}

export function LaunchIssuesAndReadiness({ diagnostics }: Props) {
  return (
    <>
      {diagnostics.issues.length > 0 && (
        <section className="rounded-lg border border-[var(--color-warning-border)] bg-[var(--color-warning-bg)] p-4">
          <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Issues ({diagnostics.issues.length})</h4>
          <ul className="mt-2 space-y-1 text-sm text-[var(--color-text-secondary)]">
            {diagnostics.issues.map((issue, index) => (
              <li key={`${issue.issueCode}-${issue.tenantId ?? 'global'}-${issue.productKey ?? index}`}>
                <span
                  className={
                    issue.severity === 'error' ? 'font-medium text-[var(--color-danger-text)]' : 'text-[var(--color-warning-text)]'
                  }
                >
                  [{issue.severity}]
                </span>{' '}
                {issue.message}
              </li>
            ))}
          </ul>
        </section>
      )}

      <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
        <table className="min-w-full text-left text-sm">
          <thead className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-muted)] text-xs uppercase text-[var(--color-text-muted)]">
            <tr>
              <th className="px-3 py-2">Tenant</th>
              <th className="px-3 py-2">Product</th>
              <th className="px-3 py-2">Launch availability</th>
              <th className="px-3 py-2">Profile</th>
              <th className="px-3 py-2">Allowlist</th>
              <th className="px-3 py-2">Handoffs</th>
              <th className="px-3 py-2">Readiness</th>
            </tr>
          </thead>
          <tbody>
            {diagnostics.rows.map((row) => (
              <tr key={`${row.tenantId}-${row.productKey}`} className="border-b border-[var(--color-border-subtle)]">
                <td className="px-3 py-2">
                  <span className="font-medium text-[var(--color-text-primary)]">{row.tenantDisplayName}</span>
                </td>
                <td className="px-3 py-2">{row.productDisplayName}</td>
                <td className="px-3 py-2">{row.hasActiveEntitlement ? 'Available' : 'Unavailable'}</td>
                <td className="px-3 py-2">
                  {row.launchProfileActive ? 'Enabled' : row.hasLaunchProfile ? 'Disabled' : 'Missing'}
                </td>
                <td className="px-3 py-2">{row.callbackAllowlistEntryCount}</td>
                <td className="px-3 py-2">
                  {row.pendingHandoffCount} pending / {row.expiredHandoffCount} expired
                </td>
                <td className={`px-3 py-2 font-medium ${readinessClass(row.launchReadiness)}`}>
                  {row.launchReadiness}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  )
}
