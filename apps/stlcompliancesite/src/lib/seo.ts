import { siteConfig } from './siteConfig'

export type OgType = 'website' | 'article'

export type PageSeoInput = {
  title: string
  description: string
  /** Path only, e.g. `/products` — used for canonical and Open Graph URL. */
  path?: string
  ogType?: OgType
  noIndex?: boolean
  ogImagePath?: string
}

const DEFAULT_OG_IMAGE = '/stl-logo.png'

export function siteBaseUrl(): string {
  const raw = import.meta.env.VITE_SITE_BASE_URL ?? 'https://stlcompliancesite.onrender.com'
  return raw.replace(/\/+$/, '')
}

export function absoluteUrl(path: string): string {
  const normalized = path.startsWith('/') ? path : `/${path}`
  if (normalized === '/') {
    return siteBaseUrl()
  }
  return `${siteBaseUrl()}${normalized}`
}

export function defaultOgImageUrl(): string {
  return absoluteUrl(DEFAULT_OG_IMAGE)
}

export function upsertMeta(attribute: 'name' | 'property', key: string, content: string): void {
  const selector = `meta[${attribute}="${key}"]`
  let element = document.querySelector(selector)
  if (!element) {
    element = document.createElement('meta')
    element.setAttribute(attribute, key)
    document.head.appendChild(element)
  }
  element.setAttribute('content', content)
}

export function upsertLinkRel(rel: string, href: string): void {
  const selector = `link[rel="${rel}"]`
  let element = document.querySelector(selector)
  if (!element) {
    element = document.createElement('link')
    element.setAttribute('rel', rel)
    document.head.appendChild(element)
  }
  element.setAttribute('href', href)
}

export function removeJsonLdScript(id: string): void {
  document.getElementById(id)?.remove()
}

export function upsertJsonLd(id: string, payload: Record<string, unknown>): void {
  removeJsonLdScript(id)
  const script = document.createElement('script')
  script.id = id
  script.type = 'application/ld+json'
  script.textContent = JSON.stringify(payload)
  document.head.appendChild(script)
}

export function buildOrganizationJsonLd(): Record<string, unknown> {
  return {
    '@context': 'https://schema.org',
    '@type': 'Organization',
    name: siteConfig.companyLegalName,
    url: siteBaseUrl(),
    description: siteConfig.defaultDescription,
  }
}

export function applyPageSeo(input: PageSeoInput): void {
  const path = input.path ?? '/'
  const canonical = absoluteUrl(path)
  const ogImage = absoluteUrl(input.ogImagePath ?? DEFAULT_OG_IMAGE)
  const ogType = input.ogType ?? 'website'

  document.title = input.title
  upsertMeta('name', 'description', input.description)
  upsertLinkRel('canonical', canonical)

  upsertMeta('property', 'og:title', input.title)
  upsertMeta('property', 'og:description', input.description)
  upsertMeta('property', 'og:url', canonical)
  upsertMeta('property', 'og:type', ogType)
  upsertMeta('property', 'og:site_name', siteConfig.siteName)
  upsertMeta('property', 'og:image', ogImage)

  upsertMeta('name', 'twitter:card', 'summary_large_image')
  upsertMeta('name', 'twitter:title', input.title)
  upsertMeta('name', 'twitter:description', input.description)
  upsertMeta('name', 'twitter:image', ogImage)

  if (input.noIndex) {
    upsertMeta('name', 'robots', 'noindex, nofollow')
  } else {
    upsertMeta('name', 'robots', 'index, follow')
  }
}
