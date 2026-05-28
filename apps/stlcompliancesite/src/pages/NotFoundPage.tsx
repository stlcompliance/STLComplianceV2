import { Link } from 'react-router-dom'
import { SiteSeo } from '../components/SiteSeo'
import { siteConfig } from '../lib/siteConfig'

export function NotFoundPage() {
  return (
    <>
      <SiteSeo title={`Not found — ${siteConfig.siteName}`} description="Page not found." />
      <section className="mx-auto max-w-lg px-4 py-24 text-center sm:px-6">
        <h1 className="text-3xl font-bold text-white">404</h1>
        <p className="mt-3 text-slate-300">That page is not part of this marketing site.</p>
        <Link to="/" className="mt-6 inline-block text-teal-400 hover:text-teal-300">
          Return home
        </Link>
      </section>
    </>
  )
}
