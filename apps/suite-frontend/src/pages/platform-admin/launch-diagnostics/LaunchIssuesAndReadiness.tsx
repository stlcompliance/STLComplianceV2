import type { LaunchDiagnosticsResponse } from '../../../api/types'
import { readinessClass } from './utils'

type Props = {
  diagnostics: LaunchDiagnosticsResponse
}

export function LaunchIssuesAndReadiness({ diagnostics }: Props) {
  return (
    <>
      {diagnostics.issues.length > 0 && (
        <section className="rounded-lg border border-amber-200 bg-amber-50/50 p-4">
          <h4 className="text-sm font-semibold text-stl-navy">Issues ({diagnostics.issues.length})</h4>
          <ul className="mt-2 space-y-1 text-sm text-slate-700">
            {diagnostics.issues.map((issue, index) => (
              <li key={`${issue.issueCode}-${issue.tenantId ?? 'global'}-${issue.productKey ?? index}`}>
                <span
                  className={
                    issue.severity === 'error' ? 'font-medium text-red-700' : 'text-amber-800'
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

      <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
        <table className="min-w-full text-left text-sm">
          <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
            <tr>
              <th className="px-3 py-2">Tenant</th>
              <th className="px-3 py-2">Product</th>
              <th className="px-3 py-2">Entitled</th>
              <th className="px-3 py-2">Profile</th>
              <th className="px-3 py-2">Allowlist</th>
              <th className="px-3 py-2">Handoffs</th>
              <th className="px-3 py-2">Readiness</th>
            </tr>
          </thead>
          <tbody>
            {diagnostics.rows.map((row) => (
              <tr key={`${row.tenantId}-${row.productKey}`} className="border-b border-slate-100">
                <td className="px-3 py-2">
                  <span className="font-medium text-stl-navy">{row.tenantDisplayName}</span>
                  <span className="block text-xs text-slate-500">{row.tenantSlug}</span>
                </td>
                <td className="px-3 py-2">{row.productDisplayName}</td>
                <td className="px-3 py-2">{row.hasActiveEntitlement ? 'Yes' : 'No'}</td>
                <td className="px-3 py-2">
                  {row.launchProfileActive ? 'Active' : row.hasLaunchProfile ? 'Inactive' : 'Missing'}
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
