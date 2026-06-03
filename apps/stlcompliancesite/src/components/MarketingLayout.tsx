import { Link, Outlet } from 'react-router-dom'
import { CookieNotice } from './CookieNotice'
import { siteConfig, suiteLoginUrl } from '../lib/siteConfig'

const navLinks = [
  { to: '/products', label: 'Products' },
  { to: '/compare', label: 'Compare' },
  { to: '/pricing', label: 'Pricing' },
  { to: '/resources', label: 'Resources' },
  { to: '/security', label: 'Security' },
  { to: '/data-ownership', label: 'Records' },
  { to: '/demo', label: 'Demo & contact' },
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
              className="h-12 w-auto rounded-sm bg-white/95 px-2 py-1"
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
              className="h-14 w-auto rounded-sm bg-white/95 px-2 py-1"
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
            <Link to="/privacy" className="hover:text-teal-300">
              Privacy
            </Link>
            <Link to="/terms" className="hover:text-teal-300">
              Terms
            </Link>
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
