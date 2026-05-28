import { describe, expect, it } from 'vitest'

import {
  applyPersonExportFilterPreset,
  describeActiveExportFilters,
  isPersonExportFilterPresetEnabled,
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
})
