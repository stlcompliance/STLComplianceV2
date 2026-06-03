import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { siteConfig } from '../lib/siteConfig'

export function DataOwnershipPage() {
  return (
    <>
      <SiteSeo
        title={`Records — ${siteConfig.siteName}`}
        description="How STL Compliance keeps records with the product built for that work while connecting the suite around them."
        path="/data-ownership"
      />
      <PageHero
        eyebrow="Records"
        title="Each product keeps the truth for its work"
        subtitle="Training proof belongs with training. Maintenance proof belongs with maintenance. Dispatch proof belongs with dispatch. The suite connects those records so teams can see the full story."
      />
      <section className="mx-auto max-w-3xl space-y-6 px-4 pb-16 text-slate-200 sm:px-6">
        <p>
          A connected view can show information from another product, but the original record stays
          with the product built to manage that work.
        </p>
        <p>
          That helps supervisors and compliance teams see the important relationships without
          forcing every department to maintain duplicate versions of the same truth.
        </p>
        <p>
          The public website only explains the platform. Customer records and daily work happen
          after secure sign-in.
        </p>
      </section>
    </>
  )
}
