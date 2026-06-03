import type { LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'
import { LogOut } from 'lucide-react'
import { NavLink, useLocation } from 'react-router-dom'
import { getSuiteProductIcon } from './productCatalog'
import { ProductSwitcher } from './ProductSwitcher'
import { WorkspaceUserChrome } from './WorkspaceUserChrome'

export type ProductNavItem = {
  label: string
  to: string
  icon?: LucideIcon
  sectionBreakBefore?: boolean
  children?: ProductNavItem[]
}

export type ProductAppShellProps = {
  productName: string
  productKey: string
  workspaceSubtitle?: string
  tenantDisplayName?: string
  tenantSlug?: string
  userDisplayName?: string
  entitlements?: readonly string[]
  suiteHomeUrl?: string
  productLaunchUrls?: Record<string, string>
  onSelectProduct?: (productKey: string) => void
  onSignOut?: () => void
  isProductLaunchPending?: boolean
  productLaunchError?: string | null
  navItems?: ProductNavItem[]
  /** Compact layout hides sidebar navigation (field/mobile apps). */
  layoutVariant?: 'standard' | 'compact'
  children: ReactNode
}

function WorkspaceTopBar({
  productName,
  productKey,
  workspaceSubtitle,
  tenantDisplayName,
  tenantSlug,
  userDisplayName,
  entitlements,
  suiteHomeUrl,
  productLaunchUrls,
  onSelectProduct,
  onSignOut,
  isProductLaunchPending,
  productLaunchError,
}: {
  productName: string
  productKey: string
  workspaceSubtitle: string
  tenantDisplayName?: string
  tenantSlug?: string
  userDisplayName?: string
  entitlements: readonly string[]
  suiteHomeUrl: string
  productLaunchUrls?: Record<string, string>
  onSelectProduct?: (productKey: string) => void
  onSignOut?: () => void
  isProductLaunchPending?: boolean
  productLaunchError?: string | null
}) {
  const ProductIcon = getSuiteProductIcon(productKey)

  return (
    <header className="flex shrink-0 items-center justify-between border-b border-slate-700/70 bg-[#0a101c] px-6 py-4">
      <div className="flex min-w-0 items-center gap-3">
        <ProductIcon className="h-5 w-5 shrink-0 text-teal-400" aria-hidden />
        <div className="min-w-0">
          <p className="truncate text-base font-semibold text-white">{productName}</p>
          <p className="truncate text-xs text-slate-400">{workspaceSubtitle}</p>
        </div>
      </div>
      <div className="flex items-center gap-3">
        <ProductSwitcher
          currentProductKey={productKey}
          entitlements={entitlements}
          suiteHomeUrl={suiteHomeUrl}
          productLaunchUrls={productLaunchUrls}
          onSelectProduct={onSelectProduct}
          isPending={isProductLaunchPending}
          errorMessage={productLaunchError}
        />
        {onSignOut ? (
          <button
            type="button"
            onClick={onSignOut}
            className="inline-flex items-center gap-2 rounded-md border border-slate-600 bg-slate-900/60 px-3 py-1.5 text-sm text-slate-100 hover:border-teal-500/50 hover:bg-slate-800/80"
          >
            <LogOut className="h-4 w-4 shrink-0 text-slate-300" aria-hidden />
            <span className="hidden sm:inline">Sign out</span>
          </button>
        ) : null}
        <WorkspaceUserChrome
          userDisplayName={userDisplayName}
          tenantDisplayName={tenantDisplayName}
          tenantSlug={tenantSlug}
        />
      </div>
    </header>
  )
}

export function ProductAppShell({
  productName,
  productKey,
  workspaceSubtitle = 'Operational workspace',
  tenantDisplayName,
  tenantSlug,
  userDisplayName,
  entitlements = [],
  suiteHomeUrl = 'http://localhost:5174/app',
  productLaunchUrls,
  onSelectProduct,
  onSignOut,
  isProductLaunchPending,
  productLaunchError,
  navItems = [{ label: 'Workspace', to: '/' }],
  layoutVariant = 'standard',
  children,
}: ProductAppShellProps) {
  const location = useLocation()
  const showSidebar = layoutVariant === 'standard'
  const ProductIcon = getSuiteProductIcon(productKey)
  const navLinkClassName = ({ isActive }: { isActive: boolean }) =>
    [
      'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
      isActive
        ? 'border-l-2 border-teal-400 bg-slate-800/80 pl-[10px] text-white'
        : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
    ].join(' ')

  const routeIsActive = (to: string) =>
    location.pathname === to || location.pathname.startsWith(`${to}/`)

  const mobileNavLinkClassName = (to: string) =>
    [
      'flex shrink-0 items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
      routeIsActive(to)
        ? 'bg-slate-800 text-white ring-1 ring-teal-500/50'
        : 'text-slate-300 hover:bg-slate-800/60 hover:text-white',
    ].join(' ')

  const routeSection = (to: string) => {
    const parts = to.split('/').filter(Boolean)
    return parts.length > 0 ? `/${parts[0]}` : to
  }

  const routeSectionIsActive = (to: string) => {
    const section = routeSection(to)
    return location.pathname === section || location.pathname.startsWith(`${section}/`)
  }

  const mobileNavItems = navItems.flatMap((item) => {
    if (item.children?.length && routeSectionIsActive(item.to)) {
      return [item, ...item.children]
    }
    return [item]
  })

  if (!showSidebar) {
    return (
      <div className="flex min-h-screen flex-col bg-[#0f172a] text-slate-100">
        <WorkspaceTopBar
          productName={productName}
          productKey={productKey}
          workspaceSubtitle={workspaceSubtitle}
          tenantDisplayName={tenantDisplayName}
          tenantSlug={tenantSlug}
          userDisplayName={userDisplayName}
          entitlements={entitlements}
          suiteHomeUrl={suiteHomeUrl}
          productLaunchUrls={productLaunchUrls}
          onSelectProduct={onSelectProduct}
          onSignOut={onSignOut}
          isProductLaunchPending={isProductLaunchPending}
          productLaunchError={productLaunchError}
        />
        <main className="min-h-0 flex-1 overflow-auto px-4 pb-8 pt-4">{children}</main>
      </div>
    )
  }

  return (
    <div className="flex min-h-screen bg-[#0f172a] text-slate-100">
      <aside className="hidden w-64 shrink-0 flex-col min-h-0 overflow-y-auto border-r border-slate-700/70 bg-[#0a101c] p-4 lg:flex">
        <div className="mb-6 shrink-0">
          <p className="text-xs font-semibold uppercase tracking-wide text-teal-400">STL Compliance</p>
          <h1 className="text-lg font-semibold text-white">{productName}</h1>
          {(userDisplayName || tenantDisplayName || tenantSlug) && (
            <div className="mt-1 space-y-0.5 text-xs text-slate-400">
              {userDisplayName ? <p>{userDisplayName}</p> : null}
              {tenantDisplayName ? <p>{tenantDisplayName}</p> : null}
              {tenantSlug ? <p className="font-mono text-slate-500">{tenantSlug}</p> : null}
            </div>
          )}
        </div>

        <nav aria-label={`${productName} navigation`} className="flex flex-col gap-1">
          {navItems.map((item) => {
            const Icon = item.icon ?? ProductIcon
            const childActive = item.children?.some((child) => routeIsActive(child.to)) ?? false
            const expanded = routeIsActive(item.to) || routeSectionIsActive(item.to) || childActive
            return (
              <div key={item.to} className={item.sectionBreakBefore ? 'mt-2 border-t border-slate-700/70 pt-2' : ''}>
                <NavLink to={item.to} end={item.to === '/'} className={navLinkClassName}>
                  <Icon className="h-4 w-4 shrink-0" aria-hidden />
                  <span>{item.label}</span>
                </NavLink>
                {item.children?.length && expanded ? (
                  <div className="ml-5 mt-1 flex flex-col gap-1 border-l border-slate-700/60 pl-2">
                    {item.children.map((child) => {
                      const ChildIcon = child.icon ?? Icon
                      return (
                        <NavLink key={child.to} to={child.to} end={child.to === '/'} className={navLinkClassName}>
                          <ChildIcon className="h-4 w-4 shrink-0 opacity-80" aria-hidden />
                          <span>{child.label}</span>
                        </NavLink>
                      )
                    })}
                  </div>
                ) : null}
              </div>
            )
          })}
        </nav>
      </aside>

      <div className="flex min-h-0 min-w-0 flex-1 flex-col">
        <WorkspaceTopBar
          productName={productName}
          productKey={productKey}
          workspaceSubtitle={workspaceSubtitle}
          tenantDisplayName={tenantDisplayName}
          tenantSlug={tenantSlug}
          userDisplayName={userDisplayName}
          entitlements={entitlements}
          suiteHomeUrl={suiteHomeUrl}
          productLaunchUrls={productLaunchUrls}
          onSelectProduct={onSelectProduct}
          onSignOut={onSignOut}
          isProductLaunchPending={isProductLaunchPending}
          productLaunchError={productLaunchError}
        />
        <nav aria-label={`${productName} mobile navigation`} className="border-b border-slate-700/70 bg-[#0a101c] px-4 py-2 lg:hidden">
          <div className="flex gap-2 overflow-x-auto pb-1 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
            {mobileNavItems.map((item) => {
              const Icon = item.icon ?? ProductIcon
              return (
                <NavLink key={item.to} to={item.to} end={item.to === '/'} className={mobileNavLinkClassName(item.to)}>
                  <Icon className="h-4 w-4 shrink-0" aria-hidden />
                  <span>{item.label}</span>
                </NavLink>
              )
            })}
          </div>
        </nav>
        <main className="min-h-0 flex-1 overflow-auto p-4 lg:p-6">{children}</main>
      </div>
    </div>
  )
}
