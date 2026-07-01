import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  AlertTriangle,
  CheckCircle2,
  ExternalLink,
  FileText,
  Package,
  Route,
  ShieldCheck,
  Truck,
  X,
} from 'lucide-react'
import { Link } from 'react-router-dom'
import { useState } from 'react'
import {
  ApiErrorCallout,
  DetailBadge,
  DetailEmptyState,
  getErrorMessage,
  ProfileDetailsLayout,
  type DetailRailSectionConfig,
  type DetailTone,
} from '@stl/shared-ui'

import {
  downloadTripCaptureAttachment,
  getRoute,
  getRoutes,
  getTrip,
  getTripAuditTrail,
  getTripCaptureReadiness,
  getTripExecutionSummary,
  listDispatchExceptions,
  overrideTripSupplierReadiness,
  submitTripDvir,
  updateTripStatus,
} from '../api/client'
import type { RouteDetailResponse, TripDetailResponse } from '../api/types'
import { TripCaptureAttachmentPanel } from './TripCaptureAttachmentPanel'
import { TripDvirSubmitForm } from './TripDvirSubmitForm'

type Props = {
  accessToken: string
  tripId: string
  canDispatch: boolean
  canPerform: boolean
  canManage: boolean
  canOverrideSupplierReadiness: boolean
}

function formatTimestamp(iso: string | null | undefined) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
  } catch {
    return iso
  }
}

function humanize(value: string | null | undefined) {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatQuantity(value: number | null | undefined) {
  if (value == null) return 'Not recorded'
  return value.toLocaleString()
}

function isPendingStopStatus(status: string) {
  return !['completed', 'skipped', 'cancelled'].includes(status.toLowerCase())
}

function statusTone(value: string | null | undefined): DetailTone {
  const normalized = value?.toLowerCase() ?? ''
  if (['assigned', 'dispatched', 'in_progress', 'active', 'arrived'].includes(normalized)) return 'info'
  if (['completed', 'closed', 'delivered'].includes(normalized)) return 'good'
  if (['draft', 'planned', 'pending'].includes(normalized)) return 'warn'
  if (['cancelled', 'failed', 'blocked', 'skipped'].includes(normalized)) return 'bad'
  return 'neutral'
}

function statusOptionsFor(currentStatus: string, canDispatch: boolean, canManage: boolean): string[] {
  if (currentStatus === 'planned') {
    return canManage ? ['planned', 'cancelled'] : ['planned']
  }
  if (currentStatus === 'assigned') {
    const options = ['assigned']
    if (canDispatch) {
      options.push('dispatched')
    }
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
    return <DetailEmptyState text="No routes are linked to this trip yet." />
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
              <p className="text-xs text-[var(--color-text-muted)]">
                {route.routeNumber} · {route.routeStatus.replace('_', ' ')}
              </p>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-xs text-slate-400">
                {route.stops.filter((s) => s.stopStatus === 'completed').length}/{route.stops.length}{' '}
                stops done
              </span>
              <Link
                to={`/routes/${route.routeId}`}
                className="rounded-full border border-slate-700 px-2 py-1 text-xs text-sky-300 hover:border-sky-500 hover:text-sky-200"
              >
                Open route
              </Link>
            </div>
          </div>
          {route.stops.length > 0 ? (
            <ol className="mt-2 space-y-1 border-t border-slate-800 pt-2">
              {route.stops.map((stop) => (
                <li
                  key={stop.stopId}
                  className="flex flex-wrap items-center gap-2 text-xs text-slate-300"
                  data-testid={`trip-workspace-stop-${stop.stopId}`}
                >
                  <span className="text-[var(--color-text-muted)]">{stop.sequenceNumber}.</span>
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

export function ExecutionProofDvirSection({
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
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">No proof captured yet.</p>
        ) : (
          <ul className="mt-2 space-y-2">
            {execution.proofs.map((proof) => (
              <li
                key={proof.proofId}
                className="rounded border border-slate-700 bg-slate-950/50 p-2 text-xs"
              >
                <span className="font-medium text-slate-200">{proof.proofType}</span>
                {proof.referenceKey ? ` · ${proof.referenceKey}` : ''}
                <p className="text-[var(--color-text-muted)]">{formatTimestamp(proof.capturedAt)}</p>
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
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">No DVIR submitted.</p>
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
                <p className="text-[var(--color-text-muted)]">{formatTimestamp(dvir.submittedAt)}</p>
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
          <p className="text-xs text-[var(--color-text-muted)]">
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
  canDispatch,
  canPerform,
  canManage,
  canOverrideSupplierReadiness,
}: Props) {
  const queryClient = useQueryClient()
  const [statusMessage, setStatusMessage] = useState<string | null>(null)
  const [dvirError, setDvirError] = useState<string | null>(null)
  const [overrideReason, setOverrideReason] = useState('')
  const [overrideSuccessMessage, setOverrideSuccessMessage] = useState<string | null>(null)

  const tripQuery = useQuery({
    queryKey: ['routarr-trip', accessToken, tripId],
    queryFn: () => getTrip(accessToken, tripId),
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

  const exceptionsQuery = useQuery({
    queryKey: ['routarr-trip-exceptions', accessToken, tripId],
    queryFn: () => listDispatchExceptions(accessToken),
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

  const supplierOverrideMutation = useMutation({
    mutationFn: (reason: string) =>
      overrideTripSupplierReadiness(accessToken, tripId, { reason }),
    onMutate: () => {
      setOverrideSuccessMessage(null)
    },
    onSuccess: async () => {
      setOverrideReason('')
      setOverrideSuccessMessage('Supplier-readiness override recorded. Dispatch can proceed with audit trail.')
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip', accessToken, tripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-readiness', accessToken, tripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-execution', accessToken, tripId] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip-audit', accessToken, tripId] })
    },
  })

  if (tripQuery.isLoading) {
    return <p className="text-sm text-slate-400">Loading trip execution workspace…</p>
  }

  if (tripQuery.isError || !tripQuery.data) {
    return (
      <ApiErrorCallout
        message={getErrorMessage(
          tripQuery.error,
          'Trip not found or access denied.',
        )}
        onRetry={() => void tripQuery.refetch()}
        retryLabel="Retry trip workspace"
      />
    )
  }

  const trip: TripDetailResponse = tripQuery.data
  const active =
    trip.dispatchStatus !== 'completed' && trip.dispatchStatus !== 'cancelled'
  const routes = routesQuery.data ?? []
  const pendingStopCount = routes.reduce(
    (total, route) => total + route.stops.filter((stop) => isPendingStopStatus(stop.stopStatus)).length,
    0,
  )
  const execution = executionQuery.data ?? null
  const readiness = readinessQuery.data ?? null
  const openExceptions = (exceptionsQuery.data?.items ?? []).filter((item) => item.tripId === trip.tripId)
  const openExceptionCount = openExceptions.length
  const hasReadinessGap = Boolean(
    readiness?.items.some((item) => item.required && !item.satisfied),
  )
  const activeSupplierBlock =
    trip.dispatchBlocks?.find(
      (block) => block.blockType === 'supplier_readiness' && block.status === 'active',
    ) ?? null
  const hasSupplierLink = Boolean(trip.supplierOrderId || trip.brokerOrderId)
  const blocked =
    trip.dispatchStatus === 'cancelled' ||
    !trip.assignedDriverPersonId ||
    !trip.vehicleRefKey ||
    Boolean(activeSupplierBlock)
  const atRisk = openExceptionCount > 0 || hasReadinessGap || Boolean(trip.dispatchOverrideAt)
  const decisionTone: DetailTone = blocked ? 'bad' : atRisk ? 'warn' : 'good'
  const decisionLabel = activeSupplierBlock
    ? 'Blocked by supplier readiness'
    : blocked
      ? 'Needs assignment'
      : atRisk
        ? 'Watch closely'
        : 'Dispatchable'
  const decisionSummary = activeSupplierBlock
    ? 'Trip is blocked until SupplyArr releases supplier readiness or an authorized override is recorded.'
    : blocked
      ? 'Trip needs dispatch attention'
    : atRisk
      ? 'Trip is on watch for capture readiness gaps or open exceptions'
      : 'Trip can proceed through dispatch'
  const decisionDetail = activeSupplierBlock
    ? 'Supplier readiness is tracked separately from trip execution. Resolve the active block or record an override reason before dispatch.'
    : blocked
      ? 'Driver, vehicle, or lifecycle status must be resolved before normal dispatch execution.'
    : atRisk
      ? 'Open exceptions or required capture steps need close monitoring before closeout.'
      : 'Driver, vehicle, capture readiness, and trip status support normal dispatch execution.'
  const railSections: DetailRailSectionConfig[] = [
    {
      title: 'Supplier readiness',
      icon: <Package className="h-5 w-5" />,
      content: hasSupplierLink || activeSupplierBlock || trip.supplierReadinessStatusSnapshot ? (
        <div className="space-y-4" data-testid="trip-workspace-supplier-readiness">
          <div className="flex flex-wrap items-center gap-2">
            {trip.supplierOrderId ? (
              <DetailBadge label={`Supplier order ${trip.supplierOrderId}`} tone="info" />
            ) : null}
            {trip.brokerOrderId ? (
              <DetailBadge label={`Broker order ${trip.brokerOrderId}`} tone="neutral" />
            ) : null}
            <DetailBadge
              label={trip.releasedForDispatchAt ? 'Released for dispatch' : activeSupplierBlock ? 'Dispatch blocked' : humanize(trip.supplierReadinessStatusSnapshot)}
              tone={trip.releasedForDispatchAt ? 'good' : activeSupplierBlock ? 'bad' : statusTone(trip.supplierReadinessStatusSnapshot)}
            />
            {trip.dispatchOverrideAt ? (
              <DetailBadge label="Override recorded" tone="warn" />
            ) : null}
          </div>

          {activeSupplierBlock ? (
            <div className="rounded-xl border border-red-800/60 bg-red-950/30 p-4 text-sm text-red-100">
              <p className="font-semibold">Supplier-readiness block is active.</p>
              <p className="mt-1 text-xs text-red-100/80">
                {humanize(activeSupplierBlock.blockReason)}. Dispatch remains blocked until SupplyArr releases the linked order or an authorized override is recorded in RoutArr.
              </p>
            </div>
          ) : null}

          <dl className="grid gap-3 md:grid-cols-2">
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
              <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Supplier status snapshot</dt>
              <dd className="mt-1 text-sm text-slate-100">{humanize(trip.supplierReadinessStatusSnapshot)}</dd>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
              <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Quantity snapshot</dt>
              <dd className="mt-1 text-sm text-slate-100">
                {formatQuantity(trip.supplierQuantityReadySnapshot)} of {formatQuantity(trip.supplierOrderedQuantitySnapshot)} ready
              </dd>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
              <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Expected ready</dt>
              <dd className="mt-1 text-sm text-slate-100">{formatTimestamp(trip.supplierExpectedReadyAtSnapshot)}</dd>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
              <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Confirmed ready</dt>
              <dd className="mt-1 text-sm text-slate-100">{formatTimestamp(trip.supplierConfirmedReadyAtSnapshot)}</dd>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
              <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Released for dispatch</dt>
              <dd className="mt-1 text-sm text-slate-100">{formatTimestamp(trip.releasedForDispatchAt)}</dd>
            </div>
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-3">
              <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Override reason</dt>
              <dd className="mt-1 text-sm text-slate-100">{trip.dispatchOverrideReason ?? 'Not recorded'}</dd>
            </div>
          </dl>

          {trip.dispatchReleaseSnapshot ? (
            <div className="rounded-xl border border-emerald-800/40 bg-emerald-950/20 p-4 text-sm text-emerald-100">
              <p className="font-semibold">Dispatcher release snapshot</p>
              <p className="mt-1 text-xs text-emerald-100/80">{trip.dispatchReleaseSnapshot.summary}</p>
              <p className="mt-2 text-xs text-emerald-100/80">
                Released {formatTimestamp(trip.dispatchReleaseSnapshot.releasedAt)} · Driver assignment {trip.dispatchReleaseSnapshot.driverCanAssign ? 'ready' : 'blocked'} · Vehicle assignment {trip.dispatchReleaseSnapshot.vehicleCanAssign ? 'ready' : 'blocked'}
              </p>
            </div>
          ) : null}

          {canOverrideSupplierReadiness && activeSupplierBlock ? (
            <div className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <h4 className="text-sm font-semibold text-slate-100">Supplier-readiness override</h4>
              <p className="mt-1 text-xs text-slate-400">
                Tenant admins, RoutArr admins, and RoutArr managers can resolve the block with a required reason. This writes an audit trail and emits `routarr.dispatch.override.performed`.
              </p>
              <label className="mt-3 block text-sm text-slate-300" htmlFor="trip-workspace-supplier-override-reason">
                Override reason
                <textarea
                  id="trip-workspace-supplier-override-reason"
                  className="mt-1 min-h-24 w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2"
                  value={overrideReason}
                  onChange={(event) => setOverrideReason(event.target.value)}
                  placeholder="Explain why dispatch should proceed despite the supplier-readiness block."
                />
              </label>
              <div className="mt-3 flex flex-wrap items-center gap-3">
                <button
                  type="button"
                  className="rounded-xl bg-red-500 px-4 py-2 text-sm font-semibold text-white hover:bg-red-400 disabled:opacity-50"
                  disabled={supplierOverrideMutation.isPending || !overrideReason.trim()}
                  onClick={() => supplierOverrideMutation.mutate(overrideReason.trim())}
                >
                  {supplierOverrideMutation.isPending ? 'Recording override…' : 'Override supplier-readiness block'}
                </button>
                {overrideSuccessMessage ? (
                  <p className="text-xs text-emerald-300">{overrideSuccessMessage}</p>
                ) : null}
              </div>
              {supplierOverrideMutation.isError ? (
                <p className="mt-2 text-xs text-red-300" role="alert">
                  {getErrorMessage(supplierOverrideMutation.error, 'Failed to record supplier-readiness override.')}
                </p>
              ) : null}
            </div>
          ) : null}
        </div>
      ) : (
        <DetailEmptyState text="This trip does not have a linked supplier order or supplier-readiness snapshot." />
      ),
    },
    {
      title: 'Related records',
      icon: <Route className="h-5 w-5" />,
      content: routes.length > 0 ? (
        <div className="space-y-3">
          {routes.map((route) => (
            <div key={route.routeId} className="rounded-xl border border-slate-800 bg-slate-900/70 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="font-medium text-white">{route.title}</p>
                  <p className="mt-1 text-xs text-slate-400">
                    {route.routeNumber} · {humanize(route.routeStatus)} · {route.stops.length} stop(s)
                  </p>
                </div>
                <DetailBadge label={humanize(route.routeStatus)} tone={statusTone(route.routeStatus)} />
              </div>
              <div className="mt-3">
                <Link to={`/routes/${route.routeId}`} className="text-sm text-sky-300 hover:text-sky-200">
                  Open route
                </Link>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <DetailEmptyState text="No routes are linked to this trip." />
      ),
    },
    {
      title: 'Capture readiness',
      icon: <ShieldCheck className="h-5 w-5" />,
      content: readiness ? (
        <div className="space-y-3" data-testid="trip-workspace-readiness">
          <div className="flex flex-wrap items-center gap-2">
            <DetailBadge label={readiness.canStartTrip ? 'Start ready' : 'Start blocked'} tone={readiness.canStartTrip ? 'good' : 'warn'} />
            <DetailBadge label={readiness.canCompleteTrip ? 'Complete ready' : 'Complete blocked'} tone={readiness.canCompleteTrip ? 'good' : 'warn'} />
          </div>
          <ul className="space-y-2">
            {readiness.items.map((item) => (
              <li key={item.key} className="flex items-start gap-2 text-sm text-slate-300">
                {item.satisfied ? (
                  <CheckCircle2 className="mt-0.5 h-4 w-4 shrink-0 text-emerald-400" />
                ) : (
                  <AlertTriangle className={`mt-0.5 h-4 w-4 shrink-0 ${item.required ? 'text-amber-400' : 'text-[var(--color-text-muted)]'}`} />
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
            className="inline-flex items-center gap-1 text-sm text-teal-300 hover:text-teal-200"
          >
            Open driver portal
            <ExternalLink className="h-4 w-4" />
          </Link>
        </div>
      ) : (
        <DetailEmptyState text="Trip capture readiness is unavailable right now." />
      ),
    },
    {
      title: 'Evidence and documents',
      icon: <FileText className="h-5 w-5" />,
      content: execution ? (
        <div className="space-y-4 text-sm">
          <p className="text-slate-300" data-testid="trip-workspace-execution-header">
            Pre DVIR {execution.hasPreTripDvir ? 'captured' : 'missing'} · Post DVIR{' '}
            {execution.hasPostTripDvir ? 'captured' : 'missing'} · Driver closed{' '}
            {execution.closedAt ? formatTimestamp(execution.closedAt) : 'no'}
          </p>

          <div>
            <h4 className="font-medium text-slate-200">Proof ({execution.proofs.length})</h4>
            {execution.proofs.length === 0 ? (
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">No proof captured yet.</p>
            ) : (
              <ul className="mt-2 space-y-2">
                {execution.proofs.map((proof) => (
                  <li
                    key={proof.proofId}
                    className="rounded border border-slate-700 bg-slate-950/50 p-2 text-xs"
                  >
                    <span className="font-medium text-slate-200">{proof.proofType}</span>
                    {proof.referenceKey ? ` · ${proof.referenceKey}` : ''}
                    <p className="text-[var(--color-text-muted)]">{formatTimestamp(proof.capturedAt)}</p>
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
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">No DVIR submitted.</p>
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
                    <p className="text-[var(--color-text-muted)]">{formatTimestamp(dvir.submittedAt)}</p>
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
                    {canPerform && active ? (
                      <TripCaptureAttachmentPanel
                        accessToken={accessToken}
                        tripId={tripId}
                        subjectType="dvir"
                        subjectId={dvir.dvirId}
                        subjectLabel={`${dvir.phase.replace('_', ' ')} DVIR`}
                        attachments={dvir.attachments}
                        captureChannel="operator"
                        onUploaded={() => void invalidateCaptureQueries()}
                      />
                    ) : null}
                  </li>
                ))}
              </ul>
            )}
          </div>

          {canPerform && active ? (
            <div
              className="space-y-3 border-t border-slate-800 pt-3"
              data-testid="trip-workspace-dvir-capture"
            >
              <p className="text-xs text-[var(--color-text-muted)]">
                Operator capture for assigned trips. Submitted DVIR updates capture readiness gates.
              </p>
              {!execution.hasPreTripDvir ? (
                <TripDvirSubmitForm
                  phase="pre_trip"
                  label="Pre-trip DVIR"
                  vehicleRefKey={trip.vehicleRefKey}
                  disabled={dvirMutation.isPending}
                  pending={dvirMutation.isPending}
                  onSubmit={(payload) => dvirMutation.mutate(payload)}
                />
              ) : null}
              {!execution.hasPostTripDvir ? (
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
      ) : (
        <DetailEmptyState text="Evidence and DVIR history are unavailable." />
      ),
    },
    {
      title: 'Activity',
      icon: <FileText className="h-5 w-5" />,
      content: (
        <section
          className="rounded-xl border border-slate-700 bg-slate-900/60 p-4"
          data-testid="trip-workspace-audit-trail"
        >
          <h3 className="text-sm font-semibold text-slate-200">Transportation audit trail</h3>
          <p className="mt-1 text-xs text-[var(--color-text-muted)]">
            Trip-scoped RoutArr audit events including status, proof, and DVIR capture.
          </p>
          {auditQuery.isLoading ? (
            <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading audit trail…</p>
          ) : auditQuery.isError ? (
            <ApiErrorCallout
              className="mt-2"
              message={getErrorMessage(auditQuery.error, 'Failed to load transportation audit trail.')}
              onRetry={() => void auditQuery.refetch()}
              retryLabel="Retry audit trail"
            />
          ) : auditQuery.data && auditQuery.data.entries.length > 0 ? (
            <ul className="mt-3 max-h-56 divide-y divide-slate-800 overflow-y-auto text-xs text-slate-300">
              {auditQuery.data.entries.map((entry) => (
                <li key={entry.auditEventId} className="py-2">
                  <span className="text-[var(--color-text-muted)]">{formatTimestamp(entry.occurredAt)}</span>
                  <span className="ml-2 font-medium text-slate-200">{entry.action}</span>
                  <span className="ml-2 text-slate-400">{entry.result}</span>
                </li>
              ))}
            </ul>
          ) : (
            <p className="mt-2 text-sm text-[var(--color-text-muted)]">No audit events recorded for this trip yet.</p>
          )}
        </section>
      ),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="trip-execution-workspace-panel"
      backLabel="Trips"
      backTo="/trips"
      breadcrumbs={[trip.tripNumber, trip.title]}
      icon={<Truck className="h-9 w-9" />}
      title={trip.title}
      subtitle={
        <span className="flex flex-wrap items-center gap-2">
          <span>{trip.tripNumber}</span>
          <span className="text-[var(--color-text-muted)]">-</span>
          <span>{trip.assignedDriverPersonId ?? 'Unassigned driver'}</span>
          <span className="text-[var(--color-text-muted)]">-</span>
          <span>{trip.vehicleRefKey ?? 'Unassigned vehicle'}</span>
          {trip.supplierOrderId ? (
            <>
              <span className="text-[var(--color-text-muted)]">-</span>
              <span>Supplier order {trip.supplierOrderId}</span>
            </>
          ) : null}
        </span>
      }
      badges={[
        { label: trip.tripNumber, tone: 'info' },
        { label: humanize(trip.dispatchStatus), tone: statusTone(trip.dispatchStatus) },
        activeSupplierBlock
          ? { label: 'Supplier blocked', tone: 'bad' as const }
          : { label: atRisk ? 'At risk' : 'On track', tone: atRisk ? 'warn' : 'good' as const },
      ]}
      actions={
        <>
          <Link
            to="/dispatch"
            className="inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-3 text-sm font-bold text-[var(--color-text-primary)] hover:bg-sky-400"
          >
            Open dispatch board
          </Link>
          <Link
            to="/driver-portal"
            className="inline-flex items-center gap-2 rounded-xl border border-slate-700 bg-slate-900 px-4 py-3 text-sm font-bold text-white hover:bg-slate-800"
          >
            Driver portal
          </Link>
        </>
      }
      metrics={[
        {
          label: 'Dispatch state',
          value: humanize(trip.dispatchStatus),
          hint: `Scheduled ${formatTimestamp(trip.scheduledStartAt)} → ${formatTimestamp(trip.scheduledEndAt)}`,
          icon: <Truck className="h-5 w-5" />,
          tone: statusTone(trip.dispatchStatus),
        },
        {
          label: 'Routes',
          value: routes.length,
          hint: `${pendingStopCount} pending stop(s) · ${openExceptionCount} open exception(s)`,
          icon: <Route className="h-5 w-5" />,
          tone: routes.length > 0 ? 'info' : 'neutral',
        },
        {
          label: 'Supplier readiness',
          value: activeSupplierBlock
            ? 'Blocked'
            : trip.releasedForDispatchAt
              ? 'Released'
              : trip.supplierOrderId
                ? humanize(trip.supplierReadinessStatusSnapshot)
                : 'No link',
          hint: trip.supplierOrderId
            ? `${formatQuantity(trip.supplierQuantityReadySnapshot)} of ${formatQuantity(trip.supplierOrderedQuantitySnapshot)} ready`
            : 'No linked SupplyArr supplier order',
          icon: <Package className="h-5 w-5" />,
          tone: activeSupplierBlock ? 'bad' : trip.releasedForDispatchAt ? 'good' : trip.supplierOrderId ? 'warn' : 'neutral',
        },
        {
          label: 'Proofs',
          value: execution?.proofs.length ?? 0,
          hint: `${execution?.dvirInspections.length ?? 0} DVIR inspection(s)`,
          icon: <FileText className="h-5 w-5" />,
          tone: (execution?.proofs.length ?? 0) > 0 ? 'good' : 'warn',
        },
        {
          label: 'Readiness',
          value: readiness ? (readiness.canStartTrip ? 'Ready' : 'Blocked') : 'Unavailable',
          hint: readiness
            ? `${readiness.items.filter((item) => item.required && !item.satisfied).length} required item(s) missing`
            : 'No readiness snapshot',
          icon: <ShieldCheck className="h-5 w-5" />,
          tone: blocked ? 'bad' : atRisk ? 'warn' : 'good',
        },
      ]} 
      tabs={['Overview', 'Routes', 'Readiness', 'Evidence', 'History']}
      snapshotTitle="Trip snapshot"
      snapshotSubtitle="Dispatch identity, assignment, timing, and reference labels for cross-product use."
      snapshotFields={[
        { label: 'Trip ID', value: trip.tripId, source: 'RoutArr record' },
        { label: 'Trip number', value: trip.tripNumber, source: 'Trip registry' },
        { label: 'Description', value: trip.description || 'Not recorded', source: 'Trip plan' },
        { label: 'Dispatch status', value: humanize(trip.dispatchStatus), source: 'Dispatch execution' },
        { label: 'Driver', value: trip.assignedDriverPersonId ?? 'Unassigned', source: 'StaffArr personId' },
        { label: 'Vehicle', value: trip.vehicleRefKey ?? 'Unassigned', source: 'MaintainArr asset ref' },
        { label: 'Scheduled start', value: formatTimestamp(trip.scheduledStartAt), source: 'Dispatch plan' },
        { label: 'Scheduled end', value: formatTimestamp(trip.scheduledEndAt), source: 'Dispatch plan' },
        { label: 'Assigned at', value: formatTimestamp(trip.assignedAt), source: 'Execution record' },
        { label: 'Supplier order', value: trip.supplierOrderId ?? 'Not linked', source: 'SupplyArr reference' },
        { label: 'Broker order', value: trip.brokerOrderId ?? 'Not linked', source: 'OrdArr reference snapshot' },
        { label: 'Supplier readiness', value: humanize(trip.supplierReadinessStatusSnapshot), source: 'SupplyArr readiness snapshot' },
        {
          label: 'Supplier quantity ready',
          value: formatQuantity(trip.supplierQuantityReadySnapshot),
          source: 'SupplyArr quantity snapshot',
        },
        {
          label: 'Supplier ordered quantity',
          value: formatQuantity(trip.supplierOrderedQuantitySnapshot),
          source: 'SupplyArr quantity snapshot',
        },
        {
          label: 'Supplier expected ready',
          value: formatTimestamp(trip.supplierExpectedReadyAtSnapshot),
          source: 'SupplyArr readiness snapshot',
        },
        {
          label: 'Supplier confirmed ready',
          value: formatTimestamp(trip.supplierConfirmedReadyAtSnapshot),
          source: 'SupplyArr readiness snapshot',
        },
        {
          label: 'Released for dispatch',
          value: formatTimestamp(trip.releasedForDispatchAt),
          source: 'RoutArr release audit',
        },
        {
          label: 'Released by event',
          value: trip.releasedForDispatchByEventId ?? 'Not recorded',
          source: 'SupplyArr event receipt',
        },
        { label: 'Dispatched at', value: formatTimestamp(trip.dispatchedAt), source: 'Execution record' },
        { label: 'Started at', value: formatTimestamp(trip.startedAt), source: 'Execution record' },
        { label: 'Completed at', value: formatTimestamp(trip.completedAt), source: 'Execution record' },
        {
          label: 'Dispatch override at',
          value: formatTimestamp(trip.dispatchOverrideAt),
          source: 'RoutArr override audit',
        },
        {
          label: 'Dispatch override by',
          value: trip.dispatchOverrideByPersonId ?? 'Not recorded',
          source: 'RoutArr override audit',
        },
        {
          label: 'Dispatch override reason',
          value: trip.dispatchOverrideReason ?? 'Not recorded',
          source: 'RoutArr override audit',
        },
      ]}
      mainContent={(
        <div className="space-y-5">
          <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
            <h3 className="text-sm font-semibold text-slate-200">Routes & stops</h3>
            {routesQuery.isLoading ? (
              <p className="mt-2 text-sm text-[var(--color-text-muted)]">Loading routes…</p>
            ) : routesQuery.isError ? (
              <ApiErrorCallout
                className="mt-2"
                message={getErrorMessage(routesQuery.error, 'Failed to load routes for this trip.')}
                onRetry={() => void routesQuery.refetch()}
                retryLabel="Retry routes"
              />
            ) : (
              <div className="mt-3">
                <RouteStopsSection routes={routes} />
              </div>
            )}
          </section>

          {canPerform && active ? (
            <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4">
              <h3 className="text-sm font-semibold text-slate-200">Dispatcher status</h3>
              <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                Operator override for dispatch status. Capture requirements still apply on driver portal start/complete.
              </p>
              {activeSupplierBlock ? (
                <div className="mt-3 rounded-xl border border-amber-700/50 bg-amber-950/30 p-3 text-sm text-amber-100">
                  Supplier readiness is currently blocking dispatch with reason {humanize(activeSupplierBlock.blockReason)}.
                  Resolve the linked SupplyArr order or record an authorized override before dispatching.
                </div>
              ) : null}
              <label className="mt-3 block text-sm text-slate-300" htmlFor="tripexecutionworkspace-dispatch-status">
                Dispatch status
                <select
                  id="tripexecutionworkspace-dispatch-status"
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
                  {statusOptionsFor(trip.dispatchStatus, canDispatch, canManage).map((status) => (
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
        </div>
      )}
      decisionTitle="Dispatch decision"
      decisionBadge={{ label: decisionLabel, tone: decisionTone }}
      decisionIcon={
        blocked ? (
          <X className="h-5 w-5 text-red-300" />
        ) : atRisk ? (
          <AlertTriangle className="h-5 w-5 text-amber-300" />
        ) : (
          <CheckCircle2 className="h-5 w-5 text-emerald-300" />
        )
      }
      decisionSummary={decisionSummary}
      decisionDetail={decisionDetail}
      allowedChecks={[
        Boolean(trip.assignedDriverPersonId),
        Boolean(trip.vehicleRefKey),
        trip.dispatchStatus !== 'cancelled',
        Boolean(execution?.hasPreTripDvir || execution?.hasPostTripDvir),
      ].filter(Boolean).length}
      blockedChecks={[
        !trip.assignedDriverPersonId,
        !trip.vehicleRefKey,
        trip.dispatchStatus === 'cancelled',
      ].filter(Boolean).length}
      railSections={railSections}
    />
  )
}
