import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet, useSearchParams } from 'react-router-dom'
import {
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  type ProductNavItem,
} from '@stl/shared-ui'
import { getMe } from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

const navItems: ProductNavItem[] = [{ label: 'Dispatch board', to: '/' }]

export function ProductWorkspaceLayout() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const session = loadSession()

  const meQuery = useQuery({
    queryKey: ['routarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (meQuery.isError && resolveProductWorkspaceBootstrapError(meQuery.error)) {
      clearSession()
    }
  }, [meQuery.isError, meQuery.error])

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
          tenantDisplayName: session.tenantSlug,
        }
      : null

  return (
    <ProductWorkspaceFrame
      productName="RoutArr"
      productKey="routarr"
      workspaceSubtitle="Routes, trips, and dispatch"
      navItems={navItems}
      entitlements={meQuery.data?.entitlements ?? []}
      suiteHomeUrl={suiteHomeUrl}
      productLaunchUrls={productLaunchUrls}
      workspaceSession={workspaceSession}
      isBootstrapping={Boolean(session?.accessToken) && meQuery.isLoading}
      bootstrapError={bootstrapError}
    >
      <Outlet />
    </ProductWorkspaceFrame>
  )
}
