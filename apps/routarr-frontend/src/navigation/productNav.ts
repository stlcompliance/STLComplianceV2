import { Radio, Route, Map, UserCheck, Calendar, Settings, Truck, BarChart3, ListCollapse, StickyNote } from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const routarrNavItems: ProductNavItem[] = [

  { label: 'Dispatch', to: '/dispatch', icon: Radio as NavIcon },

  { label: 'Driver portal', to: '/driver-portal', icon: Truck as NavIcon },

  {
    label: 'Trips',
    to: '/trips/drawer',
    icon: Route as NavIcon,
    children: [
      { label: 'Details', to: '/trips/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/trips/create', icon: StickyNote as NavIcon },
    ],
  },

  {
    label: 'Routes',
    to: '/routes/drawer',
    icon: Map as NavIcon,
    children: [
      { label: 'Details', to: '/routes/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/routes/create', icon: StickyNote as NavIcon },
    ],
  },

  { label: 'Availability', to: '/availability', icon: UserCheck as NavIcon },

  { label: 'Calendar', to: '/calendar', icon: Calendar as NavIcon },

  { label: 'Reports', to: '/reports', icon: BarChart3 as NavIcon, sectionBreakBefore: true },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },

]


