import { Search } from 'lucide-react'
import { useId, useMemo, useState } from 'react'

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
  allowCustomValue?: boolean
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
  allowCustomValue = false,
  testId,
}: StaticSearchPickerProps) {
  const [query, setQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)

  const mergedOptions = useMemo(
    () => mergePickerOptions(options, value, selectedOption),
    [options, value, selectedOption],
  )

  const selected = mergedOptions.find((option) => option.value === value)
  const generatedId = useId()
  const fieldId = id ?? testId ?? `static-picker-${generatedId.replace(/:/g, '')}`
  const listboxId = `${fieldId}-listbox`

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

  const typedQuery = query.trim()
  const canUseTypedValue =
    allowCustomValue && typedQuery.length > 0 && !mergedOptions.some((option) => option.value === typedQuery)

  return (
    <div className="relative" data-testid={testId}>
      {label && fieldId ? (
        <label htmlFor={fieldId} className="mb-1 block text-sm text-[var(--color-text-primary)]">
          {label}
        </label>
      ) : label ? (
        <span className="mb-1 block text-sm text-[var(--color-text-primary)]">{label}</span>
      ) : null}
      <div className="flex min-h-10 items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 shadow-sm transition hover:bg-[var(--color-bg-control-hover)] focus-within:border-[var(--color-accent-border)] focus-within:ring-2 focus-within:ring-[var(--color-focus-ring)]">
        <Search className="h-4 w-4 shrink-0 text-[var(--color-text-muted)]" aria-hidden />
        <input
          id={fieldId}
          type="search"
          aria-autocomplete="list"
          aria-expanded={isOpen && !disabled}
          aria-controls={isOpen ? listboxId : undefined}
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
          onKeyDown={(event) => {
            if (event.key === 'Escape') {
              setIsOpen(false)
            }
          }}
          placeholder={placeholder}
          disabled={disabled}
          className="w-full bg-transparent text-sm text-[var(--color-text-primary)] placeholder:text-[var(--color-text-muted)] focus:outline-none"
        />
      </div>
      {isOpen && !disabled ? (
        <ul
          id={listboxId}
          role="listbox"
          className="absolute z-50 mt-1 max-h-[min(16rem,calc(100vh-12rem))] w-full overflow-y-auto rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-surface-elevated)] shadow-xl shadow-slate-950/40"
        >
          {canUseTypedValue ? (
            <li>
              <button
                type="button"
                role="option"
                aria-selected={typedQuery === value}
                className="w-full px-3 py-2 text-left text-sm text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
                onMouseDown={(event) => event.preventDefault()}
                onClick={() => {
                  onChange(typedQuery)
                  setQuery('')
                  setIsOpen(false)
                }}
              >
                Use "{typedQuery}"
              </button>
            </li>
          ) : null}
          {filtered.length === 0 ? (
            <li className="px-3 py-2 text-sm text-[var(--color-text-muted)]">No matches.</li>
          ) : (
            filtered.map((option) => (
              <li key={option.value}>
                <button
                  type="button"
                  role="option"
                  aria-selected={option.value === value}
                  className="w-full px-3 py-2 text-left text-sm text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)] disabled:cursor-not-allowed disabled:opacity-50"
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
