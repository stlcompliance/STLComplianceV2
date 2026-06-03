import { Link } from 'react-router-dom'
import { BrandLogoFrame } from './BrandLogoFrame'
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
        <BrandLogoFrame src={product.brandImageSrc} size="md" />
        <Icon className="sr-only" aria-hidden />
        <div className="min-w-0 flex-1">
          <h2 className="text-lg font-semibold text-white group-hover:text-teal-200">
            {product.displayName}
          </h2>
        </div>
      </div>
      <p className="mt-3 flex-1 text-sm text-slate-300">{product.tagline}</p>
      <ul className="mt-4 space-y-2 text-xs text-slate-400">
        {product.primaryWorkflows.slice(0, 2).map((workflow) => (
          <li key={workflow} className="flex gap-2">
            <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-teal-300" />
            <span>{workflow}</span>
          </li>
        ))}
      </ul>
      <span className="mt-4 text-sm font-medium text-teal-400">See what it does →</span>
    </Link>
  )
}
