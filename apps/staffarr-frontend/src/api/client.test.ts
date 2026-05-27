import { afterEach, describe, expect, it, vi } from 'vitest'
import { getPeople, StaffArrApiError } from './client'

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
})
