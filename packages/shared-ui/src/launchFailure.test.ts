import { describe, expect, it } from 'vitest'
import { resolveNexArrLaunchFailureMessage } from './launchFailure'

describe('resolveNexArrLaunchFailureMessage', () => {
  it('maps entitlement failures to friendly message', () => {
    const message = resolveNexArrLaunchFailureMessage('StaffArr', {
      status: 403,
      body: JSON.stringify({
        code: 'handoff.not_entitled',
        message: 'Tenant not entitled.',
      }),
    })

    expect(message).toBe('Your account is not entitled to StaffArr for this tenant.')
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
