import { GraduationCap, ListChecks, RefreshCw, BookOpen, Package, BadgeCheck, BarChart3, Settings, ListCollapse, StickyNote } from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const trainarrNavItems: ProductNavItem[] = [
  {
    label: 'Programs',
    to: '/programs/drawer',
    icon: GraduationCap as NavIcon,
    children: [
      { label: 'Details', to: '/programs/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/programs/create', icon: StickyNote as NavIcon },
    ],
  },

  {
    label: 'Assignments',
    to: '/assignments',
    icon: ListChecks as NavIcon,
    children: [
      { label: 'Manual', to: '/assignments/manual' },
      { label: 'Queue', to: '/assignments/queue' },
      { label: 'Evaluation', to: '/assignments/evaluation' },
    ],
  },

  { label: 'Remediation', to: '/remediation', icon: RefreshCw as NavIcon },

  { label: 'Citations', to: '/citations', icon: BookOpen as NavIcon },

  {
    label: 'Rule packs',
    to: '/rule-packs/drawer',
    icon: Package as NavIcon,
    children: [
      { label: 'Details', to: '/rule-packs/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/rule-packs/create', icon: StickyNote as NavIcon },
    ],
  },

  { label: 'Qualifications', to: '/qualifications', icon: BadgeCheck as NavIcon },

  { label: 'Reports', to: '/reports', icon: BarChart3 as NavIcon, sectionBreakBefore: true },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },

]


