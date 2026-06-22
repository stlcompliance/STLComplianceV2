import {
  LayoutDashboard,
  FileText,
  ClipboardList,
  FileSignature,
  PackageSearch,
  ShieldAlert,
  ShoppingCart,
  LineChart,
  Gauge,
  Settings,
  ListCollapse,
  StickyNote,
  Store,
  Users,
  Upload,
} from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const supplyarrNavItems: ProductNavItem[] = [
  {
    label: 'Dashboard',
    to: '/dashboard',
    icon: LayoutDashboard as NavIcon,
  },
  {
    label: 'Suppliers',
    to: '/suppliers/drawer',
    icon: Users as NavIcon,
    children: [
      { label: 'Details', to: '/suppliers/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/suppliers/create', icon: StickyNote as NavIcon },
    ],
  },
  { label: 'Onboarding', to: '/onboarding', icon: ClipboardList as NavIcon },
  {
    label: 'RFQs',
    to: '/rfqs',
    icon: FileSignature as NavIcon,
    children: [
      { label: 'Quotes', to: '/quotes' },
    ],
  },
  { label: 'Purchase orders', to: '/purchase-orders', icon: ShoppingCart as NavIcon },
  { label: 'Catalog', to: '/catalog', icon: PackageSearch as NavIcon },
  { label: 'Contracts', to: '/contracts', icon: FileText as NavIcon },
  { label: 'Documents', to: '/documents', icon: FileText as NavIcon },
  { label: 'Performance', to: '/performance', icon: LineChart as NavIcon },
  { label: 'Risk', to: '/risk', icon: Gauge as NavIcon },
  { label: 'Corrective actions', to: '/corrective-actions', icon: ShieldAlert as NavIcon },
  { label: 'Supplier portal', to: '/supplier-portal', icon: Store as NavIcon },
  { label: 'Reports', to: '/reports', icon: FileText as NavIcon },
  {
    label: 'Settings',
    to: '/settings',
    icon: Settings as NavIcon,
    sectionBreakBefore: true,
    children: [{ label: 'Imports', to: '/imports', icon: Upload as NavIcon }],
  },
]
