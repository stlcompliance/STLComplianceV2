import { useQuery } from '@tanstack/react-query'
import { NavLink } from 'react-router-dom'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { PermissionGate } from './PermissionGate'
import { getProductIcon } from '../lib/productIcons'
import {
  canAccessProductRoute,
  canLaunchFromContext,
  hasProductEntitlement,
  isInSuiteProduct,
} from '../lib/permissions'
import { useProductLaunch } from '../hooks/useProductLaunch'

export function ProductSwitcher() {
  const { me } = useAuth()
  const launch = useProductLaunch()
  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId],
    queryFn: () => nexarr.getNavigation(),
    enabled: me !== undefined,
  })

  const products = navigationQuery.data?.products ?? []
  const entitlements = me?.entitlements ?? []

  return (
    <nav aria-label="Products" className="flex flex-col gap-1">
      {navigationQuery.isLoading && (
        <p className="px-3 text-xs text-slate-500">Loading products…</p>
      )}
      {products.map((product) => {
        const Icon = getProductIcon(product.productKey)
        const entitled = hasProductEntitlement(entitlements, product.productKey)
        const canNavigate = canAccessProductRoute(entitlements, product.productKey)

        return (
          <div key={product.productKey} className="flex flex-col gap-0.5">
            <PermissionGate allowed={canNavigate}>
              <NavLink
                to={product.routePath}
                className={({ isActive }) =>
                  [
                    'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'border-l-2 border-stl-teal bg-slate-800/80 pl-[10px] text-white'
                      : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
                  ].join(' ')
                }
              >
                <Icon className="h-4 w-4 shrink-0" aria-hidden />
                <span>{product.displayName}</span>
              </NavLink>
            </PermissionGate>

            <PermissionGate allowed={entitled && !isInSuiteProduct(product.productKey)}>
              <LaunchButton
                displayName={product.displayName}
                disabled={launch.isPending}
                onLaunch={() => launch.mutate(product.productKey)}
              />
            </PermissionGate>
          </div>
        )
      })}
      {launch.isError && (
        <p className="px-3 text-xs text-red-300" role="alert">
          Launch failed: {(launch.error as Error).message}
        </p>
      )}
    </nav>
  )
}

function LaunchButton({
  displayName,
  disabled,
  onLaunch,
}: {
  displayName: string
  disabled: boolean
  onLaunch: () => void
}) {
  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onLaunch}
      className="mx-3 rounded border border-stl-teal/40 px-2 py-1 text-left text-xs text-stl-teal hover:bg-slate-800/50 disabled:opacity-50"
    >
      Open {displayName} app
    </button>
  )
}

export function useLaunchContextGate(productKey: string) {
  const { me } = useAuth()
  return useQuery({
    queryKey: ['launch-context', productKey, me?.tenantId],
    queryFn: () => nexarr.getLaunchContext(productKey),
    enabled: Boolean(me) && !isInSuiteProduct(productKey),
    select: (ctx) => canLaunchFromContext(ctx),
  })
}
