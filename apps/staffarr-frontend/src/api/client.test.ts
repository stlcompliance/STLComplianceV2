import { afterEach, describe, expect, it, vi } from 'vitest'
import { getPeople, redeemHandoff, StaffArrApiError } from './client'

describe('staffarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads people directory successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify([
          {
            personId: '11111111-1111-1111-1111-111111111111',
            externalUserId: null,
            displayName: 'Alice Admin',
            primaryEmail: 'alice@example.com',
            employmentStatus: 'active',
            primaryOrgUnitId: null,
            primaryOrgUnitName: null,
            managerPersonId: null,
            jobTitle: 'Supervisor',
          },
        ]),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const result = await getPeople('token-123')
    expect(result).toHaveLength(1)
    expect(result[0]?.displayName).toBe('Alice Admin')
    expect(fetchMock).toHaveBeenCalledWith('/api/people', expect.any(Object))
  })

  it('throws StaffArrApiError when people directory is forbidden', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response('Forbidden', { status: 403, headers: { 'Content-Type': 'text/plain' } }),
    )

    await expect(getPeople('token-123')).rejects.toBeInstanceOf(StaffArrApiError)
  })

  it('surfaces problem details title/detail in API errors', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'People query failed',
          detail: 'Directory service is unavailable.',
        }),
        { status: 503, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getPeople('token-123')).rejects.toMatchObject({
      status: 503,
      message: 'People query failed - Directory service is unavailable.',
    })
  })

  it('surfaces validation errors in API error messages', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'Validation failed',
          errors: {
            orgUnitId: ['Org unit does not exist.'],
            employmentStatus: ['Employment status is required.'],
          },
        }),
        { status: 422, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getPeople('token-123')).rejects.toMatchObject({
      status: 422,
      message:
        'Validation failed - orgUnitId: Org unit does not exist.; employmentStatus: Employment status is required.',
    })
  })

  it('normalizes legacy launch-key aliases in handoff session responses', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          accessToken: 'access-token',
          accessTokenExpiresAt: '2026-06-25T20:00:00.000Z',
          userId: 'user-1',
          personId: 'person-1',
          email: 'user@example.com',
          displayName: 'Alice Admin',
          tenantId: 'tenant-1',
          tenantSlug: 'tenant-one',
          tenantDisplayName: 'Tenant One',
          sessionId: 'session-1',
          tenantRoleKey: 'staffarr_admin',
          isPlatformAdmin: false,
          launchableProductKeys: ['staffarr', 'trainarr'],
          callbackUrl: null,
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(redeemHandoff('handoff-123')).resolves.toMatchObject({
      launchableProductKeys: ['staffarr', 'trainarr'],
    })
  })
})
