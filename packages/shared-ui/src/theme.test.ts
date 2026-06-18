import { afterEach, describe, expect, it } from 'vitest'
import {
  applyThemePreference,
  buildThemePreferenceStorageKey,
  loadThemePreference,
  saveThemePreference,
} from './theme'

describe('theme preference storage', () => {
  afterEach(() => {
    localStorage.clear()
    document.documentElement.removeAttribute('data-theme')
    document.documentElement.classList.remove('dark', 'light')
    document.documentElement.style.colorScheme = ''
  })

  it('defaults to dark when no user preference is stored', () => {
    expect(loadThemePreference({ userId: 'user-1', tenantId: 'tenant-1' })).toBe('dark')
  })

  it('stores the selected mode per tenant and user', () => {
    const identity = { userId: 'user-1', tenantId: 'tenant-1' }
    const otherUser = { userId: 'user-2', tenantId: 'tenant-1' }

    saveThemePreference('light', identity)

    expect(localStorage.getItem(buildThemePreferenceStorageKey(identity))).toBe('light')
    expect(loadThemePreference(identity)).toBe('light')
    expect(loadThemePreference(otherUser)).toBe('dark')
  })

  it('applies the selected mode to the document root', () => {
    applyThemePreference('light')

    expect(document.documentElement.dataset.theme).toBe('light')
    expect(document.documentElement.classList.contains('light')).toBe(true)
    expect(document.documentElement.classList.contains('dark')).toBe(false)
    expect(document.documentElement.style.colorScheme).toBe('light')
  })
})
