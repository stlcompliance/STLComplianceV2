import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  applyThemePreference,
  buildThemePreferenceStorageKey,
  loadThemePreference,
  normalizeThemeMode,
  parseThemeMode,
  resolveThemeMode,
  readThemePreferenceFromStorageEvent,
  readStoredThemePreferenceDetails,
  readStoredThemePreference,
  saveThemePreference,
  THEME_PREFERENCE_CHANGED_EVENT,
  DEFAULT_THEME_MODE,
  type StlThemeMode,
  type ThemePreferenceChange,
  type ThemePreferenceIdentity,
} from './theme'

export type UseThemePreferenceOptions = ThemePreferenceIdentity & {
  initialTheme?: unknown
  onThemeChange?: (theme: StlThemeMode) => void | Promise<void>
}

export function useThemePreference(options?: UseThemePreferenceOptions) {
  const storageKey = useMemo(
    () => buildThemePreferenceStorageKey(options),
    [options?.appKey, options?.tenantId, options?.userId],
  )
  const storedPreference = useMemo(
    () => readStoredThemePreference(options),
    [options?.appKey, options?.tenantId, options?.userId],
  )
  const initialPreference =
    storedPreference ?? parseThemeMode(options?.initialTheme) ?? DEFAULT_THEME_MODE
  const [preference, setPreferenceState] = useState<StlThemeMode>(initialPreference)
  const [resolvedTheme, setResolvedThemeState] = useState(() => resolveThemeMode(initialPreference))

  useEffect(() => {
    const stored = readStoredThemePreferenceDetails(options)
    const initialTheme = parseThemeMode(options?.initialTheme)
    const next = stored?.theme ?? initialTheme ?? loadThemePreference(options)
    if (stored?.isLegacy && options?.appKey) {
      saveThemePreference(stored.theme, options)
    } else if (!stored && initialTheme) {
      saveThemePreference(initialTheme, options)
    }
    setPreferenceState(next)
    setResolvedThemeState(resolveThemeMode(next))
    applyThemePreference(next)
  }, [options?.appKey, options?.initialTheme, options?.tenantId, options?.userId, storageKey])

  useEffect(() => {
    function syncTheme(nextTheme: StlThemeMode) {
      setPreferenceState(nextTheme)
      setResolvedThemeState(resolveThemeMode(nextTheme))
      applyThemePreference(nextTheme)
    }

    function handleStorage(event: StorageEvent) {
      const next = readThemePreferenceFromStorageEvent(event, storageKey)
      if (!next) {
        return
      }
      syncTheme(next)
    }

    function handleLocalChange(event: Event) {
      const change = (event as CustomEvent<ThemePreferenceChange>).detail
      if (change?.storageKey !== storageKey) {
        return
      }
      const next = normalizeThemeMode(change.theme)
      syncTheme(next)
    }

    function handleSystemThemeChange() {
      if (preference !== 'system') {
        return
      }
      setResolvedThemeState(resolveThemeMode(preference))
      applyThemePreference(preference)
    }

    window.addEventListener('storage', handleStorage)
    window.addEventListener(THEME_PREFERENCE_CHANGED_EVENT, handleLocalChange)

    const mediaQuery =
      typeof globalThis.matchMedia === 'function'
        ? globalThis.matchMedia('(prefers-color-scheme: dark)')
        : null
    mediaQuery?.addEventListener('change', handleSystemThemeChange)

    return () => {
      window.removeEventListener('storage', handleStorage)
      window.removeEventListener(THEME_PREFERENCE_CHANGED_EVENT, handleLocalChange)
      mediaQuery?.removeEventListener('change', handleSystemThemeChange)
    }
  }, [preference, storageKey])

  const setTheme = useCallback(
    (nextTheme: StlThemeMode) => {
      const next = normalizeThemeMode(nextTheme)
      saveThemePreference(next, options)
      setPreferenceState(next)
      setResolvedThemeState(resolveThemeMode(next))
      applyThemePreference(next)

      void Promise.resolve(options?.onThemeChange?.(next)).catch(() => undefined)
    },
    [options?.appKey, options?.onThemeChange, options?.tenantId, options?.userId],
  )

  const toggleTheme = useCallback(() => {
    setTheme(resolvedTheme === 'dark' ? 'light' : 'dark')
  }, [resolvedTheme, setTheme])

  return { theme: resolvedTheme, preference, setTheme, toggleTheme }
}
