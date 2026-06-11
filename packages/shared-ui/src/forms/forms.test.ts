import { describe, expect, it } from 'vitest'

import { mergePickerOptions } from './pickerTypes'
import { buildSemanticKey, chooseSemanticAlias } from './semanticKey'
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

  it('normalizes repeated separators around uncontrolled labels', () => {
    expect(slugifyKey(`${'-'.repeat(5000)}Product Name${'-'.repeat(5000)}`)).toBe('product-name')
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

describe('semantic keys', () => {
  it('uses known aliases when available', () => {
    expect(
      buildSemanticKey({
        domain: 'docs',
        kind: 'req',
        title: 'Driver Qualification File Required Documents',
      }),
    ).toBe('docs.req.dqf')
  })

  it('falls back to compact semantic slug and appends deterministic suffixes', () => {
    expect(
      buildSemanticKey({
        domain: 'inspection',
        kind: 'req',
        title: 'Pre Trip Inspection Required',
        existingKeys: ['inspection.req.pretripinspectionrequired'],
      }),
    ).toBe('inspection.req.pretripinspectionrequired.2')
  })

  it('supports explicit alias hints', () => {
    expect(chooseSemanticAlias('Internal Label', ['Loto'])).toBe('loto')
  })

  it('respects maximum key length constraints', () => {
    expect(
      buildSemanticKey({
        domain: 'train',
        kind: 'step',
        title: 'This is a very long training step name that should be safely truncated',
        maxLength: 64,
      }),
    ).toMatch(/^train\.step\.[a-z0-9]+$/)
    expect(
      buildSemanticKey({
        domain: 'train',
        kind: 'step',
        title: 'This is a very long training step name that should be safely truncated',
        maxLength: 64,
      }).length,
    ).toBeLessThanOrEqual(64)
  })

  it('keeps collision suffixes within max length', () => {
    const first = buildSemanticKey({
      domain: 'train',
      kind: 'step',
      title: 'Step title',
      maxLength: 16,
    })
    const second = buildSemanticKey({
      domain: 'train',
      kind: 'step',
      title: 'Step title',
      existingKeys: [first],
      maxLength: 16,
    })

    expect(second).toMatch(/\.2$/)
    expect(second.length).toBeLessThanOrEqual(16)
  })
})
