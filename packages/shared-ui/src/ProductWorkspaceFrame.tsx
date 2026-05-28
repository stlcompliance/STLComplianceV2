import type { ReactNode } from 'react'
import { ProductAppShell, type ProductAppShellProps } from './ProductAppShell'

export type ProductWorkspaceSession = {
  userDisplayName: string
  tenantDisplayName: string
}

export type ProductWorkspaceFrameProps = {
  productName: string
  productKey: string
  workspaceSubtitle?: string
  navItems?: ProductAppShellProps['navItems']
  layoutVariant?: ProductAppShellProps['layoutVariant']
  entitlements?: readonly string[]
  suiteHomeUrl?: string
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
    <main className="flex min-h-screen items-center justify-center p-6">
      <div className="max-w-md rounded-xl border border-slate-700 bg-slate-900/80 p-8 text-center shadow-lg">
        <p className="text-xs font-semibold uppercase tracking-wide text-sky-400">{productName}</p>
        <h1 className="mt-2 text-xl font-semibold text-white">{title}</h1>
        <p className="mt-4 text-sm text-slate-400">{message}</p>
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
  entitlements = [],
  suiteHomeUrl,
  workspaceSession,
  isBootstrapping = false,
  bootstrapError = null,
  children,
}: ProductWorkspaceFrameProps) {
  if (!workspaceSession && !isBootstrapping && !bootstrapError) {
    return (
      <WorkspaceMessage
        productName={productName}
        title="Sign in required"
        message={`Launch ${productName} from the STL Compliance suite to open your workspace.`}
      />
    )
  }

  if (isBootstrapping) {
    return (
      <WorkspaceMessage
        productName={productName}
        title="Loading workspace"
        message="Verifying your session and entitlements…"
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
            ? `Your account is not entitled to ${productName} for this tenant. Relaunch from the suite or contact an administrator.`
            : `Your ${productName} session expired or is invalid. Relaunch from the suite to continue.`
        }
      />
    )
  }

  if (!workspaceSession) {
    return (
      <WorkspaceMessage
        productName={productName}
        title="Sign in required"
        message={`Launch ${productName} from the STL Compliance suite to open your workspace.`}
      />
    )
  }

  return (
    <ProductAppShell
      productName={productName}
      productKey={productKey}
      workspaceSubtitle={workspaceSubtitle}
      tenantDisplayName={workspaceSession.tenantDisplayName}
      userDisplayName={workspaceSession.userDisplayName}
      entitlements={entitlements}
      suiteHomeUrl={suiteHomeUrl}
      navItems={navItems}
      layoutVariant={layoutVariant}
    >
      {children}
    </ProductAppShell>
  )
}
