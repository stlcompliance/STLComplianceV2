import { describe, expect, it } from 'vitest'

import { isWebPushSupported, pushReadinessLabel } from './pushNotifications'

describe('pushNotifications', () => {
  it('reports unsupported when browser APIs are missing', () => {
    expect(isWebPushSupported()).toBe(false)
  })

  it('maps permission states to plain labels', () => {
    expect(pushReadinessLabel('granted')).toContain('granted')
    expect(pushReadinessLabel('denied')).toContain('denied')
    expect(pushReadinessLabel('default')).toContain('not requested')
    expect(pushReadinessLabel('unsupported')).toContain('not supported')
  })
})
