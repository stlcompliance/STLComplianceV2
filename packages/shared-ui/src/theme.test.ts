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
    expect(loadThemePreference({ userId: 'user-1', tenantId: 'tenant-1', appKey: 'suite' })).toBe('system')
  })

  it('stores the selected mode per tenant and user', () => {
    const identity = { userId: 'user-1', tenantId: 'tenant-1', appKey: 'suite' }
    const otherUser = { userId: 'user-2', tenantId: 'tenant-1', appKey: 'suite' }

    saveThemePreference('light', identity)

    expect(localStorage.getItem(buildThemePreferenceStorageKey(identity))).toBe('light')
    expect(loadThemePreference(identity)).toBe('light')
    expect(loadThemePreference(otherUser)).toBe('system')
  })

  it('keeps app-scoped themes isolated from each other', () => {
    const suiteIdentity = { userId: 'user-1', tenantId: 'tenant-1', appKey: 'suite' }
    const productIdentity = { userId: 'user-1', tenantId: 'tenant-1', appKey: 'staffarr' }

    saveThemePreference('dark', suiteIdentity)

    expect(loadThemePreference(productIdentity)).toBe('system')
  })

  it('falls back to the legacy tenant and user key when migrating app-scoped themes', () => {
    const identity = { userId: 'user-1', tenantId: 'tenant-1', appKey: 'staffarr' }

    localStorage.setItem('stl.theme.preference.v1:tenant:tenant-1:user:user-1', 'light')

    expect(loadThemePreference(identity)).toBe('light')
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
    const identity = { userId: 'user-1', tenantId: 'tenant-1', appKey: 'staffarr' }

    expect(saveThemePreferenceFromSession({ ...identity, themePreference: 'dark' }, identity)).toBe('dark')

    expect(localStorage.getItem(buildThemePreferenceStorageKey(identity))).toBe('dark')
    expect(document.documentElement.dataset.theme).toBe('dark')
    expect(document.documentElement.dataset.themePreference).toBe('dark')
  })

  it('does not overwrite an existing app theme from the session default', () => {
    const identity = { userId: 'user-1', tenantId: 'tenant-1', appKey: 'staffarr' }
    saveThemePreference('light', identity)

    expect(
      saveThemePreferenceFromSession({ ...identity, themePreference: 'dark' }, identity),
    ).toBe('light')
    expect(localStorage.getItem(buildThemePreferenceStorageKey(identity))).toBe('light')
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

  it('normalizes common warning and error utility classes through the shared theme', () => {
    const requiredClasses = [
      'text-rose-300',
      'text-amber-100',
      'bg-amber-50',
      'border-rose-100',
    ]

    for (const className of requiredClasses) {
      expect(themeCss).toContain(className)
    }
  })

  it('includes suite-wide print preview and print media rules', () => {
    expect(themeCss).toContain("[data-print-preview='true']")
    expect(themeCss).toContain('[data-print-document]')
    expect(themeCss).toContain('@media print')
    expect(themeCss).toContain('[data-print-hide]')
    expect(themeCss).toContain('display: table-header-group')
    expect(themeCss).toContain('color-scheme: light !important')
    expect(themeCss).toContain('page-break-before: always')
    expect(themeCss).toContain('page-break-after: always')
  })
})
