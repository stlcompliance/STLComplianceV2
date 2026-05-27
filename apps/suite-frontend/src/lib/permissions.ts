import type { LaunchContextResponse, MeResponse } from '../api/types'

export function hasProductEntitlement(
  entitlements: readonly string[],
  productKey: string,
): boolean {
  const normalized = productKey.trim().toLowerCase()
  return entitlements.some((e) => e.trim().toLowerCase() === normalized)
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
  const path = `/app/${productKey.trim().toLowerCase()}`
  return `${window.location.origin}${path}`
}
