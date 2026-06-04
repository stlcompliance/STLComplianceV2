/** Public marketing routes included in sitemap generation (no auth or API routes). */
function routeSlug(productKey: string): string {
  const normalized = productKey.trim().toLowerCase().replace(/[-_]/g, '')
  return normalized === 'companion' ? 'fieldcompanion' : normalized === 'fieldcompanion'
    ? 'field-companion'
    : normalized
}

export const MARKETING_PRODUCT_KEYS = [
  'staffarr',
  'trainarr',
  'maintainarr',
  'routarr',
  'supplyarr',
  'compliancecore',
  'fieldcompanion',
] as const

export function productPath(productKey: string): string {
  return `/products/${routeSlug(productKey)}`
}

export function buildStaticPublicPaths(): string[] {
  return [
    '/',
    '/products',
    '/resources',
    '/compare',
    '/pricing',
    '/security',
    '/data-ownership',
    '/demo',
    '/privacy',
    '/terms',
    ...MARKETING_PRODUCT_KEYS.map(productPath),
  ]
}
