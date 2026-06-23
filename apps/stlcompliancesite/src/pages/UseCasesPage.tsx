import { Link } from 'react-router-dom'
import { MARKETING_PRODUCTS, productPagePath } from '../content/products'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'

const USE_CASES = [
  {
    title: 'Asset maintenance needs parts',
    summary:
      'A truck is down and parts are needed. MaintainArr opens the work request, SupplyArr gives item context, LoadArr shows actual parts status, and AssurArr can hold release if needed.',
    products: ['maintainarr', 'supplyarr', 'loadarr', 'assurarr', 'recordarr'],
  },
  {
    title: 'Driver qualification before dispatch',
    summary:
      'An assignment is ready. StaffArr and TrainArr confirm person readiness. RoutArr confirms route and readiness checks, and Compliance Core verifies evidence requirements.',
    products: ['staffarr', 'trainarr', 'routarr', 'compliancecore', 'recordarr'],
  },
  {
    title: 'Receiving and proof capture',
    summary:
      'Goods arrive at the dock. LoadArr tracks receiving and storage movement. RecordArr stores receiving proof, and ReportArr keeps the team aware of delays.',
    products: ['loadarr', 'recordarr', 'reportarr', 'supplyarr'],
  },
  {
    title: 'Supplier issue and customer hold',
    summary:
      'A supplier problem affects order execution. SupplyArr records the issue, AssurArr opens an assurance case, and the impacted order holds continue with full traceability.',
    products: ['supplyarr', 'assurarr', 'reportarr', 'recordarr'],
  },
  {
    title: 'Field execution from mobile',
    summary:
      'Field crew gets the right task from a mobile handoff and attaches photos and signatures to the right record.',
    products: ['fieldcompanion', 'maintainarr', 'routarr', 'loadarr', 'recordarr'],
  },
]

export function UseCasesPage() {
  return (
    <>
      <SiteSeo
        title={`Use Cases — STL Compliance`}
        description="Real operational use cases for STL Compliance with practical workflows and accountable handoffs."
        path="/use-cases"
      />
      <PageHero
        eyebrow="Use Cases"
        title="Use STL Compliance how teams use it every day"
        subtitle="From dispatch readiness to receiving, from maintenance to audit prep, the suite keeps handoffs moving clearly."
      />
      <section className="mx-auto max-w-6xl space-y-4 px-4 pb-16 sm:px-6">
        {USE_CASES.map((item) => (
          <article
            key={item.title}
            className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5"
          >
            <h2 className="text-xl font-semibold text-white">{item.title}</h2>
            <p className="mt-2 text-sm text-slate-300">{item.summary}</p>
            <div className="mt-4 flex flex-wrap gap-2">
              {item.products.map((productKey) => {
                const product = MARKETING_PRODUCTS.find((entry) => entry.productKey === productKey)
                return (
                  <Link
                    key={productKey}
                    to={productPagePath(productKey)}
                    className="rounded-full border border-slate-600 bg-slate-950 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-slate-300 hover:border-teal-400"
                  >
                    {product?.displayName ?? productKey}
                  </Link>
                )
              })}
            </div>
          </article>
        ))}
      </section>
    </>
  )
}
