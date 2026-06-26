import { Link } from 'react-router-dom'
import { ApiErrorCallout, getErrorMessage } from '@stl/shared-ui'
import { useProductLaunch } from '../../hooks/useProductLaunch'
import { buildQuickLaunchProducts } from '../../lib/dashboard'
import { getProductIcon } from '../../lib/productIcons'
import type { NavigationItem } from '../../api/types'
import { DashboardCard } from './DashboardCard'

export function QuickLaunchWidget({
  navigationProducts,
}: {
  navigationProducts: readonly NavigationItem[]
}) {
  const launch = useProductLaunch()
  const products = buildQuickLaunchProducts(navigationProducts)

  if (products.length === 0) {
    return (
      <DashboardCard title="Quick launch">
        <p className="text-sm text-[var(--color-text-muted)]">
          Product destinations are temporarily unavailable in navigation. Refresh the page if the
          catalog does not return.
        </p>
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
              className="flex items-center justify-between gap-2 rounded-md border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface-elevated)] px-3 py-2"
            >
              <Link
                to={product.routePath}
                className="flex min-w-0 flex-1 items-center gap-2 text-sm font-medium text-[var(--color-text-primary)] hover:text-[var(--color-accent)]"
              >
                <Icon className="h-4 w-4 shrink-0" aria-hidden />
                <span className="truncate">{product.displayName}</span>
              </Link>
              {!product.inSuite && product.launchable && (
                <button
                  type="button"
                  disabled={launch.isPending}
                  onClick={() => launch.mutate(product.productKey)}
                  className="shrink-0 rounded border border-[var(--color-accent-border)] px-2 py-1 text-xs text-[var(--color-accent)] transition-colors hover:bg-[var(--color-bg-surface)] disabled:opacity-50"
                >
                  Open app
                </button>
              )}
            </li>
          )
        })}
      </ul>
      {launch.isError && (
        <ApiErrorCallout
          className="mt-2 text-xs"
          title="Unable to launch product"
          message={getErrorMessage(launch.error, 'Failed to launch product.')}
        />
      )}
    </DashboardCard>
  )
}
