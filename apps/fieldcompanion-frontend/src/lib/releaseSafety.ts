import { resolveFieldCompanionAppVersion } from './deviceCapabilities'

export type FieldCompanionReleaseMode = 'ready' | 'paused' | 'required'
export type FieldCompanionReleaseTone = 'info' | 'warning' | 'error'

export interface FieldCompanionReleaseSafetyConfig {
  appVersion: string
  minimumSupportedVersion: string | null
  releaseMode: FieldCompanionReleaseMode
  releaseMessage: string | null
  stagedFlags: string[]
  killSwitches: string[]
}

export interface FieldCompanionReleaseSafetySnapshot extends FieldCompanionReleaseSafetyConfig {
  isUpdateRequired: boolean
  isPaused: boolean
  isActionBlocked: boolean
  tone: FieldCompanionReleaseTone
  title: string
  message: string
}

export function parseFieldCompanionReleaseCsv(value: string | null | undefined): string[] {
  return (value ?? '')
    .split(',')
    .map((item) => item.trim())
    .filter((item) => item.length > 0)
}

function parseVersionParts(value: string): number[] | null {
  const normalized = value.trim().replace(/^v/i, '')
  if (!/^\d+(\.\d+)*$/.test(normalized)) {
    return null
  }

  return normalized.split('.').map((part) => Number.parseInt(part, 10))
}

export function compareFieldCompanionReleaseVersions(
  currentVersion: string,
  minimumVersion: string,
): number | null {
  const currentParts = parseVersionParts(currentVersion)
  const minimumParts = parseVersionParts(minimumVersion)

  if (!currentParts || !minimumParts) {
    return null
  }

  const maxLength = Math.max(currentParts.length, minimumParts.length)
  for (let index = 0; index < maxLength; index += 1) {
    const current = currentParts[index] ?? 0
    const minimum = minimumParts[index] ?? 0

    if (current < minimum) {
      return -1
    }

    if (current > minimum) {
      return 1
    }
  }

  return 0
}

export function buildFieldCompanionReleaseSafetySnapshot(
  config: FieldCompanionReleaseSafetyConfig,
): FieldCompanionReleaseSafetySnapshot {
  const comparison =
    config.minimumSupportedVersion != null
      ? compareFieldCompanionReleaseVersions(config.appVersion, config.minimumSupportedVersion)
      : 0

  const isUpdateRequired =
    config.releaseMode === 'required' ||
    (config.minimumSupportedVersion != null && comparison != null && comparison < 0)

  const isPaused = config.releaseMode === 'paused'
  const isActionBlocked = isUpdateRequired
  const tone: FieldCompanionReleaseTone = isActionBlocked
    ? 'error'
    : isPaused
      ? 'warning'
      : 'info'

  const title = isActionBlocked
    ? 'Update required'
    : isPaused
      ? 'Release paused'
      : config.stagedFlags.length > 0 || config.killSwitches.length > 0
        ? 'Release staged'
        : 'Release ready'

  const message =
    config.releaseMessage ??
    (isActionBlocked
      ? config.minimumSupportedVersion != null && comparison != null && comparison < 0
        ? `This build (${config.appVersion}) is below the minimum supported version (${config.minimumSupportedVersion}).`
        : 'This build is not permitted right now.'
      : isPaused
        ? 'The current release is paused. You can keep the shell open, but affected workflows may be limited.'
        : 'The current build is within the supported release window.')

  return {
    ...config,
    isUpdateRequired,
    isPaused,
    isActionBlocked,
    tone,
    title,
    message,
  }
}

export function readCurrentFieldCompanionReleaseSafetySnapshot(): FieldCompanionReleaseSafetySnapshot {
  return buildFieldCompanionReleaseSafetySnapshot({
    appVersion: resolveFieldCompanionAppVersion(),
    minimumSupportedVersion: import.meta.env.VITE_FIELD_COMPANION_MIN_APP_VERSION?.trim() || null,
    releaseMode: ((import.meta.env.VITE_FIELD_COMPANION_RELEASE_MODE?.trim() || 'ready') as FieldCompanionReleaseMode),
    releaseMessage: import.meta.env.VITE_FIELD_COMPANION_RELEASE_MESSAGE?.trim() || null,
    stagedFlags: parseFieldCompanionReleaseCsv(import.meta.env.VITE_FIELD_COMPANION_STAGED_FLAGS),
    killSwitches: parseFieldCompanionReleaseCsv(import.meta.env.VITE_FIELD_COMPANION_KILL_SWITCHES),
  })
}
