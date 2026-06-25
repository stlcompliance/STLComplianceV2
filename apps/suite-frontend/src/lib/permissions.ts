import type { LaunchContextResponse, MeResponse } from '../api/types'
import { getProductRouteSlug, normalizeProductKey } from '@stl/shared-ui/productCatalog'

export function canLaunchFromContext(context: LaunchContextResponse): boolean {
  return context.canLaunch
}

export function isPlatformAdmin(me: MeResponse | undefined): boolean {
  return me?.isPlatformAdmin === true
}

/** In-suite products stay on client routes; others use NexArr handoff to product base URL. */
export function isInSuiteProduct(productKey: string): boolean {
  return normalizeProductKey(productKey) === 'nexarr'
}

export function buildProductCallbackUrl(productKey: string): string {
  const path = `/app/${getProductRouteSlug(productKey)}`
  return `${window.location.origin}${path}`
}
