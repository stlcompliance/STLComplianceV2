import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { siteConfig } from '../lib/siteConfig'

const principles = [
  'Tenant context validated server-side on every product API.',
  'NexArr identity and product entitlement checked before business operations.',
  'Product-specific permissions enforced in the owning service — not in shared UI libraries.',
  'Cross-product integration via APIs, events, and rebuildable mirrors — no cross-database foreign keys.',
  'Customer-hosted or external data treated as untrusted until validated by the owning product.',
] as const

export function SecurityPage() {
  return (
    <>
      <SiteSeo
        title={`Security — ${siteConfig.siteName}`}
        description="Trust and security posture for the STL Compliance suite: server authority, tenant isolation, and integration boundaries."
        path="/security"
      />
      <PageHero
        eyebrow="Trust"
        title="Security and platform posture"
        subtitle="The suite is designed so business authority never lives in marketing pages, shared UI helpers, or the browser alone."
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
