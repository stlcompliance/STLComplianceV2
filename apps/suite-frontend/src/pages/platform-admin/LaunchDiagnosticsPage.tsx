import { useMutation, useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
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
  const [selectedTenantId, setSelectedTenantId] = useState('')
  const [selectedProductKey, setSelectedProductKey] = useState('')

  const diagnosticsQuery = useQuery({
    queryKey: ['platform-admin-launch-diagnostics'],
    queryFn: () => nexarr.getPlatformAdminLaunchDiagnostics({ page: 1, pageSize: 100 }),
  })
  const attemptsQuery = useQuery({
    queryKey: ['platform-admin-launch-attempts'],
    queryFn: () => nexarr.getPlatformAdminLaunchAttempts({ page: 1, pageSize: 25 }),
  })

  const validateLaunchMutation = useMutation({
    mutationFn: nexarr.validatePlatformLaunch,
  })

  const data = diagnosticsQuery.data
  const attempts = attemptsQuery.data?.items ?? []
  const tenants = useMemo(
    () =>
      data
        ? [...new Map(data.rows.map((row) => [row.tenantId, row])).values()].sort((a, b) =>
            a.tenantDisplayName.localeCompare(b.tenantDisplayName),
          )
        : [],
    [data],
  )
  const products = useMemo(
    () =>
      data
        ? [...new Map(data.rows.map((row) => [row.productKey, row])).values()].sort((a, b) =>
            a.productDisplayName.localeCompare(b.productDisplayName),
          )
        : [],
    [data],
  )

  if (diagnosticsQuery.isLoading || attemptsQuery.isLoading) {
    return <p className="text-sm text-slate-500">Loading launch diagnostics…</p>
  }

  if (diagnosticsQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(diagnosticsQuery.error, 'Failed to load diagnostics.')}
        onRetry={() => void diagnosticsQuery.refetch()}
        retryLabel="Retry diagnostics"
      />
    )
  }

  const diagnostics = data!

  return (
    <div className="space-y-6">
      <section className="rounded-lg border border-slate-200 bg-white p-4">
        <h4 className="text-sm font-semibold text-stl-navy">Validate launch eligibility</h4>
        <p className="mt-1 text-xs text-slate-500">
          Check whether a tenant can launch a product right now and see the denial reason code.
        </p>
        <div className="mt-3 grid gap-3 md:grid-cols-3">
          <label className="text-xs font-medium text-slate-600">
            Tenant
            <select
              className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
              value={selectedTenantId}
              onChange={(event) => setSelectedTenantId(event.target.value)}
            >
              <option value="">Select tenant…</option>
              {tenants.map((tenant) => (
                <option key={tenant.tenantId} value={tenant.tenantId}>
                  {tenant.tenantDisplayName} ({tenant.tenantSlug})
                </option>
              ))}
            </select>
          </label>
          <label className="text-xs font-medium text-slate-600">
            Product
            <select
              className="mt-1 w-full rounded-md border border-slate-300 px-2 py-1 text-sm"
              value={selectedProductKey}
              onChange={(event) => setSelectedProductKey(event.target.value)}
            >
              <option value="">Select product…</option>
              {products.map((product) => (
                <option key={product.productKey} value={product.productKey}>
                  {product.productDisplayName}
                </option>
              ))}
            </select>
          </label>
          <div className="flex items-end">
            <button
              type="button"
              className="rounded-md bg-stl-navy px-3 py-2 text-sm font-medium text-white disabled:cursor-not-allowed disabled:opacity-60"
              disabled={!selectedTenantId || !selectedProductKey || validateLaunchMutation.isPending}
              onClick={() =>
                validateLaunchMutation.mutate({
                  tenantId: selectedTenantId,
                  productKey: selectedProductKey,
                })
              }
            >
              {validateLaunchMutation.isPending ? 'Validating…' : 'Validate launch'}
            </button>
          </div>
        </div>
        {validateLaunchMutation.isError ? (
          <ApiErrorCallout
            className="mt-3"
            message={getErrorMessage(validateLaunchMutation.error, 'Failed to validate launch.')}
          />
        ) : null}
        {validateLaunchMutation.data ? (
          <div className="mt-3 rounded-md border border-slate-200 bg-slate-50 p-3 text-sm">
            <p>
              <span className="font-medium text-stl-navy">Can launch:</span>{' '}
              {validateLaunchMutation.data.canLaunch ? 'Yes' : 'No'}
            </p>
            <p>
              <span className="font-medium text-stl-navy">Reason:</span>{' '}
              {validateLaunchMutation.data.reasonCode ?? 'none'}
            </p>
            <p className="break-all">
              <span className="font-medium text-stl-navy">Launch URL:</span>{' '}
              {validateLaunchMutation.data.launchUrl ?? 'none'}
            </p>
          </div>
        ) : null}
      </section>

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

      <section className="space-y-3">
        <div>
          <h4 className="text-sm font-semibold text-stl-navy">Recent launch attempts</h4>
          <p className="mt-1 text-xs text-slate-500">
            Updated {new Date(diagnostics.generatedAt).toLocaleString()}
          </p>
        </div>
        {attemptsQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(attemptsQuery.error, 'Failed to load launch attempts.')}
            onRetry={() => void attemptsQuery.refetch()}
            retryLabel="Retry launch attempts"
          />
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
