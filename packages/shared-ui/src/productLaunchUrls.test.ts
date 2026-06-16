import { describe, expect, it } from 'vitest'
import { buildProductLaunchUrlMap, resolveProductLaunchUrl } from './productLaunchUrls'

describe('productLaunchUrls', () => {
  it('builds canonical Field Companion launch URLs from env', () => {
    const map = buildProductLaunchUrlMap({
      VITE_FIELDCOMPANION_FRONTEND_BASE: 'http://localhost:5181/',
    })

    expect(map.fieldcompanion).toBe('http://localhost:5181/launch')
    expect(map.nexarr).toBeUndefined()
  })

  it('resolves direct launch URLs using canonical product keys only', () => {
    const launchUrls = {
      fieldcompanion: 'http://localhost:5181/launch',
    }

    expect(resolveProductLaunchUrl('fieldcompanion', 'http://localhost:5174', launchUrls)).toBe(
      'http://localhost:5181/launch',
    )
    expect(resolveProductLaunchUrl('fieldcompanion', 'http://localhost:5174', {})).toBe(
      'http://localhost:5174/app/field-companion/launch',
    )
  })

  it('uses app public base and route slugs for implemented products', () => {
    const map = buildProductLaunchUrlMap({
      VITE_APP_PUBLIC_BASE_URL: 'https://app.stlcompliance.com/',
    })

    expect(map.staffarr).toBe('https://app.stlcompliance.com/staffarr/launch')
    expect(map.recordarr).toBe('https://app.stlcompliance.com/recordarr/launch')
    expect(map.customarr).toBe('https://app.stlcompliance.com/customarr/launch')
    expect(map.fieldcompanion).toBe('https://app.stlcompliance.com/field-companion/launch')
    expect(map.nexarr).toBeUndefined()
    expect(map.ordarr).toBeUndefined()
  })

  it('upgrades same-host launch URLs to https when the browser is on https', () => {
    const originalWindow = (globalThis as typeof globalThis & { window?: Window }).window
    Object.defineProperty(globalThis, 'window', {
      configurable: true,
      value: {
        location: {
          href: 'https://app.stlcompliance.com/assurarr',
          hostname: 'app.stlcompliance.com',
          port: '',
          protocol: 'https:',
        },
      },
    })

    try {
      expect(
        resolveProductLaunchUrl(
          'assurarr',
          'http://app.stlcompliance.com/app',
          {
            assurarr: 'http://app.stlcompliance.com/assurarr/launch',
          },
        ),
      ).toBe('https://app.stlcompliance.com/assurarr/launch')
    } finally {
      if (originalWindow === undefined) {
        Reflect.deleteProperty(globalThis, 'window')
      } else {
        Object.defineProperty(globalThis, 'window', {
          configurable: true,
          value: originalWindow,
        })
      }
    }
  })
})
