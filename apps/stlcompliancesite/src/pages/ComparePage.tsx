import { Link } from 'react-router-dom'
import {
  CanWorkStartChecklist,
  CategoryComparisonCards,
  FeatureChecklistTable,
  ObjectionCards,
  ProductStackTable,
  UsualStackTable,
} from '../components/MarketComparisonTable'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { siteConfig } from '../lib/siteConfig'

const comparisonCategories = [
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

export function ComparePage() {
  return (
    <>
      <SiteSeo
        title={`Compare STL Compliance — ${siteConfig.siteName}`}
        description="Compare STL Compliance against WMS, CMMS, LMS, WFM, TMS, GRC, and IAM point tools by workflow, readiness, and evidence features."
        path="/compare"
      />
      <PageHero
        eyebrow="Suite comparison"
        title="Stop comparing one app to one app. Compare the whole workflow."
        subtitle="Most software solves one department’s problem. STL Compliance connects the work between them."
      >
        <Link
          to="/compare#suite-checklist"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Compare the suite
        </Link>
        <Link
          to="/products"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          See product map
        </Link>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <div className="grid gap-6 lg:grid-cols-[1.2fr_.8fr]">
          <div>
            <p className="text-lg text-slate-200">
              A WMS moves inventory. A CMMS manages work orders. An LMS tracks training. A WFM
              system schedules people. A TMS dispatches routes. A GRC tool stores compliance
              requirements.
            </p>
            <p className="mt-5 text-xl font-semibold text-white">
              STL Compliance is built for companies that need to know:
            </p>
            <p className="mt-3 text-2xl font-bold text-teal-200">
              Can this person perform this work, with this equipment, at this site, using this
              material, under these rules, and can we prove it later?
            </p>
          </div>
          <aside className="rounded-2xl border border-teal-500/30 bg-teal-950/20 p-6">
            <p className="text-sm font-semibold uppercase text-teal-300">The difference</p>
            <p className="mt-3 text-lg font-semibold text-white">
              STL Compliance is not just another WMS, CMMS, LMS, WFM, TMS, IAM, or GRC product.
            </p>
            <p className="mt-3 text-sm text-slate-300">
              It is an Adaptive Risk Reduction platform for real operations.
            </p>
          </aside>
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Everyday category comparison</h2>
        <p className="mt-2 max-w-4xl text-sm text-slate-400">
          Teams compare against these familiar stacks more often than they compare against another app:
        </p>
        <ul className="mt-4 grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
          {comparisonCategories.map((category) => (
            <li
              key={category}
              className="rounded-xl border border-slate-700 bg-slate-950/50 px-4 py-3 text-sm text-slate-200"
            >
              {category}
            </li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">The usual software stack</h2>
        <p className="mt-2 max-w-4xl text-sm text-slate-400">
          Category leaders are strong in their own lanes. STL Compliance is biased toward the gaps
          that appear when those lanes have to produce one work decision.
        </p>
        <div className="mt-6">
          <UsualStackTable />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <div className="rounded-2xl border border-slate-700 bg-slate-900/60 p-6 text-center">
          <p className="text-sm font-semibold uppercase text-slate-400">Point tools answer</p>
          <h2 className="mt-2 text-2xl font-bold text-white">Did my department complete its task?</h2>
          <p className="mt-6 text-sm font-semibold uppercase text-teal-300">STL Compliance answers</p>
          <h2 className="mt-2 text-3xl font-bold text-teal-100">Should this work be allowed to happen?</h2>
        </div>
      </section>

      <section id="suite-checklist" className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Compare STL Compliance to Other Solutions</h2>
        <div className="mt-6">
          <FeatureChecklistTable />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Category-by-category comparison</h2>
        <p className="mt-2 max-w-4xl text-sm text-slate-400">
          The point is not that specialists are bad. The point is that the handoff is where risk,
          evidence, and accountability slip.
        </p>
        <div className="mt-6">
          <CategoryComparisonCards />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">The “Can work start?” checklist</h2>
        <p className="mt-2 max-w-4xl text-sm text-slate-400">
          Before STL, supervisors check multiple systems, spreadsheets, binders, emails, and memory.
          With STL, the operating model checks the conditions that decide whether work is allowed,
          blocked, or missing evidence.
        </p>
        <div className="mt-6">
          <CanWorkStartChecklist />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Product stack comparison</h2>
        <p className="mt-2 max-w-4xl text-sm text-slate-400">
          Each product focuses on a practical part of the workflow, while the suite keeps readiness and
          proof connected across the operation.
        </p>
        <div className="mt-6">
          <ProductStackTable />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Why not just buy best-of-breed?</h2>
        <div className="mt-4 max-w-4xl space-y-4 text-sm text-slate-300">
          <p>You can. For some companies, that is the right answer.</p>
          <p>
            But your audit does not care that every department bought a strong app. Your supervisor
            does not have time to check six systems before assigning work.
          </p>
          <p className="text-lg font-semibold text-teal-200">
            STL Compliance wins when the problem is not one department. STL Compliance wins when
            the problem is the handoff.
          </p>
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-xl font-bold text-white">Objection handling</h2>
        <div className="mt-6">
          <ObjectionCards />
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <div className="rounded-2xl border border-teal-500/30 bg-teal-950/20 px-6 py-8 text-center">
          <h2 className="text-2xl font-bold text-white">
            Your software should not just record work. It should help decide whether the work should happen.
          </h2>
          <p className="mx-auto mt-3 max-w-3xl text-sm text-slate-300">
            STL Compliance connects people, training, assets, inventory, dispatch, incidents,
            vendors, and compliance evidence into one Adaptive Risk Reduction platform.
          </p>
          <div className="mt-6 flex flex-wrap justify-center gap-3">
            <Link
              to="/demo"
              className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
            >
              See how STL Compliance works
            </Link>
            <Link
              to="/products"
              className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
            >
              Compare products by workflow
            </Link>
          </div>
        </div>
      </section>
    </>
  )
}
