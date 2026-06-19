import { useQuery } from '@tanstack/react-query'
import { useMemo } from 'react'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import type { PartResponse, SubstitutionItemResponse } from '../api/types'
import { getSubstitutions } from '../api/client'
import { toPartPickerOptions } from '../forms/controlledFormHelpers'

interface PartSubstitutionsPanelProps {
  accessToken: string
  parts: PartResponse[]
  canRead: boolean
  selectedPartId: string
  onSelectedPartIdChange: (value: string) => void
}

export function PartSubstitutionsPanel({
  accessToken,
  parts,
  canRead,
  selectedPartId,
  onSelectedPartIdChange,
}: PartSubstitutionsPanelProps) {
  const partOptions = useMemo<PickerOption[]>(() => toPartPickerOptions(parts), [parts])
  const selectedPart = useMemo(
    () => parts.find((part) => part.partId === selectedPartId) ?? null,
    [parts, selectedPartId],
  )
  const selectedPartOption = useMemo<PickerOption | undefined>(
    () =>
      partOptions.find((option) => option.value === selectedPartId) ??
      (selectedPart
        ? {
            value: selectedPartId,
            label:
              selectedPart.partKey && selectedPart.displayName
                ? `${selectedPart.partKey} — ${selectedPart.displayName}`
                : selectedPartId,
          }
        : undefined),
    [partOptions, selectedPart, selectedPartId],
  )
  const substitutionsQuery = useQuery({
    queryKey: ['supplyarr-substitutions', accessToken, selectedPartId],
    queryFn: () => getSubstitutions(accessToken, selectedPartId || undefined),
    enabled: canRead,
  })

  if (!canRead) {
    return null
  }

  const substitutions = substitutionsQuery.data ?? []

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2" data-testid="part-substitutions-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-medium text-white">Part substitutions</h2>
          <p className="mt-1 text-sm text-slate-400">
            Manufacturer aliases and substitute identifiers recorded against part records.
          </p>
        </div>
        <span className="rounded-full border border-slate-700 px-3 py-1 text-xs uppercase tracking-wide text-slate-400">
          {substitutions.length} entries
        </span>
      </div>

      <div className="mt-4 max-w-md">
        <StaticSearchPicker
          id="part-substitutions-filter"
          label="Part filter"
          value={selectedPartId}
          onChange={onSelectedPartIdChange}
          options={partOptions}
          selectedOption={selectedPartOption}
          placeholder="Search parts…"
          testId="part-substitutions-filter"
        />
      </div>

      {substitutionsQuery.isLoading ? (
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading substitutions…</p>
      ) : substitutionsQuery.isError ? (
        <div className="mt-4">
          <ApiErrorCallout
            title="Substitutions unavailable"
            message={getErrorMessage(substitutionsQuery.error, 'Failed to load substitutions.')}
            retryLabel="Retry substitutions"
            onRetry={() => {
              void substitutionsQuery.refetch()
            }}
          />
        </div>
      ) : substitutions.length === 0 ? (
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">No substitutions or aliases recorded for this filter.</p>
      ) : (
        <ul className="mt-4 grid gap-3 md:grid-cols-2">
          {substitutions.map((item: SubstitutionItemResponse) => (
            <li key={item.aliasId} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3 text-sm">
              <div className="flex items-start justify-between gap-2">
                <div>
                  <div className="font-medium text-slate-100">
                    {item.partKey} · {item.partDisplayName}
                  </div>
                  <div className="text-slate-400">
                    {item.manufacturerName}
                    {item.manufacturerPartNumber ? ` · ${item.manufacturerPartNumber}` : ''}
                  </div>
                </div>
                <span className="rounded-full bg-cyan-500/15 px-2 py-0.5 text-xs uppercase tracking-wide text-cyan-300">
                  alias
                </span>
              </div>
              <div className="mt-2 text-xs text-[var(--color-text-muted)]">
                Alias {item.aliasKey} · Added {new Date(item.createdAt).toLocaleString()}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
