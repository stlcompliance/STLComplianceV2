import type { PersonExportFilters } from '../api/types'

export type PersonExportFilterPresetKey =
  | 'all-people'
  | 'active-workforce'
  | 'inactive-records'
  | 'terminated-records'
  | 'active-at-org-unit'

export interface PersonExportFilterPreset {
  key: PersonExportFilterPresetKey
  label: string
  description: string
  requiresOrgUnit: boolean
}

export const PERSON_EXPORT_FILTER_PRESETS: PersonExportFilterPreset[] = [
  {
    key: 'all-people',
    label: 'All people',
    description: 'Export the full workforce directory with no filters.',
    requiresOrgUnit: false,
  },
  {
    key: 'active-workforce',
    label: 'Active workforce',
    description: 'Export people with employment status active.',
    requiresOrgUnit: false,
  },
  {
    key: 'inactive-records',
    label: 'Inactive records',
    description: 'Export people with employment status inactive.',
    requiresOrgUnit: false,
  },
  {
    key: 'terminated-records',
    label: 'Terminated records',
    description: 'Export people with employment status terminated.',
    requiresOrgUnit: false,
  },
  {
    key: 'active-at-org-unit',
    label: 'Active at org unit',
    description: 'Export active people scoped to the selected primary org unit.',
    requiresOrgUnit: true,
  },
]

export interface PersonExportFilterState {
  employmentStatus: string
  orgUnitId: string
}

export function resolvePersonExportFilters(state: PersonExportFilterState): PersonExportFilters {
  return {
    employmentStatus: state.employmentStatus || undefined,
    orgUnitId: state.orgUnitId || undefined,
  }
}

export function applyPersonExportFilterPreset(
  presetKey: PersonExportFilterPresetKey,
  current: PersonExportFilterState,
): PersonExportFilterState {
  switch (presetKey) {
    case 'all-people':
      return { employmentStatus: '', orgUnitId: '' }
    case 'active-workforce':
      return { employmentStatus: 'active', orgUnitId: '' }
    case 'inactive-records':
      return { employmentStatus: 'inactive', orgUnitId: '' }
    case 'terminated-records':
      return { employmentStatus: 'terminated', orgUnitId: '' }
    case 'active-at-org-unit':
      return { employmentStatus: 'active', orgUnitId: current.orgUnitId }
    default:
      return current
  }
}

export function isPersonExportFilterPresetEnabled(
  preset: PersonExportFilterPreset,
  orgUnitId: string,
): boolean {
  if (!preset.requiresOrgUnit) {
    return true
  }

  return orgUnitId.trim().length > 0
}

export function describeActiveExportFilters(state: PersonExportFilterState): string {
  const parts: string[] = []
  if (state.employmentStatus) {
    parts.push(`status ${state.employmentStatus}`)
  }
  if (state.orgUnitId) {
    parts.push('org unit selected')
  }
  if (parts.length === 0) {
    return 'No filters applied'
  }
  return `Filtering by ${parts.join(' and ')}`
}

export function inferPersonExportFilterPresetKey(
  state: PersonExportFilterState,
): PersonExportFilterPresetKey | null {
  for (const preset of PERSON_EXPORT_FILTER_PRESETS) {
    const applied = applyPersonExportFilterPreset(preset.key, state)
    if (
      applied.employmentStatus === state.employmentStatus &&
      applied.orgUnitId === state.orgUnitId
    ) {
      return preset.key
    }
  }

  return null
}

export interface StoredPersonExportPreset {
  employmentStatus: string | null
  orgUnitId: string | null
  presetKey: string | null
  updatedAt: string
}

export function personExportPresetResponseToState(
  preset: StoredPersonExportPreset,
): PersonExportFilterState {
  if (preset.presetKey) {
    const presetKey = preset.presetKey as PersonExportFilterPresetKey
    const base = applyPersonExportFilterPreset(presetKey, {
      employmentStatus: preset.employmentStatus ?? '',
      orgUnitId: preset.orgUnitId ?? '',
    })
    if (presetKey === 'active-at-org-unit' && preset.orgUnitId) {
      return { employmentStatus: base.employmentStatus, orgUnitId: preset.orgUnitId }
    }
    return base
  }

  return {
    employmentStatus: preset.employmentStatus ?? '',
    orgUnitId: preset.orgUnitId ?? '',
  }
}

export function describeTenantExportPreset(preset: StoredPersonExportPreset): string {
  const state = personExportPresetResponseToState(preset)
  const summary = describeActiveExportFilters(state)
  const savedAt = new Date(preset.updatedAt).toLocaleString()
  return `Tenant default: ${summary} (saved ${savedAt})`
}
