import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet, useSearchParams } from 'react-router-dom'
import {
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  formatProductLaunchError,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  useProductWorkspaceLaunch,
} from '@stl/shared-ui'
import { getMe } from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'
import { trainarrNavItems } from '../navigation/productNav'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_TRAINARR_API_BASE ?? ''

export function ProductWorkspaceLayout() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const session = loadSession()

  const meQuery = useQuery({
    queryKey: ['trainarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (meQuery.isError && resolveProductWorkspaceBootstrapError(meQuery.error)) {
      clearSession()
    }
  }, [meQuery.isError, meQuery.error])

  const productLaunch = useProductWorkspaceLaunch({
    apiBase,
    accessToken: session?.accessToken ?? '',
    currentProductKey: 'trainarr',
    suiteHomeUrl,
    productLaunchUrls,
  })

  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const bootstrapError = meQuery.isError
    ? resolveProductWorkspaceBootstrapError(meQuery.error)
    : null

  const workspaceSession =
    session && meQuery.data && !bootstrapError
      ? {
          userDisplayName: meQuery.data.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  return (
    <ProductWorkspaceFrame
      productName="TrainArr"
      productKey="trainarr"
      workspaceSubtitle="Training and qualifications"
      navItems={trainarrNavItems}
      entitlements={meQuery.data?.entitlements ?? []}
      suiteHomeUrl={suiteHomeUrl}
      productLaunchUrls={productLaunchUrls}
      onSelectProduct={(productKey) => {
        if (session?.accessToken) {
          void productLaunch.mutate(productKey)
        }
      }}
      isProductLaunchPending={productLaunch.isPending}
      productLaunchError={
        productLaunch.isError ? formatProductLaunchError(productLaunch.error) : null
      }
      workspaceSession={workspaceSession}
      isBootstrapping={Boolean(session?.accessToken) && meQuery.isLoading}
      bootstrapError={bootstrapError}
    >
      <Outlet />
    </ProductWorkspaceFrame>
  )
}
