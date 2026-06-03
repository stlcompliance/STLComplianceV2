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
        description="Overview of the STL Compliance product suite for people, training, maintenance, dispatch, supply, warehouse work, field work, and compliance proof."
        path="/products"
      />
      <PageHero
        eyebrow="Product suite"
        title="Connected tools for the work that creates compliance risk"
        subtitle="Every product focuses on a real part of operations. Together, they help teams know who can work, what is ready, what is missing, and what proof exists."
      />

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-lg font-semibold text-white">Quick comparison</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          A simple view of what each product helps your team manage.
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
              {products.length} product{products.length === 1 ? '' : 's'} in this part of the
              suite.
            </p>
            <div className="mt-6 grid gap-4 sm:grid-cols-2">
              {products.map((product) => (
                <ProductCard key={product.productKey} product={product} />
              ))}
            </div>
          </section>
        )
      })}

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <p className="text-center text-sm text-slate-500">
          {MARKETING_PRODUCTS.length} products · secure customer sign-in through NexArr
        </p>
      </section>
    </>
  )
}
