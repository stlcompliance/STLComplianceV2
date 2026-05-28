import { ExternalLink } from 'lucide-react'
import { listEntitledSuiteProducts, normalizeProductKey } from './productCatalog'

export type ProductSwitcherProps = {
  currentProductKey: string
  entitlements: readonly string[]
  suiteHomeUrl: string
  layoutVariant?: 'standard' | 'compact'
}

function resolveSuiteHomeUrl(suiteHomeUrl: string): string {
  const trimmed = suiteHomeUrl.trim()
  if (!trimmed) {
    return 'http://localhost:5174/app'
  }
  return trimmed.endsWith('/app') ? trimmed : `${trimmed.replace(/\/$/, '')}/app`
}

export function ProductSwitcher({
  currentProductKey,
  entitlements,
  suiteHomeUrl,
  layoutVariant = 'standard',
}: ProductSwitcherProps) {
  const suiteUrl = resolveSuiteHomeUrl(suiteHomeUrl)
  const currentKey = normalizeProductKey(currentProductKey)
  const entitledProducts = listEntitledSuiteProducts(entitlements)
  const isCompact = layoutVariant === 'compact'

  if (isCompact) {
    return (
      <a
        href={suiteUrl}
        className="inline-flex items-center gap-1.5 rounded-md border border-slate-600 px-2.5 py-1.5 text-xs text-slate-200 hover:border-teal-500/50 hover:text-white"
      >
        <ExternalLink className="h-3.5 w-3.5 shrink-0" aria-hidden />
        Suite
      </a>
    )
  }

  return (
    <div className="mb-6">
      <p className="px-3 text-xs font-semibold uppercase tracking-wide text-teal-400">
        STL Compliance
      </p>
      <a
        href={suiteUrl}
        className="mt-2 flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium text-slate-300 transition-colors hover:bg-slate-800/50 hover:text-white"
      >
        <ExternalLink className="h-4 w-4 shrink-0" aria-hidden />
        Suite home
      </a>

      <nav aria-label="Products" className="mt-4 flex flex-col gap-1">
        <p className="px-3 text-xs font-semibold uppercase tracking-wide text-slate-500">
          Products
        </p>
        {entitledProducts.length === 0 ? (
          <p className="px-3 pt-1 text-xs text-slate-500">No entitled products</p>
        ) : (
          entitledProducts.map((product) => {
            const Icon = product.icon
            const isCurrent = normalizeProductKey(product.productKey) === currentKey
            const productSuitePath = `${suiteUrl}/${normalizeProductKey(product.productKey)}`

            return (
              <a
                key={product.productKey}
                href={productSuitePath}
                className={[
                  'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                  isCurrent
                    ? 'border-l-2 border-teal-400 bg-slate-800/80 pl-[10px] text-white'
                    : 'border-l-2 border-transparent text-slate-300 hover:bg-slate-800/50 hover:text-white',
                ].join(' ')}
                aria-current={isCurrent ? 'page' : undefined}
              >
                <Icon className="h-4 w-4 shrink-0" aria-hidden />
                <span>{product.displayName}</span>
              </a>
            )
          })
        )}
      </nav>
    </div>
  )
}
