import { describe, expect, it } from 'vitest'
import { getMarketingProduct, MARKETING_PRODUCTS, productPagePath } from './products'
import { MARKETING_PRODUCT_KEYS } from '../lib/publicRoutes'

const removedPublicProductKey = 'nex' + 'arr'

describe('MARKETING_PRODUCTS', () => {
  it('includes all suite products with unique keys', () => {
    const keys = MARKETING_PRODUCTS.map((p) => p.productKey)
    expect(new Set(keys).size).toBe(keys.length)
    expect(keys).not.toContain(removedPublicProductKey)
    expect(keys).toContain('customarr')
    expect(keys).toContain('ordarr')
    expect(keys).toContain('compliancecore')
  })

  it('keeps public marketing routes aligned to marketed products', () => {
    expect([...MARKETING_PRODUCT_KEYS].sort()).toEqual(
      [...MARKETING_PRODUCTS.map((product) => product.productKey)].sort(),
    )
  })

  it('resolves product by key case-insensitively', () => {
    expect(getMarketingProduct('TrainArr')?.displayName).toBe('TrainArr')
  })

  it('assigns product categories', () => {
    expect(getMarketingProduct('staffarr')?.category).toBe('workforce')
    expect(getMarketingProduct('fieldcompanion')?.category).toBe('field')
    expect(getMarketingProduct('field-companion')?.category).toBe('field')
    expect(getMarketingProduct('fieldcompanion')?.category).toBe('field')
    expect(MARKETING_PRODUCTS.every((p) => p.tagline.length > 0)).toBe(true)
  })

  it('documents direct cross-product dependencies in the checklist', () => {
    const maintainarr = getMarketingProduct('maintainarr')!
    const routarr = getMarketingProduct('routarr')!

    expect(maintainarr.checklist.training).toBe('connected')
    expect(maintainarr.connectedReasons.training).toMatch(/qualification/i)
    expect(routarr.checklist.warehouse).toBe('connected')
    expect(routarr.connectedReasons.warehouse).toMatch(/stock|load/i)
    expect(getMarketingProduct('ordarr')!.checklist.customer).toBe('connected')
    expect(getMarketingProduct('customarr')!.checklist.orders).toBe('connected')
  })

  it('keeps legacy fieldcompanion links on the canonical Field Companion page', () => {
    expect(productPagePath('fieldcompanion')).toBe('/products/field-companion')
    expect(productPagePath('field-companion')).toBe('/products/field-companion')
  })
})
