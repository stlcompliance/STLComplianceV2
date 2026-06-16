import { BookOpen, LogIn, Search, ShieldCheck } from 'lucide-react'
import { type FormEvent, useEffect, useState } from 'react'
import { Link, NavLink, Outlet, useNavigate, useSearchParams } from 'react-router-dom'
import { KB_SECTIONS } from '../content/docs'
import { kbConfig } from '../lib/siteConfig'

const primarySections = ['getting-started', 'how-to', 'products', 'roles', 'troubleshooting'] as const

export function KbLayout() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const [query, setQuery] = useState(searchParams.get('q') ?? '')

  useEffect(() => {
    setQuery(searchParams.get('q') ?? '')
  }, [searchParams])

  function handleSearchSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const trimmed = query.trim()
    navigate(trimmed ? `/?q=${encodeURIComponent(trimmed)}` : '/')
  }

  return (
    <div className="app-shell">
      <header className="site-header">
        <div className="header-inner">
          <Link className="brand-link" to="/" aria-label="STL Compliance Knowledge Base home">
            <img src="/brand/stl-fullcolor.png" alt="STL Compliance" width={260} height={90} />
            <span>Knowledge Base</span>
          </Link>
          <nav aria-label="Knowledge base navigation" className="top-nav">
            {primarySections.map((sectionId) => {
              const section = KB_SECTIONS.find((item) => item.id === sectionId)!
              return (
                <NavLink key={section.id} to={`/sections/${section.id}`}>
                  {section.label}
                </NavLink>
              )
            })}
          </nav>
          <form className="header-search" role="search" onSubmit={handleSearchSubmit}>
            <Search aria-hidden="true" />
            <label className="sr-only" htmlFor="global-kb-search">
              Search the knowledge base
            </label>
            <input
              id="global-kb-search"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Search KB"
              type="search"
            />
          </form>
          <div className="header-actions">
            <a href={kbConfig.marketingSiteUrl}>
              <BookOpen aria-hidden="true" />
              Main site
            </a>
            <a className="login-link" href={kbConfig.suiteLoginUrl}>
              <LogIn aria-hidden="true" />
              Sign in
            </a>
          </div>
        </div>
      </header>

      <main>
        <Outlet />
      </main>

      <footer className="site-footer">
        <div>
          <p className="footer-kicker">
            <ShieldCheck aria-hidden="true" />
            Public help for tenant users and product admins
          </p>
          <p>
            This KB is informational. Product data, account access, and tenant changes remain in the
            authenticated STL Compliance suite.
          </p>
        </div>
        <Link className="footer-search" to="/">
          <Search aria-hidden="true" />
          Search the KB
        </Link>
      </footer>
    </div>
  )
}
