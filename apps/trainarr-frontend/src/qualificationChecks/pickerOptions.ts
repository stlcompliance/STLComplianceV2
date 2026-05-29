import type { PickerOption } from '@stl/shared-ui'

export const DEFAULT_RULE_PACK_OPTIONS: PickerOption[] = [
  { value: 'driver_qualification', label: 'driver_qualification' },
  { value: 'hazmat_endorsement', label: 'hazmat_endorsement' },
]

export function buildRulePackOptions(rulePackKeys: string[], currentValue?: string): PickerOption[] {
  const keys = new Set<string>(DEFAULT_RULE_PACK_OPTIONS.map((option) => option.value))
  for (const key of rulePackKeys) {
    const trimmed = key.trim()
    if (trimmed) {
      keys.add(trimmed)
    }
  }
  if (currentValue?.trim()) {
    keys.add(currentValue.trim())
  }

  return [...keys].sort().map((value) => ({ value, label: value }))
}

export function buildPersonPickerOptions(
  entries: Array<{ personId: string; label: string }>,
): PickerOption[] {
  const seen = new Set<string>()
  const options: PickerOption[] = []
  for (const entry of entries) {
    const personId = entry.personId.trim().toLowerCase()
    if (!personId || seen.has(personId)) {
      continue
    }
    seen.add(personId)
    options.push({ value: personId, label: entry.label })
  }
  return options.sort((left, right) => left.label.localeCompare(right.label))
}
