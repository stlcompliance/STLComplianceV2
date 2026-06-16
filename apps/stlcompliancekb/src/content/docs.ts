import { rawKbArticles } from 'virtual:kb-docs'

type MarkdownModuleMap = Record<string, string>

export type KbSectionId =
  | 'getting-started'
  | 'roles'
  | 'products'
  | 'how-to'
  | 'workflows'
  | 'compliance'
  | 'troubleshooting'
  | 'reference'
  | 'faq'
  | 'overview'

export type KbArticle = {
  slug: string
  relativePath: string
  title: string
  summary: string
  sectionId: KbSectionId
  sectionLabel: string
  audience: string
  body: string
  searchText: string
}

export type ResolvedArticleLink = {
  href: string
  title?: string
}

export type KbSection = {
  id: KbSectionId
  label: string
  description: string
  audience: string
}

const markdownModules: MarkdownModuleMap = Object.fromEntries(
  rawKbArticles.map((article) => [article.relativePath, article.markdown]),
)

export const KB_SECTIONS: KbSection[] = [
  {
    id: 'getting-started',
    label: 'Getting Started',
    description: 'Sign in, switch products, understand navigation, and manage your profile.',
    audience: 'Tenant users',
  },
  {
    id: 'roles',
    label: 'Role Guides',
    description: 'Guides for product admins, managers, dispatchers, trainers, operators, and vendors.',
    audience: 'Role-based users',
  },
  {
    id: 'products',
    label: 'Product Guides',
    description: 'Product-by-product orientation across the STL Compliance suite.',
    audience: 'All users',
  },
  {
    id: 'how-to',
    label: 'How-To',
    description: 'Task walkthroughs grouped by product and workflow.',
    audience: 'Daily operators and admins',
  },
  {
    id: 'workflows',
    label: 'Workflows',
    description: 'Cross-product flows from request, work, evidence, and reporting handoffs.',
    audience: 'Managers and coordinators',
  },
  {
    id: 'compliance',
    label: 'Compliance',
    description: 'Audit readiness, evidence expectations, citations, and record-finding basics.',
    audience: 'Compliance users',
  },
  {
    id: 'troubleshooting',
    label: 'Troubleshooting',
    description: 'What to check when access, records, training, parts, assets, or reports look wrong.',
    audience: 'Support-facing users',
  },
  {
    id: 'reference',
    label: 'Reference',
    description: 'Common statuses, notifications, record types, glossary, and permission language.',
    audience: 'Admins and reviewers',
  },
  {
    id: 'faq',
    label: 'FAQ',
    description: 'Short answers for access, reports, maintenance, records, training, and mobile work.',
    audience: 'All users',
  },
  {
    id: 'overview',
    label: 'KB Overview',
    description: 'The curated entry point for STL Compliance user documentation.',
    audience: 'All users',
  },
]

const sectionById = Object.fromEntries(KB_SECTIONS.map((section) => [section.id, section])) as Record<
  KbSectionId,
  KbSection
>

export const PRODUCT_LABELS: Record<string, string> = {
  assurarr: 'AssurArr',
  customarr: 'CustomArr',
  'field-companion': 'Field Companion',
  loadarr: 'LoadArr',
  maintainarr: 'MaintainArr',
  nexarr: 'NexArr',
  ordarr: 'OrdArr',
  recordarr: 'RecordArr',
  reportarr: 'ReportArr',
  routarr: 'RoutArr',
  staffarr: 'StaffArr',
  supplyarr: 'SupplyArr',
  trainarr: 'TrainArr',
}

function normalizePath(modulePath: string): string {
  return modulePath
    .replace(/\\/g, '/')
    .replace(/^.*docs\/user\//, '')
    .replace(/^\.\.\/*/, '')
}

function titleFromMarkdown(markdown: string, relativePath: string): string {
  const heading = markdown.match(/^#\s+(.+)$/m)?.[1]?.trim()
  if (heading) {
    return heading
  }

  const name = relativePath.split('/').at(-1)?.replace(/\.md$/i, '') ?? 'article'
  return name
    .split(/[-_]/g)
    .filter(Boolean)
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')
}

function bodyWithoutTitle(markdown: string): string {
  return markdown.replace(/^#\s+.+\r?\n?/, '').trim()
}

function summarize(markdown: string): string {
  const lines = bodyWithoutTitle(markdown)
    .split(/\r?\n/)
    .map((line) => line.replace(/^[-*\d.\s]+/, '').trim())
    .filter((line) => line.length > 0 && !line.startsWith('#'))

  return lines[0] ?? 'STL Compliance user guidance.'
}

function slugFromPath(relativePath: string): string {
  const withoutExtension = relativePath.replace(/\.md$/i, '')
  if (withoutExtension === 'index') {
    return 'overview'
  }

  return withoutExtension.replace(/\/index$/i, '').replace(/\//g, '--')
}

export function isIndexDocPath(relativePath: string): boolean {
  return relativePath === 'index.md' || relativePath.endsWith('/index.md')
}

function sectionFromPath(relativePath: string): KbSectionId {
  const [firstSegment] = relativePath.split('/')
  if (firstSegment === 'index.md') {
    return 'overview'
  }

  if (firstSegment === 'getting-started') {
    return 'getting-started'
  }

  if (firstSegment === 'roles') {
    return 'roles'
  }

  if (firstSegment === 'products') {
    return 'products'
  }

  if (firstSegment === 'how-to') {
    return 'how-to'
  }

  if (firstSegment === 'workflows') {
    return 'workflows'
  }

  if (firstSegment === 'compliance') {
    return 'compliance'
  }

  if (firstSegment === 'troubleshooting') {
    return 'troubleshooting'
  }

  if (firstSegment === 'reference') {
    return 'reference'
  }

  if (firstSegment === 'faq') {
    return 'faq'
  }

  return 'overview'
}

function audienceFromPath(relativePath: string, sectionId: KbSectionId): string {
  if (sectionId !== 'how-to') {
    return sectionById[sectionId].audience
  }

  const productKey = relativePath.split('/')[1]
  return PRODUCT_LABELS[productKey] ? `${PRODUCT_LABELS[productKey]} users` : 'Daily operators and admins'
}

export const KB_ARTICLES: KbArticle[] = Object.entries(markdownModules)
  .map(([modulePath, raw]) => ({
    raw,
    relativePath: normalizePath(modulePath),
  }))
  .filter(({ relativePath }) => !isIndexDocPath(relativePath))
  .map(({ relativePath, raw }) => {
    const sanitized = raw.trim()
    const sectionId = sectionFromPath(relativePath)
    const title = titleFromMarkdown(sanitized, relativePath)
    const section = sectionById[sectionId]

    return {
      slug: slugFromPath(relativePath),
      relativePath,
      title,
      summary: summarize(sanitized),
      sectionId,
      sectionLabel: section.label,
      audience: audienceFromPath(relativePath, sectionId),
      body: bodyWithoutTitle(sanitized),
      searchText: `${title} ${section.label} ${audienceFromPath(relativePath, sectionId)} ${sanitized}`.toLowerCase(),
    }
  })
  .sort((a, b) => a.title.localeCompare(b.title))

export const ARTICLE_BY_SLUG = Object.fromEntries(KB_ARTICLES.map((article) => [article.slug, article])) as Record<
  string,
  KbArticle | undefined
>

export const ARTICLE_BY_PATH = Object.fromEntries(
  KB_ARTICLES.map((article) => [article.relativePath, article]),
) as Record<string, KbArticle | undefined>

export function articlesForSection(sectionId: KbSectionId): KbArticle[] {
  return KB_ARTICLES.filter((article) => article.sectionId === sectionId)
}

export function groupLabelForArticle(article: KbArticle): string {
  const segments = article.relativePath.split('/')

  if (article.sectionId === 'how-to') {
    return PRODUCT_LABELS[segments[1]] ?? 'General tasks'
  }

  if (article.sectionId === 'products') {
    const productKey = article.relativePath.split('/').at(-1)?.replace(/-user-guide\.md$/i, '') ?? ''
    return PRODUCT_LABELS[productKey] ?? 'Product orientation'
  }

  if (article.sectionId === 'roles') {
    return article.title.replace(/\s+Guide$/i, '')
  }

  return article.sectionLabel
}

export function findArticleBySlug(slug: string | undefined): KbArticle | undefined {
  return slug ? ARTICLE_BY_SLUG[slug] : undefined
}

export function sectionBySlug(slug: string | undefined): KbSection | undefined {
  return KB_SECTIONS.find((section) => section.id === slug)
}

export function articleHref(article: KbArticle): string {
  return `/articles/${article.slug}`
}

export function resolveArticleLink(currentPath: string, href: string): ResolvedArticleLink | undefined {
  if (/^(https?:|mailto:|tel:|#)/i.test(href)) {
    return { href }
  }

  const [pathPart, anchorPart] = href.split('#')
  const baseDirectory = currentPath.includes('/') ? currentPath.slice(0, currentPath.lastIndexOf('/')) : ''
  const normalized = [...baseDirectory.split('/'), ...pathPart.split('/')]
    .filter((part) => part.length > 0 && part !== '.')
    .reduce<string[]>((parts, part) => {
      if (part === '..') {
        parts.pop()
      } else {
        parts.push(part)
      }
      return parts
    }, [])
    .join('/')
    .replace(/\.md$/i, '.md')

  const targetPath = normalized.endsWith('.md') ? normalized : `${normalized}.md`
  const article = ARTICLE_BY_PATH[targetPath]
  if (!article) {
    return undefined
  }

  return {
    href: `${articleHref(article)}${anchorPart ? `#${anchorPart}` : ''}`,
    title: article.title,
  }
}

export function searchArticles(query: string): KbArticle[] {
  const terms = query
    .toLowerCase()
    .split(/\s+/)
    .map((term) => term.trim())
    .filter(Boolean)

  if (terms.length === 0) {
    return []
  }

  return KB_ARTICLES.filter((article) => terms.every((term) => article.searchText.includes(term))).slice(0, 24)
}

export function visibleKbText(): string {
  return KB_ARTICLES.map((article) => `${article.title}\n${article.summary}\n${article.body}`).join('\n')
}
