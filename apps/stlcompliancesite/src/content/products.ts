import type { LucideIcon } from 'lucide-react'
import {
  ClipboardCheck,
  GraduationCap,
  Inbox,
  PackageSearch,
  Route,
  ShieldCheck,
  Users,
  Wrench,
} from 'lucide-react'

import { productPath } from '../lib/publicRoutes'
import { PRODUCT_OWNERSHIP } from './ownershipBoundaries'

export type ProductCategoryKey =
  | 'control-plane'
  | 'workforce'
  | 'operations'
  | 'compliance'
  | 'field'

export type PublicCapabilityMaturity = 'v1-operational' | 'v1-partial'

export type MarketingProduct = {
  productKey: string
  displayName: string
  tagline: string
  owns: string
  doesNotOwn: string
  icon: LucideIcon
  sortOrder: number
  category: ProductCategoryKey
  maturity: PublicCapabilityMaturity
  maturityLabel: string
}

export const PRODUCT_CATEGORY_LABELS: Record<ProductCategoryKey, string> = {
  'control-plane': 'Secure access',
  workforce: 'Workforce and readiness',
  operations: 'Daily operations',
  compliance: 'Compliance proof',
  field: 'Field work',
}

export const MATURITY_LABELS: Record<PublicCapabilityMaturity, string> = {
  'v1-operational': 'Available in V1',
  'v1-partial': 'Early access',
}

export const MATURITY_STACK_LABEL =
  'Core workflows are available now; deeper coverage continues to expand.'

export const MARKETING_PRODUCTS: MarketingProduct[] = [
  {
    productKey: 'nexarr',
    displayName: 'NexArr',
    tagline: 'One secure front door for users, companies, products, and access.',
    owns: PRODUCT_OWNERSHIP.nexarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.nexarr.doesNotOwn,
    icon: ShieldCheck,
    sortOrder: 0,
    category: 'control-plane',
    maturity: 'v1-operational',
    maturityLabel: 'Login, product access, and administration are available now.',
  },
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    tagline: 'People, sites, roles, permissions, incidents, and readiness in one place.',
    owns: PRODUCT_OWNERSHIP.staffarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.staffarr.doesNotOwn,
    icon: Users,
    sortOrder: 10,
    category: 'workforce',
    maturity: 'v1-operational',
    maturityLabel: MATURITY_STACK_LABEL,
  },
  {
    productKey: 'trainarr',
    displayName: 'TrainArr',
    tagline: 'Training, signoffs, evaluations, certificates, and qualification proof.',
    owns: PRODUCT_OWNERSHIP.trainarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.trainarr.doesNotOwn,
    icon: GraduationCap,
    sortOrder: 20,
    category: 'workforce',
    maturity: 'v1-operational',
    maturityLabel: MATURITY_STACK_LABEL,
  },
  {
    productKey: 'maintainarr',
    displayName: 'MaintainArr',
    tagline: 'Assets, inspections, defects, work orders, repairs, and readiness.',
    owns: PRODUCT_OWNERSHIP.maintainarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.maintainarr.doesNotOwn,
    icon: Wrench,
    sortOrder: 30,
    category: 'operations',
    maturity: 'v1-operational',
    maturityLabel: MATURITY_STACK_LABEL,
  },
  {
    productKey: 'routarr',
    displayName: 'RoutArr',
    tagline: 'Routes, dispatch, drivers, vehicles, trip status, and exceptions.',
    owns: PRODUCT_OWNERSHIP.routarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.routarr.doesNotOwn,
    icon: Route,
    sortOrder: 40,
    category: 'operations',
    maturity: 'v1-operational',
    maturityLabel: MATURITY_STACK_LABEL,
  },
  {
    productKey: 'supplyarr',
    displayName: 'SupplyArr',
    tagline: 'Vendors, customers, parts, purchasing, approvals, and supply records.',
    owns: PRODUCT_OWNERSHIP.supplyarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.supplyarr.doesNotOwn,
    icon: PackageSearch,
    sortOrder: 50,
    category: 'operations',
    maturity: 'v1-operational',
    maturityLabel: MATURITY_STACK_LABEL,
  },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    tagline: 'Rules, evidence expectations, citations, and compliance checks.',
    owns: PRODUCT_OWNERSHIP.compliancecore.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.compliancecore.doesNotOwn,
    icon: ClipboardCheck,
    sortOrder: 60,
    category: 'compliance',
    maturity: 'v1-operational',
    maturityLabel: MATURITY_STACK_LABEL,
  },
  {
    productKey: 'companion',
    displayName: 'Companion',
    tagline: 'A focused field inbox for tasks, messages, and quick handoffs.',
    owns: PRODUCT_OWNERSHIP.companion.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.companion.doesNotOwn,
    icon: Inbox,
    sortOrder: 70,
    category: 'field',
    maturity: 'v1-partial',
    maturityLabel: 'Field inbox and product handoffs are available in early form.',
  },
]

export function getMarketingProduct(productKey: string): MarketingProduct | undefined {
  const normalized = productKey.trim().toLowerCase()
  return MARKETING_PRODUCTS.find((p) => p.productKey === normalized)
}

export function productPagePath(productKey: string): string {
  return productPath(productKey)
}

export function productsByCategory(category: ProductCategoryKey): MarketingProduct[] {
  return MARKETING_PRODUCTS.filter((p) => p.category === category).sort(
    (a, b) => a.sortOrder - b.sortOrder,
  )
}

export const PRODUCT_CATEGORY_ORDER: ProductCategoryKey[] = [
  'control-plane',
  'workforce',
  'operations',
  'compliance',
  'field',
]
