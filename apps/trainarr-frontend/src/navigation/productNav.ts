import {
  BadgeCheck,
  BookOpen,
  GraduationCap,
  ClipboardCheck,
  LayoutDashboard,
  ListChecks,
  ListCollapse,
  RefreshCw,
  Layers,
  FileText,
  Settings,
  StickyNote,
} from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const trainarrNavItems: ProductNavItem[] = [
  { label: 'My Training', to: '/my-training', icon: LayoutDashboard as NavIcon },

  {
    label: 'Training Catalog',
    to: '/catalog',
    icon: BookOpen as NavIcon,
    children: [
      { label: 'Browse', to: '/catalog', icon: BookOpen as NavIcon },
      { label: 'Builder', to: '/programs/drawer', icon: GraduationCap as NavIcon },
      { label: 'Create', to: '/programs/create', icon: StickyNote as NavIcon },
    ],
  },

  {
    label: 'Course Player',
    to: '/assignments',
    icon: ListChecks as NavIcon,
    children: [
      { label: 'Learner Queue', to: '/assignments/queue' },
      { label: 'Instructor View', to: '/assignments/manual' },
      { label: 'Evaluator View', to: '/assignments/evaluation' },
    ],
  },

  { label: 'Instructor Console', to: '/instructor', icon: ClipboardCheck as NavIcon },

  { label: 'Evaluator Console', to: '/evaluator', icon: BadgeCheck as NavIcon },

  { label: 'Remediation', to: '/remediation', icon: RefreshCw as NavIcon },

  { label: 'Content Library', to: '/citations', icon: BookOpen as NavIcon },

  {
    label: 'Compliance Mapping',
    to: '/rule-packs/drawer',
    icon: Layers as NavIcon,
    children: [
      { label: 'Details', to: '/rule-packs/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/rule-packs/create', icon: StickyNote as NavIcon },
    ],
  },

  { label: 'Training Matrix', to: '/matrix', icon: ListChecks as NavIcon },

  { label: 'Certificate Registry', to: '/certificates', icon: BadgeCheck as NavIcon },

  { label: 'Reports', to: '/reports', icon: FileText as NavIcon },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },

]


