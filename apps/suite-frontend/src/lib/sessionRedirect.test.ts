import { afterEach, describe, expect, it, vi } from 'vitest'
import { NexarrApiError } from '../api/types'
import { buildSuiteLoginRedirectUrl, redirectToSuiteLoginIfSessionExpired } from './sessionRedirect'

describe('sessionRedirect', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('builds NexArr login URLs with the current suite callback context', () => {
    vi.stubGlobal('location', {
      href: 'https://suite.example.com/app/products/staffarr',
      assign: vi.fn(),
    })

    expect(buildSuiteLoginRedirectUrl('staffarr')).toBe(
      'https://suite.example.com/login?productKey=staffarr&callbackUrl=https%3A%2F%2Fsuite.example.com%2Fapp%2Fproducts%2Fstaffarr',
    )
  })

  it('redirects only when the error is an expired NexArr session', () => {
    const assign = vi.fn()
    vi.stubGlobal('location', {
      href: 'https://suite.example.com/app/products/staffarr',
      assign,
    })

    expect(redirectToSuiteLoginIfSessionExpired(new NexarrApiError(401, 'Unauthorized'), 'staffarr')).toBe(true)
    expect(assign).toHaveBeenCalledWith(
      'https://suite.example.com/login?productKey=staffarr&callbackUrl=https%3A%2F%2Fsuite.example.com%2Fapp%2Fproducts%2Fstaffarr',
    )

    assign.mockReset()
    expect(redirectToSuiteLoginIfSessionExpired(new NexarrApiError(403, 'Forbidden'), 'staffarr')).toBe(false)
    expect(assign).not.toHaveBeenCalled()
  })
})
