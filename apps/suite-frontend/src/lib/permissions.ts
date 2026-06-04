import type { LaunchContextResponse, MeResponse } from '../api/types'
import { getProductRouteSlug } from '@stl/shared-ui'

export function hasProductEntitlement(
  entitlements: readonly string[],
  productKey: string,
): boolean {
  const normalized = productKey.trim().toLowerCase()
  const canonical = normalized === 'companion' ? 'fieldcompanion' : normalized
  return entitlements.some((e) => {
    const entitlement = e.trim().toLowerCase()
    return (entitlement === 'companion' ? 'fieldcompanion' : entitlement) === canonical
  })
}

export function canAccessProductRoute(
  entitlements: readonly string[],
  productKey: string,
): boolean {
  return hasProductEntitlement(entitlements, productKey)
}

export function canLaunchFromContext(context: LaunchContextResponse): boolean {
  return context.canLaunch
}

export function isPlatformAdmin(me: MeResponse | undefined): boolean {
  return me?.isPlatformAdmin === true
}

/** In-suite products stay on client routes; others use NexArr handoff to product base URL. */
export function isInSuiteProduct(productKey: string): boolean {
  return productKey.trim().toLowerCase() === 'nexarr'
}

export function buildProductCallbackUrl(productKey: string): string {
  const path = `/app/${getProductRouteSlug(productKey)}`
  return `${window.location.origin}${path}`
}
