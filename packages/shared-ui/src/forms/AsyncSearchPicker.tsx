import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'

import { formatPickerLabel, mergePickerOptions, type PickerOption } from './pickerTypes'

export type AsyncSearchPickerProps = {
  value: string
  onChange: (value: string) => void
  queryKey: readonly unknown[]
  queryFn: (query: string) => Promise<PickerOption[]>
  selectedOption?: PickerOption
  label?: string
  placeholder?: string
  minQueryLength?: number
  debounceMs?: number
  enabled?: boolean
  disabled?: boolean
  testId?: string
}

export function AsyncSearchPicker({
  value,
  onChange,
  queryKey,
  queryFn,
  selectedOption,
  label,
  placeholder = 'Search…',
  minQueryLength = 2,
  debounceMs = 300,
  enabled = true,
  disabled = false,
  testId,
}: AsyncSearchPickerProps) {
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
  const mergedResults = useMemo(
    () => mergePickerOptions(results, value, selectedOption),
    [results, value, selectedOption],
  )

  const selected = mergedResults.find((option) => option.value === value)

  return (
    <div className="relative" data-testid={testId}>
      {label ? <span className="mb-1 block text-sm text-slate-300">{label}</span> : null}
      <div className="flex items-center gap-2 rounded-md border border-slate-700 bg-slate-950 px-3 py-2">
        <Search className="h-4 w-4 shrink-0 text-slate-400" aria-hidden />
        <input
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
      {isOpen && !disabled && debouncedQuery.length >= minQueryLength ? (
        <div className="absolute z-20 mt-1 max-h-60 w-full overflow-y-auto rounded-md border border-slate-700 bg-slate-950 shadow-lg">
          {searchQuery.isLoading ? (
            <p className="px-3 py-2 text-sm text-slate-500">Searching…</p>
          ) : null}
          {searchQuery.isError ? (
            <p className="px-3 py-2 text-sm text-rose-400">Search failed.</p>
          ) : null}
          {searchQuery.isSuccess && mergedResults.length === 0 ? (
            <p className="px-3 py-2 text-sm text-slate-500">No matches.</p>
          ) : null}
          {mergedResults.length > 0 ? (
            <ul>
              {mergedResults.map((option) => (
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
              ))}
            </ul>
          ) : null}
        </div>
      ) : null}
    </div>
  )
}
