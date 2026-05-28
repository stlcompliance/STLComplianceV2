import { LayoutDashboard, Boxes, CalendarClock, Gauge, Wrench, AlertTriangle, ClipboardCheck, FileStack, History, Settings } from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const maintainarrNavItems: ProductNavItem[] = [
  { label: 'Overview', to: '/overview', icon: LayoutDashboard as NavIcon },
  { label: 'Assets', to: '/assets', icon: Boxes as NavIcon },
  { label: 'PM programs', to: '/pm-programs', icon: CalendarClock as NavIcon },
  { label: 'Meters', to: '/meters', icon: Gauge as NavIcon },
  { label: 'Work orders', to: '/work-orders', icon: Wrench as NavIcon },
  { label: 'Defects', to: '/defects', icon: AlertTriangle as NavIcon },
  { label: 'Inspections', to: '/inspections', icon: ClipboardCheck as NavIcon },
  { label: 'Templates', to: '/inspection-templates', icon: FileStack as NavIcon },
  { label: 'History', to: '/history', icon: History as NavIcon },
  { label: 'Settings', to: '/settings', icon: Settings as NavIcon },
]
