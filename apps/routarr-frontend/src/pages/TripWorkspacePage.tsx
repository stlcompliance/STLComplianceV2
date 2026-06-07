import { useQuery } from '@tanstack/react-query'
import { Navigate, useParams, useSearchParams } from 'react-router-dom'
import { getMe } from '../api/client'
import { loadSession } from '../auth/sessionStorage'
import { canManageTrips, canPerformTrips } from '../auth/sessionStorage'
import { TripExecutionWorkspacePanel } from '../components/TripExecutionWorkspacePanel'

export function TripWorkspacePage() {
  const { tripId } = useParams<{ tripId: string }>()
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const meQuery = useQuery({
    queryKey: ['routarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  if (!session) {
    return <p className="text-sm text-slate-400">Loading trip…</p>
  }

  if (!tripId) {
    return <Navigate to="/trips" replace />
  }

  const roleKey = meQuery.data?.tenantRoleKey ?? 'tenant_member'
  const isPlatformAdmin = meQuery.data?.isPlatformAdmin ?? false

  return (
    <div className="mx-auto max-w-4xl" data-testid="trip-workspace">
      <TripExecutionWorkspacePanel
        accessToken={session.accessToken}
        tripId={tripId}
        canPerform={canPerformTrips(roleKey, isPlatformAdmin)}
        canManage={canManageTrips(roleKey, isPlatformAdmin)}
      />
    </div>
  )
}
