import { cleanup, render, waitFor } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { useFieldCompanionWebPush } from './useFieldCompanionWebPush'

vi.mock('../lib/pushNotifications', () => ({
  getPushPermissionState: vi.fn(),
  isWebPushSupported: vi.fn(),
  syncFieldCompanionPushSubscription: vi.fn(),
}))

const pushNotifications = await import('../lib/pushNotifications')

function Harness({ accessToken }: { accessToken?: string }) {
  useFieldCompanionWebPush(accessToken)
  return null
}

describe('useFieldCompanionWebPush', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  afterEach(() => {
    cleanup()
  })

  it('syncs the push subscription once when supported and granted', async () => {
    vi.mocked(pushNotifications.isWebPushSupported).mockReturnValue(true)
    vi.mocked(pushNotifications.getPushPermissionState).mockReturnValue('granted')
    vi.mocked(pushNotifications.syncFieldCompanionPushSubscription).mockResolvedValue('subscribed')

    render(<Harness accessToken="access-token" />)

    await waitFor(() => {
      expect(pushNotifications.syncFieldCompanionPushSubscription).toHaveBeenCalledWith('access-token')
    })

    expect(pushNotifications.syncFieldCompanionPushSubscription).toHaveBeenCalledTimes(1)
  })

  it('does not sync when push is unsupported or permission is unavailable', async () => {
    vi.mocked(pushNotifications.isWebPushSupported).mockReturnValue(true)
    vi.mocked(pushNotifications.getPushPermissionState).mockReturnValue('default')

    render(<Harness accessToken="access-token" />)

    await waitFor(() => {
      expect(pushNotifications.getPushPermissionState).toHaveBeenCalledTimes(1)
    })

    expect(pushNotifications.syncFieldCompanionPushSubscription).not.toHaveBeenCalled()
  })
})
