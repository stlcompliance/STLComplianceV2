import type {
  InventoryBinResponse,
  InventoryLocationResponse,
  PartResponse,
  PartStockLevelResponse,
} from '../api/types'

interface InventoryPanelProps {
  locations: InventoryLocationResponse[]
  bins: InventoryBinResponse[]
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
  onCreateLocation: () => void
  onCreateBin: () => void
  onUpsertStock: () => void
  isCreatingLocation: boolean
  isCreatingBin: boolean
  isUpsertingStock: boolean
}

function statusBadgeClass(status: string): string {
  return status === 'active'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
}

export function InventoryPanel({
  locations,
  bins,
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
  onCreateLocation,
  onCreateBin,
  onUpsertStock,
  isCreatingLocation,
  isCreatingBin,
  isUpsertingStock,
}: InventoryPanelProps) {
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
                    {row.locationName}/{row.binKey}: on hand {row.quantityOnHand}, available{' '}
                    {row.quantityAvailable}
                  </li>
                ))}
              </ul>
            )}
          </div>
        </>
      )}

      {canManage ? (
        <div className="mt-6 space-y-6 border-t border-slate-800 pt-6">
          <div>
            <h3 className="text-sm font-medium text-slate-300">Add warehouse / site</h3>
            <div className="mt-2 grid gap-2 sm:grid-cols-2">
              <input
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Location key"
                value={locationKey}
                onChange={(e) => onLocationKeyChange(e.target.value)}
              />
              <input
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Display name"
                value={locationName}
                onChange={(e) => onLocationNameChange(e.target.value)}
              />
              <select
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={locationType}
                onChange={(e) => onLocationTypeChange(e.target.value)}
              >
                <option value="warehouse">warehouse</option>
                <option value="site">site</option>
              </select>
              <input
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Address (optional)"
                value={addressLine}
                onChange={(e) => onAddressLineChange(e.target.value)}
              />
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
            <select
              className="mt-2 w-full rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
              value={selectedLocationId}
              onChange={(e) => onSelectedLocationIdChange(e.target.value)}
            >
              <option value="">Select location</option>
              {locations.map((loc) => (
                <option key={loc.locationId} value={loc.locationId}>
                  {loc.name}
                </option>
              ))}
            </select>
            <div className="mt-2 grid gap-2 sm:grid-cols-2">
              <input
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Bin key"
                value={binKey}
                onChange={(e) => onBinKeyChange(e.target.value)}
              />
              <input
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Bin name"
                value={binName}
                onChange={(e) => onBinNameChange(e.target.value)}
              />
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
              <select
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={selectedPartId}
                onChange={(e) => onSelectedPartIdChange(e.target.value)}
              >
                <option value="">Select part</option>
                {parts.map((part) => (
                  <option key={part.partId} value={part.partId}>
                    {part.displayName} ({part.partKey})
                  </option>
                ))}
              </select>
              <select
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                value={selectedBinId}
                onChange={(e) => onSelectedBinIdChange(e.target.value)}
              >
                <option value="">Select bin</option>
                {bins.map((bin) => (
                  <option key={bin.binId} value={bin.binId}>
                    {bin.locationKey}/{bin.binKey}
                  </option>
                ))}
              </select>
              <input
                className="rounded border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-white"
                placeholder="Quantity on hand"
                type="number"
                min="0"
                step="any"
                value={stockQuantity}
                onChange={(e) => onStockQuantityChange(e.target.value)}
              />
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
        </div>
      ) : null}
    </section>
  )
}
