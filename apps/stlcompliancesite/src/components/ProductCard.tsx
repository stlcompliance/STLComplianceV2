import { Link } from 'react-router-dom'
import type { MarketingProduct } from '../content/products'
import { MATURITY_LABELS, productPagePath } from '../content/products'

type ProductCardProps = {
  product: MarketingProduct
  showMaturity?: boolean
}

export function ProductCard({ product, showMaturity = false }: ProductCardProps) {
  const Icon = product.icon
  return (
    <Link
      to={productPagePath(product.productKey)}
      className="group flex flex-col rounded-2xl border border-slate-700 bg-slate-900/70 p-6 shadow-sm transition hover:border-teal-500/60 hover:bg-slate-900"
    >
      <div className="flex items-center gap-3">
        <span className="rounded-xl bg-teal-950/80 p-2 text-teal-300">
          <Icon className="h-6 w-6" aria-hidden />
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-lg font-semibold text-white group-hover:text-teal-200">
            {product.displayName}
          </h2>
          {showMaturity ? (
            <p className="mt-1 text-xs font-medium text-slate-400">
              {MATURITY_LABELS[product.maturity]} · {product.maturityLabel}
            </p>
          ) : null}
        </div>
      </div>
      <p className="mt-3 flex-1 text-sm text-slate-300">{product.tagline}</p>
      <span className="mt-4 text-sm font-medium text-teal-400">Learn ownership →</span>
    </Link>
  )
}
