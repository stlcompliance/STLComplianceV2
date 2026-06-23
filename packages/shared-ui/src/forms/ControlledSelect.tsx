import { useId } from 'react'

import { formatPickerLabel, mergePickerOptions, type PickerOption } from './pickerTypes'

export type ControlledSelectProps = {
  value: string
  onChange: (value: string) => void
  options: readonly PickerOption[]
  selectedOption?: PickerOption
  label?: string
  id?: string
  emptyLabel?: string
  disabled?: boolean
  testId?: string
  className?: string
}

const defaultSelectClassName =
  'mt-1 w-full rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] shadow-sm outline-none transition hover:bg-[var(--color-bg-control-hover)] focus:border-[var(--color-accent-border)] focus:ring-2 focus:ring-[var(--color-focus-ring)] disabled:cursor-not-allowed disabled:opacity-60'

export function ControlledSelect({
  value,
  onChange,
  options,
  selectedOption,
  label,
  id,
  emptyLabel = 'Select…',
  disabled = false,
  testId,
  className = defaultSelectClassName,
}: ControlledSelectProps) {
  const mergedOptions = mergePickerOptions(options, value, selectedOption)
  const generatedId = useId()
  const fieldId = id ?? testId ?? `controlled-select-${generatedId.replace(/:/g, '')}`

  const field = (
    <select
      id={fieldId}
      value={value}
      onChange={(event) => onChange(event.target.value)}
      disabled={disabled}
      data-testid={testId}
      className={className}
    >
      <option value="">{emptyLabel}</option>
      {mergedOptions.map((option) => (
        <option key={option.value} value={option.value} disabled={option.inactive && option.value !== value}>
          {formatPickerLabel(option)}
        </option>
      ))}
    </select>
  )

  if (!label) {
    return field
  }

  return (
    <label htmlFor={fieldId} className="block text-sm font-medium text-[var(--color-text-primary)]">
      {label}
      {field}
    </label>
  )
}
