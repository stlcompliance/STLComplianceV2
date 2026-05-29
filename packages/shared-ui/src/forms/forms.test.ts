import { describe, expect, it } from 'vitest'

import { mergePickerOptions } from './pickerTypes'
import { slugifyKey, withKeySuffix } from './slugifyKey'
import { normalizeUom } from './normalizeUom'

describe('slugifyKey', () => {
  it('normalizes labels into keys', () => {
    expect(slugifyKey('Product Name')).toBe('product-name')
    expect(slugifyKey('  EA Widget  ')).toBe('ea-widget')
  })

  it('returns empty for too-short labels', () => {
    expect(slugifyKey('a')).toBe('')
  })

  it('adds collision suffixes within max length', () => {
    expect(withKeySuffix('product-name', 2)).toBe('product-name-2')
  })
})

describe('normalizeUom', () => {
  it('maps aliases to canonical values', () => {
    expect(normalizeUom('EA')).toBe('each')
    expect(normalizeUom('pcs')).toBe('each')
    expect(normalizeUom('piece')).toBe('each')
  })

  it('defaults empty to each', () => {
    expect(normalizeUom('')).toBe('each')
  })
})

describe('mergePickerOptions', () => {
  it('preserves orphan selected values as inactive', () => {
    const merged = mergePickerOptions(
      [{ value: 'active', label: 'Active' }],
      'gone',
      { value: 'gone', label: 'Former driver', inactive: true },
    )
    expect(merged[0]).toEqual({ value: 'gone', label: 'Former driver', inactive: true })
  })
})
