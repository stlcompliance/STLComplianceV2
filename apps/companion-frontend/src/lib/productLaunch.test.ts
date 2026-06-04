import { describe, expect, it } from 'vitest'

import { buildCompanionProductCallbackUrl, formatProductLaunchError } from './productLaunch'

describe('buildCompanionProductCallbackUrl', () => {
  const launchUrls = {
    staffarr: 'http://localhost:5175/launch',
    trainarr: 'http://localhost:5176/launch',
    fieldcompanion: 'http://localhost:5181/launch',
  }

  it('uses product launch map for operational products', () => {
    expect(
      buildCompanionProductCallbackUrl('trainarr', 'http://localhost:5174/app', launchUrls),
    ).toBe('http://localhost:5176/launch')
  })

  it('routes nexarr to the suite home', () => {
    expect(
      buildCompanionProductCallbackUrl('nexarr', 'http://localhost:5174/app', launchUrls),
    ).toBe('http://localhost:5174/app')
  })

  it('uses the field companion launch entry for current product handoff', () => {
    expect(
      buildCompanionProductCallbackUrl('field-companion', 'http://localhost:5174/app', launchUrls),
    ).toBe('http://localhost:5181/launch')
  })

  it('formats launch denial codes into plain language', () => {
    expect(formatProductLaunchError(new Error('not_entitled'))).toContain('entitled')
  })
})
