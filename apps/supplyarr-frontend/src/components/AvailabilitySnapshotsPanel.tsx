import { useMemo } from 'react'

import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type { AvailabilitySnapshotResponse, PartResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'
import {
  formatSupplierIdentitySummary,
  formatSupplierOperationalContext,
  humanizeSupplierUnitKind,
} from '../utils/supplierPresentation'

interface AvailabilitySnapshotsPanelProps {
  parts: PartResponse[]
  availabilitySnapshots: AvailabilitySnapshotResponse[]
  canManage: boolean
  isLoading: boolean
  snapshotKey: string
  selectedSourceLinkId: string
  quantityAvailable: string
  availabilityStatus: string
  snapshotNotes: string
  currentOnlyFilter: boolean
  onSnapshotKeyChange: (value: string) => void
  onSelectedSourceLinkIdChange: (value: string) => void
  onQuantityAvailableChange: (value: string) => void
  onAvailabilityStatusChange: (value: string) => void
  onSnapshotNotesChange: (value: string) => void
  onCurrentOnlyFilterChange: (value: boolean) => void
  onCreateAvailabilitySnapshot: () => void
  isCreating: boolean
}

const statusLabels: Record<string, string> = {
  in_stock: 'In stock',
  limited: 'Limited',
  backorder: 'Backorder',
  out_of_stock: 'Out of stock',
  discontinued: 'Discontinued',
}

export function AvailabilitySnapshotsPanel({
  parts,
  availabilitySnapshots,
  canManage,
  isLoading,
  snapshotKey,
  selectedSourceLinkId,
  quantityAvailable,
  availabilityStatus,
  snapshotNotes,
  currentOnlyFilter,
  onSnapshotKeyChange,
  onSelectedSourceLinkIdChange,
  onQuantityAvailableChange,
  onAvailabilityStatusChange,
  onSnapshotNotesChange,
  onCurrentOnlyFilterChange,
  onCreateAvailabilitySnapshot,
  isCreating,
}: AvailabilitySnapshotsPanelProps) {
  const supplierSourceLinkOptions = useMemo<PickerOption[]>(
    () =>
      parts.flatMap((part) =>
        part.supplierLinks.map((link) => ({
          value: link.linkId,
          label: `${part.partKey} · ${formatSupplierIdentitySummary({
            supplierDisplayName: link.supplierDisplayName,
            supplierKey: link.supplierKey,
            parentSupplierDisplayName: link.parentSupplierDisplayName,
            supplierUnitKind: link.supplierUnitKind,
          })} · ${link.supplierPartNumber}`,
        })),
      ),
    [parts],
  )
  const selectedSourceLinkOption = useMemo<PickerOption | undefined>(
    () => supplierSourceLinkOptions.find((link) => link.value === selectedSourceLinkId),
    [selectedSourceLinkId, supplierSourceLinkOptions],
  )
  const selectedSourceLink = useMemo(
    () => parts.flatMap((part) => part.supplierLinks).find((link) => link.linkId === selectedSourceLinkId),
    [parts, selectedSourceLinkId],
  )
  const selectedSourceLinkLabel = selectedSourceLinkOption?.label ?? ''
  const snapshotKeySource = selectedSourceLinkLabel
    ? `${selectedSourceLinkLabel} ${availabilityStatus} snapshot`
    : ''
  const existingSnapshotKeys = availabilitySnapshots.map((row) => row.snapshotKey)

  const filteredSnapshots = currentOnlyFilter
    ? availabilitySnapshots.filter((row) => row.isCurrent)
    : availabilitySnapshots

  return (
    <section
      data-testid="availability-snapshots-panel"
      className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg lg:col-span-2"
    >
      <h2 className="text-lg font-medium text-white">Supplier availability</h2>
      <p className="mt-1 text-sm text-slate-400">
        Record supplier source availability and quantity history with effective dates.
      </p>

      <label htmlFor="availability-current-only-filter" className="mt-4 flex items-center gap-2 text-sm text-slate-400">
        <input
          id="availability-current-only-filter"
          type="checkbox"
          className="rounded border-slate-600"
          checked={currentOnlyFilter}
          onChange={(e) => onCurrentOnlyFilterChange(e.target.checked)}
        />
        Show current snapshots only
      </label>

      {isLoading ? <p className="mt-4 text-sm text-[var(--color-text-muted)]">Loading availability snapshots…</p> : null}

      <ul className="mt-4 max-h-56 space-y-2 overflow-y-auto">
        {filteredSnapshots.map((row) => (
          <li
            key={row.availabilitySnapshotId}
            className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2 text-sm"
          >
            <div className="flex items-center justify-between gap-2">
              <span className="font-medium text-slate-200">{row.snapshotKey}</span>
              {row.isCurrent ? (
                <span className="rounded bg-emerald-500/20 px-2 py-0.5 text-xs text-emerald-300">
                  current
                </span>
              ) : null}
            </div>
            <p className="mt-1 text-slate-400">
              {row.partKey} ·{' '}
              {formatSupplierIdentitySummary({
                supplierDisplayName: row.supplierDisplayName,
                supplierKey: row.supplierKey,
                parentSupplierDisplayName: row.parentSupplierDisplayName ?? null,
                supplierUnitKind: row.supplierUnitKind ?? 'identity',
              })}{' '}
              ·{' '}
              {statusLabels[row.availabilityStatus] ?? row.availabilityStatus}
              {row.quantityAvailable != null ? ` · qty ${row.quantityAvailable}` : ''}
            </p>
            <p className="text-xs text-[var(--color-text-muted)]">
              {formatSupplierOperationalContext({
                supplierServiceTypes: row.supplierServiceTypes ?? [],
                addressLine1:
                  parts.flatMap((part) => part.supplierLinks).find((link) => link.linkId === row.partSupplierLinkId)
                    ?.supplierAddressLine1 ?? null,
                locality:
                  parts.flatMap((part) => part.supplierLinks).find((link) => link.linkId === row.partSupplierLinkId)
                    ?.supplierLocality ?? null,
                regionCode:
                  parts.flatMap((part) => part.supplierLinks).find((link) => link.linkId === row.partSupplierLinkId)
                    ?.supplierRegionCode ?? null,
                postalCode:
                  parts.flatMap((part) => part.supplierLinks).find((link) => link.linkId === row.partSupplierLinkId)
                    ?.supplierPostalCode ?? null,
              })}
            </p>
            <p className="text-xs text-[var(--color-text-muted)]">
              effective {new Date(row.effectiveFrom).toLocaleDateString()}
              {row.effectiveTo
                ? ` – ${new Date(row.effectiveTo).toLocaleDateString()}`
                : ' – open'}
            </p>
          </li>
        ))}
        {filteredSnapshots.length === 0 ? (
          <li className="text-sm text-[var(--color-text-muted)]">No availability snapshots yet.</li>
        ) : null}
      </ul>

      {canManage ? (
        <div className="mt-6 space-y-4 rounded-lg border border-slate-800 bg-slate-950/40 p-4">
          <label htmlFor="availability-supplier-source-link" className="block text-sm text-slate-400">
            Supplier source link
            <StaticSearchPicker
              id="availability-supplier-source-link"
              placeholder="Search supplier source links…"
              value={selectedSourceLinkId}
              options={supplierSourceLinkOptions}
              selectedOption={selectedSourceLinkOption}
              onChange={onSelectedSourceLinkIdChange}
              testId="availability-supplier-source-link-picker"
            />
          </label>
          {selectedSourceLink ? (
            <p className="text-xs text-[var(--color-text-muted)]">
              {humanizeSupplierUnitKind(selectedSourceLink.supplierUnitKind)} ·{' '}
              {formatSupplierOperationalContext(selectedSourceLink)}
            </p>
          ) : null}

          <div className="grid gap-3 sm:grid-cols-2">
            <GeneratedKeyFieldGroup
              sourceLabel={snapshotKeySource}
              existingKeys={existingSnapshotKeys}
              onKeyChange={onSnapshotKeyChange}
              domain="purchase"
              kind="availability"
              maxLength={128}
              label="Snapshot key"
              disabled={isCreating}
            />
            <label htmlFor="availability-status" className="block text-sm text-slate-400">
              Availability status
              <select
                id="availability-status"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={availabilityStatus}
                onChange={(e) => onAvailabilityStatusChange(e.target.value)}
              >
                <option value="in_stock">In stock</option>
                <option value="limited">Limited</option>
                <option value="backorder">Backorder</option>
                <option value="out_of_stock">Out of stock</option>
                <option value="discontinued">Discontinued</option>
              </select>
            </label>
            <label htmlFor="availability-quantity" className="block text-sm text-slate-400 sm:col-span-2">
              Quantity available (optional)
              <input
                id="availability-quantity"
                type="number"
                min="0"
                step="0.01"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={quantityAvailable}
                onChange={(e) => onQuantityAvailableChange(e.target.value)}
              />
            </label>
          </div>

          <label htmlFor="availability-snapshot-notes" className="block text-sm text-slate-400">
            Snapshot notes
            <input
              id="availability-snapshot-notes"
              className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
              value={snapshotNotes}
              onChange={(e) => onSnapshotNotesChange(e.target.value)}
            />
          </label>

          <button
            type="button"
            className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white hover:bg-amber-500 disabled:opacity-50"
            disabled={isCreating || !selectedSourceLinkId || !snapshotKey || !availabilityStatus}
            onClick={onCreateAvailabilitySnapshot}
          >
            {isCreating ? 'Saving availability…' : 'Record availability'}
          </button>
        </div>
      ) : null}
    </section>
  )
}
