import { useEffect, useRef, useState } from 'react'
import { Navigate, Outlet, useSearchParams } from 'react-router-dom'
import {
  AppWindow,
  Camera,
  Bell,
  CloudOff,
  Clock3,
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
import { renewFieldCompanionSession } from '../api/client'
import { clearSession } from '../auth/sessionStorage'
import { useFieldCompanionProductLaunch } from '../hooks/useFieldCompanionProductLaunch'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'
import { formatProductLaunchError } from '../lib/productLaunch'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_NEXARR_API_BASE ?? ''

const navItems: ProductNavItem[] = [
  { label: 'My work', to: '/', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'Clock', to: '/clock', icon: Clock3 as ProductNavItem['icon'] },
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
  const { session, accessToken, meQuery } = useFieldCompanionWorkspace()
  const [, bumpBootstrapTick] = useState(0)
  const renewInFlightRef = useRef(false)

  useEffect(() => {
    if (!session || accessToken || renewInFlightRef.current) {
      return
    }

    renewInFlightRef.current = true
    let cancelled = false
    void renewFieldCompanionSession()
      .then(() => {
        if (!cancelled) {
          bumpBootstrapTick((value) => value + 1)
        }
      })
      .catch(() => {
        if (!cancelled) {
          clearSession()
          bumpBootstrapTick((value) => value + 1)
        }
      })
      .finally(() => {
        renewInFlightRef.current = false
      })

    return () => {
      cancelled = true
    }
  }, [accessToken, session?.sessionId])

  useEffect(() => {
    if (meQuery.isError && resolveProductWorkspaceBootstrapError(meQuery.error)) {
      clearSession()
    }
  }, [meQuery.isError, meQuery.error])

  const productLaunch = useFieldCompanionProductLaunch({
    accessToken,
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
        if (accessToken) {
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
        accessToken ? { apiBase, accessToken } : undefined
      }
      workspaceSession={workspaceSession}
      isBootstrapping={Boolean(session) && (meQuery.isLoading || !accessToken)}
      bootstrapError={bootstrapError}
    >
      <Outlet />
    </ProductWorkspaceFrame>
  )
}
