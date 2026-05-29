import { useQuery } from '@tanstack/react-query'
import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
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
    <div className="mx-auto max-w-4xl space-y-6" data-testid="trip-workspace">
      <PageHeader
        title="Trip execution"
        subtitle="Assign, guide execution, capture proof, and close out transportation work."
      />
      <p className="text-sm">
        <Link to="/trips" className="text-teal-300 hover:text-teal-200">
          ← Back to trips
        </Link>
        {' · '}
        <Link to="/dispatch" className="text-teal-300 hover:text-teal-200">
          Dispatch board
        </Link>
      </p>
      <TripExecutionWorkspacePanel
        accessToken={session.accessToken}
        tripId={tripId}
        canPerform={canPerformTrips(roleKey, isPlatformAdmin)}
        canManage={canManageTrips(roleKey, isPlatformAdmin)}
      />
    </div>
  )
}
