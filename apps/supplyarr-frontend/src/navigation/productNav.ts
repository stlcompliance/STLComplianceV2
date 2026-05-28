import {
  Building2,
  PackageSearch,
  Warehouse,
  ShoppingCart,
  Truck,
  Tags,
  LineChart,
  BarChart3,
  Settings,
} from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const supplyarrNavItems: ProductNavItem[] = [
  { label: 'Parties', to: '/parties', icon: Building2 as NavIcon },
  { label: 'Catalog', to: '/catalog', icon: PackageSearch as NavIcon },
  { label: 'Inventory', to: '/inventory', icon: Warehouse as NavIcon },
  { label: 'Purchasing', to: '/purchasing', icon: ShoppingCart as NavIcon },
  { label: 'Receiving', to: '/receiving', icon: Truck as NavIcon },
  { label: 'Pricing', to: '/pricing', icon: Tags as NavIcon },
  { label: 'Planning', to: '/planning', icon: LineChart as NavIcon },
  { label: 'Reports', to: '/reports', icon: BarChart3 as NavIcon },
  { label: 'Settings', to: '/settings', icon: Settings as NavIcon },
]
