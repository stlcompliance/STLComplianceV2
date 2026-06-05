import {
  Activity,
  AlertTriangle,
  Camera,
  Archive,
  Bell,
  BookOpenCheck,
  Boxes,
  Building2,
  CalendarClock,
  ClipboardCheck,
  ClipboardList,
  Database,
  Factory,
  FileText,
  GraduationCap,
  HardHat,
  Inbox,
  KeyRound,
  LayoutDashboard,
  LockKeyhole,
  PackageSearch,
  BarChart3,
  Route,
  Search,
  Settings,
  Shield,
  ShieldAlert,
  ShieldCheck,
  Truck,
  UserCog,
  Users,
  Warehouse,
  Wrench,
  type LucideIcon,
} from 'lucide-react'

export const navIcons = {
  dashboard: LayoutDashboard,
  nexarr: ShieldCheck,
  staffarr: Users,
  trainarr: GraduationCap,
  maintainarr: Wrench,
  routarr: Route,
  supplyarr: PackageSearch,
  loadarr: Warehouse,
  recordarr: Archive,
  reportarr: BarChart3,
  assurarr: ShieldAlert,
  fieldcompanion: Inbox,
  inbox: Inbox,
  camera: Camera,
  complianceCore: ClipboardCheck,
  settings: Settings,
  notifications: Bell,
  search: Search,
  documents: FileText,
  sites: Building2,
  permissions: KeyRound,
  fleet: Truck,
  safety: HardHat,
  inventory: Boxes,
  training: BookOpenCheck,
  database: Database,
  activity: Activity,
  warning: AlertTriangle,
  inspections: ClipboardList,
  preventiveMaintenance: CalendarClock,
  facilities: Factory,
  warehouse: Warehouse,
  userAdmin: UserCog,
  auth: LockKeyhole,
  complianceAlert: ShieldAlert,
  shield: Shield,
} satisfies Record<string, LucideIcon>

export type NavIconKey = keyof typeof navIcons

export function getNavIcon(iconKey: string): LucideIcon {
  const normalized = iconKey.trim() as NavIconKey
  return navIcons[normalized] ?? navIcons.dashboard
}

export function getProductNavIcon(productKey: string): LucideIcon {
  const normalized = productKey.trim().toLowerCase() as NavIconKey
  if (normalized in navIcons) {
    return navIcons[normalized as NavIconKey]
  }

  return navIcons.dashboard
}
