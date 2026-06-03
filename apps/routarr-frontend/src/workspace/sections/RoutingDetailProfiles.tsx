import {
  AlertTriangle,
  CheckCircle2,
  ClipboardList,
  MapPinned,
  Navigation,
  Play,
  Route,
  Truck,
  UserCheck,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router-dom'
import {
  DetailBadge,
  DetailEmptyState,
  ProfileDetailsLayout,
  type DetailRailSectionConfig,
  type DetailTone,
} from '@stl/shared-ui'
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

function actionLink(to: string, label: string, icon: ReactNode, primary = false) {
  return (
    <Link
      to={to}
      className={`inline-flex items-center gap-2 rounded-xl px-4 py-3 text-sm font-semibold ${
        primary
          ? 'bg-sky-500 text-slate-950 hover:bg-sky-400'
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
        className="mt-5 inline-flex items-center gap-2 rounded-xl bg-sky-500 px-4 py-2 text-sm font-semibold text-slate-950 hover:bg-sky-400"
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
  const blocked = ['cancelled'].includes(trip.dispatchStatus) || unassigned || Boolean(dispatchability?.isBlocking)
  const rails: DetailRailSectionConfig[] = [
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
        { label: `${loadCount} loads`, tone: 'neutral' },
      ]}
      actions={<>{actionLink(`/trips/${trip.tripId}`, 'Open trip workspace', <Play className="h-4 w-4" />, true)}</>}
      metrics={[
        { label: 'Dispatch state', value: humanize(trip.dispatchStatus), hint: `Created ${formatDate(trip.createdAt)}`, icon: <Navigation className="h-5 w-5" />, tone: statusTone(trip.dispatchStatus) },
        { label: 'Driver', value: trip.assignedDriverPersonId ? 'Assigned' : 'Open', hint: trip.assignedDriverPersonId ?? 'No driver assignment', icon: <UserCheck className="h-5 w-5" />, tone: trip.assignedDriverPersonId ? 'good' : 'warn' },
        { label: 'Vehicle', value: trip.vehicleRefKey ?? 'Open', hint: 'Equipment reference', icon: <Truck className="h-5 w-5" />, tone: trip.vehicleRefKey ? 'good' : 'warn' },
        { label: 'Loads', value: loadCount, hint: 'Trip load plan', icon: <ClipboardList className="h-5 w-5" />, tone: 'info' },
      ]}
      tabs={['Overview', 'Loads', 'Driver', 'Vehicle', 'Proofs', 'DVIR', 'History']}
      snapshotTitle="Trip snapshot"
      snapshotSubtitle="Dispatch identity, driver and vehicle assignment, load count, timing, and lifecycle status."
      snapshotFields={[
        { label: 'Trip ID', value: trip.tripId, source: 'RoutArr source of truth' },
        { label: 'Trip number', value: trip.tripNumber, source: 'Trip registry' },
        { label: 'Description', value: detail?.description ?? 'Not recorded', source: 'Trip plan' },
        { label: 'Driver', value: trip.assignedDriverPersonId ?? 'Unassigned', source: 'StaffArr personId' },
        { label: 'Vehicle', value: trip.vehicleRefKey ?? 'Unassigned', source: 'MaintainArr asset ref' },
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
                  <span className="text-xs text-slate-500">
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
                  <p className="text-xs text-slate-500">MaintainArr dispatchability details are unavailable.</p>
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
  if (!route) {
    return noSelection('No route selected', 'Select or create a route to view stop progression and trip linkage.', '/routes/drawer')
  }

  const stops = detail?.stops ?? []
  const stopCount = detail?.stops.length ?? summary?.stopCount ?? 0
  const pendingStops = stops.filter((stop) => stop.stopStatus === 'pending')
  const completedStops = stops.filter((stop) => stop.stopStatus === 'completed')
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
        { label: 'Route ID', value: route.routeId, source: 'RoutArr source of truth' },
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
        <section className="rounded-2xl border border-slate-800 bg-slate-950/60 p-5">
          <h3 className="text-lg font-bold text-white">Stop progression</h3>
          <div className="mt-4">
            {emptyOrList(stops.slice(0, 6), 'No stops loaded for this route.', (stop) => (
              <div key={stop.stopId} className="rounded-xl border border-slate-800 bg-slate-950/80 p-4">
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <h4 className="font-semibold text-white">{stop.label}</h4>
                    <p className="mt-1 text-sm text-sky-100/75">{stop.addressLabel}</p>
                  </div>
                  <DetailBadge label={humanize(stop.stopStatus)} tone={statusTone(stop.stopStatus)} />
                </div>
              </div>
            ))}
          </div>
        </section>
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
      ]}
    />
  )
}
