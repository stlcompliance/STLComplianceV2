/** Public marketing routes included in sitemap generation (no auth or API routes). */
import { getProductRouteSlug, IMPLEMENTED_PRODUCT_KEYS } from '@stl/shared-ui'

const nonMarketingProductKeys = new Set(['nexarr'])

export const MARKETING_PRODUCT_KEYS = IMPLEMENTED_PRODUCT_KEYS.filter(
  (productKey) => !nonMarketingProductKeys.has(productKey),
)

export function productPath(productKey: string): string {
  return `/products/${getProductRouteSlug(productKey)}`
}

export function buildStaticPublicPaths(): string[] {
  return [
    '/',
    '/platform-overview',
    '/products',
    '/industries',
    '/use-cases',
    '/compliance',
    '/why-stl-compliance',
    '/about-founder',
    '/pricing',
    '/contact',
    '/faq',
    '/resources',
    '/compare',
    '/security',
    '/data-ownership',
    '/demo',
    '/privacy',
    '/terms',
    ...MARKETING_PRODUCT_KEYS.map(productPath),
  ]
}
