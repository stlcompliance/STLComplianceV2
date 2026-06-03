import { describe, expect, it } from 'vitest'
import {
  MATURITY_DISCLAIMER,
  MILESTONE_POSTURE_LABELS,
  PROGRAM_MILESTONES,
  PROGRAM_SNAPSHOT,
  VERIFICATION_HIGHLIGHTS,
} from './implementationMaturity'
import { MARKETING_PRODUCTS } from './products'

describe('implementationMaturity content', () => {
  it('defines unique milestone ids M0 through M13', () => {
    const ids = PROGRAM_MILESTONES.map((m) => m.id)
    expect(new Set(ids).size).toBe(ids.length)
    expect(ids).toContain('M0')
    expect(ids).toContain('M13')
    expect(PROGRAM_MILESTONES.length).toBe(14)
  })

  it('uses known posture labels only', () => {
    const allowed = new Set(Object.keys(MILESTONE_POSTURE_LABELS))
    for (const row of PROGRAM_MILESTONES) {
      expect(allowed.has(row.posture)).toBe(true)
      expect(row.summary.trim().length).toBeGreaterThan(0)
    }
  })

  it('states plain status transparency and tracks rollout snapshot', () => {
    expect(MATURITY_DISCLAIMER.toLowerCase()).toMatch(/status|snapshot/)
    expect(MATURITY_DISCLAIMER.toLowerCase()).toMatch(/roadmap|contract/)
    expect(PROGRAM_SNAPSHOT.lastUpdatedLabel).toMatch(/June 2026/)
    expect(VERIFICATION_HIGHLIGHTS.join(' ')).toMatch(/Core product workflows/i)
    expect(VERIFICATION_HIGHLIGHTS.length).toBeGreaterThanOrEqual(3)
  })

  it('aligns product count with marketing catalog', () => {
    expect(MARKETING_PRODUCTS.length).toBeGreaterThanOrEqual(8)
    const partial = MARKETING_PRODUCTS.filter((p) => p.maturity === 'v1-partial')
    expect(partial.some((p) => p.productKey === 'companion')).toBe(true)
  })
})
