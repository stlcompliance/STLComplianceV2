import { describe, expect, it } from 'vitest'

import {
  buildLaunchFailureFromContext,
  formatLaunchFailureError,
  resolveLaunchFailureCopy,
} from './launchFailure'

describe('launchFailure', () => {
  it('maps known denial reason codes to friendly copy', () => {
    const copy = resolveLaunchFailureCopy('not_entitled')
    expect(copy.title).toBe('Product not entitled')
    expect(copy.severity).toBe('warning')
  })

  it('falls back for unknown codes', () => {
    const copy = resolveLaunchFailureCopy('launch.custom_block')
    expect(copy.message).toContain('launch.custom_block')
  })

  it('returns null when launch context permits launch', () => {
    const result = buildLaunchFailureFromContext({
      tenantId: 't',
      tenantSlug: 'demo',
      tenantDisplayName: 'Demo',
      userId: 'u',
      userEmail: 'a@b.c',
      productKey: 'staffarr',
      productDisplayName: 'StaffArr',
      baseLaunchUrl: 'http://localhost:5175',
      launchUrl: 'http://localhost:5175',
      canLaunch: true,
      denialReasonCode: null,
    })
    expect(result).toBeNull()
  })

  it('formats launch errors for inline alerts', () => {
    expect(formatLaunchFailureError('tenant_suspended')).toContain('organization workspace')
  })
})
