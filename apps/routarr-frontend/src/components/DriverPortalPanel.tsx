import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'

import {
  completeDriverPortalTrip,
  closeDriverPortalTrip,
  createDriverPortalTripProof,
  dispatchDriverPortalTrip,
  getDriverPortalCaptureReadiness,
  getDriverPortalSchedule,
  getDriverPortalTripExecution,
  reportDriverPortalTripException,
  startDriverPortalTrip,
  submitDriverPortalTripDvir,
} from '../api/client'
import type {
  DispatchExceptionSummaryResponse,
  DriverPortalTripRow,
  TripDvirInspectionResponse,
  TripProofRecordResponse,
} from '../api/types'
import {
  enqueueDriverPortalOfflineAction,
  formatDriverPortalOfflineAction,
  isDriverPortalOfflineError,
  loadDriverPortalOfflineActions,
  removeDriverPortalOfflineAction,
  replayDriverPortalOfflineAction,
  type DriverPortalOfflineAction,
} from '../lib/driverPortalOfflineQueue'
import { TripCaptureAttachmentPanel } from './TripCaptureAttachmentPanel'
import { TripDvirSubmitForm } from './TripDvirSubmitForm'
import { DriverTimeTrackingPanel } from './DriverTimeTrackingPanel'

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

const REPORTABLE_EXCEPTION_STATUSES = new Set(['assigned', 'dispatched', 'in_progress'])

function DriverPortalExceptionSection({
  accessToken,
  trip,
  onReported,
  onOfflineQueueChanged,
}: {
  accessToken: string
  trip: DriverPortalTripRow
  onReported: () => void
  onOfflineQueueChanged: (message: string) => void
}) {
  const [open, setOpen] = useState(false)
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [exceptionType, setExceptionType] = useState('traffic_delay')
  const [reportError, setReportError] = useState<string | null>(null)
  const [reportedKey, setReportedKey] = useState<string | null>(null)
  const [offlineNotice, setOfflineNotice] = useState<string | null>(null)

  if (!REPORTABLE_EXCEPTION_STATUSES.has(trip.dispatchStatus)) {
    return null
  }

  const reportMutation = useMutation({
    mutationFn: async () => {
      const payload = {
        title,
        description,
        exceptionType,
      }
      try {
        if (typeof window !== 'undefined' && window.navigator.onLine === false) {
          const queued = enqueueDriverPortalOfflineAction({
            kind: 'exception',
            tripId: trip.tripId,
            tripNumber: trip.tripNumber,
            tripTitle: trip.title,
            payload,
          })
          onOfflineQueueChanged(
            `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
          )
          return { queued: true as const }
        }
        return await reportDriverPortalTripException(accessToken, trip.tripId, payload)
      } catch (err) {
        if (isDriverPortalOfflineError(err)) {
          const queued = enqueueDriverPortalOfflineAction({
            kind: 'exception',
            tripId: trip.tripId,
            tripNumber: trip.tripNumber,
            tripTitle: trip.title,
            payload,
          })
          onOfflineQueueChanged(
            `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
          )
          return { queued: true as const }
        }
        throw err
      }
    },
    onSuccess: (created) => {
      setReportError(null)
      if ('queued' in created && created.queued) {
        setOfflineNotice('Exception saved offline and will sync when you reconnect.')
        setReportedKey(null)
      } else {
        setOfflineNotice(null)
        const onlineCreated = created as DispatchExceptionSummaryResponse
        setReportedKey(onlineCreated.exceptionKey)
        onReported()
      }
      setTitle('')
      setDescription('')
      setOpen(false)
    },
    onError: (err: Error) => setReportError(err.message),
  })

  return (
    <div
      className="mt-3 border-t border-slate-700 pt-3"
      data-testid={`driver-portal-exception-${trip.tripId}`}
    >
      {offlineNotice ? (
        <p className="text-xs text-emerald-400">{offlineNotice}</p>
      ) : reportedKey ? (
        <p className="text-xs text-emerald-400">
          Exception {reportedKey} reported to dispatch.
        </p>
      ) : null}
      {!open ? (
        <button
          type="button"
          className="rounded border border-amber-700/60 bg-amber-950/40 px-2 py-1 text-xs text-amber-200"
          onClick={() => setOpen(true)}
        >
          Report exception
        </button>
      ) : (
        <div className="space-y-2">
          <p className="text-xs font-medium text-slate-300">Report exception</p>
          <select
            className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
            value={exceptionType}
            onChange={(e) => setExceptionType(e.target.value)}
            aria-label="Exception type"
          >
            <option value="traffic_delay">Traffic / delay</option>
            <option value="equipment_issue">Equipment issue</option>
            <option value="customer_access">Customer / stop access</option>
            <option value="route_issue">Route issue</option>
            <option value="other">Other</option>
          </select>
          <input
            type="text"
            className="w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
            placeholder="Short title (required)"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <textarea
            className="min-h-[72px] w-full rounded border border-slate-600 bg-slate-950 px-2 py-1 text-xs text-slate-100"
            placeholder="What happened?"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <div className="flex flex-wrap gap-1">
            <button
              type="button"
              className="rounded bg-amber-700 px-2 py-1 text-xs text-white disabled:opacity-50"
              disabled={reportMutation.isPending || !title.trim()}
              onClick={() => reportMutation.mutate()}
            >
              Submit to dispatch
            </button>
            <button
              type="button"
              className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-300"
              disabled={reportMutation.isPending}
              onClick={() => {
                setOpen(false)
                setReportError(null)
              }}
            >
              Cancel
            </button>
          </div>
          {reportError ? (
            <p className="text-xs text-red-400" role="alert">
              {reportError}
            </p>
          ) : null}
        </div>
      )}
    </div>
  )
}

function TripProofDvirSection({
  accessToken,
  trip,
  onUpdated,
  onOfflineQueueChanged,
}: {
  accessToken: string
  trip: DriverPortalTripRow
  onUpdated: () => void
  onOfflineQueueChanged: (message: string) => void
}) {
  const [proofType, setProofType] = useState<'pickup' | 'delivery'>('pickup')
  const [referenceKey, setReferenceKey] = useState('')
  const [notes, setNotes] = useState('')
  const [dvirError, setDvirError] = useState<string | null>(null)
  const [captureNotice, setCaptureNotice] = useState<string | null>(null)

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

  const proofMutation = useMutation<
    TripProofRecordResponse | { queued: true },
    Error,
    'pickup' | 'delivery'
  >({
    mutationFn: async (type: 'pickup' | 'delivery') => {
      const payload = {
        proofType: type,
        referenceKey: referenceKey || undefined,
        notes: notes || undefined,
        vehicleRefKey: trip.vehicleRefKey,
      }
      try {
        if (typeof window !== 'undefined' && window.navigator.onLine === false) {
          const queued = enqueueDriverPortalOfflineAction({
            kind: 'proof',
            tripId: trip.tripId,
            tripNumber: trip.tripNumber,
            tripTitle: trip.title,
            payload,
          })
          onOfflineQueueChanged(
            `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
          )
          return { queued: true as const }
        }
        return await createDriverPortalTripProof(accessToken, trip.tripId, payload)
      } catch (err) {
        if (isDriverPortalOfflineError(err)) {
          const queued = enqueueDriverPortalOfflineAction({
            kind: 'proof',
            tripId: trip.tripId,
            tripNumber: trip.tripNumber,
            tripTitle: trip.title,
            payload,
          })
          onOfflineQueueChanged(
            `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
          )
          return { queued: true as const }
        }
        throw err
      }
    },
    onSuccess: (result) => {
      setReferenceKey('')
      setNotes('')
      setCaptureNotice(
        'queued' in result && result.queued
          ? 'Pickup proof saved offline and will sync when you reconnect.'
          : null,
      )
      if (!('queued' in result && result.queued)) {
        onUpdated()
      }
    },
  })

  const dvirMutation = useMutation<
    TripDvirInspectionResponse | { queued: true },
    Error,
    {
      phase: 'pre_trip' | 'post_trip'
      result: string
      odometerReading?: number
      defectNotes?: string
      vehicleRefKey?: string
    }
  >({
    mutationFn: async (payload) => {
      const request = {
        phase: payload.phase,
        result: payload.result,
        vehicleRefKey: payload.vehicleRefKey ?? trip.vehicleRefKey ?? undefined,
        odometerReading: payload.odometerReading,
        defectNotes: payload.defectNotes,
      }
      try {
        if (typeof window !== 'undefined' && window.navigator.onLine === false) {
          const queued = enqueueDriverPortalOfflineAction({
            kind: 'dvir',
            tripId: trip.tripId,
            tripNumber: trip.tripNumber,
            tripTitle: trip.title,
            payload: request,
          })
          onOfflineQueueChanged(
            `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
          )
          return { queued: true as const }
        }
        return await submitDriverPortalTripDvir(accessToken, trip.tripId, request)
      } catch (err) {
        if (isDriverPortalOfflineError(err)) {
          const queued = enqueueDriverPortalOfflineAction({
            kind: 'dvir',
            tripId: trip.tripId,
            tripNumber: trip.tripNumber,
            tripTitle: trip.title,
            payload: request,
          })
          onOfflineQueueChanged(
            `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
          )
          return { queued: true as const }
        }
        throw err
      }
    },
    onSuccess: (result) => {
      setDvirError(null)
      setCaptureNotice(
        'queued' in result && result.queued
          ? 'DVIR saved offline and will sync when you reconnect.'
          : null,
      )
      if (!('queued' in result && result.queued)) {
        onUpdated()
      }
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

      {captureNotice ? <p className="mt-2 text-xs text-emerald-400">{captureNotice}</p> : null}

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
  onOfflineQueueChanged,
}: {
  trip: DriverPortalTripRow
  accessToken: string
  onAction: (tripId: string, action: 'dispatch' | 'start' | 'complete' | 'close') => void
  onProofDvirUpdated: () => void
  pendingTripId: string | null
  onOfflineQueueChanged: (message: string) => void
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

      <DriverPortalExceptionSection
        accessToken={accessToken}
        trip={trip}
        onReported={onProofDvirUpdated}
        onOfflineQueueChanged={onOfflineQueueChanged}
      />

      <TripProofDvirSection
        accessToken={accessToken}
        trip={trip}
        onUpdated={onProofDvirUpdated}
        onOfflineQueueChanged={onOfflineQueueChanged}
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
  onOfflineQueueChanged,
}: {
  title: string
  trips: DriverPortalTripRow[]
  accessToken: string
  emptyMessage: string
  onAction: (tripId: string, action: 'dispatch' | 'start' | 'complete' | 'close') => void
  onProofDvirUpdated: () => void
  pendingTripId: string | null
  onOfflineQueueChanged: (message: string) => void
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
              onOfflineQueueChanged={onOfflineQueueChanged}
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
  const [offlineActions, setOfflineActions] = useState<DriverPortalOfflineAction[]>(
    () => loadDriverPortalOfflineActions(),
  )
  const [offlineNotice, setOfflineNotice] = useState<string | null>(null)

  const scheduleQuery = useQuery({
    queryKey: ['driver-portal-schedule'],
    queryFn: () => getDriverPortalSchedule(accessToken),
  })

  const refreshOfflineActions = () => {
    setOfflineActions(loadDriverPortalOfflineActions())
  }

  const onOfflineQueueChanged = (message: string) => {
    refreshOfflineActions()
    setOfflineNotice(message)
  }

  const invalidateProofQueries = async () => {
    await queryClient.invalidateQueries({ queryKey: ['driver-portal-schedule'] })
    await queryClient.invalidateQueries({ queryKey: ['driver-portal-execution'] })
    await queryClient.invalidateQueries({ queryKey: ['driver-portal-capture-readiness'] })
  }

  const scheduleTrips = [...(scheduleQuery.data?.todayTrips ?? []), ...(scheduleQuery.data?.upcomingTrips ?? [])]

  const mutation = useMutation<unknown, Error, { tripId: string; action: 'dispatch' | 'start' | 'complete' | 'close' }>({
    mutationFn: async ({
      tripId,
      action,
    }: {
      tripId: string
      action: 'dispatch' | 'start' | 'complete' | 'close'
    }) => {
      setPendingTripId(tripId)
      setActionError(null)
      try {
        if (typeof window !== 'undefined' && window.navigator.onLine === false) {
          const trip = scheduleTrips.find((item) => item.tripId === tripId)
          if (trip) {
            const queued = enqueueDriverPortalOfflineAction({
              kind: 'trip',
              tripId: trip.tripId,
              tripNumber: trip.tripNumber,
              tripTitle: trip.title,
              action,
            })
            onOfflineQueueChanged(
              `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
            )
          }
          return { queued: true as const }
        }

        if (action === 'dispatch') return await dispatchDriverPortalTrip(accessToken, tripId)
        if (action === 'start') return await startDriverPortalTrip(accessToken, tripId)
        if (action === 'complete') return await completeDriverPortalTrip(accessToken, tripId)
        return await closeDriverPortalTrip(accessToken, tripId)
      } catch (err) {
        if (isDriverPortalOfflineError(err)) {
          const trip = scheduleTrips.find((item) => item.tripId === tripId)
          if (trip) {
            const queued = enqueueDriverPortalOfflineAction({
              kind: 'trip',
              tripId: trip.tripId,
              tripNumber: trip.tripNumber,
              tripTitle: trip.title,
              action,
            })
            onOfflineQueueChanged(
              `Saved ${formatDriverPortalOfflineAction(queued)} offline and queued it for sync.`,
            )
          }
          return { queued: true as const }
        }
        throw err
      }
    },
    onSuccess: async (result) => {
      if (typeof result === 'object' && result !== null && 'queued' in result) {
        return
      }
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
      <ApiErrorCallout
        message={getErrorMessage(scheduleQuery.error, 'Failed to load your driver schedule.')}
        onRetry={() => void scheduleQuery.refetch()}
        retryLabel="Retry schedule"
      />
    )
  }

  const schedule = scheduleQuery.data!
  const hasOfflineActions = offlineActions.length > 0

  const handleResyncOfflineActions = async () => {
    setActionError(null)
    setOfflineNotice(null)

    for (const action of [...offlineActions]) {
      try {
        await replayDriverPortalOfflineAction(accessToken, action)
        removeDriverPortalOfflineAction(action.id)
        refreshOfflineActions()
      } catch (err) {
        setActionError(
          getErrorMessage(err, 'One or more offline entries could not be resynced yet.'),
        )
        return
      }
    }

    await invalidateProofQueries()
    await queryClient.invalidateQueries({ queryKey: ['trips'] })
    await queryClient.invalidateQueries({ queryKey: ['dispatch-active-trips'] })
    setOfflineNotice('All pending offline entries were resynced.')
  }

  return (
    <div data-testid="driver-portal-panel">
      <header>
        <h2 className="text-lg font-semibold text-slate-100">Driver portal</h2>
        <p className="mt-1 text-sm text-slate-400">
          Today&apos;s assignments and upcoming trips for your person record. Execute dispatch,
          start, complete, and close only on trips assigned to you. Report operational exceptions to
          dispatch on active trips. Capture pickup/delivery proof and pre/post-trip DVIR; tenant policy
          may require DVIR before start.
        </p>
      </header>

      {actionError ? (
        <p className="mt-3 text-sm text-red-400" role="alert">
          {actionError}
        </p>
      ) : null}

      {offlineNotice ? (
        <p className="mt-3 text-sm text-emerald-400" role="status">
          {offlineNotice}
        </p>
      ) : null}

      <section className="mt-4 rounded border border-slate-700 bg-slate-950/50 p-3">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div>
            <h3 className="text-sm font-semibold text-slate-200">Offline queue</h3>
            <p className="text-xs text-slate-500">
              Capture form entries while offline, then resync them when connectivity returns.
            </p>
          </div>
          <button
            type="button"
            className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
            disabled={!hasOfflineActions}
            onClick={() => void handleResyncOfflineActions()}
          >
            Resync pending entries
          </button>
        </div>
        {hasOfflineActions ? (
          <ul className="mt-3 space-y-1 text-xs text-slate-400" data-testid="driver-portal-offline-queue">
            {offlineActions.map((action) => (
              <li key={action.id} className="rounded border border-slate-800 bg-slate-950/60 px-2 py-1">
                {formatDriverPortalOfflineAction(action)}
                <span className="ml-2 text-slate-600">{formatTimestamp(action.createdAt)}</span>
              </li>
            ))}
          </ul>
        ) : (
          <p className="mt-3 text-xs text-slate-500" data-testid="driver-portal-offline-empty">
            No pending offline entries.
          </p>
        )}
      </section>

      <div className="mt-4">
        <DriverTimeTrackingPanel accessToken={accessToken} />
      </div>

      <TripList
        title="Today"
        trips={schedule.todayTrips}
        accessToken={accessToken}
        emptyMessage="No trips scheduled or active for today."
        onAction={(tripId, action) => mutation.mutate({ tripId, action })}
        onProofDvirUpdated={() => void invalidateProofQueries()}
        pendingTripId={pendingTripId}
        onOfflineQueueChanged={onOfflineQueueChanged}
      />

      <TripList
        title="Upcoming"
        trips={schedule.upcomingTrips}
        accessToken={accessToken}
        emptyMessage="No upcoming assigned trips in the next week."
        onAction={(tripId, action) => mutation.mutate({ tripId, action })}
        onProofDvirUpdated={() => void invalidateProofQueries()}
        pendingTripId={pendingTripId}
        onOfflineQueueChanged={onOfflineQueueChanged}
      />
    </div>
  )
}
