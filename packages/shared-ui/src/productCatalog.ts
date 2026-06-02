import {
  ClipboardCheck,
  GraduationCap,
  Inbox,
  PackageSearch,
  Route,
  ShieldCheck,
  Users,
  Warehouse,
  Wrench,
  type LucideIcon,
} from 'lucide-react'

export type SuiteProductCatalogEntry = {
  productKey: string
  displayName: string
  description?: string
  icon: LucideIcon
  sortOrder: number
}

export const SUITE_PRODUCT_CATALOG: SuiteProductCatalogEntry[] = [
  {
    productKey: 'nexarr',
    displayName: 'NexArr',
    description: 'Suite dashboard and control plane',
    icon: ShieldCheck,
    sortOrder: 0,
  },
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    description: 'People, org, and readiness',
    icon: Users,
    sortOrder: 10,
  },
  {
    productKey: 'trainarr',
    displayName: 'TrainArr',
    description: 'Training and qualifications',
    icon: GraduationCap,
    sortOrder: 20,
  },
  {
    productKey: 'maintainarr',
    displayName: 'MaintainArr',
    description: 'Assets and maintenance',
    icon: Wrench,
    sortOrder: 30,
  },
  {
    productKey: 'routarr',
    displayName: 'RoutArr',
    description: 'Routes and dispatch',
    icon: Route,
    sortOrder: 40,
  },
  {
    productKey: 'supplyarr',
    displayName: 'SupplyArr',
    description: 'Procurement and inventory',
    icon: PackageSearch,
    sortOrder: 50,
  },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    description: 'Rules, vocabulary, and references',
    icon: ClipboardCheck,
    sortOrder: 60,
  },
  {
    productKey: 'loadarr',
    displayName: 'LoadArr',
    description: 'Warehouse load execution',
    icon: Warehouse,
    sortOrder: 65,
  },
  {
    productKey: 'companion',
    displayName: 'Companion',
    description: 'Field inbox and mobile tasks',
    icon: Inbox,
    sortOrder: 70,
  },
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
