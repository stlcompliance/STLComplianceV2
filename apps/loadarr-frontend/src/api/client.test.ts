import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  LoadArrApiError,
  getSessionBootstrap,
  redeemHandoff,
} from './client'

describe('loadarr api client', () => {
  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('surfaces session bootstrap auth failures with status metadata', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ code: 'auth.session_expired' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    await expect(getSessionBootstrap('token-123')).rejects.toMatchObject({
      name: 'LoadArrApiError',
      status: 401,
      body: '{"code":"auth.session_expired"}',
    })
  })

  it('preserves handoff API response bodies for launch failure messaging', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValue(
      new Response(JSON.stringify({ code: 'launch.handoff_expired' }), {
        status: 401,
        headers: { 'Content-Type': 'application/json' },
      }),
    )

    const error = await redeemHandoff('handoff-123').catch((caught) => caught)

    expect(error).toBeInstanceOf(LoadArrApiError)
    expect(error).toMatchObject({
      status: 401,
      body: '{"code":"launch.handoff_expired"}',
    })
  })
})
