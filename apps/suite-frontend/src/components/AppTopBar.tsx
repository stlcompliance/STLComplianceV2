import { useState } from 'react'
import { useLocation } from 'react-router-dom'
import { sendAiAssistantMessage } from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { ProductSwitcher } from './ProductSwitcher'
import { AiHelpButton, AiHelpDrawer, type AiHelpMessage } from '@stl/shared-ui/AiHelpDrawer'
import { AccountMenuPopover } from '@stl/shared-ui/AccountMenuPopover'
import { ProductBrandLogo } from '@stl/shared-ui/BrandLogos'
import { ThemeToggleButton } from '@stl/shared-ui/ThemeToggleButton'
import { buildAiNavigationLinks } from '@stl/shared-ui/aiNavigationLinks'
import { buildProductLaunchUrlMap } from '@stl/shared-ui/productLaunchUrls'
import { getSuiteProductCatalogEntry } from '@stl/shared-ui/productCatalog'
import { normalizeProductKey } from '@stl/shared-ui/productCatalog'
import { useHintsPreference } from '@stl/shared-ui/HintsPreferenceContext'
import type { StlThemeMode } from '@stl/shared-ui/theme'

const suiteHomeUrl = '/app'
const productLaunchUrls = buildProductLaunchUrlMap(import.meta.env)

function resolveTitle(pathname: string, isPlatformAdminUser: boolean): { title: string; subtitle: string } {
  const preferenceMatch = /^\/app\/([^/]+)\/preferences\/?$/.exec(pathname)
  if (preferenceMatch || pathname === '/app/preferences' || pathname === '/app/preferences/') {
    return {
      title: 'Preferences',
      subtitle: 'Personal preferences for this app.',
    }
  }

  if (pathname.startsWith('/app/platform-admin')) {
    return { title: 'Platform administration', subtitle: 'NexArr platform workspace' }
  }

  if (pathname.startsWith('/app/imports')) {
    return { title: 'Smart Import', subtitle: 'NexArr intake and review' }
  }

  if (pathname === '/app' || pathname === '/app/') {
    return isPlatformAdminUser
      ? { title: 'Suite dashboard', subtitle: 'Cross-product overview' }
      : { title: 'Product launcher', subtitle: 'Choose a product to launch' }
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
  const preferenceMatch = /^\/app\/([^/]+)\/preferences\/?$/.exec(pathname)
  if (preferenceMatch) {
    const productKey = normalizeProductKey(preferenceMatch[1])
    return getSuiteProductCatalogEntry(productKey) ? productKey : 'nexarr'
  }

  if (pathname === '/app/preferences' || pathname === '/app/preferences/') {
    return 'nexarr'
  }

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

export function AppTopBar({
  theme,
  onToggleTheme,
}: {
  theme: StlThemeMode
  onToggleTheme: () => void
}) {
  const { me, logout } = useAuth()
  const location = useLocation()
  const { title, subtitle } = resolveTitle(location.pathname, me?.isPlatformAdmin === true)
  const productKey = resolveCurrentProductKey(location.pathname)
  const productMatch =
    location.pathname === '/app/preferences' || location.pathname === '/app/preferences/'
      ? null
      : /^\/app\/([^/]+)/.exec(location.pathname)
  const matchedProductKey = productMatch ? normalizeProductKey(productMatch[1]) : null
  const matchedProduct = matchedProductKey ? getSuiteProductCatalogEntry(matchedProductKey) : undefined
  const currentProduct = getSuiteProductCatalogEntry(productKey)
  const topbarLogoLabel = matchedProduct?.displayName ?? currentProduct?.displayName ?? title
  const [aiSessionId, setAiSessionId] = useState<string | null>(null)
  const [aiMessages, setAiMessages] = useState<AiHelpMessage[]>([])
  const [aiError, setAiError] = useState<string | null>(null)
  const [aiSending, setAiSending] = useState(false)
  const { showHints, setShowHints } = useHintsPreference()
  const closeHints = () => {
    setAiError(null)
    setShowHints(false)
  }

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
      console.error('Suite AI assistance failed', error)
      setAiError('AI assistance is temporarily unavailable. Please try again.')
    } finally {
      setAiSending(false)
    }
  }

  const toggleHints = () => {
    const next = !showHints
    if (next) {
      setShowHints(true)
    } else {
      closeHints()
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
          <ThemeToggleButton theme={theme} onToggle={onToggleTheme} />
          <AiHelpButton onClick={toggleHints} label={showHints ? 'Hide hints' : 'Show hints'} />
          <ProductSwitcher />
          {me && (
          <AccountMenuPopover
            displayName={me.displayName}
            subtitle={me.tenantDisplayName}
            preferencesHref={`/app/${productKey}/preferences`}
            onSignOut={() => void logout()}
          />
          )}
        </div>
      </header>
      <AiHelpDrawer
        open={showHints}
        title="AI assistance"
        productKey={productKey}
        route={location.pathname}
        messages={aiMessages}
        isSending={aiSending}
        errorMessage={aiError}
        onClose={closeHints}
        onSend={sendMessage}
      />
    </>
  )
}
