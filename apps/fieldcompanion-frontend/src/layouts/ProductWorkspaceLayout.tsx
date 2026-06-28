import { useCallback, useEffect, useRef, useState } from 'react'
import { Navigate, Outlet, useNavigate, useSearchParams } from 'react-router-dom'
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
import { FieldCompanionReleaseSafetyBanner } from '../components/FieldCompanionReleaseSafetyBanner'
import { SharedDeviceProtectionOverlay } from '../components/SharedDeviceProtectionOverlay'
import { useFieldCompanionProductLaunch } from '../hooks/useFieldCompanionProductLaunch'
import { useFieldCompanionWorkspace } from '../hooks/useFieldCompanionWorkspace'
import { getOfflineQueueSnapshot } from '../lib/offlineQueue'
import { formatProductLaunchError } from '../lib/productLaunch'
import { readCurrentFieldCompanionReleaseSafetySnapshot } from '../lib/releaseSafety'
import {
  isFieldCompanionSharedDeviceModeEnabled,
  useSharedDeviceProtection,
} from '../lib/sharedDeviceProtection'

const suiteHomeUrl = resolveSuiteHomeUrl(import.meta.env.VITE_SUITE_URL)
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)
const apiBase = import.meta.env.VITE_NEXARR_API_BASE ?? ''

type SharedDevicePromptMode = 'warning' | 'locked' | null

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
  const navigate = useNavigate()
  const handoff = searchParams.get('handoff')
  const { session, accessToken, meQuery } = useFieldCompanionWorkspace()
  const releaseSafety = readCurrentFieldCompanionReleaseSafetySnapshot()
  const [, bumpBootstrapTick] = useState(0)
  const renewInFlightRef = useRef(false)
  const sharedDeviceEnabled = isFieldCompanionSharedDeviceModeEnabled()
  const {
    phase: sharedDevicePhase,
    isEnabled: sharedDeviceProtectionEnabled,
    recordActivity: sharedDeviceRecordActivity,
    lockNow: sharedDeviceLockNow,
  } = useSharedDeviceProtection(sharedDeviceEnabled)
  const [sharedDevicePromptMode, setSharedDevicePromptMode] = useState<SharedDevicePromptMode>(null)
  const sharedDeviceQueueSnapshot = getOfflineQueueSnapshot()
  const sharedDeviceQueuedActions =
    sharedDeviceQueueSnapshot.pending.length > 0
      ? sharedDeviceQueueSnapshot.pending
      : sharedDeviceQueueSnapshot.conflicts.map((conflict) => conflict.action)

  useEffect(() => {
    if (!sharedDeviceProtectionEnabled) {
      setSharedDevicePromptMode(null)
      return
    }

    if (sharedDevicePhase === 'warning') {
      setSharedDevicePromptMode((current) => current ?? 'warning')
      return
    }

    if (sharedDevicePhase === 'locked') {
      setSharedDevicePromptMode((current) => current ?? 'locked')
      return
    }

    setSharedDevicePromptMode(null)
  }, [sharedDevicePhase, sharedDeviceProtectionEnabled])

  const exitSharedDeviceSession = useCallback(() => {
    clearSession()
    window.location.assign(suiteHomeUrl)
  }, [])

  const handleStaySignedIn = useCallback(() => {
    sharedDeviceRecordActivity()
    setSharedDevicePromptMode(null)
  }, [sharedDeviceRecordActivity])

  const handleOpenOfflineQueue = useCallback(() => {
    sharedDeviceRecordActivity()
    setSharedDevicePromptMode(null)
    navigate('/offline-queue')
  }, [navigate, sharedDeviceRecordActivity])

  const handleSignInPrompt = useCallback(() => {
    const queueSnapshot = getOfflineQueueSnapshot()
    const hasQueuedWork = queueSnapshot.pending.length > 0 || queueSnapshot.conflicts.length > 0
    if (sharedDeviceEnabled && hasQueuedWork) {
      sharedDeviceLockNow()
      setSharedDevicePromptMode('locked')
      return
    }

    exitSharedDeviceSession()
  }, [exitSharedDeviceSession, sharedDeviceEnabled, sharedDeviceLockNow])

  const handleDiscardQueuedWorkAndSignOut = useCallback(() => {
    exitSharedDeviceSession()
  }, [exitSharedDeviceSession])

  const refreshApp = useCallback(() => {
    window.location.reload()
  }, [])

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
          isPlatformAdmin: session.isPlatformAdmin,
        }
      : null

  return (
    <>
      <ProductWorkspaceFrame
        productName="Field Companion"
        productKey="fieldcompanion"
        workspaceSubtitle="Field inbox and mobile tasks"
        navItems={navItems}
        layoutVariant="compact"
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
        onSignOut={handleSignInPrompt}
        aiAssistance={
          accessToken ? { apiBase, accessToken } : undefined
        }
        workspaceSession={workspaceSession}
        isBootstrapping={Boolean(session) && (meQuery.isLoading || !accessToken)}
        bootstrapError={bootstrapError}
      >
        <FieldCompanionReleaseSafetyBanner
          snapshot={releaseSafety}
          suiteHomeUrl={suiteHomeUrl}
          onRefresh={refreshApp}
        />
        {!releaseSafety.isActionBlocked ? <Outlet /> : null}
      </ProductWorkspaceFrame>

      {sharedDevicePromptMode ? (
        <SharedDeviceProtectionOverlay
          isVisible
          isWarning={sharedDevicePromptMode === 'warning'}
          userDisplayName={workspaceSession?.userDisplayName ?? 'Signed-in worker'}
          tenantDisplayName={workspaceSession?.tenantDisplayName ?? 'Current tenant'}
          pendingActions={sharedDeviceQueuedActions}
          onOpenOfflineQueue={handleOpenOfflineQueue}
          onReauthenticate={handleSignInPrompt}
          onDiscardQueuedWorkAndSignOut={handleDiscardQueuedWorkAndSignOut}
          onStaySignedIn={handleStaySignedIn}
        />
      ) : null}
    </>
  )
}
