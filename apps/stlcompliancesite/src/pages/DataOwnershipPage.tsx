import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import { siteConfig } from '../lib/siteConfig'

export function DataOwnershipPage() {
  return (
    <>
      <SiteSeo
        title={`Records — ${siteConfig.siteName}`}
        description="How STL Compliance keeps records connected to the work that created them."
        path="/data-ownership"
      />
      <PageHero
        eyebrow="Records"
        title="Records stay connected to the work"
        subtitle="Training proof stays with training. Maintenance proof stays with maintenance. Dispatch proof stays with dispatch. The suite connects those records so teams can see the full story."
      />
      <section className="mx-auto max-w-3xl space-y-6 px-4 pb-16 text-slate-200 sm:px-6">
        <p>
          A connected view can show information from another workflow, but the original record stays
          with the work that created it.
        </p>
        <p>
          That helps supervisors and compliance teams see the important relationships without
          forcing every department to maintain duplicate versions of the same record.
        </p>
        <p>
          The public website only explains the platform. Customer records and daily work happen
          after secure sign-in.
        </p>
      </section>
    </>
  )
}
