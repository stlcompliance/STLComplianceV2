export type ResourceCategory = 'suite' | 'trust' | 'records' | 'contact'

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
    title: 'Products hub',
    summary:
      'Compare NexArr, StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, and Companion in plain language.',
    href: '/products',
    category: 'suite',
  },
  {
    id: 'nexarr-sign-in',
    title: 'Client sign-in through NexArr',
    summary: 'Customers use NexArr as the secure front door to the products their teams use.',
    href: '/products/nexarr',
    category: 'suite',
  },
  {
    id: 'security-trust',
    title: 'Security and platform trust',
    summary: 'How STL Compliance approaches secure access, customer records, and trusted workflows.',
    href: '/security',
    category: 'trust',
  },
  {
    id: 'data-ownership',
    title: 'Where records live',
    summary:
      'How records stay with the product built for that work, while connected views keep teams aligned.',
    href: '/data-ownership',
    category: 'records',
  },
  {
    id: 'implementation-maturity',
    title: 'Product status',
    summary: 'A plain snapshot of what is available now and what is still expanding.',
    href: '/maturity',
    category: 'suite',
  },
  {
    id: 'compare-approaches',
    title: 'Compare spreadsheets, point tools, and the suite',
    summary:
      'When spreadsheets or single-purpose tools still fit, and where connected operations help.',
    href: '/compare',
    category: 'suite',
  },
  {
    id: 'pricing-licensing',
    title: 'Pricing and licensing',
    summary: 'How product mix, rollout needs, and operational scope shape pricing.',
    href: '/pricing',
    category: 'contact',
  },
  {
    id: 'demo-contact',
    title: 'Demo and contact',
    summary: 'Request a walkthrough or reach the team.',
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
  records: 'Records',
  contact: 'Get started',
}

export function resourcesByCategory(category: ResourceCategory): ResourceLink[] {
  return RESOURCE_LINKS.filter((link) => link.category === category)
}
