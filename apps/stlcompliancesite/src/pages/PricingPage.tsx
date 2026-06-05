import { Link } from 'react-router-dom'
import { BrandLogoFrame } from '../components/BrandLogoFrame'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import {
  ENTITLEMENT_EXAMPLES,
  LICENSING_PILLARS,
  PRICING_DISCLAIMER,
} from '../content/pricing'
import { getMarketingProduct, productPagePath } from '../content/products'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

export function PricingPage() {
  return (
    <>
      <SiteSeo
        title={`Pricing / Request Access — ${siteConfig.siteName}`}
        description="Pricing and access are scoped by product mix, operational scale, and compliance complexity."
        path="/pricing"
      />
      <PageHero
        eyebrow="Pricing"
        title="Start with the products your operation needs"
        subtitle="STL Compliance is scoped around your product mix, workforce size, sites, and compliance needs. The goal is practical packaging, not a shopping-cart surprise."
      >
        <Link
          to="/contact"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Request access
        </Link>
        <a
          href={suiteLoginUrl()}
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Client sign in
        </a>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 pb-8 sm:px-6">
        <div
          data-testid="pricing-disclaimer"
          className="rounded-2xl border border-amber-500/30 bg-amber-950/20 px-5 py-4 text-sm text-amber-100"
        >
          <strong className="font-semibold">Pricing note:</strong> {PRICING_DISCLAIMER}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">How packaging works</h2>
        <div className="mt-6 grid gap-4 sm:grid-cols-2">
          {LICENSING_PILLARS.map((pillar) => (
            <article
              key={pillar.id}
              data-testid={`pricing-pillar-${pillar.id}`}
              className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5"
            >
              <h3 className="text-lg font-semibold text-teal-200">{pillar.title}</h3>
              <p className="mt-2 text-sm text-slate-300">{pillar.body}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Common product mix</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          Customers mix products to match the work they need to control first, then expand as more
          teams and workflows come online.
        </p>
        <ul className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {ENTITLEMENT_EXAMPLES.map((item) => {
            const product = getMarketingProduct(item.productKey)

            return (
              <li
                key={item.productKey}
                className="rounded-xl border border-slate-700 bg-slate-950/50 px-4 py-3 text-sm"
              >
                {product ? (
                  <BrandLogoFrame src={product.brandImageSrc} size="sm" className="mb-3" />
                ) : null}
                <Link
                  to={productPagePath(item.productKey)}
                  className="font-semibold text-teal-400 hover:text-teal-300"
                >
                  {item.displayName}
                </Link>
                <p className="mt-1 text-slate-400">{item.summary}</p>
              </li>
            )
          })}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="rounded-2xl border border-slate-600 bg-slate-900/40 px-6 py-8 text-center">
          <h2 className="text-lg font-semibold text-white">Ready for commercial terms?</h2>
          <p className="mx-auto mt-2 max-w-2xl text-sm text-slate-400">
            Contact the team for packaging aligned to your fleet size, workforce, sites, product
            mix, and compliance scope. Existing customers can use client sign-in.
          </p>
          <div className="mt-6 flex flex-wrap justify-center gap-3">
            <Link
              to="/contact"
              className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
            >
              Demo & contact
            </Link>
            <Link
              to="/products"
              className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
            >
              Explore products
            </Link>
          </div>
          <p className="mt-6 text-xs text-slate-500">
            No shopping cart, payment form, or license activation exists on this public site.
          </p>
        </div>
      </section>
    </>
  )
}
