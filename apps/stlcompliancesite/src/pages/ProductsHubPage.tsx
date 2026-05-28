import { PageHero } from '../components/PageHero'
import { ProductCard } from '../components/ProductCard'
import { ProductsComparisonTable } from '../components/ProductsComparisonTable'
import { SiteSeo } from '../components/SiteSeo'
import {
  MARKETING_PRODUCTS,
  PRODUCT_CATEGORY_LABELS,
  PRODUCT_CATEGORY_ORDER,
  productsByCategory,
} from '../content/products'
import { siteConfig } from '../lib/siteConfig'

export function ProductsHubPage() {
  return (
    <>
      <SiteSeo
        title={`Products — ${siteConfig.siteName}`}
        description="Overview of NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, and Companion with accurate ownership and V1 maturity labels."
        path="/products"
      />
      <PageHero
        eyebrow="Products hub"
        title="One suite, explicit ownership"
        subtitle="Use this hub to explain what each product owns before a customer signs in through NexArr and the suite shell. Maturity labels describe what ships in V1 — not marketing-only mock screens."
      />

      <section className="mx-auto max-w-6xl px-4 pb-8 sm:px-6">
        <div className="rounded-2xl border border-slate-600 bg-slate-900/40 px-5 py-4 text-sm text-slate-300">
          <strong className="font-semibold text-white">Public capability accuracy:</strong>{' '}
          <span className="text-teal-300">V1 operational</span> means a real API, PostgreSQL database,
          worker where applicable, and authenticated product UI.{' '}
          <span className="text-amber-200">V1 partial</span> means the surface ships with clear
          boundaries (for example Companion aggregates inbox items but does not own workflow state).
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-lg font-semibold text-white">Comparison</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          Quick reference for sales and security reviewers. Detail pages expand owns / does-not-own
          boundaries per product.
        </p>
        <div className="mt-6">
          <ProductsComparisonTable />
        </div>
      </section>

      {PRODUCT_CATEGORY_ORDER.map((category) => {
        const products = productsByCategory(category)
        if (products.length === 0) {
          return null
        }
        return (
          <section key={category} className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
            <h2 className="text-xl font-bold text-white">{PRODUCT_CATEGORY_LABELS[category]}</h2>
            <p className="mt-2 text-sm text-slate-400">
              {products.length} product{products.length === 1 ? '' : 's'} in this layer of the suite.
            </p>
            <div className="mt-6 grid gap-4 sm:grid-cols-2">
              {products.map((product) => (
                <ProductCard key={product.productKey} product={product} showMaturity />
              ))}
            </div>
          </section>
        )
      })}

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <p className="text-center text-sm text-slate-500">
          {MARKETING_PRODUCTS.length} products · marketing site only · sign-in via NexArr
        </p>
      </section>
    </>
  )
}
