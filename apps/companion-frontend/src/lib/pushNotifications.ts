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
