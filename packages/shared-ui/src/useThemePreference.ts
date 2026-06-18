import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  applyThemePreference,
  buildThemePreferenceStorageKey,
  loadThemePreference,
  normalizeThemeMode,
  parseThemeMode,
  readThemePreferenceFromStorageEvent,
  saveThemePreference,
  THEME_PREFERENCE_CHANGED_EVENT,
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
    [options?.tenantId, options?.userId],
  )
  const [theme, setThemeState] = useState<StlThemeMode>(
    () => parseThemeMode(options?.initialTheme) ?? loadThemePreference(options),
  )

  useEffect(() => {
    const initialTheme = parseThemeMode(options?.initialTheme)
    const next = initialTheme ?? loadThemePreference(options)
    if (initialTheme) {
      saveThemePreference(initialTheme, options)
    }
    setThemeState(next)
    applyThemePreference(next)
  }, [options?.initialTheme, options?.tenantId, options?.userId, storageKey])

  useEffect(() => {
    function handleStorage(event: StorageEvent) {
      const next = readThemePreferenceFromStorageEvent(event, storageKey)
      if (!next) {
        return
      }
      setThemeState(next)
      applyThemePreference(next)
    }

    function handleLocalChange(event: Event) {
      const change = (event as CustomEvent<ThemePreferenceChange>).detail
      if (change?.storageKey !== storageKey) {
        return
      }
      const next = normalizeThemeMode(change.theme)
      setThemeState(next)
      applyThemePreference(next)
    }

    window.addEventListener('storage', handleStorage)
    window.addEventListener(THEME_PREFERENCE_CHANGED_EVENT, handleLocalChange)
    return () => {
      window.removeEventListener('storage', handleStorage)
      window.removeEventListener(THEME_PREFERENCE_CHANGED_EVENT, handleLocalChange)
    }
  }, [storageKey])

  const setTheme = useCallback(
    (nextTheme: StlThemeMode) => {
      const next = normalizeThemeMode(nextTheme)
      saveThemePreference(next, options)
      setThemeState(next)
      applyThemePreference(next)

      void Promise.resolve(options?.onThemeChange?.(next)).catch(() => undefined)
    },
    [options?.onThemeChange, options?.tenantId, options?.userId],
  )

  const toggleTheme = useCallback(() => {
    setTheme(theme === 'dark' ? 'light' : 'dark')
  }, [setTheme, theme])

  return { theme, setTheme, toggleTheme }
}
