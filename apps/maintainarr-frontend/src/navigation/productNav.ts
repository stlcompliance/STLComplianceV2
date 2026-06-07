import { LayoutDashboard, Boxes, CalendarClock, Gauge, Wrench, AlertTriangle, ClipboardCheck, FileStack, History, TimerOff, Settings, ListCollapse, StickyNote, Package } from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const maintainarrNavItems: ProductNavItem[] = [
  { label: 'Readiness', to: '/overview', icon: LayoutDashboard as NavIcon },
  {
    label: 'Assets',
    to: '/assets/drawer',
    icon: Boxes as NavIcon,
    children: [
      { label: 'Details', to: '/assets/drawer', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/assets/new', icon: StickyNote as NavIcon },
    ],
  },
  {
    label: 'PM programs',
    to: '/pm-programs/drawer',
    icon: CalendarClock as NavIcon,
    children: [
      { label: 'Details', to: '/pm-programs/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/pm-programs/create', icon: StickyNote as NavIcon },
    ],
  },
  {
    label: 'Meters',
    to: '/meters/drawer',
    icon: Gauge as NavIcon,
    children: [
      { label: 'Details', to: '/meters/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/meters/create', icon: StickyNote as NavIcon },
    ],
  },
  {
    label: 'Work orders',
    to: '/work-orders/drawer',
    icon: Wrench as NavIcon,
    children: [
      { label: 'Details', to: '/work-orders/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/work-orders/create', icon: StickyNote as NavIcon },
    ],
  },
  {
    label: 'Defects',
    to: '/defects/drawer',
    icon: AlertTriangle as NavIcon,
    children: [
      { label: 'Details', to: '/defects/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/defects/create', icon: StickyNote as NavIcon },
    ],
  },
  {
    label: 'Inspections',
    to: '/inspections/drawer',
    icon: ClipboardCheck as NavIcon,
    children: [
      { label: 'Details', to: '/inspections/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/inspections/create', icon: StickyNote as NavIcon },
    ],
  },
  {
    label: 'Templates',
    to: '/inspection-templates/drawer',
    icon: FileStack as NavIcon,
    children: [
      { label: 'Details', to: '/inspection-templates/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/inspection-templates/create', icon: StickyNote as NavIcon },
    ],
  },
  { label: 'Parts kits', to: '/parts-kits', icon: Package as NavIcon },
  { label: 'History', to: '/history', icon: History as NavIcon },
  { label: 'Downtime', to: '/downtime', icon: TimerOff as NavIcon },
  {
    label: 'Settings',
    to: '/settings',
    icon: Settings as NavIcon,
    sectionBreakBefore: true,
    children: [
      { label: 'Workspace', to: '/settings' },
      { label: 'Source Data', to: '/settings/source-data' },
    ],
  },
]
