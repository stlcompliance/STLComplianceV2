import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { Building2, LayoutDashboard, ShieldCheck, User } from 'lucide-react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

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
    return <p className="text-sm text-slate-400">Loading platform overview…</p>
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
    return <p className="text-sm text-slate-400">Loading platform overview…</p>
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
        <h3 className="text-xl font-semibold text-white">Platform overview</h3>
        <p className="mt-1 text-sm text-slate-400">
          NexArr control center for your account, tenant workspace, and available suite products.
        </p>
      </header>

      <div className="grid gap-4 lg:grid-cols-2">
        <DashboardCard title="Account & workspace">
          <div className="flex gap-3">
            <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-slate-800">
              <User className="h-5 w-5 text-teal-400" aria-hidden />
            </div>
            <dl className="min-w-0 flex-1 space-y-3 text-sm">
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                  Signed in as
                </dt>
                <dd className="mt-0.5 font-medium text-white">{me.displayName}</dd>
                <dd className="text-xs text-slate-400">{me.email}</dd>
              </div>
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                  Current workspace
                </dt>
                <dd className="mt-0.5 text-slate-300">
                  {me.tenantDisplayName}{' '}
                  <span className="font-mono text-xs text-[var(--color-text-muted)]">({me.tenantSlug})</span>
                </dd>
                {currentTenant?.roleKey ? (
                  <dd className="mt-0.5 text-xs capitalize text-slate-400">
                    {currentTenant.roleKey.replace(/_/g, ' ')}
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
                        ? 'inline-flex rounded-full bg-emerald-950/50 px-2 py-0.5 text-xs font-medium text-emerald-300'
                        : 'inline-flex rounded-full bg-amber-950/50 px-2 py-0.5 text-xs font-medium text-amber-300'
                    }
                  >
                    {tenantStatusLabel(currentTenant?.status)}
                  </span>
                </dd>
              </div>
              {isPlatformAdmin(me) ? (
                <div className="flex items-center gap-2 text-xs font-medium text-teal-400">
                  <ShieldCheck className="h-4 w-4 shrink-0" aria-hidden />
                  Platform administrator
                </div>
              ) : null}
            </dl>
          </div>
        </DashboardCard>

        <DashboardCard title="Suite products">
          {workspaceProducts.length === 0 ? (
            <p className="text-sm text-slate-400">
              No suite products are available in this workspace.
            </p>
          ) : (
            <ul className="space-y-2">
              {workspaceProducts.map((product) => (
                <li
                  key={product.productKey}
                  className="flex items-center justify-between gap-2 rounded-md border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm"
                >
                  <span className="font-medium text-white">{product.displayName}</span>
                  <span className="text-xs font-medium text-slate-400">
                    {product.isCurrent ? 'Current' : `${listEnabledSurfaces(product.surfaces).length} surfaces`}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </DashboardCard>

        <DashboardCard title="NexArr surfaces">
          {surfaces.length === 0 ? (
            <p className="text-sm text-slate-400">No in-suite surfaces are enabled.</p>
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
                      <div className="flex items-center gap-2 rounded-md border border-teal-800/50 bg-teal-950/20 px-3 py-2 text-sm text-teal-200">
                        <LayoutDashboard className="h-4 w-4 shrink-0" aria-hidden />
                        <span className="font-medium">{surfaceLabel}</span>
                        <span className="ml-auto text-xs text-teal-400/80">Current</span>
                      </div>
                    ) : (
                      <Link
                        to={href}
                        className="flex items-center gap-2 rounded-md border border-slate-700 bg-slate-950/40 px-3 py-2 text-sm font-medium text-slate-100 hover:border-teal-700/50 hover:text-teal-300"
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
          <dl className="space-y-3 text-sm text-slate-300">
            <div>
              <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                Session activity
              </dt>
              <dd className="mt-0.5 font-medium text-white">{activeSessionCount}</dd>
            </div>
            {tenants.length > 1 ? (
              <div>
                <dt className="text-xs font-medium uppercase tracking-wide text-[var(--color-text-muted)]">
                  Tenant memberships
                </dt>
                <dd className="mt-0.5 text-slate-300">{tenants.length}</dd>
              </div>
            ) : null}
          </dl>
          <Link
            to="/app/nexarr/identity"
            className="mt-4 inline-flex text-xs font-medium text-teal-400 hover:text-teal-300"
          >
            Manage identity & sessions
          </Link>
        </DashboardCard>

        {isPlatformAdmin(me) ? (
          <DashboardCard title="Platform administration" className="lg:col-span-2">
            <p className="text-sm text-slate-400">
              Tenant lifecycle, launch diagnostics, availability, and suite-wide health live in the
              platform admin control plane.
            </p>
            <Link
              to="/app/platform-admin"
              className="mt-3 inline-flex rounded-md bg-teal-600 px-3 py-1.5 text-xs font-medium text-white hover:bg-teal-500"
            >
              Open platform admin
            </Link>
          </DashboardCard>
        ) : null}
      </div>
    </div>
  )
}
