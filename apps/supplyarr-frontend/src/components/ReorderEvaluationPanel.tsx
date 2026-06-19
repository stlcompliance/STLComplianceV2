import { useMemo } from 'react'

import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type { ReorderSuggestionResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface ReorderEvaluationPanelProps {
  suggestions: ReorderSuggestionResponse[]
  parts: { partId: string; partKey: string; displayName: string }[]
  canManagePolicy: boolean
  canCreatePurchaseRequest: boolean
  isLoading: boolean
  selectedPartId: string
  reorderPoint: string
  reorderQuantity: string
  selectedSuggestionPartIds: string[]
  prRequestKey: string
  prTitle: string
  prNotes: string
  onSelectedPartIdChange: (value: string) => void
  onReorderPointChange: (value: string) => void
  onReorderQuantityChange: (value: string) => void
  onSelectedSuggestionPartIdsChange: (value: string[]) => void
  onPrRequestKeyChange: (value: string) => void
  onPrTitleChange: (value: string) => void
  onPrNotesChange: (value: string) => void
  onSavePolicy: () => void
  onRefreshEvaluation: () => void
  onCreatePurchaseRequest: () => void
  isSavingPolicy: boolean
  isCreatingPurchaseRequest: boolean
}

export function ReorderEvaluationPanel({
  suggestions,
  parts,
  canManagePolicy,
  canCreatePurchaseRequest,
  isLoading,
  selectedPartId,
  reorderPoint,
  reorderQuantity,
  selectedSuggestionPartIds,
  prRequestKey,
  prTitle,
  prNotes,
  onSelectedPartIdChange,
  onReorderPointChange,
  onReorderQuantityChange,
  onSelectedSuggestionPartIdsChange,
  onPrRequestKeyChange,
  onPrTitleChange,
  onPrNotesChange,
  onSavePolicy,
  onRefreshEvaluation,
  onCreatePurchaseRequest,
  isSavingPolicy,
  isCreatingPurchaseRequest,
}: ReorderEvaluationPanelProps) {
  const partOptions = useMemo<PickerOption[]>(
    () =>
      parts.map((part) => ({
        value: part.partId,
        label: `${part.partKey} · ${part.displayName}`,
      })),
    [parts],
  )
  const selectedPartOption = useMemo<PickerOption | undefined>(
    () => partOptions.find((option) => option.value === selectedPartId),
    [partOptions, selectedPartId],
  )
  const selectedPartLabels = suggestions
    .filter((suggestion) => selectedSuggestionPartIds.includes(suggestion.partId))
    .map((suggestion) => suggestion.partKey)
  const prRequestKeySource =
    prTitle.trim() ||
    (selectedPartLabels.length > 0 ? `reorder ${selectedPartLabels.join(' ')}` : '')

  const toggleSuggestion = (partId: string) => {
    if (selectedSuggestionPartIds.includes(partId)) {
      onSelectedSuggestionPartIdsChange(selectedSuggestionPartIds.filter((id) => id !== partId))
      return
    }

    onSelectedSuggestionPartIdsChange([...selectedSuggestionPartIds, partId])
  }

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg lg:col-span-2">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-medium text-white">Reorder evaluation</h2>
          <p className="mt-1 text-sm text-slate-400">
            Compare stock levels to reorder points and draft purchase request lines for low stock.
          </p>
        </div>
        <button
          type="button"
          className="rounded-lg border border-slate-700 px-3 py-2 text-sm text-slate-200 hover:bg-slate-800"
          onClick={onRefreshEvaluation}
        >
          Refresh evaluation
        </button>
      </div>

      {canManagePolicy ? (
        <div className="mt-4 grid gap-3 md:grid-cols-4">
          <StaticSearchPicker
            id="reorder-policy-part"
            label="Reorder policy part"
            value={selectedPartId}
            onChange={onSelectedPartIdChange}
            options={partOptions}
            selectedOption={selectedPartOption}
            placeholder="Search parts…"
            testId="reorder-policy-part-picker"
          />
          <label htmlFor="reorder-policy-point" className="block text-sm text-slate-400">
            Reorder point
            <input
              id="reorder-policy-point"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
              value={reorderPoint}
              onChange={(event) => onReorderPointChange(event.target.value)}
              placeholder="e.g. 10"
            />
          </label>
          <label htmlFor="reorder-policy-quantity" className="block text-sm text-slate-400">
            Reorder quantity
            <input
              id="reorder-policy-quantity"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
              value={reorderQuantity}
              onChange={(event) => onReorderQuantityChange(event.target.value)}
              placeholder="optional"
            />
          </label>
          <div className="md:col-span-4">
            <button
              type="button"
              className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={!selectedPartId || isSavingPolicy}
              onClick={onSavePolicy}
            >
              {isSavingPolicy ? 'Saving…' : 'Save reorder policy'}
            </button>
          </div>
        </div>
      ) : null}

      <div className="mt-6 overflow-x-auto">
        {isLoading ? (
          <p className="text-sm text-slate-400">Loading reorder suggestions…</p>
        ) : suggestions.length === 0 ? (
          <p className="text-sm text-slate-400">No parts are below their reorder point.</p>
        ) : (
          <table className="min-w-full text-left text-sm text-slate-300">
            <thead className="border-b border-slate-800 text-slate-400">
              <tr>
                {canCreatePurchaseRequest ? <th className="py-2 pr-3">Select</th> : null}
                <th className="py-2 pr-3">Part</th>
                <th className="py-2 pr-3">Available</th>
                <th className="py-2 pr-3">Reorder point</th>
                <th className="py-2 pr-3">Suggested qty</th>
                <th className="py-2 pr-3">Preferred vendor</th>
                <th className="py-2">Status</th>
              </tr>
            </thead>
            <tbody>
              {suggestions.map((suggestion) => (
                <tr key={suggestion.partId} className="border-b border-slate-900/80">
                  {canCreatePurchaseRequest ? (
                    <td className="py-2 pr-3">
                      <label htmlFor={`reorder-suggestion-${suggestion.partId}`} className="sr-only">
                        Select {suggestion.partKey} for reorder purchase request
                      </label>
                      <input
                        id={`reorder-suggestion-${suggestion.partId}`}
                        type="checkbox"
                        checked={selectedSuggestionPartIds.includes(suggestion.partId)}
                        disabled={suggestion.hasOpenPurchaseRequest}
                        onChange={() => toggleSuggestion(suggestion.partId)}
                      />
                    </td>
                  ) : null}
                  <td className="py-2 pr-3">
                    <div className="font-medium text-white">{suggestion.partKey}</div>
                    <div className="text-xs text-[var(--color-text-muted)]">{suggestion.displayName}</div>
                  </td>
                  <td className="py-2 pr-3">{suggestion.quantityAvailable}</td>
                  <td className="py-2 pr-3">{suggestion.reorderPoint}</td>
                  <td className="py-2 pr-3">{suggestion.suggestedOrderQuantity}</td>
                  <td className="py-2 pr-3">
                    {suggestion.preferredVendorDisplayName ?? '—'}
                  </td>
                  <td className="py-2">
                    {suggestion.hasOpenPurchaseRequest ? (
                      <span className="rounded-full bg-amber-500/20 px-2 py-0.5 text-xs text-amber-300 ring-1 ring-amber-500/40">
                        Open PR
                      </span>
                    ) : (
                      <span className="rounded-full bg-rose-500/20 px-2 py-0.5 text-xs text-rose-300 ring-1 ring-rose-500/40">
                        Reorder
                      </span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {canCreatePurchaseRequest && suggestions.some((x) => !x.hasOpenPurchaseRequest) ? (
        <div className="mt-4 grid gap-3 md:grid-cols-3">
          <GeneratedKeyFieldGroup
            sourceLabel={prRequestKeySource}
            existingKeys={[]}
            onKeyChange={onPrRequestKeyChange}
            domain="purchase"
            kind="request"
            maxLength={128}
            label="PR request key"
            disabled={isCreatingPurchaseRequest}
          />
          <label htmlFor="reorder-pr-title" className="block text-sm text-slate-400">
            PR title
            <input
              id="reorder-pr-title"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
              value={prTitle}
              onChange={(event) => onPrTitleChange(event.target.value)}
            />
          </label>
          <label htmlFor="reorder-pr-notes" className="block text-sm text-slate-400 md:col-span-1">
            PR notes
            <input
              id="reorder-pr-notes"
              className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-white"
              value={prNotes}
              onChange={(event) => onPrNotesChange(event.target.value)}
            />
          </label>
          <div className="md:col-span-3">
            <button
              type="button"
              className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-500 disabled:opacity-50"
              disabled={
                selectedSuggestionPartIds.length === 0 ||
                !prRequestKey.trim() ||
                isCreatingPurchaseRequest
              }
              onClick={onCreatePurchaseRequest}
            >
              {isCreatingPurchaseRequest ? 'Creating…' : 'Create draft purchase request'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
