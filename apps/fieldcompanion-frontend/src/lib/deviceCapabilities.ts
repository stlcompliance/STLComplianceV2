import type { PushPermissionState } from './pushNotifications'
import { getPushPermissionState, isWebPushSupported, pushReadinessLabel } from './pushNotifications'

export type DeviceCapabilityStatus = 'ready' | 'degraded' | 'unsupported'

export interface FieldCompanionNetworkProfile {
  supported: boolean
  saveData: boolean
  effectiveType: string | null
  downlinkMbps: number | null
}

export function resolveFieldCompanionAppVersion(): string {
  return import.meta.env.VITE_FIELD_COMPANION_APP_VERSION ?? import.meta.env.MODE ?? 'unknown'
}

export interface DeviceCapabilityEnvironment {
  appVersion: string
  browserUserAgent: string
  connectionDownlinkMbps: number | null
  connectionEffectiveType: string | null
  connectionSaveData: boolean
  connectionSupported: boolean
  language: string
  online: boolean
  platform: string
  installedMode: boolean
  serviceWorkerSupported: boolean
  backgroundSyncSupported: boolean
  pushPermission: PushPermissionState
  pushSupported: boolean
  cameraSupported: boolean
  geolocationSupported: boolean
  storageSupported: boolean
  permissionApiSupported: boolean
}

export interface DeviceCapabilityItem {
  key: string
  label: string
  status: DeviceCapabilityStatus
  value: string
  fallback: string
}

export interface DeviceCapabilitySnapshot {
  appVersion: string
  browserUserAgent: string
  checkedAt: string
  capabilities: DeviceCapabilityItem[]
  language: string
  online: boolean
  platform: string
  warnings: string[]
}

function hasAccessibleStorage(): boolean {
  if (typeof window === 'undefined' || !('localStorage' in window) || !('indexedDB' in window)) {
    return false
  }

  try {
    const key = 'stl.fieldcompanion.device-capability-check'
    window.localStorage.setItem(key, '1')
    window.localStorage.removeItem(key)
    return true
  } catch {
    return false
  }
}

function isInstalledMode(): boolean {
  if (typeof window === 'undefined') {
    return false
  }

  const navigatorWithStandalone = navigator as Navigator & { standalone?: boolean }
  if (navigatorWithStandalone.standalone === true) {
    return true
  }

  if (typeof window.matchMedia === 'function') {
    try {
      return window.matchMedia('(display-mode: standalone)').matches
    } catch {
      return false
    }
  }

  return false
}

export function readCurrentFieldCompanionNetworkProfile(): FieldCompanionNetworkProfile {
  const connection = typeof navigator !== 'undefined'
    ? (navigator as Navigator & {
        connection?: Partial<FieldCompanionNetworkProfile> & {
          effectiveType?: string
          downlink?: number
        }
      }).connection
    : undefined

  return {
    supported: Boolean(connection),
    saveData: Boolean(connection?.saveData),
    effectiveType: typeof connection?.effectiveType === 'string' ? connection.effectiveType : null,
    downlinkMbps: typeof connection?.downlink === 'number' ? connection.downlink : null,
  }
}

export function isFieldCompanionLowDataConnection(profile: FieldCompanionNetworkProfile): boolean {
  return (
    profile.saveData
    || profile.effectiveType === 'slow-2g'
    || profile.effectiveType === '2g'
    || (profile.downlinkMbps != null && profile.downlinkMbps < 1.5)
  )
}

function formatFieldCompanionConnectionProfile(profile: FieldCompanionNetworkProfile): string {
  if (!profile.supported) {
    return 'Unavailable'
  }

  if (profile.saveData) {
    return 'Low-data mode'
  }

  if (profile.effectiveType) {
    const suffix = profile.downlinkMbps != null ? ` · ${profile.downlinkMbps.toFixed(profile.downlinkMbps >= 10 ? 0 : 1)} Mbps` : ''
    return `${profile.effectiveType.toUpperCase()}${suffix}`
  }

  if (profile.downlinkMbps != null) {
    return `${profile.downlinkMbps.toFixed(profile.downlinkMbps >= 10 ? 0 : 1)} Mbps`
  }

  return 'Connection hints available'
}

function readCurrentDeviceCapabilityEnvironment(): DeviceCapabilityEnvironment {
  const appVersion = resolveFieldCompanionAppVersion()
  const browserUserAgent = typeof navigator !== 'undefined' ? navigator.userAgent || 'Unknown' : 'Unknown'
  const networkProfile = readCurrentFieldCompanionNetworkProfile()
  const language = typeof navigator !== 'undefined' ? navigator.language || 'Unknown' : 'Unknown'
  const online = typeof navigator !== 'undefined' ? navigator.onLine : true
  const platform = typeof navigator !== 'undefined' ? navigator.platform || 'Unknown' : 'Unknown'
  const serviceWorkerSupported = typeof navigator !== 'undefined' && 'serviceWorker' in navigator
  const backgroundSyncSupported = serviceWorkerSupported && typeof window !== 'undefined' && 'SyncManager' in window
  const pushPermission = getPushPermissionState()
  const pushSupported = isWebPushSupported()
  const cameraSupported = typeof navigator !== 'undefined' && !!navigator.mediaDevices?.getUserMedia
  const geolocationSupported = typeof navigator !== 'undefined' && 'geolocation' in navigator
  const storageSupported = hasAccessibleStorage()
  const permissionApiSupported = typeof navigator !== 'undefined' && 'permissions' in navigator

  return {
    appVersion,
    browserUserAgent,
    connectionDownlinkMbps: networkProfile.downlinkMbps,
    connectionEffectiveType: networkProfile.effectiveType,
    connectionSaveData: networkProfile.saveData,
    connectionSupported: networkProfile.supported,
    language,
    online,
    platform,
    installedMode: isInstalledMode(),
    serviceWorkerSupported,
    backgroundSyncSupported,
    pushPermission,
    pushSupported,
    cameraSupported,
    geolocationSupported,
    storageSupported,
    permissionApiSupported,
  }
}

export function formatDeviceCapabilityDiagnosticSummary(snapshot: DeviceCapabilitySnapshot): string {
  const lines = [
    'Field Companion device diagnostics',
    `App version: ${snapshot.appVersion}`,
    `Browser: ${snapshot.browserUserAgent}`,
    `Platform: ${snapshot.platform}`,
    `Language: ${snapshot.language}`,
    `Online: ${snapshot.online ? 'yes' : 'no'}`,
    `Checked at: ${snapshot.checkedAt}`,
    'Capabilities:',
  ]

  for (const item of snapshot.capabilities) {
    const fallback = item.status === 'ready' ? '' : ` - ${item.fallback}`
    lines.push(`- ${item.label}: ${item.value} (${item.status})${fallback}`)
  }

  if (snapshot.warnings.length > 0) {
    lines.push('Warnings:')
    for (const warning of snapshot.warnings) {
      lines.push(`- ${warning}`)
    }
  }

  return lines.join('\n')
}

function createCapability(
  key: string,
  label: string,
  status: DeviceCapabilityStatus,
  value: string,
  fallback: string,
): DeviceCapabilityItem {
  return { key, label, status, value, fallback }
}

export function buildDeviceCapabilitySnapshot(
  environment: DeviceCapabilityEnvironment = readCurrentDeviceCapabilityEnvironment(),
): DeviceCapabilitySnapshot {
  const connectionProfile: FieldCompanionNetworkProfile = {
    supported: environment.connectionSupported,
    saveData: environment.connectionSaveData,
    effectiveType: environment.connectionEffectiveType,
    downlinkMbps: environment.connectionDownlinkMbps,
  }

  const capabilities = [
    createCapability(
      'install-mode',
      'Install mode',
      'ready',
      environment.installedMode ? 'Installed app' : 'Browser tab',
      environment.installedMode
        ? 'The installed shell keeps the field experience compact and app-like.'
        : 'Install to the home screen for faster launches and a more app-like offline flow.',
    ),
    createCapability(
      'network',
      'Network',
      environment.online ? 'ready' : 'degraded',
      environment.online ? 'Online' : 'Offline',
      environment.online
        ? 'No fallback required.'
        : 'Queued actions will sync automatically when the connection returns.',
    ),
    createCapability(
      'connection',
      'Connection profile',
      environment.connectionSupported
        ? isFieldCompanionLowDataConnection(connectionProfile)
          ? 'degraded'
          : 'ready'
        : 'unsupported',
      formatFieldCompanionConnectionProfile(connectionProfile),
      environment.connectionSupported
        ? isFieldCompanionLowDataConnection(connectionProfile)
          ? 'Use smaller photos, shorter uploads, and retry large work on a stronger connection.'
          : 'The browser can report connection hints for adaptive uploads.'
        : 'Use smaller photos, shorter uploads, and manual retries on this device.',
    ),
    createCapability(
      'push',
      'Push notifications',
      environment.pushSupported && environment.pushPermission === 'granted'
        ? 'ready'
        : environment.pushSupported
          ? 'degraded'
          : 'unsupported',
      pushReadinessLabel(environment.pushPermission),
      environment.pushSupported
        ? environment.pushPermission === 'default'
          ? 'Request permission from Notifications when you want live updates.'
          : 'Use the in-app inbox if browser notifications stay blocked.'
        : 'Use the in-app inbox and manual refresh instead.',
    ),
    createCapability(
      'service-worker',
      'Service worker',
      environment.serviceWorkerSupported ? 'ready' : 'unsupported',
      environment.serviceWorkerSupported ? 'Available' : 'Unavailable',
      environment.serviceWorkerSupported
        ? 'Background caching is available for the installed shell.'
        : 'Offline caching is limited; keep the app open and sync manually.',
    ),
    createCapability(
      'background-sync',
      'Background sync',
      environment.backgroundSyncSupported
        ? 'ready'
        : environment.serviceWorkerSupported
          ? 'degraded'
          : 'unsupported',
      environment.backgroundSyncSupported
        ? 'Available'
        : environment.serviceWorkerSupported
          ? 'Manual sync only'
          : 'Unavailable',
      environment.backgroundSyncSupported
        ? 'Queued actions can retry automatically after reconnect.'
        : environment.serviceWorkerSupported
          ? 'Refresh the queue when connectivity returns.'
          : 'Use the open page to sync after reconnecting.',
    ),
    createCapability(
      'camera',
      'Camera capture',
      environment.cameraSupported ? 'ready' : 'unsupported',
      environment.cameraSupported ? 'Available' : 'Unavailable',
      environment.cameraSupported
        ? 'Photo and video capture can use the device camera.'
        : 'Use file upload or typed notes instead.',
    ),
    createCapability(
      'location',
      'Location services',
      environment.geolocationSupported ? 'ready' : 'unsupported',
      environment.geolocationSupported ? 'Available' : 'Unavailable',
      environment.geolocationSupported
        ? 'Location prompts can support site-aware workflows when permitted.'
        : 'Use manual site selection, QR scan, or typed location entry.',
    ),
    createCapability(
      'storage',
      'Storage',
      environment.storageSupported ? 'ready' : 'unsupported',
      environment.storageSupported ? 'Available' : 'Unavailable',
      environment.storageSupported
        ? 'Local persistence is available for offline work.'
        : 'Use a browser with persistent storage for queued actions.',
    ),
    createCapability(
      'permissions',
      'Permissions API',
      environment.permissionApiSupported ? 'ready' : 'unsupported',
      environment.permissionApiSupported ? 'Available' : 'Unavailable',
      environment.permissionApiSupported
        ? 'The browser can report permission state before prompting.'
        : 'Permission checks will rely on browser prompt flows.',
    ),
  ]

  return {
    appVersion: environment.appVersion,
    browserUserAgent: environment.browserUserAgent,
    checkedAt: new Date().toISOString(),
    capabilities,
    language: environment.language,
    online: environment.online,
    platform: environment.platform,
    warnings: capabilities.filter((item) => item.status !== 'ready').map((item) => item.fallback),
  }
}
