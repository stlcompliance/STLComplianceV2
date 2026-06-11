import { describe, expect, it } from 'vitest'
import { resolveLoginRedirectTarget } from './loginRedirect'

describe('loginRedirect', () => {
  const launchUrls = {
    staffarr: 'http://localhost:5175/launch',
    trainarr: 'http://localhost:5176/launch',
  }

  it('resolves allowed product callback redirects', () => {
    const target = resolveLoginRedirectTarget(
      '?productKey=StaffArr&callbackUrl=http%3A%2F%2Flocalhost%3A5175%2Fpeople%3Ftab%3Droles',
      launchUrls,
      'http://localhost:5174/login',
    )

    expect(target).toEqual({
      kind: 'product',
      productKey: 'staffarr',
      callbackUrl: 'http://localhost:5175/people?tab=roles',
    })
  })

  it('rejects product callbacks from an unexpected origin', () => {
    expect(
      resolveLoginRedirectTarget(
        '?productKey=staffarr&callbackUrl=https%3A%2F%2Fevil.example%2Fpeople',
        launchUrls,
        'http://localhost:5174/login',
      ),
    ).toBeNull()
  })

  it('resolves same-origin callback URLs as internal navigation', () => {
    const target = resolveLoginRedirectTarget(
      '?callbackurl=%2Fapp%2Fplatform-admin%2Fsessions',
      launchUrls,
      'http://localhost:5174/login',
    )

    expect(target).toEqual({
      kind: 'internal',
      to: '/app/platform-admin/sessions',
    })
  })
})
