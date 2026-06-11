import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canPerformTrips, canViewAllTrips } from '../../auth/sessionStorage'
import { RoutesPanel } from '../../components/RoutesPanel'

type Props = { state: RoutArrWorkspaceState }

export function StopsSection({ state }: Props) {
  const {
    roleKey,
    isPlatformAdmin,
    routesQuery,
    routeDetailQuery,
    selectedRouteId,
    selectedTripId,
    routeTitle,
    routeDescription,
    stopKey,
    stopLabel,
    stopAddress,
    stopType,
    stopScheduledArrivalAt,
    stopGeofenceAnchorLatitude,
    stopGeofenceAnchorLongitude,
    stopGeofenceRadiusMeters,
    linkRouteMutation,
    updateStopStatusMutation,
    setSelectedRouteId,
    setRouteTitle,
    setRouteDescription,
    setStopKey,
    setStopLabel,
    setStopAddress,
    setStopType,
    setStopScheduledArrivalAt,
    setStopGeofenceAnchorLatitude,
    setStopGeofenceAnchorLongitude,
    setStopGeofenceRadiusMeters,
  } = state

  return (
    <div className="mt-8">
      <RoutesPanel
        mode="details"
        canCreate={false}
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
        stopScheduledArrivalAt={stopScheduledArrivalAt}
        stopGeofenceAnchorLatitude={stopGeofenceAnchorLatitude}
        stopGeofenceAnchorLongitude={stopGeofenceAnchorLongitude}
        stopGeofenceRadiusMeters={stopGeofenceRadiusMeters}
        isLoading={routesQuery.isLoading}
        isDetailLoading={routeDetailQuery.isLoading}
        isCreating={false}
        isLinking={linkRouteMutation.isPending}
        isUpdatingStop={updateStopStatusMutation.isPending}
        onSelectedRouteIdChange={setSelectedRouteId}
        onRouteTitleChange={setRouteTitle}
        onRouteDescriptionChange={setRouteDescription}
        onStopKeyChange={setStopKey}
        onStopLabelChange={setStopLabel}
        onStopAddressChange={setStopAddress}
        onStopTypeChange={setStopType}
        onStopScheduledArrivalAtChange={setStopScheduledArrivalAt}
        onStopGeofenceAnchorLatitudeChange={setStopGeofenceAnchorLatitude}
        onStopGeofenceAnchorLongitudeChange={setStopGeofenceAnchorLongitude}
        onStopGeofenceRadiusMetersChange={setStopGeofenceRadiusMeters}
        onCreateRoute={() => {}}
        onLinkTrip={() => linkRouteMutation.mutate()}
        onUpdateStopStatus={(stopId, status) => updateStopStatusMutation.mutate({ stopId, status })}
      />
    </div>
  )
}
