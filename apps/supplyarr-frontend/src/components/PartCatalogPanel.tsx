import type { PartCatalogResponse, PartResponse } from '../api/types'

interface PartCatalogPanelProps {
  catalogs: PartCatalogResponse[]
  parts: PartResponse[]
  canManage: boolean
  isLoading: boolean
  catalogKey: string
  catalogName: string
  catalogDescription: string
  partKey: string
  partName: string
  partCategory: string
  partUom: string
  partManufacturer: string
  partMfgNumber: string
  selectedCatalogId: string
  vendorPartNumber: string
  selectedPartId: string
  selectedVendorId: string
  vendors: { partyId: string; displayName: string; partyKey: string }[]
  onCatalogKeyChange: (value: string) => void
  onCatalogNameChange: (value: string) => void
  onCatalogDescriptionChange: (value: string) => void
  onPartKeyChange: (value: string) => void
  onPartNameChange: (value: string) => void
  onPartCategoryChange: (value: string) => void
  onPartUomChange: (value: string) => void
  onPartManufacturerChange: (value: string) => void
  onPartMfgNumberChange: (value: string) => void
  onSelectedCatalogIdChange: (value: string) => void
  onVendorPartNumberChange: (value: string) => void
  onSelectedPartIdChange: (value: string) => void
  onSelectedVendorIdChange: (value: string) => void
  onCreateCatalog: () => void
  onCreatePart: () => void
  onLinkVendor: () => void
  isCreatingCatalog: boolean
  isCreatingPart: boolean
  isLinkingVendor: boolean
}

function statusBadgeClass(status: string): string {
  return status === 'active'
    ? 'bg-emerald-500/20 text-emerald-300 ring-emerald-500/40'
    : 'bg-slate-500/20 text-slate-300 ring-slate-500/40'
}

export function PartCatalogPanel({
  catalogs,
  parts,
  canManage,
  isLoading,
  catalogKey,
  catalogName,
  catalogDescription,
  partKey,
  partName,
  partCategory,
  partUom,
  partManufacturer,
  partMfgNumber,
  selectedCatalogId,
  vendorPartNumber,
  selectedPartId,
  selectedVendorId,
  vendors,
  onCatalogKeyChange,
  onCatalogNameChange,
  onCatalogDescriptionChange,
  onPartKeyChange,
  onPartNameChange,
  onPartCategoryChange,
  onPartUomChange,
  onPartManufacturerChange,
  onPartMfgNumberChange,
  onSelectedCatalogIdChange,
  onVendorPartNumberChange,
  onSelectedPartIdChange,
  onSelectedVendorIdChange,
  onCreateCatalog,
  onCreatePart,
  onLinkVendor,
  isCreatingCatalog,
  isCreatingPart,
  isLinkingVendor,
}: PartCatalogPanelProps) {
  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading part catalog…</p>
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2">
      <h2 className="text-lg font-medium text-white">Part catalog</h2>
      <p className="mt-1 text-sm text-slate-400">SKUs, categories, UOM, and vendor cross-references.</p>

      <div className="mt-6 grid gap-6 md:grid-cols-2">
        <div>
          <h3 className="text-sm font-medium text-slate-300">Catalogs</h3>
          <ul className="mt-3 space-y-2 text-sm">
            {catalogs.length === 0 ? (
              <li className="text-slate-400">No catalogs yet.</li>
            ) : (
              catalogs.map((catalog) => (
                <li key={catalog.catalogId} className="rounded-lg border border-slate-800 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <div className="font-medium">{catalog.name}</div>
                      <div className="text-slate-400">{catalog.catalogKey}</div>
                    </div>
                    <span className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(catalog.status)}`}>
                      {catalog.status}
                    </span>
                  </div>
                </li>
              ))
            )}
          </ul>
        </div>

        <div>
          <h3 className="text-sm font-medium text-slate-300">Parts</h3>
          <ul className="mt-3 space-y-2 text-sm">
            {parts.length === 0 ? (
              <li className="text-slate-400">No parts yet.</li>
            ) : (
              parts.map((part) => (
                <li key={part.partId} className="rounded-lg border border-slate-800 p-3">
                  <div className="flex items-start justify-between gap-2">
                    <div>
                      <div className="font-medium">{part.displayName}</div>
                      <div className="text-slate-400">
                        {part.partKey}
                        {part.catalogKey ? ` · ${part.catalogKey}` : ''}
                      </div>
                      <div className="mt-1 text-slate-500">
                        {part.categoryKey} · {part.unitOfMeasure}
                      </div>
                    </div>
                    <span className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(part.status)}`}>
                      {part.status}
                    </span>
                  </div>
                  {part.vendorLinks.length > 0 ? (
                    <p className="mt-2 text-slate-400">
                      Vendor: {part.vendorLinks.find((v) => v.isPreferred)?.partyDisplayName ?? part.vendorLinks[0].partyDisplayName}
                      {part.vendorLinks[0].vendorPartNumber
                        ? ` (${part.vendorLinks[0].vendorPartNumber})`
                        : ''}
                    </p>
                  ) : null}
                </li>
              ))
            )}
          </ul>
        </div>
      </div>

      {canManage ? (
        <div className="mt-6 grid gap-6 border-t border-slate-800 pt-6 lg:grid-cols-3">
          <div className="space-y-2">
            <h3 className="text-sm font-medium text-slate-300">Add catalog</h3>
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Catalog key"
              value={catalogKey}
              onChange={(e) => onCatalogKeyChange(e.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Name"
              value={catalogName}
              onChange={(e) => onCatalogNameChange(e.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Description"
              value={catalogDescription}
              onChange={(e) => onCatalogDescriptionChange(e.target.value)}
            />
            <button
              type="button"
              className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={isCreatingCatalog || !catalogKey.trim() || !catalogName.trim()}
              onClick={onCreateCatalog}
            >
              {isCreatingCatalog ? 'Creating…' : 'Add catalog'}
            </button>
          </div>

          <div className="space-y-2">
            <h3 className="text-sm font-medium text-slate-300">Add part SKU</h3>
            <select
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              value={selectedCatalogId}
              onChange={(e) => onSelectedCatalogIdChange(e.target.value)}
            >
              <option value="">No catalog</option>
              {catalogs.map((c) => (
                <option key={c.catalogId} value={c.catalogId}>
                  {c.name}
                </option>
              ))}
            </select>
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Part key"
              value={partKey}
              onChange={(e) => onPartKeyChange(e.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Display name"
              value={partName}
              onChange={(e) => onPartNameChange(e.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Category key"
              value={partCategory}
              onChange={(e) => onPartCategoryChange(e.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Unit of measure"
              value={partUom}
              onChange={(e) => onPartUomChange(e.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Manufacturer"
              value={partManufacturer}
              onChange={(e) => onPartManufacturerChange(e.target.value)}
            />
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Manufacturer part #"
              value={partMfgNumber}
              onChange={(e) => onPartMfgNumberChange(e.target.value)}
            />
            <button
              type="button"
              className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={isCreatingPart || !partKey.trim() || !partName.trim()}
              onClick={onCreatePart}
            >
              {isCreatingPart ? 'Creating…' : 'Add part'}
            </button>
          </div>

          <div className="space-y-2">
            <h3 className="text-sm font-medium text-slate-300">Link vendor</h3>
            <select
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              value={selectedPartId}
              onChange={(e) => onSelectedPartIdChange(e.target.value)}
            >
              <option value="">Select part</option>
              {parts.map((p) => (
                <option key={p.partId} value={p.partId}>
                  {p.displayName}
                </option>
              ))}
            </select>
            <select
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              value={selectedVendorId}
              onChange={(e) => onSelectedVendorIdChange(e.target.value)}
            >
              <option value="">Select vendor</option>
              {vendors.map((v) => (
                <option key={v.partyId} value={v.partyId}>
                  {v.displayName}
                </option>
              ))}
            </select>
            <input
              className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Vendor part number"
              value={vendorPartNumber}
              onChange={(e) => onVendorPartNumberChange(e.target.value)}
            />
            <button
              type="button"
              className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={
                isLinkingVendor || !selectedPartId || !selectedVendorId || !vendorPartNumber.trim()
              }
              onClick={onLinkVendor}
            >
              {isLinkingVendor ? 'Linking…' : 'Link vendor'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
