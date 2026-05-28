import { describe, expect, it } from 'vitest'

import {
  applyPersonExportFilterPreset,
  describeActiveExportFilters,
  inferPersonExportFilterPresetKey,
  isPersonExportFilterPresetEnabled,
  personExportPresetResponseToState,
  resolvePersonExportFilters,
} from './personExportFilterPresets'

describe('personExportFilterPresets', () => {
  it('applyPersonExportFilterPreset clears filters for all people', () => {
    expect(
      applyPersonExportFilterPreset('all-people', {
        employmentStatus: 'active',
        orgUnitId: '11111111-1111-1111-1111-111111111111',
      }),
    ).toEqual({ employmentStatus: '', orgUnitId: '' })
  })

  it('applyPersonExportFilterPreset sets active workforce', () => {
    expect(
      applyPersonExportFilterPreset('active-workforce', { employmentStatus: '', orgUnitId: '' }),
    ).toEqual({ employmentStatus: 'active', orgUnitId: '' })
  })

  it('applyPersonExportFilterPreset keeps selected org unit for active-at-org-unit', () => {
    expect(
      applyPersonExportFilterPreset('active-at-org-unit', {
        employmentStatus: '',
        orgUnitId: '11111111-1111-1111-1111-111111111111',
      }),
    ).toEqual({
      employmentStatus: 'active',
      orgUnitId: '11111111-1111-1111-1111-111111111111',
    })
  })

  it('resolvePersonExportFilters omits empty values', () => {
    expect(resolvePersonExportFilters({ employmentStatus: 'active', orgUnitId: '' })).toEqual({
      employmentStatus: 'active',
    })
    expect(
      resolvePersonExportFilters({
        employmentStatus: 'active',
        orgUnitId: '11111111-1111-1111-1111-111111111111',
      }),
    ).toEqual({
      employmentStatus: 'active',
      orgUnitId: '11111111-1111-1111-1111-111111111111',
    })
  })

  it('isPersonExportFilterPresetEnabled requires org unit for active-at-org-unit', () => {
    const preset = {
      key: 'active-at-org-unit' as const,
      label: 'Active at org unit',
      description: 'test',
      requiresOrgUnit: true,
    }
    expect(isPersonExportFilterPresetEnabled(preset, '')).toBe(false)
    expect(isPersonExportFilterPresetEnabled(preset, '11111111-1111-1111-1111-111111111111')).toBe(true)
  })

  it('describeActiveExportFilters summarizes combined filters', () => {
    expect(describeActiveExportFilters({ employmentStatus: '', orgUnitId: '' })).toBe('No filters applied')
    expect(
      describeActiveExportFilters({
        employmentStatus: 'active',
        orgUnitId: '11111111-1111-1111-1111-111111111111',
      }),
    ).toBe('Filtering by status active and org unit selected')
  })

  it('inferPersonExportFilterPresetKey matches known preset states', () => {
    expect(
      inferPersonExportFilterPresetKey({ employmentStatus: 'active', orgUnitId: '' }),
    ).toBe('active-workforce')
    expect(
      inferPersonExportFilterPresetKey({
        employmentStatus: 'active',
        orgUnitId: '11111111-1111-1111-1111-111111111111',
      }),
    ).toBe('active-at-org-unit')
    expect(
      inferPersonExportFilterPresetKey({ employmentStatus: 'inactive', orgUnitId: '11111111-1111-1111-1111-111111111111' }),
    ).toBe(null)
  })

  it('personExportPresetResponseToState restores stored preset filters', () => {
    expect(
      personExportPresetResponseToState({
        employmentStatus: 'active',
        orgUnitId: '11111111-1111-1111-1111-111111111111',
        presetKey: 'active-at-org-unit',
        updatedAt: '2026-05-27T12:00:00Z',
      }),
    ).toEqual({
      employmentStatus: 'active',
      orgUnitId: '11111111-1111-1111-1111-111111111111',
    })
  })
})
