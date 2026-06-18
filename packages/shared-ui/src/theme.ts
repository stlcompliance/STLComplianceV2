export type StlThemeMode = 'dark' | 'light'

export type ThemePreferenceIdentity = {
  userId?: string | null
  tenantId?: string | null
}

export type ThemePreferenceChange = {
  theme: StlThemeMode
  storageKey: string
}

export const DEFAULT_THEME_MODE: StlThemeMode = 'dark'
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

export function parseThemeMode(value: unknown): StlThemeMode | null {
  if (typeof value !== 'string') {
    return null
  }

  const normalized = value.trim().toLowerCase()
  return normalized === 'dark' || normalized === 'light' ? normalized : null
}

export function normalizeThemeMode(value: unknown): StlThemeMode {
  return parseThemeMode(value) ?? DEFAULT_THEME_MODE
}

export function buildThemePreferenceStorageKey(identity?: ThemePreferenceIdentity): string {
  const tenantKey = normalizeKeyPart(identity?.tenantId, 'anonymous-tenant')
  const userKey = normalizeKeyPart(identity?.userId, 'anonymous-user')
  return `${STORAGE_PREFIX}:tenant:${tenantKey}:user:${userKey}`
}

export function loadThemePreference(identity?: ThemePreferenceIdentity): StlThemeMode {
  const storage = getStorage()
  if (!storage) {
    return DEFAULT_THEME_MODE
  }

  return normalizeThemeMode(storage.getItem(buildThemePreferenceStorageKey(identity)))
}

export function applyThemePreference(theme: StlThemeMode): void {
  const root = globalThis.document?.documentElement
  if (!root) {
    return
  }

  root.dataset.theme = theme
  root.classList.toggle('dark', theme === 'dark')
  root.classList.toggle('light', theme === 'light')
  root.style.colorScheme = theme
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
): StlThemeMode | null {
  const theme = parseThemeMode(session?.themePreference)
  if (!theme) {
    return null
  }

  saveThemePreference(theme, {
    tenantId: session?.tenantId,
    userId: session?.userId,
  })
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
