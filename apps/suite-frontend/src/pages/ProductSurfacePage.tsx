import { useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { PermissionGate } from '../components/PermissionGate'
import { useLaunchContextGate } from '../components/ProductSwitcher'
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
    return <p className="text-sm text-slate-600">Select a surface from the product navigation.</p>
  }

  if (!surface.isEnabled) {
    return (
      <p className="text-sm text-red-700" role="alert">
        {surface.permissionHint ?? 'You do not have access to this surface.'}
      </p>
    )
  }

  if (isLaunchSurface(surface)) {
    return (
      <div className="max-w-2xl space-y-4">
        <h3 className="text-xl font-semibold text-stl-navy">{surface.label}</h3>
        {surface.permissionHint && (
          <p className="text-sm text-slate-600">{surface.permissionHint}</p>
        )}
        {contextQuery.data && (
          <div className="rounded-lg border border-slate-200 bg-white p-4 text-sm">
            <p>
              <span className="font-medium">Launch URL:</span> {contextQuery.data.launchUrl || '—'}
            </p>
            <p className="mt-2">
              <span className="font-medium">Can launch:</span>{' '}
              {contextQuery.data.canLaunch ? 'Yes' : 'No'}
              {contextQuery.data.denialReasonCode
                ? ` (${contextQuery.data.denialReasonCode})`
                : ''}
            </p>
          </div>
        )}
        <PermissionGate allowed={launchAllowedQuery.data === true}>
          <button
            type="button"
            disabled={launch.isPending}
            onClick={() => launch.mutate(normalized)}
            className="rounded-md bg-stl-teal px-4 py-2 text-sm font-medium text-white hover:bg-stl-teal/90 disabled:opacity-60"
          >
            {launch.isPending ? 'Launching…' : 'Launch product (handoff)'}
          </button>
        </PermissionGate>
      </div>
    )
  }

  return (
    <div className="max-w-3xl space-y-3">
      <h3 className="text-xl font-semibold text-stl-navy">{surface.label}</h3>
      <p className="text-sm text-slate-700">
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
