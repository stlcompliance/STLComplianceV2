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
      'Compare StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, LoadArr, Compliance Core, and Field Companion in plain language.',
    href: '/products',
    category: 'suite',
  },
  {
    id: 'platform-overview-resource',
    title: 'Platform overview',
    summary:
      'A practical view of why STL Compliance is organized as one suite with focused products and shared accountability.',
    href: '/platform-overview',
    category: 'suite',
  },
  {
    id: 'industries-resource',
    title: 'Industry use cases',
    summary: 'See how STL Compliance applies to fleet, warehousing, maintenance, and field operations.',
    href: '/industries',
    category: 'suite',
  },
  {
    id: 'use-cases-resource',
    title: 'Use case workflows',
    summary:
      'Review practical examples: dispatch readiness, receiving, maintenance parts, and supplier exception workflows.',
    href: '/use-cases',
    category: 'suite',
  },
  {
    id: 'client-sign-in',
    title: 'Client sign-in',
    summary: 'Customers use secure sign-in to access the products their teams use.',
    href: '/demo',
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
    summary: 'How product mix, operational scale, and compliance scope shape pricing.',
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
  {
    id: 'compliance-overview',
    title: 'Compliance guidance',
    summary: 'How STL Compliance helps prepare for audit review and supports evidence organization.',
    href: '/compliance',
    category: 'trust',
  },
  {
    id: 'about-founder-resource',
    title: 'About the founder',
    summary: 'A grounded perspective from the team behind STL Compliance and why this stack was built.',
    href: '/about-founder',
    category: 'suite',
  },
  {
    id: 'faq-resource',
    title: 'FAQ',
    summary: 'Answers about rollout, pricing approach, and practical expectations.',
    href: '/faq',
    category: 'suite',
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
