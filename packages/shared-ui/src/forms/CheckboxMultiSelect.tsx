import { formatPickerLabel, type PickerOption } from './pickerTypes'

export type CheckboxMultiSelectProps = {
  values: string[]
  onChange: (values: string[]) => void
  options: PickerOption[]
  label?: string
  disabled?: boolean
  testId?: string
}

export function CheckboxMultiSelect({
  values,
  onChange,
  options,
  label,
  disabled = false,
  testId,
}: CheckboxMultiSelectProps) {
  function toggle(value: string) {
    if (disabled) {
      return
    }
    const next = values.includes(value) ? values.filter((item) => item !== value) : [...values, value]
    onChange(next)
  }

  return (
    <fieldset className="space-y-2" data-testid={testId}>
      {label ? <legend className="text-sm text-slate-300">{label}</legend> : null}
      <ul className="space-y-1">
        {options.map((option) => {
          const checked = values.includes(option.value)
          const isDisabled = disabled || (option.inactive && !checked)
          return (
            <li key={option.value}>
              <label className="flex items-center gap-2 text-sm text-slate-200">
                <input
                  type="checkbox"
                  checked={checked}
                  disabled={isDisabled}
                  onChange={() => toggle(option.value)}
                />
                <span className={option.inactive ? 'text-slate-500' : undefined}>
                  {formatPickerLabel(option)}
                </span>
              </label>
            </li>
          )
        })}
      </ul>
    </fieldset>
  )
}
