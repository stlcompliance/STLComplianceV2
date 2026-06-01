import { useEffect, useMemo, useState } from 'react'

import type {
  AssetClassResponse,
  AssetReadinessSummaryResponse,
  AssetResponse,
  AssetTypeResponse,
  FieldMetadataResponse,
  FieldsetResponse,
} from '../api/types'

interface AssetRegistryPanelProps {
  mode: 'drawer' | 'details' | 'create'
  showSourceData?: boolean
  showAssetsTable?: boolean
  showAssetCreateForm?: boolean
  canManage: boolean
  classes: AssetClassResponse[]
  types: AssetTypeResponse[]
  assets: AssetResponse[]
  readinessByAssetId: Record<string, AssetReadinessSummaryResponse>
  selectedAssetId: string | null
  onSelectAsset: (assetId: string) => void
  isLoading: boolean
  isReadinessLoading: boolean
  className: string
  classDescription: string
  confirmedClassKey: string | null
  selectedClassId: string
  typeName: string
  typeDescription: string
  confirmedTypeKey: string | null
  selectedTypeId: string
  assetTag: string
  assetName: string
  assetDescription: string
  siteRef: string
  onClassNameChange: (value: string) => void
  onClassDescriptionChange: (value: string) => void
  onSelectedClassIdChange: (value: string) => void
  onTypeNameChange: (value: string) => void
  onTypeDescriptionChange: (value: string) => void
  onSelectedTypeIdChange: (value: string) => void
  onAssetTagChange: (value: string) => void
  onAssetNameChange: (value: string) => void
  onAssetDescriptionChange: (value: string) => void
  onSiteRefChange: (value: string) => void
  onCreateClass: () => void
  onCreateType: () => void
  onCreateAsset: () => void
  isCreatingClass: boolean
  isCreatingType: boolean
  isCreatingAsset: boolean
  assetFieldset?: FieldsetResponse | null
  assetFieldValues?: Record<string, unknown>
  onAssetFieldChange?: (fieldKey: string, value: unknown) => void
}

function readinessLabel(status: AssetReadinessSummaryResponse['readinessStatus']): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

function toStringValue(value: unknown): string {
  if (value == null) return ''
  if (Array.isArray(value)) return value.join(', ')
  return String(value)
}

function toStringArray(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.map((item) => String(item))
  }
  if (value == null) return []
  const text = String(value).trim()
  return text ? [text] : []
}

function fieldShouldRender(field: FieldMetadataResponse): boolean {
  const hidden = new Set(['description', 'notes', 'VIN', 'serialNumber', 'licensePlate', 'unitNumber', 'fleetNumber'])
  return !hidden.has(field.key)
}

function renderControl(
  field: FieldMetadataResponse,
  filteredOptions: Array<{ key: string; label: string }>,
  value: unknown,
  onChange: (nextValue: unknown) => void,
) {
  if (field.control === 'multiSelect') {
    const selected = new Set(toStringArray(value))
    return (
      <select
        multiple
        value={Array.from(selected)}
        onChange={(event) => {
          const items = Array.from(event.target.selectedOptions).map((option) => option.value)
          onChange(items)
        }}
        className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
      >
        {filteredOptions.map((option) => (
          <option key={option.key} value={option.key}>
            {option.label}
          </option>
        ))}
      </select>
    )
  }

  if (field.control === 'select' || field.control === 'searchableSelect' || field.control === 'asyncCombobox') {
    return (
      <select
        value={toStringValue(value)}
        onChange={(event) => onChange(event.target.value)}
        className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
      >
        <option value="">{field.required ? 'Select value' : 'Optional'}</option>
        {filteredOptions.map((option) => (
          <option key={option.key} value={option.key}>
            {option.label}
          </option>
        ))}
      </select>
    )
  }

  if (field.control === 'textArea') {
    return (
      <textarea
        value={toStringValue(value)}
        onChange={(event) => onChange(event.target.value)}
        rows={3}
        className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
      />
    )
  }

  return (
    <input
      value={toStringValue(value)}
      onChange={(event) => onChange(event.target.value)}
      className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
    />
  )
}

export function AssetRegistryPanel({
  mode,
  showSourceData,
  showAssetsTable = true,
  showAssetCreateForm,
  canManage,
  classes,
  types,
  assets,
  readinessByAssetId,
  selectedAssetId,
  onSelectAsset,
  isLoading,
  isReadinessLoading,
  className,
  classDescription,
  confirmedClassKey,
  selectedClassId,
  typeName,
  typeDescription,
  confirmedTypeKey,
  selectedTypeId,
  assetTag,
  assetName,
  assetDescription,
  siteRef,
  onClassNameChange,
  onClassDescriptionChange,
  onSelectedClassIdChange,
  onTypeNameChange,
  onTypeDescriptionChange,
  onSelectedTypeIdChange,
  onAssetTagChange,
  onAssetNameChange,
  onAssetDescriptionChange,
  onSiteRefChange,
  onCreateClass,
  onCreateType,
  onCreateAsset,
  isCreatingClass,
  isCreatingType,
  isCreatingAsset,
  assetFieldset,
  assetFieldValues,
  onAssetFieldChange,
}: AssetRegistryPanelProps) {
  type AssetColumnKey = 'tag' | 'name' | 'class' | 'type' | 'site' | 'status' | 'readiness'
  const STORAGE_KEY = 'maintainarr.assets.drawer.columns.v1'
  const allColumns: Array<{ key: AssetColumnKey; label: string }> = [
    { key: 'tag', label: 'Unit number' },
    { key: 'name', label: 'Asset name' },
    { key: 'class', label: 'Class' },
    { key: 'type', label: 'Type' },
    { key: 'site', label: 'Site' },
    { key: 'status', label: 'Lifecycle status' },
    { key: 'readiness', label: 'Readiness' },
  ]
  const [selectedColumns, setSelectedColumns] = useState<AssetColumnKey[]>(['tag', 'name', 'class', 'status', 'readiness'])

  useEffect(() => {
    try {
      const raw = window.localStorage.getItem(STORAGE_KEY)
      if (!raw) return
      const parsed = JSON.parse(raw) as AssetColumnKey[]
      const valid = parsed.filter((column) => allColumns.some((candidate) => candidate.key === column)).slice(0, 5)
      if (valid.length > 0) setSelectedColumns(valid)
    } catch {
      // Ignore malformed stored columns.
    }
  }, [])

  useEffect(() => {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(selectedColumns))
  }, [selectedColumns])

  const visibleColumns = useMemo(() => selectedColumns.slice(0, 5), [selectedColumns])
  const toggleColumn = (column: AssetColumnKey) => {
    setSelectedColumns((previous) => {
      if (previous.includes(column)) {
        const next = previous.filter((item) => item !== column)
        return next.length > 0 ? next : previous
      }
      if (previous.length >= 5) return previous
      return [...previous, column]
    })
  }

  if (isLoading) {
    return <p className="text-sm text-slate-400">Loading asset registry…</p>
  }

  void confirmedClassKey
  void confirmedTypeKey
  void selectedClassId
  void selectedTypeId
  void className
  void classDescription
  void typeName
  void typeDescription
  void siteRef
  void onClassNameChange
  void onClassDescriptionChange
  void onSelectedClassIdChange
  void onTypeNameChange
  void onTypeDescriptionChange
  void onSelectedTypeIdChange
  void onSiteRefChange
  void onCreateClass
  void onCreateType
  void isCreatingClass
  void isCreatingType

  const renderSourceData = showSourceData ?? mode === 'create'
  const renderAssetCreateForm = showAssetCreateForm ?? (mode === 'create' && showAssetsTable)
  const showAssetsSection = showAssetsTable

  const fieldsetFields = (assetFieldset?.fields ?? []).filter(fieldShouldRender)
  const fieldByCatalogKey = new Map<string, string>()
  for (const field of fieldsetFields) {
    if (field.catalogKey) {
      fieldByCatalogKey.set(field.catalogKey, field.key)
    }
  }

  const resolveFilteredOptions = (field: FieldMetadataResponse): Array<{ key: string; label: string }> => {
    const options = (field.options ?? []).map((option) => ({ key: option.key, label: option.label, dependency: option.dependency }))
    if (options.length === 0) return []
    if (!assetFieldValues || !onAssetFieldChange) {
      return options.map((option) => ({ key: option.key, label: option.label }))
    }
    return options
      .filter((option) => {
        const deps = option.dependency ?? {}
        return Object.entries(deps).every(([dependsOnCatalogKey, dependsOnOptionKey]) => {
          const parentFieldKey = fieldByCatalogKey.get(dependsOnCatalogKey) ?? dependsOnCatalogKey
          const parentValue = assetFieldValues[parentFieldKey]
          const parentValues = toStringArray(parentValue)
          return parentValues.includes(dependsOnOptionKey)
        })
      })
      .map((option) => ({ key: option.key, label: option.label }))
  }

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      {renderSourceData ? (
        <>
          <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
            <h2 className="text-lg font-medium text-white">Asset classes</h2>
            <ul className="mt-4 space-y-2 text-sm">
              {classes.length === 0 ? (
                <li className="text-slate-400">No asset classes yet.</li>
              ) : (
                classes.map((item) => (
                  <li key={item.assetClassId} className="rounded-lg border border-slate-800 p-3">
                    <div className="font-medium">{item.name}</div>
                  </li>
                ))
              )}
            </ul>
          </section>

          <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5">
            <h2 className="text-lg font-medium text-white">Asset types</h2>
            <ul className="mt-4 space-y-2 text-sm">
              {types.length === 0 ? (
                <li className="text-slate-400">No asset types yet.</li>
              ) : (
                types.map((item) => (
                  <li key={item.assetTypeId} className="rounded-lg border border-slate-800 p-3">
                    <div className="font-medium">{item.name}</div>
                    <div className="text-slate-400">{item.className}</div>
                  </li>
                ))
              )}
            </ul>
          </section>
        </>
      ) : null}

      {showAssetsSection ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5 lg:col-span-2" data-testid="asset-registry-panel">
          <h2 className="text-lg font-medium text-white">Assets</h2>
          <div className="mt-4 rounded-md border border-slate-700 p-2">
            <p className="text-xs text-slate-400">Visible columns (max 5)</p>
            <div className="mt-2 flex flex-wrap gap-3">
              {allColumns.map((column) => (
                <label key={column.key} className="inline-flex items-center gap-2 text-xs text-slate-300">
                  <input
                    type="checkbox"
                    checked={visibleColumns.includes(column.key)}
                    onChange={() => toggleColumn(column.key)}
                  />
                  {column.label}
                </label>
              ))}
            </div>
          </div>
          <div className="mt-3 overflow-x-auto rounded-md border border-slate-700">
            <table className="min-w-full text-left text-sm" data-testid="asset-registry-list">
              <thead className="bg-slate-950/70">
                <tr>
                  {visibleColumns.map((column) => (
                    <th key={column} className="px-3 py-2 text-xs font-medium uppercase tracking-wide text-slate-400">
                      {allColumns.find((item) => item.key === column)?.label}
                    </th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {assets.length === 0 ? (
                  <tr>
                    <td colSpan={visibleColumns.length} className="px-3 py-4 text-slate-400">No assets registered yet.</td>
                  </tr>
                ) : (
                  assets.map((item) => {
                    const readiness = readinessByAssetId[item.assetId]
                    const isSelected = selectedAssetId === item.assetId
                    return (
                      <tr
                        key={item.assetId}
                        data-testid={`asset-registry-row-${item.assetId}`}
                        className={`border-t border-slate-800 cursor-pointer ${isSelected ? 'bg-amber-500/10' : ''}`}
                        onClick={() => onSelectAsset(item.assetId)}
                      >
                        {visibleColumns.map((column) => (
                          <td key={`${item.assetId}-${column}`} className="px-3 py-2 text-slate-200">
                            {column === 'tag' ? item.assetTag : null}
                            {column === 'name' ? item.name : null}
                            {column === 'class' ? item.className : null}
                            {column === 'type' ? item.typeName : null}
                            {column === 'site' ? item.siteRef ?? 'Unassigned' : null}
                            {column === 'status' ? item.lifecycleStatus : null}
                            {column === 'readiness'
                              ? isReadinessLoading
                                ? 'Loading…'
                                : readiness
                                  ? `${readinessLabel(readiness.readinessStatus)}${readiness.blockerCount > 0 ? ` (${readiness.blockerCount})` : ''}`
                                  : '—'
                              : null}
                          </td>
                        ))}
                      </tr>
                    )
                  })
                )}
              </tbody>
            </table>
          </div>

          {renderAssetCreateForm && canManage ? (
            <div className="mt-4 space-y-4">
              <div className="grid gap-2 md:grid-cols-2">
                <input
                  className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                  placeholder="Unit number or identifier"
                  value={assetTag}
                  onChange={(event) => onAssetTagChange(event.target.value)}
                />
                <input
                  className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                  placeholder="Display name (optional)"
                  value={assetName}
                  onChange={(event) => onAssetNameChange(event.target.value)}
                />
                <input
                  className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm md:col-span-2"
                  placeholder="Description"
                  value={assetDescription}
                  onChange={(event) => onAssetDescriptionChange(event.target.value)}
                />
              </div>

              <div className="grid gap-3 md:grid-cols-2">
                {fieldsetFields.map((field) => {
                  if (field.key === 'description' || field.key === 'notes') {
                    return null
                  }
                  const filteredOptions = resolveFilteredOptions(field)
                  const value = assetFieldValues?.[field.key]
                  return (
                    <div key={field.key} className={field.control === 'multiSelect' ? 'md:col-span-2' : ''}>
                      <label className="mb-1 block text-xs font-medium uppercase tracking-wide text-slate-400">
                        {field.label}
                        {field.required ? ' *' : ''}
                      </label>
                      {renderControl(
                        field,
                        filteredOptions,
                        value,
                        (nextValue) => onAssetFieldChange?.(field.key, nextValue),
                      )}
                    </div>
                  )
                })}
              </div>

              <button
                type="button"
                className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                disabled={isCreatingAsset}
                onClick={onCreateAsset}
              >
                {isCreatingAsset ? 'Creating…' : 'Create asset'}
              </button>
            </div>
          ) : null}
        </section>
      ) : null}
    </div>
  )
}
