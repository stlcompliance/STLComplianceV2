import { ChevronDown } from 'lucide-react'
import { useEffect, useId, useRef, useState } from 'react'
import {
  getSuiteProductIcon,
  SUITE_PRODUCT_CATALOG,
  normalizeProductKey,
  type SuiteProductCatalogEntry,
} from './productCatalog'
import { resolveProductLaunchUrl } from './productLaunchUrls'

export type ProductSwitcherProps = {
  currentProductKey: string
  suiteHomeUrl: string
  productLaunchUrls?: Record<string, string>
  /** When set, menu items invoke NexArr handoff instead of direct launch URLs. */
  onSelectProduct?: (productKey: string) => void
  isPending?: boolean
  errorMessage?: string | null
}

function findCatalogEntry(productKey: string): SuiteProductCatalogEntry | undefined {
  const normalized = normalizeProductKey(productKey)
  return SUITE_PRODUCT_CATALOG.find(
    (entry) => normalizeProductKey(entry.productKey) === normalized,
  )
}

export function ProductSwitcher({
  currentProductKey,
  suiteHomeUrl,
  productLaunchUrls = {},
  onSelectProduct,
  isPending = false,
  errorMessage = null,
}: ProductSwitcherProps) {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const menuId = useId()
  const currentKey = normalizeProductKey(currentProductKey)
  const catalogProducts = SUITE_PRODUCT_CATALOG
  const CurrentIcon = getSuiteProductIcon(currentKey)
  const currentEntry =
    catalogProducts.find((entry) => normalizeProductKey(entry.productKey) === currentKey) ??
    findCatalogEntry(currentKey)

  useEffect(() => {
    if (!open) {
      return
    }

    function handlePointerDown(event: MouseEvent) {
      if (!containerRef.current?.contains(event.target as Node)) {
        setOpen(false)
      }
    }

    function handleEscape(event: KeyboardEvent) {
      if (event.key === 'Escape') {
        setOpen(false)
      }
    }

    document.addEventListener('mousedown', handlePointerDown)
    document.addEventListener('keydown', handleEscape)
    return () => {
      document.removeEventListener('mousedown', handlePointerDown)
      document.removeEventListener('keydown', handleEscape)
    }
  }, [open])

  return (
    <div ref={containerRef} className="relative max-w-full">
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={menuId}
        disabled={isPending}
        onClick={() => setOpen((value) => !value)}
        className="inline-flex h-9 max-w-[14rem] items-center gap-2 rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-control)] px-3 text-left text-sm text-[var(--color-text-primary)] transition hover:border-[var(--color-accent-border)] hover:bg-[var(--color-bg-control-hover)] focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-[var(--color-focus-ring)] disabled:cursor-not-allowed disabled:opacity-50"
      >
        <CurrentIcon className="h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
        <span className="min-w-0 truncate font-medium">
          {currentEntry?.displayName ?? currentKey}
        </span>
        <ChevronDown
          className={['h-4 w-4 shrink-0 text-[var(--color-text-muted)] transition-transform', open ? 'rotate-180' : ''].join(
            ' ',
          )}
          aria-hidden
        />
      </button>

      {open ? (
        <ul
          id={menuId}
          role="menu"
          aria-label="Switch product"
          className="absolute right-0 z-50 mt-2 max-h-[min(28rem,calc(100vh-6rem))] w-72 max-w-[calc(100vw-2rem)] overflow-y-auto rounded-lg border border-[var(--color-border-strong)] bg-[var(--color-bg-shell)] py-1 shadow-xl [box-shadow:var(--shadow-shell-menu)]"
        >
          {catalogProducts.map((product) => {
            const Icon = product.icon
            const isCurrent = normalizeProductKey(product.productKey) === currentKey
            const href = resolveProductLaunchUrl(
              product.productKey,
              suiteHomeUrl,
              productLaunchUrls,
            )
            const itemClassName = [
              'flex w-full items-start gap-3 px-3 py-2 text-left text-sm transition-colors',
              isCurrent
                ? 'bg-[var(--color-accent-soft)] text-[var(--color-text-primary)]'
                : 'text-[var(--color-text-secondary)] hover:bg-[var(--color-bg-control-hover)] hover:text-[var(--color-text-primary)] focus-visible:bg-[var(--color-bg-control-hover)] focus-visible:text-[var(--color-text-primary)]',
            ].join(' ')

            return (
              <li key={product.productKey} role="none">
                {onSelectProduct ? (
                  <button
                    type="button"
                    role="menuitem"
                    aria-current={isCurrent ? 'true' : undefined}
                    disabled={isPending}
                    onClick={() => {
                      setOpen(false)
                      if (!isCurrent) {
                        onSelectProduct(product.productKey)
                      }
                    }}
                    className={[itemClassName, 'disabled:opacity-50'].join(' ')}
                  >
                    <Icon className="mt-0.5 h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
                    <span className="min-w-0">
                      <span className="block font-medium">{product.displayName}</span>
                      {product.description ? (
                        <span className="mt-0.5 block text-xs text-[var(--color-text-muted)]">
                          {product.description}
                        </span>
                      ) : null}
                    </span>
                  </button>
                ) : (
                  <a
                    role="menuitem"
                    href={href}
                    aria-current={isCurrent ? 'true' : undefined}
                    onClick={() => setOpen(false)}
                    className={itemClassName}
                  >
                    <Icon className="mt-0.5 h-4 w-4 shrink-0 text-[var(--color-accent)]" aria-hidden />
                    <span className="min-w-0">
                      <span className="block font-medium">{product.displayName}</span>
                      {product.description ? (
                        <span className="mt-0.5 block text-xs text-[var(--color-text-muted)]">
                          {product.description}
                        </span>
                      ) : null}
                    </span>
                  </a>
                )}
              </li>
            )
          })}
        </ul>
      ) : null}

      {errorMessage ? (
        <p className="absolute right-0 mt-1 w-72 max-w-[calc(100vw-2rem)] text-xs text-[var(--color-destructive-text)]" role="alert">
          {errorMessage}
        </p>
      ) : null}
    </div>
  )
}
