import { Link } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'

const wins = [
  {
    title: 'Faster handoff with ownership',
    body: 'No one waits on six systems to decide who can work. STL clarifies which product owns each question.',
  },
  {
    title: 'One place for readiness, less noise',
    body: 'You still run strong tools, but STL tracks the practical gate: ready, blocked, missing proof, or needs follow-up.',
  },
  {
    title: 'Fewer missing links before closeout',
    body: 'Field actions and office workflows stay connected to one operating record instead of drifting into folders and inboxes.',
  },
]

export function WhyStlCompliancePage() {
  return (
    <>
      <SiteSeo
        title={`Why STL Compliance`}
        description="Why teams choose STL Compliance when they need practical accountability, connected operations, and evidence-aware workflows."
        path="/why-stl-compliance"
      />
      <PageHero
        eyebrow="Why STL"
        title="When operations need one operating story, not six disconnected ones"
        subtitle="STL Compliance is designed for people who live in real work, not in systems checklists."
      >
        <Link
          to="/platform-overview"
          className="rounded-lg border border-slate-500 px-5 py-2.5 text-sm font-semibold text-slate-100 hover:border-teal-400"
        >
          See platform overview
        </Link>
        <Link
          to="/contact"
          className="rounded-lg bg-teal-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-teal-500"
        >
          Talk to the team
        </Link>
      </PageHero>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <h2 className="text-2xl font-bold text-white">Built for teams, not for slideware</h2>
        <p className="mt-3 max-w-3xl text-slate-300">
          Most marketing platforms promise perfect systems. STL Compliance is about practical flow:
          who can do work, what is safe, what is ready, and what evidence is needed to support each action.
        </p>
        <div className="mt-6 grid gap-4 lg:grid-cols-3">
          {wins.map((item) => (
            <article key={item.title} className="rounded-2xl border border-slate-700 bg-slate-900/60 p-5">
              <h3 className="text-lg font-semibold text-teal-200">{item.title}</h3>
              <p className="mt-2 text-sm text-slate-300">{item.body}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <article className="rounded-2xl border border-teal-500/30 bg-teal-950/20 p-6">
          <h2 className="text-xl font-bold text-white">Plain comparison language</h2>
          <p className="mt-3 text-sm text-slate-300">
            Most stacks solve one department’s problem at a time. STL Compliance focuses on the moment
            work moves between teams. That is where missed follow-up, unclear ownership, and audit
            pain usually starts.
          </p>
        </article>
      </section>
    </>
  )
}
