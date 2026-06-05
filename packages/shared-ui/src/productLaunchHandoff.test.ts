import { describe, expect, it } from 'vitest'
import { buildProductWorkspaceCallbackUrl } from './productLaunchHandoff'

describe('buildProductWorkspaceCallbackUrl', () => {
  const launchUrls = {
    staffarr: 'http://localhost:5175/launch',
    trainarr: 'http://localhost:5176/launch',
  }

  it('resolves direct product launch URLs from env map', () => {
    expect(
      buildProductWorkspaceCallbackUrl('trainarr', 'http://localhost:5174/app', launchUrls),
    ).toBe('http://localhost:5176/launch')
  })

  it('falls back to suite launch route when product URL is missing', () => {
    expect(
      buildProductWorkspaceCallbackUrl('maintainarr', 'http://localhost:5174/app', launchUrls),
    ).toBe('http://localhost:5174/app/maintainarr/launch')
  })

  it('resolves Field Companion through canonical launch URLs', () => {
    expect(
      buildProductWorkspaceCallbackUrl('field-companion', 'http://localhost:5174/app', {
        fieldcompanion: 'http://localhost:5181/launch',
      }),
    ).toBe('http://localhost:5181/launch')
  })
})
