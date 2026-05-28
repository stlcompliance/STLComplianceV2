import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { siteConfig } from '../lib/siteConfig'

export function TermsPage() {
  return (
    <>
      <SiteSeo
        title={`Terms — ${siteConfig.siteName}`}
        description="Terms of use for the STL Compliance public marketing site."
      />
      <PageHero
        eyebrow="Legal"
        title="Terms of use"
        subtitle="Marketing content is provided for education and positioning. Entitled use of the suite is governed by your agreement with STL Compliance."
      />
      <article className="mx-auto max-w-3xl space-y-4 px-4 pb-16 text-slate-300 sm:px-6">
        <p>
          Capability descriptions on this site reflect product ownership boundaries and V1
          implementation goals. Feature availability for your tenant depends on entitlements and
          deployed product versions.
        </p>
        <p>
          Do not rely on this site for operational decisions, compliance determinations, or emergency
          response. Sign in through NexArr for authoritative product data.
        </p>
        <p className="text-sm text-slate-500">© {new Date().getFullYear()} {siteConfig.companyLegalName}</p>
      </article>
    </>
  )
}
