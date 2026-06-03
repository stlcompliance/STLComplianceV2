import { describe, expect, it } from 'vitest'
import {
  COMPLIANCE_CORE_EDUCATION,
  DOCS_11_ACCEPTANCE_NOTE,
  PRODUCT_OWNERSHIP,
} from './ownershipBoundaries'
import { MARKETING_PRODUCTS } from './products'

describe('ownershipBoundaries', () => {
  it('defines ownership for every marketed product', () => {
    for (const product of MARKETING_PRODUCTS) {
      expect(PRODUCT_OWNERSHIP[product.productKey]).toBeDefined()
    }
  })

  it('matches marketing owns/doesNotOwn strings exactly', () => {
    for (const product of MARKETING_PRODUCTS) {
      const boundary = PRODUCT_OWNERSHIP[product.productKey]
      expect(product.owns).toBe(boundary.owns)
      expect(product.doesNotOwn).toBe(boundary.doesNotOwn)
    }
  })

  it('states Compliance Core rules and proof role', () => {
    expect(COMPLIANCE_CORE_EDUCATION.headline.toLowerCase()).toMatch(/rules/)
    expect(COMPLIANCE_CORE_EDUCATION.lead).toMatch(/proof matters/i)
    expect(COMPLIANCE_CORE_EDUCATION.bullets.join(' ')).toMatch(/evidence expectations/i)
  })

  it('documents status labels without implying 100% complete', () => {
    expect(DOCS_11_ACCEPTANCE_NOTE.toLowerCase()).toMatch(/usable workflows/)
    expect(DOCS_11_ACCEPTANCE_NOTE.toLowerCase()).toMatch(/do not overpromise/)
  })
})
