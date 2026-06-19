import type { PickerOption } from './forms/pickerTypes'

export const COMMON_TIME_ZONE_OPTIONS: readonly PickerOption[] = [
  { value: 'UTC', label: 'UTC' },
  { value: 'America/New_York', label: 'America/New_York' },
  { value: 'America/Chicago', label: 'America/Chicago' },
  { value: 'America/Denver', label: 'America/Denver' },
  { value: 'America/Phoenix', label: 'America/Phoenix' },
  { value: 'America/Los_Angeles', label: 'America/Los_Angeles' },
  { value: 'America/Anchorage', label: 'America/Anchorage' },
  { value: 'Pacific/Honolulu', label: 'Pacific/Honolulu' },
] as const

export const SYSTEM_TIME_ZONE_OPTION = { value: 'system', label: 'System default' } as const
