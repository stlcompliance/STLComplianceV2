import { SiteSeo } from '../components/SiteSeo'
import { PageHero } from '../components/PageHero'
import { siteConfig } from '../lib/siteConfig'

export function PrivacyPage() {
  return (
    <>
      <SiteSeo
        title={`Privacy — ${siteConfig.siteName}`}
        description="Privacy notice for the STL Compliance public marketing site."
      />
      <PageHero
        eyebrow="Legal"
        title="Privacy notice"
        subtitle="This public marketing site does not authenticate users or store tenant operational records."
      />
      <article className="prose prose-invert mx-auto max-w-3xl px-4 pb-16 text-slate-300 sm:px-6">
        <p>
          Information you submit through the demo/contact form is handled in your browser until you
          choose to email {siteConfig.contactEmail}. Product tenants sign in through NexArr and
          governed product APIs with their own privacy and retention policies.
        </p>
        <p>
          We may collect standard web analytics on this static site as configured by our hosting
          provider. Contact us for data subject requests related to sales inquiries.
        </p>
      </article>
    </>
  )
}
