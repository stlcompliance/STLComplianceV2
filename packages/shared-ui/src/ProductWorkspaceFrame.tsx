import { useEffect, type ReactNode } from 'react'
import { ProductAppShell, type ProductAppShellProps } from './ProductAppShell'
import { buildNexArrLoginUrl } from './productWorkspaceAuth'

export type ProductWorkspaceSession = {
  userId?: string
  tenantId?: string
  themePreference?: string | null
  accessToken?: string
  userDisplayName: string
  tenantDisplayName: string
  tenantSlug: string
}

export type ProductWorkspaceFrameProps = {
  productName: string
  productKey: string
  workspaceSubtitle?: string
  navItems?: ProductAppShellProps['navItems']
  layoutVariant?: ProductAppShellProps['layoutVariant']
  suiteHomeUrl?: string
  platformApiBase?: string
  productApiBase?: string
  productLaunchUrls?: Record<string, string>
  onSelectProduct?: (productKey: string) => void
  onSignOut?: () => void
  isProductLaunchPending?: boolean
  productLaunchError?: string | null
  aiAssistance?: ProductAppShellProps['aiAssistance']
  workspaceSession: ProductWorkspaceSession | null
  isBootstrapping?: boolean
  bootstrapError?: 'forbidden' | 'expired' | null
  children: ReactNode
}

function WorkspaceMessage({
  productName,
  title,
  message,
}: {
  productName: string
  title: string
  message: string
}) {
  return (
    <main className="flex min-h-screen items-center justify-center bg-[var(--color-bg-app)] p-6 text-[var(--color-text-primary)]">
      <div className="max-w-md rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-8 text-center shadow-lg">
        <p className="text-xs font-semibold uppercase tracking-wide text-[var(--color-accent)]">{productName}</p>
        <h1 className="mt-2 text-xl font-semibold text-[var(--color-text-primary)]">{title}</h1>
        <p className="mt-4 text-sm text-[var(--color-text-muted)]">{message}</p>
      </div>
    </main>
  )
}

export function ProductWorkspaceFrame({
  productName,
  productKey,
  workspaceSubtitle,
  navItems,
  layoutVariant,
  suiteHomeUrl,
  platformApiBase,
  productApiBase,
  productLaunchUrls,
  onSelectProduct,
  onSignOut,
  isProductLaunchPending,
  productLaunchError,
  aiAssistance,
  workspaceSession,
  isBootstrapping = false,
  bootstrapError = null,
  children,
}: ProductWorkspaceFrameProps) {
  const shouldRedirectToNexArr =
    (!workspaceSession && !isBootstrapping && !bootstrapError) || bootstrapError === 'expired'
  const nexArrLoginUrl = shouldRedirectToNexArr
    ? buildNexArrLoginUrl({ suiteHomeUrl, productKey })
    : null

  useEffect(() => {
    if (!nexArrLoginUrl) {
      return
    }
    globalThis.location?.assign(nexArrLoginUrl)
  }, [nexArrLoginUrl])

  if (shouldRedirectToNexArr) {
    return (
      <WorkspaceMessage
        productName={productName}
        title="Redirecting to sign in"
        message={`Your ${productName} session is not active. Sending you to NexArr to sign in again.`}
      />
    )
  }

  if (isBootstrapping) {
    return (
      <WorkspaceMessage
        productName={productName}
        title="Loading workspace"
        message="Verifying your session and workspace access…"
      />
    )
  }

  if (bootstrapError) {
    return (
      <WorkspaceMessage
        productName={productName}
        title={bootstrapError === 'forbidden' ? 'Access denied' : 'Session expired'}
        message={
          bootstrapError === 'forbidden'
            ? `Your account does not have access to ${productName} for this tenant. Relaunch from the suite or contact an administrator.`
            : `Your ${productName} session expired or is invalid. Relaunch from the suite to continue.`
        }
      />
    )
  }

  if (!workspaceSession) {
    return (
      <WorkspaceMessage
        productName={productName}
        title="Redirecting to sign in"
        message={`Your ${productName} session is not active. Sending you to NexArr to sign in again.`}
      />
    )
  }

  return (
    <ProductAppShell
      productName={productName}
      productKey={productKey}
      workspaceSubtitle={workspaceSubtitle}
      tenantDisplayName={workspaceSession.tenantDisplayName}
      tenantSlug={workspaceSession.tenantSlug}
      userId={workspaceSession.userId}
      tenantId={workspaceSession.tenantId}
      themePreference={workspaceSession.themePreference}
      userDisplayName={workspaceSession.userDisplayName}
      suiteHomeUrl={suiteHomeUrl}
      platformApiBase={platformApiBase}
      productApiBase={productApiBase}
      workspaceAccessToken={workspaceSession.accessToken}
      productLaunchUrls={productLaunchUrls}
      onSelectProduct={onSelectProduct}
      onSignOut={onSignOut}
      isProductLaunchPending={isProductLaunchPending}
      productLaunchError={productLaunchError}
      aiAssistance={aiAssistance}
      navItems={navItems}
      layoutVariant={layoutVariant}
    >
      {children}
    </ProductAppShell>
  )
}
