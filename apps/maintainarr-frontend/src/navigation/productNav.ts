import { LayoutDashboard, Boxes, CalendarClock, Gauge, Wrench, AlertTriangle, ClipboardCheck, FileStack, History, TimerOff, BarChart3, Settings } from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const maintainarrNavItems: ProductNavItem[] = [
  { label: 'Overview', to: '/overview', icon: LayoutDashboard as NavIcon },
  {
    label: 'Assets',
    to: '/assets/drawer',
    icon: Boxes as NavIcon,
    children: [
      { label: 'Details', to: '/assets/details' },
      { label: 'Create', to: '/assets/create' },
    ],
  },
  {
    label: 'PM programs',
    to: '/pm-programs/drawer',
    icon: CalendarClock as NavIcon,
    children: [
      { label: 'Details', to: '/pm-programs/details' },
      { label: 'Create', to: '/pm-programs/create' },
    ],
  },
  {
    label: 'Meters',
    to: '/meters/drawer',
    icon: Gauge as NavIcon,
    children: [
      { label: 'Details', to: '/meters/details' },
      { label: 'Create', to: '/meters/create' },
    ],
  },
  {
    label: 'Work orders',
    to: '/work-orders/drawer',
    icon: Wrench as NavIcon,
    children: [
      { label: 'Details', to: '/work-orders/details' },
      { label: 'Create', to: '/work-orders/create' },
    ],
  },
  {
    label: 'Defects',
    to: '/defects/drawer',
    icon: AlertTriangle as NavIcon,
    children: [
      { label: 'Details', to: '/defects/details' },
      { label: 'Create', to: '/defects/create' },
    ],
  },
  {
    label: 'Inspections',
    to: '/inspections/drawer',
    icon: ClipboardCheck as NavIcon,
    children: [
      { label: 'Details', to: '/inspections/details' },
      { label: 'Create', to: '/inspections/create' },
    ],
  },
  {
    label: 'Templates',
    to: '/inspection-templates/drawer',
    icon: FileStack as NavIcon,
    children: [
      { label: 'Details', to: '/inspection-templates/details' },
      { label: 'Create', to: '/inspection-templates/create' },
    ],
  },
  { label: 'History', to: '/history', icon: History as NavIcon },
  { label: 'Downtime', to: '/downtime', icon: TimerOff as NavIcon },
  {
    label: 'Reports',
    to: '/reports',
    icon: BarChart3 as NavIcon,
    sectionBreakBefore: true,
    children: [
      { label: 'Compliance', to: '/reports/compliance' },
      { label: 'Executive', to: '/reports/executive' },
      { label: 'Maintenance', to: '/reports/maintenance' },
      { label: 'Exports', to: '/reports/exports' },
    ],
  },
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
