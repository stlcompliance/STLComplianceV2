import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { siteConfig } from '../lib/siteConfig'

export function TermsPage() {
  return (
    <>
      <SiteSeo
        title={`Terms — ${siteConfig.siteName}`}
        description="Terms of use for the STL Compliance public marketing site."
        path="/terms"
      />
      <PageHero
        eyebrow="Legal"
        title="Terms of use"
        subtitle="This public site helps visitors understand STL Compliance. Customer use of the suite is governed by your agreement with STL Compliance."
      />
      <article className="mx-auto max-w-3xl space-y-4 px-4 pb-16 text-slate-300 sm:px-6">
        <p>
          Product descriptions on this site are for evaluation and education. Feature availability
          for your organization depends on your agreement and licensed scope.
        </p>
        <p>
          Do not rely on this site for operational decisions, compliance determinations, or emergency
          response. Customers should use client sign-in for actual product records.
        </p>
        <p className="text-sm text-slate-500">© {new Date().getFullYear()} {siteConfig.companyLegalName}</p>
      </article>
    </>
  )
}
