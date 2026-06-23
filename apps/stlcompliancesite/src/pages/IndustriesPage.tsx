import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { productPagePath, MARKETING_PRODUCTS } from '../content/products'
import { Link } from 'react-router-dom'

const INDUSTRIES = [
  {
    title: 'Fleet and transportation operations',
    pains: [
      'Drivers are assigned before readiness is fully checked.',
      'Paper and email trails are used for dispatch proof.',
      'Dispatch, maintenance, and training are tracked separately.',
    ],
    products: ['nexarr', 'staffarr', 'trainarr', 'routarr', 'maintainarr', 'compliancecore'],
  },
  {
    title: 'Warehousing and distribution',
    pains: [
      'Inventory and receiving visibility does not match labor or exceptions.',
      'Quality issues and holds are logged in places that do not follow the work.',
      'Order flow can stall silently.',
    ],
    products: ['nexarr', 'loadarr', 'supplyarr', 'assurarr', 'reportarr', 'recordarr'],
  },
  {
    title: 'Field service and technicians',
    pains: [
      'Work instructions are not tied to current qualifications.',
      'Proof is uploaded late, then never found in time.',
      'Mobile updates do not close on source records.',
    ],
    products: ['fieldcompanion', 'staffarr', 'trainarr', 'maintainarr', 'recordarr'],
  },
  {
    title: 'Maintenance and regulated fleets',
    pains: [
      'Defects, inspections, and work orders are not consistently tied to route decisions.',
      'Rule requirements show up after work already happened.',
      'Compliance teams cannot see readiness at a glance.',
    ],
    products: ['maintainarr', 'compliancecore', 'assurarr', 'reportarr', 'recordarr'],
  },
]

export function IndustriesPage() {
  return (
    <>
      <SiteSeo
        title={`Industries — STL Compliance`}
        description="STL Compliance supports fleet, logistics, warehouse, maintenance, and field operations with a connected suite for readiness and accountability."
        path="/industries"
      />
      <PageHero
        eyebrow="Industries"
        title="Built for hard real-world operations"
        subtitle="STL Compliance serves teams where missed readiness, unclear handoffs, and delayed evidence can stop work or raise risk."
      />
      <section className="mx-auto max-w-6xl space-y-6 px-4 pb-16 sm:px-6">
        {INDUSTRIES.map((industry) => (
          <article
            key={industry.title}
            className="rounded-2xl border border-slate-700 bg-slate-900/60 p-6"
          >
            <h2 className="text-xl font-semibold text-white">{industry.title}</h2>
            <ul className="mt-4 space-y-2 text-sm text-slate-300">
              {industry.pains.map((pain) => (
                <li key={pain} className="flex gap-2">
                  <span className="mt-2 h-1.5 w-1.5 rounded-full bg-teal-300" />
                  <span>{pain}</span>
                </li>
              ))}
            </ul>
            <p className="mt-4 text-sm font-semibold text-slate-200">Helpful products</p>
            <ul className="mt-2 flex flex-wrap gap-3">
              {industry.products.map((key) => (
                <li key={key}>
                  <Link
                    to={productPagePath(key)}
                    className="inline-flex rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-teal-300 hover:border-teal-400 hover:text-teal-200"
                  >
                    {MARKETING_PRODUCTS.find((product) => product.productKey === key)?.displayName}
                  </Link>
                </li>
              ))}
            </ul>
          </article>
        ))}
      </section>
    </>
  )
}
