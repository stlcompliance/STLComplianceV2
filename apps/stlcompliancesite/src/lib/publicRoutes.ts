/** Public marketing routes included in sitemap generation (no auth or API routes). */
export const MARKETING_PRODUCT_KEYS = [
  'nexarr',
  'staffarr',
  'trainarr',
  'maintainarr',
  'routarr',
  'supplyarr',
  'compliancecore',
  'companion',
] as const

export function productPath(productKey: string): string {
  return `/products/${productKey.trim().toLowerCase()}`
}

export function buildStaticPublicPaths(): string[] {
  return [
    '/',
    '/products',
    '/resources',
    '/compare',
    '/maturity',
    '/pricing',
    '/security',
    '/data-ownership',
    '/demo',
    '/privacy',
    '/terms',
    ...MARKETING_PRODUCT_KEYS.map(productPath),
  ]
}
