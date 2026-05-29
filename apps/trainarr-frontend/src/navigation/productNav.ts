import { GraduationCap, ListChecks, RefreshCw, BookOpen, Package, BadgeCheck, BarChart3, Settings } from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const trainarrNavItems: ProductNavItem[] = [

  { label: 'Programs', to: '/programs', icon: GraduationCap as NavIcon },

  { label: 'Assignments', to: '/assignments', icon: ListChecks as NavIcon },

  { label: 'Remediation', to: '/remediation', icon: RefreshCw as NavIcon },

  { label: 'Citations', to: '/citations', icon: BookOpen as NavIcon },

  { label: 'Rule packs', to: '/rule-packs', icon: Package as NavIcon },

  { label: 'Qualifications', to: '/qualifications', icon: BadgeCheck as NavIcon },

  { label: 'Reports', to: '/reports', icon: BarChart3 as NavIcon },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon },

]


