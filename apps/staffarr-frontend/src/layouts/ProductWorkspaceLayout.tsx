import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet, useSearchParams } from 'react-router-dom'
import {
  ProductWorkspaceFrame,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  type ProductNavItem,
} from '@stl/shared-ui'
import { getMe } from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)

const navItems: ProductNavItem[] = [{ label: 'People workspace', to: '/' }]

export function ProductWorkspaceLayout() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  if (handoff) {
    return <Navigate to={`/launch?handoff=${encodeURIComponent(handoff)}`} replace />
  }

  const session = loadSession()

  const meQuery = useQuery({
    queryKey: ['staffarr-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (meQuery.isError && resolveProductWorkspaceBootstrapError(meQuery.error)) {
      clearSession()
    }
  }, [meQuery.isError, meQuery.error])

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
      productName="StaffArr"
      productKey="staffarr"
      workspaceSubtitle="People, org, and readiness"
      navItems={navItems}
      entitlements={meQuery.data?.entitlements ?? []}
      suiteHomeUrl={suiteHomeUrl}
      workspaceSession={workspaceSession}
      isBootstrapping={Boolean(session?.accessToken) && meQuery.isLoading}
      bootstrapError={bootstrapError}
    >
      <Outlet />
    </ProductWorkspaceFrame>
  )
}
