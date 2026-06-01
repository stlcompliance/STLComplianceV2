import { GraduationCap, ListChecks, RefreshCw, BookOpen, Package, BadgeCheck, BarChart3, Settings } from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const trainarrNavItems: ProductNavItem[] = [
  {
    label: 'Programs',
    to: '/programs/drawer',
    icon: GraduationCap as NavIcon,
    children: [
      { label: 'Details', to: '/programs/details' },
      { label: 'Create', to: '/programs/create' },
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
      { label: 'Details', to: '/rule-packs/details' },
      { label: 'Create', to: '/rule-packs/create' },
    ],
  },

  { label: 'Qualifications', to: '/qualifications', icon: BadgeCheck as NavIcon },

  { label: 'Reports', to: '/reports', icon: BarChart3 as NavIcon, sectionBreakBefore: true },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },

]


