import {
  Boxes,
  ClipboardCheck,
  GraduationCap,
  LayoutDashboard,
  Route,
  Shield,
  Users,
  Wrench,
  type LucideIcon,
} from 'lucide-react'

const iconByProductKey: Record<string, LucideIcon> = {
  nexarr: Shield,
  staffarr: Users,
  trainarr: GraduationCap,
  maintainarr: Wrench,
  routarr: Route,
  supplyarr: Boxes,
  compliancecore: ClipboardCheck,
}

export function getProductIcon(productKey: string): LucideIcon {
  return iconByProductKey[productKey.toLowerCase()] ?? LayoutDashboard
}
