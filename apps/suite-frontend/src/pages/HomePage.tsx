import { useQuery } from '@tanstack/react-query'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'

export function HomePage() {
  const { me } = useAuth()
  const entitlementsQuery = useQuery({
    queryKey: ['entitlements-summary', me?.tenantId],
    queryFn: async () => {
      const navigation = await nexarr.getNavigation()
      return navigation.products
    },
    enabled: me !== undefined,
  })

  return (
    <div className="max-w-3xl space-y-4">
      <h3 className="text-xl font-semibold text-stl-navy">Welcome, {me?.displayName}</h3>
      <p className="text-sm text-slate-700">
        This shell loads navigation from{' '}
        <code className="text-xs">/api/me/navigation</code> and launches products via{' '}
        <code className="text-xs">/api/launch/context</code> and handoff when needed.
      </p>

      <section className="rounded-lg border border-slate-200 bg-white p-4">
        <h4 className="text-sm font-semibold text-stl-navy">Entitled products</h4>
        {entitlementsQuery.isLoading && (
          <p className="mt-2 text-sm text-slate-500">Loading…</p>
        )}
        <ul className="mt-2 list-disc pl-5 text-sm text-slate-700">
          {entitlementsQuery.data?.map((p) => (
            <li key={p.productKey}>{p.displayName}</li>
          ))}
        </ul>
      </section>
    </div>
  )
}
