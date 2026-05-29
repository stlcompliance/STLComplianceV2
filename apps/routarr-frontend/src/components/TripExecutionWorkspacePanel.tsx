import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Check, ExternalLink, X } from 'lucide-react'
import { Link } from 'react-router-dom'
import { useState } from 'react'

import {
  downloadTripCaptureAttachment,
  getDispatchReportTripDetail,
  getRoute,
  getRoutes,
  getTrip,
  getTripAuditTrail,
  getTripCaptureReadiness,
  getTripExecutionSummary,
  submitTripDvir,
  updateTripStatus,
} from '../api/client'
import type { RouteDetailResponse, TripDetailResponse } from '../api/types'
import { TripCaptureAttachmentPanel } from './TripCaptureAttachmentPanel'
import { TripDvirSubmitForm } from './TripDvirSubmitForm'

type Props = {
  accessToken: string
  tripId: string
  canPerform: boolean
  canManage: boolean
}

function formatTimestamp(iso: string | null | undefined) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function statusOptionsFor(currentStatus: string, canManage: boolean): string[] {
  if (currentStatus === 'planned') {
    return canManage ? ['planned', 'cancelled'] : ['planned']
  }
  if (currentStatus === 'assigned') {
    const options = ['assigned', 'dispatched']
    if (canManage) options.push('cancelled')
    return options
  }
  if (currentStatus === 'dispatched') {
    const options = ['dispatched', 'in_progress']
    if (canManage) options.push('cancelled')
    return options
  }
  if (currentStatus === 'in_progress') {
    const options = ['in_progress', 'completed']
    if (canManage) options.push('cancelled')
    return options
  }
  return [currentStatus]
}

function RouteStopsSection({ routes }: { routes: RouteDetailResponse[] }) {
  if (routes.length === 0) {
    return <p className="text-sm text-slate-500">No routes linked to this trip.</p>
  }

  return (
    <ul className="space-y-3">
      {routes.map((route) => (
        <li
          key={route.routeId}
          className="rounded-lg border border-slate-700 bg-slate-950/40 p-3"
          data-testid={`trip-workspace-route-${route.routeId}`}
        >
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div>
              <p className="font-medium text-slate-100">{route.title}</p>
              <p className="text-xs text-slate-500">
                {route.routeNumber} · {route.routeStatus.replace('_', ' ')}
              </p>
            </div>
            <span className="text-xs text-slate-400">
              {route.stops.filter((s) => s.stopStatus === 'completed').length}/{route.stops.length}{' '}
              stops done
            </span>
          </div>
          {route.stops.length > 0 ? (
            <ol className="mt-2 space-y-1 border-t border-slate-800 pt-2">
              {route.stops.map((stop) => (
                <li
                  key={stop.stopId}
                  className="flex flex-wrap items-center gap-2 text-xs text-slate-300"
                  data-testid={`trip-workspace-stop-${stop.stopId}`}
                >
                  <span className="text-slate-500">{stop.sequenceNumber}.</span>
                  <span>{stop.label || stop.stopKey}</span>
                  <span className="rounded bg-slate-800 px-1.5 py-0.5 uppercase tracking-wide text-slate-400">
                    {stop.stopStatus}
                  </span>
                </li>
              ))}
            </ol>
          ) : null}
        </li>
      ))}
    </ul>
  )
}

function ExecutionProofDvirSection({
  accessToken,
  tripId,
  vehicleRefKey,
  execution,
  canCapture,
  dvirPending,
  dvirError,
  onSubmitDvir,
  onCaptureUpdated,
}: {
  accessToken: string
  tripId: string
  vehicleRefKey: string | null
  execution: Awaited<ReturnType<typeof getTripExecutionSummary>>
  canCapture: boolean
  dvirPending: boolean
  dvirError: string | null
  onSubmitDvir: (payload: {
    phase: 'pre_trip' | 'post_trip'
    result: string
    odometerReading?: number
    defectNotes?: string
    vehicleRefKey?: string
  }) => void
  onCaptureUpdated: () => void
}) {
  return (
    <div className="space-y-4 text-sm">
      <p className="text-slate-300" data-testid="trip-workspace-execution-header">
        Pre DVIR {execution.hasPreTripDvir ? 'captured' : 'missing'} · Post DVIR{' '}
        {execution.hasPostTripDvir ? 'captured' : 'missing'} · Driver closed{' '}
        {execution.closedAt ? formatTimestamp(execution.closedAt) : 'no'}
      </p>

      <div>
        <h4 className="font-medium text-slate-200">Proof ({execution.proofs.length})</h4>
        {execution.proofs.length === 0 ? (
          <p className="mt-1 text-xs text-slate-500">No proof captured yet.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {execution.proofs.map((proof) => (
              <li
                key={proof.proofId}
                className="rounded border border-slate-700 bg-slate-950/50 p-2 text-xs"
              >
                <span className="font-medium text-slate-200">{proof.proofType}</span>
                {proof.referenceKey ? ` · ${proof.referenceKey}` : ''}
                <p className="text-slate-500">{formatTimestamp(proof.capturedAt)}</p>
                {proof.attachments.length > 0 ? (
                  <ul className="mt-1 space-y-1">
                    {proof.attachments.map((attachment) => (
                      <li key={attachment.attachmentId}>
                        <button
                          type="button"
                          className="text-sky-400 underline"
                          onClick={() =>
                            void downloadTripCaptureAttachment(
                              accessToken,
                              tripId,
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
        <h4 className="font-medium text-slate-200">DVIR ({execution.dvirInspections.length})</h4>
        {execution.dvirInspections.length === 0 ? (
          <p className="mt-1 text-xs text-slate-500">No DVIR submitted.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {execution.dvirInspections.map((dvir) => (
              <li
                key={dvir.dvirId}
                className="rounded border border-slate-700 bg-slate-950/50 p-2 text-xs"
                data-testid={`trip-workspace-dvir-${dvir.dvirId}`}
              >
                <span className="font-medium text-slate-200">
                  {dvir.phase.replace('_', ' ')} · {dvir.result}
                </span>
                <p className="text-slate-500">{formatTimestamp(dvir.submittedAt)}</p>
                {dvir.attachments.length > 0 ? (
                  <ul className="mt-1 space-y-1">
                    {dvir.attachments.map((attachment) => (
                      <li key={attachment.attachmentId}>
                        <button
                          type="button"
                          className="text-sky-400 underline"
                          onClick={() =>
                            void downloadTripCaptureAttachment(
                              accessToken,
                              tripId,
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
                {canCapture ? (
                  <TripCaptureAttachmentPanel
                    accessToken={accessToken}
                    tripId={tripId}
                    subjectType="dvir"
                    subjectId={dvir.dvirId}
                    subjectLabel={`${dvir.phase.replace('_', ' ')} DVIR`}
                    attachments={dvir.attachments}
                    captureChannel="operator"
                    onUploaded={onCaptureUpdated}
                  />
                ) : null}
              </li>
            ))}
          </ul>
        )}
      </div>

      {canCapture ? (
        <div
          className="space-y-3 border-t border-slate-800 pt-3"
          data-testid="trip-workspace-dvir-capture"
        >
          <p className="text-xs text-slate-500">
            Operator capture for assigned trips. Submitted DVIR updates capture readiness gates.
          </p>
          {!execution.hasPreTripDvir ? (
            <TripDvirSubmitForm
              phase="pre_trip"
              label="Pre-trip DVIR"
              vehicleRefKey={vehicleRefKey}
              disabled={dvirPending}
              pending={dvirPending}
              onSubmit={onSubmitDvir}
            />
          ) : null}
          {!execution.hasPostTripDvir ? (
            <TripDvirSubmitForm
              phase="post_trip"
              label="Post-trip DVIR"
              vehicleRefKey={vehicleRefKey}
              disabled={dvirPending}
              pending={dvirPending}
              onSubmit={onSubmitDvir}
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

export function TripExecutionWorkspacePanel({
  accessToken,
  tripId,
  canPerform,
  canManage,
}: Props) {
  const queryClient = useQueryClient()
  const [statusMessage, setStatusMessage] = useState<string | null>(null)
  const [dvirError, setDvirError] = useState<string | null>(null)

  const tripQuery = useQuery({
    queryKey: ['routarr-trip', accessToken, tripId],
    queryFn: () => getTrip(accessToken, tripId),
  })

  const reportQuery = useQuery({
    queryKey: ['routarr-trip-report', accessToken, tripId],
    queryFn: () => getDispatchReportTripDetail(accessToken, tripId),
  })

  const routesQuery = useQuery({
    queryKey: ['routarr-trip-routes', accessToken, tripId],
    queryFn: async () => {
      const summaries = await getRoutes(accessToken, tripId)
      return Promise.all(summaries.map((route) => getRoute(accessToken, route.routeId)))
    },
  })

  const readinessQuery = useQuery({
    queryKey: ['routarr-trip-readiness', accessToken, tripId],
    queryFn: () => getTripCaptureReadiness(accessToken, tripId),
    enabled: Boolean(tripQuery.data && tripQuery.data.dispatchStatus !== 'cancelled'),
  })

  const executionQuery = useQuery({
    queryKey: ['routarr-trip-execution', accessToken, tripId],
    queryFn: () => getTripExecutionSummary(accessToken, tripId),
  })

  const auditQuery = useQuery({
    queryKey: ['routarr-trip-audit', accessToken, tripId],
    queryFn: () => getTripAuditTrail(accessToken, tripId, 20),
  })

  const statusMutation = useMutation({
    mutationFn: (status: string) =>
      updateTripStatus(accessToken, tripId, { dispatchStatus: status }),
    onSuccess: async () => {
      setStatusMessage(null)
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip', accessToken, tripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-report', accessToken, tripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-readiness', accessToken, tripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-execution', accessToken, tripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-audit', accessToken, tripId] })
    },
    onError: (error: Error) => setStatusMessage(error.message),
  })

  const invalidateCaptureQueries = async () => {
    await queryClient.invalidateQueries({ queryKey: ['routarr-trip-readiness', accessToken, tripId] })
    await queryClient.invalidateQueries({ queryKey: ['routarr-trip-execution', accessToken, tripId] })
    await queryClient.invalidateQueries({ queryKey: ['routarr-trip-audit', accessToken, tripId] })
  }

  const dvirMutation = useMutation({
    mutationFn: (payload: {
      phase: 'pre_trip' | 'post_trip'
      result: string
      odometerReading?: number
      defectNotes?: string
      vehicleRefKey?: string
    }) =>
      submitTripDvir(accessToken, tripId, {
        phase: payload.phase,
        result: payload.result,
        vehicleRefKey: payload.vehicleRefKey ?? undefined,
        odometerReading: payload.odometerReading,
        defectNotes: payload.defectNotes,
      }),
    onSuccess: async () => {
      setDvirError(null)
      await invalidateCaptureQueries()
    },
    onError: (error: Error) => setDvirError(error.message),
  })

  if (tripQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading trip execution workspace…</p>
  }

  if (tripQuery.isError || !tripQuery.data) {
    return (
      <p className="text-sm text-red-400" role="alert">
        {(tripQuery.error as Error | undefined)?.message ?? 'Trip not found or access denied.'}
      </p>
    )
  }

  const trip: TripDetailResponse = tripQuery.data
  const report = reportQuery.data
  const active =
    trip.dispatchStatus !== 'completed' && trip.dispatchStatus !== 'cancelled'

  return (
    <div className="space-y-6" data-testid="trip-execution-workspace-panel">
      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-white">{trip.title}</h2>
            <p className="text-sm text-slate-400">{trip.tripNumber}</p>
          </div>
          <span className="rounded bg-slate-800 px-2 py-1 text-xs uppercase tracking-wide text-slate-200">
            {trip.dispatchStatus.replace('_', ' ')}
          </span>
        </div>
        <p className="mt-2 text-sm text-slate-300">{trip.description || 'No description'}</p>
        <dl className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4 text-sm">
          <div>
            <dt className="text-slate-500">Driver</dt>
            <dd className="text-slate-200">{trip.assignedDriverPersonId ?? 'Unassigned'}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Vehicle</dt>
            <dd className="text-slate-200">{trip.vehicleRefKey ?? '—'}</dd>
          </div>
          <div>
            <dt className="text-slate-500">Scheduled</dt>
            <dd className="text-slate-200">
              {formatTimestamp(trip.scheduledStartAt)} → {formatTimestamp(trip.scheduledEndAt)}
            </dd>
          </div>
          {report ? (
            <div>
              <dt className="text-slate-500">Execution</dt>
              <dd className="text-slate-200">
                {report.pendingStopCount} pending stop(s) · {report.linkedExceptionCount} open
                exception(s)
              </dd>
            </div>
          ) : null}
        </dl>
        {report?.isLate ? (
          <p className="mt-3 text-sm text-red-300">Trip is late against schedule.</p>
        ) : report?.isAtRisk ? (
          <p className="mt-3 text-sm text-amber-300">Trip is at risk.</p>
        ) : null}
      </section>

      {readinessQuery.data && active ? (
        <section
          className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
          data-testid="trip-workspace-readiness"
        >
          <h3 className="text-sm font-semibold text-slate-200">Capture readiness</h3>
          <p className="mt-1 text-xs text-slate-500">
            Tenant policy gates for start and complete. Capture in this workspace or the driver portal.
          </p>
          <div className="mt-3 flex flex-wrap gap-3 text-xs">
            <span className={readinessQuery.data.canStartTrip ? 'text-emerald-400' : 'text-amber-400'}>
              Start {readinessQuery.data.canStartTrip ? 'ready' : 'blocked'}
            </span>
            <span
              className={readinessQuery.data.canCompleteTrip ? 'text-emerald-400' : 'text-amber-400'}
            >
              Complete {readinessQuery.data.canCompleteTrip ? 'ready' : 'blocked'}
            </span>
          </div>
          <ul className="mt-3 space-y-1">
            {readinessQuery.data.items.map((item) => (
              <li key={item.key} className="flex items-start gap-2 text-xs text-slate-300">
                {item.satisfied ? (
                  <Check className="mt-0.5 h-3.5 w-3.5 shrink-0 text-emerald-400" />
                ) : (
                  <X
                    className={`mt-0.5 h-3.5 w-3.5 shrink-0 ${
                      item.required ? 'text-amber-400' : 'text-slate-500'
                    }`}
                  />
                )}
                <span>
                  {item.label}
                  {item.message ? ` — ${item.message}` : ''}
                </span>
              </li>
            ))}
          </ul>
          <Link
            to="/driver-portal"
            className="mt-3 inline-flex items-center gap-1 text-xs text-teal-300 hover:text-teal-200"
          >
            Open driver portal
            <ExternalLink className="h-3 w-3" />
          </Link>
        </section>
      ) : null}

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h3 className="text-sm font-semibold text-slate-200">Routes & stops</h3>
        {routesQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-500">Loading routes…</p>
        ) : (
          <div className="mt-3">
            <RouteStopsSection routes={routesQuery.data ?? []} />
          </div>
        )}
      </section>

      <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
        <h3 className="text-sm font-semibold text-slate-200">Proof & DVIR</h3>
        {executionQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-500">Loading execution capture…</p>
        ) : executionQuery.data ? (
          <div className="mt-3">
            <ExecutionProofDvirSection
              accessToken={accessToken}
              tripId={tripId}
              vehicleRefKey={trip.vehicleRefKey}
              execution={executionQuery.data}
              canCapture={canPerform && active}
              dvirPending={dvirMutation.isPending}
              dvirError={dvirError}
              onSubmitDvir={(payload) => dvirMutation.mutate(payload)}
              onCaptureUpdated={() => void invalidateCaptureQueries()}
            />
          </div>
        ) : null}
      </section>

      {canPerform && active ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
          <h3 className="text-sm font-semibold text-slate-200">Dispatcher status</h3>
          <p className="mt-1 text-xs text-slate-500">
            Operator override for dispatch status. Capture requirements still apply on driver portal
            start/complete.
          </p>
          <label className="mt-3 block text-sm text-slate-300">
            Dispatch status
            <select
              className="mt-1 w-full max-w-xs rounded border border-slate-600 bg-slate-950 px-3 py-2"
              value={trip.dispatchStatus}
              disabled={statusMutation.isPending}
              onChange={(event) => {
                const next = event.target.value
                if (next !== trip.dispatchStatus) {
                  statusMutation.mutate(next)
                }
              }}
            >
              {statusOptionsFor(trip.dispatchStatus, canManage).map((status) => (
                <option key={status} value={status}>
                  {status.replace('_', ' ')}
                </option>
              ))}
            </select>
          </label>
          {statusMessage ? (
            <p className="mt-2 text-sm text-red-400" role="alert">
              {statusMessage}
            </p>
          ) : null}
        </section>
      ) : null}

      <section
        className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
        data-testid="trip-workspace-audit-trail"
      >
        <h3 className="text-sm font-semibold text-slate-200">Transportation audit trail</h3>
        <p className="mt-1 text-xs text-slate-500">
          Trip-scoped RoutArr audit events including status, proof, and DVIR capture.
        </p>
        {auditQuery.isLoading ? (
          <p className="mt-2 text-sm text-slate-500">Loading audit trail…</p>
        ) : auditQuery.data && auditQuery.data.entries.length > 0 ? (
          <ul className="mt-3 max-h-56 divide-y divide-slate-800 overflow-y-auto text-xs text-slate-300">
            {auditQuery.data.entries.map((entry) => (
              <li key={entry.auditEventId} className="py-2">
                <span className="text-slate-500">{formatTimestamp(entry.occurredAt)}</span>
                <span className="ml-2 font-medium text-slate-200">{entry.action}</span>
                <span className="ml-2 text-slate-400">{entry.result}</span>
              </li>
            ))}
          </ul>
        ) : (
          <p className="mt-2 text-sm text-slate-500">No audit events recorded for this trip yet.</p>
        )}
      </section>
    </div>
  )
}
