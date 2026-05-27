import { useQuery } from '@tanstack/react-query'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import { getMe, StaffArrApiError } from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()

  const meQuery = useQuery({
    queryKey: ['staffarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  if (!session) {
    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center">
          <h1 className="text-xl font-semibold text-white">StaffArr</h1>
          <p className="mt-4 text-sm text-slate-400">
            No active session. Launch from the suite to receive a handoff code.
          </p>
          <Link className="mt-6 inline-block text-sm text-sky-400 hover:underline" to="/launch">
            Open launch path
          </Link>
        </div>
      </main>
    )
  }

  if (meQuery.isLoading) {
    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <p className="text-slate-400">Loading your workspace…</p>
      </main>
    )
  }

  if (meQuery.isError || !meQuery.data) {
    if (meQuery.error instanceof StaffArrApiError && (meQuery.error.status === 401 || meQuery.error.status === 403)) {
      clearSession()
    }

    return (
      <main className="flex min-h-screen items-center justify-center p-6">
        <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center">
          <h1 className="text-xl font-semibold text-white">StaffArr</h1>
          <p className="mt-4 text-sm text-red-300">
            {meQuery.error instanceof StaffArrApiError && meQuery.error.status === 403
              ? 'Your session is not entitled for StaffArr access.'
              : 'Could not load your StaffArr profile.'}
          </p>
          <p className="mt-2 text-xs text-slate-500">Relaunch StaffArr from the suite shell.</p>
        </div>
      </main>
    )
  }

  const me = meQuery.data

  return (
    <main className="mx-auto max-w-2xl p-8">
      <header className="border-b border-slate-700 pb-6">
        <p className="text-xs uppercase tracking-wide text-slate-500">STL Compliance</p>
        <h1 className="mt-1 text-3xl font-semibold text-white">StaffArr</h1>
        <p className="mt-2 text-slate-400">Workforce readiness shell</p>
      </header>

      <section className="mt-8 rounded-xl border border-slate-700 bg-slate-900/60 p-6">
        <h2 className="text-sm font-medium text-slate-300">Signed in</h2>
        <dl className="mt-4 grid gap-3 text-sm">
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Name</dt>
            <dd className="text-right text-white">{me.displayName}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Email</dt>
            <dd className="text-right text-white">{me.email}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Tenant</dt>
            <dd className="text-right font-mono text-xs text-slate-300">{session.tenantSlug}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">Person ID</dt>
            <dd className="text-right font-mono text-xs text-slate-300">{me.personId}</dd>
          </div>
          <div className="flex justify-between gap-4">
            <dt className="text-slate-500">StaffArr entitlement</dt>
            <dd className="text-right text-emerald-400">
              {me.hasStaffArrEntitlement ? 'Active' : 'Missing'}
            </dd>
          </div>
        </dl>
      </section>
    </main>
  )
}
