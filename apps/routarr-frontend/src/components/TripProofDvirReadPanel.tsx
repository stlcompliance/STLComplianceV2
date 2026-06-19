import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { AdvancedReferenceField, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import { getTripExecutionSummary, getTrips, downloadTripCaptureAttachment } from '../api/client'

type Props = {
  accessToken: string
}

function formatTimestamp(iso: string) {
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

export function TripProofDvirReadPanel({ accessToken }: Props) {
  const [tripId, setTripId] = useState('')
  const [lookupId, setLookupId] = useState<string | null>(null)

  const tripsQuery = useQuery({
    queryKey: ['routarr-trips-proof-dvir', accessToken],
    queryFn: () => getTrips(accessToken),
    enabled: Boolean(accessToken),
  })

  const tripOptions = useMemo(
    () =>
      (tripsQuery.data ?? []).map((trip) => ({
        value: trip.tripId,
        label: `${trip.tripNumber} · ${trip.title}`,
      })),
    [tripsQuery.data],
  )

  const selectedTripOption = useMemo((): PickerOption | undefined => {
    const trip = (tripsQuery.data ?? []).find((item) => item.tripId === tripId)
    return trip
      ? {
          value: trip.tripId,
          label: `${trip.tripNumber} · ${trip.title}`,
        }
      : undefined
  }, [tripId, tripsQuery.data])

  const summaryQuery = useQuery({
    queryKey: ['trip-execution-summary', lookupId],
    queryFn: () => getTripExecutionSummary(accessToken, lookupId!),
    enabled: Boolean(lookupId),
  })

  return (
    <section data-testid="trip-proof-dvir-read-panel" className="rounded-lg border border-slate-700 p-4">
      <header>
        <h2 className="text-lg font-semibold text-slate-100">Trip proof &amp; DVIR</h2>
        <p className="mt-1 text-sm text-slate-400">
          Dispatcher read-only view of pickup/delivery proof and pre/post-trip DVIR for a trip.
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
            testId="trip-proof-dvir-trip-picker"
          />
          <AdvancedReferenceField
            value={tripId}
            onChange={setTripId}
            label="Trip id"
            testId="trip-proof-dvir-trip-advanced"
          />
        </div>
        <button
          type="button"
          className="rounded bg-slate-700 px-3 py-1.5 text-sm text-white disabled:opacity-50"
          disabled={!tripId.trim()}
          onClick={() => setLookupId(tripId.trim())}
        >
          Load execution
        </button>
      </div>

      {summaryQuery.isLoading ? (
        <p className="mt-4 text-sm text-slate-400">Loading execution summary…</p>
      ) : null}

      {summaryQuery.isError ? (
        <ApiErrorCallout
          className="mt-4"
          message={getErrorMessage(summaryQuery.error, 'Failed to load execution summary.')}
          onRetry={() => void summaryQuery.refetch()}
          retryLabel="Retry execution summary"
        />
      ) : null}

      {summaryQuery.data ? (
        <div className="mt-4 space-y-4 text-sm">
          <p className="text-slate-300" data-testid="trip-execution-summary-header">
            {summaryQuery.data.tripNumber} · driver{' '}
            {summaryQuery.data.assignedDriverPersonId ?? 'unassigned'} · status{' '}
            {summaryQuery.data.dispatchStatus.replace('_', ' ')} · driver closed{' '}
            {summaryQuery.data.closedAt ? 'yes' : 'no'} · pre DVIR{' '}
            {summaryQuery.data.hasPreTripDvir ? 'yes' : 'no'} · post DVIR{' '}
            {summaryQuery.data.hasPostTripDvir ? 'yes' : 'no'}
          </p>

          <div>
            <h3 className="font-medium text-slate-200">Proof records ({summaryQuery.data.proofs.length})</h3>
            {summaryQuery.data.proofs.length === 0 ? (
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">No proof captured.</p>
            ) : (
              <ul className="mt-2 space-y-2">
                {summaryQuery.data.proofs.map((proof) => (
                  <li
                    key={proof.proofId}
                    className="rounded border border-slate-700 bg-slate-950/50 p-2 text-xs"
                    data-testid={`proof-row-${proof.proofId}`}
                  >
                    <span className="font-medium text-slate-200">{proof.proofType}</span>
                    {proof.referenceKey ? ` · ${proof.referenceKey}` : ''}
                    <p className="text-[var(--color-text-muted)]">{formatTimestamp(proof.capturedAt)}</p>
                    {proof.notes ? <p className="text-slate-400">{proof.notes}</p> : null}
                    {proof.attachments.length > 0 ? (
                      <ul className="mt-1 space-y-1">
                        {proof.attachments.map((attachment) => (
                          <li key={attachment.attachmentId}>
                            <button
                              type="button"
                              className="text-sky-400 underline"
                              data-testid={`proof-attachment-${attachment.attachmentId}`}
                              onClick={() =>
                                void downloadTripCaptureAttachment(
                                  accessToken,
                                  summaryQuery.data!.tripId,
                                  'proof',
                                  proof.proofId,
                                  attachment.attachmentId,
                                  attachment.fileName,
                                )
                              }
                            >
                              {attachment.attachmentKind}: {attachment.fileName}
                            </button>
                          </li>
                        ))}
                      </ul>
                    ) : null}
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div>
            <h3 className="font-medium text-slate-200">
              DVIR inspections ({summaryQuery.data.dvirInspections.length})
            </h3>
            {summaryQuery.data.dvirInspections.length === 0 ? (
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">No DVIR submitted.</p>
            ) : (
              <ul className="mt-2 space-y-2">
                {summaryQuery.data.dvirInspections.map((dvir) => (
                  <li
                    key={dvir.dvirId}
                    className="rounded border border-slate-700 bg-slate-950/50 p-2 text-xs"
                    data-testid={`dvir-row-${dvir.dvirId}`}
                  >
                    <span className="font-medium text-slate-200">
                      {dvir.phase.replace('_', ' ')} · {dvir.result}
                    </span>
                    {dvir.vehicleRefKey ? ` · ${dvir.vehicleRefKey}` : ''}
                    {dvir.odometerReading != null ? ` · odo ${dvir.odometerReading}` : ''}
                    <p className="text-[var(--color-text-muted)]">{formatTimestamp(dvir.submittedAt)}</p>
                    {dvir.defectNotes ? <p className="text-slate-400">{dvir.defectNotes}</p> : null}
                    {dvir.attachments.length > 0 ? (
                      <ul className="mt-1 space-y-1">
                        {dvir.attachments.map((attachment) => (
                          <li key={attachment.attachmentId}>
                            <button
                              type="button"
                              className="text-sky-400 underline"
                              data-testid={`dvir-attachment-${attachment.attachmentId}`}
                              onClick={() =>
                                void downloadTripCaptureAttachment(
                                  accessToken,
                                  summaryQuery.data!.tripId,
                                  'dvir',
                                  dvir.dvirId,
                                  attachment.attachmentId,
                                  attachment.fileName,
                                )
                              }
                            >
                              {attachment.attachmentKind}: {attachment.fileName}
                            </button>
                          </li>
                        ))}
                      </ul>
                    ) : null}
                  </li>
                ))}
              </ul>
            )}
          </div>
        </div>
      ) : null}
    </section>
  )
}
