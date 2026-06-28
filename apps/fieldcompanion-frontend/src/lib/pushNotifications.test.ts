import { afterEach, describe, expect, it, vi } from 'vitest'

const { subscribeFieldCompanionPushMock } = vi.hoisted(() => ({
  subscribeFieldCompanionPushMock: vi.fn(),
}))

vi.mock('../api/client', () => ({
  getFieldCompanionPushVapidPublicKey: vi.fn().mockResolvedValue({ publicKey: 'AQID' }),
  subscribeFieldCompanionPush: subscribeFieldCompanionPushMock,
  unsubscribeFieldCompanionPush: vi.fn(),
}))

import { isWebPushSupported, pushReadinessLabel, syncFieldCompanionPushSubscription } from './pushNotifications'

describe('pushNotifications', () => {
  afterEach(() => {
    subscribeFieldCompanionPushMock.mockReset()
    Object.defineProperty(window, 'PushManager', { configurable: true, value: undefined })
    Object.defineProperty(window, 'Notification', { configurable: true, value: undefined })
    Object.defineProperty(globalThis, 'Notification', { configurable: true, value: undefined })
    Object.defineProperty(navigator, 'serviceWorker', { configurable: true, value: undefined })
  })

  it('reports unsupported when browser APIs are missing', () => {
    expect(isWebPushSupported()).toBe(false)
  })

  it('maps permission states to plain labels', () => {
    expect(pushReadinessLabel('granted')).toContain('granted')
    expect(pushReadinessLabel('denied')).toContain('denied')
    expect(pushReadinessLabel('default')).toContain('not requested')
    expect(pushReadinessLabel('unsupported')).toContain('not supported')
  })

  it('registers push subscriptions with coarse device diagnostics instead of raw user agent strings', async () => {
    const subscription = {
      endpoint: 'https://push.example/subscription',
      toJSON: () => ({
        endpoint: 'https://push.example/subscription',
        keys: {
          p256dh: 'p256dh-key',
          auth: 'auth-key',
        },
      }),
      unsubscribe: vi.fn(),
    }
    const registration = {
      pushManager: {
        getSubscription: vi.fn().mockResolvedValue(null),
        subscribe: vi.fn().mockResolvedValue(subscription),
      },
    }

    Object.defineProperty(window, 'PushManager', { configurable: true, value: function PushManager() {} })
    Object.defineProperty(window, 'Notification', { configurable: true, value: { permission: 'granted' } })
    Object.defineProperty(globalThis, 'Notification', { configurable: true, value: { permission: 'granted' } })
    Object.defineProperty(navigator, 'serviceWorker', {
      configurable: true,
      value: {
        register: vi.fn().mockResolvedValue(registration),
        ready: Promise.resolve(registration),
      },
    })

    await expect(syncFieldCompanionPushSubscription('token')).resolves.toBe('subscribed')

    expect(subscribeFieldCompanionPushMock).toHaveBeenCalledWith(
      'token',
      expect.objectContaining({
        userAgent: expect.stringMatching(/ on /),
      }),
    )
    const payload = subscribeFieldCompanionPushMock.mock.calls[0]?.[1]
    expect(payload.userAgent).not.toContain('Mozilla')
    expect(payload.userAgent).not.toContain('AppleWebKit')
  })
})
