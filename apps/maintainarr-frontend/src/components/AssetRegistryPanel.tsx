import { ControlledSelect } from '@stl/shared-ui'
import { useEffect, useMemo, useState } from 'react'

import type {
  AssetClassResponse,
  AssetReadinessSummaryResponse,
  AssetResponse,
  AssetTypeResponse,
} from '../api/types'

interface AssetRegistryPanelProps {
  mode: 'drawer' | 'details' | 'create'
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
}

function readinessLabel(status: AssetReadinessSummaryResponse['readinessStatus']): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

export function AssetRegistryPanel({
  mode,
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
}: AssetRegistryPanelProps) {
  type AssetColumnKey = 'tag' | 'name' | 'class' | 'type' | 'site' | 'status' | 'readiness'
  const STORAGE_KEY = 'maintainarr.assets.drawer.columns.v1'
  const allColumns: Array<{ key: AssetColumnKey; label: string }> = [
    { key: 'tag', label: 'Asset tag' },
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

  const classOptions = classes.map((item) => ({
    value: item.assetClassId,
    label: item.name,
  }))
  const typeOptions = types.map((item) => ({
    value: item.assetTypeId,
    label: `${item.className} / ${item.name}`,
  }))
  void confirmedClassKey
  void confirmedTypeKey

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      {mode === 'create' ? (
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
            {canManage ? (
              <div className="mt-4 space-y-2">
                <input id="assetregistry-input-field-8"
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                  placeholder="Class name"
                  value={className}
                  onChange={(event) => onClassNameChange(event.target.value)}
                />
                <input id="assetregistry-input-field-7"
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                  placeholder="Description"
                  value={classDescription}
                  onChange={(event) => onClassDescriptionChange(event.target.value)}
                />
                <button
                  type="button"
                  className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                  disabled={isCreatingClass}
                  onClick={onCreateClass}
                >
                  {isCreatingClass ? 'Creating…' : 'Create class'}
                </button>
              </div>
            ) : null}
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
                    <div className="text-slate-400">
                      {item.className}
                    </div>
                  </li>
                ))
              )}
            </ul>
            {canManage ? (
              <div className="mt-4 space-y-2">
                <ControlledSelect
                  label="Asset class"
                  value={selectedClassId}
                  onChange={onSelectedClassIdChange}
                  options={classOptions}
                  emptyLabel="Select asset class"
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                />
                <input id="assetregistry-input-field-6"
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                  placeholder="Type name"
                  value={typeName}
                  onChange={(event) => onTypeNameChange(event.target.value)}
                />
                <input id="assetregistry-input-field-5"
                  className="w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
                  placeholder="Description"
                  value={typeDescription}
                  onChange={(event) => onTypeDescriptionChange(event.target.value)}
                />
                <button
                  type="button"
                  className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                  disabled={isCreatingType || !selectedClassId}
                  onClick={onCreateType}
                >
                  {isCreatingType ? 'Creating…' : 'Create type'}
                </button>
              </div>
            ) : null}
          </section>
        </>
      ) : null}

      <section
        className={`rounded-xl border border-slate-700 bg-slate-900/60 p-5 ${mode === 'create' ? 'lg:col-span-2' : 'lg:col-span-2'}`}
        data-testid="asset-registry-panel"
      >
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
        {mode === 'create' && canManage ? (
          <div className="mt-4 grid gap-2 md:grid-cols-2">
            <ControlledSelect
              label="Asset type"
              value={selectedTypeId}
              onChange={onSelectedTypeIdChange}
              options={typeOptions}
              emptyLabel="Select asset type"
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm md:col-span-2"
            />
            <input id="assetregistry-input-field-4"
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Asset tag"
              value={assetTag}
              onChange={(event) => onAssetTagChange(event.target.value)}
            />
            <input id="assetregistry-input-field-3"
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm"
              placeholder="Asset name"
              value={assetName}
              onChange={(event) => onAssetNameChange(event.target.value)}
            />
            <input id="assetregistry-input-field-2"
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm md:col-span-2"
              placeholder="Description"
              value={assetDescription}
              onChange={(event) => onAssetDescriptionChange(event.target.value)}
            />
            <input id="assetregistry-input-field"
              className="rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-sm md:col-span-2"
              placeholder="Site reference (optional)"
              value={siteRef}
              onChange={(event) => onSiteRefChange(event.target.value)}
            />
            <button
              type="button"
              className="rounded-lg bg-amber-600 px-4 py-2 text-sm font-medium text-white disabled:opacity-50 md:col-span-2"
              disabled={isCreatingAsset || !selectedTypeId}
              onClick={onCreateAsset}
            >
              {isCreatingAsset ? 'Creating…' : 'Create asset'}
            </button>
          </div>
        ) : null}
      </section>
    </div>
  )
}
