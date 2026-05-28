import {
  ClipboardCheck,
  GraduationCap,
  Inbox,
  PackageSearch,
  Route,
  ShieldCheck,
  Users,
  Wrench,
  type LucideIcon,
} from 'lucide-react'

export type SuiteProductCatalogEntry = {
  productKey: string
  displayName: string
  icon: LucideIcon
  sortOrder: number
}

export const SUITE_PRODUCT_CATALOG: SuiteProductCatalogEntry[] = [
  { productKey: 'nexarr', displayName: 'NexArr', icon: ShieldCheck, sortOrder: 0 },
  { productKey: 'staffarr', displayName: 'StaffArr', icon: Users, sortOrder: 10 },
  { productKey: 'trainarr', displayName: 'TrainArr', icon: GraduationCap, sortOrder: 20 },
  { productKey: 'maintainarr', displayName: 'MaintainArr', icon: Wrench, sortOrder: 30 },
  { productKey: 'routarr', displayName: 'RoutArr', icon: Route, sortOrder: 40 },
  { productKey: 'supplyarr', displayName: 'SupplyArr', icon: PackageSearch, sortOrder: 50 },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    icon: ClipboardCheck,
    sortOrder: 60,
  },
  { productKey: 'companion', displayName: 'Companion', icon: Inbox, sortOrder: 70 },
]

export function normalizeProductKey(productKey: string): string {
  return productKey.trim().toLowerCase()
}

export function getSuiteProductIcon(productKey: string): LucideIcon {
  const normalized = normalizeProductKey(productKey)
  return (
    SUITE_PRODUCT_CATALOG.find((entry) => entry.productKey === normalized)?.icon ?? ShieldCheck
  )
}

export function hasProductEntitlement(
  entitlements: readonly string[],
  productKey: string,
): boolean {
  const normalized = normalizeProductKey(productKey)
  return entitlements.some((entry) => normalizeProductKey(entry) === normalized)
}

export function listEntitledSuiteProducts(
  entitlements: readonly string[],
): SuiteProductCatalogEntry[] {
  return SUITE_PRODUCT_CATALOG.filter((entry) =>
    hasProductEntitlement(entitlements, entry.productKey),
  )
}
