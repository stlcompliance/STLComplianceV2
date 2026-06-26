import { afterEach, describe, expect, it, vi } from 'vitest'

import {
  AssurArrApiError,
  getSessionBootstrap,
  redeemHandoff,
} from './client'

describe('assurarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('normalizes legacy launch-key aliases in session bootstrap responses', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(
        JSON.stringify({
          userId: 'user-1',
          personId: 'person-1',
          tenantId: 'tenant-1',
          sessionId: 'session-1',
          tenantRoleKey: 'tenant_member',
          isPlatformAdmin: false,
          productKey: 'assurarr',
          hasAssurArrAccess: true,
          launchableProductKeys: ['assurarr', 'recordarr'],
        }),
        {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        },
      ),
    )

    await expect(getSessionBootstrap('token-123')).resolves.toMatchObject({
      hasAssurArrAccess: true,
      launchableProductKeys: ['assurarr', 'recordarr'],
    })
  })

  it('preserves handoff API error status metadata', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response('handoff expired', {
        status: 401,
        headers: { 'Content-Type': 'text/plain' },
      }),
    )

    const error = await redeemHandoff('handoff-123').catch((caught) => caught)

    expect(error).toBeInstanceOf(AssurArrApiError)
    expect(error).toMatchObject({
      status: 401,
    })
  })
})
