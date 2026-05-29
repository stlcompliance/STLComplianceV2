import { describe, expect, it } from 'vitest'

import { fromDatetimeLocalValue, toDatetimeLocalValue } from './availabilityDateTime'

describe('availabilityDateTime', () => {
  it('round-trips ISO timestamps through datetime-local values', () => {
    const iso = '2026-05-27T14:30:00.000Z'
    const local = toDatetimeLocalValue(iso)
    expect(local).toMatch(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/)
    expect(fromDatetimeLocalValue(local)).toBe(new Date(local).toISOString())
  })
})
