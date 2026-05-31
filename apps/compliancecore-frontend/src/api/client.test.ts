import { afterEach, describe, expect, it, vi } from 'vitest'
import { ComplianceCoreApiError, getMe } from './client'

describe('compliancecore api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads profile successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          userId: '11111111-1111-1111-1111-111111111111',
          personId: '22222222-2222-2222-2222-222222222222',
          email: 'operator@example.com',
          displayName: 'Operator One',
          tenantId: '33333333-3333-3333-3333-333333333333',
          tenantRoleKey: 'compliance_operator',
          isPlatformAdmin: false,
          productKey: 'compliancecore',
          hasComplianceCoreEntitlement: true,
          entitlements: ['compliancecore.use'],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const profile = await getMe('token-123')
    expect(profile.displayName).toBe('Operator One')
    expect(fetchMock).toHaveBeenCalledWith('/api/me', expect.any(Object))
  })

  it('surfaces problem details title/detail in API errors', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'Rule evaluation blocked',
          detail: 'No active rule pack is available.',
        }),
        { status: 409, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getMe('token-123')).rejects.toMatchObject({
      status: 409,
      message: 'Rule evaluation blocked - No active rule pack is available.',
    })
  })

  it('surfaces validation errors in API error messages', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'Validation failed',
          errors: {
            regulatoryProgramId: ['Regulatory program is required.'],
            rulePackId: ['Rule pack must be a valid GUID.'],
          },
        }),
        { status: 422, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getMe('token-123')).rejects.toMatchObject({
      status: 422,
      message:
        'Validation failed - regulatoryProgramId: Regulatory program is required.; rulePackId: Rule pack must be a valid GUID.',
      name: new ComplianceCoreApiError('', 0, '').name,
    })
  })
})
