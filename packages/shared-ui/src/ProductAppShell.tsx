import type { LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'
import { useCallback, useState } from 'react'
import { Upload } from 'lucide-react'
import { NavLink, useLocation, useNavigate } from 'react-router-dom'
import { AiHelpButton, AiHelpDrawer, type AiHelpMessage } from './AiHelpDrawer'
import { AccountMenuPopover } from './AccountMenuPopover'
import { sendProductAiAssistantMessage } from './aiAssistance'
import { buildAiNavigationLinks } from './aiNavigationLinks'
import { ProductBrandLogo, StlComplianceLogo } from './BrandLogos'
import { PrintActionBar } from './print/PrintActionBar'
import { PrintRuntimeProvider, usePrintRuntime } from './print/PrintRuntime'
import { getSuiteProductIcon } from './productCatalog'
import { ProductSwitcher } from './ProductSwitcher'
import { ThemeToggleButton } from './ThemeToggleButton'
import { updatePlatformThemePreference, type StlThemeMode } from './theme'
import { useThemePreference } from './useThemePreference'

export type ProductNavItem = {
  label: string
  to: string
  icon?: LucideIcon
  sectionBreakBefore?: boolean
  children?: ProductNavItem[]
}

export type ProductAiAssistanceConfig = {
  apiBase: string
  accessToken: string
  surface?: string
  category?: string
  pageContext?: Record<string, unknown>
  allowedBehaviors?: string[]
}

export type ProductAppShellProps = {
  productName: string
  productKey: string
  workspaceSubtitle?: string
  tenantDisplayName?: string
  tenantSlug?: string
  userId?: string
  tenantId?: string
  themePreference?: string | null
  userDisplayName?: string
  entitlements?: readonly string[]
  suiteHomeUrl?: string
  platformApiBase?: string
  productApiBase?: string
  workspaceAccessToken?: string
  productLaunchUrls?: Record<string, string>
  onSelectProduct?: (productKey: string) => void
  onSignOut?: () => void
  isProductLaunchPending?: boolean
  productLaunchError?: string | null
  aiAssistance?: ProductAiAssistanceConfig
  navItems?: ProductNavItem[]
  /** Compact layout hides sidebar navigation (field/mobile apps). */
  layoutVariant?: 'standard' | 'compact'
  children: ReactNode
}

function WorkspaceTopBar({
  productName,
  productKey,
  tenantDisplayName,
  userDisplayName,
  entitlements,
  suiteHomeUrl,
  productLaunchUrls,
  onSelectProduct,
  onSignOut,
  isProductLaunchPending,
  productLaunchError,
  onOpenAiHelp,
  theme,
  onToggleTheme,
}: {
  productName: string
  productKey: string
  tenantDisplayName?: string
  userDisplayName?: string
  entitlements: readonly string[]
  suiteHomeUrl: string
  productLaunchUrls?: Record<string, string>
  onSelectProduct?: (productKey: string) => void
  onSignOut?: () => void
  isProductLaunchPending?: boolean
  productLaunchError?: string | null
  onOpenAiHelp?: () => void
  theme: StlThemeMode
  onToggleTheme: () => void
}) {
  const smartImportUrl = `${suiteHomeUrl.replace(/\/$/, '')}/imports?destinationProduct=${encodeURIComponent(productKey)}`
  const preferencesHref = `${suiteHomeUrl.replace(/\/$/, '')}/${productKey}/preferences`

  return (
    <header
      className="flex shrink-0 flex-wrap items-center justify-between gap-3 border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-shell)] px-3 py-3 sm:px-5"
      data-print-hide
    >
      <div className="flex min-w-[9rem] max-w-[52vw] items-center">
        <ProductBrandLogo
          productName={productName}
          productKey={productKey}
          theme={theme}
          className="h-9 w-[11rem] max-w-full object-contain object-left sm:w-[13rem]"
        />
      </div>
      <div className="flex min-w-0 flex-1 flex-wrap items-center justify-end gap-2 sm:gap-3">
        <ThemeToggleButton theme={theme} onToggle={onToggleTheme} />
        {onOpenAiHelp ? <AiHelpButton onClick={onOpenAiHelp} /> : null}
        <a
          href={smartImportUrl}
          title="Global Smart Import"
          aria-label="Global Smart Import"
          className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] text-[var(--color-text-primary)] transition hover:border-[var(--color-accent-border)] hover:bg-[var(--color-bg-control-hover)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
        >
          <Upload className="h-4 w-4 shrink-0 text-[var(--color-text-secondary)]" aria-hidden />
        </a>
        <ProductSwitcher
          currentProductKey={productKey}
          entitlements={entitlements}
          suiteHomeUrl={suiteHomeUrl}
          productLaunchUrls={productLaunchUrls}
          onSelectProduct={onSelectProduct}
          isPending={isProductLaunchPending}
          errorMessage={productLaunchError}
        />
        <AccountMenuPopover
          displayName={userDisplayName ?? 'Account'}
          subtitle={tenantDisplayName}
          preferencesHref={preferencesHref}
          onSignOut={onSignOut}
        />
      </div>
    </header>
  )
}

function resolvePlatformApiBase(platformApiBase: string | undefined, suiteHomeUrl: string): string {
  const explicitBase = platformApiBase?.trim()
  if (explicitBase) {
    return explicitBase.replace(/\/$/, '')
  }

  try {
    const currentHref =
      typeof globalThis.location?.href === 'string' ? globalThis.location.href : 'http://localhost/'
    return new URL(suiteHomeUrl, currentHref).origin
  } catch {
    return ''
  }
}

function resolveProductApiBase(productApiBase: string | undefined): string {
  const explicitBase = productApiBase?.trim()
  if (explicitBase) {
    return explicitBase.replace(/\/$/, '')
  }

  try {
    return typeof globalThis.location?.origin === 'string' ? globalThis.location.origin : ''
  } catch {
    return ''
  }
}

function ProductAppShellFrame({
  productName,
  productKey,
  workspaceSubtitle = 'Operational workspace',
  tenantDisplayName,
  userId,
  tenantId,
  themePreference,
  userDisplayName,
  entitlements = [],
  suiteHomeUrl = 'http://localhost:5174/app',
  platformApiBase,
  productApiBase,
  workspaceAccessToken,
  productLaunchUrls,
  onSelectProduct,
  onSignOut,
  isProductLaunchPending,
  productLaunchError,
  aiAssistance,
  navItems = [{ label: 'Workspace', to: '/' }],
  layoutVariant = 'standard',
  children,
}: ProductAppShellProps) {
  const location = useLocation()
  const navigate = useNavigate()
  const { surface } = usePrintRuntime()
  const resolvedPlatformApiBase = resolvePlatformApiBase(platformApiBase, suiteHomeUrl)
  const resolvedProductApiBase = resolveProductApiBase(productApiBase)
  const resolvedWorkspaceAccessToken = workspaceAccessToken ?? aiAssistance?.accessToken
  const persistThemePreference = useCallback(
    (nextTheme: StlThemeMode) => {
      if (!resolvedPlatformApiBase || !resolvedWorkspaceAccessToken) {
        return undefined
      }
      return updatePlatformThemePreference(
        resolvedPlatformApiBase,
        resolvedWorkspaceAccessToken,
        nextTheme,
      )
    },
    [resolvedPlatformApiBase, resolvedWorkspaceAccessToken],
  )
  const { theme, toggleTheme } = useThemePreference({
    userId,
    tenantId,
    initialTheme: themePreference,
    onThemeChange: persistThemePreference,
  })
  const [aiOpen, setAiOpen] = useState(false)
  const [aiSessionId, setAiSessionId] = useState<string | null>(null)
  const [aiMessages, setAiMessages] = useState<AiHelpMessage[]>([])
  const [aiError, setAiError] = useState<string | null>(null)
  const [aiSending, setAiSending] = useState(false)
  const showSidebar = layoutVariant === 'standard'
  const ProductIcon = getSuiteProductIcon(productKey)
  const aiHelpAvailable = Boolean(aiAssistance?.accessToken)
  const previewSearchParam = surface?.previewSearchParam ?? 'printPreview'
  const previewSearch = new URLSearchParams(location.search)
  const isPrintPreview = previewSearch.get(previewSearchParam) === '1'
  const printableRouteSearch = new URLSearchParams(location.search)
  printableRouteSearch.delete(previewSearchParam)
  const currentRouteRef = printableRouteSearch.toString()
    ? `${location.pathname}?${printableRouteSearch.toString()}`
    : location.pathname
  const navLinkClassName = (isActive: boolean) =>
    [
      'flex min-h-10 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400',
      isActive
        ? 'border-l-2 border-[var(--color-accent)] bg-[var(--color-accent-soft)] pl-[10px] text-[var(--color-text-primary)]'
        : 'border-l-2 border-transparent text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)]',
    ].join(' ')

  const routeIsActive = (to: string) =>
    location.pathname === to || location.pathname.startsWith(`${to}/`)

  const mobileNavLinkClassName = (to: string, forceActive = false) =>
    [
      'flex min-h-10 shrink-0 items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-sky-400',
      forceActive || routeIsActive(to)
        ? 'bg-[var(--color-accent-soft)] text-[var(--color-text-primary)] ring-1 ring-[var(--color-accent-border)]'
        : 'text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)]',
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
    const childActive = item.children?.some((child) => routeIsActive(child.to)) ?? false
    if (item.children?.length && (routeSectionIsActive(item.to) || childActive)) {
      return [item, ...item.children]
    }
    return [item]
  })

  const sendAiMessage = async (message: string) => {
    if (!aiAssistance?.accessToken) {
      setAiError('AI assistance requires an active workspace session.')
      return
    }

    const userMessage: AiHelpMessage = {
      id: `user-${Date.now()}`,
      role: 'user',
      text: message,
    }
    setAiMessages((current) => [...current, userMessage])
    setAiError(null)
    setAiSending(true)
    try {
      const response = await sendProductAiAssistantMessage(
        aiAssistance.apiBase,
        aiAssistance.accessToken,
        {
          sessionId: aiSessionId,
          productKey,
          surface: aiAssistance.surface ?? 'product-shell',
          route: location.pathname,
          category: aiAssistance.category ?? 'guidance',
          message,
          pageContext: {
            productName,
            workspaceSubtitle,
            tenant: tenantDisplayName,
            ...aiAssistance.pageContext,
            navigationLinks: buildAiNavigationLinks({
              currentProductKey: productKey,
              entitlements,
              suiteHomeUrl,
              productLaunchUrls,
              currentNavItems: navItems,
            }),
          },
          allowedBehaviors: aiAssistance.allowedBehaviors ?? [
            'explain',
            'summarize',
            'troubleshoot',
            'recommend',
          ],
        },
      )
      setAiSessionId(response.sessionId)
      setAiMessages((current) => [
        ...current,
        {
          id: response.messageId,
          role: 'assistant',
          text: response.answer,
          outcome: response.outcome,
        },
      ])
    } catch (error) {
      setAiError(error instanceof Error ? error.message : 'AI assistance failed.')
    } finally {
      setAiSending(false)
    }
  }

  const aiDrawer = aiHelpAvailable ? (
    <AiHelpDrawer
      open={aiOpen}
      title="AI assistance"
      productKey={productKey}
      route={location.pathname}
      messages={aiMessages}
      isSending={aiSending}
      errorMessage={aiError}
      onClose={() => setAiOpen(false)}
      onSend={sendAiMessage}
    />
  ) : null

  const enterPrintPreview = () => {
    const next = new URLSearchParams(location.search)
    next.set(previewSearchParam, '1')
    navigate(
      {
        pathname: location.pathname,
        search: `?${next.toString()}`,
      },
      { replace: false },
    )
  }

  const exitPrintPreview = () => {
    const next = new URLSearchParams(location.search)
    next.delete(previewSearchParam)
    navigate(
      {
        pathname: location.pathname,
        search: next.toString() ? `?${next.toString()}` : '',
      },
      { replace: false },
    )
  }

  const printActionBar = surface ? (
    <PrintActionBar
      apiBase={resolvedProductApiBase}
      accessToken={resolvedWorkspaceAccessToken}
      productKey={productKey}
      currentRouteRef={currentRouteRef}
      isPreviewMode={isPrintPreview}
      surface={surface}
      onEnterPreview={enterPrintPreview}
      onExitPreview={exitPrintPreview}
    />
  ) : null

  if (isPrintPreview) {
    return (
      <div className="flex min-h-screen flex-col bg-white text-slate-900" data-print-preview="true">
        {printActionBar}
        {!surface ? (
          <div className="border-b border-slate-200 bg-white px-4 py-3" data-print-hide>
            <button
              type="button"
              className="rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-900"
              onClick={exitPrintPreview}
            >
              Exit preview
            </button>
          </div>
        ) : null}
        <main className="min-h-0 flex-1 overflow-auto px-4 py-6 sm:px-6" data-print-surface>
          {children}
        </main>
      </div>
    )
  }

  if (!showSidebar) {
    return (
      <div className="flex min-h-screen flex-col bg-[var(--color-bg-app)] text-[var(--color-text-primary)]">
        <WorkspaceTopBar
          productName={productName}
          productKey={productKey}
          tenantDisplayName={tenantDisplayName}
          userDisplayName={userDisplayName}
          entitlements={entitlements}
          suiteHomeUrl={suiteHomeUrl}
          productLaunchUrls={productLaunchUrls}
          onSelectProduct={onSelectProduct}
          onSignOut={onSignOut}
          isProductLaunchPending={isProductLaunchPending}
          productLaunchError={productLaunchError}
          onOpenAiHelp={aiHelpAvailable ? () => setAiOpen(true) : undefined}
          theme={theme}
          onToggleTheme={toggleTheme}
        />
        {aiDrawer}
        {printActionBar}
        <main className="min-h-0 flex-1 overflow-auto px-3 pb-8 pt-4 sm:px-4" data-print-surface>
          {children}
        </main>
      </div>
    )
  }

  return (
    <div className="flex min-h-screen bg-[var(--color-bg-app)] text-[var(--color-text-primary)]">
      <aside
        className="hidden min-h-0 w-64 shrink-0 flex-col overflow-y-auto border-r border-[var(--color-border-subtle)] bg-[var(--color-bg-shell)] p-4 lg:flex"
        data-print-hide
      >
        <div className="mb-6 shrink-0">
          <StlComplianceLogo theme={theme} className="h-12 w-[13rem] object-contain object-left" />
        </div>

        <nav aria-label={`${productName} navigation`} className="flex flex-col gap-1">
          {navItems.map((item) => {
            const Icon = item.icon ?? ProductIcon
            const childActive = item.children?.some((child) => routeIsActive(child.to)) ?? false
            const expanded = routeIsActive(item.to) || routeSectionIsActive(item.to) || childActive
            return (
              <div key={item.to} className={item.sectionBreakBefore ? 'mt-2 border-t border-[var(--color-border-subtle)] pt-2' : ''}>
                <NavLink
                  to={item.to}
                  end={item.to === '/'}
                  className={({ isActive }) => navLinkClassName(isActive || childActive)}
                >
                  <Icon className="h-4 w-4 shrink-0" aria-hidden />
                  <span>{item.label}</span>
                </NavLink>
                {item.children?.length && expanded ? (
                  <div className="ml-5 mt-1 flex flex-col gap-1 border-l border-[var(--color-border-subtle)] pl-2">
                    {item.children.map((child) => {
                      const ChildIcon = child.icon ?? Icon
                      return (
                        <NavLink
                          key={child.to}
                          to={child.to}
                          end={child.to === '/'}
                          className={({ isActive }) => navLinkClassName(isActive)}
                        >
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
          tenantDisplayName={tenantDisplayName}
          userDisplayName={userDisplayName}
          entitlements={entitlements}
          suiteHomeUrl={suiteHomeUrl}
          productLaunchUrls={productLaunchUrls}
          onSelectProduct={onSelectProduct}
          onSignOut={onSignOut}
          isProductLaunchPending={isProductLaunchPending}
          productLaunchError={productLaunchError}
          onOpenAiHelp={aiHelpAvailable ? () => setAiOpen(true) : undefined}
          theme={theme}
          onToggleTheme={toggleTheme}
        />
        {aiDrawer}
        {printActionBar}
        <nav
          aria-label={`${productName} mobile navigation`}
          className="border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-shell)] px-3 py-2 lg:hidden"
          data-print-hide
        >
          <div className="flex gap-2 overflow-x-auto pb-1 [&::-webkit-scrollbar]:hidden">
            {mobileNavItems.map((item) => {
              const Icon = item.icon ?? ProductIcon
              const childActive = item.children?.some((child) => routeIsActive(child.to)) ?? false
              return (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.to === '/'}
                  className={mobileNavLinkClassName(item.to, childActive)}
                >
                  <Icon className="h-4 w-4 shrink-0" aria-hidden />
                  <span>{item.label}</span>
                </NavLink>
              )
            })}
          </div>
        </nav>
        <main className="min-h-0 flex-1 overflow-auto p-3 sm:p-4 lg:p-6" data-print-surface>
          {children}
        </main>
      </div>
    </div>
  )
}

export function ProductAppShell(props: ProductAppShellProps) {
  return (
    <PrintRuntimeProvider>
      <ProductAppShellFrame {...props} />
    </PrintRuntimeProvider>
  )
}
