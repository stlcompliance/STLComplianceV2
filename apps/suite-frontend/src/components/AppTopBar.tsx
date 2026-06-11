import { LayoutDashboard } from 'lucide-react'
import {
  AiHelpButton,
  AiHelpDrawer,
  getSuiteProductCatalogEntry,
  getSuiteProductIcon,
  type AiHelpMessage,
} from '@stl/shared-ui'
import { useState } from 'react'
import { useLocation } from 'react-router-dom'
import { sendAiAssistantMessage } from '../api/nexarrClient'
import { useAuth } from '../auth/AuthProvider'
import { ProductSwitcher } from './ProductSwitcher'
import { normalizeProductKey } from '../navigation/suiteNavigation'

function resolveTitle(pathname: string): { title: string; subtitle: string } {
  if (pathname.startsWith('/app/platform-admin')) {
    return { title: 'Platform administration', subtitle: 'NexArr control plane' }
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

export function AppTopBar() {
  const { me } = useAuth()
  const location = useLocation()
  const { title, subtitle } = resolveTitle(location.pathname)
  const productMatch = /^\/app\/([^/]+)/.exec(location.pathname)
  const productKey = productMatch ? normalizeProductKey(productMatch[1]) : 'nexarr'
  const ProductIcon = productMatch ? getSuiteProductIcon(productKey) : LayoutDashboard
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
          tenant: me?.tenantDisplayName,
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
      <header className="flex shrink-0 items-center justify-between border-b border-slate-700/40 bg-stl-navy px-6 py-4 text-white">
        <div className="flex min-w-0 items-center gap-3">
          <ProductIcon className="h-5 w-5 shrink-0 text-stl-teal" aria-hidden />
          <div className="min-w-0">
            <h2 className="truncate text-base font-semibold">{title}</h2>
            <p className="truncate text-xs text-slate-300">{subtitle}</p>
          </div>
        </div>

        <div className="flex items-center gap-4 text-sm">
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
              <p data-testid="suite-tenant-display-name" className="text-xs text-slate-300">
                {me.tenantDisplayName}
              </p>
              <p data-testid="suite-tenant-slug" className="font-mono text-xs text-slate-400">
                {me.tenantSlug}
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
