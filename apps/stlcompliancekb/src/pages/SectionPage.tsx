import { Link, useParams } from 'react-router-dom'
import { articleHref, articlesForSection, groupLabelForArticle, sectionBySlug } from '../content/docs'
import { NotFoundPage } from './NotFoundPage'

export function SectionPage() {
  const { sectionId } = useParams()
  const section = sectionBySlug(sectionId)

  if (!section) {
    return <NotFoundPage />
  }

  const articles = articlesForSection(section.id)
  const groups = Object.entries(
    articles.reduce<Record<string, typeof articles>>((grouped, article) => {
      const label = groupLabelForArticle(article)
      grouped[label] = [...(grouped[label] ?? []), article]
      return grouped
    }, {}),
  ).sort(([a], [b]) => a.localeCompare(b))

  return (
    <section className="content-band page-band">
      <div className="page-heading">
        <Link to="/">Knowledge Base</Link>
        <p className="eyebrow">{section.audience}</p>
        <h1>{section.label}</h1>
        <p>{section.description}</p>
      </div>

      <div className="jump-links" aria-label={`${section.label} groups`}>
        {groups.map(([groupLabel]) => (
          <a key={groupLabel} href={`#${groupLabel.toLowerCase().replace(/[^a-z0-9]+/g, '-')}`}>
            {groupLabel}
          </a>
        ))}
      </div>

      <div className="article-list">
        {groups.map(([groupLabel, groupArticles]) => (
          <section key={groupLabel} className="article-group" id={groupLabel.toLowerCase().replace(/[^a-z0-9]+/g, '-')}>
            <h2>{groupLabel}</h2>
            {groupArticles.map((article) => (
              <article key={article.slug} className="article-row">
                <div>
                  <div className="card-meta">
                    <span>{article.sectionLabel}</span>
                    <span>{article.audience}</span>
                  </div>
                  <h3>
                    <Link to={articleHref(article)}>{article.title}</Link>
                  </h3>
                  <p>{article.summary}</p>
                </div>
              </article>
            ))}
          </section>
        ))}
      </div>
    </section>
  )
}
