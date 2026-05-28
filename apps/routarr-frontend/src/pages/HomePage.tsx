import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import {
  assignTripDriver,
  createRoute,
  createTrip,
  getMe,
  getRoute,
  getRoutes,
  getTrip,
  getTrips,
  linkRouteTrip,
  updateRouteStopStatus,
  updateTripStatus,
} from '../api/client'
import {
  canAssignDrivers,
  canCreateTrips,
  canManageTrips,
  canManageNotificationSettings,
  canPerformTrips,
  canViewAllTrips,
  canManageDriverAvailability,
  canManageEquipmentAvailability,
  loadSession,
} from '../auth/sessionStorage'
import { NotificationSettingsPanel } from '../components/NotificationSettingsPanel'
import { TripsPanel } from '../components/TripsPanel'
import { RoutesPanel } from '../components/RoutesPanel'
import { DispatchBoardPanel } from '../components/DispatchBoardPanel'
import { RouteCalendarPanel } from '../components/RouteCalendarPanel'
import { DriverAvailabilityPanel } from '../components/DriverAvailabilityPanel'
import { EquipmentAvailabilityPanel } from '../components/EquipmentAvailabilityPanel'
import { DispatchAssignmentPanel } from '../components/DispatchAssignmentPanel'
import { BulkDispatchPanel } from '../components/BulkDispatchPanel'
import { DispatchCloseoutPanel } from '../components/DispatchCloseoutPanel'

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const queryClient = useQueryClient()
  const [statusFilter, setStatusFilter] = useState('')
  const [selectedTripId, setSelectedTripId] = useState('')
  const [tripTitle, setTripTitle] = useState('')
  const [tripDescription, setTripDescription] = useState('')
  const [vehicleRefKey, setVehicleRefKey] = useState('')
  const [driverPersonId, setDriverPersonId] = useState('')
  const [loadKey, setLoadKey] = useState('')
  const [loadOrigin, setLoadOrigin] = useState('')
  const [loadDestination, setLoadDestination] = useState('')
  const [selectedRouteId, setSelectedRouteId] = useState('')
  const [routeTitle, setRouteTitle] = useState('')
  const [routeDescription, setRouteDescription] = useState('')
  const [stopKey, setStopKey] = useState('')
  const [stopLabel, setStopLabel] = useState('')
  const [stopAddress, setStopAddress] = useState('')
  const [stopType, setStopType] = useState('pickup')
  const [boardScope, setBoardScope] = useState<'daily' | 'weekly'>('daily')

  const meQuery = useQuery({
    queryKey: ['routarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const tripsQuery = useQuery({
    queryKey: ['routarr-trips', session?.accessToken, statusFilter],
    queryFn: () => getTrips(session!.accessToken, statusFilter || undefined),
    enabled: Boolean(session?.accessToken),
  })

  const tripDetailQuery = useQuery({
    queryKey: ['routarr-trip', session?.accessToken, selectedTripId],
    queryFn: () => getTrip(session!.accessToken, selectedTripId),
    enabled: Boolean(session?.accessToken && selectedTripId),
  })

  const routesQuery = useQuery({
    queryKey: ['routarr-routes', session?.accessToken, selectedTripId],
    queryFn: () => getRoutes(session!.accessToken, selectedTripId || undefined),
    enabled: Boolean(session?.accessToken),
  })

  const routeDetailQuery = useQuery({
    queryKey: ['routarr-route', session?.accessToken, selectedRouteId],
    queryFn: () => getRoute(session!.accessToken, selectedRouteId),
    enabled: Boolean(session?.accessToken && selectedRouteId),
  })

  useEffect(() => {
    if (!driverPersonId && session?.personId) {
      setDriverPersonId(session.personId)
    }
  }, [driverPersonId, session?.personId])

  const createTripMutation = useMutation({
    mutationFn: () =>
      createTrip(session!.accessToken, {
        title: tripTitle,
        description: tripDescription,
        vehicleRefKey: vehicleRefKey || null,
        loads: loadKey.trim()
          ? [
              {
                loadKey,
                description: `${loadOrigin} to ${loadDestination}`.trim(),
                loadType: 'general',
                sequenceNumber: 1,
                originLabel: loadOrigin,
                destinationLabel: loadDestination,
              },
            ]
          : null,
      }),
    onSuccess: async (created) => {
      setTripTitle('')
      setTripDescription('')
      setVehicleRefKey('')
      setLoadKey('')
      setLoadOrigin('')
      setLoadDestination('')
      setSelectedTripId(created.tripId)
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
    },
  })

  const assignDriverMutation = useMutation({
    mutationFn: () =>
      assignTripDriver(session!.accessToken, selectedTripId, {
        driverPersonId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip', session?.accessToken, selectedTripId] })
    },
  })

  const updateStatusMutation = useMutation({
    mutationFn: ({ tripId, status }: { tripId: string; status: string }) =>
      updateTripStatus(session!.accessToken, tripId, { dispatchStatus: status }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-trips'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-trip', session?.accessToken, selectedTripId] })
    },
  })

  const createRouteMutation = useMutation({
    mutationFn: () =>
      createRoute(session!.accessToken, {
        title: routeTitle,
        description: routeDescription,
        tripId: selectedTripId || null,
        stops: stopKey.trim()
          ? [
              {
                stopKey,
                label: stopLabel,
                addressLabel: stopAddress,
                stopType,
                sequenceNumber: 1,
              },
            ]
          : null,
      }),
    onSuccess: async (created) => {
      setRouteTitle('')
      setRouteDescription('')
      setStopKey('')
      setStopLabel('')
      setStopAddress('')
      setSelectedRouteId(created.routeId)
      await queryClient.invalidateQueries({ queryKey: ['routarr-routes'] })
    },
  })

  const linkRouteMutation = useMutation({
    mutationFn: () =>
      linkRouteTrip(session!.accessToken, selectedRouteId, {
        tripId: selectedTripId,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-routes'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-route', session?.accessToken, selectedRouteId] })
    },
  })

  const updateStopStatusMutation = useMutation({
    mutationFn: ({ stopId, status }: { stopId: string; status: string }) =>
      updateRouteStopStatus(session!.accessToken, stopId, { stopStatus: status }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['routarr-routes'] })
      await queryClient.invalidateQueries({ queryKey: ['routarr-route', session?.accessToken, selectedRouteId] })
    },
  })

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading dispatch workspace…</p>
  }

  const me = meQuery.data
  const roleKey = me.tenantRoleKey ?? 'tenant_member'
  const isPlatformAdmin = me.isPlatformAdmin ?? false

  return (
    <div className="mx-auto max-w-6xl space-y-6">
      <PageHeader
        title="Dispatch workspace"
        subtitle={`${me.displayName} · ${me.tenantRoleKey ?? 'member'}`}
      />

      <DispatchBoardPanel
        accessToken={session.accessToken}
        scope={boardScope}
        onScopeChange={setBoardScope}
      />

      {canManageNotificationSettings(roleKey, isPlatformAdmin) ? (
        <div className="mt-8">
          <NotificationSettingsPanel
            accessToken={session.accessToken}
            canManage={canManageNotificationSettings(roleKey, isPlatformAdmin)}
          />
        </div>
      ) : null}

      {canAssignDrivers(roleKey, isPlatformAdmin) ? (
        <div className="mt-8">
          <DispatchCloseoutPanel
            accessToken={session.accessToken}
            scope={boardScope}
            canAssign={canAssignDrivers(roleKey, isPlatformAdmin)}
          />
        </div>
      ) : null}

      {canAssignDrivers(roleKey, isPlatformAdmin) ? (
        <div className="mt-8">
          <BulkDispatchPanel
            accessToken={session.accessToken}
            canAssign={canAssignDrivers(roleKey, isPlatformAdmin)}
          />
        </div>
      ) : null}

      {canAssignDrivers(roleKey, isPlatformAdmin) ? (
        <div className="mt-8">
          <DispatchAssignmentPanel
            accessToken={session.accessToken}
            scope={boardScope}
            canAssign={canAssignDrivers(roleKey, isPlatformAdmin)}
          />
        </div>
      ) : null}

      <div className="mt-8">
        <RouteCalendarPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
        />
      </div>

      <div className="mt-8">
        <DriverAvailabilityPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
          canManage={canManageDriverAvailability(roleKey, isPlatformAdmin)}
          sessionPersonId={session.personId}
        />
      </div>

      <div className="mt-8">
        <EquipmentAvailabilityPanel
          accessToken={session.accessToken}
          scope={boardScope}
          onScopeChange={setBoardScope}
          canManage={canManageEquipmentAvailability(roleKey, isPlatformAdmin)}
        />
      </div>

      <div className="mt-8">
      <TripsPanel
        canCreate={canCreateTrips(roleKey, isPlatformAdmin)}
        canAssign={canAssignDrivers(roleKey, isPlatformAdmin)}
        canPerform={canPerformTrips(roleKey, isPlatformAdmin)}
        canManage={canManageTrips(roleKey, isPlatformAdmin)}
        viewAllTrips={canViewAllTrips(roleKey, isPlatformAdmin)}
        sessionPersonId={session.personId}
        trips={tripsQuery.data ?? []}
        selectedTrip={tripDetailQuery.data ?? null}
        selectedTripId={selectedTripId}
        tripTitle={tripTitle}
        tripDescription={tripDescription}
        vehicleRefKey={vehicleRefKey}
        driverPersonId={driverPersonId}
        loadKey={loadKey}
        loadOrigin={loadOrigin}
        loadDestination={loadDestination}
        statusFilter={statusFilter}
        isLoading={tripsQuery.isLoading}
        isDetailLoading={tripDetailQuery.isLoading}
        isCreating={createTripMutation.isPending}
        isAssigning={assignDriverMutation.isPending}
        isUpdatingStatus={updateStatusMutation.isPending}
        onSelectedTripIdChange={setSelectedTripId}
        onTripTitleChange={setTripTitle}
        onTripDescriptionChange={setTripDescription}
        onVehicleRefKeyChange={setVehicleRefKey}
        onDriverPersonIdChange={setDriverPersonId}
        onLoadKeyChange={setLoadKey}
        onLoadOriginChange={setLoadOrigin}
        onLoadDestinationChange={setLoadDestination}
        onStatusFilterChange={setStatusFilter}
        onCreateTrip={() => createTripMutation.mutate()}
        onAssignDriver={() => assignDriverMutation.mutate()}
        onUpdateStatus={(tripId, status) => {
          if (status !== tripDetailQuery.data?.dispatchStatus) {
            updateStatusMutation.mutate({ tripId, status })
          }
        }}
      />
      </div>

      <div className="mt-8">
        <RoutesPanel
          canCreate={canCreateTrips(roleKey, isPlatformAdmin)}
          canPerform={canPerformTrips(roleKey, isPlatformAdmin)}
          viewAllRoutes={canViewAllTrips(roleKey, isPlatformAdmin)}
          routes={routesQuery.data ?? []}
          selectedRoute={routeDetailQuery.data ?? null}
          selectedRouteId={selectedRouteId}
          selectedTripId={selectedTripId}
          routeTitle={routeTitle}
          routeDescription={routeDescription}
          stopKey={stopKey}
          stopLabel={stopLabel}
          stopAddress={stopAddress}
          stopType={stopType}
          isLoading={routesQuery.isLoading}
          isDetailLoading={routeDetailQuery.isLoading}
          isCreating={createRouteMutation.isPending}
          isLinking={linkRouteMutation.isPending}
          isUpdatingStop={updateStopStatusMutation.isPending}
          onSelectedRouteIdChange={setSelectedRouteId}
          onRouteTitleChange={setRouteTitle}
          onRouteDescriptionChange={setRouteDescription}
          onStopKeyChange={setStopKey}
          onStopLabelChange={setStopLabel}
          onStopAddressChange={setStopAddress}
          onStopTypeChange={setStopType}
          onCreateRoute={() => createRouteMutation.mutate()}
          onLinkTrip={() => linkRouteMutation.mutate()}
          onUpdateStopStatus={(stopId, status) => updateStopStatusMutation.mutate({ stopId, status })}
        />
      </div>
    </div>
  )
}
