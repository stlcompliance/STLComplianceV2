import { describe, expect, it } from 'vitest'

import { availabilityStatusClass } from './availability'

describe('availabilityStatusClass', () => {
  it('returns active styling for active availability records', () => {
    expect(availabilityStatusClass('active')).toContain('emerald')
  })

  it('returns muted styling for other statuses', () => {
    expect(availabilityStatusClass('revoked')).toContain('slate')
  })
})
