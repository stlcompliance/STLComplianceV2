import {
  AlertTriangle,
  CheckCircle2,
  ClipboardList,
  MapPinned,
  Navigation,
  Package,
  Play,
  Route,
  Truck,
  UserCheck,
} from 'lucide-react'
import { useState } from 'react'
import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import {
  DetailBadge,
  DetailEmptyState,
  ProfileDetailsLayout,
  type DetailRailSectionConfig,
  type DetailTone,
} from '@stl/shared-ui'
import { buildOpenStreetMapUrl, OpenStreetMapCard } from '../../components/OpenStreetMapCard'
import type { RouteStopSummaryResponse } from '../../api/types'
import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatDate(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' })
}

function statusTone(value: string | null | undefined): DetailTone {
  const normalized = value?.toLowerCase() ?? ''
  if (['assigned', 'dispatched', 'in_progress', 'active', 'arrived'].includes(normalized)) return 'info'
  if (['completed', 'closed', 'delivered'].includes(normalized)) return 'good'
  if (['draft', 'planned', 'pending'].includes(normalized)) return 'warn'
  if (['cancelled', 'failed', 'blocked', 'skipped'].includes(normalized)) return 'bad'
  return 'neutral'
}

function geofenceTone(value: string | null | undefined): DetailTone {
  const normalized = value?.toLowerCase() ?? ''
  if (normalized === 'inside') return 'good'
  if (normalized === 'nearby') return 'warn'
  if (normalized === 'outside') return 'bad'
  return 'neutral'
}

function formatGeofenceDistance(value: number | null | undefined) {
  if (value == null) return 'Not recorded'
  return `${value.toFixed(2)} m`
}

function isEditableRouteStatus(value: string | null | undefined) {
  const normalized = value?.toLowerCase() ?? ''
  return ['draft', 'planned'].includes(normalized)
}

function formatDateTime(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return 'Not recorded'
  return date.toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
}

function buildRouteOptimizationPreview(stops: RouteStopSummaryResponse[]) {
  const currentOrder = [...stops].sort((left, right) => left.sequenceNumber - right.sequenceNumber)
  const suggestedOrder = [...currentOrder].sort((left, right) => {
    const leftScheduled = left.scheduledArrivalAt
    const rightScheduled = right.scheduledArrivalAt
    const leftHasSchedule = Boolean(leftScheduled)
    const rightHasSchedule = Boolean(rightScheduled)

    if (leftHasSchedule !== rightHasSchedule) {
      return leftHasSchedule ? -1 : 1
    }

    if (leftScheduled && rightScheduled && leftScheduled !== rightScheduled) {
      return leftScheduled.localeCompare(rightScheduled)
    }

    if (left.sequenceNumber !== right.sequenceNumber) {
      return left.sequenceNumber - right.sequenceNumber
    }

    return left.stopKey.localeCompare(right.stopKey)
  })

  const outOfOrderCount = currentOrder.filter((stop, index) => stop.stopId !== suggestedOrder[index]?.stopId).length
  const scheduledCount = currentOrder.filter((stop) => Boolean(stop.scheduledArrivalAt)).length
  const hasRecommendation = currentOrder.length > 1 && outOfOrderCount > 0
  const summary = hasRecommendation
    ? `${outOfOrderCount} stop(s) can be re-ordered to better match scheduled arrivals.`
    : currentOrder.length > 1
      ? 'Current stop order already matches scheduled arrivals.'
      : 'Add more stops to preview route optimization.'

  return {
    currentOrder,
    suggestedOrder,
    outOfOrderCount,
    scheduledCount,
    hasRecommendation,
    summary,
  }
}

function actionLink(to: string, label: string, icon: ReactNode, primary = false) {
  return (
    <Link
      to={to}
      className={`inline-flex items-center gap-2 rounded-xl px-4 py-3 text-sm font-semibold ${
        primary
          ? 'bg-sky-500 text-[var(--color-text-primary)] hover:bg-sky-400'
          : 'border border-slate-800 bg-slate-900 text-white hover:border-sky-700'
      }`}
    >
      {icon}
      {label}
    </Link>
  )
}

function noSelection(title: string, text: string, to: string) {
  return (
    <div className="rounded-3xl border border-slate-800 bg-slate-950/70 p-8 text-center">
      <Route className="mx-auto h-10 w-10 text-sky-300" />
      <h1 className="mt-4 text-2xl font-bold text-white">{title}</h1>
      <p className="mt-2 text-sm text-slate-400">{text}</p>
      <Link
        to={to}
        className="mt-5 inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-[var(--color-text-primary)] hover:bg-sky-400"
      >
        Open drawer
      </Link>
    </div>
  )
}

function emptyOrList<T>(items: T[], emptyText: string, render: (item: T) => ReactNode) {
  if (items.length === 0) return <DetailEmptyState text={emptyText} />
  return <div className="space-y-3">{items.map(render)}</div>
}

function dispatchabilityLabel(outcome: string | null | undefined): string {
  switch (outcome) {
    case 'allow':
      return 'Dispatch ready'
    case 'warn':
      return 'Dispatch warning'
    case 'block':
      return 'Dispatch blocked'
    default:
      return outcome ?? 'Unknown'
  }
}

export function TripProfile({ state: s }: { state: RoutArrWorkspaceState }) {
  const detail = s.tripDetailQuery?.data ?? null
  const summary = (s.tripsQuery?.data ?? []).find((trip) => trip.tripId === s.selectedTripId)
    ?? (s.tripsQuery?.data ?? [])[0]
    ?? null
  const trip = detail ?? summary
  if (!trip) {
    return noSelection('No trip selected', 'Select or create a trip to view dispatch profile details.', '/trips/drawer')
  }

  const loads = detail?.loads ?? []
  const loadCount = detail?.loads.length ?? summary?.loadCount ?? 0
  const dispatchability = s.tripAssetDispatchabilityQuery?.data ?? null
  const unassigned = !trip.assignedDriverPersonId || !trip.vehicleRefKey
  const activeSupplierBlock =
    trip.dispatchBlocks?.find(
      (block) => block.blockType === 'supplier_readiness' && block.status === 'active',
    ) ?? null
  const blocked =
    ['cancelled'].includes(trip.dispatchStatus) ||
    unassigned ||
    Boolean(dispatchability?.isBlocking) ||
    Boolean(activeSupplierBlock)
  const rails: DetailRailSectionConfig[] = [
    {
      title: 'Supplier readiness',
      icon: <Package className="h-5 w-5" />,
      content: trip.supplierOrderId || trip.supplierReadinessStatusSnapshot || activeSupplierBlock ? (
        <div className="space-y-3">
          <div className="flex flex-wrap items-center gap-2">
            {trip.supplierOrderId ? (
              <DetailBadge label={`Supplier order ${trip.supplierOrderId}`} tone="info" />
            ) : null}
            {trip.brokerOrderId ? (
              <DetailBadge label={`Broker order ${trip.brokerOrderId}`} tone="neutral" />
            ) : null}
            <DetailBadge
              label={activeSupplierBlock ? 'Dispatch blocked' : trip.releasedForDispatchAt ? 'Released' : humanize(trip.supplierReadinessStatusSnapshot)}
              tone={activeSupplierBlock ? 'bad' : trip.releasedForDispatchAt ? 'good' : 'warn'}
            />
          </div>
          <div className="rounded-xl border border-slate-800 bg-slate-900 p-4 text-sm text-slate-300">
            <p className="font-semibold text-white">SupplyArr readiness snapshot</p>
            <p className="mt-2">
              {trip.supplierReadinessStatusSnapshot
                ? humanize(trip.supplierReadinessStatusSnapshot)
                : 'No supplier-readiness snapshot recorded yet.'}
            </p>
            <p className="mt-2 text-xs text-[var(--color-text-muted)]">
              {trip.supplierQuantityReadySnapshot != null || trip.supplierOrderedQuantitySnapshot != null
                ? `${trip.supplierQuantityReadySnapshot ?? 0} of ${trip.supplierOrderedQuantitySnapshot ?? 0} ready`
                : 'Quantity snapshot unavailable'}
            </p>
            {activeSupplierBlock ? (
              <p className="mt-2 text-xs text-amber-200">
                Active block reason: {humanize(activeSupplierBlock.blockReason)}
              </p>
            ) : null}
            {trip.dispatchOverrideReason ? (
              <p className="mt-2 text-xs text-amber-200">
                Override reason: {trip.dispatchOverrideReason}
              </p>
            ) : null}
          </div>
        </div>
      ) : (
        <DetailEmptyState text="No linked supplier order or supplier-readiness snapshot." />
      ),
    },
    {
      title: 'Loads',
      icon: <ClipboardList className="h-5 w-5" />,
      content: emptyOrList(loads.slice(0, 4), 'No loads loaded for this trip.', (load) => (
        <div key={load.loadId} className="rounded-xl border border-slate-800 bg-slate-900 p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <h3 className="font-semibold text-white">{load.loadKey}</h3>
              <p className="mt-1 text-xs text-slate-400">{load.originLabel} to {load.destinationLabel}</p>
            </div>
            <DetailBadge label={humanize(load.status)} tone={statusTone(load.status)} />
          </div>
        </div>
      )),
    },
  ]

  return (
    <ProfileDetailsLayout
      testId="routarr-trip-profile"
      backLabel="Trips"
      backTo="/trips/drawer"
      breadcrumbs={[trip.tripNumber, trip.title]}
      icon={<Truck className="h-9 w-9" />}
      title={trip.title}
      subtitle={<span>{trip.vehicleRefKey ?? 'No vehicle'} - Driver {trip.assignedDriverPersonId ?? 'unassigned'}</span>}
      badges={[
        { label: trip.tripNumber, tone: 'info' },
        { label: humanize(trip.dispatchStatus), tone: statusTone(trip.dispatchStatus) },
        activeSupplierBlock ? { label: 'Supplier blocked', tone: 'bad' } : null,
        { label: `${loadCount} loads`, tone: 'neutral' },
      ].filter(Boolean) as Array<{ label: string; tone: DetailTone }>}
      actions={<>{actionLink(`/trips/${trip.tripId}`, 'Open trip workspace', <Play className="h-4 w-4" />, true)}</>}
      metrics={[
        { label: 'Dispatch state', value: humanize(trip.dispatchStatus), hint: `Created ${formatDate(trip.createdAt)}`, icon: <Navigation className="h-5 w-5" />, tone: statusTone(trip.dispatchStatus) },
        { label: 'Driver', value: trip.assignedDriverPersonId ? 'Assigned' : 'Open', hint: trip.assignedDriverPersonId ?? 'No driver assignment', icon: <UserCheck className="h-5 w-5" />, tone: trip.assignedDriverPersonId ? 'good' : 'warn' },
        { label: 'Vehicle', value: trip.vehicleRefKey ?? 'Open', hint: 'Equipment reference', icon: <Truck className="h-5 w-5" />, tone: trip.vehicleRefKey ? 'good' : 'warn' },
        {
          label: 'Supplier readiness',
          value: activeSupplierBlock ? 'Blocked' : trip.releasedForDispatchAt ? 'Released' : trip.supplierOrderId ? humanize(trip.supplierReadinessStatusSnapshot) : 'No link',
          hint: trip.supplierOrderId
            ? `${trip.supplierQuantityReadySnapshot ?? 0} of ${trip.supplierOrderedQuantitySnapshot ?? 0} ready`
            : 'No linked SupplyArr supplier order',
          icon: <Package className="h-5 w-5" />,
          tone: activeSupplierBlock ? 'bad' : trip.releasedForDispatchAt ? 'good' : trip.supplierOrderId ? 'warn' : 'neutral',
        },
        { label: 'Loads', value: loadCount, hint: 'Trip load plan', icon: <ClipboardList className="h-5 w-5" />, tone: 'info' },
      ]}
      tabs={['Overview', 'Loads', 'Driver', 'Vehicle', 'Proofs', 'DVIR', 'History']}
      snapshotTitle="Trip snapshot"
      snapshotSubtitle="Dispatch identity, driver and vehicle assignment, load count, timing, and lifecycle status."
      snapshotFields={[
        { label: 'Trip ID', value: trip.tripId, source: 'RoutArr record' },
        { label: 'Trip number', value: trip.tripNumber, source: 'Trip registry' },
        { label: 'Description', value: detail?.description ?? 'Not recorded', source: 'Trip plan' },
        { label: 'Driver', value: trip.assignedDriverPersonId ?? 'Unassigned', source: 'StaffArr personId' },
        { label: 'Vehicle', value: trip.vehicleRefKey ?? 'Unassigned', source: 'MaintainArr asset ref' },
        { label: 'Supplier order', value: trip.supplierOrderId ?? 'Not linked', source: 'SupplyArr reference' },
        { label: 'Broker order', value: trip.brokerOrderId ?? 'Not linked', source: 'OrdArr reference snapshot' },
        { label: 'Supplier readiness', value: humanize(trip.supplierReadinessStatusSnapshot), source: 'SupplyArr readiness snapshot' },
        { label: 'Supplier quantity ready', value: trip.supplierQuantityReadySnapshot ?? 'Not recorded', source: 'SupplyArr quantity snapshot' },
        { label: 'Supplier ordered quantity', value: trip.supplierOrderedQuantitySnapshot ?? 'Not recorded', source: 'SupplyArr quantity snapshot' },
        { label: 'Released for dispatch', value: formatDateTime(trip.releasedForDispatchAt), source: 'RoutArr release audit' },
        { label: 'Override reason', value: trip.dispatchOverrideReason ?? 'Not recorded', source: 'RoutArr override audit' },
        { label: 'Scheduled start', value: formatDate(trip.scheduledStartAt), source: 'Dispatch plan' },
        { label: 'Scheduled end', value: formatDate(trip.scheduledEndAt), source: 'Dispatch plan' },
        { label: 'Started', value: formatDate(trip.startedAt), source: 'Execution record' },
        { label: 'Completed', value: formatDate(trip.completedAt), source: 'Execution record' },
      ]}
      mainContent={(
        <div className="space-y-5">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Load plan</h3>
            <div className="mt-4">
              {emptyOrList(loads.slice(0, 5), 'Select a trip with loaded detail to view load legs.', (load) => (
                <div key={load.loadId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                  <h4 className="font-semibold text-white">{load.description || load.loadKey}</h4>
                  <p className="mt-1 text-sm text-sky-100/75">{load.originLabel} to {load.destinationLabel}</p>
                </div>
              ))}
            </div>
          </section>
          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Dispatch readiness</h3>
            {s.tripAssetDispatchabilityQuery.isLoading ? (
              <p className="mt-3 text-sm text-slate-400">Loading asset dispatchability…</p>
            ) : dispatchability ? (
              <div className="mt-4 space-y-3 text-sm text-slate-300">
                <div className="flex flex-wrap items-center gap-2">
                  <DetailBadge
                    label={dispatchabilityLabel(dispatchability.outcome)}
                    tone={dispatchability.isBlocking ? 'bad' : dispatchability.outcome === 'warn' ? 'warn' : 'good'}
                  />
                  <span className="text-xs text-[var(--color-text-muted)]">
                    {dispatchability.vehicleRefKey ?? 'Unlinked vehicle'} · {dispatchability.reasonCode}
                  </span>
                </div>
                <p>{dispatchability.message}</p>
                {dispatchability.maintainArr ? (
                  <div className="rounded-xl border border-slate-800 bg-slate-950/80 p-4 text-xs text-slate-400">
                    <p className="text-slate-200">
                      MaintainArr asset {dispatchability.maintainArr.assetTag} is{' '}
                      {humanize(dispatchability.maintainArr.readinessStatus)}
                    </p>
                    <p className="mt-1">
                      Basis: {humanize(dispatchability.maintainArr.readinessBasis)} · Blockers{' '}
                      {dispatchability.maintainArr.blockerCount}
                    </p>
                    {dispatchability.maintainArr.primaryBlockerMessage ? (
                      <p className="mt-1 text-amber-200">{dispatchability.maintainArr.primaryBlockerMessage}</p>
                    ) : null}
                  </div>
                ) : (
                  <p className="text-xs text-[var(--color-text-muted)]">MaintainArr dispatchability details are unavailable.</p>
                )}
              </div>
            ) : (
              <p className="mt-3 text-sm text-slate-400">
                Select a trip with a vehicle to review MaintainArr dispatch readiness.
              </p>
            )}
          </section>
        </div>
      )}
      decisionTitle="Dispatch decision"
      decisionBadge={{ label: blocked ? 'Needs assignment' : 'Dispatchable', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <AlertTriangle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Trip needs dispatch attention' : 'Trip can proceed through dispatch'}
      decisionDetail={blocked ? 'Driver, vehicle, or lifecycle status must be resolved before normal dispatch execution.' : 'Driver, vehicle, and trip status support normal dispatch execution.'}
      allowedChecks={[Boolean(trip.assignedDriverPersonId), Boolean(trip.vehicleRefKey), trip.dispatchStatus !== 'cancelled'].filter(Boolean).length}
      blockedChecks={[!trip.assignedDriverPersonId, !trip.vehicleRefKey, trip.dispatchStatus === 'cancelled'].filter(Boolean).length}
      railSections={rails}
    />
  )
}

export function RouteProfile({ state: s }: { state: RoutArrWorkspaceState }) {
  const detail = s.routeDetailQuery?.data ?? null
  const summary = (s.routesQuery?.data ?? []).find((route) => route.routeId === s.selectedRouteId)
    ?? (s.routesQuery?.data ?? [])[0]
    ?? null
  const route = detail ?? summary
  const [selectedGeofenceStopId, setSelectedGeofenceStopId] = useState('')
  const [reportedLatitude, setReportedLatitude] = useState('')
  const [reportedLongitude, setReportedLongitude] = useState('')
  if (!route) {
    return noSelection('No route selected', 'Select or create a route to view stop progression and trip linkage.', '/routes/drawer')
  }

  const stops = detail?.stops ?? []
  const stopCount = detail?.stops.length ?? summary?.stopCount ?? 0
  const pendingStops = stops.filter((stop) => stop.stopStatus === 'pending')
  const completedStops = stops.filter((stop) => stop.stopStatus === 'completed')
  const optimizationPreview = detail ? buildRouteOptimizationPreview(detail.stops) : null
  const canOptimize = Boolean(
    detail && isEditableRouteStatus(route.routeStatus) && optimizationPreview?.hasRecommendation,
  )
  const selectedGeofenceStop = stops.find((stop) => stop.stopId === selectedGeofenceStopId) ?? null
  const blocked = route.routeStatus === 'cancelled' || !route.tripId

  return (
    <ProfileDetailsLayout
      testId="routarr-route-profile"
      backLabel="Routes"
      backTo="/routes/drawer"
      breadcrumbs={[route.routeNumber, route.title]}
      icon={<MapPinned className="h-9 w-9" />}
      title={route.title}
      subtitle={<span>{route.tripId ? `Linked trip ${route.tripId}` : 'No linked trip'} - {stopCount} stops</span>}
      badges={[
        { label: route.routeNumber, tone: 'info' },
        { label: humanize(route.routeStatus), tone: statusTone(route.routeStatus) },
      ]}
      actions={<>{actionLink('/routes/drawer', 'Edit route', <Route className="h-4 w-4" />)}</>}
      metrics={[
        { label: 'Route state', value: humanize(route.routeStatus), hint: `Created ${formatDate(route.createdAt)}`, icon: <Navigation className="h-5 w-5" />, tone: statusTone(route.routeStatus) },
        { label: 'Stops', value: stopCount, hint: `${pendingStops.length} pending`, icon: <MapPinned className="h-5 w-5" />, tone: 'info' },
        { label: 'Completed', value: completedStops.length, hint: 'Completed stops', icon: <CheckCircle2 className="h-5 w-5" />, tone: 'good' },
        { label: 'Trip link', value: route.tripId ? 'Linked' : 'Open', hint: route.tripId ?? 'No linked trip', icon: <Truck className="h-5 w-5" />, tone: route.tripId ? 'good' : 'warn' },
      ]}
      tabs={['Overview', 'Stops', 'Trip Link', 'Proofs', 'Exceptions', 'History']}
      snapshotTitle="Route snapshot"
      snapshotSubtitle="Route identity, trip linkage, stop plan, progression, timing, and lifecycle state."
      snapshotFields={[
        { label: 'Route ID', value: route.routeId, source: 'RoutArr record' },
        { label: 'Route number', value: route.routeNumber, source: 'Route registry' },
        { label: 'Description', value: detail?.description ?? 'Not recorded', source: 'Route plan' },
        { label: 'Trip', value: route.tripId ?? 'Not linked', source: 'Trip linkage' },
        { label: 'Stop count', value: stopCount, source: 'Route stops' },
        { label: 'Status', value: humanize(route.routeStatus), source: 'Lifecycle state' },
        { label: 'Activated', value: formatDate(route.activatedAt), source: 'Dispatch record' },
        { label: 'Completed', value: formatDate(route.completedAt), source: 'Execution record' },
        { label: 'Updated', value: formatDate(route.updatedAt), source: 'Audit trail' },
      ]}
      mainContent={(
        <div className="space-y-5">
          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="text-lg font-bold text-white">Stop progression</h3>
            <div className="mt-4">
              {emptyOrList(stops.slice(0, 6), 'No stops loaded for this route.', (stop) => {
                const hasGeofenceAnchor = stop.geofenceAnchorLatitude != null && stop.geofenceAnchorLongitude != null
                const osmLink = buildOpenStreetMapUrl({
                  addressQuery: stop.addressLabel,
                  latitude: stop.geofenceAnchorLatitude,
                  longitude: stop.geofenceAnchorLongitude,
                })

                return (
                  <div
                    key={stop.stopId}
                    className={`rounded-xl border border-slate-800 bg-slate-950/80 p-4 ${
                      selectedGeofenceStopId === stop.stopId ? 'ring-1 ring-sky-500' : ''
                    }`}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <h4 className="font-semibold text-white">{stop.label}</h4>
                        <p className="mt-1 text-sm text-sky-100/75">{stop.addressLabel}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {stop.stopType} · {stop.stopKey}
                        </p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          Geofence anchor:{' '}
                          {hasGeofenceAnchor
                            ? `${stop.geofenceAnchorLatitude!.toFixed(4)}, ${stop.geofenceAnchorLongitude!.toFixed(4)}`
                            : 'Not configured'}
                          {stop.geofenceRadiusMeters != null ? ` · radius ${stop.geofenceRadiusMeters}m` : ''}
                        </p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                          Latest geofence:{' '}
                          {stop.lastGeofenceResult
                            ? `${humanize(stop.lastGeofenceResult)} · ${formatGeofenceDistance(stop.lastGeofenceDistanceMeters)}`
                            : 'No checks yet'}
                        </p>
                        {stop.lastGeofenceResult ? (
                          <div className="mt-2">
                            <DetailBadge
                              label={humanize(stop.lastGeofenceResult)}
                              tone={geofenceTone(stop.lastGeofenceResult)}
                            />
                          </div>
                        ) : null}
                      </div>
                      <div className="flex flex-col items-end gap-2">
                        <DetailBadge label={humanize(stop.stopStatus)} tone={statusTone(stop.stopStatus)} />
                        {hasGeofenceAnchor ? (
                          <button
                            type="button"
                            className="rounded border border-slate-700 px-2 py-1 text-xs text-slate-200 hover:border-sky-500"
                            onClick={() => {
                              setSelectedGeofenceStopId(stop.stopId)
                              setReportedLatitude(stop.geofenceAnchorLatitude?.toString() ?? '')
                              setReportedLongitude(stop.geofenceAnchorLongitude?.toString() ?? '')
                            }}
                          >
                            Check geofence
                          </button>
                        ) : null}
                        {osmLink ? (
                          <a
                            href={osmLink.url}
                            target="_blank"
                            rel="noreferrer"
                            className="text-xs text-sky-300 underline-offset-2 hover:text-sky-200 hover:underline"
                          >
                            {osmLink.source === 'address' ? 'Open address' : 'Open coordinates'}
                          </a>
                        ) : null}
                      </div>
                    </div>
                  </div>
                )
              })}
            </div>
          </section>

          <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
            <h3 className="flex items-center gap-2 text-lg font-bold text-white">
              <MapPinned className="h-5 w-5" />
              Geofence check
            </h3>
            {selectedGeofenceStop ? (
              <div className="mt-4 space-y-4">
                <p className="text-sm text-slate-300">
                  Checking {selectedGeofenceStop.label} against its configured anchor and radius.
                </p>
                <div className="grid gap-3 md:grid-cols-2">
                  <label className="text-sm text-slate-300" htmlFor="route-geofence-latitude">
                    Reported latitude
                    <input
                      id="route-geofence-latitude"
                      className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
                      value={reportedLatitude}
                      onChange={(event) => setReportedLatitude(event.target.value)}
                    />
                  </label>
                  <label className="text-sm text-slate-300" htmlFor="route-geofence-longitude">
                    Reported longitude
                    <input
                      id="route-geofence-longitude"
                      className="mt-1 w-full rounded border border-slate-600 bg-slate-950 px-3 py-2"
                      value={reportedLongitude}
                      onChange={(event) => setReportedLongitude(event.target.value)}
                    />
                  </label>
                </div>
                <OpenStreetMapCard
                  latitude={selectedGeofenceStop.geofenceAnchorLatitude}
                  longitude={selectedGeofenceStop.geofenceAnchorLongitude}
                  label={`${selectedGeofenceStop.label} stop address`}
                  addressQuery={selectedGeofenceStop.addressLabel}
                  heightClassName="h-72"
                  emptyMessage="This stop needs an address before OpenStreetMap search is available."
                />
                <button
                  type="button"
                  className="rounded bg-sky-600 px-3 py-2 text-sm font-semibold text-white hover:bg-sky-500 disabled:opacity-50"
                  disabled={
                    s.checkRouteStopGeofenceMutation.isPending ||
                    !reportedLatitude.trim() ||
                    !reportedLongitude.trim()
                  }
                  onClick={() =>
                    s.checkRouteStopGeofenceMutation.mutate({
                      stopId: selectedGeofenceStop.stopId,
                      reportedLatitude: Number(reportedLatitude),
                      reportedLongitude: Number(reportedLongitude),
                    })
                  }
                >
                  {s.checkRouteStopGeofenceMutation.isPending ? 'Checking…' : 'Run geofence check'}
                </button>
              </div>
            ) : (
              <DetailEmptyState text="Select a stop with a geofence anchor to run a GPS proximity check." />
            )}
          </section>
        </div>
      )}
      decisionTitle="Route decision"
      decisionBadge={{ label: blocked ? 'Setup needed' : 'Ready', tone: blocked ? 'warn' : 'good' }}
      decisionIcon={blocked ? <AlertTriangle className="h-5 w-5 text-amber-300" /> : <CheckCircle2 className="h-5 w-5 text-emerald-300" />}
      decisionSummary={blocked ? 'Route needs trip linkage or status review' : 'Route ready for execution'}
      decisionDetail={blocked ? 'Routes without a linked trip or with cancelled status need dispatch review.' : 'Linked trip and stop progression support normal route execution.'}
      allowedChecks={[Boolean(route.tripId), route.routeStatus !== 'cancelled', stopCount > 0].filter(Boolean).length}
      blockedChecks={[!route.tripId, route.routeStatus === 'cancelled'].filter(Boolean).length}
      railSections={[
        {
          title: 'Stop status',
          icon: <MapPinned className="h-5 w-5" />,
          content: (
            <div className="grid grid-cols-2 gap-3">
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <p className="text-xs text-slate-400">Pending</p>
                <p className="mt-2 text-xl font-bold text-white">{pendingStops.length}</p>
              </div>
              <div className="rounded-xl border border-slate-800 bg-slate-900 p-4">
                <p className="text-xs text-slate-400">Completed</p>
                <p className="mt-2 text-xl font-bold text-white">{completedStops.length}</p>
              </div>
            </div>
          ),
        },
        {
          title: 'Route optimization',
          icon: <Navigation className="h-5 w-5" />,
          content: optimizationPreview ? (
            <div className="space-y-3 text-sm text-slate-300">
              <div className="flex items-center gap-2">
                <DetailBadge
                  label={optimizationPreview.hasRecommendation ? 'Needs optimization' : 'Optimized'}
                  tone={optimizationPreview.hasRecommendation ? 'warn' : 'good'}
                />
                <span className="text-xs text-[var(--color-text-muted)]">
                  {optimizationPreview.scheduledCount} scheduled stop(s)
                </span>
              </div>
              <p className="text-xs text-slate-400">{optimizationPreview.summary}</p>
              {optimizationPreview.suggestedOrder.length > 0 ? (
                <ol className="space-y-2">
                  {optimizationPreview.suggestedOrder.map((stop, index) => (
                    <li key={stop.stopId} className="rounded-xl border border-slate-800 bg-slate-900 p-3 text-xs">
                      <p className="font-medium text-slate-100">
                        {index + 1}. {stop.label || stop.stopKey}
                      </p>
                      <p className="mt-1 text-slate-400">
                        {stop.stopKey} · {stop.stopType}
                      </p>
                      <p className="text-[var(--color-text-muted)]">
                        Scheduled {formatDateTime(stop.scheduledArrivalAt)}
                      </p>
                    </li>
                  ))}
                </ol>
              ) : (
                <DetailEmptyState text="No stops are available for optimization preview." />
              )}
              {canOptimize ? (
                <button
                  type="button"
                  className="rounded-md bg-sky-600 px-3 py-2 text-xs font-semibold text-white hover:bg-sky-500 disabled:opacity-50"
                  disabled={s.optimizeRouteMutation.isPending}
                  onClick={() => s.optimizeRouteMutation.mutate()}
                >
                  {s.optimizeRouteMutation.isPending ? 'Optimizing…' : 'Optimize stop order'}
                </button>
              ) : null}
            </div>
          ) : (
            <DetailEmptyState text="Select a route with stop detail to preview optimization." />
          ),
        },
      ]}
    />
  )
}
