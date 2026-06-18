import { describe, expect, it } from 'vitest'
import {
  getProductRouteSlug,
  hasProductEntitlement,
  listEntitledSuiteProducts,
  normalizeProductKey,
  SUITE_PRODUCT_CATALOG,
} from './productCatalog'

describe('productCatalog', () => {
  it('canonicalizes Field Companion product keys only', () => {
    expect(normalizeProductKey('field-companion')).toBe('fieldcompanion')
    expect(normalizeProductKey('field_fieldcompanion')).toBe('fieldfieldcompanion')
    expect(normalizeProductKey('fieldcompanion')).toBe('fieldcompanion')
    expect(getProductRouteSlug('fieldcompanion')).toBe('field-companion')
    expect(getProductRouteSlug('companion')).toBe('companion')
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
      'customarr',
      'ordarr',
      'ledgarr',
      'compliancecore',
      'loadarr',
      'recordarr',
      'reportarr',
      'assurarr',
      'fieldcompanion',
    ])
  })

  it('matches entitlements through canonical keys only', () => {
    expect(hasProductEntitlement(['fieldcompanion'], 'fieldcompanion')).toBe(true)
    expect(hasProductEntitlement(['field-companion'], 'fieldcompanion')).toBe(true)
    expect(hasProductEntitlement(['fieldcompanion'], 'trainarr')).toBe(false)
    expect(listEntitledSuiteProducts(['fieldcompanion']).map((entry) => entry.productKey)).toEqual([
      'fieldcompanion',
    ])
  })
})
