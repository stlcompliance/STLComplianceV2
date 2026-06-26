import type { MeResponse, TenantSummary } from '../../api/types'
import { formatRoleDisplayName } from '@stl/shared-ui'
import { findCurrentTenant, isTenantActive } from '../../lib/dashboard'
import { DashboardCard } from './DashboardCard'

function tenantStatusLabel(status: string | null | undefined): string {
  const normalized = status?.trim().toLowerCase()
  if (normalized === 'active') {
    return 'Enabled'
  }
  if (normalized === 'suspended') {
    return 'Suspended'
  }
  return status ?? 'Unknown'
}

export function TenantContextWidget({
  me,
  tenants,
}: {
  me: MeResponse
  tenants: readonly TenantSummary[]
}) {
  const current = findCurrentTenant(tenants, me.tenantId)
  const active = isTenantActive(current)

  return (
    <DashboardCard title="Tenant context">
      <dl className="space-y-2 text-sm text-[var(--color-text-secondary)]">
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
            Current tenant
          </dt>
          <dd className="mt-0.5 font-medium text-[var(--color-text-primary)]">{me.tenantDisplayName}</dd>
          <dd className="text-xs text-[var(--color-text-muted)]">
            {me.tenantSlug}
            {current?.roleKey ? ` · ${formatRoleDisplayName(current.roleKey)}` : ''}
          </dd>
        </div>
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Status</dt>
          <dd className="mt-0.5">
            <span
              className={
                active
                  ? 'inline-flex rounded-full border border-[var(--color-success-border)] bg-[var(--color-success-bg)] px-2 py-0.5 text-xs font-medium text-[var(--color-success-text)]'
                  : 'inline-flex rounded-full border border-[var(--color-warning-border)] bg-[var(--color-warning-bg)] px-2 py-0.5 text-xs font-medium text-[var(--color-warning-text)]'
              }
            >
              {tenantStatusLabel(current?.status)}
            </span>
          </dd>
        </div>
        {tenants.length > 1 && (
          <div>
            <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
              Memberships
            </dt>
            <dd className="mt-1">
              <ul className="list-disc pl-4 text-xs text-[var(--color-text-muted)]">
                {tenants.map((t) => (
                  <li key={t.tenantId}>
                    {t.displayName}
                    {t.tenantId === me.tenantId ? ' (current)' : ''}
                  </li>
                ))}
              </ul>
            </dd>
          </div>
        )}
      </dl>
    </DashboardCard>
  )
}
