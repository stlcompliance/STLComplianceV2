import { describe, expect, it } from 'vitest'
import { hasStaffArrEntitlement } from './sessionStorage'

describe('hasStaffArrEntitlement', () => {
  it('returns true when staffarr is present', () => {
    expect(hasStaffArrEntitlement(['nexarr', 'staffarr'])).toBe(true)
  })

  it('returns false when staffarr is absent', () => {
    expect(hasStaffArrEntitlement(['nexarr'])).toBe(false)
  })
})
