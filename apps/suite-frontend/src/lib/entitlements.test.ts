import { describe, expect, it } from 'vitest'

import { entitlementStatusClass } from './entitlements'

describe('entitlementStatusClass', () => {
  it('returns active styling for active entitlements', () => {
    expect(entitlementStatusClass('active')).toContain('emerald')
  })

  it('returns muted styling for other statuses', () => {
    expect(entitlementStatusClass('revoked')).toContain('slate')
  })
})
