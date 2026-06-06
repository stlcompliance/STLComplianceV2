import { useQuery } from '@tanstack/react-query'
import {
  getErrorMessage,
  getSuiteProductCatalogEntry,
  ProductSwitcher as SharedProductSwitcher,
  type ProductSwitcherProps as SharedProductSwitcherProps,
} from '@stl/shared-ui'
import { useLocation, useNavigate } from 'react-router-dom'
import * as nexarr from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { useProductLaunch } from '../hooks/useProductLaunch'
import { isInSuiteProduct } from '../lib/permissions'
import { normalizeProductKey } from '../navigation/suiteNavigation'

function resolveCurrentProductKey(pathname: string): string {
  if (pathname.startsWith('/app/platform-admin')) {
    return 'nexarr'
  }
  const match = /^\/app\/([^/]+)/.exec(pathname)
  if (match) {
    return normalizeProductKey(match[1])
  }
  return 'nexarr'
}

export function ProductSwitcher() {
  const location = useLocation()
  const navigate = useNavigate()
  const { me } = useAuth()
  const launch = useProductLaunch()
  const currentProductKey = resolveCurrentProductKey(location.pathname)

  const navigationQuery = useQuery({
    queryKey: ['navigation', me?.tenantId, currentProductKey],
    queryFn: () => nexarr.getNavigation(currentProductKey),
    enabled: me !== undefined,
  })

  const entitledProducts = resolveSwitcherProductKeys(
    me?.entitlements ?? [],
    navigationQuery.data?.products.map((product) => product.productKey) ?? [],
  )

  function handleSelect(productKey: string) {
    const normalized = normalizeProductKey(productKey)

    if (normalized === currentProductKey) {
      return
    }

    if (isInSuiteProduct(normalized)) {
      navigate('/app')
      return
    }

    void launch.mutate(normalized)
  }

  if (navigationQuery.isLoading) {
    return <span className="text-xs text-slate-400">Loading products…</span>
  }

  if (entitledProducts.length === 0) {
    return <span className="text-xs text-slate-500">No entitled products</span>
  }

  return (
    <SharedProductSwitcher
      currentProductKey={currentProductKey}
      entitlements={entitledProducts}
      suiteHomeUrl="/app"
      onSelectProduct={handleSelect}
      isPending={launch.isPending}
      errorMessage={launch.isError ? getErrorMessage(launch.error, 'Failed to launch product.') : null}
    />
  )
}

function resolveSwitcherProductKeys(
  entitlements: readonly string[],
  navigationProductKeys: readonly string[],
): SharedProductSwitcherProps['entitlements'] {
  const allowedKeys = new Set(
    navigationProductKeys
      .map((productKey) => normalizeProductKey(productKey))
      .filter((productKey) => Boolean(getSuiteProductCatalogEntry(productKey))),
  )
  const seenKeys = new Set<string>()

  return entitlements.filter((productKey) => {
    const normalized = normalizeProductKey(productKey)
    if (!allowedKeys.has(normalized) || seenKeys.has(normalized)) {
      return false
    }

    seenKeys.add(normalized)
    return true
  })
}
