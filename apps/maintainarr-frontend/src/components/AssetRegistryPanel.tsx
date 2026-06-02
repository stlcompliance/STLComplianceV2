import { useEffect, useMemo, useState } from 'react'

import type {
  AssetClassResponse,
  AssetReadinessSummaryResponse,
  AssetResponse,
  AssetTypeResponse,
} from '../api/types'

interface AssetRegistryPanelProps {
  showSourceData?: boolean
  showAssetsTable?: boolean
  classes: AssetClassResponse[]
  types: AssetTypeResponse[]
  assets: AssetResponse[]
  readinessByAssetId: Record<string, AssetReadinessSummaryResponse>
  selectedAssetId: string | null
  onSelectAsset: (assetId: string) => void
  isLoading: boolean
  isReadinessLoading: boolean
}

function readinessLabel(status: AssetReadinessSummaryResponse['readinessStatus']): string {
  return status === 'ready' ? 'Ready' : 'Not ready'
}

export function AssetRegistryPanel({
  showSourceData = false,
  showAssetsTable = true,
  classes,
  types,
  assets,
  readinessByAssetId,
  selectedAssetId,
  onSelectAsset,
  isLoading,
  isReadinessLoading,
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
    return <p className="text-sm text-slate-400">Loading asset registry...</p>
  }

  return (
    <div className="grid gap-6 lg:grid-cols-2">
      {showSourceData ? (
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

      {showAssetsTable ? (
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
                        className={`cursor-pointer border-t border-slate-800 ${isSelected ? 'bg-amber-500/10' : ''}`}
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
                                ? 'Loading...'
                                : readiness
                                  ? `${readinessLabel(readiness.readinessStatus)}${readiness.blockerCount > 0 ? ` (${readiness.blockerCount})` : ''}`
                                  : 'No readiness data'
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
        </section>
      ) : null}
    </div>
  )
}
