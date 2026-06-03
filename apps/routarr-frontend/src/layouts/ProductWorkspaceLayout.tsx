import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet, useSearchParams } from 'react-router-dom'
import {
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  getLaunchCatalog,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
} from '@stl/shared-ui'
import { getSessionBootstrap } from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'
import { routarrNavItems } from '../navigation/productNav'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_ROUTARR_API_BASE ?? ''

export function ProductWorkspaceLayout() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const session = loadSession()

  const sessionQuery = useQuery({
    queryKey: ['routarr-session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['routarr-launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'routarr'),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (sessionQuery.isError && resolveProductWorkspaceBootstrapError(sessionQuery.error)) {
      clearSession()
    }
  }, [sessionQuery.isError, sessionQuery.error])

  useEffect(() => {
    if (launchCatalogQuery.isError && resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)) {
      clearSession()
    }
  }, [launchCatalogQuery.isError, launchCatalogQuery.error])

  const productLaunch = useProductWorkspaceLaunch({
    apiBase,
    accessToken: session?.accessToken ?? '',
    currentProductKey: 'routarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const bootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : null

  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  const switcherEntitlements =
    launchCatalogQuery.data?.products.map((product) => product.productKey) ??
    sessionQuery.data?.entitlements ??
    []

  return (
    <ProductWorkspaceFrame
      productName="RoutArr"
      productKey="routarr"
      workspaceSubtitle="Routes, trips, and dispatch"
      navItems={routarrNavItems}
      entitlements={switcherEntitlements}
      suiteHomeUrl={suiteHomeUrl}
      productLaunchUrls={productLaunchUrls}
      onSelectProduct={(productKey) => {
        if (session?.accessToken) {
          void productLaunch.mutate(productKey)
        }
      }}
      onSignOut={() => {
        clearSession()
        window.location.assign(suiteHomeUrl)
      }}
      isProductLaunchPending={productLaunch.isPending}
      productLaunchError={
        productLaunch.isError ? formatProductLaunchError(productLaunch.error) : null
      }
      workspaceSession={workspaceSession}
      isBootstrapping={Boolean(session?.accessToken) && sessionQuery.isLoading}
      bootstrapError={bootstrapError}
    >
      <Outlet />
    </ProductWorkspaceFrame>
  )
}
