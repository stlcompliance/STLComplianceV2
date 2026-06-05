import { describe, expect, it } from 'vitest'
import { buildProductLaunchUrlMap, resolveProductLaunchUrl } from './productLaunchUrls'

describe('productLaunchUrls', () => {
  it('builds canonical and legacy Field Companion launch URLs from env', () => {
    const map = buildProductLaunchUrlMap({
      VITE_COMPANION_FRONTEND_BASE: 'http://localhost:5181/',
    })

    expect(map.fieldcompanion).toBe('http://localhost:5181/launch')
    expect(map.companion).toBe('http://localhost:5181/launch')
  })

  it('resolves direct launch URLs using canonical or legacy product keys', () => {
    const launchUrls = {
      companion: 'http://localhost:5181/launch',
    }

    expect(resolveProductLaunchUrl('fieldcompanion', 'http://localhost:5174', launchUrls)).toBe(
      'http://localhost:5181/launch',
    )
    expect(resolveProductLaunchUrl('companion', 'http://localhost:5174', launchUrls)).toBe(
      'http://localhost:5181/launch',
    )
  })

  it('uses app public base and route slugs for implemented products', () => {
    const map = buildProductLaunchUrlMap({
      VITE_APP_PUBLIC_BASE_URL: 'https://app.stlcompliance.com/',
    })

    expect(map.staffarr).toBe('https://app.stlcompliance.com/staffarr/launch')
    expect(map.fieldcompanion).toBe('https://app.stlcompliance.com/field-companion/launch')
    expect(map.companion).toBe('https://app.stlcompliance.com/field-companion/launch')
  })
})
