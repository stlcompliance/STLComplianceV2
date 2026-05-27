import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { Navigate, useSearchParams } from 'react-router-dom'
import { getFieldInbox, getMe } from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'
import { FieldInboxPanel } from '../components/FieldInboxPanel'
import { entitledProductKeys } from '../lib/fieldInbox'

export function HomePage() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()
  const [productFilter, setProductFilter] = useState('')

  const meQuery = useQuery({
    queryKey: ['companion-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
  })

  const inboxQuery = useQuery({
    queryKey: ['companion-field-inbox', session?.accessToken],
    queryFn: () => getFieldInbox(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    refetchInterval: 60_000,
  })

  const entitledProducts = useMemo(
    () => (inboxQuery.data ? entitledProductKeys(inboxQuery.data.sources) : []),
    [inboxQuery.data],
  )

  if (!session) {
    return (
      <main className="mx-auto flex min-h-screen max-w-lg items-center px-4">
        <div className="w-full rounded-xl border border-slate-700 bg-slate-900/80 p-6 text-center">
          <h1 className="text-lg font-semibold text-white">STL Companion</h1>
          <p className="mt-2 text-sm text-slate-300">
            Launch this app from the suite with a NexArr handoff code to see assigned field work.
          </p>
        </div>
      </main>
    )
  }

  return (
    <div className="mx-auto min-h-screen max-w-2xl px-4 pb-8 pt-[max(1rem,env(safe-area-inset-top))]">
      <header className="sticky top-0 z-10 -mx-4 border-b border-slate-800 bg-slate-950/95 px-4 py-4 backdrop-blur">
        <div className="flex items-start justify-between gap-3">
          <div>
            <p className="text-xs font-semibold uppercase tracking-wide text-teal-300">
              Field inbox
            </p>
            <h1 className="text-xl font-semibold text-white">
              {meQuery.data?.displayName ?? session.displayName}
            </h1>
            <p className="text-sm text-slate-400">
              {session.tenantSlug} · {entitledProducts.length} entitled products
            </p>
          </div>
          <button
            type="button"
            className="rounded-lg border border-slate-600 px-3 py-2 text-xs text-slate-300 hover:border-slate-400"
            onClick={() => {
              clearSession()
              window.location.href = '/'
            }}
          >
            Sign out
          </button>
        </div>
      </header>

      <main className="mt-4 space-y-4">
        {inboxQuery.isLoading && (
          <p className="rounded-xl border border-slate-700 bg-slate-900/70 px-4 py-6 text-sm text-slate-300">
            Loading assigned work across products…
          </p>
        )}

        {inboxQuery.error && (
          <p className="rounded-xl border border-red-500/40 bg-red-950/30 px-4 py-3 text-sm text-red-200">
            {inboxQuery.error instanceof Error
              ? inboxQuery.error.message
              : 'Failed to load field inbox.'}
          </p>
        )}

        {inboxQuery.data && (
          <FieldInboxPanel
            inbox={inboxQuery.data}
            productFilter={productFilter}
            onProductFilterChange={setProductFilter}
          />
        )}
      </main>
    </div>
  )
}
