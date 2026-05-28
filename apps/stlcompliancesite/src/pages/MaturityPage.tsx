import { Link } from 'react-router-dom'
import { ProgramMilestoneTable } from '../components/ProgramMilestoneTable'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import {
  MATURITY_DISCLAIMER,
  MATURITY_LEAD,
  OPEN_HONESTY_NOTES,
  PROGRAM_SNAPSHOT,
  VERIFICATION_HIGHLIGHTS,
  maturityBadgeClass,
} from '../content/implementationMaturity'
import { MARKETING_PRODUCTS, MATURITY_LABELS, productPagePath } from '../content/products'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

export function MaturityPage() {
  const operationalCount = MARKETING_PRODUCTS.filter((p) => p.maturity === 'v1-operational').length
  const partialCount = MARKETING_PRODUCTS.length - operationalCount

  return (
    <>
      <SiteSeo
        title={`V1 implementation maturity — ${siteConfig.siteName}`}
        description="Honest public snapshot of STL Compliance V1 progress by product and program milestone — marketing transparency aligned to implementation docs, not live tenant data."
        path="/maturity"
      />
      <PageHero
        eyebrow="Implementation transparency"
        title="V1 maturity by product and milestone"
        subtitle={MATURITY_LEAD}
      >
        <Link
          to="/products"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Products hub
        </Link>
        <Link
          to="/compare"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Compare approaches
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
          data-testid="maturity-disclaimer"
          className="rounded-2xl border border-amber-500/30 bg-amber-950/20 px-5 py-4 text-sm text-amber-100"
        >
          <strong className="font-semibold">Marketing only:</strong> {MATURITY_DISCLAIMER}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Program snapshot</h2>
        <p className="mt-2 text-sm text-slate-400" data-testid="maturity-snapshot">
          Through {PROGRAM_SNAPSHOT.lastUpdatedLabel}: {PROGRAM_SNAPSHOT.completedWorkersThrough}{' '}
          documented worker slices complete. Latest slice: {PROGRAM_SNAPSHOT.latestSliceSummary} (
          <code className="text-teal-300">{PROGRAM_SNAPSHOT.latestCommitShort}</code>). Source of
          truth in-repo: <span className="text-slate-300">{PROGRAM_SNAPSHOT.sliceStateDoc}</span> and{' '}
          <span className="text-slate-300">{PROGRAM_SNAPSHOT.statusDoc}</span>.
        </p>
        <ul className="mt-6 grid gap-3 sm:grid-cols-2">
          {VERIFICATION_HIGHLIGHTS.map((item) => (
            <li
              key={item}
              className="rounded-xl border border-slate-700 bg-slate-950/50 px-4 py-3 text-sm text-slate-300"
            >
              {item}
            </li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Product capability labels</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          Public labels match the products hub comparison table.{' '}
          <span className="text-slate-200">
            {operationalCount} V1 operational, {partialCount} V1 partial
          </span>{' '}
          (of {MARKETING_PRODUCTS.length} marketed products).
        </p>
        <ul className="mt-6 grid gap-3 sm:grid-cols-2">
          {MARKETING_PRODUCTS.map((product) => (
            <li
              key={product.productKey}
              data-testid={`maturity-product-${product.productKey}`}
              className="rounded-xl border border-slate-700 bg-slate-900/60 px-4 py-4"
            >
              <div className="flex flex-wrap items-center justify-between gap-2">
                <Link
                  to={productPagePath(product.productKey)}
                  className="font-semibold text-teal-400 hover:text-teal-300"
                >
                  {product.displayName}
                </Link>
                <span className={maturityBadgeClass(product.maturity)}>
                  {MATURITY_LABELS[product.maturity]}
                </span>
              </div>
              <p className="mt-2 text-sm text-slate-300">{product.maturityLabel}</p>
            </li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Milestone posture (M0–M13)</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          Summarized from the milestone masterplan and worker slice history — not a per-feature
          matrix export.
        </p>
        <div className="mt-6">
          <ProgramMilestoneTable />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">What this page does not claim</h2>
        <ul className="mt-4 list-disc space-y-2 pl-5 text-sm text-slate-300">
          {OPEN_HONESTY_NOTES.map((note) => (
            <li key={note}>{note}</li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="rounded-2xl border border-slate-600 bg-slate-900/40 px-6 py-8 text-center">
          <h2 className="text-lg font-semibold text-white">Need detail for your rollout?</h2>
          <p className="mx-auto mt-2 max-w-2xl text-sm text-slate-400">
            Request a walkthrough focused on entitled products, integration boundaries, and realistic
            V1 scope — not marketing labels alone.
          </p>
          <div className="mt-6 flex flex-wrap justify-center gap-3">
            <Link
              to="/demo"
              className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
            >
              Demo & contact
            </Link>
            <Link
              to="/pricing"
              className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
            >
              Pricing & licensing
            </Link>
          </div>
        </div>
      </section>
    </>
  )
}
