import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  applyThemePreference,
  buildThemePreferenceStorageKey,
  loadThemePreference,
  saveThemePreference,
  resolveThemeMode,
} from './theme'

describe('theme preference storage', () => {
  afterEach(() => {
    localStorage.clear()
    document.documentElement.removeAttribute('data-theme')
    document.documentElement.removeAttribute('data-theme-preference')
    document.documentElement.classList.remove('dark', 'light')
    document.documentElement.style.colorScheme = ''
    vi.unstubAllGlobals()
  })

  it('defaults to system when no user preference is stored', () => {
    expect(loadThemePreference({ userId: 'user-1', tenantId: 'tenant-1' })).toBe('system')
  })

  it('stores the selected mode per tenant and user', () => {
    const identity = { userId: 'user-1', tenantId: 'tenant-1' }
    const otherUser = { userId: 'user-2', tenantId: 'tenant-1' }

    saveThemePreference('light', identity)

    expect(localStorage.getItem(buildThemePreferenceStorageKey(identity))).toBe('light')
    expect(loadThemePreference(identity)).toBe('light')
    expect(loadThemePreference(otherUser)).toBe('system')
  })

  it('applies the selected mode to the document root', () => {
    applyThemePreference('light')

    expect(document.documentElement.dataset.theme).toBe('light')
    expect(document.documentElement.classList.contains('light')).toBe(true)
    expect(document.documentElement.classList.contains('dark')).toBe(false)
    expect(document.documentElement.style.colorScheme).toBe('light')
  })

  it('resolves system mode using the operating system color scheme', () => {
    vi.stubGlobal('matchMedia', (query: string) => ({
      media: query,
      matches: true,
      onchange: null,
      addEventListener: () => undefined,
      removeEventListener: () => undefined,
      dispatchEvent: () => false,
    }))

    expect(resolveThemeMode('system')).toBe('dark')
  })
})
