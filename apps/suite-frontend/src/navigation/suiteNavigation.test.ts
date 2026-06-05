import { describe, expect, it } from 'vitest'
import {
  buildProductSurfacePath,
  findNavigationProduct,
  getProductDisplayName,
  isLaunchSurface,
  normalizeProductKey,
  resolveActiveSurface,
} from './suiteNavigation'
import type { NavigationSurfaceItem } from '../api/types'

const surfaces: NavigationSurfaceItem[] = [
  {
    surfaceKey: 'overview',
    label: 'Overview',
    relativePath: '',
    iconKey: 'dashboard',
    sortOrder: 0,
    isEnabled: true,
    permissionHint: null,
  },
  {
    surfaceKey: 'dispatch',
    label: 'Dispatch',
    relativePath: 'dispatch',
    iconKey: 'fleet',
    sortOrder: 10,
    isEnabled: true,
    permissionHint: null,
  },
  {
    surfaceKey: 'launch',
    label: 'Open app',
    relativePath: 'launch',
    iconKey: 'fleet',
    sortOrder: 90,
    isEnabled: true,
    permissionHint: 'Opens dedicated workspace',
  },
]

describe('suiteNavigation', () => {
  it('builds overview and nested surface paths', () => {
    expect(buildProductSurfacePath('routarr', surfaces[0])).toBe('/app/routarr')
    expect(buildProductSurfacePath('routarr', surfaces[1])).toBe('/app/routarr/dispatch')
  })

  it('resolves active surface from route segment', () => {
    expect(resolveActiveSurface(surfaces, 'dispatch')?.surfaceKey).toBe('dispatch')
    expect(resolveActiveSurface(surfaces, undefined)?.surfaceKey).toBe('overview')
  })

  it('detects launch surfaces', () => {
    expect(isLaunchSurface(surfaces[2])).toBe(true)
    expect(isLaunchSurface(surfaces[0])).toBe(false)
  })

  it('finds navigation product by key', () => {
    const product = findNavigationProduct(
      [
        {
          productKey: 'staffarr',
          displayName: 'StaffArr',
          routePath: '/app/staffarr',
          sortOrder: 1,
          surfaces,
        },
      ],
      'StaffArr',
    )

    expect(product?.productKey).toBe('staffarr')
  })

  it('canonicalizes the legacy fieldcompanion alias', () => {
    expect(normalizeProductKey('fieldcompanion')).toBe('fieldcompanion')
    expect(normalizeProductKey('field-companion')).toBe('fieldcompanion')
    expect(buildProductSurfacePath('fieldcompanion', surfaces[0])).toBe('/app/field-companion')
    expect(buildProductSurfacePath('field-companion', surfaces[1])).toBe('/app/field-companion/dispatch')
    expect(getProductDisplayName('fieldcompanion', 'fieldcompanion App')).toBe('Field Companion')
  })
})
