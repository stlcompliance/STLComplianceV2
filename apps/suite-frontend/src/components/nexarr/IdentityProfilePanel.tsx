import { useQuery } from '@tanstack/react-query'
import { ApiErrorCallout, formatRoleDisplayName, getErrorMessage } from '@stl/shared-ui'
import { Building2, Package, ShieldCheck, User } from 'lucide-react'

import * as nexarr from '../../api/nexarrClient'
import type { TenantSummary } from '../../api/types'
import { useAuth } from '../../auth/AuthProvider'
import { isPlatformAdmin } from '../../lib/permissions'

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

function TenantMembershipRow({
  tenant,
  isActive,
}: {
  tenant: TenantSummary
  isActive: boolean
}) {
  return (
    <li
      className={[
        'flex flex-col gap-2 px-4 py-3 sm:flex-row sm:items-center sm:justify-between',
        isActive ? 'bg-[var(--color-accent-soft)]' : '',
      ].join(' ')}
    >
      <div className="min-w-0 space-y-0.5">
        <p className="text-sm font-medium text-[var(--color-text-primary)]">
          {tenant.displayName}
          {isActive ? (
            <span className="ml-2 text-xs font-normal text-[var(--color-accent)]">Current tenant</span>
          ) : null}
        </p>
        <p className="font-mono text-xs text-[var(--color-text-muted)]">{tenant.slug}</p>
      </div>
      <div className="flex shrink-0 flex-wrap items-center gap-2 text-xs">
        <span
          className={`inline-flex rounded-full border px-2 py-0.5 font-medium ${tenantStatusBadgeClass(
            tenant.status,
          )}`}
        >
          {tenantStatusLabel(tenant.status)}
        </span>
        <span className="text-[var(--color-text-muted)]">{formatRoleDisplayName(tenant.roleKey)}</span>
      </div>
    </li>
  )
}

export function IdentityProfilePanel() {
  const { me } = useAuth()

  const tenantsQuery = useQuery({
    queryKey: ['my-tenants', me?.userId],
    queryFn: () => nexarr.getMyTenants(),
    enabled: me !== undefined,
  })

  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId],
    queryFn: () => nexarr.getNavigation(),
    enabled: me !== undefined,
  })

  if (!me) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading profile…</p>
  }

  const tenants = tenantsQuery.data ?? []
  const workspaceProducts = navigationQuery.data?.products ?? []

  return (
    <section aria-labelledby="identity-profile-heading" className="space-y-4">
      <div>
        <h3 id="identity-profile-heading" className="text-xl font-semibold text-[var(--color-text-primary)]">
          Profile
        </h3>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          Your NexArr account and tenant memberships for this platform identity.
        </p>
      </div>

      <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
        <div className="flex gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-[var(--color-bg-surface-elevated)]">
            <User className="h-5 w-5 text-[var(--color-accent)]" aria-hidden />
          </div>
          <dl className="min-w-0 flex-1 space-y-3 text-sm">
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                Display name
              </dt>
              <dd className="mt-0.5 font-medium text-[var(--color-text-primary)]">{me.displayName}</dd>
            </div>
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                Email
              </dt>
              <dd className="mt-0.5 text-[var(--color-text-secondary)]">{me.email}</dd>
            </div>
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                Current workspace
              </dt>
              <dd className="mt-0.5 text-[var(--color-text-secondary)]">
                {me.tenantDisplayName}{' '}
                <span className="font-mono text-xs text-[var(--color-text-muted)]">({me.tenantSlug})</span>
              </dd>
            </div>
            {isPlatformAdmin(me) ? (
              <div className="flex items-center gap-2 text-xs font-medium text-[var(--color-accent)]">
                <ShieldCheck className="h-4 w-4 shrink-0" aria-hidden />
                Platform administrator
              </div>
            ) : null}
          </dl>
        </div>
      </div>

      <div className="space-y-2" data-testid="profile-products-section">
        <div className="flex items-center gap-2">
          <Package className="h-4 w-4 text-[var(--color-text-muted)]" aria-hidden />
          <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">
            Workspace products
            <span className="ml-2 font-normal text-[var(--color-text-muted)]">({me.tenantSlug})</span>
          </h4>
        </div>

        <p className="text-xs text-[var(--color-text-muted)]">
          Products available in your current workspace navigation.
        </p>

        {navigationQuery.isLoading ? (
          <p className="text-sm text-[var(--color-text-muted)]">Loading products…</p>
        ) : null}

        {navigationQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(navigationQuery.error, 'Failed to load products.')}
            onRetry={() => void navigationQuery.refetch()}
            retryLabel="Retry products"
          />
        ) : null}

        {navigationQuery.isSuccess && workspaceProducts.length === 0 ? (
          <p className="text-sm text-[var(--color-text-muted)]">
            Workspace product listings are temporarily unavailable.
          </p>
        ) : null}

        {workspaceProducts.length > 0 ? (
          <ul className="divide-y divide-[var(--color-border-subtle)] rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
            {workspaceProducts.map((product) => (
              <li
                key={product.productKey}
                className="flex items-center justify-between gap-2 px-4 py-3 text-sm"
              >
                <span className="font-medium text-[var(--color-text-primary)]">{product.displayName}</span>
                <span className="text-xs font-medium text-[var(--color-text-muted)]">
                  {product.isCurrent ? 'Current' : `${product.surfaces.length} surfaces`}
                </span>
              </li>
            ))}
          </ul>
        ) : null}
      </div>

      <div className="space-y-2">
        <div className="flex items-center gap-2">
          <Building2 className="h-4 w-4 text-[var(--color-text-muted)]" aria-hidden />
          <h4 className="text-sm font-semibold text-[var(--color-text-primary)]">Tenant memberships</h4>
        </div>

        {tenantsQuery.isLoading ? (
          <p className="text-sm text-[var(--color-text-muted)]">Loading tenant memberships…</p>
        ) : null}

        {tenantsQuery.isError ? (
          <ApiErrorCallout
            message={getErrorMessage(tenantsQuery.error, 'Failed to load tenant memberships.')}
            onRetry={() => void tenantsQuery.refetch()}
            retryLabel="Retry tenant memberships"
          />
        ) : null}

        {tenantsQuery.isSuccess && tenants.length === 0 ? (
          <p className="text-sm text-[var(--color-text-muted)]">No tenant memberships found.</p>
        ) : null}

        {tenants.length > 0 ? (
          <ul className="divide-y divide-[var(--color-border-subtle)] rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)]">
            {tenants.map((tenant) => (
              <TenantMembershipRow
                key={tenant.tenantId}
                tenant={tenant}
                isActive={tenant.tenantId === me.tenantId}
              />
            ))}
          </ul>
        ) : null}

        {tenants.length > 1 ? (
          <p className="text-xs text-[var(--color-text-muted)]">
            To switch workspaces, sign out and sign in with the desired tenant.
          </p>
        ) : null}
      </div>
    </section>
  )
}
