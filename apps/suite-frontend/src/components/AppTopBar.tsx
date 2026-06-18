import { useState } from 'react'
import { useLocation } from 'react-router-dom'
import { sendAiAssistantMessage, updateMyPreferences } from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { ProductSwitcher } from './ProductSwitcher'
import { AiHelpButton, AiHelpDrawer, type AiHelpMessage } from '@stl/shared-ui/AiHelpDrawer'
import { ProductBrandLogo } from '@stl/shared-ui/BrandLogos'
import { ThemeToggleButton } from '@stl/shared-ui/ThemeToggleButton'
import { buildAiNavigationLinks } from '@stl/shared-ui/aiNavigationLinks'
import { buildProductLaunchUrlMap } from '@stl/shared-ui/productLaunchUrls'
import { getSuiteProductCatalogEntry } from '@stl/shared-ui/productCatalog'
import { normalizeProductKey } from '@stl/shared-ui/productCatalog'
import { useThemePreference } from '@stl/shared-ui/useThemePreference'

const suiteHomeUrl = '/app'
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

function resolveTitle(pathname: string): { title: string; subtitle: string } {
  if (pathname.startsWith('/app/platform-admin')) {
    return { title: 'Platform administration', subtitle: 'NexArr control plane' }
  }

  if (pathname.startsWith('/app/imports')) {
    return { title: 'Smart Import', subtitle: 'NexArr intake and review' }
  }

  if (pathname === '/app' || pathname === '/app/') {
    return { title: 'Suite dashboard', subtitle: 'Cross-product overview' }
  }

  const match = /^\/app\/([^/]+)/.exec(pathname)
  if (!match) {
    return { title: 'Authenticated workspace', subtitle: 'STL Compliance Suite' }
  }

  const productKey = normalizeProductKey(match[1])
  const product = getSuiteProductCatalogEntry(productKey)
  return {
    title: product?.displayName ?? productKey.charAt(0).toUpperCase() + productKey.slice(1),
    subtitle: 'Product workspace',
  }
}

function resolveCurrentProductKey(pathname: string): string {
  if (pathname.startsWith('/app/platform-admin') || pathname.startsWith('/app/imports')) {
    return 'nexarr'
  }

  const match = /^\/app\/([^/]+)/.exec(pathname)
  if (!match) {
    return 'nexarr'
  }

  const productKey = normalizeProductKey(match[1])
  return getSuiteProductCatalogEntry(productKey) ? productKey : 'nexarr'
}

export function AppTopBar() {
  const { me } = useAuth()
  const location = useLocation()
  const { title, subtitle } = resolveTitle(location.pathname)
  const productKey = resolveCurrentProductKey(location.pathname)
  const { theme, toggleTheme } = useThemePreference({
    userId: me?.userId,
    tenantId: me?.tenantId,
    initialTheme: me?.themePreference,
    onThemeChange: async (themePreference) => {
      await updateMyPreferences({ themePreference })
    },
  })
  const productMatch = /^\/app\/([^/]+)/.exec(location.pathname)
  const matchedProductKey = productMatch ? normalizeProductKey(productMatch[1]) : null
  const matchedProduct = matchedProductKey ? getSuiteProductCatalogEntry(matchedProductKey) : undefined
  const topbarLogoLabel = matchedProduct?.displayName ?? title
  const [aiOpen, setAiOpen] = useState(false)
  const [aiSessionId, setAiSessionId] = useState<string | null>(null)
  const [aiMessages, setAiMessages] = useState<AiHelpMessage[]>([])
  const [aiError, setAiError] = useState<string | null>(null)
  const [aiSending, setAiSending] = useState(false)

  const sendMessage = async (message: string) => {
    const userMessage: AiHelpMessage = {
      id: `user-${Date.now()}`,
      role: 'user',
      text: message,
    }
    setAiMessages((current) => [...current, userMessage])
    setAiError(null)
    setAiSending(true)
    try {
      const response = await sendAiAssistantMessage({
        sessionId: aiSessionId,
        productKey,
        surface: 'suite-shell',
        route: location.pathname,
        category: 'guidance',
        message,
        pageContext: {
          title,
          subtitle,
          tenant: me?.tenantDisplayName,
          navigationLinks: buildAiNavigationLinks({
            currentProductKey: productKey,
            entitlements: me?.entitlements ?? [],
            suiteHomeUrl,
            productLaunchUrls,
          }),
        },
        allowedBehaviors: ['explain', 'summarize', 'troubleshoot', 'recommend'],
      })
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

  return (
    <>
      <header className="flex shrink-0 flex-wrap items-center justify-between gap-3 border-b border-[var(--color-border-subtle)] bg-[var(--color-bg-shell)] px-3 py-3 text-[var(--color-text-primary)] sm:px-5">
        <div className="flex min-w-[9rem] max-w-[52vw] items-center">
          <ProductBrandLogo
            productName={topbarLogoLabel}
            productKey={productKey}
            theme={theme}
            className="h-9 w-[11rem] max-w-full object-contain object-left sm:w-[13rem]"
          />
        </div>

        <div className="flex min-w-0 flex-1 flex-wrap items-center justify-end gap-2 text-sm sm:gap-3">
          <ThemeToggleButton theme={theme} onToggle={toggleTheme} />
          <AiHelpButton onClick={() => setAiOpen(true)} />
          <ProductSwitcher />
          {me && (
            <div
              data-testid="suite-user-chrome"
              className="hidden text-right sm:block"
            >
              <p data-testid="suite-user-display-name" className="font-medium">
                {me.displayName}
              </p>
              <p data-testid="suite-tenant-display-name" className="text-xs text-[var(--color-text-muted)]">
                {me.tenantDisplayName}
              </p>
            </div>
          )}
        </div>
      </header>
      <AiHelpDrawer
        open={aiOpen}
        title="AI assistance"
        productKey={productKey}
        route={location.pathname}
        messages={aiMessages}
        isSending={aiSending}
        errorMessage={aiError}
        onClose={() => setAiOpen(false)}
        onSend={sendMessage}
      />
    </>
  )
}
