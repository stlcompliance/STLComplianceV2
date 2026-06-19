import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { AdvancedReferenceField, ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import {
  createTripPartsDemandLine,
  getTripLoads,
  getTripPartsDemand,
  getTrips,
  publishTripPartsDemand,
} from '../api/client'

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

export function TripPartsDemandPanel({ accessToken }: Props) {
  const queryClient = useQueryClient()
  const [tripId, setTripId] = useState('')
  const [lookupTripId, setLookupTripId] = useState('')
  const [partNumber, setPartNumber] = useState('')
  const [description, setDescription] = useState('')
  const [quantityRequested, setQuantityRequested] = useState('1')
  const [unitOfMeasure, setUnitOfMeasure] = useState('EA')
  const [notes, setNotes] = useState('')
  const [createPurchaseRequestDraft, setCreatePurchaseRequestDraft] = useState(true)

  const tripsQuery = useQuery({
    queryKey: ['routarr-trip-parts-demand-trips', accessToken],
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

  const loadsQuery = useQuery({
    queryKey: ['routarr-trip-load-visibility', accessToken, lookupTripId],
    queryFn: () => getTripLoads(accessToken, lookupTripId),
    enabled: Boolean(lookupTripId),
  })

  const demandQuery = useQuery({
    queryKey: ['routarr-trip-parts-demand', accessToken, lookupTripId],
    queryFn: () => getTripPartsDemand(accessToken, lookupTripId),
    enabled: Boolean(lookupTripId),
  })

  const createMutation = useMutation({
    mutationFn: () =>
      createTripPartsDemandLine(accessToken, lookupTripId, {
        partNumber: partNumber.trim() || null,
        description: description.trim() || null,
        quantityRequested: Number(quantityRequested),
        unitOfMeasure: unitOfMeasure.trim() || null,
        notes: notes.trim() || null,
      }),
    onSuccess: async () => {
      setPartNumber('')
      setDescription('')
      setQuantityRequested('1')
      setUnitOfMeasure('EA')
      setNotes('')
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-parts-demand', accessToken, lookupTripId] })
    },
  })

  const publishMutation = useMutation({
    mutationFn: () =>
      publishTripPartsDemand(accessToken, lookupTripId, {
        createPurchaseRequestDraft,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-parts-demand', accessToken, lookupTripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-load-visibility', accessToken, lookupTripId] })
    },
  })

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="trip-parts-demand-panel">
      <header>
        <h2 className="text-lg font-semibold text-slate-50">Load visibility</h2>
        <p className="mt-1 text-sm text-slate-400">
          Trip loads and RoutArr demand lines that can be published to SupplyArr as purchase-request context.
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
            testId="trip-parts-demand-trip-picker"
          />
          <AdvancedReferenceField
            value={tripId}
            onChange={setTripId}
            label="Trip id"
            testId="trip-parts-demand-trip-advanced"
          />
        </div>
        <button
          type="button"
          className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
          disabled={!tripId.trim()}
          onClick={() => setLookupTripId(tripId.trim())}
        >
          Load visibility
        </button>
      </div>

      {loadsQuery.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(loadsQuery.error, 'Failed to load trip loads.')}
          onRetry={() => void loadsQuery.refetch()}
          retryLabel="Retry loads"
        />
      ) : null}

      {demandQuery.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(demandQuery.error, 'Failed to load trip parts demand.')}
          onRetry={() => void demandQuery.refetch()}
          retryLabel="Retry parts demand"
        />
      ) : null}

      {createMutation.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(createMutation.error, 'Failed to create trip parts demand line.')}
        />
      ) : null}

      {publishMutation.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(publishMutation.error, 'Failed to publish trip parts demand.')}
        />
      ) : null}

      {lookupTripId ? (
        <div className="mt-5 grid gap-5 lg:grid-cols-2">
          <div className="rounded-lg border border-slate-700 bg-slate-950/50 p-4">
            <h3 className="text-sm font-semibold text-slate-200">Trip loads</h3>
            {loadsQuery.isLoading ? (
              <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading trip loads…</p>
            ) : (loadsQuery.data ?? []).length === 0 ? (
              <p className="mt-2 text-sm text-[var(--color-text-muted)]">No loads recorded for this trip.</p>
            ) : (
              <ul className="mt-3 space-y-2">
                {(loadsQuery.data ?? []).map((load) => (
                  <li key={load.loadId} className="rounded border border-slate-800 bg-slate-900/60 p-3 text-sm">
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <p className="font-medium text-slate-100">{load.loadKey}</p>
                        <p className="text-xs text-[var(--color-text-muted)]">{load.loadType} · {load.status}</p>
                      </div>
                      <span className="text-xs text-slate-400">#{load.sequenceNumber}</span>
                    </div>
                    <p className="mt-2 text-xs text-slate-400">
                      {load.originLabel} → {load.destinationLabel}
                    </p>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className="rounded-lg border border-slate-700 bg-slate-950/50 p-4">
            <h3 className="text-sm font-semibold text-slate-200">Parts demand</h3>
            <form
              className="mt-3 grid gap-2"
              onSubmit={(event) => {
                event.preventDefault()
                createMutation.mutate()
              }}
            >
              <input
                className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
                placeholder="Part number"
                value={partNumber}
                onChange={(event) => setPartNumber(event.target.value)}
              />
              <input
                className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
                placeholder="Description"
                value={description}
                onChange={(event) => setDescription(event.target.value)}
              />
              <div className="grid gap-2 sm:grid-cols-2">
                <input
                  className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
                  type="number"
                  min="0.01"
                  step="0.01"
                  placeholder="Quantity"
                  value={quantityRequested}
                  onChange={(event) => setQuantityRequested(event.target.value)}
                  required
                />
                <input
                  className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
                  placeholder="UOM"
                  value={unitOfMeasure}
                  onChange={(event) => setUnitOfMeasure(event.target.value)}
                />
              </div>
              <input
                className="rounded border border-slate-600 bg-slate-900 px-2 py-1 text-sm text-slate-200"
                placeholder="Notes"
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
              />
              <label className="flex items-center gap-2 text-xs text-slate-300">
                <input
                  type="checkbox"
                  checked={createPurchaseRequestDraft}
                  onChange={(event) => setCreatePurchaseRequestDraft(event.target.checked)}
                />
                Create purchase-request draft when published
              </label>
              <button
                type="submit"
                className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
                disabled={!lookupTripId || !Number(quantityRequested) || createMutation.isPending}
              >
                Add demand line
              </button>
            </form>

            {demandQuery.isLoading ? (
              <p className="mt-3 text-sm text-[var(--color-text-muted)]">Loading demand lines…</p>
            ) : (demandQuery.data ?? []).length === 0 ? (
              <p className="mt-3 text-sm text-[var(--color-text-muted)]">No pending demand lines yet.</p>
            ) : (
              <ul className="mt-3 space-y-2">
                {(demandQuery.data ?? []).map((line) => (
                  <li key={line.demandLineId} className="rounded border border-slate-800 bg-slate-900/60 p-3 text-xs text-slate-300">
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <p className="font-medium text-slate-100">
                          {line.lineNumber}. {line.partNumber}
                        </p>
                        <p className="text-[var(--color-text-muted)]">{line.description}</p>
                      </div>
                      <span className="rounded bg-slate-800 px-2 py-0.5 uppercase tracking-wide text-slate-300">
                        {line.status}
                      </span>
                    </div>
                    <p className="mt-2 text-slate-400">
                      {line.quantityRequested} {line.unitOfMeasure}
                      {line.quantityReceived > 0 ? ` · received ${line.quantityReceived}` : ''}
                    </p>
                    <p className="mt-1 text-[var(--color-text-muted)]">
                      Published {formatTimestamp(line.publishedAt)} · procurement {line.procurementStatus}
                    </p>
                  </li>
                ))}
              </ul>
            )}

            <button
              type="button"
              className="mt-4 rounded bg-emerald-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
              disabled={!lookupTripId || publishMutation.isPending}
              onClick={() => publishMutation.mutate()}
            >
              Publish demand to SupplyArr
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
