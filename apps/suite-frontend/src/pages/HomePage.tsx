import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { QuickLaunchWidget } from '../components/dashboard/QuickLaunchWidget'
import { SessionInfoWidget } from '../components/dashboard/SessionInfoWidget'
import { TenantContextWidget } from '../components/dashboard/TenantContextWidget'
import { WhatINeedWidget } from '../components/dashboard/WhatINeedWidget'
import { useDashboardData } from '../hooks/useDashboardData'

export function HomePage() {
  const { me, session, entitlements, tenants, navigationProducts, isLoading, error } =
    useDashboardData()

  if (isLoading || !me) {
    return <p className="text-sm text-[var(--color-text-muted)]">Loading your workspace…</p>
  }

  if (error) {
    return (
      <ApiErrorCallout message={getErrorMessage(error, 'Failed to load dashboard.')} />
    )
  }

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <header>
        <h3 className="text-xl font-semibold text-white">Welcome, {me.displayName}</h3>
        <p className="mt-1 text-sm text-slate-400">
          Cross-product overview from NexArr — entitlements, tenant context, and launch paths.
        </p>
      </header>

      <div className="grid gap-4 lg:grid-cols-2">
        <QuickLaunchWidget
          navigationProducts={navigationProducts}
          entitlements={me.entitlements}
        />
        <TenantContextWidget me={me} tenants={tenants} />
        {session && <SessionInfoWidget me={me} session={session} />}
        <WhatINeedWidget
          me={me}
          tenants={tenants}
          entitlements={entitlements}
          navigationProducts={navigationProducts}
        />
      </div>
    </div>
  )
}
