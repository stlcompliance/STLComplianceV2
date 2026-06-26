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
    return 'border-[var(--color-success-border)] bg-[var(--color-success-bg)] text-[var(--color-success-text)]'
  }
  if (normalized === 'suspended') {
    return 'border-[var(--color-warning-border)] bg-[var(--color-warning-bg)] text-[var(--color-warning-text)]'
  }
  return 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] text-[var(--color-text-muted)]'
}

function tenantStatusLabel(status: string): string {
  const normalized = status.trim().toLowerCase()
  if (normalized === 'active') {
    return 'Enabled'
  }
  if (normalized === 'suspended') {
    return 'Suspended'
  }
  return status
}

function countActiveTenants(tenants: readonly TenantOverviewRow[]): number {
  return tenants.filter((tenant) => tenant.status.trim().toLowerCase() === 'active').length
}

function getLaunchContextCount(tenant: TenantOverviewRow): number {
  return tenant.launchableDestinationCount
}

export function NexArrTenantsPanel() {
  const { me } = useAuth()

  const overviewQuery = useQuery({
    queryKey: ['nexarr-tenants-overview'],
    queryFn: () => nexarr.getPlatformAdminTenantOverview(1, 100),
    enabled: me !== undefined,
  })

  if (!me) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading tenant registry…</p>
  }

  if (overviewQuery.isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading tenant registry…</p>
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
        <h3 className="text-xl font-semibold text-[var(--color-text-primary)]">Tenant registry</h3>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Platform-wide tenant catalog from NexArr. Create and update tenants in platform
          administration.
        </p>
      </header>

      <div className="grid gap-4 sm:grid-cols-2">
        <DashboardCard title="Total tenants">
          <p className="text-2xl font-semibold text-[var(--color-text-primary)]">{tenants.length}</p>
        </DashboardCard>
        <DashboardCard title="Active tenants">
          <p className="text-2xl font-semibold text-[var(--color-success-text)]">{activeCount}</p>
        </DashboardCard>
      </div>

      {tenants.length === 0 ? (
        <p className="text-sm text-[var(--color-text-muted)]">No tenants registered on this platform.</p>
      ) : (
        <div className="overflow-x-auto rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-[var(--color-border-subtle)] text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
              <tr>
                <th className="px-4 py-3 font-medium">Tenant</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">Launch contexts</th>
                <th className="px-4 py-3 font-medium">Members</th>
                <th className="px-4 py-3 font-medium">Created</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-[var(--color-border-subtle)]">
              {tenants.map((tenant) => {
                const launchContextCount = getLaunchContextCount(tenant)

                return (
                  <tr key={tenant.tenantId}>
                    <td className="px-4 py-3">
                      <div className="flex items-center gap-2">
                        <Building2 className="h-4 w-4 shrink-0 text-[var(--color-text-muted)]" aria-hidden />
                        <div>
                          <p className="font-medium text-[var(--color-text-primary)]">{tenant.displayName}</p>
                          <p className="font-mono text-xs text-[var(--color-text-muted)]">{tenant.slug}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <span
                        className={`inline-flex rounded-full border px-2 py-0.5 text-xs font-medium capitalize ${tenantStatusBadgeClass(tenant.status)}`}
                      >
                        {tenantStatusLabel(tenant.status)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-[var(--color-text-secondary)]">{launchContextCount}</td>
                    <td className="px-4 py-3 text-[var(--color-text-secondary)]">{tenant.membershipCount}</td>
                    <td className="px-4 py-3 text-[var(--color-text-muted)]">
                      {new Date(tenant.createdAt).toLocaleDateString()}
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <p className="text-sm text-[var(--color-text-muted)]">
          Create tenants, update display names, and manage lifecycle status from the platform admin
          workspace.
        </p>
        <Link
          to="/app/platform-admin/tenants"
          className="mt-3 inline-flex items-center gap-1.5 rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-xs font-medium text-[var(--color-button-primary-text)] transition-colors hover:bg-[var(--color-accent-strong)]"
        >
          Manage tenants
          <ExternalLink className="h-3.5 w-3.5" aria-hidden />
        </Link>
      </div>
    </div>
  )
}
