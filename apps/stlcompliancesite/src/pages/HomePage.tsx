import { Link } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { ProductCard } from '../components/ProductCard'
import { SiteSeo } from '../components/SiteSeo'
import { MARKETING_PRODUCTS } from '../content/products'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

export function HomePage() {
  const featured = MARKETING_PRODUCTS.filter((p) => p.productKey !== 'companion').slice(0, 6)

  return (
    <>
      <SiteSeo
        title={`${siteConfig.siteName} — ${siteConfig.arrTagline} Suite`}
        description={siteConfig.defaultDescription}
        path="/"
        includeOrganizationJsonLd
      />
      <PageHero
        eyebrow={siteConfig.arrTagline}
        title="Operational compliance with clear product boundaries"
        subtitle="STL Compliance is a multi-product suite for workforce readiness, training proof, asset maintenance, transportation execution, supply chain, and compliance authority — each product owns its data and APIs."
      >
        <Link
          to="/demo"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Request a demo
        </Link>
        <Link
          to="/pricing"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Pricing & licensing
        </Link>
        <a
          href={suiteLoginUrl()}
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Client sign in (NexArr)
        </a>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 py-12 sm:px-6">
        <div className="rounded-2xl border border-amber-500/30 bg-amber-950/20 px-5 py-4 text-sm text-amber-100">
          <strong className="font-semibold">V1 implementation maturity:</strong> the suite ships
          real APIs, databases, workers, and authenticated product UIs. This site is marketing and
          education only — it does not grant access, store tenant data, or execute product workflows.
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-white">Suite products</h2>
            <p className="mt-2 max-w-2xl text-slate-300">
              Each Arr product has a dedicated PostgreSQL database and server-enforced permissions.
              Cross-product relationships use APIs, events, and rebuildable mirrors — never shared
              database foreign keys.
            </p>
          </div>
          <div className="flex flex-wrap gap-4 text-sm font-medium">
            <Link to="/products" className="text-teal-400 hover:text-teal-300">
              View all products →
            </Link>
            <Link to="/resources" className="text-teal-400 hover:text-teal-300">
              Resources →
            </Link>
          </div>
        </div>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {featured.map((product) => (
            <ProductCard key={product.productKey} product={product} />
          ))}
        </div>
      </section>
    </>
  )
}
