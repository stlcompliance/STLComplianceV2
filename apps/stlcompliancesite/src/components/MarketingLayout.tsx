import { Link, Outlet } from 'react-router-dom'
import { CookieNotice } from './CookieNotice'
import { knowledgeBaseUrl, siteConfig, suiteLoginUrl } from '../lib/siteConfig'

const navLinks = [
  { to: '/', label: 'Home' },
  { to: '/platform-overview', label: 'Platform Overview' },
  { to: '/products', label: 'Products' },
  { to: '/industries', label: 'Industries' },
  { to: '/use-cases', label: 'Use Cases' },
  { to: '/compliance', label: 'Compliance' },
  { to: '/why-stl-compliance', label: 'Why STL Compliance' },
  { to: '/about-founder', label: 'About' },
  { to: '/resources', label: 'Resources' },
  { to: '/pricing', label: 'Pricing/Request Access' },
  { to: '/contact', label: 'Contact' },
  { to: '/faq', label: 'FAQ' },
] as const

export function MarketingLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="border-b border-slate-700/60 bg-slate-950/70 backdrop-blur">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-4 px-4 py-4 sm:px-6">
          <Link to="/" className="flex items-center gap-3">
            <img
              src="/brand/stl-fullcolor.png"
              alt={siteConfig.siteName}
              className="h-12 w-auto rounded-sm bg-[var(--color-bg-surface)] px-2 py-1"
              width={260}
              height={90}
            />
          </Link>
          <nav className="flex flex-wrap items-center gap-1 text-sm font-medium">
            {navLinks.map((link) => (
              <Link
                key={link.to}
                to={link.to}
                className="rounded-lg px-3 py-2 text-slate-200 hover:bg-slate-800 hover:text-white"
              >
                {link.label}
              </Link>
            ))}
            <a
              href={knowledgeBaseUrl()}
              className="rounded-lg px-3 py-2 text-slate-200 hover:bg-slate-800 hover:text-white"
            >
              Knowledge Base
            </a>
            <a
              href={suiteLoginUrl()}
              className="ml-2 rounded-lg bg-teal-600 px-4 py-2 text-white hover:bg-teal-500"
            >
              Client sign in
            </a>
          </nav>
        </div>
      </header>

      <main className="flex-1">
        <Outlet />
      </main>

      <footer className="border-t border-slate-700/60 bg-slate-950/80">
        <div className="mx-auto flex max-w-6xl flex-col gap-5 px-4 py-8 text-sm text-slate-400 sm:flex-row sm:items-center sm:justify-between sm:px-6">
          <div className="space-y-3">
            <img
              src="/brand/stl-fullcolor.png"
              alt={siteConfig.siteName}
              className="h-14 w-auto rounded-sm bg-[var(--color-bg-surface)] px-2 py-1"
              width={280}
              height={96}
            />
            <p>
              © {new Date().getFullYear()} {siteConfig.companyLegalName}. {siteConfig.arrTagline}{' '}
              suite.
            </p>
          </div>
          <div className="flex flex-wrap gap-4">
            <Link to="/compare" className="hover:text-teal-300">
              Compare
            </Link>
            <Link to="/pricing" className="hover:text-teal-300">
              Pricing
            </Link>
            <Link to="/compliance" className="hover:text-teal-300">
              Compliance
            </Link>
            <Link to="/contact" className="hover:text-teal-300">
              Contact
            </Link>
            <Link to="/privacy" className="hover:text-teal-300">
              Privacy
            </Link>
            <Link to="/terms" className="hover:text-teal-300">
              Terms
            </Link>
            <Link to="/faq" className="hover:text-teal-300">
              FAQ
            </Link>
            <a href={knowledgeBaseUrl()} className="hover:text-teal-300">
              Knowledge Base
            </a>
            <Link to="/demo" className="hover:text-teal-300">
              Contact
            </Link>
          </div>
        </div>
      </footer>

      <CookieNotice />
    </div>
  )
}
