import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import {
  checkAssetDispatchability,
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
import { loadSession } from '../auth/sessionStorage'

export function useRoutArrWorkspaceState() {

  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const handoffRedirect = handoff
    ? <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
    : null

  const session = loadSession()
  const accessToken = session?.accessToken ?? ''
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
  const [apiError, setApiError] = useState<string | null>(null)

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

  const selectedTripVehicleRefKey =
    tripDetailQuery.data?.vehicleRefKey ?? tripsQuery.data?.find((trip) => trip.tripId === selectedTripId)?.vehicleRefKey ?? null

  const tripAssetDispatchabilityQuery = useQuery({
    queryKey: ['routarr-trip-asset-dispatchability', session?.accessToken, selectedTripId, selectedTripVehicleRefKey],
    queryFn: () =>
      checkAssetDispatchability(session!.accessToken, {
        vehicleRefKey: selectedTripVehicleRefKey ?? undefined,
      }),
    enabled: Boolean(session?.accessToken && selectedTripId && selectedTripVehicleRefKey),
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

  const me = meQuery.data
  const roleKey = me?.tenantRoleKey ?? 'tenant_member'
  const isPlatformAdmin = me?.isPlatformAdmin ?? false
  const ready = Boolean(session && me)
  const loadingMessage = 'Loading dispatch workspace…'

  return {
    handoffRedirect,
    ready,
    loadingMessage,
    me: me!,
    session: session!,
    accessToken,
    apiError,
    searchParams,
    statusFilter,
    selectedTripId,
    tripTitle,
    tripDescription,
    vehicleRefKey,
    driverPersonId,
    loadKey,
    loadOrigin,
    loadDestination,
    selectedRouteId,
    routeTitle,
    routeDescription,
    stopKey,
    stopLabel,
    stopAddress,
    stopType,
    boardScope,
    setBoardScope,
    setStatusFilter,
    setSelectedTripId,
    setTripTitle,
    setTripDescription,
    setVehicleRefKey,
    setDriverPersonId,
    setLoadKey,
    setLoadOrigin,
    setLoadDestination,
    setSelectedRouteId,
    setRouteTitle,
    setRouteDescription,
    setStopKey,
    setStopLabel,
    setStopAddress,
    setStopType,
    setApiError,
    meQuery,
    tripsQuery,
      tripDetailQuery,
      tripAssetDispatchabilityQuery,
      routesQuery,
      routeDetailQuery,
    createTripMutation,
    assignDriverMutation,
    updateStatusMutation,
    createRouteMutation,
    linkRouteMutation,
    updateStopStatusMutation,
    roleKey,
    isPlatformAdmin,
  }
}

export type RoutArrWorkspaceState = ReturnType<typeof useRoutArrWorkspaceState>
