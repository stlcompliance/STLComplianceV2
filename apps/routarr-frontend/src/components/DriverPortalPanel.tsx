import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'

import {
  completeDriverPortalTrip,
  closeDriverPortalTrip,
  createDriverPortalTripProof,
  dispatchDriverPortalTrip,
  getDriverPortalCaptureReadiness,
  getDriverPortalSchedule,
  getDriverPortalTripExecution,
  startDriverPortalTrip,
  submitDriverPortalTripDvir,
} from '../api/client'
import type { DriverPortalTripRow } from '../api/types'
import { TripCaptureAttachmentPanel } from './TripCaptureAttachmentPanel'
import { TripDvirSubmitForm } from './TripDvirSubmitForm'

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

function TripProofDvirSection({
  accessToken,
  trip,
  onUpdated,
}: {
  accessToken: string
  trip: DriverPortalTripRow
  onUpdated: () => void
}) {
  const [proofType, setProofType] = useState<'pickup' | 'delivery'>('pickup')
  const [referenceKey, setReferenceKey] = useState('')
  const [notes, setNotes] = useState('')
  const [dvirError, setDvirError] = useState<string | null>(null)

  const executionQuery = useQuery({
    queryKey: ['driver-portal-execution', trip.tripId],
    queryFn: () => getDriverPortalTripExecution(accessToken, trip.tripId),
    enabled:
      trip.dispatchStatus === 'in_progress'
      || trip.dispatchStatus === 'dispatched'
      || trip.proofCount > 0
      || trip.hasPreTripDvir
      || trip.hasPostTripDvir,
  })

  const readinessQuery = useQuery({
    queryKey: ['driver-portal-capture-readiness', trip.tripId],
    queryFn: () => getDriverPortalCaptureReadiness(accessToken, trip.tripId),
    enabled:
      trip.dispatchStatus === 'in_progress' || trip.dispatchStatus === 'dispatched',
  })

  const proofMutation = useMutation({
    mutationFn: (type: 'pickup' | 'delivery') =>
      createDriverPortalTripProof(accessToken, trip.tripId, {
        proofType: type,
        referenceKey: referenceKey || undefined,
        notes: notes || undefined,
        vehicleRefKey: trip.vehicleRefKey,
      }),
    onSuccess: () => {
      setReferenceKey('')
      setNotes('')
      onUpdated()
    },
  })

  const dvirMutation = useMutation({
    mutationFn: (payload: {
      phase: 'pre_trip' | 'post_trip'
      result: string
      odometerReading?: number
      defectNotes?: string
      vehicleRefKey?: string
    }) =>
      submitDriverPortalTripDvir(accessToken, trip.tripId, {
        phase: payload.phase,
        result: payload.result,
        vehicleRefKey: payload.vehicleRefKey ?? trip.vehicleRefKey ?? undefined,
        odometerReading: payload.odometerReading,
        defectNotes: payload.defectNotes,
      }),
    onSuccess: () => {
      setDvirError(null)
      onUpdated()
    },
    onError: (err: Error) => setDvirError(err.message),
  })

  const showCapture =
    trip.dispatchStatus === 'in_progress' || trip.dispatchStatus === 'dispatched'

  const blockedItems =
    readinessQuery.data?.items.filter((item) => item.required && !item.satisfied) ?? []

  return (
    <div
      className="mt-3 border-t border-slate-700 pt-3"
      data-testid={`driver-portal-proof-dvir-${trip.tripId}`}
    >
      <p className="text-xs text-slate-500">
        Proof {trip.proofCount}
        {trip.hasPreTripDvir ? ' · pre DVIR' : ''}
        {trip.hasPostTripDvir ? ' · post DVIR' : ''}
        {!trip.captureStartReady && trip.dispatchStatus === 'dispatched'
          ? ' · start blocked'
          : ''}
      </p>

      {readinessQuery.data && blockedItems.length > 0 ? (
        <ul className="mt-2 space-y-1" data-testid="capture-readiness-blockers">
          {blockedItems.map((item) => (
            <li key={item.key} className="text-xs text-amber-400">
              {item.message ?? item.label}
            </li>
          ))}
        </ul>
      ) : null}

      {executionQuery.data && executionQuery.data.proofs.length > 0 ? (
        <ul className="mt-2 space-y-1 text-xs text-slate-400">
          {executionQuery.data.proofs.slice(0, 5).map((p) => (
            <li key={p.proofId}>
              {p.proofType}
              {p.referenceKey ? ` · ${p.referenceKey}` : ''}
              {p.attachments.length > 0 ? ` · ${p.attachments.length} attachment(s)` : ''}
            </li>
          ))}
        </ul>
      ) : null}

      {executionQuery.data?.proofs.map((proof) => (
        <TripCaptureAttachmentPanel
          key={proof.proofId}
          accessToken={accessToken}
          tripId={trip.tripId}
          subjectType="proof"
          subjectId={proof.proofId}
          subjectLabel={`${proof.proofType} proof`}
          attachments={proof.attachments}
          onUploaded={onUpdated}
        />
      ))}

      {executionQuery.data?.dvirInspections.map((dvir) => (
        <TripCaptureAttachmentPanel
          key={dvir.dvirId}
          accessToken={accessToken}
          tripId={trip.tripId}
          subjectType="dvir"
          subjectId={dvir.dvirId}
          subjectLabel={`${dvir.phase.replace('_', ' ')} DVIR`}
          attachments={dvir.attachments}
          onUploaded={onUpdated}
        />
      ))}

      {showCapture ? (
        <div className="mt-2 space-y-3">
          <div className="flex flex-wrap gap-1">
            <button
              type="button"
              className="rounded bg-slate-800 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
              disabled={proofMutation.isPending}
              onClick={() => proofMutation.mutate('pickup')}
            >
              Quick pickup proof
            </button>
            <button
              type="button"
              className="rounded bg-slate-800 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
              disabled={proofMutation.isPending}
              onClick={() => proofMutation.mutate('delivery')}
            >
              Quick delivery proof
            </button>
          </div>

          <div className="flex flex-wrap gap-2">
            <select id="driverportal-select-field"
              className="rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
              value={proofType}
              onChange={(e) => setProofType(e.target.value as 'pickup' | 'delivery')}
            >
              <option value="pickup">Pickup proof</option>
              <option value="delivery">Delivery proof</option>
            </select>
            <input id="driverportal-input-field-2"
              type="text"
              className="min-w-[120px] flex-1 rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
              placeholder="Reference (BOL, POD…)"
              value={referenceKey}
              onChange={(e) => setReferenceKey(e.target.value)}
            />
          </div>
          <input id="driverportal-input-field"
            type="text"
            className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
            placeholder="Notes"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
          />
          <button
            type="button"
            className="rounded bg-indigo-700 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={proofMutation.isPending}
            onClick={() => proofMutation.mutate(proofType)}
          >
            Capture proof
          </button>

          {!trip.hasPreTripDvir ? (
            <TripDvirSubmitForm
              phase="pre_trip"
              label="Pre-trip DVIR"
              vehicleRefKey={trip.vehicleRefKey}
              disabled={dvirMutation.isPending}
              pending={dvirMutation.isPending}
              onSubmit={(payload) => dvirMutation.mutate(payload)}
            />
          ) : null}

          {!trip.hasPostTripDvir ? (
            <TripDvirSubmitForm
              phase="post_trip"
              label="Post-trip DVIR"
              vehicleRefKey={trip.vehicleRefKey}
              disabled={dvirMutation.isPending}
              pending={dvirMutation.isPending}
              onSubmit={(payload) => dvirMutation.mutate(payload)}
            />
          ) : null}

          {dvirError ? (
            <p className="text-xs text-red-400" role="alert">
              {dvirError}
            </p>
          ) : null}
        </div>
      ) : null}
    </div>
  )
}

function TripCard({
  trip,
  accessToken,
  onAction,
  onProofDvirUpdated,
  pendingTripId,
}: {
  trip: DriverPortalTripRow
  accessToken: string
  onAction: (tripId: string, action: 'dispatch' | 'start' | 'complete' | 'close') => void
  onProofDvirUpdated: () => void
  pendingTripId: string | null
}) {
  const busy = pendingTripId === trip.tripId
  const startTitle =
    trip.dispatchStatus === 'dispatched' && !trip.captureStartReady
      ? 'Complete capture requirements before starting'
      : undefined

  return (
    <li
      className="rounded-md border border-slate-700 bg-slate-950/50 p-3 text-sm"
      data-testid={`driver-portal-trip-${trip.tripId}`}
    >
      <p className="font-medium text-slate-100">{trip.title}</p>
      <p className="text-xs text-slate-500">
        {trip.tripNumber} · {trip.dispatchStatus.replace('_', ' ')}
        {trip.vehicleRefKey ? ` · ${trip.vehicleRefKey}` : ''}
      </p>
      <p className="mt-1 text-xs text-slate-400">
        Start {formatTimestamp(trip.scheduledStartAt)} · End {formatTimestamp(trip.scheduledEndAt)}
      </p>
      <div className="mt-2 flex flex-wrap gap-1">
        {trip.canDispatch ? (
          <button
            type="button"
            className="rounded bg-violet-700 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={busy}
            onClick={() => onAction(trip.tripId, 'dispatch')}
          >
            Dispatch
          </button>
        ) : null}
        {trip.dispatchStatus === 'dispatched' ? (
          <button
            type="button"
            className="rounded bg-emerald-700 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={busy || !trip.canStart}
            title={startTitle}
            onClick={() => onAction(trip.tripId, 'start')}
          >
            Start trip
          </button>
        ) : null}
        {trip.dispatchStatus === 'in_progress' ? (
          <button
            type="button"
            className="rounded bg-sky-700 px-2 py-1 text-xs text-white disabled:opacity-50"
            disabled={busy || !trip.canComplete}
            title={
              !trip.captureCompleteReady
                ? 'Complete capture requirements before completing the trip'
                : undefined
            }
            onClick={() => onAction(trip.tripId, 'complete')}
          >
            Complete
          </button>
        ) : null}
        {trip.canClose ? (
          <button
            type="button"
            className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
            disabled={busy}
            onClick={() => onAction(trip.tripId, 'close')}
          >
            Close
          </button>
        ) : null}
      </div>

      <TripProofDvirSection
        accessToken={accessToken}
        trip={trip}
        onUpdated={onProofDvirUpdated}
      />
    </li>
  )
}

function TripList({
  title,
  trips,
  accessToken,
  emptyMessage,
  onAction,
  onProofDvirUpdated,
  pendingTripId,
}: {
  title: string
  trips: DriverPortalTripRow[]
  accessToken: string
  emptyMessage: string
  onAction: (tripId: string, action: 'dispatch' | 'start' | 'complete' | 'close') => void
  onProofDvirUpdated: () => void
  pendingTripId: string | null
}) {
  return (
    <section className="mt-6" data-testid={`driver-portal-${title.toLowerCase().replace(/\s+/g, '-')}`}>
      <h3 className="text-sm font-semibold text-slate-200">{title}</h3>
      {trips.length === 0 ? (
        <p className="mt-2 text-xs text-slate-500">{emptyMessage}</p>
      ) : (
        <ul className="mt-2 space-y-2">
          {trips.map((trip) => (
            <TripCard
              key={trip.tripId}
              trip={trip}
              accessToken={accessToken}
              onAction={onAction}
              onProofDvirUpdated={onProofDvirUpdated}
              pendingTripId={pendingTripId}
            />
          ))}
        </ul>
      )}
    </section>
  )
}

export function DriverPortalPanel({ accessToken }: Props) {
  const queryClient = useQueryClient()
  const [pendingTripId, setPendingTripId] = useState<string | null>(null)
  const [actionError, setActionError] = useState<string | null>(null)

  const scheduleQuery = useQuery({
    queryKey: ['driver-portal-schedule'],
    queryFn: () => getDriverPortalSchedule(accessToken),
  })

  const invalidateProofQueries = async () => {
    await queryClient.invalidateQueries({ queryKey: ['driver-portal-schedule'] })
    await queryClient.invalidateQueries({ queryKey: ['driver-portal-execution'] })
    await queryClient.invalidateQueries({ queryKey: ['driver-portal-capture-readiness'] })
  }

  const mutation = useMutation({
    mutationFn: async ({
      tripId,
      action,
    }: {
      tripId: string
      action: 'dispatch' | 'start' | 'complete' | 'close'
    }) => {
      setPendingTripId(tripId)
      setActionError(null)
      if (action === 'dispatch') return dispatchDriverPortalTrip(accessToken, tripId)
      if (action === 'start') return startDriverPortalTrip(accessToken, tripId)
      if (action === 'complete') return completeDriverPortalTrip(accessToken, tripId)
      return closeDriverPortalTrip(accessToken, tripId)
    },
    onSuccess: async () => {
      await invalidateProofQueries()
      await queryClient.invalidateQueries({ queryKey: ['trips'] })
      await queryClient.invalidateQueries({ queryKey: ['dispatch-active-trips'] })
    },
    onError: (err: Error) => setActionError(err.message),
    onSettled: () => setPendingTripId(null),
  })

  if (scheduleQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading your schedule…</p>
  }

  if (scheduleQuery.isError) {
    return (
      <p className="text-sm text-red-400" role="alert">
        {(scheduleQuery.error as Error).message}
      </p>
    )
  }

  const schedule = scheduleQuery.data!

  return (
    <div data-testid="driver-portal-panel">
      <header>
        <h2 className="text-lg font-semibold text-slate-100">Driver portal</h2>
        <p className="mt-1 text-sm text-slate-400">
          Today&apos;s assignments and upcoming trips for your person record. Execute dispatch,
          start, complete, and close only on trips assigned to you. Capture pickup/delivery proof and
          pre/post-trip DVIR on active trips; tenant policy may require DVIR before start.
        </p>
      </header>

      {actionError ? (
        <p className="mt-3 text-sm text-red-400" role="alert">
          {actionError}
        </p>
      ) : null}

      <TripList
        title="Today"
        trips={schedule.todayTrips}
        accessToken={accessToken}
        emptyMessage="No trips scheduled or active for today."
        onAction={(tripId, action) => mutation.mutate({ tripId, action })}
        onProofDvirUpdated={() => void invalidateProofQueries()}
        pendingTripId={pendingTripId}
      />

      <TripList
        title="Upcoming"
        trips={schedule.upcomingTrips}
        accessToken={accessToken}
        emptyMessage="No upcoming assigned trips in the next week."
        onAction={(tripId, action) => mutation.mutate({ tripId, action })}
        onProofDvirUpdated={() => void invalidateProofQueries()}
        pendingTripId={pendingTripId}
      />
    </div>
  )
}
