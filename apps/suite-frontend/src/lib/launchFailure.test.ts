import { describe, expect, it } from 'vitest'

import {
  buildLaunchFailureFromContext,
  describeLaunchFailure,
  formatLaunchFailureError,
  normalizeLaunchRemediationHint,
  resolveLaunchFailureCopy,
} from './launchFailure'

describe('launchFailure', () => {
  it('maps known denial reason codes to friendly copy', () => {
    const copy = resolveLaunchFailureCopy('product_unavailable')
    expect(copy.title).toBe('Product unavailable')
    expect(copy.severity).toBe('warning')
  })

  it('maps legacy availability-flavored denial reason codes to launch-destination copy', () => {
    const copy = resolveLaunchFailureCopy('availability_inactive')
    expect(copy.title).toBe('Launch destination inactive')
    expect(copy.severity).toBe('warning')
    expect(copy.message).toContain('available operating state')
    expect(copy.guidance).toContain('tenant status in NexArr')
  })

  it('normalizes legacy availability-inactive codes to the canonical launch-destination code', () => {
    expect(describeLaunchFailure('availability_inactive')).toMatchObject({
      title: 'Launch destination inactive',
      normalizedCode: 'launch_destination_inactive',
      rawCode: null,
    })
  })

  it('hides raw legacy alias details for launch-destination-inactive codes', () => {
    expect(describeLaunchFailure('launch.availability_inactive')).toMatchObject({
      normalizedCode: 'launch_destination_inactive',
      rawCode: null,
    })
  })

  it('maps launch-destination-inactive denial reason codes to canonical copy', () => {
    const copy = resolveLaunchFailureCopy('launch_destination_inactive')
    expect(copy.title).toBe('Launch destination inactive')
    expect(copy.guidance).toContain('product destination status')
    expect(copy.guidance).toContain('destination product permissions')
  })

  it('maps revoked and handoff availability aliases to product-unavailable copy', () => {
    expect(resolveLaunchFailureCopy('product_not_available').title).toBe('Product unavailable')
    expect(resolveLaunchFailureCopy('launch.product_unavailable').title).toBe('Product unavailable')
    expect(resolveLaunchFailureCopy('availability_revoked').title).toBe('Product unavailable')
    expect(resolveLaunchFailureCopy('launch.availability_revoked').title).toBe('Product unavailable')
    expect(resolveLaunchFailureCopy('handoff.not_available').title).toBe('Product unavailable')
    expect(resolveLaunchFailureCopy('not_available').title).toBe('Product unavailable')
  })

  it('maps platform-admin-only launch denials to guidance copy', () => {
    const copy = resolveLaunchFailureCopy('platform_admin_required')
    expect(copy.title).toBe('Platform administrator required')
    expect(copy.message).toContain('restricted to NexArr platform administrators')
  })

  it('describes compatibility launch reason codes without repeating raw alias details', () => {
    const description = describeLaunchFailure('handoff.not_available')
    expect(description).toMatchObject({
      title: 'Product unavailable',
      normalizedCode: 'product_unavailable',
      rawCode: null,
    })
  })

  it('normalizes stale launch remediation hints for legacy availability aliases', () => {
    expect(
      normalizeLaunchRemediationHint(
        'Activate or reactivate the tenant launch availability for the requested product.',
        'not_available',
      ),
    ).toBe(
      'Confirm the tenant is active, then review the destination product status and local permissions.',
    )
  })

  it('prefers canonical current guidance when a stale remediation hint arrives with an unavailable product reason', () => {
    expect(
      normalizeLaunchRemediationHint(
        'Activate or reactivate the tenant launch availability for the requested product.',
        'product_not_available',
      ),
    ).toBe(
      'Confirm your tenant membership, product status, and permissions, then try again.',
    )
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
