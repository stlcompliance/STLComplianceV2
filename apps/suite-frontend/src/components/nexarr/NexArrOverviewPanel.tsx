import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Building2, LayoutDashboard, ShieldCheck, User } from 'lucide-react'
import { ApiErrorCallout, formatRoleDisplayName, getErrorMessage } from '@stl/shared-ui'

import * as nexarr from '../../api/nexarrClient'
import { useAuth } from '../../auth/AuthProvider'
import { DashboardCard } from '../dashboard/DashboardCard'
import { findCurrentTenant, isTenantActive } from '../../lib/dashboard'
import { countActiveSessions, listEnabledSurfaces } from '../../lib/nexarrOverview'
import { isPlatformAdmin } from '../../lib/permissions'
import {
  buildProductSurfacePath,
  findNavigationProduct,
} from '../../navigation/suiteNavigation'

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

export function NexArrOverviewPanel() {
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

  const sessionsQuery = useQuery({
    queryKey: ['my-sessions'],
    queryFn: () => nexarr.getMySessions(),
    enabled: me !== undefined,
  })

  if (!me) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading platform overview…</p>
  }

  const isLoading =
    tenantsQuery.isLoading ||
    navigationQuery.isLoading ||
    sessionsQuery.isLoading

  const error =
    tenantsQuery.error ??
    navigationQuery.error ??
    sessionsQuery.error ??
    null

  if (isLoading) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading platform overview…</p>
  }

  if (error) {
    const retry = () => {
      void tenantsQuery.refetch()
      void navigationQuery.refetch()
      void sessionsQuery.refetch()
    }
    return (
      <ApiErrorCallout
        message={getErrorMessage(error, 'Failed to load platform overview.')}
        onRetry={retry}
        retryLabel="Retry overview"
      />
    )
  }

  const tenants = tenantsQuery.data ?? []
  const workspaceProducts = navigationQuery.data?.products ?? []
  const currentTenant = findCurrentTenant(tenants, me.tenantId)
  const tenantActive = isTenantActive(currentTenant)
  const nexarrProduct = findNavigationProduct(navigationQuery.data?.products ?? [], 'nexarr')
  const surfaces = listEnabledSurfaces(nexarrProduct?.surfaces ?? [])
  const activeSessionCount = countActiveSessions(sessionsQuery.data?.sessions ?? [])

  return (
    <div className="max-w-5xl space-y-6" data-testid="nexarr-overview-panel">
      <header>
        <h3 className="text-xl font-semibold text-[var(--color-text-primary)]">Platform overview</h3>
        <p className="mt-1 text-sm text-[var(--color-text-muted)]">
          NexArr overview for your account, tenant workspace, and available suite products.
        </p>
      </header>

      <div className="grid gap-4 lg:grid-cols-2">
        <DashboardCard title="Account & workspace">
          <div className="flex gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-[var(--color-bg-surface-elevated)]">
              <User className="h-5 w-5 text-[var(--color-accent)]" aria-hidden />
            </div>
            <dl className="min-w-0 flex-1 space-y-3 text-sm">
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                  Signed in as
                </dt>
                <dd className="mt-0.5 font-medium text-[var(--color-text-primary)]">{me.displayName}</dd>
                <dd className="text-xs text-[var(--color-text-muted)]">{me.email}</dd>
              </div>
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                  Current workspace
                </dt>
                <dd className="mt-0.5 text-[var(--color-text-secondary)]">
                  {me.tenantDisplayName}{' '}
                  <span className="font-mono text-xs text-[var(--color-text-muted)]">({me.tenantSlug})</span>
                </dd>
                {currentTenant?.roleKey ? (
                  <dd className="mt-0.5 text-xs text-[var(--color-text-muted)]">
                    {formatRoleDisplayName(currentTenant.roleKey)}
                  </dd>
                ) : null}
              </div>
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                  Tenant status
                </dt>
                <dd className="mt-0.5">
                  <span
                    className={
                      tenantActive
                        ? 'inline-flex rounded-full border border-[var(--color-success-border)] bg-[var(--color-success-bg)] px-2 py-0.5 text-xs font-medium text-[var(--color-success-text)]'
                        : 'inline-flex rounded-full border border-[var(--color-warning-border)] bg-[var(--color-warning-bg)] px-2 py-0.5 text-xs font-medium text-[var(--color-warning-text)]'
                    }
                  >
                    {tenantStatusLabel(currentTenant?.status)}
                  </span>
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
        </DashboardCard>

        <DashboardCard title="Suite products">
          {workspaceProducts.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">
              Workspace product listings are temporarily unavailable in navigation.
            </p>
          ) : (
            <ul className="space-y-2">
              {workspaceProducts.map((product) => (
                <li
                  key={product.productKey}
                  className="flex items-center justify-between gap-2 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm"
                >
                  <span className="font-medium text-[var(--color-text-primary)]">{product.displayName}</span>
                  <span className="text-xs font-medium text-[var(--color-text-muted)]">
                    {product.isCurrent ? 'Current' : `${listEnabledSurfaces(product.surfaces).length} surfaces`}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </DashboardCard>

        <DashboardCard title="NexArr surfaces">
          {surfaces.length === 0 ? (
            <p className="text-sm text-[var(--color-text-muted)]">No in-suite surfaces are enabled.</p>
          ) : (
            <ul className="space-y-2">
              {surfaces.map((surface) => {
                const href = nexarrProduct
                  ? buildProductSurfacePath(nexarrProduct.productKey, surface)
                  : '/app/nexarr'
                const isCurrent = surface.surfaceKey === 'overview'
                const surfaceLabel =
                  surface.surfaceKey === 'identity' ? 'Identity & sessions' : surface.label
                return (
                  <li key={surface.surfaceKey}>
                    {isCurrent ? (
                      <div className="flex items-center gap-2 rounded-md border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] px-3 py-2 text-sm text-[var(--color-accent)]">
                        <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
                        <span className="font-medium">{surfaceLabel}</span>
                        <span className="ml-auto text-xs text-[var(--color-accent)]/80">Current</span>
                      </div>
                    ) : (
                      <Link
                        to={href}
                        className="flex items-center gap-2 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2 text-sm font-medium text-[var(--color-text-primary)] transition-colors hover:border-[var(--color-accent-border)] hover:text-[var(--color-accent)]"
                      >
                        {surface.surfaceKey === 'identity' ? (
                          <ShieldCheck className="h-4 w-4 shrink-0" aria-hidden />
                        ) : surface.surfaceKey === 'tenants' ? (
                          <Building2 className="h-4 w-4 shrink-0" aria-hidden />
                        ) : (
                          <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
                        )}
                        {surfaceLabel}
                      </Link>
                    )}
                  </li>
                )
              })}
            </ul>
          )}
        </DashboardCard>

        <DashboardCard title="Security snapshot">
          <dl className="space-y-3 text-sm text-[var(--color-text-secondary)]">
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                Session activity
              </dt>
              <dd className="mt-0.5 font-medium text-[var(--color-text-primary)]">{activeSessionCount}</dd>
            </div>
            {tenants.length > 1 ? (
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                  Tenant memberships
                </dt>
                <dd className="mt-0.5 text-[var(--color-text-secondary)]">{tenants.length}</dd>
              </div>
            ) : null}
          </dl>
          <Link
            to="/app/nexarr/identity"
            className="mt-4 inline-flex text-xs font-medium text-[var(--color-accent)] hover:text-[var(--color-accent-strong)]"
          >
            Manage identity & sessions
          </Link>
        </DashboardCard>

        {isPlatformAdmin(me) ? (
          <DashboardCard title="Platform administration" className="lg:col-span-2">
            <p className="text-sm text-[var(--color-text-muted)]">
              Tenant lifecycle, launch diagnostics, destination health, and suite-wide controls live
              in the platform admin workspace.
            </p>
            <Link
              to="/app/platform-admin"
              className="mt-3 inline-flex rounded-md bg-[var(--color-accent)] px-3 py-1.5 text-xs font-medium text-[var(--color-button-primary-text)] transition-colors hover:bg-[var(--color-accent-strong)]"
            >
              Open platform admin
            </Link>
          </DashboardCard>
        ) : null}
      </div>
    </div>
  )
}
