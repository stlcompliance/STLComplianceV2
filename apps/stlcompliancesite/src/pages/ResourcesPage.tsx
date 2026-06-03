import { Link } from 'react-router-dom'
import { PageHero } from '../components/PageHero'
import { SiteSeo } from '../components/SiteSeo'
import {
  RESOURCE_CATEGORY_LABELS,
  RESOURCE_LINKS,
  type ResourceCategory,
} from '../content/resources'
import { siteConfig } from '../lib/siteConfig'

const CATEGORY_ORDER: ResourceCategory[] = ['suite', 'records', 'trust', 'contact']

export function ResourcesPage() {
  return (
    <>
      <SiteSeo
        title={`Resources — ${siteConfig.siteName}`}
        description="Resources for learning how STL Compliance connects operations, records, product status, and demos."
        path="/resources"
        ogType="website"
      />
      <PageHero
        eyebrow="Resources"
        title="Learn how STL Compliance fits real operations"
        subtitle="Use these pages to compare products, understand rollout status, review trust basics, and request a walkthrough."
      />

      <section className="mx-auto max-w-6xl space-y-12 px-4 pb-16 sm:px-6">
        {CATEGORY_ORDER.map((category) => {
          const links = RESOURCE_LINKS.filter((item) => item.category === category)
          if (links.length === 0) {
            return null
          }

          return (
            <div key={category}>
              <h2 className="text-xl font-bold text-white">{RESOURCE_CATEGORY_LABELS[category]}</h2>
              <ul className="mt-4 grid gap-4 sm:grid-cols-2">
                {links.map((item) => (
                  <li key={item.id}>
                    <article className="flex h-full flex-col rounded-2xl border border-slate-700 bg-slate-900/60 p-5">
                      <h3 className="text-lg font-semibold text-white">
                        {item.external ? (
                          <a
                            href={item.href}
                            className="text-teal-400 hover:text-teal-300"
                            rel="noopener noreferrer"
                            target="_blank"
                          >
                            {item.title}
                          </a>
                        ) : (
                          <Link to={item.href} className="text-teal-400 hover:text-teal-300">
                            {item.title}
                          </Link>
                        )}
                      </h3>
                      <p className="mt-2 flex-1 text-sm text-slate-300">{item.summary}</p>
                    </article>
                  </li>
                ))}
              </ul>
            </div>
          )
        })}
      </section>
    </>
  )
}
