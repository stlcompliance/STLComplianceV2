import { describe, expect, it } from 'vitest'

import { launchContextStatusClass } from './launchContextStatus'

describe('launchContextStatusClass', () => {
  it('returns active styling for active launch contexts', () => {
    expect(launchContextStatusClass('active')).toContain('emerald')
  })

  it('returns muted styling for other statuses', () => {
    expect(launchContextStatusClass('revoked')).toContain('slate')
  })
})
