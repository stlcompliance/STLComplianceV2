import {
  Building2,
  PackageSearch,
  ShoppingCart,
  Tags,
  LineChart,
  Gauge,
  Settings,
  ListCollapse,
  StickyNote,
} from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const supplyarrNavItems: ProductNavItem[] = [
  {
    label: 'Parties',
    to: '/parties/drawer',
    icon: Building2 as NavIcon,
    children: [
      { label: 'Details', to: '/parties/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/parties/create', icon: StickyNote as NavIcon },
    ],
  },
  { label: 'Catalog', to: '/catalog', icon: PackageSearch as NavIcon },
  {
    label: 'Purchasing',
    to: '/purchasing',
    icon: ShoppingCart as NavIcon,
    children: [
      { label: 'Procurement', to: '/purchasing/procurement' },
      { label: 'Approvals', to: '/purchasing/approvals' },
      { label: 'Exceptions', to: '/purchasing/exceptions' },
      { label: 'Vendor orders', to: '/purchasing/vendor-orders' },
      { label: 'Create vendor order', to: '/purchasing/vendor-orders/create' },
    ],
  },
  { label: 'Pricing', to: '/pricing', icon: Tags as NavIcon },
  { label: 'Planning', to: '/planning', icon: LineChart as NavIcon },
  { label: 'Readiness', to: '/readiness', icon: Gauge as NavIcon },
  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },
]
