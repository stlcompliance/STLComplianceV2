import { Radio, Route, Map, UserCheck, Calendar, Settings } from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const routarrNavItems: ProductNavItem[] = [

  { label: 'Dispatch', to: '/dispatch', icon: Radio as NavIcon },

  { label: 'Trips', to: '/trips', icon: Route as NavIcon },

  { label: 'Routes', to: '/routes', icon: Map as NavIcon },

  { label: 'Availability', to: '/availability', icon: UserCheck as NavIcon },

  { label: 'Calendar', to: '/calendar', icon: Calendar as NavIcon },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon },

]


