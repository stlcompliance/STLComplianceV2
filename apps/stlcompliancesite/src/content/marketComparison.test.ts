import { describe, expect, it } from 'vitest'
import { MARKET_CHECKLIST_ROWS, MARKET_PRODUCT_COMPARISONS } from './marketComparison'

describe('marketComparison content', () => {
  it('compares named products across major operating categories', () => {
    const products = MARKET_PRODUCT_COMPARISONS.map((row) => row.product)

    expect(products).toContain('Manhattan Active Warehouse Management')
    expect(products).toContain('IBM Maximo Application Suite')
    expect(products).toContain('Cornerstone Learning Management')
    expect(products).toContain('UKG Pro Workforce Management')
    expect(products).toContain('Oracle Transportation Management')
    expect(MARKET_PRODUCT_COMPARISONS.length).toBeGreaterThanOrEqual(10)
  })

  it('keeps every competitor row sourced and positioned against STL', () => {
    for (const row of MARKET_PRODUCT_COMPARISONS) {
      expect(row.sourceHref).toMatch(/^https:\/\//)
      expect(row.bestAt.trim().length).toBeGreaterThan(20)
      expect(row.stlDifference).toMatch(/STL|LoadArr|MaintainArr|TrainArr|StaffArr|RoutArr/)
    }
  })

  it('includes checklist-style buyer questions', () => {
    expect(MARKET_CHECKLIST_ROWS.length).toBeGreaterThanOrEqual(4)
    expect(MARKET_CHECKLIST_ROWS.some((row) => row.question.includes('should this work start'))).toBe(
      true,
    )
  })
})
