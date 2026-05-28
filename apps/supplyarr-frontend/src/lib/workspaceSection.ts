export const WORKSPACE_SECTIONS = [
  'parties',
  'catalog',
  'inventory',
  'purchasing',
  'receiving',
  'pricing',
  'planning',
  'reports',
  'settings',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'parties'

export function workspaceSectionFromPathname(pathname: string): WorkspaceSection {
  const segment = pathname.split('/').filter(Boolean)[0]
  if (segment && WORKSPACE_SECTIONS.includes(segment as WorkspaceSection)) {
    return segment as WorkspaceSection
  }
  return DEFAULT_WORKSPACE_SECTION
}

export const workspaceSectionHeaders: Record<
  WorkspaceSection,
  { title: string; subtitle: string }
> = {
  parties: { title: 'Parties', subtitle: 'Vendors, dealers, and suppliers' },
  catalog: { title: 'Part catalog', subtitle: 'Parts, alternates, and catalog maintenance' },
  inventory: { title: 'Inventory', subtitle: 'Stock positions and adjustments' },
  purchasing: { title: 'Purchasing', subtitle: 'Purchase requests and orders' },
  receiving: { title: 'Receiving', subtitle: 'Receiving, returns, and backorders' },
  pricing: { title: 'Pricing', subtitle: 'Pricing snapshots and lead times' },
  planning: {
    title: 'Planning',
    subtitle: 'Availability, reorder evaluation, and demand references',
  },
  reports: {
    title: 'Reports',
    subtitle: 'Vendor, inventory, and procurement reporting',
  },
  settings: { title: 'Settings', subtitle: 'Notification preferences' },
}
