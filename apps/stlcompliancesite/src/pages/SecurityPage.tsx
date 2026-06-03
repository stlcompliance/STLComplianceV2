import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { siteConfig } from '../lib/siteConfig'

const principles = [
  'Customers use a secure sign-in before reaching suite products.',
  'Users only see the products and records they are allowed to use.',
  'Important actions are checked by the product responsible for that work.',
  'Connected products share the context needed for the job without turning records into a free-for-all.',
  'Uploaded or outside information is checked before it becomes trusted evidence.',
] as const

export function SecurityPage() {
  return (
    <>
      <SiteSeo
        title={`Security — ${siteConfig.siteName}`}
        description="Trust and security posture for STL Compliance customers."
        path="/security"
      />
      <PageHero
        eyebrow="Trust"
        title="Security built around real work"
        subtitle="STL Compliance is designed so access, approvals, and evidence are controlled by the products where the work actually happens."
      />
      <section className="mx-auto max-w-3xl px-4 pb-16 sm:px-6">
        <ul className="space-y-4">
          {principles.map((item) => (
            <li
              key={item}
              className="rounded-xl border border-slate-700 bg-slate-900/60 px-5 py-4 text-slate-200"
            >
              {item}
            </li>
          ))}
        </ul>
      </section>
    </>
  )
}
