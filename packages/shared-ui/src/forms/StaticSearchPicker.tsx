import { Search } from 'lucide-react'
import { useMemo, useState } from 'react'

import { formatPickerLabel, mergePickerOptions, type PickerOption } from './pickerTypes'

export type StaticSearchPickerProps = {
  value: string
  onChange: (value: string) => void
  options: readonly PickerOption[]
  selectedOption?: PickerOption
  label?: string
  id?: string
  placeholder?: string
  disabled?: boolean
  testId?: string
}

export function StaticSearchPicker({
  value,
  onChange,
  options,
  selectedOption,
  label,
  id,
  placeholder = 'Search…',
  disabled = false,
  testId,
}: StaticSearchPickerProps) {
  const [query, setQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)

  const mergedOptions = useMemo(
    () => mergePickerOptions(options, value, selectedOption),
    [options, value, selectedOption],
  )

  const selected = mergedOptions.find((option) => option.value === value)
  const fieldId = id ?? testId

  const filtered = useMemo(() => {
    const needle = query.trim().toLowerCase()
    if (!needle) {
      return mergedOptions.filter((option) => !option.inactive || option.value === value)
    }
    return mergedOptions.filter(
      (option) =>
        option.label.toLowerCase().includes(needle) ||
        option.value.toLowerCase().includes(needle),
    )
  }, [mergedOptions, query, value])

  return (
    <div className="relative" data-testid={testId}>
      {label && fieldId ? (
        <label htmlFor={fieldId} className="mb-1 block text-sm text-slate-300">
          {label}
        </label>
      ) : label ? (
        <span className="mb-1 block text-sm text-slate-300">{label}</span>
      ) : null}
      <div className="flex items-center gap-2 rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
        <Search className="h-4 w-4 shrink-0 text-slate-400" aria-hidden />
        <input
          id={fieldId}
          type="search"
          value={isOpen ? query : selected ? formatPickerLabel(selected) : query}
          onChange={(event) => {
            setQuery(event.target.value)
            setIsOpen(true)
          }}
          onFocus={() => {
            setQuery('')
            setIsOpen(true)
          }}
          onBlur={() => {
            window.setTimeout(() => setIsOpen(false), 150)
          }}
          placeholder={placeholder}
          disabled={disabled}
          className="w-full bg-transparent text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none"
        />
      </div>
      {isOpen && !disabled ? (
        <ul className="absolute z-20 mt-1 max-h-60 w-full overflow-y-auto rounded-md border border-slate-700 bg-slate-950 shadow-lg">
          {filtered.length === 0 ? (
            <li className="px-3 py-2 text-sm text-slate-500">No matches.</li>
          ) : (
            filtered.map((option) => (
              <li key={option.value}>
                <button
                  type="button"
                  className="w-full px-3 py-2 text-left text-sm hover:bg-slate-900 disabled:cursor-not-allowed disabled:opacity-50"
                  disabled={option.inactive && option.value !== value}
                  onMouseDown={(event) => event.preventDefault()}
                  onClick={() => {
                    onChange(option.value)
                    setQuery('')
                    setIsOpen(false)
                  }}
                >
                  {formatPickerLabel(option)}
                </button>
              </li>
            ))
          )}
        </ul>
      ) : null}
    </div>
  )
}
