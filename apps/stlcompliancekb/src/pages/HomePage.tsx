import { ArrowRight, LifeBuoy, Search, ShieldCheck, UsersRound, Wrench } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import {
  articleHref,
  articlesForSection,
  KB_ARTICLES,
  KB_SECTIONS,
  searchArticles,
  type KbArticle,
  type KbSectionId,
} from '../content/docs'

const audienceCards = [
  {
    label: 'Tenant users',
    description: 'Start with login, navigation, product switching, profile, and common access questions.',
    sectionId: 'getting-started',
    icon: UsersRound,
  },
  {
    label: 'Product admins',
    description: 'Find role, permission, StaffArr, product setup, and daily administration guidance.',
    sectionId: 'roles',
    icon: ShieldCheck,
  },
  {
    label: 'Frontline teams',
    description: 'Use how-to guides for maintenance, dispatch, receiving, training, and field work.',
    sectionId: 'how-to',
    icon: Wrench,
  },
  {
    label: 'Troubleshooters',
    description: 'Check missing access, missing records, certificate issues, reports, parts, and asset visibility.',
    sectionId: 'troubleshooting',
    icon: LifeBuoy,
  },
] as const

const featuredSlugs = [
  'getting-started--first-login',
  'getting-started--product-switching',
  'troubleshooting--product-not-visible',
  'compliance--audit-readiness-overview',
  'workflows--dispatch-to-completion',
  'how-to--recordarr--how-to-upload-a-document',
] as const

const taskCards = [
  {
    prompt: "I can't sign in",
    detail: 'Check account, tenant, and login basics before escalating access issues.',
    slug: 'troubleshooting--cannot-sign-in',
  },
  {
    prompt: 'I cannot see a product',
    detail: 'Confirm tenant context, product launch availability, and role access without using support-only controls.',
    slug: 'troubleshooting--product-not-visible',
  },
  {
    prompt: 'I need to create work',
    detail: 'Start a work order, dispatch, order request, training assignment, or receiving flow.',
    slug: 'how-to--maintainarr--how-to-create-a-work-order',
  },
  {
    prompt: 'I need audit proof',
    detail: 'Find records, attach evidence, review audit-ready packages, and understand retention basics.',
    slug: 'compliance--audit-readiness-overview',
  },
] as const

const productGuideSlugs = [
  'products--staffarr-user-guide',
  'products--trainarr-user-guide',
  'products--maintainarr-user-guide',
  'products--routarr-user-guide',
  'products--assurarr-user-guide',
  'products--loadarr-user-guide',
  'products--supplyarr-user-guide',
  'products--recordarr-user-guide',
  'products--reportarr-user-guide',
] as const

function ArticleCard({ article }: { article: KbArticle }) {
  return (
    <article className="article-card">
      <div className="card-meta">
        <span>{article.sectionLabel}</span>
        <span>{article.audience}</span>
      </div>
      <h3>
        <Link to={articleHref(article)}>{article.title}</Link>
      </h3>
      <p>{article.summary}</p>
    </article>
  )
}

export function HomePage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [query, setQuery] = useState('')
  const results = searchArticles(query)
  const featuredArticles = useMemo(
    () =>
      featuredSlugs
        .map((slug) => KB_ARTICLES.find((article) => article.slug === slug))
      .filter((article): article is KbArticle => Boolean(article)),
    [],
  )
  const productGuides = useMemo(
    () =>
      productGuideSlugs
        .map((slug) => KB_ARTICLES.find((article) => article.slug === slug))
        .filter((article): article is KbArticle => Boolean(article)),
    [],
  )

  useEffect(() => {
    setQuery(searchParams.get('q') ?? '')
  }, [searchParams])

  function updateQuery(value: string) {
    setQuery(value)
    const trimmed = value.trim()
    setSearchParams(trimmed ? { q: trimmed } : {})
  }

  return (
    <>
      <section className="hero">
        <div className="hero-content">
          <p className="eyebrow">STL Compliance Knowledge Base</p>
          <h1>Find the next right step without digging through product notes.</h1>
          <p>
            Search end-user guides for tenant users, product admins, managers, compliance users,
            operators, dispatchers, trainers, warehouse teams, vendors, and field workers.
          </p>
          <label className="search-box">
            <Search aria-hidden="true" />
            <span className="sr-only">Search the knowledge base</span>
            <input
              value={query}
              onChange={(event) => updateQuery(event.target.value)}
              placeholder="Search login, work orders, training, receiving, reports..."
              type="search"
            />
          </label>
        </div>
      </section>

      {query.trim().length > 0 ? (
        <section className="content-band">
          <div className="section-heading">
            <div>
              <p className="eyebrow">Search results</p>
              <h2>{results.length} matching articles</h2>
            </div>
          </div>
          <div className="article-grid">
            {results.length > 0 ? (
              results.map((article) => <ArticleCard key={article.slug} article={article} />)
            ) : (
              <p className="empty-state">No matching article found. Try a product name, record type, or workflow verb.</p>
            )}
          </div>
        </section>
      ) : null}

      <section className="content-band">
        <div className="section-heading">
          <div>
            <p className="eyebrow">I need to...</p>
            <h2>Start from the problem in front of you</h2>
          </div>
        </div>
        <div className="task-grid">
          {taskCards.map((task) => {
            const article = KB_ARTICLES.find((item) => item.slug === task.slug)
            return (
              <Link key={task.prompt} className="task-card" to={article ? articleHref(article) : '/'}>
                <span>{task.prompt}</span>
                <p>{task.detail}</p>
                <strong>
                  Open guide <ArrowRight aria-hidden="true" />
                </strong>
              </Link>
            )
          })}
        </div>
      </section>

      <section className="content-band">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Choose your lane</p>
            <h2>Help organized by how people use the suite</h2>
          </div>
        </div>
        <div className="audience-grid">
          {audienceCards.map((card) => {
            const Icon = card.icon
            return (
              <Link key={card.label} className="audience-card" to={`/sections/${card.sectionId}`}>
                <Icon aria-hidden="true" />
                <h3>{card.label}</h3>
                <p>{card.description}</p>
                <span>
                  Browse articles <ArrowRight aria-hidden="true" />
                </span>
              </Link>
            )
          })}
        </div>
      </section>

      <section className="content-band">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Browse the KB</p>
            <h2>Sections</h2>
          </div>
        </div>
        <div className="section-grid">
          {KB_SECTIONS.filter((section) => section.id !== 'overview').map((section) => (
            <Link key={section.id} className="section-card" to={`/sections/${section.id}`}>
              <span>{articlesForSection(section.id as KbSectionId).length} articles</span>
              <h3>{section.label}</h3>
              <p>{section.description}</p>
            </Link>
          ))}
        </div>
      </section>

      <section className="content-band">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Product quick links</p>
            <h2>Open a product guide</h2>
          </div>
          <Link className="text-link" to="/sections/products">
            View all product guides
          </Link>
        </div>
        <div className="product-link-grid">
          {productGuides.map((article) => (
            <Link key={article.slug} to={articleHref(article)}>
              {article.title.replace(/\s+User Guide$/i, '')}
            </Link>
          ))}
        </div>
      </section>

      <section className="content-band">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Suggested starts</p>
            <h2>Common first stops</h2>
          </div>
        </div>
        <div className="article-grid">
          {featuredArticles.map((article) => (
            <ArticleCard key={article.slug} article={article} />
          ))}
        </div>
      </section>
    </>
  )
}
