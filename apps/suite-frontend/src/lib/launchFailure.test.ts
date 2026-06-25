import { describe, expect, it } from 'vitest'

import {
  buildLaunchFailureFromContext,
  formatLaunchFailureError,
  resolveLaunchFailureCopy,
} from './launchFailure'

describe('launchFailure', () => {
  it('maps known denial reason codes to friendly copy', () => {
    const copy = resolveLaunchFailureCopy('product_unavailable')
    expect(copy.title).toBe('Product unavailable')
    expect(copy.severity).toBe('warning')
  })

  it('maps availability-flavored denial reason codes to friendly copy', () => {
    const copy = resolveLaunchFailureCopy('availability_inactive')
    expect(copy.title).toBe('Launch context inactive')
    expect(copy.severity).toBe('warning')
  })

  it('maps platform-admin-only launch denials to guidance copy', () => {
    const copy = resolveLaunchFailureCopy('platform_admin_required')
    expect(copy.title).toBe('Platform administrator required')
    expect(copy.message).toContain('restricted to NexArr platform administrators')
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
