import { useMutation, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import {
  exportPartsInventoryReportSummaryCsv,
  getPartsInventoryLocationDetail,
  getPartsInventoryPartDetail,
  getPartsInventoryReportSummary,
} from '../api/client'

interface PartsInventoryReportsPanelProps {
  accessToken: string
  canRead: boolean
  canExport: boolean
}

export function PartsInventoryReportsPanel({
  accessToken,
  canRead,
  canExport,
}: PartsInventoryReportsPanelProps) {
  const [activePartsOnly, setActivePartsOnly] = useState(true)
  const [belowReorderOnly, setBelowReorderOnly] = useState(false)
  const [locationFilter, setLocationFilter] = useState('')
  const [selectedPartId, setSelectedPartId] = useState<string | null>(null)
  const [selectedLocationId, setSelectedLocationId] = useState<string | null>(null)

  const summaryQuery = useQuery({
    queryKey: [
      'supplyarr-parts-inventory-report-summary',
      accessToken,
      activePartsOnly,
      belowReorderOnly,
      locationFilter,
    ],
    queryFn: () =>
      getPartsInventoryReportSummary(accessToken, {
        activePartsOnly: activePartsOnly || undefined,
        belowReorderOnly: belowReorderOnly || undefined,
        inventoryLocationId: locationFilter || undefined,
      }),
    enabled: canRead,
  })

  const partDetailQuery = useQuery({
    queryKey: ['supplyarr-parts-inventory-part-detail', accessToken, selectedPartId],
    queryFn: () => getPartsInventoryPartDetail(accessToken, selectedPartId!),
    enabled: canRead && Boolean(selectedPartId),
  })

  const locationDetailQuery = useQuery({
    queryKey: ['supplyarr-parts-inventory-location-detail', accessToken, selectedLocationId],
    queryFn: () => getPartsInventoryLocationDetail(accessToken, selectedLocationId!),
    enabled: canRead && Boolean(selectedLocationId),
  })

  const locationOptions = (summaryQuery.data?.locations ?? []).map<PickerOption>((loc) => ({
    value: loc.inventoryLocationId,
    label: `${loc.locationKey} · ${loc.name}`,
  }))
  const selectedLocationOption = locationOptions.find((loc) => loc.value === locationFilter)

  const exportMutation = useMutation({
    mutationFn: () =>
      exportPartsInventoryReportSummaryCsv(accessToken, {
        activePartsOnly: activePartsOnly || undefined,
        belowReorderOnly: belowReorderOnly || undefined,
        inventoryLocationId: locationFilter || undefined,
      }),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = `supplyarr-parts-inventory-${new Date().toISOString().slice(0, 10)}.csv`
      anchor.click()
      URL.revokeObjectURL(url)
    },
  })

  if (!canRead) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="parts-inventory-reports-panel"
    >
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-slate-50">Parts and inventory reports</h2>
          <p className="mt-1 text-sm text-slate-400">
            Stock positions, reorder signals, and location rollups from SupplyArr truth.
          </p>
        </div>
        {canExport ? (
          <button
            type="button"
            className="rounded-md bg-sky-700 px-3 py-1.5 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
            disabled={exportMutation.isPending}
            onClick={() => exportMutation.mutate()}
          >
            {exportMutation.isPending ? 'Exporting…' : 'Export parts CSV'}
          </button>
        ) : null}
      </div>

      <div className="mt-4 flex flex-wrap gap-3 text-sm">
        <label htmlFor="parts-inventory-active-only" className="flex items-center gap-2 text-slate-300">
          <input
            id="parts-inventory-active-only"
            type="checkbox"
            checked={activePartsOnly}
            onChange={(event) => setActivePartsOnly(event.target.checked)}
          />
          Active parts only
        </label>
        <label htmlFor="parts-inventory-below-reorder" className="flex items-center gap-2 text-slate-300">
          <input
            id="parts-inventory-below-reorder"
            type="checkbox"
            checked={belowReorderOnly}
            onChange={(event) => setBelowReorderOnly(event.target.checked)}
          />
          Below reorder point only
        </label>
        <label htmlFor="parts-inventory-location-filter" className="flex items-center gap-2 text-slate-300">
          Inventory location filter
          <StaticSearchPicker
            id="parts-inventory-location-filter"
            value={locationFilter}
            options={locationOptions}
            selectedOption={selectedLocationOption}
            onChange={(value) => {
              setLocationFilter(value)
              setSelectedLocationId(value || null)
              setSelectedPartId(null)
            }}
            placeholder="All locations"
            testId="parts-inventory-location-filter"
          />
        </label>
      </div>

      {summaryQuery.isLoading && (
        <p className="mt-3 text-sm text-slate-500">Loading parts and inventory summary…</p>
      )}

      {summaryQuery.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="Parts and inventory report unavailable"
            message={getErrorMessage(summaryQuery.error, 'Failed to load parts and inventory report.')}
            retryLabel="Retry summary"
            onRetry={() => {
              void summaryQuery.refetch()
            }}
          />
        </div>
      )}

      {exportMutation.isError && (
        <div className="mt-3">
          <ApiErrorCallout
            title="CSV export failed"
            message={getErrorMessage(exportMutation.error, 'Unable to export parts and inventory CSV.')}
          />
        </div>
      )}

      {summaryQuery.data && (
        <>
          <div className="mt-4 flex flex-wrap gap-2 text-xs">
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              Parts: {summaryQuery.data.totals.activePartCount} active /{' '}
              {summaryQuery.data.totals.totalPartCount} total
            </span>
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              On hand: {summaryQuery.data.totals.totalQuantityOnHand}
            </span>
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-300">
              Available: {summaryQuery.data.totals.totalQuantityAvailable}
            </span>
            <span className="rounded-md bg-amber-950 px-2 py-1 text-amber-200">
              Below reorder: {summaryQuery.data.totals.belowReorderPointCount}
            </span>
            <span className="rounded-md bg-slate-800 px-2 py-1 text-slate-400">
              Locations: {summaryQuery.data.totals.locationCount} · Bins:{' '}
              {summaryQuery.data.totals.binCount}
            </span>
          </div>

          {summaryQuery.data.parts.length === 0 ? (
            <p className="mt-4 text-sm text-slate-500">No parts match the current filters.</p>
          ) : (
            <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
              {summaryQuery.data.parts.map((part) => (
                <li key={part.partId} className="px-3 py-3">
                  <button
                    type="button"
                    className="w-full text-left"
                    onClick={() => {
                      setSelectedPartId(part.partId)
                      setSelectedLocationId(null)
                    }}
                  >
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">
                          {part.partKey} · {part.displayName}
                        </div>
                        <div className="text-xs text-slate-500">
                          {part.status} · {part.categoryKey || 'uncategorized'}
                        </div>
                      </div>
                      {part.belowReorderPoint ? (
                        <span className="rounded bg-amber-950 px-2 py-0.5 text-xs text-amber-200">
                          below reorder
                        </span>
                      ) : null}
                    </div>
                    <p className="mt-2 text-xs text-slate-400">
                      On hand {part.quantityOnHand} · Reserved {part.quantityReserved} · Available{' '}
                      {part.quantityAvailable} · {part.vendorLinkCount} vendor links
                    </p>
                  </button>
                </li>
              ))}
            </ul>
          )}

          {summaryQuery.data.locations.length > 0 && !locationFilter && (
            <div className="mt-6">
              <h3 className="text-sm font-semibold text-slate-200">By location</h3>
              <ul className="mt-2 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
                {summaryQuery.data.locations.map((loc) => (
                  <li key={loc.inventoryLocationId} className="px-3 py-2">
                    <button
                      type="button"
                      className="w-full text-left text-slate-300 hover:text-slate-100"
                      onClick={() => {
                        setSelectedLocationId(loc.inventoryLocationId)
                        setSelectedPartId(null)
                        setLocationFilter(loc.inventoryLocationId)
                      }}
                    >
                      {loc.locationKey} · {loc.name} — {loc.partCountWithStock} parts, available{' '}
                      {loc.quantityAvailable}
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </>
      )}

      {selectedPartId && partDetailQuery.data && (
        <div className="mt-6 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <h3 className="text-sm font-semibold text-slate-100">
            Part detail · {partDetailQuery.data.summary.displayName}
          </h3>
          {partDetailQuery.data.stockByBin.length > 0 && (
            <ul className="mt-3 space-y-1 text-sm text-slate-300">
              {partDetailQuery.data.stockByBin.map((row) => (
                <li key={row.partStockLevelId}>
                  {row.locationKey}/{row.binKey}: available {row.quantityAvailable} (on hand{' '}
                  {row.quantityOnHand})
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {selectedLocationId && locationDetailQuery.data && (
        <div className="mt-6 rounded-lg border border-slate-800 bg-slate-950/60 p-4">
          <h3 className="text-sm font-semibold text-slate-100">
            Location detail · {locationDetailQuery.data.summary.name}
          </h3>
          <p className="mt-1 text-xs text-slate-500">
            {locationDetailQuery.data.summary.binCount} bins · available{' '}
            {locationDetailQuery.data.summary.quantityAvailable}
          </p>
          {locationDetailQuery.data.parts.length > 0 && (
            <ul className="mt-3 space-y-1 text-sm text-slate-300">
              {locationDetailQuery.data.parts.map((row) => (
                <li key={row.partId}>
                  {row.partKey}: available {row.quantityAvailable}
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </section>
  )
}
