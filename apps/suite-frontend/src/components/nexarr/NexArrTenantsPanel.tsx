import { useQuery } from '@tanstack/react-query'
import { Building2, ExternalLink } from 'lucide-react'
import { Link } from 'react-router-dom'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import type { TenantOverviewRow } from '../../api/types'
import { useAuth } from '../../auth/AuthProvider'
import { DashboardCard } from '../dashboard/DashboardCard'

function tenantStatusBadgeClass(status: string): string {
  const normalized = status.trim().toLowerCase()
  if (normalized === 'active') {
    return 'bg-emerald-950/50 text-emerald-300'
  }
  if (normalized === 'suspended') {
    return 'bg-amber-950/50 text-amber-300'
  }
  return 'bg-slate-800 text-slate-400'
}

function countActiveTenants(tenants: readonly TenantOverviewRow[]): number {
  return tenants.filter((tenant) => tenant.status.trim().toLowerCase() === 'active').length
}

export function NexArrTenantsPanel() {
  const { me } = useAuth()

  const overviewQuery = useQuery({
    queryKey: ['nexarr-tenants-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
    enabled: me !== undefined,
  })

  if (!me) {
    return <p className="text-sm text-slate-400">Loading tenant registry…</p>
  }

  if (overviewQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading tenant registry…</p>
  }

  if (overviewQuery.isError) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(overviewQuery.error, 'Failed to load tenants.')}
        onRetry={() => void overviewQuery.refetch()}
        retryLabel="Retry tenants"
      />
    )
  }

  const tenants = overviewQuery.data?.items ?? []
  const activeCount = countActiveTenants(tenants)

  return (
    <div className="max-w-5xl space-y-6" data-testid="nexarr-tenants-panel">
      <header>
        <h3 className="text-xl font-semibold text-white">Tenant registry</h3>
        <p className="mt-1 text-sm text-slate-400">
          Platform-wide tenant catalog from NexArr. Create and update tenants in platform
          administration.
        </p>
      </header>

      <div className="grid gap-4 sm:grid-cols-2">
        <DashboardCard title="Total tenants">
          <p className="text-2xl font-semibold text-white">{tenants.length}</p>
        </DashboardCard>
        <DashboardCard title="Active tenants">
          <p className="text-2xl font-semibold text-emerald-300">{activeCount}</p>
        </DashboardCard>
      </div>

      {tenants.length === 0 ? (
        <p className="text-sm text-slate-400">No tenants registered on this platform.</p>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-slate-700 bg-slate-900/60">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-slate-700 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
              <tr>
                <th className="px-4 py-3 font-medium">Tenant</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Entitlements</th>
                <th className="px-4 py-3 font-medium">Members</th>
                <th className="px-4 py-3 font-medium">Created</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-700">
              {tenants.map((tenant) => (
                <tr key={tenant.tenantId}>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <Building2 className="h-4 w-4 shrink-0 text-[var(--color-text-muted)]" aria-hidden />
                      <div>
                        <p className="font-medium text-white">{tenant.displayName}</p>
                        <p className="font-mono text-xs text-[var(--color-text-muted)]">{tenant.slug}</p>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium capitalize ${tenantStatusBadgeClass(tenant.status)}`}
                    >
                      {tenant.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-slate-300">{tenant.activeEntitlementCount}</td>
                  <td className="px-4 py-3 text-slate-300">{tenant.membershipCount}</td>
                  <td className="px-4 py-3 text-slate-400">
                    {new Date(tenant.createdAt).toLocaleDateString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <div className="rounded-lg border border-slate-700 bg-slate-900/40 p-4">
        <p className="text-sm text-slate-400">
          Create tenants, update display names, and manage lifecycle status from the platform admin
          control plane.
        </p>
        <Link
          to="/app/platform-admin/tenants"
          className="mt-3 inline-flex items-center gap-1.5 rounded-md bg-teal-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-teal-500"
        >
          Manage tenants
          <ExternalLink className="h-3.5 w-3.5" aria-hidden />
        </Link>
      </div>
    </div>
  )
}
