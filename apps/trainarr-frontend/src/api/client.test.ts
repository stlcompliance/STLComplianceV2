import { afterEach, describe, expect, it, vi } from 'vitest'
import { getMe, TrainArrApiError } from './client'

describe('trainarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('loads profile successfully', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          userId: '11111111-1111-1111-1111-111111111111',
          personId: '22222222-2222-2222-2222-222222222222',
          email: 'trainer@example.com',
          displayName: 'Trainer One',
          tenantId: '33333333-3333-3333-3333-333333333333',
          tenantRoleKey: 'trainarr_admin',
          isPlatformAdmin: false,
          productKey: 'trainarr',
          hasTrainArrAccess: true,
          launchableProductKeys: ['trainarr.use'],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    const profile = await getMe('token-123')
    expect(profile.displayName).toBe('Trainer One')
    expect(fetchMock).toHaveBeenCalledWith('/api/me', expect.any(Object))
  })

  it('normalizes legacy launch-key aliases in profile responses', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          userId: '11111111-1111-1111-1111-111111111111',
          personId: '22222222-2222-2222-2222-222222222222',
          email: 'trainer@example.com',
          displayName: 'Trainer One',
          tenantId: '33333333-3333-3333-3333-333333333333',
          tenantRoleKey: 'trainarr_admin',
          isPlatformAdmin: false,
          productKey: 'trainarr',
          hasTrainArrAccess: true,
          launchableProductKeys: ['trainarr', 'reportarr'],
        }),
        { status: 200, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getMe('token-123')).resolves.toMatchObject({
      hasTrainArrAccess: true,
      launchableProductKeys: ['trainarr', 'reportarr'],
    })
  })

  it('surfaces problem details title/detail in API errors', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'Training sync failed',
          detail: 'Upstream eligibility service is unavailable.',
        }),
        { status: 503, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getMe('token-123')).rejects.toMatchObject({
      status: 503,
      message: 'Training sync failed - Upstream eligibility service is unavailable.',
    })
  })

  it('surfaces validation errors in API error messages', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          title: 'Validation failed',
          errors: {
            personId: ['PersonId is required.'],
            qualificationKey: ['Qualification key is invalid.'],
          },
        }),
        { status: 422, headers: { 'Content-Type': 'application/json' } },
      ),
    )

    await expect(getMe('token-123')).rejects.toMatchObject({
      status: 422,
      message:
        'Validation failed - personId: PersonId is required.; qualificationKey: Qualification key is invalid.',
      name: new TrainArrApiError('', 0, '').name,
    })
  })
})

