import { useQuery } from '@tanstack/react-query'
import { Link, Navigate, useParams, useSearchParams } from 'react-router-dom'
import { PageHeader } from '@stl/shared-ui'
import { getTrip } from '../api/client'
import { loadSession } from '../auth/sessionStorage'

export function TripWorkspacePage() {
  const { tripId } = useParams<{ tripId: string }>()
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const tripQuery = useQuery({
    queryKey: ['routarr-trip', session?.accessToken, tripId],
    queryFn: () => getTrip(session!.accessToken, tripId!),
    enabled: Boolean(session?.accessToken && tripId),
  })

  if (!session) {
    return <p className="text-sm text-slate-400">Loading trip…</p>
  }

  if (!tripId) {
    return <Navigate to="/" replace />
  }

  const trip = tripQuery.data

  return (
    <div className="mx-auto max-w-3xl space-y-6" data-testid="trip-workspace">
      <PageHeader title="Trip" subtitle={trip?.title ?? 'Loading trip…'} />
      <p className="text-sm">
        <Link to="/" className="text-teal-300 hover:text-teal-200">
          ← Back to dispatch workspace
        </Link>
      </p>
      {trip ? (
        <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-4 text-sm text-slate-200">
          <p>
            <span className="text-slate-400">Trip number:</span> {trip.tripNumber}
          </p>
          <p className="mt-2">
            <span className="text-slate-400">Dispatch status:</span> {trip.dispatchStatus}
          </p>
        </section>
      ) : null}
    </div>
  )
}
