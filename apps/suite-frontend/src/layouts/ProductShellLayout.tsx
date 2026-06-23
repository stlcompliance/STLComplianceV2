import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet, useParams } from 'react-router-dom'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { ProductSurfaceNav } from '../components/ProductSurfaceNav'
import { canAccessProductRoute } from '../lib/permissions'
import {
  findNavigationProduct,
  getProductDisplayName,
  normalizeProductKey,
} from '../navigation/suiteNavigation'

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

  const productDisplayName = getProductDisplayName(normalized, product?.displayName)

  return (
    <div className="flex min-h-0 flex-col gap-4 lg:flex-row lg:gap-6">
      <aside className="hidden w-56 shrink-0 lg:block">
        <div className="rounded-lg border border-[var(--color-border-subtle)] bg-[var(--color-bg-shell)] p-3 text-[var(--color-text-primary)] shadow-sm">
          <p className="px-3 text-sm font-semibold">{productDisplayName}</p>
          {navigationQuery.isLoading && (
            <p className="px-3 pt-2 text-xs text-[var(--color-text-muted)]">Loading navigation…</p>
          )}
          {product && <ProductSurfaceNav productKey={normalized} surfaces={product.surfaces} />}
        </div>
      </aside>
      <div className="min-w-0 flex-1">
        {product ? (
          <div className="border-b border-[var(--color-border-subtle)] pb-3 lg:hidden">
            <p className="text-sm font-semibold text-[var(--color-text-primary)]">{productDisplayName}</p>
            <ProductSurfaceNav productKey={normalized} surfaces={product.surfaces} variant="mobile" />
          </div>
        ) : null}
        <Outlet />
      </div>
    </div>
  )
}
