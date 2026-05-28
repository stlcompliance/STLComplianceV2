import { Link } from 'react-router-dom'
import type { MarketingProduct } from '../content/products'
import { productPagePath } from '../content/products'

export function ProductCard({ product }: { product: MarketingProduct }) {
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
        <h2 className="text-lg font-semibold text-white group-hover:text-teal-200">
          {product.displayName}
        </h2>
      </div>
      <p className="mt-3 flex-1 text-sm text-slate-300">{product.tagline}</p>
      <span className="mt-4 text-sm font-medium text-teal-400">Learn ownership →</span>
    </Link>
  )
}
