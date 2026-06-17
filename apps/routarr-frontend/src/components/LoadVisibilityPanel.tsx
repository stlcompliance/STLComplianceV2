import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { AdvancedReferenceField, ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import { getLoadVisibility, getTrips, listTransportationDemands } from '../api/client'

type Props = {
  accessToken: string
}

function formatTimestamp(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

export function LoadVisibilityPanel({ accessToken }: Props) {
  const [tripId, setTripId] = useState('')
  const [lookupTripId, setLookupTripId] = useState('')

  const tripsQuery = useQuery({
    queryKey: ['routarr-load-visibility-trips', accessToken],
    queryFn: () => getTrips(accessToken),
  })

  const tripOptions = useMemo(
    () =>
      (tripsQuery.data ?? []).map((trip) => ({
        value: trip.tripId,
        label: `${trip.tripNumber} · ${trip.title}`,
      })),
    [tripsQuery.data],
  )

  const selectedTripOption = useMemo<PickerOption | undefined>(
    () => tripOptions.find((option) => option.value === tripId),
    [tripId, tripOptions],
  )

  const visibilityQuery = useQuery({
    queryKey: ['routarr-load-visibility', accessToken, lookupTripId],
    queryFn: () => getLoadVisibility(accessToken, lookupTripId || undefined),
  })

  const transportationDemandsQuery = useQuery({
    queryKey: ['routarr-load-visibility-transportation-demands', accessToken, lookupTripId],
    queryFn: () => listTransportationDemands(accessToken, { tripId: lookupTripId }),
    enabled: Boolean(lookupTripId),
  })
  const hasFilter = Boolean(lookupTripId.trim())

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="load-visibility-panel">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Load visibility</h2>
        <p className="mt-1 text-sm text-slate-400">
          RoutArr transportation load snapshots with source, status, and document context.
        </p>
      </header>

      <div className="mt-4 flex flex-wrap items-end gap-2">
        <div className="min-w-[280px] flex-1">
          <StaticSearchPicker
            label="Trip"
            value={tripId}
            onChange={setTripId}
            options={tripOptions}
            selectedOption={selectedTripOption}
            placeholder="Search trips…"
            disabled={tripsQuery.isLoading}
            testId="load-visibility-trip-picker"
          />
          <AdvancedReferenceField
            value={tripId}
            onChange={setTripId}
            label="Trip id"
            testId="load-visibility-trip-advanced"
          />
        </div>
        <button
          type="button"
          className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
          disabled={visibilityQuery.isFetching && lookupTripId === tripId.trim()}
          onClick={() => setLookupTripId(tripId.trim())}
        >
          Refresh visibility
        </button>
      </div>

      {visibilityQuery.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(visibilityQuery.error, 'Failed to load load visibility.')}
          onRetry={() => void visibilityQuery.refetch()}
          retryLabel="Retry visibility"
        />
      ) : null}

      {hasFilter ? (
        <section className="mt-4 rounded border border-slate-700 bg-slate-950/60 p-4">
          <h3 className="text-sm font-semibold text-slate-100">Transportation demand refs</h3>
          {transportationDemandsQuery.isLoading ? (
            <p className="mt-2 text-sm text-slate-500">Loading demand refs...</p>
          ) : transportationDemandsQuery.isError ? (
            <ApiErrorCallout
              className="mt-3"
              message={getErrorMessage(transportationDemandsQuery.error, 'Failed to load transportation demand refs.')}
              onRetry={() => void transportationDemandsQuery.refetch()}
              retryLabel="Retry demand refs"
            />
          ) : (transportationDemandsQuery.data ?? []).length === 0 ? (
            <p className="mt-2 text-sm text-slate-500">No transportation demand refs found for this trip.</p>
          ) : (
            <div className="mt-3 grid gap-2 sm:grid-cols-2">
              {(transportationDemandsQuery.data ?? []).map((demand) => {
                const blockers = [
                  demand.planningStatus !== 'planned' && demand.status !== 'planned' ? 'planning' : null,
                  demand.tenderStatus === 'tender_required' ? 'tender' : null,
                  demand.freshnessState !== 'current' ? 'freshness' : null,
                ].filter(Boolean)
                return (
                  <a
                    key={demand.transportationDemandId}
                    className="rounded border border-slate-700 bg-slate-900 px-3 py-2 text-sm text-slate-200 hover:border-sky-600"
                    href={`/transportation-demands?demand=${encodeURIComponent(demand.transportationDemandId)}`}
                  >
                    <span className="block font-semibold">{demand.demandNumber}</span>
                    <span className="mt-1 block text-xs text-slate-500">
                      {demand.sourceProduct} · {demand.freshnessState} · {demand.status}
                    </span>
                    <span className="mt-1 block text-xs text-slate-400">
                      {blockers.length > 0 ? `Blockers: ${blockers.join(', ')}` : 'Ready'}
                    </span>
                  </a>
                )
              })}
            </div>
          )}
        </section>
      ) : null}

      <div className="mt-5">
        <p className="text-xs uppercase tracking-wide text-slate-500">
          {hasFilter ? `Filtered by trip ${lookupTripId}` : 'Showing all visible loads'}
        </p>
        {visibilityQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-500">Loading load visibility…</p>
        ) : (visibilityQuery.data ?? []).length === 0 ? (
          <p className="mt-2 text-sm text-slate-500">No load visibility records found for this filter.</p>
        ) : (
          <ul className="mt-3 space-y-3">
            {(visibilityQuery.data ?? []).map((load) => (
              <li key={load.transportationLoadVisibilityId} className="rounded-lg border border-slate-700 bg-slate-950/50 p-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="text-sm font-semibold text-slate-100">{load.loadNumber}</p>
                    <p className="text-xs text-slate-500">
                      {load.sourceProduct} · {load.loadType} · {load.status}
                    </p>
                  </div>
                  <span className="rounded bg-slate-800 px-2 py-0.5 text-xs uppercase tracking-wide text-slate-300">
                    {load.sourceObjectRef ?? 'manual'}
                  </span>
                </div>
                <p className="mt-2 text-sm text-slate-300">{load.itemSummarySnapshot}</p>
                <p className="mt-1 text-xs text-slate-500">
                  {load.originLocationRef ?? 'Unknown origin'} → {load.destinationLocationRef ?? 'Unknown destination'}
                </p>
                <p className="mt-1 text-xs text-slate-500">
                  Created {formatTimestamp(load.createdAt)} · Updated {formatTimestamp(load.updatedAt)}
                </p>
                <p className="mt-2 text-xs text-slate-400">
                  Documents {load.documentRefs.length} · Orders {load.orderRefs.length} · Receipts {load.expectedReceiptRefs.length}
                  {load.hazmatFlag ? ' · hazmat' : ''}
                  {load.temperatureRequirement ? ` · temp ${load.temperatureRequirement}` : ''}
                </p>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  )
}
