import { useMemo, useState, type FormEvent, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { ArrowRight, Bot, Sparkles } from 'lucide-react'
import { ApiErrorCallout, getErrorMessage, ProductBrandLogo } from '@stl/shared-ui'
import { buildAiNavigationLinks } from '@stl/shared-ui/aiNavigationLinks'
import { buildProductLaunchUrlMap } from '@stl/shared-ui/productLaunchUrls'
import { useHintsPreference } from '@stl/shared-ui/HintsPreferenceContext'
import type { MeResponse, NavigationItem } from '../api/types'
import { sendAiAssistantMessage } from '../api/nexarrClient'
import { useProductLaunch } from '../hooks/useProductLaunch'
import { buildQuickLaunchProducts } from '../lib/dashboard'
import { resolveLaunchpadDeepLink } from '../lib/launchpadAi'

type LaunchPadPageProps = {
  me: MeResponse
  navigationProducts: readonly NavigationItem[]
}

type AssistantState = {
  answer: string
  link: ReturnType<typeof resolveLaunchpadDeepLink>
  sessionId: string | null
}

function isInternalHref(href: string): boolean {
  return href.startsWith('/')
}

function LaunchLink({
  href,
  className,
  label,
  children,
}: {
  href: string
  className: string
  label?: string
  children?: ReactNode
}) {
  if (isInternalHref(href)) {
    return (
      <Link to={href} className={className}>
        {children ?? label}
      </Link>
    )
  }

  return (
    <a href={href} className={className}>
      {children ?? label}
    </a>
  )
}

export function LaunchPadPage({ me, navigationProducts }: LaunchPadPageProps) {
  const { showHints } = useHintsPreference()
  const launch = useProductLaunch()
  const productLaunchUrls = useMemo(() => buildProductLaunchUrlMap(import.meta.env), [])
  const aiNavigationLinks = useMemo(
    () =>
      buildAiNavigationLinks({
        currentProductKey: 'nexarr',
        entitlements: me.entitlements,
        suiteHomeUrl: '/app',
        productLaunchUrls,
      }),
    [me.entitlements, productLaunchUrls],
  )
  const launchProducts = useMemo(
    () => buildQuickLaunchProducts(navigationProducts, me.entitlements),
    [me.entitlements, navigationProducts],
  )
  const [question, setQuestion] = useState('')
  const [assistantState, setAssistantState] = useState<AssistantState | null>(null)
  const [assistantError, setAssistantError] = useState<string | null>(null)
  const [assistantSending, setAssistantSending] = useState(false)

  const handleAsk = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const trimmed = question.trim()
    if (!trimmed) {
      return
    }

    setAssistantError(null)
    setAssistantSending(true)

    try {
      const response = await sendAiAssistantMessage({
        sessionId: assistantState?.sessionId,
        productKey: 'nexarr',
        surface: 'suite-launchpad',
        route: '/app',
        category: 'guidance',
        message: trimmed,
        pageContext: {
          title: 'Product launcher',
          subtitle: 'Choose a product or ask for routing guidance.',
          tenant: me.tenantDisplayName,
          navigationLinks: aiNavigationLinks,
        },
        allowedBehaviors: ['explain', 'summarize', 'troubleshoot', 'recommend'],
      })

      const link = resolveLaunchpadDeepLink(response.answer, aiNavigationLinks)
      setAssistantState({
        answer: response.answer,
        link,
        sessionId: response.sessionId,
      })
    } catch (error) {
      console.error('Launchpad AI assistance failed', error)
      setAssistantError('AI assistance is temporarily unavailable. Please try again.')
    } finally {
      setAssistantSending(false)
    }
  }

  return (
    <div className="mx-auto max-w-7xl space-y-6">
      <header className="space-y-3">
        <div className="inline-flex items-center gap-2 rounded-full border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] px-3 py-1 text-xs font-semibold uppercase tracking-[0.24em] text-[var(--color-accent)]">
          <Sparkles className="h-3.5 w-3.5" aria-hidden />
          NexArr launchpad
        </div>
        <div className="max-w-3xl space-y-2">
          <h1 className="text-3xl font-semibold tracking-tight text-white sm:text-4xl">
            What do you need to do?
          </h1>
          <p className="text-sm leading-6 text-slate-400 sm:text-base">
            {showHints
              ? 'Select a product to launch, or ask the helper and it will point you to the relevant page or section. NexArr keeps login, tenant, and launch control centralized.'
              : 'Select a product to launch. NexArr keeps login, tenant, and launch control centralized.'}
          </p>
        </div>
      </header>

      <section className="grid gap-5 xl:grid-cols-[minmax(0,1.5fr)_minmax(20rem,0.9fr)]">
        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
            {launchProducts.map((product) => {
              const ProductBadge = (
                <div className="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl border border-white/10 bg-white/5">
                  <ProductBrandLogo
                    productName={product.displayName}
                    productKey={product.productKey}
                    className="h-7 w-10 object-contain"
                  />
                </div>
              )
              const body = (
                <>
                  <div className="flex min-w-0 flex-1 items-start gap-3">
                    {ProductBadge}
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <h2 className="truncate text-sm font-semibold text-white">
                          {product.displayName}
                        </h2>
                        <span className="rounded-full border border-white/10 px-2 py-0.5 text-[10px] font-semibold uppercase tracking-[0.18em] text-slate-300">
                          {product.inSuite ? 'Suite' : product.launchable ? 'Launch' : 'Open'}
                        </span>
                      </div>
                      <p className="mt-1 line-clamp-3 text-xs leading-5 text-slate-400">
                        {product.displayName} workspace and related launch path.
                      </p>
                    </div>
                  </div>
                  <ArrowRight className="h-4 w-4 shrink-0 text-slate-400 transition group-hover:translate-x-0.5 group-hover:text-white" aria-hidden />
                  <span className="sr-only">
                    {product.inSuite ? 'Open' : product.launchable ? 'Launch' : 'Open'}{' '}
                    {product.displayName}
                  </span>
                </>
              )

              if (product.inSuite || !product.launchable) {
                return (
                  <LaunchLink
                    key={product.productKey}
                    href={product.routePath}
                    className="group flex min-h-[7.75rem] items-center gap-3 rounded-3xl border border-[var(--color-border-subtle)] bg-[linear-gradient(180deg,rgba(15,23,42,0.96),rgba(17,24,39,0.96))] p-4 text-left transition hover:-translate-y-0.5 hover:border-[var(--color-accent-border)] hover:shadow-lg hover:shadow-sky-950/20 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)]"
                  >
                    {body}
                  </LaunchLink>
                )
              }

              return (
                <button
                  key={product.productKey}
                  type="button"
                  disabled={launch.isPending}
                  onClick={() => launch.mutate(product.productKey)}
                  className="group flex min-h-[7.75rem] items-center gap-3 rounded-3xl border border-[var(--color-border-subtle)] bg-[linear-gradient(180deg,rgba(15,23,42,0.96),rgba(17,24,39,0.96))] p-4 text-left transition hover:-translate-y-0.5 hover:border-[var(--color-accent-border)] hover:shadow-lg hover:shadow-sky-950/20 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)] disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {body}
                </button>
              )
            })}
          </div>

          {launchProducts.length === 0 ? (
            <div className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 text-sm text-slate-300">
              No launchable products are available for this workspace yet.
            </div>
          ) : null}

          {launch.isError ? (
            <ApiErrorCallout
              message={getErrorMessage(launch.error, 'Failed to launch product.')}
              title="Unable to open product"
            />
          ) : null}
        </div>

        <aside className="space-y-4">
          <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5 shadow-lg shadow-slate-950/20">
            <div className="flex items-center gap-2">
              <div className="flex h-10 w-10 items-center justify-center rounded-2xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)]">
                <Bot className="h-5 w-5 text-[var(--color-accent)]" aria-hidden />
              </div>
              <div>
                <h2 className="text-sm font-semibold text-white">What do you need to do?</h2>
                <p className="text-xs text-slate-400">Ask for the page or section you need.</p>
              </div>
            </div>

            <form className="mt-4 space-y-3" onSubmit={handleAsk}>
              <label className="sr-only" htmlFor="launchpad-question">
                What do you need to do?
              </label>
              <textarea
                id="launchpad-question"
                value={question}
                onChange={(event) => setQuestion(event.target.value)}
                placeholder="Example: I need to review a driver qualification."
                rows={4}
                className="min-h-[6.5rem] w-full rounded-2xl border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] outline-none transition focus:border-[var(--color-accent-border)] focus:ring-2 focus:ring-[var(--color-focus-ring)]"
              />
              <button
                type="submit"
                disabled={assistantSending}
                className="inline-flex w-full items-center justify-center rounded-2xl bg-[var(--color-accent)] px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-[var(--color-accent-hover)] disabled:cursor-not-allowed disabled:opacity-60"
              >
                {assistantSending ? 'Thinking…' : 'Ask NexArr'}
              </button>
            </form>

            {assistantError ? (
              <div className="mt-4">
                <ApiErrorCallout message={assistantError} />
              </div>
            ) : null}

            {assistantState ? (
              <div className="mt-4 rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-app)] p-4">
                <p className="text-xs font-semibold uppercase tracking-[0.2em] text-[var(--color-text-muted)]">
                  Suggested next step
                </p>
                <p className="mt-2 whitespace-pre-wrap text-sm leading-6 text-[var(--color-text-primary)]">
                  {assistantState.answer}
                </p>
                {assistantState.link ? (
                  <LaunchLink
                    href={assistantState.link.href}
                    label={`Open ${assistantState.link.label}`}
                    className="mt-3 inline-flex w-full items-center justify-center rounded-2xl border border-[var(--color-accent-border)] bg-[var(--color-accent-soft)] px-4 py-2 text-sm font-semibold text-[var(--color-accent)] transition hover:border-[var(--color-accent)] hover:text-white"
                  />
                ) : (
                  <p className="mt-3 text-xs text-slate-400">
                    Ask again with a more specific product or task and NexArr will try to map it to
                    a route.
                  </p>
                )}
              </div>
            ) : null}
          </section>

          <section className="rounded-3xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
            <h2 className="text-sm font-semibold text-white">Current workspace</h2>
            <dl className="mt-3 space-y-3 text-sm">
              <div>
                <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                  Signed in as
                </dt>
                <dd className="mt-0.5 text-[var(--color-text-primary)]">{me.displayName}</dd>
                <dd className="text-xs text-slate-400">{me.email}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                  Tenant
                </dt>
                <dd className="mt-0.5 text-[var(--color-text-primary)]">{me.tenantDisplayName}</dd>
                <dd className="text-xs text-slate-400">{me.tenantSlug}</dd>
              </div>
              <div>
                <dt className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                  Entitled products
                </dt>
                <dd className="mt-0.5 text-[var(--color-text-primary)]">{me.entitlements.length}</dd>
              </div>
            </dl>
          </section>
        </aside>
      </section>
    </div>
  )
}
