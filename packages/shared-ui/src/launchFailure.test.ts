import { describe, expect, it } from 'vitest'
import { resolveNexArrLaunchFailureMessage } from './launchFailure'

describe('resolveNexArrLaunchFailureMessage', () => {
  it('maps access-context failures to friendly message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'availability_inactive',
        message: 'Tenant unavailable.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps current availability-denial aliases to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'availability_inactive',
        message: 'Current availability state.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps canonical product-unavailable failures to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'product_unavailable',
        message: 'Product unavailable.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps shared-framework product-not-available failures to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'product_not_available',
        message: 'Product not available.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps canonical launch product-unavailable failures to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'launch.product_unavailable',
        message: 'Launch product unavailable.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps bare availability-revoked aliases to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'availability_revoked',
        message: 'Availability was revoked.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps current handoff-unavailable failures to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'handoff.not_available',
        message: 'Current availability state.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps canonical availability-revoked failures to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'launch.availability_revoked',
        message: 'Availability was revoked.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps current bare availability failures to the same availability message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'not_available',
        message: 'Current availability state.',
      }),
    })

    expect(message).toBe('StaffArr is unavailable for your current tenant context.')
  })

  it('maps callback mismatch to invalid callback state', () => {
    const message = resolveNexArrLaunchFailureMessage('TrainArr', {
      status: 403,
      body: JSON.stringify({
        code: 'handoff.product_mismatch',
        message: 'Wrong product.',
      }),
    })

    expect(message).toBe('Invalid callback for TrainArr. Relaunch from NexArr.')
  })

  it('maps tenant mismatch to tenant selection guidance', () => {
    const message = resolveNexArrLaunchFailureMessage('MaintainArr', {
      status: 403,
      body: JSON.stringify({
        code: 'auth.tenant_forbidden',
        message: 'Forbidden tenant scope.',
      }),
    })

    expect(message).toContain('Select the correct tenant in NexArr and relaunch.')
  })

  it('maps platform-admin failures to administrator guidance', () => {
    const message = resolveNexArrLaunchFailureMessage('Compliance Core', {
      status: 403,
      body: JSON.stringify({
        code: 'auth.platform_admin_required',
        message: 'Platform administrator access is required.',
      }),
    })

    expect(message).toBe('Compliance Core requires platform administrator access in NexArr.')
  })

  it('maps ended NexArr sessions to relaunch guidance', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 401,
      body: JSON.stringify({
        code: 'launch.session_revoked',
        message: 'The source NexArr session has ended.',
      }),
    })

    expect(message).toBe('Your NexArr session has ended. Sign in again and relaunch from the suite.')
  })
})
