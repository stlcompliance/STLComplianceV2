import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useEffect, useId, useMemo, useState } from 'react'

import { formatPickerLabel, mergePickerOptions, type PickerOption } from './pickerTypes'

export type AsyncSearchPickerProps = {
  value: string
  onChange: (value: string) => void
  queryKey: readonly unknown[]
  queryFn: (query: string) => Promise<PickerOption[]>
  selectedOption?: PickerOption
  label?: string
  id?: string
  placeholder?: string
  minQueryLength?: number
  debounceMs?: number
  enabled?: boolean
  disabled?: boolean
  quickCreateOption?: {
    label?: string
    description?: string
    onSelect: (query: string) => void
  } | null
  testId?: string
}

export function AsyncSearchPicker({
  value,
  onChange,
  queryKey,
  queryFn,
  selectedOption,
  label,
  id,
  placeholder = 'Search…',
  minQueryLength = 2,
  debounceMs = 300,
  enabled = true,
  disabled = false,
  quickCreateOption,
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
  const generatedId = useId()
  const fieldId = id ?? testId ?? `async-picker-${generatedId.replace(/:/g, '')}`
  const listboxId = `${fieldId}-listbox`

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
      {isOpen && !disabled && debouncedQuery.length >= minQueryLength ? (
        <div
          id={listboxId}
          role="listbox"
          className="absolute z-50 mt-1 max-h-[min(16rem,calc(100vh-12rem))] w-full overflow-y-auto rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-surface-elevated)] shadow-xl shadow-slate-950/40"
        >
          {searchQuery.isLoading ? (
            <p className="px-3 py-2 text-sm text-[var(--color-text-muted)]">Searching…</p>
          ) : null}
          {searchQuery.isError ? (
            <p className="px-3 py-2 text-sm text-[var(--color-destructive-text)]">Search failed.</p>
          ) : null}
          {searchQuery.isSuccess && mergedResults.length === 0 ? (
            <p className="px-3 py-2 text-sm text-[var(--color-text-muted)]">No matches.</p>
          ) : null}
          {mergedResults.length > 0 ? (
            <ul>
              {mergedResults.map((option) => (
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
              ))}
            </ul>
          ) : null}
          {quickCreateOption ? (
            <button
              type="button"
              role="option"
              className="flex w-full items-start gap-2 border-t border-[var(--color-border-subtle)] px-3 py-2 text-left text-sm text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
              onMouseDown={(event) => event.preventDefault()}
              onClick={() => {
                quickCreateOption.onSelect(query)
                setIsOpen(false)
              }}
            >
              <span className="min-w-0">
                <span className="block truncate font-medium text-[var(--color-text-primary)]">
                  {quickCreateOption.label ?? 'Quick create'}
                </span>
                {quickCreateOption.description ? (
                  <span className="block truncate text-xs text-[var(--color-text-muted)]">
                    {quickCreateOption.description}
                  </span>
                ) : null}
              </span>
            </button>
          ) : null}
        </div>
      ) : null}
    </div>
  )
}
