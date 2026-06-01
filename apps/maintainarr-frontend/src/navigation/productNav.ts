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
      { label: 'Drawer', to: '/assets/drawer' },
      { label: 'Details', to: '/assets/details' },
      { label: 'Create', to: '/assets/create' },
    ],
  },
  {
    label: 'PM programs',
    to: '/pm-programs/drawer',
    icon: CalendarClock as NavIcon,
    children: [
      { label: 'Drawer', to: '/pm-programs/drawer' },
      { label: 'Details', to: '/pm-programs/details' },
      { label: 'Create', to: '/pm-programs/create' },
    ],
  },
  {
    label: 'Meters',
    to: '/meters/drawer',
    icon: Gauge as NavIcon,
    children: [
      { label: 'Drawer', to: '/meters/drawer' },
      { label: 'Details', to: '/meters/details' },
      { label: 'Create', to: '/meters/create' },
    ],
  },
  { label: 'Work orders', to: '/work-orders', icon: Wrench as NavIcon },
  { label: 'Defects', to: '/defects', icon: AlertTriangle as NavIcon },
  { label: 'Inspections', to: '/inspections', icon: ClipboardCheck as NavIcon },
  { label: 'Templates', to: '/inspection-templates', icon: FileStack as NavIcon },
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
  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },
]
