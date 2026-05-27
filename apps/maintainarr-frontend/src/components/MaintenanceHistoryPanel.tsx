import type { AssetResponse, MaintenanceHistoryEntryResponse } from '../api/types'

interface MaintenanceHistoryPanelProps {
  assets: AssetResponse[]
  entries: MaintenanceHistoryEntryResponse[]
  totalCount: number
  selectedAssetId: string
  isLoading: boolean
  onSelectedAssetIdChange: (assetId: string) => void
}

function categoryLabel(category: MaintenanceHistoryEntryResponse['category']): string {
  switch (category) {
    case 'inspection':
      return 'Inspection'
    case 'defect':
      return 'Defect'
    case 'work_order':
      return 'Work order'
    case 'pm':
      return 'PM'
    default:
      return category
  }
}

function eventTypeLabel(eventType: string): string {
  switch (eventType) {
    case 'inspection_started':
      return 'Inspection started'
    case 'inspection_completed':
      return 'Inspection completed'
    case 'defect_reported':
      return 'Defect reported'
    case 'defect_resolved':
      return 'Defect resolved'
    case 'work_order_created':
      return 'Work order created'
    case 'work_order_started':
      return 'Work order started'
    case 'work_order_completed':
      return 'Work order completed'
    case 'work_order_cancelled':
      return 'Work order cancelled'
    case 'pm_schedule_created':
      return 'PM schedule created'
    case 'pm_completed':
      return 'PM completed'
    case 'pm_marked_due':
      return 'PM marked due'
    case 'pm_marked_overdue':
      return 'PM marked overdue'
    default:
      return eventType.replaceAll('_', ' ')
  }
}

export function MaintenanceHistoryPanel({
  assets,
  entries,
  totalCount,
  selectedAssetId,
  isLoading,
  onSelectedAssetIdChange,
}: MaintenanceHistoryPanelProps) {
  const selectedAsset = assets.find((asset) => asset.assetId === selectedAssetId)

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <h2 className="text-sm font-medium text-slate-300">Maintenance history</h2>
      <p className="mt-2 text-xs text-slate-500">
        Aggregated timeline from inspections, defects, work orders, and PM events for a selected asset.
      </p>

      <div className="mt-4">
        <label className="block text-xs text-slate-400" htmlFor="history-asset-select">
          Asset
        </label>
        <select
          id="history-asset-select"
          className="mt-1 w-full max-w-md rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
          value={selectedAssetId}
          onChange={(event) => onSelectedAssetIdChange(event.target.value)}
        >
          <option value="">Select an asset…</option>
          {assets.map((asset) => (
            <option key={asset.assetId} value={asset.assetId}>
              {asset.assetTag} — {asset.name}
            </option>
          ))}
        </select>
      </div>

      {!selectedAssetId ? (
        <p className="mt-4 text-sm text-slate-400">Choose an asset to view its maintenance history.</p>
      ) : isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading maintenance history…</p>
      ) : entries.length === 0 ? (
        <p className="mt-4 text-sm text-slate-400">
          No maintenance events recorded yet for {selectedAsset?.name ?? 'this asset'}.
        </p>
      ) : (
        <>
          <p className="mt-3 text-xs text-slate-500">
            {totalCount} event{totalCount === 1 ? '' : 's'} total for {selectedAsset?.assetTag ?? 'asset'}
          </p>
          <ul className="mt-3 divide-y divide-slate-700 text-sm">
            {entries.map((entry) => (
              <li key={entry.entryId} className="py-3">
                <div className="flex flex-wrap items-baseline gap-2">
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs text-slate-300">
                    {categoryLabel(entry.category)}
                  </span>
                  <p className="text-white">{entry.title}</p>
                </div>
                <p className="mt-1 text-xs text-slate-400">{eventTypeLabel(entry.eventType)}</p>
                {entry.detail ? <p className="mt-1 text-xs text-slate-500">{entry.detail}</p> : null}
                {entry.relatedEntityId ? (
                  <p className="mt-1 text-xs text-slate-500">Related: {entry.relatedEntityId}</p>
                ) : null}
                <p className="mt-1 text-xs text-slate-500">{new Date(entry.occurredAt).toLocaleString()}</p>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  )
}
