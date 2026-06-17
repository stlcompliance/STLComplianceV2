import { describe, expect, it } from 'vitest'

import { isActiveTenantStatus, normalizeTenantStatusValue } from './tenantStatus'

describe('tenant status helpers', () => {
  it('normalizes tenant lifecycle status casing and whitespace', () => {
    expect(normalizeTenantStatusValue(' Active ')).toBe('active')
    expect(isActiveTenantStatus('Active')).toBe(true)
    expect(isActiveTenantStatus('active')).toBe(true)
    expect(isActiveTenantStatus('Suspended')).toBe(false)
    expect(isActiveTenantStatus(null)).toBe(false)
  })
})
