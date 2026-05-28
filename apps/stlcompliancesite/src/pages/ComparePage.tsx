import { Link } from 'react-router-dom'
import { AlternativeComparisonTable } from '../components/AlternativeComparisonTable'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import {
  ALTERNATIVE_SCENARIOS,
  COMPARE_DISCLAIMER,
  COMPARE_LEAD,
  SUITE_HONESTY_NOTES,
} from '../content/compare'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

export function ComparePage() {
  return (
    <>
      <SiteSeo
        title={`Compare approaches — ${siteConfig.siteName}`}
        description="Honest comparison of spreadsheets, single-domain point tools, and the STL Compliance multi-product suite — architecture, ownership, and when each approach fits."
        path="/compare"
      />
      <PageHero
        eyebrow="Suite comparison"
        title="Spreadsheets, point tools, or a bounded product suite"
        subtitle={COMPARE_LEAD}
      >
        <Link
          to="/demo"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Request a walkthrough
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

      <section className="mx-auto max-w-6xl px-4 pb-8 sm:px-6">
        <div
          data-testid="compare-disclaimer"
          className="rounded-2xl border border-amber-500/30 bg-amber-950/20 px-5 py-4 text-sm text-amber-100"
        >
          <strong className="font-semibold">Marketing only:</strong> {COMPARE_DISCLAIMER}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">When alternatives still make sense</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          We do not claim the suite replaces every spreadsheet or vendor overnight. Many customers
          start with one entitled product and grow as operational scope expands.
        </p>
        <div className="mt-6 grid gap-4 sm:grid-cols-2">
          {ALTERNATIVE_SCENARIOS.map((scenario) => (
            <article
              key={scenario.id}
              data-testid={`compare-scenario-${scenario.id}`}
              className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5"
            >
              <h3 className="text-lg font-semibold text-teal-200">{scenario.title}</h3>
              <p className="mt-3 text-sm font-medium text-slate-200">When it fits</p>
              <p className="mt-1 text-sm text-slate-300">{scenario.whenItFits}</p>
              <p className="mt-4 text-sm font-medium text-slate-200">Typical limitations</p>
              <p className="mt-1 text-sm text-slate-400">{scenario.limitations}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Side-by-side dimensions</h2>
        <p className="mt-2 max-w-3xl text-sm text-slate-400">
          Architecture and ownership differences — not a feature scorecard. For per-product scope,
          see the{' '}
          <Link to="/products" className="text-teal-400 hover:text-teal-300">
            products hub
          </Link>{' '}
          and maturity labels.
        </p>
        <div className="mt-6">
          <AlternativeComparisonTable />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Suite honesty</h2>
        <ul className="mt-4 list-disc space-y-2 pl-5 text-sm text-slate-300">
          {SUITE_HONESTY_NOTES.map((note) => (
            <li key={note}>{note}</li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="rounded-2xl border border-slate-600 bg-slate-900/40 px-6 py-8 text-center">
          <h2 className="text-lg font-semibold text-white">Evaluate fit with your team</h2>
          <p className="mx-auto mt-2 max-w-2xl text-sm text-slate-400">
            Walk through product boundaries, entitlements, and a realistic rollout path. Licensing
            narrative lives on the pricing page — still no checkout on this site.
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
