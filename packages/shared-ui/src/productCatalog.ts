import {
  Archive,
  BarChart3,
  ClipboardCheck,
  GraduationCap,
  Inbox,
  PackageSearch,
  Route,
  ShieldAlert,
  ShieldCheck,
  Users,
  Warehouse,
  Wrench,
  type LucideIcon,
} from 'lucide-react'
import {
  getProductOwnershipManifestEntry,
  getProductRouteSlug,
  IMPLEMENTED_PRODUCT_OWNERSHIP,
  normalizeProductKey,
  toLegacyProductKey,
} from './productOwnershipManifest'
export {
  getProductRouteSlug,
  normalizeProductKey,
  toLegacyProductKey,
} from './productOwnershipManifest'

export type SuiteProductCatalogEntry = {
  productKey: string
  displayName: string
  description?: string
  icon: LucideIcon
  sortOrder: number
}

const productIcons: Record<string, LucideIcon> = {
  nexarr: ShieldCheck,
  staffarr: Users,
  trainarr: GraduationCap,
  maintainarr: Wrench,
  routarr: Route,
  supplyarr: PackageSearch,
  compliancecore: ClipboardCheck,
  loadarr: Warehouse,
  recordarr: Archive,
  reportarr: BarChart3,
  assurarr: ShieldAlert,
  fieldcompanion: Inbox,
}

export const SUITE_PRODUCT_CATALOG: SuiteProductCatalogEntry[] = IMPLEMENTED_PRODUCT_OWNERSHIP.map(
  (entry) => ({
    productKey: entry.productKey,
    displayName: entry.displayName,
    description: entry.catalogDescription,
    icon: productIcons[entry.productKey] ?? ShieldCheck,
    sortOrder: entry.sortOrder,
  }),
)

export function getSuiteProductIcon(productKey: string): LucideIcon {
  return getProductOwnershipManifestEntry(productKey)
    ? productIcons[normalizeProductKey(productKey)] ?? ShieldCheck
    : ShieldCheck
}

export function getSuiteProductCatalogEntry(
  productKey: string,
): SuiteProductCatalogEntry | undefined {
  const normalized = normalizeProductKey(productKey)
  return SUITE_PRODUCT_CATALOG.find((entry) => entry.productKey === normalized)
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
