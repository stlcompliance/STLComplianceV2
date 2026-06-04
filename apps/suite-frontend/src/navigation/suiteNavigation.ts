import type { NavigationItem, NavigationSurfaceItem } from '../api/types'
import { getProductRouteSlug } from '@stl/shared-ui'

export function normalizeProductKey(productKey: string): string {
  const normalized = productKey.trim().toLowerCase().replace(/[-_]/g, '')
  return normalized === 'companion' ? 'fieldcompanion' : normalized
}

export function getProductDisplayName(productKey: string, fallback?: string): string {
  const normalized = normalizeProductKey(productKey)
  if (normalized === 'fieldcompanion') {
    return 'Field Companion'
  }
  return fallback?.trim() || normalized
}

export function findNavigationProduct(
  products: readonly NavigationItem[],
  productKey: string,
): NavigationItem | undefined {
  const normalized = normalizeProductKey(productKey)
  return products.find((p) => normalizeProductKey(p.productKey) === normalized)
}

export function buildProductSurfacePath(productKey: string, surface: NavigationSurfaceItem): string {
  const base = `/app/${getProductRouteSlug(productKey)}`
  if (!surface.relativePath) {
    return base
  }

  return `${base}/${surface.relativePath}`
}

export function resolveActiveSurface(
  surfaces: readonly NavigationSurfaceItem[],
  surfaceSegment: string | undefined,
): NavigationSurfaceItem | undefined {
  if (!surfaceSegment) {
    return surfaces.find((s) => s.surfaceKey === 'overview') ?? surfaces[0]
  }

  const normalized = surfaceSegment.trim().toLowerCase()
  return (
    surfaces.find((s) => s.relativePath.toLowerCase() === normalized) ??
    surfaces.find((s) => s.surfaceKey.toLowerCase() === normalized)
  )
}

export function isLaunchSurface(surface: NavigationSurfaceItem): boolean {
  return surface.surfaceKey === 'launch' || surface.relativePath === 'launch'
}
