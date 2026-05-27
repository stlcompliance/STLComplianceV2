import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet, useParams } from 'react-router-dom'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { ProductSurfaceNav } from '../components/ProductSurfaceNav'
import { canAccessProductRoute } from '../lib/permissions'
import { findNavigationProduct, normalizeProductKey } from '../navigation/suiteNavigation'

export function ProductShellLayout() {
  const { productKey = '' } = useParams<{ productKey: string }>()
  const { me } = useAuth()
  const normalized = normalizeProductKey(productKey)
  const canAccess = canAccessProductRoute(me?.entitlements ?? [], normalized)

  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId],
    queryFn: () => nexarr.getNavigation(),
    enabled: me !== undefined,
  })

  const product = findNavigationProduct(navigationQuery.data?.products ?? [], normalized)

  if (!canAccess) {
    return <Navigate to="/app" replace />
  }

  return (
    <div className="flex min-h-full gap-6">
      <aside className="w-56 shrink-0">
        <div className="rounded-lg border border-slate-200 bg-white p-3">
          <p className="px-3 text-sm font-semibold text-stl-navy">{product?.displayName ?? normalized}</p>
          {navigationQuery.isLoading && (
            <p className="px-3 pt-2 text-xs text-slate-500">Loading navigation…</p>
          )}
          {product && <ProductSurfaceNav productKey={normalized} surfaces={product.surfaces} />}
        </div>
      </aside>
      <div className="min-w-0 flex-1">
        <Outlet />
      </div>
    </div>
  )
}
