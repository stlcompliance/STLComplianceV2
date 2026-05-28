import { describe, expect, it } from 'vitest'
import { ENTITLEMENT_EXAMPLES, LICENSING_PILLARS, PRICING_DISCLAIMER } from './pricing'

describe('pricing content', () => {
  it('defines unique licensing pillars', () => {
    const ids = LICENSING_PILLARS.map((pillar) => pillar.id)
    expect(new Set(ids).size).toBe(ids.length)
    expect(LICENSING_PILLARS.some((pillar) => pillar.id === 'honesty')).toBe(true)
  })

  it('lists entitlement examples without dollar amounts', () => {
    expect(ENTITLEMENT_EXAMPLES.length).toBeGreaterThanOrEqual(5)
    const combined = `${PRICING_DISCLAIMER} ${ENTITLEMENT_EXAMPLES.map((e) => e.summary).join(' ')}`
    expect(combined).not.toMatch(/\$\d/)
  })
})
