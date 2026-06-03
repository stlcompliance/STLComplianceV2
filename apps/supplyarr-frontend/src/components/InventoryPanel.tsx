import { ControlledSelect, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useMemo } from 'react'

import type {
  InventoryBinResponse,
  InventoryLocationResponse,
  PartResponse,
  PartStockLevelResponse,
  WmsMovementResponse,
  WmsStockLedgerEntryResponse,
} from '../api/types'
import {
  LOCATION_TYPE_OPTIONS,
  toBinPickerOptions,
  toPartPickerOptions,
} from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface InventoryPanelProps {
  locations: InventoryLocationResponse[]
  bins: InventoryBinResponse[]
  transferBins?: InventoryBinResponse[]
  stockLevels: PartStockLevelResponse[]
  parts: PartResponse[]
  canManage: boolean
  isLoading: boolean
  locationKey: string
  locationName: string
  locationType: string
  addressLine: string
  binKey: string
  binName: string
  selectedLocationId: string
  selectedPartId: string
  selectedBinId: string
  stockQuantity: string
  transferKey?: string
  transferPartId?: string
  transferFromBinId?: string
  transferToBinId?: string
  transferQuantity?: string
  transferNotes?: string
  lastTransferResult?: WmsMovementResponse | null
  stockLedger?: WmsStockLedgerEntryResponse[]
  onLocationKeyChange: (value: string) => void
  onLocationNameChange: (value: string) => void
  onLocationTypeChange: (value: string) => void
  onAddressLineChange: (value: string) => void
  onBinKeyChange: (value: string) => void
  onBinNameChange: (value: string) => void
  onSelectedLocationIdChange: (value: string) => void
  onSelectedPartIdChange: (value: string) => void
  onSelectedBinIdChange: (value: string) => void
  onStockQuantityChange: (value: string) => void
  onTransferKeyChange?: (value: string) => void
  onTransferPartIdChange?: (value: string) => void
  onTransferFromBinIdChange?: (value: string) => void
  onTransferToBinIdChange?: (value: string) => void
  onTransferQuantityChange?: (value: string) => void
  onTransferNotesChange?: (value: string) => void
  onCreateLocation: () => void
  onCreateBin: () => void
  onUpsertStock: () => void
  onTransferStock?: () => void
  isCreatingLocation: boolean
  isCreatingBin: boolean
  isUpsertingStock: boolean
  isTransferring?: boolean
}

function statusBadgeClass(status: string): string {
  return status === 'active'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
}

export function InventoryPanel({
  locations,
  bins,
  transferBins = [],
  stockLevels,
  parts,
  canManage,
  isLoading,
  locationKey,
  locationName,
  locationType,
  addressLine,
  binKey,
  binName,
  selectedLocationId,
  selectedPartId,
  selectedBinId,
  stockQuantity,
  transferKey = '',
  transferPartId = '',
  transferFromBinId = '',
  transferToBinId = '',
  transferQuantity = '',
  transferNotes = '',
  lastTransferResult = null,
  stockLedger = [],
  onLocationKeyChange,
  onLocationNameChange,
  onLocationTypeChange,
  onAddressLineChange,
  onBinKeyChange,
  onBinNameChange,
  onSelectedLocationIdChange,
  onSelectedPartIdChange,
  onSelectedBinIdChange,
  onStockQuantityChange,
  onTransferKeyChange = () => {},
  onTransferPartIdChange = () => {},
  onTransferFromBinIdChange = () => {},
  onTransferToBinIdChange = () => {},
  onTransferQuantityChange = () => {},
  onTransferNotesChange = () => {},
  onCreateLocation,
  onCreateBin,
  onUpsertStock,
  onTransferStock = () => {},
  isCreatingLocation,
  isCreatingBin,
  isUpsertingStock,
  isTransferring = false,
}: InventoryPanelProps) {
  const locationKeys = useMemo(() => locations.map((location) => location.locationKey), [locations])
  const binKeys = useMemo(() => bins.map((bin) => bin.binKey), [bins])
  const locationOptions = useMemo<PickerOption[]>(
    () =>
      locations.map((location) => ({
        value: location.locationId,
        label: `${location.locationKey} · ${location.name}`,
      })),
    [locations],
  )
  const selectedLocationOption = useMemo<PickerOption | undefined>(
    () => locationOptions.find((option) => option.value === selectedLocationId),
    [locationOptions, selectedLocationId],
  )
  const partOptions = useMemo<PickerOption[]>(
    () => toPartPickerOptions(parts),
    [parts],
  )
  const selectedPartOption = useMemo<PickerOption | undefined>(
    () => partOptions.find((option) => option.value === selectedPartId),
    [partOptions, selectedPartId],
  )
  const binOptions = useMemo<PickerOption[]>(
    () => toBinPickerOptions(bins),
    [bins],
  )
  const selectedBinOption = useMemo<PickerOption | undefined>(
    () => binOptions.find((option) => option.value === selectedBinId),
    [binOptions, selectedBinId],
  )
  const transferPartOptions = useMemo<PickerOption[]>(
    () => toPartPickerOptions(parts),
    [parts],
  )
  const selectedTransferPartOption = useMemo<PickerOption | undefined>(
    () => transferPartOptions.find((option) => option.value === transferPartId),
    [transferPartOptions, transferPartId],
  )
  const transferFromBinOptions = useMemo<PickerOption[]>(
    () => toBinPickerOptions(transferBins),
    [transferBins],
  )
  const selectedTransferFromBinOption = useMemo<PickerOption | undefined>(
    () => transferFromBinOptions.find((option) => option.value === transferFromBinId),
    [transferFromBinOptions, transferFromBinId],
  )
  const transferToBinOptions = useMemo<PickerOption[]>(
    () => toBinPickerOptions(transferBins),
    [transferBins],
  )
  const selectedTransferToBinOption = useMemo<PickerOption | undefined>(
    () => transferToBinOptions.find((option) => option.value === transferToBinId),
    [transferToBinOptions, transferToBinId],
  )
  const transferSourceLabel = useMemo(() => {
    const part = parts.find((item) => item.partId === transferPartId)
    const fromBin = transferBins.find((item) => item.binId === transferFromBinId)
    const toBin = transferBins.find((item) => item.binId === transferToBinId)
    return [part?.displayName, fromBin ? `${fromBin.locationKey}/${fromBin.binKey}` : '', toBin ? `${toBin.locationKey}/${toBin.binKey}` : '']
      .filter(Boolean)
      .join(' transfer ')
  }, [parts, transferBins, transferPartId, transferFromBinId, transferToBinId])
  const formatLedgerChange = (entry: WmsStockLedgerEntryResponse): string => {
    const onHand = entry.quantityOnHandDelta >= 0 ? `+${entry.quantityOnHandDelta}` : `${entry.quantityOnHandDelta}`
    const reserved = entry.quantityReservedDelta >= 0 ? `+${entry.quantityReservedDelta}` : `${entry.quantityReservedDelta}`
    return `on hand ${onHand} → ${entry.quantityOnHandAfter}, reserved ${reserved} → ${entry.quantityReservedAfter}`
  }

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5">
      <h2 className="text-lg font-medium text-white">Inventory locations</h2>
      <p className="mt-1 text-sm text-slate-400">Warehouses, bins, and stock on hand per part.</p>

      {isLoading ? (
        <p className="mt-4 text-sm text-slate-500">Loading inventory…</p>
      ) : (
        <>
          <div className="mt-4 space-y-2">
            {locations.length === 0 ? (
              <p className="text-sm text-slate-500">No inventory locations yet.</p>
            ) : (
              locations.map((loc) => (
                <div
                  key={loc.locationId}
                  className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-slate-800 bg-slate-950/50 px-3 py-2"
                >
                  <div>
                    <div className="font-medium text-slate-200">
                      {loc.name}{' '}
                      <span className="text-xs font-normal text-slate-500">({loc.locationKey})</span>
                    </div>
                    <div className="text-xs text-slate-500">
                      {loc.locationType} · {loc.binCount} bin{loc.binCount === 1 ? '' : 's'}
                      {loc.addressLine ? ` · ${loc.addressLine}` : ''}
                    </div>
                  </div>
                  <span
                    className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(loc.status)}`}
                  >
                    {loc.status}
                  </span>
                </div>
              ))
            )}
          </div>

          {bins.length > 0 ? (
            <div className="mt-4">
              <h3 className="text-sm font-medium text-slate-300">Bins at selected location</h3>
              <ul className="mt-2 space-y-1 text-sm text-slate-400">
                {bins.map((bin) => (
                  <li key={bin.binId}>
                    {bin.name} ({bin.binKey}) —{' '}
                    <span className={bin.status === 'active' ? 'text-emerald-400' : 'text-slate-500'}>
                      {bin.status}
                    </span>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

          <div className="mt-4">
            <h3 className="text-sm font-medium text-slate-300">Stock levels</h3>
            {stockLevels.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500">No stock recorded yet.</p>
            ) : (
              <ul className="mt-2 space-y-1 text-sm text-slate-400">
                {stockLevels.map((row) => (
                  <li key={row.stockLevelId}>
                    <span className="text-slate-200">{row.partDisplayName}</span> ({row.partKey}) @{' '}
                    {row.locationName}/{row.binKey}: on hand {row.quantityOnHand}, reserved{' '}
                    {row.quantityReserved}, available {row.quantityAvailable}
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="mt-4">
            <h3 className="text-sm font-medium text-slate-300">Stock ledger</h3>
            <p className="mt-1 text-sm text-slate-500">
              Immutable movement history for the currently selected inventory context.
            </p>
            {stockLedger.length === 0 ? (
              <p className="mt-2 text-sm text-slate-500">No ledger entries found for this filter.</p>
            ) : (
              <div className="mt-2 overflow-x-auto rounded-lg border border-slate-800">
                <table className="min-w-full divide-y divide-slate-800 text-sm">
                  <thead className="bg-slate-950/70 text-slate-300">
                    <tr>
                      <th className="px-3 py-2 text-left font-medium">When</th>
                      <th className="px-3 py-2 text-left font-medium">Movement</th>
                      <th className="px-3 py-2 text-left font-medium">Part</th>
                      <th className="px-3 py-2 text-left font-medium">Bin</th>
                      <th className="px-3 py-2 text-left font-medium">Change</th>
                      <th className="px-3 py-2 text-left font-medium">Source</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-900 bg-slate-950/30 text-slate-400">
                    {stockLedger.map((entry) => (
                      <tr key={entry.ledgerEntryId} data-testid={`stock-ledger-row-${entry.ledgerEntryId}`}>
                        <td className="px-3 py-2 whitespace-nowrap">
                          {new Date(entry.createdAt).toLocaleString()}
                        </td>
                        <td className="px-3 py-2 whitespace-nowrap text-slate-200">{entry.movementType}</td>
                        <td className="px-3 py-2">
                          {entry.partDisplayName} <span className="text-xs text-slate-500">({entry.partKey})</span>
                        </td>
                        <td className="px-3 py-2">
                          {entry.locationKey}/{entry.binKey}
                        </td>
                        <td className="px-3 py-2 whitespace-nowrap">{formatLedgerChange(entry)}</td>
                        <td className="px-3 py-2">
                          <span className="block">{entry.sourceType}</span>
                          {entry.notes ? <span className="text-xs text-slate-500">{entry.notes}</span> : null}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}

      {canManage ? (
        <div className="mt-6 space-y-6 border-t border-slate-800 pt-6">
          <div>
            <h3 className="text-sm font-medium text-slate-300">Add warehouse / site</h3>
            <div className="mt-2 grid gap-2 sm:grid-cols-2">
              <label htmlFor="inventory-location-name" className="block text-sm text-slate-400 sm:col-span-2">
                Display name
                <input
                  id="inventory-location-name"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={locationName}
                  onChange={(e) => onLocationNameChange(e.target.value)}
                />
              </label>
              <div className="sm:col-span-2">
                <GeneratedKeyFieldGroup
                  sourceLabel={locationName}
                  existingKeys={locationKeys}
                  onKeyChange={onLocationKeyChange}
                  domain="inventory"
                  kind="location"
                  label="Location key"
                />
              </div>
              <ControlledSelect
                label="Location type"
                value={locationType}
                onChange={onLocationTypeChange}
                options={LOCATION_TYPE_OPTIONS}
              />
              <label htmlFor="inventory-location-address" className="block text-sm text-slate-400">
                Address (optional)
                <input
                  id="inventory-location-address"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={addressLine}
                  onChange={(e) => onAddressLineChange(e.target.value)}
                />
              </label>
            </div>
            <button
              type="button"
              className="mt-2 rounded bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={!locationKey || !locationName || isCreatingLocation}
              onClick={onCreateLocation}
            >
              {isCreatingLocation ? 'Creating…' : 'Create location'}
            </button>
          </div>

          <div>
            <h3 className="text-sm font-medium text-slate-300">Add bin</h3>
            <StaticSearchPicker
              label="Location"
              id="inventory-bin-location"
              value={selectedLocationId}
              options={locationOptions}
              selectedOption={selectedLocationOption}
              onChange={onSelectedLocationIdChange}
              placeholder="Select location"
              testId="inventory-bin-location-picker"
            />
            <div className="mt-2 grid gap-2 sm:grid-cols-2">
              <label htmlFor="inventory-bin-name" className="block text-sm text-slate-400 sm:col-span-2">
                Bin name
                <input
                  id="inventory-bin-name"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  value={binName}
                  onChange={(e) => onBinNameChange(e.target.value)}
                />
              </label>
              <div className="sm:col-span-2">
                <GeneratedKeyFieldGroup
                  sourceLabel={binName}
                  existingKeys={binKeys}
                  onKeyChange={onBinKeyChange}
                  domain="inventory"
                  kind="bin"
                  label="Bin key"
                />
              </div>
            </div>
            <button
              type="button"
              className="mt-2 rounded bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={!selectedLocationId || !binKey || !binName || isCreatingBin}
              onClick={onCreateBin}
            >
              {isCreatingBin ? 'Creating…' : 'Create bin'}
            </button>
          </div>

          <div>
            <h3 className="text-sm font-medium text-slate-300">Set stock on hand</h3>
            <div className="mt-2 grid gap-2">
              <StaticSearchPicker
                label="Part"
                id="inventory-stock-part"
                value={selectedPartId}
                options={partOptions}
                selectedOption={selectedPartOption}
                onChange={onSelectedPartIdChange}
                placeholder="Select part"
                testId="inventory-stock-part-picker"
              />
              <StaticSearchPicker
                label="Bin"
                id="inventory-stock-bin"
                value={selectedBinId}
                options={binOptions}
                selectedOption={selectedBinOption}
                onChange={onSelectedBinIdChange}
                placeholder="Select bin"
                testId="inventory-stock-bin-picker"
              />
              <label htmlFor="inventory-stock-quantity" className="block text-sm text-slate-400">
                Quantity on hand
                <input
                  id="inventory-stock-quantity"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  type="number"
                  min="0"
                  step="any"
                  value={stockQuantity}
                  onChange={(e) => onStockQuantityChange(e.target.value)}
                />
              </label>
            </div>
            <button
              type="button"
              className="mt-2 rounded bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={
                !selectedPartId || !selectedBinId || stockQuantity === '' || isUpsertingStock
              }
              onClick={onUpsertStock}
            >
              {isUpsertingStock ? 'Saving…' : 'Save stock level'}
            </button>
          </div>

          <div>
            <h3 className="text-sm font-medium text-slate-300">Transfer stock</h3>
            <p className="mt-1 text-sm text-slate-500">
              Move on-hand inventory from one bin to another and write a ledger trail.
            </p>
            <div className="mt-2 grid gap-2">
              <StaticSearchPicker
                label="Part"
                id="inventory-transfer-part"
                value={transferPartId}
                options={transferPartOptions}
                selectedOption={selectedTransferPartOption}
                onChange={onTransferPartIdChange}
                placeholder="Select part"
                testId="inventory-transfer-part-picker"
              />
              <StaticSearchPicker
                label="From bin"
                id="inventory-transfer-from-bin"
                value={transferFromBinId}
                options={transferFromBinOptions}
                selectedOption={selectedTransferFromBinOption}
                onChange={onTransferFromBinIdChange}
                placeholder="Select source bin"
                testId="inventory-transfer-from-bin-picker"
              />
              <StaticSearchPicker
                label="To bin"
                id="inventory-transfer-to-bin"
                value={transferToBinId}
                options={transferToBinOptions}
                selectedOption={selectedTransferToBinOption}
                onChange={onTransferToBinIdChange}
                placeholder="Select destination bin"
                testId="inventory-transfer-to-bin-picker"
              />
              <label htmlFor="inventory-transfer-quantity" className="block text-sm text-slate-400">
                Quantity
                <input
                  id="inventory-transfer-quantity"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  type="number"
                  min="0"
                  step="any"
                  value={transferQuantity}
                  onChange={(e) => onTransferQuantityChange(e.target.value)}
                />
              </label>
              <label htmlFor="inventory-transfer-notes" className="block text-sm text-slate-400">
                Notes
                <textarea
                  id="inventory-transfer-notes"
                  className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                  rows={3}
                  value={transferNotes}
                  onChange={(e) => onTransferNotesChange(e.target.value)}
                />
              </label>
              <div className="sm:col-span-2">
                <GeneratedKeyFieldGroup
                  sourceLabel={transferSourceLabel}
                  existingKeys={[]}
                  onKeyChange={onTransferKeyChange}
                  domain="inventory"
                  kind="transfer"
                  label="Idempotency key"
                />
              </div>
            </div>
            <button
              type="button"
              className="mt-2 rounded bg-sky-600 px-3 py-1.5 text-sm text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={
                !transferKey ||
                !transferPartId ||
                !transferFromBinId ||
                !transferToBinId ||
                transferFromBinId === transferToBinId ||
                transferQuantity === '' ||
                isTransferring
              }
              onClick={onTransferStock}
            >
              {isTransferring ? 'Transferring…' : 'Transfer stock'}
            </button>
            {lastTransferResult ? (
              <div className="mt-3 rounded border border-emerald-500/30 bg-emerald-500/10 p-3 text-sm text-emerald-100">
                <p className="font-medium">
                  Transfer complete. Movement group{' '}
                  <span className="font-mono">{lastTransferResult.movementGroupId}</span>.
                </p>
                <p className="mt-1 text-emerald-100/80">
                  {lastTransferResult.entries.length} ledger entr{lastTransferResult.entries.length === 1 ? 'y' : 'ies'} recorded with key{' '}
                  <span className="font-mono">{lastTransferResult.idempotencyKey}</span>.
                </p>
                <ul className="mt-2 space-y-1 text-xs text-emerald-100/80" data-testid="inventory-transfer-result">
                  {lastTransferResult.entries.map((entry) => (
                    <li key={entry.ledgerEntryId}>
                      {entry.movementType} · {entry.locationKey}/{entry.binKey} · on hand {entry.quantityOnHandDelta >= 0 ? '+' : ''}
                      {entry.quantityOnHandDelta} → {entry.quantityOnHandAfter}
                    </li>
                  ))}
                </ul>
              </div>
            ) : null}
          </div>
        </div>
      ) : null}
    </section>
  )
}

