import {
  UserCircle,
  Users,
  UsersRound,
  Network,
  Shield,
  Activity,
  AlertCircle,
  ClipboardCheck,
  Award,
  Settings,
  ListCollapse,
  StickyNote,
} from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

type NavIcon = NonNullable<ProductNavItem['icon']>

export const staffarrNavItems: ProductNavItem[] = [
  { label: 'My profile', to: '/me', icon: UserCircle },
  { label: 'My team', to: '/my-team', icon: UsersRound },
  {
    label: 'People',
    to: '/people/drawer',
    icon: Users,
    children: [
      { label: 'Details', to: '/people/details', icon: ListCollapse as NavIcon },
      { label: 'Create', to: '/people/create', icon: StickyNote as NavIcon },
    ],
  },
  { label: 'Org structure', to: '/org', icon: Network },
  { label: 'Permissions', to: '/permissions', icon: Shield },
  { label: 'Readiness', to: '/readiness', icon: Activity },
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
  { label: 'Certifications', to: '/certifications' , icon: Award },
  { label: 'Admin', to: '/admin', icon: Settings, sectionBreakBefore: true },
]
