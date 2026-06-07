import {
  Database,
  FileQuestion,
  GitBranch,
  LayoutDashboard,
  Library,
  ListCollapse,
  Play,
  Scale,
  Upload,
  Search,
  Settings,
  StickyNote,
  FileText,
  Terminal,
  BarChart3,
  ListChecks,
} from 'lucide-react'

import type { ProductNavItem } from '@stl/shared-ui'



type NavIcon = NonNullable<ProductNavItem['icon']>



export const complianceCoreNavItems: ProductNavItem[] = [
  {
    label: 'Dashboard',
    to: '/dashboard',
    icon: LayoutDashboard as NavIcon,
  },
  {
    label: 'Registry',
    to: '/registry/drawer',
    icon: Library as NavIcon,
    children: [
      { label: 'Details', to: '/registry/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/registry/create', icon: StickyNote as NavIcon },
      { label: 'Governing bodies', to: '/governing-bodies', icon: Library as NavIcon },
      { label: 'Jurisdictions', to: '/jurisdictions', icon: Library as NavIcon },
      { label: 'Regulation sources', to: '/regulation-sources', icon: Library as NavIcon },
      { label: 'Rule packs', to: '/rulepacks', icon: Library as NavIcon },
    ],
  },

  {
    label: 'Mappings',
    to: '/mappings',
    icon: GitBranch as NavIcon,
    children: [
      { label: 'Citations', to: '/citations', icon: FileText as NavIcon },
      { label: 'Requirements', to: '/requirements', icon: FileText as NavIcon },
      { label: 'Evidence types', to: '/evidence-types', icon: FileText as NavIcon },
      { label: 'Evidence requirements', to: '/evidence-requirements', icon: FileText as NavIcon },
    ],
  },

  { label: 'Findings', to: '/findings', icon: Search as NavIcon },

  {
    label: 'Evaluation',
    to: '/evaluation',
    icon: Play as NavIcon,
    children: [
      { label: 'Applicability logic', to: '/applicability-logic', icon: Play as NavIcon },
    ],
  },

  { label: 'Theoretical situation', to: '/theoretical-situation', icon: FileQuestion as NavIcon },

  { label: 'Evidence mapping', to: '/evidence-mapping', icon: ListChecks as NavIcon },

  { label: 'Fact sources', to: '/fact-sources', icon: Database as NavIcon },

  { label: 'Imports', to: '/imports', icon: Upload as NavIcon },

  { label: 'Rule pack diff', to: '/rulepack-diff', icon: Scale as NavIcon },

  { label: 'Change impact', to: '/change-impact', icon: BarChart3 as NavIcon },

  {
    label: 'Reports',
    to: '/reports',
    icon: FileText as NavIcon,
    children: [
      { label: 'Exception exemptions', to: '/exception-exemptions', icon: FileText as NavIcon },
      { label: 'Exceptions', to: '/exceptions', icon: FileText as NavIcon },
      { label: 'Exemptions', to: '/exemptions', icon: FileText as NavIcon },
      { label: 'Waivers', to: '/waivers', icon: FileText as NavIcon },
      { label: 'Retention rules', to: '/retention-rules', icon: FileText as NavIcon },
    ],
  },

  { label: 'Operator', to: '/operator', icon: Terminal as NavIcon },

  { label: 'Admin', to: '/admin', icon: Settings as NavIcon, sectionBreakBefore: true },

]


