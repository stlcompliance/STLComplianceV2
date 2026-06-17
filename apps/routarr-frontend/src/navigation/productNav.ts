import {
  AlertTriangle,
  Calendar,
  Clock3,
  ClipboardList,
  Boxes,
  FileText,
  Globe,
  LayoutDashboard,
  ListCollapse,
  Map,
  MapPinned,
  Radio,
  Route,
  Settings,
  ShieldAlert,
  StickyNote,
  Truck,
  UserCheck,
} from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const routarrNavItems: ProductNavItem[] = [

  { label: 'Dashboard', to: '/dashboard', icon: LayoutDashboard as NavIcon },

  { label: 'Dispatch board', to: '/dispatch', icon: Radio as NavIcon },

  { label: 'Dispatch plans', to: '/dispatch-plans', icon: ShieldAlert as NavIcon },

  { label: 'TMS planning', to: '/transportation-demands', icon: Boxes as NavIcon },

  { label: 'Route planner', to: '/route-planner', icon: Map as NavIcon },

  { label: 'Driver portal', to: '/driver-portal', icon: Truck as NavIcon },

  { label: 'Customer portal', to: '/customer-portal', icon: Globe as NavIcon },

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

  { label: 'Stops', to: '/stops', icon: MapPinned as NavIcon },

  { label: 'Exceptions', to: '/exceptions', icon: AlertTriangle as NavIcon },

  { label: 'Reports', to: '/reports', icon: FileText as NavIcon },

  { label: 'Proof review', to: '/proof-review', icon: FileText as NavIcon },

  { label: 'Dock appointments', to: '/dock-appointments', icon: Clock3 as NavIcon },

  { label: 'Load visibility', to: '/load-visibility', icon: ClipboardList as NavIcon },

  { label: 'Validation blockers', to: '/validation-blockers', icon: ShieldAlert as NavIcon, sectionBreakBefore: true },

  { label: 'Availability', to: '/availability', icon: UserCheck as NavIcon },

  { label: 'Calendar', to: '/calendar', icon: Calendar as NavIcon },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },

]


