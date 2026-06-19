import type { MeResponse, TenantSummary } from '../../api/types'
import { findCurrentTenant, isTenantActive } from '../../lib/dashboard'
import { DashboardCard } from './DashboardCard'

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
      <dl className="space-y-2 text-sm text-slate-300">
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
            Active tenant
          </dt>
          <dd className="mt-0.5 font-medium text-white">{me.tenantDisplayName}</dd>
          <dd className="text-xs text-slate-400">
            {me.tenantSlug}
            {current?.roleKey ? ` · ${current.roleKey.replace(/_/g, ' ')}` : ''}
          </dd>
        </div>
        <div>
          <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">Status</dt>
          <dd className="mt-0.5">
            <span
              className={
                active
                  ? 'inline-flex rounded-full bg-emerald-950/50 px-2 py-0.5 text-xs font-medium text-emerald-300'
                  : 'inline-flex rounded-full bg-amber-950/50 px-2 py-0.5 text-xs font-medium text-amber-300'
              }
            >
              {current?.status ?? 'Unknown'}
            </span>
          </dd>
        </div>
        {tenants.length > 1 && (
          <div>
            <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
              Memberships
            </dt>
            <dd className="mt-1">
              <ul className="list-disc pl-4 text-xs text-slate-400">
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
