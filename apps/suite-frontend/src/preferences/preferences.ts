import { useEffect, useMemo, useState } from 'react'
import { updateMyPreferences } from '../api/nexarrClient'
import {
  normalizeThemeMode,
  saveThemePreference,
  type StlThemeMode,
} from '@stl/shared-ui/theme'

export type SuitePreferences = {
  theme: StlThemeMode
  density: 'comfortable' | 'compact'
  reducedMotion: boolean
  highContrast: boolean
  timeZone: string
  dateFormat: 'MM/DD/YYYY' | 'DD/MM/YYYY' | 'YYYY-MM-DD'
  timeFormat: '12h' | '24h'
  numberFormat: 'system' | 'en-US' | 'de-DE'
  assistantDefaultVerbosity: 'concise' | 'normal' | 'detailed'
  assistantShowAssumptions: boolean
}

export type NexarrPreferences = {
  defaultLandingProduct: string
  launcherOrder: 'recommended' | 'alphabetical'
  defaultHomeView: 'dashboard' | 'launchpad'
  productAccessAlerts: boolean
  assistantLaunchBehavior: 'remember-last' | 'fresh-open'
}

export type PreferenceHookResult<TPreferences> = {
  preferences: TPreferences
  setPreference: <K extends keyof TPreferences>(key: K, value: TPreferences[K]) => void
  reset: () => void
  save: () => Promise<void>
  isLoading: boolean
  isSaving: boolean
  isDirty: boolean
  error: string | null
}

const suitePreferenceDefaults: SuitePreferences = {
  theme: 'system',
  density: 'comfortable',
  reducedMotion: false,
  highContrast: false,
  timeZone: 'system',
  dateFormat: 'MM/DD/YYYY',
  timeFormat: '12h',
  numberFormat: 'system',
  assistantDefaultVerbosity: 'normal',
  assistantShowAssumptions: true,
}

const nexarrPreferenceDefaults: NexarrPreferences = {
  defaultLandingProduct: 'dashboard',
  launcherOrder: 'recommended',
  defaultHomeView: 'launchpad',
  productAccessAlerts: true,
  assistantLaunchBehavior: 'remember-last',
}

function buildPreferenceStorageKey(scope: 'suite' | 'product', tenantId: string, personId: string, productKey?: string) {
  return scope === 'suite'
    ? `stl.preferences.suite.v1:${tenantId}:${personId}`
    : `stl.preferences.product.v1:${tenantId}:${personId}:${productKey ?? 'unknown'}`
}

function readPreferenceSnapshot<T extends Record<string, unknown>>(storageKey: string, defaults: T): T {
  if (typeof globalThis.localStorage === 'undefined') {
    return { ...defaults }
  }

  try {
    const raw = globalThis.localStorage.getItem(storageKey)
    if (!raw) {
      return { ...defaults }
    }
    const parsed = JSON.parse(raw) as Partial<T>
    return { ...defaults, ...parsed }
  } catch {
    return { ...defaults }
  }
}

function writePreferenceSnapshot<T extends Record<string, unknown>>(storageKey: string, preferences: T): void {
  try {
    globalThis.localStorage?.setItem(storageKey, JSON.stringify(preferences))
  } catch {
    // Ignore storage failures in private browsing or locked-down environments.
  }
}

function usePreferenceDraft<TPreferences extends Record<string, unknown>>(
  storageKey: string,
  defaults: TPreferences,
): PreferenceHookResult<TPreferences> {
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [baseline, setBaseline] = useState<TPreferences>(() => ({ ...defaults }))
  const [preferences, setPreferences] = useState<TPreferences>(() => ({ ...defaults }))

  useEffect(() => {
    const next = readPreferenceSnapshot(storageKey, defaults)
    setBaseline(next)
    setPreferences(next)
    setIsLoading(false)
  }, [defaults, storageKey])

  const isDirty = useMemo(
    () => JSON.stringify(preferences) !== JSON.stringify(baseline),
    [baseline, preferences],
  )

  const setPreference = <K extends keyof TPreferences>(key: K, value: TPreferences[K]) => {
    setPreferences((current) => ({ ...current, [key]: value }))
  }

  const reset = () => {
    setPreferences({ ...defaults })
    setError(null)
  }

  const save = async () => {
    setIsSaving(true)
    setError(null)
    try {
      writePreferenceSnapshot(storageKey, preferences)
      setBaseline(preferences)
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'Failed to save preferences.')
      throw cause
    } finally {
      setIsSaving(false)
    }
  }

  return {
    preferences,
    setPreference,
    reset,
    save,
    isLoading,
    isSaving,
    isDirty,
    error,
  }
}

export function useSuitePreferences(options: {
  tenantId?: string | null
  personId?: string | null
  initialTheme?: unknown
}): PreferenceHookResult<SuitePreferences> {
  const resolvedDefaults = useMemo(
    () => ({
      ...suitePreferenceDefaults,
      theme: normalizeThemeMode(options.initialTheme),
    }),
    [options.initialTheme],
  )
  const storageKey = buildPreferenceStorageKey(
    'suite',
    options.tenantId ?? 'anonymous-tenant',
    options.personId ?? 'anonymous-person',
  )
  const hook = usePreferenceDraft(storageKey, resolvedDefaults)
  const themeIdentity = {
    tenantId: options.tenantId,
    userId: options.personId,
    appKey: 'suite',
  }

  const save = async () => {
    const nextTheme = normalizeThemeMode(hook.preferences.theme)
    saveThemePreference(nextTheme, themeIdentity)
    await updateMyPreferences({ themePreference: nextTheme })
    await hook.save()
  }

  const reset = () => {
    hook.reset()
    hook.setPreference('theme', resolvedDefaults.theme)
  }

  return {
    ...hook,
    save,
    reset,
    preferences: hook.preferences,
  }
}

export function useCurrentProductPreferences(options: {
  tenantId?: string | null
  personId?: string | null
  productKey: string
}): PreferenceHookResult<NexarrPreferences> {
  const storageKey = buildPreferenceStorageKey(
    'product',
    options.tenantId ?? 'anonymous-tenant',
    options.personId ?? 'anonymous-person',
    options.productKey,
  )
  return usePreferenceDraft(storageKey, nexarrPreferenceDefaults)
}

export const suitePreferenceOptions = {
  themes: ['dark', 'light', 'system'] as const,
  densities: ['comfortable', 'compact'] as const,
  dateFormats: ['MM/DD/YYYY', 'DD/MM/YYYY', 'YYYY-MM-DD'] as const,
  timeFormats: ['12h', '24h'] as const,
  numberFormats: ['system', 'en-US', 'de-DE'] as const,
  assistantVerbosity: ['concise', 'normal', 'detailed'] as const,
} as const

export const nexarrPreferenceOptions = {
  defaultLandingProducts: [
    { value: 'dashboard', label: 'Suite dashboard' },
    { value: 'imports', label: 'Smart Import' },
    { value: 'platform-admin', label: 'Platform admin' },
  ],
  launcherOrders: [
    { value: 'recommended', label: 'Recommended order' },
    { value: 'alphabetical', label: 'Alphabetical' },
  ],
  homeViews: [
    { value: 'launchpad', label: 'Launchpad' },
    { value: 'dashboard', label: 'Dashboard' },
  ],
  assistantLaunchBehaviors: [
    { value: 'remember-last', label: 'Remember last assistant state' },
    { value: 'fresh-open', label: 'Start fresh each time' },
  ],
} as const
