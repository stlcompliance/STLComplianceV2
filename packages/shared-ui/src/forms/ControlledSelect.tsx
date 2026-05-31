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
  className = 'mt-1 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100',
}: ControlledSelectProps) {
  const mergedOptions = mergePickerOptions(options, value, selectedOption)
  const fieldId = id ?? testId

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
    <label htmlFor={fieldId} className="block text-sm text-slate-300">
      {label}
      {field}
    </label>
  )
}
