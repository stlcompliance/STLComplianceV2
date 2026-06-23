import { Link } from 'react-router-dom'
import { MARKETING_PRODUCTS } from '../content/products'
import { PageHero } from '../components/PageHero'
import { ProductCard } from '../components/ProductCard'
import { SiteSeo } from '../components/SiteSeo'

const workExamples = [
  'Too many systems for one job.',
  'Too many places to find proof.',
  'Too many handoffs with no clear owner.',
  'Too much admin to keep readiness visible.',
]

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

export function PlatformOverviewPage() {
  return (
    <>
      <SiteSeo
        title={`Platform Overview — STL Compliance`}
        description="STL Compliance is a connected operational suite for people, training, maintenance, dispatch, supply, warehouse, records, and proof."
        path="/platform-overview"
      />
      <PageHero
        eyebrow="Adaptive Risk Reduction"
        title="One stack for connected operations, built around people and readiness"
        subtitle="STL Compliance helps teams run safer, cleaner, more accountable operations without scattered spreadsheets, disconnected apps, or email chain dependency."
      >
        <Link
          to="/products"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Explore all products
        </Link>
        <Link
          to="/contact"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Ask for a walkthrough
        </Link>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 py-12 sm:px-6">
        <h2 className="text-2xl font-bold text-white">Built for the way work actually happens.</h2>
        <p className="mt-3 max-w-3xl text-slate-300">
          Most teams are forced to collect readiness from many places. STL Compliance keeps people,
          training, assets, routes, inventory, vendors, records, and audit expectations connected
          so decisions can be made in one place and then executed where the work belongs.
        </p>
        <ul className="mt-6 grid gap-3 sm:grid-cols-2">
          {workExamples.map((item) => (
            <li key={item} className="rounded-xl border border-slate-700 bg-slate-900/60 px-4 py-3 text-sm">
              {item}
            </li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-2xl font-bold text-white">One stack. Focused products. Clear handoffs.</h2>
        <p className="mt-3 max-w-3xl text-slate-300">
          Each product in STL Compliance focuses on a specific part of the workflow. The suite keeps
          the next step visible, so supervisors, auditors, and operators can keep moving.
        </p>
        <div className="mt-6 grid gap-4 lg:grid-cols-2">
          <article className="rounded-2xl border border-teal-500/25 bg-teal-950/20 p-6">
            <h3 className="text-lg font-semibold text-teal-200">How it helps daily teams</h3>
            <p className="mt-3 text-sm text-slate-200">
              It helps teams know who can work, what is ready, what is missing, and what proof they
              need to continue.
            </p>
          </article>
          <article className="rounded-2xl border border-slate-700 bg-slate-900/60 p-6">
            <h3 className="text-lg font-semibold text-slate-200">How it helps managers</h3>
            <p className="mt-3 text-sm text-slate-200">
              It helps leaders coordinate multiple workflows at once without waiting for one tool to
              tell the next tool what happened.
            </p>
          </article>
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-2xl font-bold text-white">Common disconnected setup vs STL setup</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          In many operations, software still looks connected only on paper. STL aligns where work
          and accountability actually live.
        </p>
        <ul className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {connectedTools.map((item) => (
            <li
              key={item}
              className="rounded-xl border border-slate-700 bg-slate-950/50 px-4 py-3 text-sm text-slate-300"
            >
              {item}
            </li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <h2 className="text-xl font-bold text-white">Products at a glance</h2>
        <p className="mt-3 max-w-3xl text-sm text-slate-400">
          STL Compliance includes specialized products that are built to own one part of your operation
          and hand cleanly to the next person or process.
        </p>
        <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {MARKETING_PRODUCTS.filter((product) => product.productKey !== 'fieldcompanion').map((product) => (
            <ProductCard key={product.productKey} product={product} />
          ))}
        </div>
      </section>
    </>
  )
}
