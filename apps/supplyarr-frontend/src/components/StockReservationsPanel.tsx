import { ControlledSelect } from '@stl/shared-ui'
import { useMemo } from 'react'

import type { PartResponse, PartStockLevelResponse, StockReservationResponse } from '../api/types'
import { toBinPickerOptions, toPartPickerOptions } from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface StockReservationsPanelProps {
  reservations: StockReservationResponse[]
  stockLevels: PartStockLevelResponse[]
  parts: PartResponse[]
  bins: { binId: string; binKey: string; locationKey: string; name: string }[]
  canManage: boolean
  isLoading: boolean
  reservationKey: string
  selectedReservationId: string
  selectedReservationPartId: string
  selectedReservationBinId: string
  reservationQuantity: string
  reservationNotes: string
  releaseReason: string
  statusFilter: string
  onReservationKeyChange: (value: string) => void
  onSelectedReservationIdChange: (value: string) => void
  onSelectedReservationPartIdChange: (value: string) => void
  onSelectedReservationBinIdChange: (value: string) => void
  onReservationQuantityChange: (value: string) => void
  onReservationNotesChange: (value: string) => void
  onReleaseReasonChange: (value: string) => void
  onStatusFilterChange: (value: string) => void
  onCreateReservation: () => void
  onReleaseReservation: () => void
  onFulfillReservation: () => void
  isCreating: boolean
  isReleasing: boolean
  isFulfilling: boolean
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case 'fulfilled':
      return 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    case 'active':
      return 'bg-amber-500/20 text-amber-300 ring-amber-500/40'
    case 'released':
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
    default:
      return 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
  }
}

export function StockReservationsPanel({
  reservations,
  stockLevels,
  parts,
  bins,
  canManage,
  isLoading,
  reservationKey,
  selectedReservationId,
  selectedReservationPartId,
  selectedReservationBinId,
  reservationQuantity,
  reservationNotes,
  releaseReason,
  statusFilter,
  onReservationKeyChange,
  onSelectedReservationIdChange,
  onSelectedReservationPartIdChange,
  onSelectedReservationBinIdChange,
  onReservationQuantityChange,
  onReservationNotesChange,
  onReleaseReasonChange,
  onStatusFilterChange,
  onCreateReservation,
  onReleaseReservation,
  onFulfillReservation,
  isCreating,
  isReleasing,
  isFulfilling,
}: StockReservationsPanelProps) {
  const selected = reservations.find((row) => row.reservationId === selectedReservationId)
  const stockByBinPart = new Map(
    stockLevels.map((row) => [`${row.partId}:${row.binId}`, row.quantityAvailable]),
  )
  const existingReservationKeys = useMemo(
    () => reservations.map((row) => row.reservationKey),
    [reservations],
  )
  const selectedPart = parts.find((part) => part.partId === selectedReservationPartId)
  const reservationKeySource =
    reservationNotes.trim() ||
    (selectedPart ? `${selectedPart.displayName} hold` : '')

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
      <h2 className="text-lg font-medium text-white">Stock reservations</h2>
      <p className="mt-1 text-sm text-slate-400">
        Hold available inventory for work orders, demand refs, or manual allocations. Reserved quantity
        reduces available stock until fulfilled or released.
      </p>

      <div className="mt-4 flex flex-wrap items-end gap-3">
        <label htmlFor="stock-reservation-status-filter" className="block text-sm text-slate-400">
          Reservation status filter
          <select
            id="stock-reservation-status-filter"
            className="mt-1 block w-full min-w-[8rem] rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
            value={statusFilter}
            onChange={(e) => onStatusFilterChange(e.target.value)}
          >
            <option value="">All</option>
            <option value="active">Active</option>
            <option value="fulfilled">Fulfilled</option>
            <option value="released">Released</option>
          </select>
        </label>
      </div>

      {isLoading ? <p className="mt-4 text-sm text-slate-500">Loading reservations…</p> : null}

      <ul className="mt-4 space-y-2">
        {reservations.length === 0 && !isLoading ? (
          <li className="text-sm text-slate-500">No stock reservations yet.</li>
        ) : (
          reservations.map((row) => (
            <li key={row.reservationId}>
              <button
                type="button"
                className={`w-full rounded-lg border px-3 py-2 text-left text-sm transition ${
                  selectedReservationId === row.reservationId
                    ? 'border-sky-500 bg-sky-950/40'
                    : 'border-slate-800 bg-slate-950/50 hover:border-slate-700'
                }`}
                onClick={() => onSelectedReservationIdChange(row.reservationId)}
              >
                <div className="flex flex-wrap items-center gap-2">
                  <span className="font-medium text-slate-200">{row.reservationKey}</span>
                  <span
                    className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(row.status)}`}
                  >
                    {row.status}
                  </span>
                </div>
                <p className="mt-1 text-xs text-slate-400">
                  {row.partDisplayName} ({row.partKey}) · {row.locationKey}/{row.binKey} · qty{' '}
                  {row.quantityReserved}
                </p>
              </button>
            </li>
          ))
        )}
      </ul>

      {selected && canManage && selected.status === 'active' ? (
        <div className="mt-4 rounded-lg border border-slate-800 bg-slate-950/50 p-4">
          <p className="text-sm text-slate-300">
            Selected: {selected.reservationKey} — {selected.quantityReserved} reserved at{' '}
            {selected.locationName}/{selected.binName}
          </p>
          {selected.notes ? (
            <p className="mt-1 text-xs text-slate-500">Notes: {selected.notes}</p>
          ) : null}
          <div className="mt-3 flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded bg-emerald-600 px-3 py-1.5 text-sm text-white hover:bg-emerald-500 disabled:opacity-50"
              disabled={isFulfilling}
              onClick={onFulfillReservation}
            >
              {isFulfilling ? 'Fulfilling…' : 'Fulfill (issue stock)'}
            </button>
          </div>
          <div className="mt-3">
            <label htmlFor="stock-reservation-release-reason" className="block text-sm text-slate-400">
              Release reason (optional)
              <input
                id="stock-reservation-release-reason"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Release reason (optional)"
                value={releaseReason}
                onChange={(e) => onReleaseReasonChange(e.target.value)}
              />
            </label>
            <button
              type="button"
              className="mt-2 rounded bg-slate-700 px-3 py-1.5 text-sm text-white hover:bg-slate-600 disabled:opacity-50"
              disabled={isReleasing}
              onClick={onReleaseReservation}
            >
              {isReleasing ? 'Releasing…' : 'Release reservation'}
            </button>
          </div>
        </div>
      ) : null}

      {canManage ? (
        <div className="mt-6 space-y-3 border-t border-slate-800 pt-6">
          <h3 className="text-sm font-medium text-slate-300">Create reservation</h3>
          <div className="grid gap-2 sm:grid-cols-2">
            <ControlledSelect
              id="stock-reservation-part"
              label="Part"
              value={selectedReservationPartId}
              onChange={onSelectedReservationPartIdChange}
              options={toPartPickerOptions(parts)}
              emptyLabel="Select part"
            />
            <ControlledSelect
              id="stock-reservation-bin"
              label="Bin"
              value={selectedReservationBinId}
              onChange={onSelectedReservationBinIdChange}
              options={toBinPickerOptions(bins)}
              emptyLabel="Select bin"
            />
            <label htmlFor="stock-reservation-notes" className="block text-sm text-slate-400 sm:col-span-2">
              Reservation notes (optional)
              <input
                id="stock-reservation-notes"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Notes (optional)"
                value={reservationNotes}
                onChange={(e) => onReservationNotesChange(e.target.value)}
              />
            </label>
            <div className="sm:col-span-2">
              <GeneratedKeyFieldGroup
                sourceLabel={reservationKeySource}
                existingKeys={existingReservationKeys}
                onKeyChange={onReservationKeyChange}
                label="Reservation key"
              />
            </div>
            <label htmlFor="stock-reservation-quantity" className="block text-sm text-slate-400">
              Reservation quantity
              <input
                id="stock-reservation-quantity"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Quantity"
                type="number"
                min="0"
                step="any"
                value={reservationQuantity}
                onChange={(e) => onReservationQuantityChange(e.target.value)}
              />
            </label>
          </div>
          {selectedReservationPartId && selectedReservationBinId ? (
            <p className="text-xs text-slate-500">
              Available:{' '}
              {stockByBinPart.get(`${selectedReservationPartId}:${selectedReservationBinId}`) ?? 0}
            </p>
          ) : null}
          <button
            type="button"
            className="rounded bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
            disabled={
              !reservationKey ||
              !selectedReservationPartId ||
              !selectedReservationBinId ||
              !reservationQuantity ||
              isCreating
            }
            onClick={onCreateReservation}
          >
            {isCreating ? 'Creating…' : 'Create reservation'}
          </button>
        </div>
      ) : null}
    </section>
  )
}
