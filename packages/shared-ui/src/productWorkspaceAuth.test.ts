import { describe, expect, it, vi } from 'vitest'
import {
  buildNexArrLoginUrl,
  isProductWorkspaceAuthError,
  resolveProductLaunchCallbackPath,
  resolveProductWorkspaceBootstrapError,
} from './productWorkspaceAuth'

describe('productWorkspaceAuth', () => {
  it('detects 401 and 403 API errors', () => {
    expect(isProductWorkspaceAuthError({ status: 401 })).toBe(true)
    expect(isProductWorkspaceAuthError({ status: 403 })).toBe(true)
    expect(isProductWorkspaceAuthError({ status: 500 })).toBe(false)
    expect(isProductWorkspaceAuthError(new Error('fail'))).toBe(false)
  })

  it('maps auth errors to bootstrap states', () => {
    expect(resolveProductWorkspaceBootstrapError({ status: 403 })).toBe('forbidden')
    expect(resolveProductWorkspaceBootstrapError({ status: 401 })).toBe('expired')
    expect(resolveProductWorkspaceBootstrapError({ status: 500 })).toBeNull()
  })

  it('builds NexArr login URLs with product callback context', () => {
    const url = buildNexArrLoginUrl({
      suiteHomeUrl: 'https://suite.example.com/app',
      productKey: 'StaffArr',
      callbackUrl: 'https://staffarr.example.com/people?tab=roles',
    })

    expect(url).toBe(
      'https://suite.example.com/login?productKey=staffarr&callbackUrl=https%3A%2F%2Fstaffarr.example.com%2Fpeople%3Ftab%3Droles',
    )
  })

  it('resolves only same-origin product callback paths', () => {
    const originalLocation = globalThis.location
    vi.stubGlobal('location', {
      href: 'https://staffarr.example.com/people',
    })

    try {
      expect(
        resolveProductLaunchCallbackPath('https://staffarr.example.com/people/details?id=1#training'),
      ).toBe('/people/details?id=1#training')
      expect(resolveProductLaunchCallbackPath('https://other.example.com/people')).toBe('/')
      expect(resolveProductLaunchCallbackPath('https://staffarr.example.com/launch')).toBe('/')
    } finally {
      vi.stubGlobal('location', originalLocation)
    }
  })
})
