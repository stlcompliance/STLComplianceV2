import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'

import { formatPickerLabel, type PickerOption } from './pickerTypes'

export type AsyncMultiPickerProps = {
  values: string[]
  onChange: (values: string[]) => void
  queryKey: readonly unknown[]
  queryFn: (query: string) => Promise<PickerOption[]>
  selectedOptions?: PickerOption[]
  label?: string
  placeholder?: string
  minQueryLength?: number
  debounceMs?: number
  enabled?: boolean
  disabled?: boolean
  testId?: string
}

export function AsyncMultiPicker({
  values,
  onChange,
  queryKey,
  queryFn,
  selectedOptions = [],
  label,
  placeholder = 'Search to add…',
  minQueryLength = 2,
  debounceMs = 300,
  enabled = true,
  disabled = false,
  testId,
}: AsyncMultiPickerProps) {
  const [query, setQuery] = useState('')
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)

  useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedQuery(query.trim()), debounceMs)
    return () => window.clearTimeout(timer)
  }, [query, debounceMs])

  const searchEnabled = enabled && !disabled && debouncedQuery.length >= minQueryLength

  const searchQuery = useQuery({
    queryKey: [...queryKey, debouncedQuery],
    queryFn: () => queryFn(debouncedQuery),
    enabled: searchEnabled,
  })

  const results = useMemo(() => searchQuery.data ?? [], [searchQuery.data])

  const selectedChips = useMemo(() => {
    const byValue = new Map<string, PickerOption>()
    for (const option of selectedOptions) {
      byValue.set(option.value, option)
    }
    for (const option of results) {
      if (!byValue.has(option.value)) {
        byValue.set(option.value, option)
      }
    }
    return values.map(
      (value) =>
        byValue.get(value) ?? {
          value,
          label: value,
          inactive: true,
        },
    )
  }, [selectedOptions, results, values])

  function toggle(option: PickerOption) {
    if (disabled || (option.inactive && !values.includes(option.value))) {
      return
    }
    const next = values.includes(option.value)
      ? values.filter((item) => item !== option.value)
      : [...values, option.value]
    onChange(next)
  }

  return (
    <div data-testid={testId}>
      {label ? <span className="mb-1 block text-sm text-slate-300">{label}</span> : null}
      {selectedChips.length > 0 ? (
        <ul className="mb-2 flex flex-wrap gap-2">
          {selectedChips.map((option) => (
            <li key={option.value}>
              <button
                type="button"
                className="rounded-full border border-slate-600 bg-slate-900 px-2 py-1 text-xs text-slate-200"
                disabled={disabled}
                onClick={() => toggle(option)}
              >
                {formatPickerLabel(option)} ×
              </button>
            </li>
          ))}
        </ul>
      ) : null}
      <div className="relative">
        <div className="flex items-center gap-2 rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
          <Search className="h-4 w-4 shrink-0 text-slate-400" aria-hidden />
          <input
            type="search"
            value={query}
            onChange={(event) => {
              setQuery(event.target.value)
              setIsOpen(true)
            }}
            onFocus={() => setIsOpen(true)}
            onBlur={() => {
              window.setTimeout(() => setIsOpen(false), 150)
            }}
            placeholder={placeholder}
            disabled={disabled}
            className="w-full bg-transparent text-sm text-slate-100 placeholder:text-slate-500 focus:outline-none"
          />
        </div>
        {isOpen && !disabled && debouncedQuery.length >= minQueryLength ? (
          <ul className="absolute z-20 mt-1 max-h-60 w-full overflow-y-auto rounded-md border border-slate-700 bg-slate-950 shadow-lg">
            {searchQuery.isLoading ? (
              <li className="px-3 py-2 text-sm text-slate-500">Searching…</li>
            ) : null}
            {results.map((option) => (
              <li key={option.value}>
                <button
                  type="button"
                  className="w-full px-3 py-2 text-left text-sm hover:bg-slate-900 disabled:opacity-50"
                  disabled={option.inactive && !values.includes(option.value)}
                  onMouseDown={(event) => event.preventDefault()}
                  onClick={() => toggle(option)}
                >
                  {values.includes(option.value) ? '✓ ' : ''}
                  {formatPickerLabel(option)}
                </button>
              </li>
            ))}
          </ul>
        ) : null}
      </div>
    </div>
  )
}
