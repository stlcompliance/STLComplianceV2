import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { AdvancedReferenceField, ApiErrorCallout, StaticSearchPicker, getErrorMessage, type PickerOption } from '@stl/shared-ui'

import { getTripCaptureReadiness, getTrips } from '../api/client'

type Props = {
  accessToken: string
}

export function TripCaptureReadinessPanel({ accessToken }: Props) {
  const [tripId, setTripId] = useState('')
  const [lookupTripId, setLookupTripId] = useState('')

  const tripsQuery = useQuery({
    queryKey: ['routarr-capture-readiness-trips', accessToken],
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

  const readinessQuery = useQuery({
    queryKey: ['routarr-trip-capture-readiness', accessToken, lookupTripId],
    queryFn: () => getTripCaptureReadiness(accessToken, lookupTripId),
    enabled: Boolean(lookupTripId),
  })

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/80 p-5" data-testid="trip-capture-readiness-panel">
      <h2 className="text-lg font-semibold text-slate-50">Validation blockers</h2>
      <p className="mt-1 text-sm text-slate-400">
        Trip capture readiness makes the start and complete gates visible so dispatch can spot what is blocking release.
      </p>

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
            testId="trip-capture-readiness-trip-picker"
          />
          <AdvancedReferenceField
            value={tripId}
            onChange={setTripId}
            label="Trip id"
            testId="trip-capture-readiness-trip-advanced"
          />
        </div>
        <button
          type="button"
          className="rounded bg-sky-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
          disabled={!tripId.trim()}
          onClick={() => setLookupTripId(tripId.trim())}
        >
          Load blockers
        </button>
      </div>

      {readinessQuery.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(readinessQuery.error, 'Failed to load capture readiness.')}
          onRetry={() => void readinessQuery.refetch()}
          retryLabel="Retry readiness"
        />
      ) : null}

      {readinessQuery.data ? (
        <div className="mt-4 space-y-3">
          <div className="flex flex-wrap gap-3 text-sm">
            <span className={readinessQuery.data.canStartTrip ? 'text-emerald-400' : 'text-amber-400'}>
              Start {readinessQuery.data.canStartTrip ? 'ready' : 'blocked'}
            </span>
            <span className={readinessQuery.data.canCompleteTrip ? 'text-emerald-400' : 'text-amber-400'}>
              Complete {readinessQuery.data.canCompleteTrip ? 'ready' : 'blocked'}
            </span>
          </div>
          <ul className="space-y-2">
            {readinessQuery.data.items.map((item) => (
              <li key={item.key} className="rounded border border-slate-800 bg-slate-950/50 p-3 text-sm">
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium text-slate-100">{item.label}</span>
                  <span className={item.satisfied ? 'text-emerald-400' : item.required ? 'text-amber-400' : 'text-[var(--color-text-muted)]'}>
                    {item.satisfied ? 'Satisfied' : item.required ? 'Required' : 'Optional'}
                  </span>
                </div>
                {item.message ? <p className="mt-1 text-xs text-slate-400">{item.message}</p> : null}
              </li>
            ))}
          </ul>
        </div>
      ) : null}
    </section>
  )
}
