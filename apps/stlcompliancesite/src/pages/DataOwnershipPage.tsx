import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { siteConfig } from '../lib/siteConfig'

export function DataOwnershipPage() {
  return (
    <>
      <SiteSeo
        title={`Data ownership — ${siteConfig.siteName}`}
        description="How STL Compliance products own operational data, mirrors, and compliance authority boundaries."
      />
      <PageHero
        eyebrow="Data ownership"
        title="Each product owns its truth"
        subtitle="Operational facts and workflow actions stay in the product that owns the domain. Compliance Core supplies rule context and results; products supply facts and permitted overrides with audit."
      />
      <section className="mx-auto max-w-3xl space-y-6 px-4 pb-16 text-slate-200 sm:px-6">
        <p>
          A displayed record from another product is never local truth. Local mirrors are rebuildable
          and labeled with source product, source ID, source event, and source timestamp.
        </p>
        <p>
          Durable tenant data belongs in PostgreSQL or object storage managed by the owning service —
          not on ephemeral instance filesystems. Redis-compatible Key Value is used for cache and
          coordination only.
        </p>
        <p className="text-sm text-slate-400">
          {siteConfig.siteName} marketing pages do not store or process tenant operational data.
        </p>
      </section>
    </>
  )
}
