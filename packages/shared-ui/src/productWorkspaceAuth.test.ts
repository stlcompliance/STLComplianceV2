import { describe, expect, it } from 'vitest'
import {
  isProductWorkspaceAuthError,
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
})
