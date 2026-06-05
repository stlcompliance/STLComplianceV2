import { COMPLIANCE_CORE_EDUCATION } from '../content/ownershipBoundaries'
import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'

const commitments = [
  'helps prepare for audits and review work,',
  'helps organize evidence by person, asset, route, or purchase,',
  'helps prove what changed and why,',
  'helps support readiness before work starts,',
  'helps keep evidence together without replacing judgment.',
]

const supports = [
  'Clear ownership between products.',
  'Visibility of what is missing before execution.',
  'Evidence attachment to the workflow where work occurs.',
  'Consistent language for internal audits and customer questions.',
]

const notClaims = [
  'We do not guarantee compliance.',
  'We do not replace professional compliance judgment.',
  'We do not promise to prevent every violation.',
  'We do not own every operational decision.',
]

export function CompliancePage() {
  return (
    <>
      <SiteSeo
        title={`Compliance — STL Compliance`}
        description="Plain-language overview of how STL Compliance supports evidence, audit readiness, and operational accountability."
        path="/compliance"
      />
      <PageHero
        eyebrow="Compliance"
        title="Support for audit-ready, accountable operations"
        subtitle="STL Compliance is for teams that need dependable proof, ownership, and consistency as work moves across people, vendors, equipment, and rules."
      />
      <section className="mx-auto max-w-6xl px-4 py-12 sm:px-6">
        <h2 className="text-2xl font-bold text-white">What STL Compliance helps with</h2>
        <ul className="mt-4 grid gap-2 sm:grid-cols-2">
          {commitments.map((item) => (
            <li
              key={item}
              className="rounded-xl border border-teal-500/30 bg-teal-950/20 px-4 py-3 text-sm text-slate-200"
            >
              {item}
            </li>
          ))}
        </ul>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <article className="rounded-2xl border border-slate-700 bg-slate-900/60 p-6">
          <h2 className="text-xl font-bold text-white">How it supports audit readiness</h2>
          <ul className="mt-4 space-y-2 text-sm text-slate-300">
            {supports.map((item) => (
              <li key={item} className="flex gap-2">
                <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-sky-300" />
                <span>{item}</span>
              </li>
            ))}
          </ul>
        </article>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-12 sm:px-6">
        <article className="rounded-2xl border border-purple-500/30 bg-purple-950/20 p-6">
          <h2 className="text-xl font-bold text-purple-200">{COMPLIANCE_CORE_EDUCATION.headline}</h2>
          <p className="mt-2 text-sm text-slate-200">{COMPLIANCE_CORE_EDUCATION.lead}</p>
          <ul className="mt-4 space-y-2 text-sm text-slate-200">
            {COMPLIANCE_CORE_EDUCATION.bullets.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
      </section>

      <section className="mx-auto max-w-6xl px-4 pb-16 sm:px-6">
        <article className="rounded-2xl border border-amber-500/30 bg-amber-950/20 p-6">
          <h2 className="text-xl font-bold text-amber-100">What we do not claim</h2>
          <ul className="mt-3 space-y-2 text-sm text-amber-50">
            {notClaims.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
      </section>
    </>
  )
}
