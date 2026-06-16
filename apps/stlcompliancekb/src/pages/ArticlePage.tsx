import { ChevronLeft } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'
import { articleHref, articlesForSection, findArticleBySlug } from '../content/docs'
import { MarkdownArticle } from '../components/MarkdownArticle'
import { NotFoundPage } from './NotFoundPage'

export function ArticlePage() {
  const { slug } = useParams()
  const article = findArticleBySlug(slug)

  if (!article) {
    return <NotFoundPage />
  }

  const related = articlesForSection(article.sectionId)
    .filter((item) => item.slug !== article.slug)
    .slice(0, 5)

  return (
    <section className="article-page">
      <aside className="article-sidebar" aria-label="Article context">
        <Link className="back-link" to={`/sections/${article.sectionId}`}>
          <ChevronLeft aria-hidden="true" />
          {article.sectionLabel}
        </Link>
        <div className="context-box">
          <p className="eyebrow">Audience</p>
          <p>{article.audience}</p>
        </div>
        <div className="context-box">
          <p className="eyebrow">Related</p>
          <ul>
            {related.map((item) => (
              <li key={item.slug}>
                <Link to={articleHref(item)}>{item.title}</Link>
              </li>
            ))}
          </ul>
        </div>
      </aside>

      <article className="article-shell">
        <div className="article-heading">
          <p className="eyebrow">{article.sectionLabel}</p>
          <h1>{article.title}</h1>
          <p>{article.summary}</p>
        </div>
        <MarkdownArticle article={article} />
      </article>
    </section>
  )
}
