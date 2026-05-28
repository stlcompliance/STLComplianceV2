import { ChevronDown } from 'lucide-react'
import { useEffect, useId, useRef, useState } from 'react'
import {
  getSuiteProductIcon,
  listEntitledSuiteProducts,
  normalizeProductKey,
  SUITE_PRODUCT_CATALOG,
  type SuiteProductCatalogEntry,
} from './productCatalog'
import { resolveProductLaunchUrl } from './productLaunchUrls'

export type ProductSwitcherProps = {
  currentProductKey: string
  entitlements: readonly string[]
  suiteHomeUrl: string
  productLaunchUrls?: Record<string, string>
}

function findCatalogEntry(productKey: string): SuiteProductCatalogEntry | undefined {
  const normalized = normalizeProductKey(productKey)
  return SUITE_PRODUCT_CATALOG.find(
    (entry) => normalizeProductKey(entry.productKey) === normalized,
  )
}

export function ProductSwitcher({
  currentProductKey,
  entitlements,
  suiteHomeUrl,
  productLaunchUrls = {},
}: ProductSwitcherProps) {
  const [open, setOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)
  const menuId = useId()
  const currentKey = normalizeProductKey(currentProductKey)
  const entitledProducts = listEntitledSuiteProducts(entitlements)
  const CurrentIcon = getSuiteProductIcon(currentKey)
  const currentEntry =
    entitledProducts.find((entry) => normalizeProductKey(entry.productKey) === currentKey) ??
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

  if (entitledProducts.length === 0) {
    return (
      <span className="text-xs text-slate-500" aria-live="polite">
        No entitled products
      </span>
    )
  }

  return (
    <div ref={containerRef} className="relative">
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-controls={menuId}
        onClick={() => setOpen((value) => !value)}
        className="inline-flex max-w-[14rem] items-center gap-2 rounded-md border border-slate-600 bg-slate-900/60 px-3 py-1.5 text-left text-sm text-slate-100 hover:border-teal-500/50 hover:bg-slate-800/80"
      >
        <CurrentIcon className="h-4 w-4 shrink-0 text-teal-400" aria-hidden />
        <span className="min-w-0 truncate font-medium">
          {currentEntry?.displayName ?? currentKey}
        </span>
        <ChevronDown
          className={['h-4 w-4 shrink-0 text-slate-400 transition-transform', open ? 'rotate-180' : ''].join(
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
          className="absolute right-0 z-50 mt-2 w-72 overflow-hidden rounded-md border border-slate-600 bg-[#0a101c] py-1 shadow-xl"
        >
          {entitledProducts.map((product) => {
            const Icon = product.icon
            const isCurrent = normalizeProductKey(product.productKey) === currentKey
            const href = resolveProductLaunchUrl(
              product.productKey,
              suiteHomeUrl,
              productLaunchUrls,
            )

            return (
              <li key={product.productKey} role="none">
                <a
                  role="menuitem"
                  href={href}
                  aria-current={isCurrent ? 'true' : undefined}
                  onClick={() => setOpen(false)}
                  className={[
                    'flex items-start gap-3 px-3 py-2 text-sm transition-colors',
                    isCurrent
                      ? 'bg-slate-800/80 text-white'
                      : 'text-slate-200 hover:bg-slate-800/50 hover:text-white',
                  ].join(' ')}
                >
                  <Icon className="mt-0.5 h-4 w-4 shrink-0 text-teal-400" aria-hidden />
                  <span className="min-w-0">
                    <span className="block font-medium">{product.displayName}</span>
                    {product.description ? (
                      <span className="mt-0.5 block text-xs text-slate-400">{product.description}</span>
                    ) : null}
                  </span>
                </a>
              </li>
            )
          })}
        </ul>
      ) : null}
    </div>
  )
}
