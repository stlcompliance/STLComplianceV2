import { describe, expect, it } from 'vitest'
import { ARTICLE_BY_SLUG, KB_ARTICLES, resolveArticleLink, searchArticles, visibleKbText } from './docs'

describe('KB docs content', () => {
  it('loads user documentation into public articles', () => {
    expect(KB_ARTICLES.length).toBeGreaterThan(40)
    expect(ARTICLE_BY_SLUG['getting-started--first-login']?.title).toMatch(/First login/i)
    expect(ARTICLE_BY_SLUG['roles--staffarr-admin-guide']?.title).toMatch(/StaffArr Admin/i)
  })

  it('excludes platform administration audience and language', () => {
    expect(KB_ARTICLES.some((article) => article.slug.includes('platform-admin'))).toBe(false)
    expect(visibleKbText()).not.toMatch(/platform[-_\s]+admin/i)
    expect(visibleKbText()).not.toMatch(/platform[-_\s]+administrator/i)
    expect(visibleKbText()).not.toMatch(/platform_admin/i)
  })

  it('does not publish Compliance Core content in the end-user KB', () => {
    expect(KB_ARTICLES.some((article) => article.relativePath.startsWith('how-to/compliance-core/'))).toBe(
      false,
    )
    expect(KB_ARTICLES.some((article) => article.relativePath === 'products/compliance-core-user-guide.md')).toBe(
      false,
    )
    expect(KB_ARTICLES.some((article) => article.relativePath === 'workflows/rule-import-to-evaluation.md')).toBe(
      false,
    )
    expect(visibleKbText()).not.toMatch(/Open Compliance Core/i)
    expect(visibleKbText()).not.toMatch(/Compliance Core access/i)
    expect(visibleKbText()).not.toMatch(/Compliance Core/i)
    expect(visibleKbText()).not.toMatch(/compliancecore\./i)
    expect(searchArticles('Compliance Core')).toHaveLength(0)
  })

  it('does not publish section index scaffolding as articles', () => {
    expect(KB_ARTICLES.some((article) => article.relativePath === 'index.md')).toBe(false)
    expect(KB_ARTICLES.some((article) => article.relativePath.endsWith('/index.md'))).toBe(false)
    expect(ARTICLE_BY_SLUG.compliance).toBeUndefined()
    expect(visibleKbText()).not.toMatch(/Start here for this section/i)
    expect(searchArticles('Start here for this section')).toHaveLength(0)
  })

  it('resolves internal markdown links to article routes and titles', () => {
    expect(resolveArticleLink('compliance/index.md', 'audit-readiness-overview.md')).toEqual({
      href: '/articles/compliance--audit-readiness-overview',
      title: 'Audit Readiness Overview',
    })
  })

  it('supports static search across guides', () => {
    const results = searchArticles('receive inbound goods')
    expect(results.map((article) => article.slug)).toContain('how-to--loadarr--how-to-receive-inbound-goods')
  })
})
