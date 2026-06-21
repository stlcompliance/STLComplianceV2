import { afterEach, describe, expect, it, vi } from 'vitest'
import { readFileSync } from 'node:fs'
import {
  applyThemePreference,
  buildThemePreferenceStorageKey,
  loadThemePreference,
  saveThemePreference,
  saveThemePreferenceFromSession,
  resolveThemeMode,
} from './theme'

const themeCss = readFileSync('src/theme.css', 'utf8')

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

  it('keeps explicit light and dark selections authoritative over OS preference', () => {
    vi.stubGlobal('matchMedia', (query: string) => ({
      media: query,
      matches: true,
      onchange: null,
      addEventListener: () => undefined,
      removeEventListener: () => undefined,
      dispatchEvent: () => false,
    }))

    expect(resolveThemeMode('light')).toBe('light')
    expect(resolveThemeMode('dark')).toBe('dark')
  })

  it('loads a session preference into storage and applies its resolved root theme', () => {
    const identity = { userId: 'user-1', tenantId: 'tenant-1' }

    expect(saveThemePreferenceFromSession({ ...identity, themePreference: 'dark' })).toBe('dark')

    expect(localStorage.getItem(buildThemePreferenceStorageKey(identity))).toBe('dark')
    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(document.documentElement.dataset.themePreference).toBe('dark')
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

  it('defines required shared semantic theme tokens', () => {
    const requiredTokens = [
      '--color-page-bg',
      '--color-bg-surface',
      '--color-bg-surface-elevated',
      '--color-bg-surface-muted',
      '--color-field-bg',
      '--color-table-bg',
      '--color-table-header-bg',
      '--color-table-row-hover-bg',
      '--color-table-row-selected-bg',
      '--color-border-default',
      '--color-border-subtle',
      '--color-border-strong',
      '--color-text-primary',
      '--color-text-secondary',
      '--color-text-muted',
      '--color-text-disabled',
      '--color-text-placeholder',
      '--color-icon-default',
      '--color-icon-muted',
      '--color-focus-ring',
      '--color-link-text',
      '--color-destructive-text',
      '--color-destructive-bg',
      '--color-destructive-border',
      '--color-warning-text',
      '--color-warning-bg',
      '--color-warning-border',
      '--color-success-text',
      '--color-success-bg',
      '--color-success-border',
      '--color-info-text',
      '--color-info-bg',
      '--color-info-border',
      '--color-neutral-badge-text',
      '--color-neutral-badge-bg',
      '--color-neutral-badge-border',
      '--shadow-surface',
      '--color-overlay-scrim',
      '--color-skeleton-base',
      '--color-skeleton-highlight',
    ]

    for (const token of requiredTokens) {
      expect(themeCss).toContain(token)
    }
  })

  it('defines semantic badge tone tokens for required status tones', () => {
    const requiredTones = [
      'neutral',
      'info',
      'success',
      'warning',
      'danger',
      'destructive',
      'pending',
      'inactive',
      'draft',
      'compliant',
      'non-compliant',
      'needs-review',
    ]

    for (const tone of requiredTones) {
      expect(themeCss).toContain(`--tone-${tone}-text`)
      expect(themeCss).toContain(`--tone-${tone}-bg`)
      expect(themeCss).toContain(`--tone-${tone}-border`)
      expect(themeCss).toContain(`--tone-${tone}-icon`)
    }
  })

  it('includes suite-wide print preview and print media rules', () => {
    expect(themeCss).toContain("[data-print-preview='true']")
    expect(themeCss).toContain('@media print')
    expect(themeCss).toContain('[data-print-hide]')
    expect(themeCss).toContain('display: table-header-group')
    expect(themeCss).toContain('color-scheme: light !important')
  })
})
