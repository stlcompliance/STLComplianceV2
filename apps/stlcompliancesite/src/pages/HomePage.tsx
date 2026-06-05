import { Link } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { ProductCard } from '../components/ProductCard'
import { SiteSeo } from '../components/SiteSeo'
import { MARKETING_PRODUCTS } from '../content/products'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

const proofPoints = [
  'Know who can work.',
  'Know what is ready.',
  'Know what is missing.',
  'Know what you can prove.',
] as const

const connectedTools = [
  'Fleet maintenance software',
  'Training tracking software',
  'EHS/compliance binders',
  'Warehouse systems',
  'Dispatch boards',
  'Shared spreadsheets',
  'Email follow-up',
  'Forms tools',
  'Document folders',
  'Standalone reporting dashboards',
]

export function HomePage() {
  const featured = MARKETING_PRODUCTS.filter((p) => p.productKey !== 'fieldcompanion')

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
        title="Compliance should not live in binders, spreadsheets, and disconnected apps."
        subtitle="STL Compliance connects your people, training, assets, maintenance, dispatch, inventory, vendors, and evidence into one Adaptive Risk Reduction platform."
        brandImageSrc="/brand/stl-fullcolor.png"
        brandImageAlt="STL Compliance"
        backgroundImageSrc="/brand/stl-full-bluebg.png"
      >
        <Link
          to="/contact"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Request a demo
        </Link>
        <Link
          to="/products"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Explore products
        </Link>
        <a
          href={suiteLoginUrl()}
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Client sign in
        </a>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 py-12 sm:px-6">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {proofPoints.map((point) => (
            <div
              key={point}
              className="rounded-2xl border border-teal-500/25 bg-teal-950/20 px-5 py-4 text-sm font-semibold text-teal-100"
            >
              {point}
            </div>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <div className="rounded-2xl border border-slate-700 bg-slate-900/50 px-6 py-7">
          <h2 className="text-2xl font-bold text-white">The problem is scattered information</h2>
          <p className="mt-3 max-w-3xl text-slate-300">
            A driver file in one folder. A training certificate buried in email. A defect found
            during inspection but never turned into a repair. A vendor document that expires before
            anyone notices. Most compliance failures start as everyday handoffs that slipped through
            the cracks.
          </p>
          <p className="mt-3 max-w-3xl text-slate-300">
            STL Compliance helps fix the system around the work, so the right person knows what to
            do, has the authority to do it, and leaves proof behind as the job gets done.
          </p>
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="mb-8 flex flex-wrap items-end justify-between gap-4">
          <div>
            <h2 className="text-2xl font-bold text-white">Suite products</h2>
            <p className="mt-2 max-w-2xl text-slate-300">
              Each product gives a team the depth it needs, while the suite keeps the important
              connections visible: who can work, what is ready, what is missing, and what can be
              proven.
            </p>
          </div>
          <div className="flex flex-wrap gap-4 text-sm font-medium">
            <Link to="/products" className="text-teal-400 hover:text-teal-300">
              View all products →
            </Link>
            <Link to="/compare" className="text-teal-400 hover:text-teal-300">
              Compare approaches →
            </Link>
          </div>
        </div>
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {featured.map((product) => (
            <ProductCard key={product.productKey} product={product} />
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <h2 className="text-2xl font-bold text-white">One stack. Focused products. Shared accountability.</h2>
        <p className="mt-3 max-w-3xl text-slate-300">
          STL Compliance does not ask every team to become experts in every workflow. It gives each team
          a focused workflow and keeps ownership clear across the handoffs.
        </p>
        <h2 className="text-2xl font-bold text-white">Built for the way work actually happens.</h2>
        <p className="mt-3 max-w-3xl text-slate-300">
          Teams move from "I think this is ready" to "we know what is ready, what is missing, and what
          proof is needed." STL Compliance keeps the flow moving so people spend less time chasing
          follow-up and more time getting work done safely.
        </p>
        <div className="mt-6 grid gap-4 sm:grid-cols-2">
          {connectedTools.map((tool) => (
            <article
              key={tool}
              className="rounded-xl border border-slate-700 bg-slate-950/50 px-4 py-3 text-sm text-slate-300"
            >
              {tool}
            </article>
          ))}
        </div>
      </section>
    </>
  )
}
