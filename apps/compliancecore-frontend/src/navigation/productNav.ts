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
    label: 'Overview',
    to: '/dashboard',
    icon: LayoutDashboard as NavIcon,
  },
  {
    label: 'Rulepacks',
    to: '/rulepacks',
    icon: Library as NavIcon,
    children: [
      { label: 'Installed', to: '/rulepacks/installed', icon: ListChecks as NavIcon },
      { label: 'Library', to: '/rulepacks/library', icon: Library as NavIcon },
      { label: 'Updates', to: '/rulepacks/updates', icon: Scale as NavIcon },
      { label: 'Imports', to: '/rulepacks/imports', icon: Upload as NavIcon },
    ],
  },

  {
    label: 'Mapping Center',
    to: '/mappings',
    icon: GitBranch as NavIcon,
    children: [
      { label: 'Coverage Matrix', to: '/mappings/coverage', icon: ListCollapse as NavIcon },
      { label: 'Fact Mappings', to: '/mappings/facts', icon: Database as NavIcon },
      { label: 'Evidence Mappings', to: '/mappings/evidence', icon: FileText as NavIcon },
      { label: 'Vocabulary Mappings', to: '/mappings/vocabulary', icon: StickyNote as NavIcon },
      { label: 'Subject Mappings', to: '/mappings/subjects', icon: Search as NavIcon },
      { label: 'Output Signals', to: '/mappings/outputs', icon: GitBranch as NavIcon },
    ],
  },

  {
    label: 'Evaluations',
    to: '/evaluation',
    icon: Play as NavIcon,
    children: [
      { label: 'Recent Runs', to: '/evaluation/recent', icon: BarChart3 as NavIcon },
      { label: 'Situation Tester', to: '/evaluation/tester', icon: FileQuestion as NavIcon },
      { label: 'Calculation Traces', to: '/evaluation/traces', icon: ListCollapse as NavIcon },
    ],
  },

  { label: 'Questionnaires', to: '/questionnaires', icon: FileQuestion as NavIcon },

  { label: 'Review Queue', to: '/findings', icon: Search as NavIcon },

  {
    label: 'Regulatory Registry',
    to: '/registry/drawer',
    icon: Library as NavIcon,
    children: [
      { label: 'Registry Workbench', to: '/registry/drawer', icon: ListCollapse as NavIcon },
      { label: 'Create Registry Item', to: '/registry/create', icon: StickyNote as NavIcon },
      { label: 'Governing Bodies', to: '/governing-bodies', icon: Library as NavIcon },
      { label: 'Jurisdictions', to: '/jurisdictions', icon: Library as NavIcon },
      { label: 'Citations', to: '/citations', icon: FileText as NavIcon },
      { label: 'Evidence Types', to: '/evidence-types', icon: FileText as NavIcon },
      { label: 'Evidence Requirements', to: '/evidence-requirements', icon: FileText as NavIcon },
      { label: 'Retention Rules', to: '/retention-rules', icon: FileText as NavIcon },
    ],
  },

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

  { label: 'Operator Console', to: '/operator', icon: Terminal as NavIcon },

  { label: 'Settings', to: '/settings', icon: Settings as NavIcon, sectionBreakBefore: true },

]


