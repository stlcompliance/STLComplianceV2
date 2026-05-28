import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'

import { getTripExecutionSummary } from '../api/client'

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
        <label className="flex flex-col gap-1 text-xs text-slate-400">
          Trip ID
          <input
            type="text"
            className="min-w-[280px] rounded border border-slate-600 bg-slate-950 px-2 py-1 text-sm text-slate-100"
            value={tripId}
            onChange={(e) => setTripId(e.target.value)}
            placeholder="Paste trip GUID"
          />
        </label>
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
        <p className="mt-4 text-sm text-red-400" role="alert">
          {(summaryQuery.error as Error).message}
        </p>
      ) : null}

      {summaryQuery.data ? (
        <div className="mt-4 space-y-4 text-sm">
          <p className="text-slate-300">
            {summaryQuery.data.tripNumber} · driver{' '}
            {summaryQuery.data.assignedDriverPersonId ?? 'unassigned'} · pre DVIR{' '}
            {summaryQuery.data.hasPreTripDvir ? 'yes' : 'no'} · post DVIR{' '}
            {summaryQuery.data.hasPostTripDvir ? 'yes' : 'no'}
          </p>

          <div>
            <h3 className="font-medium text-slate-200">Proof records ({summaryQuery.data.proofs.length})</h3>
            {summaryQuery.data.proofs.length === 0 ? (
              <p className="mt-1 text-xs text-slate-500">No proof captured.</p>
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
                    <p className="text-slate-500">{formatTimestamp(proof.capturedAt)}</p>
                    {proof.notes ? <p className="text-slate-400">{proof.notes}</p> : null}
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
              <p className="mt-1 text-xs text-slate-500">No DVIR submitted.</p>
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
                    <p className="text-slate-500">{formatTimestamp(dvir.submittedAt)}</p>
                    {dvir.defectNotes ? <p className="text-slate-400">{dvir.defectNotes}</p> : null}
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
