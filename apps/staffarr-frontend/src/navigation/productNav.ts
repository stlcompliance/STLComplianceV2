import {
  Users,
  Network,
  Shield,
  Activity,
  AlertCircle,
  ClipboardCheck,
  Award,
  BarChart3,
  Settings,
} from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

export const staffarrNavItems: ProductNavItem[] = [
  { label: 'People', to: '/people' , icon: Users },
  { label: 'Org structure', to: '/org', icon: Network },
  { label: 'Permissions', to: '/permissions' , icon: Shield },
  { label: 'Readiness', to: '/readiness' , icon: Activity },
  { label: 'Incidents', to: '/incidents' , icon: AlertCircle },
  { label: 'Training acks', to: '/training-acknowledgements', icon: ClipboardCheck },
  { label: 'Certifications', to: '/certifications' , icon: Award },
  { label: 'Reports', to: '/reports', icon: BarChart3 },
  { label: 'Admin', to: '/admin' , icon: Settings },
]
