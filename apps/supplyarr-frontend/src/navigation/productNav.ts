import { Building2, PackageSearch, Warehouse, ShoppingCart, Truck, Tags, LineChart, Settings } from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

export const supplyarrNavItems: ProductNavItem[] = [
  { label: 'Parties', to: '/parties' , icon: Building2 },
  { label: 'Catalog', to: '/catalog' , icon: PackageSearch },
  { label: 'Inventory', to: '/inventory' , icon: Warehouse },
  { label: 'Purchasing', to: '/purchasing' , icon: ShoppingCart },
  { label: 'Receiving', to: '/receiving' , icon: Truck },
  { label: 'Pricing', to: '/pricing' , icon: Tags },
  { label: 'Planning', to: '/planning' , icon: LineChart },
  { label: 'Settings', to: '/settings' , icon: Settings },
]
