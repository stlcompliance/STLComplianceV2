import { ControlledSelect, StaticSearchPicker, normalizeUom, type PickerOption } from '@stl/shared-ui'
import { useMemo } from 'react'

import type { PartCatalogResponse, PartResponse } from '../api/types'
import {
  distinctCategoryOptions,
  toCatalogPickerOptions,
  toPartPickerOptions,
  toPartyPickerOptions,
  UOM_OPTIONS,
} from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

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
  const catalogKeys = useMemo(() => catalogs.map((catalog) => catalog.catalogKey), [catalogs])
  const partKeys = useMemo(() => parts.map((part) => part.partKey), [parts])
  const categoryOptions = useMemo(() => distinctCategoryOptions(parts), [parts])
  const catalogOptions = useMemo<PickerOption[]>(
    () => toCatalogPickerOptions(catalogs),
    [catalogs],
  )
  const selectedCatalogOption = useMemo<PickerOption | undefined>(
    () => catalogOptions.find((option) => option.value === selectedCatalogId),
    [catalogOptions, selectedCatalogId],
  )
  const partOptions = useMemo<PickerOption[]>(
    () => toPartPickerOptions(parts),
    [parts],
  )
  const selectedPartOption = useMemo<PickerOption | undefined>(
    () => partOptions.find((option) => option.value === selectedPartId),
    [partOptions, selectedPartId],
  )
  const vendorOptions = useMemo<PickerOption[]>(
    () => toPartyPickerOptions(vendors),
    [vendors],
  )
  const selectedVendorOption = useMemo<PickerOption | undefined>(
    () => vendorOptions.find((option) => option.value === selectedVendorId),
    [selectedVendorId, vendorOptions],
  )

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
                      <div className="mt-1 text-[var(--color-text-muted)]">
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
            <label htmlFor="part-catalog-name" className="block text-sm text-slate-400">
              Catalog name
              <input
                id="part-catalog-name"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Catalog name"
                value={catalogName}
                onChange={(e) => onCatalogNameChange(e.target.value)}
              />
            </label>
            <GeneratedKeyFieldGroup
              sourceLabel={catalogName}
              existingKeys={catalogKeys}
              onKeyChange={onCatalogKeyChange}
              domain="part"
              kind="catalog"
              label="Catalog key"
            />
            <label htmlFor="part-catalog-description" className="block text-sm text-slate-400">
              Catalog description
              <input
                id="part-catalog-description"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Description"
                value={catalogDescription}
                onChange={(e) => onCatalogDescriptionChange(e.target.value)}
              />
            </label>
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
            <StaticSearchPicker
              id="part-catalog-select"
              label="Part catalog"
              value={selectedCatalogId}
              onChange={onSelectedCatalogIdChange}
              options={catalogOptions}
              selectedOption={selectedCatalogOption}
              placeholder="No catalog"
              testId="part-catalog-picker"
            />
            <label htmlFor="part-display-name" className="block text-sm text-slate-400">
              Part display name
              <input
                id="part-display-name"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Display name"
                value={partName}
                onChange={(e) => onPartNameChange(e.target.value)}
              />
            </label>
            <GeneratedKeyFieldGroup
              sourceLabel={partName}
              existingKeys={partKeys}
              onKeyChange={onPartKeyChange}
              domain="part"
              kind="item"
              label="Part key"
            />
            <ControlledSelect
              id="part-category"
              label="Part category"
              value={partCategory}
              onChange={onPartCategoryChange}
              options={categoryOptions}
              emptyLabel="Select category…"
            />
            <ControlledSelect
              id="part-unit-of-measure"
              label="Unit of measure"
              value={partUom}
              onChange={(value) => onPartUomChange(normalizeUom(value))}
              options={UOM_OPTIONS}
              emptyLabel="Select UOM…"
            />
            <label htmlFor="part-manufacturer" className="block text-sm text-slate-400">
              Manufacturer
              <input
                id="part-manufacturer"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Manufacturer"
                value={partManufacturer}
                onChange={(e) => onPartManufacturerChange(e.target.value)}
              />
            </label>
            <label htmlFor="part-manufacturer-number" className="block text-sm text-slate-400">
              Manufacturer part number
              <input
                id="part-manufacturer-number"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Manufacturer part #"
                value={partMfgNumber}
                onChange={(e) => onPartMfgNumberChange(e.target.value)}
              />
            </label>
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
            <StaticSearchPicker
              id="vendor-link-part"
              label="Part to link"
              value={selectedPartId}
              onChange={onSelectedPartIdChange}
              options={partOptions}
              selectedOption={selectedPartOption}
              placeholder="Select part"
              testId="vendor-link-part-picker"
            />
            <StaticSearchPicker
              id="vendor-link-vendor"
              label="Vendor party"
              value={selectedVendorId}
              onChange={onSelectedVendorIdChange}
              options={vendorOptions}
              selectedOption={selectedVendorOption}
              placeholder="Select vendor"
              testId="vendor-link-vendor-picker"
            />
            <label htmlFor="vendor-part-number" className="block text-sm text-slate-400">
              Vendor part number
              <input
                id="vendor-part-number"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Vendor part number"
                value={vendorPartNumber}
                onChange={(e) => onVendorPartNumberChange(e.target.value)}
              />
            </label>
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
