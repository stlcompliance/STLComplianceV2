import {
  getSuiteProductCatalogEntry,
  SUITE_PRODUCT_CATALOG,
  normalizeProductKey,
} from './productCatalog'
import { resolveProductLaunchUrl } from './productLaunchUrls'

export type AiNavigationLink = {
  label: string
  productKey: string
  route: string
  href: string
  aliases?: string[]
}

export type AiNavigationItem = {
  label: string
  to: string
  children?: readonly AiNavigationItem[]
}

export type BuildAiNavigationLinksOptions = {
  currentProductKey: string
  suiteHomeUrl?: string
  productLaunchUrls?: Record<string, string>
  currentNavItems?: readonly AiNavigationItem[]
  maxLinks?: number
}

type AiRouteHint = {
  label: string
  route: string
  aliases?: readonly string[]
}

const DEFAULT_SUITE_HOME_URL = 'http://localhost:5174/app'

const PRODUCT_AI_ROUTE_HINTS = {
  staffarr: [
    {
      label: 'StaffArr roles',
      route: '/roles',
      aliases: ['roles', 'role assignments', 'permissions', 'staffarr permissions'],
    },
  ],
} satisfies Record<string, readonly AiRouteHint[]>

export function buildAiNavigationLinks({
  currentProductKey,
  suiteHomeUrl = DEFAULT_SUITE_HOME_URL,
  productLaunchUrls = {},
  currentNavItems = [],
  maxLinks = 40,
}: BuildAiNavigationLinksOptions): AiNavigationLink[] {
  const currentKey = normalizeProductKey(currentProductKey)

  const links = new Map<string, AiNavigationLink>()
  const addLink = (link: AiNavigationLink) => {
    if (links.size >= maxLinks) return

    const productKey = normalizeProductKey(link.productKey)

    const route = normalizeRoute(link.route)
    const href = link.href.trim()
    if (!href) return

    const key = `${productKey}:${route.toLowerCase()}`
    if (!links.has(key)) {
      links.set(key, {
        ...link,
        productKey,
        route,
        href,
      })
    }
  }

  const suiteRoot = resolveProductRootUrl('nexarr', suiteHomeUrl, productLaunchUrls)
  addLink({
    label: 'Suite dashboard',
    productKey: 'nexarr',
    route: '/app',
    href: suiteRoot,
    aliases: ['dashboard', 'home'],
  })
  addLink({
    label: 'Global Smart Import',
    productKey: 'nexarr',
    route: '/app/imports',
    href: appendRoute(suiteRoot, '/imports'),
    aliases: ['imports', 'smart import', 'global smart import', 'import review'],
  })
  addLink({
    label: 'NexArr identity and access',
    productKey: 'nexarr',
    route: '/app/nexarr/identity',
    href: appendRoute(suiteRoot, '/nexarr/identity'),
    aliases: ['identity', 'access', 'users', 'login'],
  })

  for (const product of SUITE_PRODUCT_CATALOG) {
    if (normalizeProductKey(product.productKey) === 'nexarr') {
      continue
    }

    addLink({
      label: `Open ${product.displayName}`,
      productKey: product.productKey,
      route: '/launch',
      href: resolveProductLaunchUrl(product.productKey, suiteHomeUrl, productLaunchUrls),
      aliases: ['launch', product.displayName],
    })
  }

  for (const [productKey, hints] of Object.entries(PRODUCT_AI_ROUTE_HINTS)) {
    const productRoot = resolveProductRootUrl(productKey, suiteHomeUrl, productLaunchUrls)
    for (const hint of hints) {
      addLink({
        label: hint.label,
        productKey,
        route: hint.route,
        href: appendRoute(productRoot, hint.route),
        aliases: [...(hint.aliases ?? [])],
      })
    }
  }

  const currentProductRoot = resolveProductRootUrl(currentKey, suiteHomeUrl, productLaunchUrls)
  const currentProductName = getSuiteProductCatalogEntry(currentKey)?.displayName
  for (const item of flattenNavigationItems(currentNavItems)) {
    addLink({
      label: currentProductName ? `${currentProductName} ${item.label}` : item.label,
      productKey: currentKey,
      route: item.to,
      href: appendRoute(currentProductRoot, item.to),
      aliases: [item.label],
    })
  }

  return [...links.values()]
}

function flattenNavigationItems(items: readonly AiNavigationItem[]): AiNavigationItem[] {
  return items.flatMap((item) => [item, ...flattenNavigationItems(item.children ?? [])])
}

function resolveProductRootUrl(
  productKey: string,
  suiteHomeUrl: string,
  productLaunchUrls: Record<string, string>,
): string {
  const normalized = normalizeProductKey(productKey)
  const launchUrl = resolveProductLaunchUrl(normalized, suiteHomeUrl, productLaunchUrls)
  if (normalized === 'nexarr') {
    return launchUrl.replace(/\/$/, '')
  }

  return launchUrl.replace(/\/launch(?=([?#]|$))/, '').replace(/\/$/, '')
}

function appendRoute(baseUrl: string, route: string): string {
  const base = baseUrl.replace(/\/$/, '')
  const normalizedRoute = normalizeRoute(route)
  if (normalizedRoute === '/') {
    return base || '/'
  }

  return `${base}${normalizedRoute}`
}

function normalizeRoute(route: string): string {
  const trimmed = route.trim()
  if (!trimmed) {
    return '/'
  }

  return trimmed.startsWith('/') ? trimmed : `/${trimmed}`
}
