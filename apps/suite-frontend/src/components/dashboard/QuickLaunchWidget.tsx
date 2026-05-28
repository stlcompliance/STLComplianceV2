import { Link } from 'react-router-dom'
import { useProductLaunch } from '../../hooks/useProductLaunch'
import { buildQuickLaunchProducts } from '../../lib/dashboard'
import { getProductIcon } from '../../lib/productIcons'
import type { NavigationItem } from '../../api/types'
import { DashboardCard } from './DashboardCard'

export function QuickLaunchWidget({
  navigationProducts,
  entitlements,
}: {
  navigationProducts: readonly NavigationItem[]
  entitlements: readonly string[]
}) {
  const launch = useProductLaunch()
  const products = buildQuickLaunchProducts(navigationProducts, entitlements)

  if (products.length === 0) {
    return (
      <DashboardCard title="Quick launch">
        <p className="text-sm text-slate-400">No entitled products in your navigation.</p>
      </DashboardCard>
    )
  }

  return (
    <DashboardCard title="Quick launch">
      <ul className="grid gap-2 sm:grid-cols-2">
        {products.map((product) => {
          const Icon = getProductIcon(product.productKey)
          return (
            <li
              key={product.productKey}
              className="flex items-center justify-between gap-2 rounded-md border border-slate-700 bg-slate-950/40 px-3 py-2"
            >
              <Link
                to={product.routePath}
                className="flex min-w-0 flex-1 items-center gap-2 text-sm font-medium text-slate-100 hover:text-teal-400"
              >
                <Icon className="h-4 w-4 shrink-0" aria-hidden />
                <span className="truncate">{product.displayName}</span>
              </Link>
              {!product.inSuite && (
                <button
                  type="button"
                  disabled={launch.isPending}
                  onClick={() => launch.mutate(product.productKey)}
                  className="shrink-0 rounded border border-teal-500/50 px-2 py-1 text-xs text-teal-400 hover:bg-slate-800/50 disabled:opacity-50"
                >
                  Open app
                </button>
              )}
            </li>
          )
        })}
      </ul>
      {launch.isError && (
        <p className="mt-2 text-xs text-red-300" role="alert">
          Launch failed: {(launch.error as Error).message}
        </p>
      )}
    </DashboardCard>
  )
}
