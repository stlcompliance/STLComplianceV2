import {
  UserCircle,
  Users,
  UsersRound,
  Network,
  Map,
  ShieldAlert,
  Activity,
  AlertCircle,
  ClipboardCheck,
  Award,
  FileText,
  SlidersHorizontal,
  Settings,
  ListCollapse,
  StickyNote,
  KeyRound,
} from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const staffarrNavItems: ProductNavItem[] = [
  { label: 'My profile', to: '/me', icon: UserCircle },
  { label: 'My team', to: '/my-team', icon: UsersRound },
  { label: 'Timekeeping', to: '/timekeeping', icon: ClipboardCheck },
  {
    label: 'People',
    to: '/people/drawer',
    icon: Users,
    children: [
      { label: 'Details', to: '/people/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/people/create', icon: StickyNote as NavIcon },
    ],
  },
  { label: 'Org structure', to: '/organization-structure?tab=organization', icon: Network },
  { label: 'Locations', to: '/organization-structure?tab=locations', icon: Map },
  { label: 'Roles', to: '/roles', icon: KeyRound },
  { label: 'Readiness', to: '/readiness', icon: Activity },
  { label: 'Restrictions', to: '/restrictions', icon: ShieldAlert },
  {
    label: 'Incidents',
    to: '/incidents',
    icon: AlertCircle,
    children: [
      { label: 'Review', to: '/incidents', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/incidents/create', icon: StickyNote as NavIcon },
    ],
  },
  { label: 'Training acks', to: '/training-acknowledgements', icon: ClipboardCheck },
  { label: 'Reports', to: '/reports', icon: FileText },
  { label: 'Certifications', to: '/certifications', icon: Award },
  { label: 'Settings', to: '/settings', icon: SlidersHorizontal },
  { label: 'Admin', to: '/admin', icon: Settings, sectionBreakBefore: true },
]
