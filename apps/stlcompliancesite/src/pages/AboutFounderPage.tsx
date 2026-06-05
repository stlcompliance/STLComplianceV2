import { Link } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'

const timeline = [
  {
    title: 'The beginning: seeing operations split',
    body: 'The platform was shaped from seeing teams manage the same work in training files, maintenance notes, route spreadsheets, and folder-based records.',
  },
  {
    title: 'The pressure point: audit and execution',
    body: 'Businesses were often one good inspection away from a late response because proof was in the wrong system at the wrong moment.',
  },
  {
    title: 'The direction: linked operational products',
    body: 'STL Compliance was built as a connected suite where each product does one job well and hands off ownership clearly.',
  },
]

export function AboutFounderPage() {
  return (
    <>
      <SiteSeo
        title={`About STL Compliance`}
        description="From the STL Compliance founder: practical reason for building an adaptive risk reduction suite."
        path="/about-founder"
      />
      <PageHero
        eyebrow="From the founder"
        title="Built because fragmented systems were slowing down real work"
        subtitle="This site came from real operations where safety, compliance, and execution needed to work together at the pace of the job."
      >
        <Link
          to="/platform-overview"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          Read the platform overview
        </Link>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 py-12 sm:px-6">
        <div className="space-y-4">
          <p className="text-lg font-semibold text-white">A practical origin story</p>
          <p className="text-sm text-slate-300">
            STL Compliance exists to fix a common pattern: excellent teams, good systems, but poor handoff.
            People can be right on paper and still miss the right piece at the right moment.
          </p>
          <p className="text-sm text-slate-300">
            We built the suite to keep operations clear: who owns the record, what changed, and what
            proof is available to continue work safely and consistently.
          </p>
          <p className="text-sm text-slate-300">
            The focus is always the same: make operations more understandable for managers, safer for workers, and more ready for review.
          </p>
        </div>
        <div className="mt-8 grid gap-4 md:grid-cols-3">
          {timeline.map((item) => (
            <article key={item.title} className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5">
              <h2 className="text-lg font-semibold text-white">{item.title}</h2>
              <p className="mt-2 text-sm text-slate-300">{item.body}</p>
            </article>
          ))}
        </div>
      </section>
    </>
  )
}
