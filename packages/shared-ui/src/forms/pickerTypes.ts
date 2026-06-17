import { unavailableReferenceLabel } from '../displayLabels'

export type PickerOption = {
  value: string
  label: string
  inactive?: boolean
}

export function formatPickerLabel(option: PickerOption): string {
  if (option.inactive) {
    return `${option.label} (inactive)`
  }
  return option.label
}

export function mergePickerOptions(
  options: readonly PickerOption[],
  selectedValue: string,
  selectedOption?: PickerOption,
): PickerOption[] {
  if (!selectedValue) {
    return [...options]
  }

  const exists = options.some((option) => option.value === selectedValue)
  if (exists) {
    return [...options]
  }

  const orphan: PickerOption = selectedOption ?? {
    value: selectedValue,
    label: unavailableReferenceLabel(selectedValue),
    inactive: true,
  }

  return [orphan, ...options]
}
