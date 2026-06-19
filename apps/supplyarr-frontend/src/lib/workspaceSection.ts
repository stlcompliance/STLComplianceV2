export const WORKSPACE_SECTIONS = [
  'dashboard',
  'suppliers',
  'onboarding',
  'rfqs',
  'quotes',
  'purchase-orders',
  'catalog',
  'contracts',
  'documents',
  'performance',
  'risk',
  'corrective-actions',
  'supplier-portal',
  'reports',
  'settings',
] as const

export type WorkspaceSection = (typeof WORKSPACE_SECTIONS)[number]

export const DEFAULT_WORKSPACE_SECTION: WorkspaceSection = 'dashboard'

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
  dashboard: {
    title: 'Dashboard',
    subtitle: 'Supplier risk, purchasing blockers, and live procurement attention',
  },
  suppliers: {
    title: 'Suppliers',
    subtitle: 'Vendors, suppliers, dealers, and supplier detail records',
  },
  onboarding: {
    title: 'Onboarding',
    subtitle: 'Supplier qualification, documents, and approval workflow',
  },
  rfqs: { title: 'RFQs', subtitle: 'Supplier sourcing, invitations, and quote intake' },
  quotes: { title: 'Quotes', subtitle: 'Compare vendor pricing, lead time, and award decisions' },
  'purchase-orders': { title: 'Purchase orders', subtitle: 'PO lifecycle, acknowledgments, and release' },
  catalog: { title: 'Catalogs', subtitle: 'Parts, substitutes, and supplier item relationships' },
  contracts: { title: 'Contracts', subtitle: 'Agreement metadata, terms, and renewal tracking' },
  documents: { title: 'Documents', subtitle: 'Supplier documents and RecordArr handoffs' },
  performance: { title: 'Performance', subtitle: 'Scorecards, trends, and procurement analytics' },
  risk: { title: 'Risk', subtitle: 'Supplier risk, restrictions, and exception exposure' },
  'corrective-actions': {
    title: 'Corrective actions',
    subtitle: 'Supplier incidents, holds, and corrective workflows',
  },
  'supplier-portal': {
    title: 'Supplier portal',
    subtitle: 'Portal access, onboarding, quotes, acknowledgments, and updates',
  },
  reports: {
    title: 'Reports',
    subtitle: 'Vendor, compliance, inventory, and purchasing dashboards',
  },
  settings: { title: 'Settings', subtitle: 'Notifications, automation, and procurement governance' },
}
