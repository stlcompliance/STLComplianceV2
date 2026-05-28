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
})
