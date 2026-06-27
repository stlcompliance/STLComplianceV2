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
import { staffarrNavItems } from '../navigation/productNav'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_STAFFARR_API_BASE ?? ''

export function ProductWorkspaceLayout() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const session = loadSession()

  const sessionQuery = useQuery({
    queryKey: ['staffarr-session', session?.accessToken],
    queryFn: () => getSessionBootstrap(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  const launchCatalogQuery = useQuery({
    queryKey: ['staffarr-launch-catalog', session?.accessToken],
    queryFn: () => getLaunchCatalog(apiBase, session!.accessToken, 'staffarr'),
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
    currentProductKey: 'staffarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const sessionBootstrapError = sessionQuery.isError
    ? resolveProductWorkspaceBootstrapError(sessionQuery.error)
    : null
  const launchBootstrapError = launchCatalogQuery.isError
    ? resolveProductWorkspaceBootstrapError(launchCatalogQuery.error)
    : null
  const bootstrapError = sessionBootstrapError ?? launchBootstrapError

  const workspaceSession =
    session && sessionQuery.data && !bootstrapError
      ? {
          userId: session.userId,
          tenantId: session.tenantId,
          userDisplayName: session.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
          isPlatformAdmin: session.isPlatformAdmin,
        }
      : null

  return (
    <ProductWorkspaceFrame
      productName="StaffArr"
      productKey="staffarr"
      workspaceSubtitle="People, org, locations, and readiness"
      navItems={staffarrNavItems}
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
      aiAssistance={
        session?.accessToken ? { apiBase, accessToken: session.accessToken } : undefined
      }
      workspaceSession={workspaceSession}
      isBootstrapping={
        Boolean(session?.accessToken) && (sessionQuery.isLoading || launchCatalogQuery.isLoading)
      }
      bootstrapError={bootstrapError}
    >
      <Outlet />
    </ProductWorkspaceFrame>
  )
}
