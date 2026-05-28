import type { RoutArrWorkspaceState } from '../useRoutArrWorkspaceState'
import { canCreateTrips, canPerformTrips, canViewAllTrips } from '../../auth/sessionStorage'
import { RoutesPanel } from '../../components/RoutesPanel'

type Props = { state: RoutArrWorkspaceState }

export function RoutesSection({ state }: Props) {
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
    createRouteMutation,
    linkRouteMutation,
    updateStopStatusMutation,
    setSelectedRouteId,
    setRouteTitle,
    setRouteDescription,
    setStopKey,
    setStopLabel,
    setStopAddress,
    setStopType,
  } = state

  return (
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
  )
}
