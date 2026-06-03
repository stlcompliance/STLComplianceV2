import { describe, expect, it } from 'vitest'
import {
  ALTERNATIVE_SCENARIOS,
  COMPARISON_DIMENSIONS,
  COMPARE_DISCLAIMER,
  SUITE_HONESTY_NOTES,
} from './compare'

describe('compare content', () => {
  it('defines unique scenario and dimension ids', () => {
    const scenarioIds = ALTERNATIVE_SCENARIOS.map((s) => s.id)
    const dimensionIds = COMPARISON_DIMENSIONS.map((d) => d.id)
    expect(new Set(scenarioIds).size).toBe(scenarioIds.length)
    expect(new Set(dimensionIds).size).toBe(dimensionIds.length)
    expect(ALTERNATIVE_SCENARIOS.some((s) => s.id === 'spreadsheets')).toBe(true)
    expect(ALTERNATIVE_SCENARIOS.some((s) => s.id === 'point-tools')).toBe(true)
  })

  it('covers spreadsheets, point tools, and suite columns in every dimension', () => {
    expect(COMPARISON_DIMENSIONS.length).toBeGreaterThanOrEqual(4)
    for (const row of COMPARISON_DIMENSIONS) {
      expect(row.spreadsheets.trim().length).toBeGreaterThan(0)
      expect(row.pointTools.trim().length).toBeGreaterThan(0)
      expect(row.stlSuite.trim().length).toBeGreaterThan(0)
    }
  })

  it('states marketing-only perspective and suite positioning', () => {
    expect(COMPARE_DISCLAIMER.toLowerCase()).toContain('audit scramble')
    const honesty = SUITE_HONESTY_NOTES.join(' ')
    expect(honesty).toMatch(/operating model|secure sign-in/i)
    expect(honesty).not.toMatch(/\$\d/)
  })
})
