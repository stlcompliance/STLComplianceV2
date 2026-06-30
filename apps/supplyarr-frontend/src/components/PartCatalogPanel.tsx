import { ControlledSelect, StaticSearchPicker, normalizeUom, type PickerOption } from '@stl/shared-ui'
import { useMemo } from 'react'

import type { PartCatalogResponse, PartResponse } from '../api/types'
import {
  distinctCategoryOptions,
  toCatalogPickerOptions,
  toPartPickerOptions,
  toSupplierUnitPickerOptions,
  type SupplierUnitPickerSource,
  UOM_OPTIONS,
} from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'
import {
  formatSupplierIdentitySummary,
  formatSupplierOperationalContext,
  humanizeSupplierUnitKind,
  resolveSupplierId,
} from '../utils/supplierPresentation'

const PART_SOURCE_TYPE_OPTIONS = [
  { value: 'unknown', label: 'Unknown / legacy' },
  { value: 'internal_fabrication', label: 'Internal fabrication' },
  { value: 'rebuilt', label: 'Rebuilt / reman' },
  { value: 'salvage', label: 'Salvage' },
  { value: 'customer_supplied', label: 'Customer supplied' },
  { value: 'transfer', label: 'Transfer' },
  { value: 'kit_assembly', label: 'Kit assembly' },
  { value: 'manufacturer', label: 'Manufacturer direct' },
  { value: 'vendor', label: 'Supplier source' },
]

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
  partIsTrackable: boolean
  partIsStocked: boolean
  selectedCatalogId: string
  selectedSourcePartId: string
  partSourceType: string
  partSourceLabel: string
  partSourceNotes: string
  supplierPartNumber: string
  selectedPartId: string
  selectedSupplierUnitId: string
  suppliers: SupplierUnitPickerSource[]
  onCatalogKeyChange: (value: string) => void
  onCatalogNameChange: (value: string) => void
  onCatalogDescriptionChange: (value: string) => void
  onPartKeyChange: (value: string) => void
  onPartNameChange: (value: string) => void
  onPartCategoryChange: (value: string) => void
  onPartUomChange: (value: string) => void
  onPartManufacturerChange: (value: string) => void
  onPartMfgNumberChange: (value: string) => void
  onPartIsTrackableChange: (value: boolean) => void
  onPartIsStockedChange: (value: boolean) => void
  onSelectedCatalogIdChange: (value: string) => void
  onSelectedSourcePartIdChange: (value: string) => void
  onPartSourceTypeChange: (value: string) => void
  onPartSourceLabelChange: (value: string) => void
  onPartSourceNotesChange: (value: string) => void
  onSupplierPartNumberChange: (value: string) => void
  onSelectedPartIdChange: (value: string) => void
  onSelectedSupplierUnitIdChange: (value: string) => void
  onCreateCatalog: () => void
  onCreatePart: () => void
  onCreatePartSource: () => void
  onLinkSupplierSource: () => void
  isCreatingCatalog: boolean
  isCreatingPart: boolean
  isCreatingPartSource: boolean
  isLinkingSupplierSource: boolean
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
  partIsTrackable,
  partIsStocked,
  selectedCatalogId,
  selectedSourcePartId,
  partSourceType,
  partSourceLabel,
  partSourceNotes,
  supplierPartNumber,
  selectedPartId,
  selectedSupplierUnitId,
  suppliers,
  onCatalogKeyChange,
  onCatalogNameChange,
  onCatalogDescriptionChange,
  onPartKeyChange,
  onPartNameChange,
  onPartCategoryChange,
  onPartUomChange,
  onPartManufacturerChange,
  onPartMfgNumberChange,
  onPartIsTrackableChange,
  onPartIsStockedChange,
  onSelectedCatalogIdChange,
  onSelectedSourcePartIdChange,
  onPartSourceTypeChange,
  onPartSourceLabelChange,
  onPartSourceNotesChange,
  onSupplierPartNumberChange,
  onSelectedPartIdChange,
  onSelectedSupplierUnitIdChange,
  onCreateCatalog,
  onCreatePart,
  onCreatePartSource,
  onLinkSupplierSource,
  isCreatingCatalog,
  isCreatingPart,
  isCreatingPartSource,
  isLinkingSupplierSource,
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
  const selectedSourcePartOption = useMemo<PickerOption | undefined>(
    () => partOptions.find((option) => option.value === selectedSourcePartId),
    [partOptions, selectedSourcePartId],
  )
  const selectedPartOption = useMemo<PickerOption | undefined>(
    () => partOptions.find((option) => option.value === selectedPartId),
    [partOptions, selectedPartId],
  )
  const supplierOptions = useMemo<PickerOption[]>(
    () => toSupplierUnitPickerOptions(suppliers),
    [suppliers],
  )
  const selectedSupplierUnitOption = useMemo<PickerOption | undefined>(
    () => supplierOptions.find((option) => option.value === selectedSupplierUnitId),
    [selectedSupplierUnitId, supplierOptions],
  )
  const selectedSupplierUnit = useMemo(
    () => suppliers.find((supplier) => resolveSupplierId(supplier) === selectedSupplierUnitId),
    [selectedSupplierUnitId, suppliers],
  )

  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading part master…</p>
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2">
      <h2 className="text-lg font-medium text-white">Part master</h2>
      <p className="mt-1 text-sm text-slate-400">
        Canonical tenant-owned parts with optional operational sources and optional supplier sourcing overlays.
      </p>

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
                      <div className="mt-2 flex flex-wrap gap-2 text-xs text-slate-300">
                        <span className="rounded-full border border-slate-700 px-2 py-0.5">
                          {part.isTrackable ?? true ? 'Trackable' : 'Not trackable'}
                        </span>
                        <span className="rounded-full border border-slate-700 px-2 py-0.5">
                          {part.isStocked ?? true ? 'Stocked' : 'Non-stock'}
                        </span>
                      </div>
                    </div>
                    <span className={`rounded-full px-2 py-0.5 text-xs ring-1 ${statusBadgeClass(part.status)}`}>
                      {part.status}
                    </span>
                  </div>
                  {part.sources && part.sources.length > 0 ? (
                    <div className="mt-2 flex flex-wrap gap-2 text-xs text-slate-300">
                      {part.sources.map((source) => (
                        <span key={source.sourceId} className="rounded-full border border-sky-500/30 bg-sky-500/10 px-2 py-0.5">
                          {source.label}
                        </span>
                      ))}
                    </div>
                  ) : null}
                  {part.vendorLinks.length > 0 ? (
                    <div className="mt-3 space-y-2">
                      <p className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                        Supplier sources
                      </p>
                      <ul className="space-y-2">
                        {part.vendorLinks.map((link) => (
                          <li
                            key={link.linkId}
                            className={`rounded-md border px-3 py-2 text-sm ${
                              link.isPreferred
                                ? 'border-sky-500/40 bg-sky-950/20'
                                : 'border-slate-800 bg-slate-950/40'
                            }`}
                          >
                            <div className="flex flex-wrap items-start justify-between gap-2">
                              <div>
                                <div className="font-medium text-slate-100">
                                  {formatSupplierIdentitySummary({
                                    supplierDisplayName: link.supplierDisplayName,
                                    supplierKey: link.supplierKey,
                                    parentSupplierDisplayName: link.parentSupplierDisplayName,
                                    supplierUnitKind: link.supplierUnitKind,
                                  })}
                                </div>
                                <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                                  {humanizeSupplierUnitKind(link.supplierUnitKind)} · {link.vendorPartNumber}
                                </div>
                              </div>
                              {link.isPreferred ? (
                                <span className="rounded-full border border-sky-500/40 bg-sky-500/10 px-2 py-0.5 text-[11px] font-semibold uppercase tracking-wide text-sky-200">
                                  Preferred
                                </span>
                              ) : null}
                            </div>
                            <p className="mt-2 text-xs text-slate-400">
                              {formatSupplierOperationalContext(link)}
                            </p>
                          </li>
                        ))}
                      </ul>
                    </div>
                  ) : null}
                  {(!part.sources || part.sources.length === 0) && part.vendorLinks.length === 0 ? (
                    <p className="mt-2 text-slate-400">No sources configured.</p>
                  ) : null}
                </li>
              ))
            )}
          </ul>
        </div>
      </div>

      {canManage ? (
        <div className="mt-6 grid gap-6 border-t border-slate-800 pt-6 lg:grid-cols-2 2xl:grid-cols-4">
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
            <h3 className="text-sm font-medium text-slate-300">Create part</h3>
            <p className="text-sm text-slate-400">
              Create the canonical part first. Source and supplier information can be added later.
            </p>
            <StaticSearchPicker
              id="part-catalog-select"
              label="Catalog (optional)"
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
            <label className="flex items-center gap-2 rounded-lg border border-slate-800 bg-slate-950/70 px-3 py-2 text-sm text-slate-200">
              <input
                type="checkbox"
                checked={partIsTrackable}
                onChange={(e) => onPartIsTrackableChange(e.target.checked)}
              />
              Trackable item
            </label>
            <label className="flex items-center gap-2 rounded-lg border border-slate-800 bg-slate-950/70 px-3 py-2 text-sm text-slate-200">
              <input
                type="checkbox"
                checked={partIsStocked}
                onChange={(e) => onPartIsStockedChange(e.target.checked)}
              />
              Stocked item
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
            <h3 className="text-sm font-medium text-slate-300">Add operational source</h3>
            <StaticSearchPicker
              id="part-source-part"
              label="Part"
              value={selectedSourcePartId}
              onChange={onSelectedSourcePartIdChange}
              options={partOptions}
              selectedOption={selectedSourcePartOption}
              placeholder="Select part"
              testId="part-source-part-picker"
            />
            <ControlledSelect
              id="part-source-type"
              label="Source type"
              value={partSourceType}
              onChange={onPartSourceTypeChange}
              options={PART_SOURCE_TYPE_OPTIONS}
              emptyLabel="Select source type…"
            />
            <label htmlFor="part-source-label" className="block text-sm text-slate-400">
              Source label
              <input
                id="part-source-label"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Retired truck 104, Fabrication shop, Legacy stock..."
                value={partSourceLabel}
                onChange={(e) => onPartSourceLabelChange(e.target.value)}
              />
            </label>
            <label htmlFor="part-source-notes" className="block text-sm text-slate-400">
              Notes
              <textarea
                id="part-source-notes"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Optional context for how this part is created, rebuilt, salvaged, or transferred."
                value={partSourceNotes}
                onChange={(e) => onPartSourceNotesChange(e.target.value)}
                rows={3}
              />
            </label>
            <button
              type="button"
              className="rounded-lg bg-slate-700 px-4 py-2 text-sm font-medium text-white hover:bg-slate-600 disabled:opacity-50"
              disabled={isCreatingPartSource || !selectedSourcePartId || !partSourceLabel.trim()}
              onClick={onCreatePartSource}
            >
              {isCreatingPartSource ? 'Adding…' : 'Add source'}
            </button>
          </div>

          <div className="space-y-2">
            <h3 className="text-sm font-medium text-slate-300">Add supplier source</h3>
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
              id="vendor-link-supplier-unit"
              label="Supplier identity or sub-unit"
              value={selectedSupplierUnitId}
              onChange={onSelectedSupplierUnitIdChange}
              options={supplierOptions}
              selectedOption={selectedSupplierUnitOption}
              placeholder="Select supplier identity or sub-unit"
              testId="vendor-link-supplier-unit-picker"
            />
            {selectedSupplierUnit ? (
              <p className="text-xs text-[var(--color-text-muted)]">
                {formatSupplierIdentitySummary({
                  supplierDisplayName: selectedSupplierUnit.displayName,
                  supplierKey: selectedSupplierUnit.supplierKey,
                  parentSupplierDisplayName: selectedSupplierUnit.parentSupplierDisplayName,
                  supplierUnitKind: selectedSupplierUnit.unitKind,
                })}{' '}
                · {humanizeSupplierUnitKind(selectedSupplierUnit.unitKind)}
              </p>
            ) : null}
            <label htmlFor="vendor-part-number" className="block text-sm text-slate-400">
              Supplier part number
              <input
                id="vendor-part-number"
                className="mt-1 w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100"
                placeholder="Supplier part number"
                value={supplierPartNumber}
                onChange={(e) => onSupplierPartNumberChange(e.target.value)}
              />
            </label>
            <button
              type="button"
              className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={
                isLinkingSupplierSource || !selectedPartId || !selectedSupplierUnitId || !supplierPartNumber.trim()
              }
              onClick={onLinkSupplierSource}
            >
              {isLinkingSupplierSource ? 'Linking…' : 'Add supplier source'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
