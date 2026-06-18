import {
  Banknote,
  BookOpenCheck,
  Building2,
  ClipboardList,
  FileChartColumn,
  Landmark,
  LayoutDashboard,
  Receipt,
  Scale,
  Settings,
  Split,
  WalletCards,
} from 'lucide-react'
import type { ProductNavItem } from '@stl/shared-ui'

export const ledgArrNavItems: ProductNavItem[] = [
  { label: 'Dashboard', to: '/dashboard', icon: LayoutDashboard as ProductNavItem['icon'] },
  { label: 'General Ledger', to: '/general-ledger', icon: BookOpenCheck as ProductNavItem['icon'] },
  { label: 'Payables', to: '/payables', icon: WalletCards as ProductNavItem['icon'] },
  { label: 'Receivables', to: '/receivables', icon: Banknote as ProductNavItem['icon'] },
  { label: 'Billing', to: '/billing', icon: Receipt as ProductNavItem['icon'] },
  { label: 'Banking', to: '/banking', icon: Landmark as ProductNavItem['icon'] },
  { label: 'Budgets', to: '/budgets', icon: ClipboardList as ProductNavItem['icon'] },
  { label: 'Fixed Assets', to: '/fixed-assets', icon: Building2 as ProductNavItem['icon'] },
  { label: 'Tax', to: '/tax', icon: Scale as ProductNavItem['icon'] },
  { label: 'Intercompany', to: '/intercompany', icon: Split as ProductNavItem['icon'] },
  { label: 'Close', to: '/close', icon: ClipboardList as ProductNavItem['icon'] },
  { label: 'Reports', to: '/reports', icon: FileChartColumn as ProductNavItem['icon'] },
  { label: 'Settings', to: '/settings', icon: Settings as ProductNavItem['icon'], sectionBreakBefore: true },
]
