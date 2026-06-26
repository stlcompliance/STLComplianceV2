import { AppWindow, ExternalLink } from 'lucide-react'
import { PageHeader, buildProductLaunchUrlMap, resolveSuiteHomeUrl } from '@stl/shared-ui'

import { getFieldInbox } from '../api/client'
import { useFieldCompanionProductLaunch } from '../hooks/useFieldCompanionProductLaunch'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'
import { productTitle } from '../lib/fieldInbox'
import { useQuery } from '@tanstack/react-query'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

export function SurfacesPage() {
  const { session, accessToken, meQuery } = useFieldCompanionWorkspace()
  const inboxQuery = useQuery({
    queryKey: ['fieldcompanion-field-inbox', accessToken],
    queryFn: () => getFieldInbox(accessToken),
    enabled: Boolean(accessToken),
    refetchInterval: 60_000,
  })
  const productLaunch = useFieldCompanionProductLaunch({
    accessToken,
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (!session || !meQuery.data) {
    return <p className="text-sm text-slate-400">Loading product workspaces…</p>
  }

  const products = meQuery.data.fieldProductKeys

  return (
    <div className="mx-auto max-w-5xl space-y-5">
      <PageHeader
        title="Product workspaces"
        subtitle="Available mobile workspaces and what each action does."
      />

      <section className="rounded-2xl border border-slate-700 bg-slate-900/80 p-5">
        <div className="flex items-center gap-2">
          <AppWindow className="h-5 w-5 text-teal-300" aria-hidden />
          <h2 className="text-lg font-semibold text-white">Available workspaces</h2>
        </div>
        <p className="mt-2 text-sm text-slate-400">
          Field Companion is a mobile entry point. Each button below opens the right workspace for the final action.
        </p>

        <div className="mt-4 grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {products.map((productKey) => {
            const taskCount = inboxQuery.data?.summary.countByProduct[productKey] ?? 0
            const launchHref = productLaunchUrl(productKey)
            return (
              <article key={productKey} className="rounded-2xl border border-slate-700 bg-slate-950/60 p-4">
                <p className="text-xs font-semibold uppercase tracking-wide text-teal-300">
                  {productTitle(productKey)}
                </p>
                <p className="mt-2 text-sm text-slate-300">
                  {taskCount > 0
                    ? `${taskCount} task${taskCount === 1 ? '' : 's'} waiting in your field inbox.`
                    : 'No assigned tasks surfaced in the inbox right now.'}
                </p>
                <div className="mt-3 flex flex-wrap gap-2">
                  <button
                    type="button"
                    className="inline-flex min-h-11 items-center rounded-lg bg-teal-600 px-4 py-2 text-sm font-medium text-white hover:bg-teal-500 disabled:opacity-50"
                    disabled={productLaunch.isPending}
                    onClick={() => {
                      void productLaunch.mutateAsync(productKey)
                    }}
                  >
                    Open app
                  </button>
                  {launchHref && (
                    <a
                      href={launchHref}
                      className="inline-flex min-h-11 items-center gap-2 rounded-lg border border-slate-600 px-4 py-2 text-sm font-medium text-slate-100 hover:border-teal-500"
                    >
                      <ExternalLink className="h-4 w-4" aria-hidden />
                      Direct link
                    </a>
                  )}
                </div>
              </article>
            )
          })}
        </div>
      </section>
    </div>
  )
}

function productLaunchUrl(productKey: string): string | null {
  const envKey = `VITE_${productKey.toUpperCase()}_FRONTEND_BASE`
  const base = (import.meta.env[envKey] as string | undefined)?.trim()
  if (!base) {
    return null
  }

  return `${base.replace(/\/$/, '')}/`
}
