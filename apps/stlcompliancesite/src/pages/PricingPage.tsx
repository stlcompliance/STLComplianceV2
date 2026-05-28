import { Link } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import {
  ENTITLEMENT_EXAMPLES,
  LICENSING_PILLARS,
  PRICING_DISCLAIMER,
} from '../content/pricing'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

export function PricingPage() {
  return (
    <>
      <SiteSeo
        title={`Pricing & licensing — ${siteConfig.siteName}`}
        description="How STL Compliance suite licensing works: NexArr tenant entitlements per product, no checkout on this marketing site. Request a walkthrough for commercial terms."
        path="/pricing"
      />
      <PageHero
        eyebrow="Pricing narrative"
        title="Suite licensing through NexArr entitlements"
        subtitle="STL Compliance is sold as a multi-product platform. Access is granted per tenant and per product — enforced server-side, not through this public website."
      >
        <Link
          to="/demo"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Request a walkthrough
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
          <strong className="font-semibold">Marketing only:</strong> {PRICING_DISCLAIMER}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">How licensing works</h2>
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
        <h2 className="text-xl font-bold text-white">Typical entitlement packaging</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          Customers mix products to match operational scope. NexArr records which products are active
          for each tenant; platform administrators manage entitlements — not end users on this site.
        </p>
        <ul className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {ENTITLEMENT_EXAMPLES.map((item) => (
            <li
              key={item.productKey}
              className="rounded-xl border border-slate-700 bg-slate-950/50 px-4 py-3 text-sm"
            >
              <Link
                to={`/products/${item.productKey}`}
                className="font-semibold text-teal-400 hover:text-teal-300"
              >
                {item.displayName}
              </Link>
              <p className="mt-1 text-slate-400">{item.summary}</p>
            </li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="rounded-2xl border border-slate-600 bg-slate-900/40 px-6 py-8 text-center">
          <h2 className="text-lg font-semibold text-white">Ready for commercial terms?</h2>
          <p className="mx-auto mt-2 max-w-2xl text-sm text-slate-400">
            Contact the team for packaging aligned to your fleet size, product mix, and compliance
            scope. Existing entitled users should sign in through NexArr.
          </p>
          <div className="mt-6 flex flex-wrap justify-center gap-3">
            <Link
              to="/demo"
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
            No shopping cart, payment form, or license activation exists on STLComplianceSite.
          </p>
        </div>
      </section>
    </>
  )
}
