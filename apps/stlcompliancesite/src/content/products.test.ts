import { describe, expect, it } from 'vitest'
import { getMarketingProduct, MARKETING_PRODUCTS } from './products'

describe('MARKETING_PRODUCTS', () => {
  it('includes all suite products with unique keys', () => {
    const keys = MARKETING_PRODUCTS.map((p) => p.productKey)
    expect(new Set(keys).size).toBe(keys.length)
    expect(keys).toContain('nexarr')
    expect(keys).toContain('compliancecore')
  })

  it('resolves product by key case-insensitively', () => {
    expect(getMarketingProduct('TrainArr')?.displayName).toBe('TrainArr')
  })

  it('assigns product categories', () => {
    expect(getMarketingProduct('nexarr')?.category).toBe('control-plane')
    expect(getMarketingProduct('companion')?.category).toBe('field')
    expect(MARKETING_PRODUCTS.every((p) => p.tagline.length > 0)).toBe(true)
  })

  it('documents direct cross-product dependencies in the checklist', () => {
    const maintainarr = getMarketingProduct('maintainarr')!
    const routarr = getMarketingProduct('routarr')!

    expect(maintainarr.checklist.training).toBe('connected')
    expect(maintainarr.connectedReasons.training).toMatch(/qualification/i)
    expect(routarr.checklist.warehouse).toBe('connected')
    expect(routarr.connectedReasons.warehouse).toMatch(/stock|load/i)
  })
})
