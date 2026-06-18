import { useEffect } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Navigate, Outlet, useSearchParams } from 'react-router-dom'
import {
  AppWindow,
  Camera,
  Bell,
  CloudOff,
  Inbox,
  LayoutDashboard,
  ScanLine,
  UserRound,
  BellRing,
} from 'lucide-react'
import {
  ProductWorkspaceFrame,
  buildProductLaunchUrlMap,
  resolveProductWorkspaceBootstrapError,
  resolveSuiteHomeUrl,
  type ProductNavItem,
} from '@stl/shared-ui'
import { getMe } from '../api/client'
import { clearSession, loadSession } from '../auth/sessionStorage'
import { useFieldCompanionProductLaunch } from '../hooks/useFieldCompanionProductLaunch'
import { formatProductLaunchError } from '../lib/productLaunch'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_NEXARR_API_BASE ?? ''

const navItems: ProductNavItem[] = [
  { label: 'My work', to: '/', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Inbox', to: '/inbox', icon: Inbox as ProductNavItem['icon'] },
  { label: 'Scan', to: '/scan', icon: ScanLine as ProductNavItem['icon'] },
  { label: 'Capture', to: '/capture', icon: Camera as ProductNavItem['icon'] },
  { label: 'Report', to: '/report', icon: Bell as ProductNavItem['icon'] },
  { label: 'Product surfaces', to: '/surfaces', icon: AppWindow as ProductNavItem['icon'] },
  { label: 'Offline queue', to: '/offline-queue', icon: CloudOff as ProductNavItem['icon'] },
  { label: 'Profile', to: '/profile', icon: UserRound as ProductNavItem['icon'] },
  { label: 'Notifications', to: '/notifications', icon: BellRing as ProductNavItem['icon'] },
]

export function ProductWorkspaceLayout() {
  const [searchParams] = useSearchParams()
  const handoff = searchParams.get('handoff')
  const session = loadSession()

  const meQuery = useQuery({
    queryKey: ['fieldcompanion-me', session?.accessToken],
    queryFn: () => getMe(session!.accessToken),
    enabled: Boolean(session?.accessToken),
    retry: false,
  })

  useEffect(() => {
    if (meQuery.isError && resolveProductWorkspaceBootstrapError(meQuery.error)) {
      clearSession()
    }
  }, [meQuery.isError, meQuery.error])

  const productLaunch = useFieldCompanionProductLaunch({
    accessToken: session?.accessToken ?? '',
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
          userId: session.userId,
          tenantId: session.tenantId,
          userDisplayName: meQuery.data.displayName,
          tenantDisplayName: session.tenantDisplayName,
          tenantSlug: session.tenantSlug,
        }
      : null

  return (
    <ProductWorkspaceFrame
      productName="Field Companion"
      productKey="fieldcompanion"
      workspaceSubtitle="Field inbox and mobile tasks"
      navItems={navItems}
      layoutVariant="compact"
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
      onSignOut={() => {
        clearSession()
        window.location.assign(suiteHomeUrl)
      }}
      aiAssistance={
        session?.accessToken ? { apiBase, accessToken: session.accessToken } : undefined
      }
      workspaceSession={workspaceSession}
      isBootstrapping={Boolean(session?.accessToken) && meQuery.isLoading}
      bootstrapError={bootstrapError}
    >
      <Outlet />
    </ProductWorkspaceFrame>
  )
}
