import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../../api/nexarrClient'

function readinessClass(readiness: string): string {
  if (readiness === 'ready') {
    return 'text-green-700'
  }
  if (readiness === 'tenant_suspended') {
    return 'text-red-700'
  }
  return 'text-amber-700'
}

function resultClass(result: string): string {
  if (result.toLowerCase() === 'success') {
    return 'text-green-700'
  }
  if (result.toLowerCase() === 'denied') {
    return 'text-red-700'
  }
  return 'text-slate-700'
}

export function LaunchDiagnosticsPage() {
  const diagnosticsQuery = useQuery({
    queryKey: ['platform-admin-launch-diagnostics'],
    queryFn: () => nexarr.getPlatformAdminLaunchDiagnostics({ page: 1, pageSize: 100 }),
  })
  const attemptsQuery = useQuery({
    queryKey: ['platform-admin-launch-attempts'],
    queryFn: () => nexarr.getPlatformAdminLaunchAttempts({ page: 1, pageSize: 25 }),
  })

  if (diagnosticsQuery.isLoading || attemptsQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading launch diagnostics…</p>
  }

  if (diagnosticsQuery.isError) {
    return (
      <p className="text-sm text-red-700" role="alert">
        Failed to load diagnostics: {(diagnosticsQuery.error as Error).message}
      </p>
    )
  }

  const data = diagnosticsQuery.data!
  const attempts = attemptsQuery.data?.items ?? []

  return (
    <div className="space-y-6">
      {data.issues.length > 0 && (
        <section className="rounded-lg border border-amber-200 bg-amber-50/50 p-4">
          <h4 className="text-sm font-semibold text-stl-navy">Issues ({data.issues.length})</h4>
          <ul className="mt-2 space-y-1 text-sm text-slate-700">
            {data.issues.map((issue, index) => (
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
            {data.rows.map((row) => (
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

      <section className="space-y-3">
        <div>
          <h4 className="text-sm font-semibold text-stl-navy">Recent launch attempts</h4>
          <p className="mt-1 text-xs text-slate-500">
            Updated {new Date(data.generatedAt).toLocaleString()}
          </p>
        </div>
        {attemptsQuery.isError ? (
          <p className="text-sm text-red-700" role="alert">
            Failed to load launch attempts: {(attemptsQuery.error as Error).message}
          </p>
        ) : attempts.length === 0 ? (
          <p className="text-sm text-slate-500">No launch attempts recorded.</p>
        ) : (
          <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-200 bg-slate-50 text-xs uppercase text-slate-500">
                <tr>
                  <th className="px-3 py-2">Time</th>
                  <th className="px-3 py-2">Product</th>
                  <th className="px-3 py-2">Tenant</th>
                  <th className="px-3 py-2">Actor</th>
                  <th className="px-3 py-2">Result</th>
                  <th className="px-3 py-2">Reason</th>
                  <th className="px-3 py-2">Correlation</th>
                  <th className="px-3 py-2">Hint</th>
                </tr>
              </thead>
              <tbody>
                {attempts.map((attempt) => (
                  <tr key={attempt.auditEventId} className="border-b border-slate-100 align-top">
                    <td className="px-3 py-2 whitespace-nowrap">
                      {new Date(attempt.occurredAt).toLocaleString()}
                    </td>
                    <td className="px-3 py-2">
                      <span className="font-medium text-stl-navy">
                        {attempt.productDisplayName ?? attempt.productKey ?? 'Unknown'}
                      </span>
                      <span className="block text-xs text-slate-500">{attempt.action}</span>
                    </td>
                    <td className="px-3 py-2">
                      {attempt.tenantDisplayName ?? 'Unknown'}
                      {attempt.tenantSlug && (
                        <span className="block text-xs text-slate-500">{attempt.tenantSlug}</span>
                      )}
                    </td>
                    <td className="px-3 py-2">
                      {attempt.actorDisplayName ?? attempt.actorEmail ?? 'System'}
                      {attempt.actorEmail && (
                        <span className="block text-xs text-slate-500">{attempt.actorEmail}</span>
                      )}
                    </td>
                    <td className={`px-3 py-2 font-medium ${resultClass(attempt.result)}`}>
                      {attempt.result}
                    </td>
                    <td className="px-3 py-2">{attempt.reasonCode ?? 'none'}</td>
                    <td className="px-3 py-2 font-mono text-xs text-slate-600">
                      {attempt.correlationId}
                    </td>
                    <td className="px-3 py-2 text-slate-700">
                      {attempt.remediationHint ?? 'none'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
