import { PageHero } from '../components/PageHero'
import { ProductCard } from '../components/ProductCard'
import { SiteSeo } from '../components/SiteSeo'
import { MARKETING_PRODUCTS } from '../content/products'
import { siteConfig } from '../lib/siteConfig'

export function ProductsHubPage() {
  return (
    <>
      <SiteSeo
        title={`Products — ${siteConfig.siteName}`}
        description="Overview of NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, and Companion with accurate ownership language."
      />
      <PageHero
        eyebrow="Products hub"
        title="One suite, explicit ownership"
        subtitle="Use this hub to explain what each product owns before a customer signs in through NexArr and the suite shell."
      />
      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="grid gap-4 sm:grid-cols-2">
          {MARKETING_PRODUCTS.map((product) => (
            <ProductCard key={product.productKey} product={product} />
          ))}
        </div>
      </section>
    </>
  )
}
