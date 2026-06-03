import { Link } from 'react-router-dom'
import type { MarketingProduct } from '../content/products'
import { productPagePath } from '../content/products'

type ProductCardProps = {
  product: MarketingProduct
}

export function ProductCard({ product }: ProductCardProps) {
  const Icon = product.icon
  return (
    <Link
      to={productPagePath(product.productKey)}
      className="group relative flex flex-col overflow-hidden rounded-2xl border border-slate-700 bg-slate-900/70 p-6 shadow-sm transition hover:border-teal-500/60 hover:bg-slate-900"
    >
      <div
        className={`pointer-events-none absolute inset-x-0 top-0 h-24 bg-gradient-to-b ${product.brandAccentClass}`}
        aria-hidden
      />
      <div className="flex items-center gap-3">
        <span className="relative flex h-12 w-12 shrink-0 items-center justify-center rounded-xl border border-slate-700 bg-white p-1.5 shadow-sm">
          <img
            src={product.brandImageSrc}
            alt=""
            className="max-h-full max-w-full object-contain"
            aria-hidden
          />
          <Icon className="sr-only" aria-hidden />
        </span>
        <div className="min-w-0 flex-1">
          <h2 className="text-lg font-semibold text-white group-hover:text-teal-200">
            {product.displayName}
          </h2>
        </div>
      </div>
      <p className="mt-3 flex-1 text-sm text-slate-300">{product.tagline}</p>
      <span className="mt-4 text-sm font-medium text-teal-400">See what it does →</span>
    </Link>
  )
}
