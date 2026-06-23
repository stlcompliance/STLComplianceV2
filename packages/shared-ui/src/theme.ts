export type StlThemeMode = 'dark' | 'light' | 'system'
export type ResolvedThemeMode = 'dark' | 'light'

export type ThemePreferenceIdentity = {
  userId?: string | null
  tenantId?: string | null
  appKey?: string | null
}

export type ThemePreferenceChange = {
  theme: StlThemeMode
  storageKey: string
}

export const DEFAULT_THEME_MODE: StlThemeMode = 'system'
export const THEME_PREFERENCE_CHANGED_EVENT = 'stl-theme-preference-changed'

const STORAGE_PREFIX = 'stl.theme.preference.v1'
const THEME_CHANGE_STORAGE_KEY = 'stl.theme.preference.change.v1'

function getStorage(): Storage | null {
  try {
    return globalThis.localStorage ?? null
  } catch {
    return null
  }
}

function normalizeKeyPart(value: string | null | undefined, fallback: string): string {
  const trimmed = value?.trim()
  return trimmed ? trimmed.toLowerCase() : fallback
}

function readThemePreferenceFromStorage(storageKey: string): StlThemeMode | null {
  const storage = getStorage()
  if (!storage) {
    return null
  }

  return parseThemeMode(storage.getItem(storageKey))
}

function buildLegacyThemePreferenceStorageKey(identity?: ThemePreferenceIdentity): string {
  const tenantKey = normalizeKeyPart(identity?.tenantId, 'anonymous-tenant')
  const userKey = normalizeKeyPart(identity?.userId, 'anonymous-user')
  return `${STORAGE_PREFIX}:tenant:${tenantKey}:user:${userKey}`
}

type StoredThemePreferenceRecord = {
  theme: StlThemeMode
  isLegacy: boolean
}

export function parseThemeMode(value: unknown): StlThemeMode | null {
  if (typeof value !== 'string') {
    return null
  }

  const normalized = value.trim().toLowerCase()
  return normalized === 'dark' || normalized === 'light' || normalized === 'system'
    ? normalized
    : null
}

export function normalizeThemeMode(value: unknown): StlThemeMode {
  return parseThemeMode(value) ?? DEFAULT_THEME_MODE
}

export function resolveThemeMode(theme: StlThemeMode): ResolvedThemeMode {
  if (theme === 'dark' || theme === 'light') {
    return theme
  }

  const prefersDark =
    typeof globalThis.matchMedia === 'function'
      ? globalThis.matchMedia('(prefers-color-scheme: dark)').matches
      : true
  return prefersDark ? 'dark' : 'light'
}

export function buildThemePreferenceStorageKey(identity?: ThemePreferenceIdentity): string {
  const appKey = normalizeKeyPart(identity?.appKey, 'suite')
  const tenantKey = normalizeKeyPart(identity?.tenantId, 'anonymous-tenant')
  const userKey = normalizeKeyPart(identity?.userId, 'anonymous-user')
  return `${STORAGE_PREFIX}:app:${appKey}:tenant:${tenantKey}:user:${userKey}`
}

export function readStoredThemePreferenceDetails(
  identity?: ThemePreferenceIdentity,
): StoredThemePreferenceRecord | null {
  const currentKey = buildThemePreferenceStorageKey(identity)
  const current = readThemePreferenceFromStorage(currentKey)
  if (current) {
    return { theme: current, isLegacy: false }
  }

  const legacy = readThemePreferenceFromStorage(buildLegacyThemePreferenceStorageKey(identity))
  if (legacy) {
    return { theme: legacy, isLegacy: true }
  }

  return null
}

export function readStoredThemePreference(identity?: ThemePreferenceIdentity): StlThemeMode | null {
  return readStoredThemePreferenceDetails(identity)?.theme ?? null
}

export function loadThemePreference(identity?: ThemePreferenceIdentity): StlThemeMode {
  return readStoredThemePreference(identity) ?? DEFAULT_THEME_MODE
}

export function applyThemePreference(theme: StlThemeMode): void {
  const resolved = resolveThemeMode(theme)
  const root = globalThis.document?.documentElement
  if (!root) {
    return
  }

  root.dataset.theme = resolved
  root.dataset.themePreference = theme
  root.classList.toggle('dark', resolved === 'dark')
  root.classList.toggle('light', resolved === 'light')
  root.style.colorScheme = resolved
}

export function saveThemePreference(
  theme: StlThemeMode,
  identity?: ThemePreferenceIdentity,
): ThemePreferenceChange {
  const normalized = normalizeThemeMode(theme)
  const storageKey = buildThemePreferenceStorageKey(identity)
  const change: ThemePreferenceChange = { theme: normalized, storageKey }
  const storage = getStorage()

  if (storage) {
    storage.setItem(storageKey, normalized)
    storage.setItem(
      THEME_CHANGE_STORAGE_KEY,
      JSON.stringify({ ...change, changedAt: new Date().toISOString() }),
    )
  }

  globalThis.window?.dispatchEvent(
    new CustomEvent<ThemePreferenceChange>(THEME_PREFERENCE_CHANGED_EVENT, {
      detail: change,
    }),
  )

  return change
}

export function saveThemePreferenceFromSession(
  session: (ThemePreferenceIdentity & { themePreference?: unknown }) | null | undefined,
  identity?: ThemePreferenceIdentity,
): StlThemeMode | null {
  const targetIdentity = identity ?? {
    tenantId: session?.tenantId,
    userId: session?.userId,
  }

  const existingTheme = readStoredThemePreferenceDetails(targetIdentity)
  if (existingTheme) {
    if (existingTheme.isLegacy && targetIdentity.appKey) {
      saveThemePreference(existingTheme.theme, targetIdentity)
    }
    applyThemePreference(existingTheme.theme)
    return existingTheme.theme
  }

  const theme = parseThemeMode(session?.themePreference)
  if (!theme) {
    return null
  }

  if (identity?.appKey) {
    saveThemePreference(theme, targetIdentity)
  }
  applyThemePreference(theme)
  return theme
}

export function initializeSuiteTheme(
  identity?: ThemePreferenceIdentity,
  fallbackTheme: StlThemeMode = DEFAULT_THEME_MODE,
): StlThemeMode {
  const theme = loadThemePreference(identity) ?? fallbackTheme
  applyThemePreference(theme)
  return theme
}

export async function updatePlatformThemePreference(
  apiBase: string,
  accessToken: string,
  theme: StlThemeMode,
): Promise<void> {
  const base = apiBase.replace(/\/$/, '')
  const response = await fetch(`${base}/api/me/preferences`, {
    method: 'PATCH',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({ themePreference: normalizeThemeMode(theme) }),
  })

  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `Failed to update theme preference (${response.status})`)
  }
}

export function readThemePreferenceFromStorageEvent(
  event: StorageEvent,
  storageKey: string,
): StlThemeMode | null {
  if (event.key === storageKey) {
    return normalizeThemeMode(event.newValue)
  }

  if (event.key !== THEME_CHANGE_STORAGE_KEY || !event.newValue) {
    return null
  }

  try {
    const change = JSON.parse(event.newValue) as Partial<ThemePreferenceChange>
    if (change.storageKey === storageKey) {
      return normalizeThemeMode(change.theme)
    }
  } catch {
    return null
  }

  return null
}
