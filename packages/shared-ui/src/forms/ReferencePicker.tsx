import { useQuery } from '@tanstack/react-query'
import { Loader2, Plus, Search } from 'lucide-react'
import { useEffect, useId, useMemo, useState } from 'react'

import { QuickCreateDrawer } from './QuickCreateDrawer'
import { ReferenceSummaryCard } from './ReferenceSummaryCard'
import type { ReferenceProviderClient } from './ReferenceProviderClient'
import {
  referenceSummaryToSnapshot,
  type CrossProductReference,
  type QuickCreateResponse,
  type ReferenceSummaryResponse,
} from './referenceTypes'

export type ReferencePickerProps = {
  client: Pick<
    ReferenceProviderClient,
    'searchReferences' | 'getQuickCreateSchema' | 'quickCreate'
  >
  ownerProductKey: string
  referenceType: string
  value: CrossProductReference | null
  onChange: (value: CrossProductReference | null) => void
  label?: string
  id?: string
  placeholder?: string
  minQueryLength?: number
  debounceMs?: number
  limit?: number
  disabled?: boolean
  allowQuickCreate?: boolean
  required?: boolean
  testId?: string
}

export function ReferencePicker({
  client,
  ownerProductKey,
  referenceType,
  value,
  onChange,
  label,
  id,
  placeholder = 'Search owner records...',
  minQueryLength = 2,
  debounceMs = 250,
  limit = 25,
  disabled = false,
  allowQuickCreate = true,
  required = false,
  testId,
}: ReferencePickerProps) {
  const [query, setQuery] = useState('')
  const [debouncedQuery, setDebouncedQuery] = useState('')
  const [isOpen, setIsOpen] = useState(false)
  const [quickCreateOpen, setQuickCreateOpen] = useState(false)

  useEffect(() => {
    const timer = window.setTimeout(() => setDebouncedQuery(query.trim()), debounceMs)
    return () => window.clearTimeout(timer)
  }, [query, debounceMs])

  const generatedId = useId()
  const fieldId = id ?? testId ?? `reference-picker-${generatedId.replace(/:/g, '')}`
  const listboxId = `${fieldId}-listbox`
  const canSearch = !disabled && debouncedQuery.length >= minQueryLength

  const searchQuery = useQuery({
    queryKey: ['reference-picker', ownerProductKey, referenceType, debouncedQuery, limit],
    queryFn: () =>
      client.searchReferences({
        referenceType,
        query: debouncedQuery,
        limit,
      }),
    enabled: canSearch,
  })

  const schemaQuery = useQuery({
    queryKey: ['reference-picker-schema', ownerProductKey, referenceType],
    queryFn: () => client.getQuickCreateSchema(referenceType),
    enabled: allowQuickCreate && !disabled,
    staleTime: 60_000,
  })

  const results = useMemo(() => searchQuery.data?.results ?? [], [searchQuery.data?.results])
  const selectedSummary = useMemo<ReferenceSummaryResponse | null>(() => {
    if (!value) {
      return null
    }

    return {
      ownerProductKey: value.ownerProductKey,
      referenceType: value.referenceType,
      referenceId: value.referenceId,
      displayLabel: value.displayLabelSnapshot,
      secondaryLabel: value.secondaryLabelSnapshot,
      status: value.statusSnapshot,
      ownerVersion: value.ownerVersion,
    }
  }, [value])

  const mergedResults = useMemo(() => {
    if (!selectedSummary || results.some((candidate) => candidate.referenceId === selectedSummary.referenceId)) {
      return results
    }

    return [selectedSummary, ...results]
  }, [results, selectedSummary])

  const schema = schemaQuery.data
  const showQuickCreate =
    allowQuickCreate && schema && (schema.allowed || schema.disabledReason || schema.permissionKey)

  async function createReference(values: Record<string, string>): Promise<QuickCreateResponse> {
    return client.quickCreate(referenceType, {
      referenceType,
      values,
    })
  }

  return (
    <div className="space-y-2" data-testid={testId}>
      {label ? (
        <label htmlFor={fieldId} className="block text-sm text-[var(--color-text-primary)]">
          {label}
          {required ? <span className="text-[var(--color-destructive-text)]"> *</span> : null}
        </label>
      ) : null}

      {value ? (
        <ReferenceSummaryCard
          reference={value}
          disabled={disabled}
          onClear={() => onChange(null)}
          testId={testId ? `${testId}-summary` : undefined}
        />
      ) : (
        <div className="relative">
          <div className="flex min-h-10 items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 shadow-sm transition hover:bg-[var(--color-bg-control-hover)] focus-within:border-[var(--color-accent-border)] focus-within:ring-2 focus-within:ring-[var(--color-focus-ring)]">
            <Search className="h-4 w-4 shrink-0 text-[var(--color-text-muted)]" aria-hidden />
            <input
              id={fieldId}
              type="search"
              aria-autocomplete="list"
              aria-expanded={isOpen && !disabled}
              aria-controls={isOpen ? listboxId : undefined}
              required={required}
              disabled={disabled}
              value={query}
              onChange={(event) => {
                setQuery(event.target.value)
                setIsOpen(true)
              }}
              onFocus={() => setIsOpen(true)}
              onBlur={() => {
                window.setTimeout(() => setIsOpen(false), 150)
              }}
              onKeyDown={(event) => {
                if (event.key === 'Escape') {
                  setIsOpen(false)
                }
              }}
              placeholder={placeholder}
              className="w-full bg-transparent text-sm text-[var(--color-text-primary)] placeholder:text-[var(--color-text-muted)] focus:outline-none"
            />
            {searchQuery.isFetching ? (
              <Loader2 className="h-4 w-4 shrink-0 animate-spin text-[var(--color-text-muted)]" aria-hidden />
            ) : null}
          </div>

          {isOpen && !disabled && debouncedQuery.length >= minQueryLength ? (
            <div
              id={listboxId}
              role="listbox"
              className="absolute z-50 mt-1 max-h-[min(18rem,calc(100vh-12rem))] w-full overflow-y-auto rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-surface-elevated)] shadow-xl shadow-slate-950/40"
            >
              {searchQuery.isError ? (
                <p className="px-3 py-2 text-sm text-[var(--color-destructive-text)]">Search failed.</p>
              ) : null}
              {searchQuery.isSuccess && mergedResults.length === 0 ? (
                <p className="px-3 py-2 text-sm text-[var(--color-text-muted)]">No matches.</p>
              ) : null}
              {mergedResults.map((result) => (
                <button
                  key={result.referenceId}
                  type="button"
                  role="option"
                  className="block w-full px-3 py-2 text-left text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
                  onMouseDown={(event) => event.preventDefault()}
                  onClick={() => {
                    onChange(referenceSummaryToSnapshot(result))
                    setQuery('')
                    setIsOpen(false)
                  }}
                >
                  <span className="block truncate text-sm font-medium text-[var(--color-text-primary)]">
                    {result.displayLabel}
                  </span>
                  <span className="block truncate text-xs text-[var(--color-text-muted)]">
                    {[result.secondaryLabel, result.status].filter(Boolean).join(' / ') ||
                      `Managed by ${result.ownerProductKey}`}
                  </span>
                </button>
              ))}
              {showQuickCreate ? (
                <button
                  type="button"
                  role="option"
                  className="flex w-full items-start gap-2 border-t border-[var(--color-border-subtle)] px-3 py-2 text-left text-[var(--color-text-primary)] hover:bg-[var(--color-bg-control-hover)]"
                  onMouseDown={(event) => event.preventDefault()}
                  onClick={() => {
                    setQuickCreateOpen(true)
                    setIsOpen(false)
                  }}
                >
                  <Plus className="mt-0.5 h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
                  <span className="min-w-0">
                    <span className="block truncate text-sm font-medium text-[var(--color-text-primary)]">
                      Quick create
                    </span>
                    <span className="block truncate text-xs text-[var(--color-text-muted)]">
                      {schema?.disabledReason
                        ? schema.disabledReason
                        : `Create a new ${schema?.referenceType ?? referenceType} in ${schema?.managedByLabel ?? 'the owning product'}.`}
                    </span>
                  </span>
                </button>
              ) : null}
            </div>
          ) : null}
        </div>
      )}

      <QuickCreateDrawer
        open={quickCreateOpen}
        schema={schema}
        initialValues={{ name: query, displayName: query, legalName: query }}
        onClose={() => setQuickCreateOpen(false)}
        onCreate={createReference}
        onCreated={(response) => {
          if (response.reference) {
            onChange(response.reference)
          }
        }}
        testId={testId ? `${testId}-quick-create` : undefined}
      />
    </div>
  )
}
