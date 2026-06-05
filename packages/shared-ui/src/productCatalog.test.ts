import { describe, expect, it } from 'vitest'
import {
  getProductRouteSlug,
  hasProductEntitlement,
  listEntitledSuiteProducts,
  normalizeProductKey,
  SUITE_PRODUCT_CATALOG,
  toLegacyProductKey,
} from './productCatalog'

describe('productCatalog', () => {
  it('canonicalizes Field Companion while preserving the backend legacy key', () => {
    expect(normalizeProductKey('Companion')).toBe('fieldcompanion')
    expect(normalizeProductKey('field-companion')).toBe('fieldcompanion')
    expect(normalizeProductKey('field_companion')).toBe('fieldcompanion')
    expect(toLegacyProductKey('fieldcompanion')).toBe('companion')
    expect(toLegacyProductKey('staffarr')).toBe('staffarr')
    expect(getProductRouteSlug('companion')).toBe('field-companion')
  })

  it('lists implemented constitution products without future-only products', () => {
    const keys = SUITE_PRODUCT_CATALOG.map((entry) => entry.productKey)

    expect(keys).toEqual([
      'nexarr',
      'staffarr',
      'trainarr',
      'maintainarr',
      'routarr',
      'supplyarr',
      'compliancecore',
      'loadarr',
      'recordarr',
      'reportarr',
      'assurarr',
      'fieldcompanion',
    ])
    expect(keys).not.toContain('customarr')
    expect(keys).not.toContain('ordarr')
  })

  it('matches entitlements through canonical and legacy keys', () => {
    expect(hasProductEntitlement(['companion'], 'fieldcompanion')).toBe(true)
    expect(hasProductEntitlement(['field-companion'], 'companion')).toBe(true)
    expect(listEntitledSuiteProducts(['companion']).map((entry) => entry.productKey)).toEqual([
      'fieldcompanion',
    ])
  })
})
