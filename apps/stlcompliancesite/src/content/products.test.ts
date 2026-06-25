import { describe, expect, it } from 'vitest'
import {
  allMarketingProducts,
  getMarketingProduct,
  MARKETING_PRODUCTS,
  PRODUCT_CATEGORY_LABELS,
  productPagePath,
} from './products'
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

  it('describes NexArr launch context in the public marketing copy', () => {
    const nexarr = allMarketingProducts.find((product) => product.productKey === 'nexarr')!
    expect(nexarr.tagline).toContain('login')
    expect(nexarr.tagline).toContain('launch')
    expect(nexarr.recordsManaged).toContain('Launch context records')
    expect(nexarr.evidenceOutputs).toContain('Launch context snapshots')
    expect(nexarr.handoffs.some((item) => item.includes('launch readiness checks pass'))).toBe(true)
  })

  it('labels the platform category with launch language', () => {
    expect(PRODUCT_CATEGORY_LABELS.platform).toBe('Platform and launch')
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
