import {
  getFieldCompanionPushVapidPublicKey,
  subscribeFieldCompanionPush,
  unsubscribeFieldCompanionPush,
} from '../api/client'
import { formatCurrentFieldCompanionDeviceSourceLabel } from './devicePrivacy'

export type PushPermissionState = NotificationPermission | 'unsupported'

export function getPushPermissionState(): PushPermissionState {
  if (typeof window === 'undefined' || !('Notification' in window)) {
    return 'unsupported'
  }

  return Notification.permission
}

export async function requestPushPermission(): Promise<PushPermissionState> {
  if (typeof window === 'undefined' || !('Notification' in window)) {
    return 'unsupported'
  }

  if (Notification.permission === 'granted' || Notification.permission === 'denied') {
    return Notification.permission
  }

  return Notification.requestPermission()
}

export function pushReadinessLabel(permission: PushPermissionState): string {
  switch (permission) {
    case 'granted':
      return 'Browser push permission granted'
    case 'denied':
      return 'Browser push permission denied'
    case 'default':
      return 'Browser push permission not requested'
    default:
      return 'Browser push not supported on this device'
  }
}

export function isWebPushSupported(): boolean {
  return (
    typeof window !== 'undefined'
    && 'serviceWorker' in navigator
    && 'PushManager' in window
    && 'Notification' in window
  )
}

function urlBase64ToUint8Array(base64String: string): Uint8Array {
  const padding = '='.repeat((4 - (base64String.length % 4)) % 4)
  const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/')
  const rawData = window.atob(base64)
  const outputArray = new Uint8Array(rawData.length)
  for (let index = 0; index < rawData.length; index += 1) {
    outputArray[index] = rawData.charCodeAt(index)
  }
  return outputArray
}

export async function registerFieldCompanionServiceWorker(): Promise<ServiceWorkerRegistration | null> {
  if (!isWebPushSupported()) {
    return null
  }

  return navigator.serviceWorker.register('/sw.js')
}

export async function syncFieldCompanionPushSubscription(accessToken: string): Promise<'subscribed' | 'skipped' | 'failed'> {
  if (!isWebPushSupported() || Notification.permission !== 'granted') {
    return 'skipped'
  }

  try {
    const { publicKey } = await getFieldCompanionPushVapidPublicKey(accessToken)
    const registration = await registerFieldCompanionServiceWorker()
    if (!registration) {
      return 'failed'
    }

    await navigator.serviceWorker.ready
    const existing = await registration.pushManager.getSubscription()
    const applicationServerKey = urlBase64ToUint8Array(publicKey)
    const subscription =
      existing ??
      (await registration.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: applicationServerKey as BufferSource,
      }))

    const json = subscription.toJSON()
    if (!json.endpoint || !json.keys?.p256dh || !json.keys.auth) {
      return 'failed'
    }

    await subscribeFieldCompanionPush(accessToken, {
      endpoint: json.endpoint,
      keys: {
        p256dh: json.keys.p256dh,
        auth: json.keys.auth,
      },
      userAgent: formatCurrentFieldCompanionDeviceSourceLabel(),
    })

    return 'subscribed'
  } catch {
    return 'failed'
  }
}

export async function removeFieldCompanionPushSubscription(accessToken: string): Promise<void> {
  if (!isWebPushSupported()) {
    return
  }

  const registration = await navigator.serviceWorker.getRegistration()
  const subscription = await registration?.pushManager.getSubscription()
  if (!subscription) {
    return
  }

  const endpoint = subscription.endpoint
  await unsubscribeFieldCompanionPush(accessToken, { endpoint })
  await subscription.unsubscribe()
}
