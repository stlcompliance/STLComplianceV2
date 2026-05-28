export type ResourceCategory = 'suite' | 'trust' | 'ownership' | 'contact'

export type ResourceLink = {
  id: string
  title: string
  summary: string
  href: string
  category: ResourceCategory
  external?: boolean
}

export const RESOURCE_LINKS: ResourceLink[] = [
  {
    id: 'products-hub',
    title: 'Products hub and ownership map',
    summary:
      'Compare NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, and Companion with accurate owns / does-not-own language.',
    href: '/products',
    category: 'suite',
  },
  {
    id: 'nexarr-sign-in',
    title: 'Client sign-in through NexArr',
    summary:
      'Operational access always flows through NexArr identity, tenant context, entitlements, and launch — never through this marketing site.',
    href: '/products/nexarr',
    category: 'suite',
  },
  {
    id: 'security-trust',
    title: 'Security and platform trust',
    summary:
      'How the suite separates products, enforces server-side authorization, and treats customer-hosted data as untrusted until validated.',
    href: '/security',
    category: 'trust',
  },
  {
    id: 'data-ownership',
    title: 'Data ownership and mirrors',
    summary:
      'Each product owns its PostgreSQL database; cross-product relationships use APIs, events, and rebuildable mirrors — not shared foreign keys.',
    href: '/data-ownership',
    category: 'ownership',
  },
  {
    id: 'pricing-licensing',
    title: 'Pricing and licensing narrative',
    summary:
      'How NexArr tenant entitlements package the suite — no checkout or list prices on this marketing site.',
    href: '/pricing',
    category: 'contact',
  },
  {
    id: 'demo-contact',
    title: 'Demo and contact',
    summary: 'Request a walkthrough or reach the team — client-side mailto only; no tenant data is collected on this site.',
    href: '/demo',
    category: 'contact',
  },
  {
    id: 'privacy-terms',
    title: 'Privacy and terms',
    summary: 'Public legal pages for visitors evaluating the suite.',
    href: '/privacy',
    category: 'trust',
  },
]

export const RESOURCE_CATEGORY_LABELS: Record<ResourceCategory, string> = {
  suite: 'Suite education',
  trust: 'Trust and legal',
  ownership: 'Ownership model',
  contact: 'Get started',
}

export function resourcesByCategory(category: ResourceCategory): ResourceLink[] {
  return RESOURCE_LINKS.filter((link) => link.category === category)
}
