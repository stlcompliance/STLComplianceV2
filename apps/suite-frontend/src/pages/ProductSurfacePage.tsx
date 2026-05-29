import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { PermissionGate } from '../components/PermissionGate'
import { IdentityAccessPanel } from '../components/nexarr/IdentityAccessPanel'
import { LaunchFailurePanel } from '../components/nexarr/LaunchFailurePanel'
import { NexArrOverviewPanel } from '../components/nexarr/NexArrOverviewPanel'
import { NexArrTenantsPanel } from '../components/nexarr/NexArrTenantsPanel'
import { useLaunchContextGate } from '../hooks/useLaunchContextGate'
import { useProductLaunch } from '../hooks/useProductLaunch'
import {
  findNavigationProduct,
  isLaunchSurface,
  normalizeProductKey,
  resolveActiveSurface,
} from '../navigation/suiteNavigation'
import { isInSuiteProduct } from '../lib/permissions'

export function ProductSurfacePage() {
  const { productKey = '', surfaceKey = '' } = useParams<{ productKey: string; surfaceKey?: string }>()
  const { me } = useAuth()
  const normalized = normalizeProductKey(productKey)
  const launch = useProductLaunch()
  const launchAllowedQuery = useLaunchContextGate(normalized)

  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId],
    queryFn: () => nexarr.getNavigation(),
    enabled: Boolean(me),
  })

  const product = findNavigationProduct(navigationQuery.data?.products ?? [], normalized)
  const surface = product ? resolveActiveSurface(product.surfaces, surfaceKey) : undefined

  const contextQuery = useQuery({
    queryKey: ['launch-context-detail', normalized, me?.tenantId],
    queryFn: () => nexarr.getLaunchContext(normalized),
    enabled: Boolean(me) && Boolean(surface && isLaunchSurface(surface)) && !isInSuiteProduct(normalized),
  })

  if (!surface) {
    return <p className="text-sm text-slate-400">Select a surface from the product navigation.</p>
  }

  if (!surface.isEnabled) {
    return (
      <p className="text-sm text-red-700" role="alert">
        {surface.permissionHint ?? 'You do not have access to this surface.'}
      </p>
    )
  }

  if (normalized === 'nexarr' && surface.surfaceKey === 'overview') {
    return <NexArrOverviewPanel />
  }

  if (normalized === 'nexarr' && surface.surfaceKey === 'identity') {
    return <IdentityAccessPanel />
  }

  if (normalized === 'nexarr' && surface.surfaceKey === 'tenants') {
    return <NexArrTenantsPanel />
  }

  if (isLaunchSurface(surface)) {
    const launchContext = contextQuery.data
    const launchDenied = launchContext?.canLaunch === false

    return (
      <div className="max-w-2xl space-y-4">
        <h3 className="text-xl font-semibold text-white">{surface.label}</h3>
        {surface.permissionHint && (
          <p className="text-sm text-slate-400">{surface.permissionHint}</p>
        )}

        {launchDenied && launchContext && (
          <LaunchFailurePanel
            productDisplayName={launchContext.productDisplayName}
            productKey={normalized}
            context={launchContext}
          />
        )}

        {launchContext?.canLaunch && (
          <div className="rounded-lg border border-slate-700 bg-slate-900/60 p-4 text-sm text-slate-300">
            <p>
              <span className="font-medium">Launch URL:</span> {launchContext.launchUrl || '—'}
            </p>
            <p className="mt-2 text-emerald-300">
              <span className="font-medium text-slate-300">Status:</span> Ready to launch via NexArr
              handoff
            </p>
          </div>
        )}

        {launch.isError && (
          <p className="rounded-lg border border-red-800/60 bg-red-950/30 p-4 text-sm text-red-200" role="alert">
            {(launch.error as Error).message}
          </p>
        )}

        <PermissionGate allowed={launchAllowedQuery.data === true && !launchDenied}>
          <button
            type="button"
            disabled={launch.isPending || launchDenied}
            onClick={() => launch.mutate(normalized)}
            className="rounded-md bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-60"
          >
            {launch.isPending ? 'Launching…' : 'Launch product (handoff)'}
          </button>
        </PermissionGate>
      </div>
    )
  }

  return (
    <div className="max-w-3xl space-y-3">
      <h3 className="text-xl font-semibold text-white">{surface.label}</h3>
      <p className="text-sm text-slate-300">
        This in-suite surface is wired to NexArr navigation and entitlements. Product-specific
        workflows launch in the dedicated {product?.displayName ?? normalized} application when
        shipped.
      </p>
      {surface.permissionHint && (
        <p className="text-xs text-slate-500">{surface.permissionHint}</p>
      )}
    </div>
  )
}
