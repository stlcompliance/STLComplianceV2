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

  /** Plain-English label shown on the products hub comparison table. */

  maturityLabel: string

}



export const PRODUCT_CATEGORY_LABELS: Record<ProductCategoryKey, string> = {

  'control-plane': 'Control plane',

  workforce: 'Workforce and readiness',

  operations: 'Operations execution',

  compliance: 'Compliance authority',

  field: 'Field surfaces',

}



export const MATURITY_LABELS: Record<PublicCapabilityMaturity, string> = {
  'v1-operational': 'V1 stack operational',
  'v1-partial': 'V1 partial',
}

/** Plain label on product cards — stack vs docs/11 featureset honesty. */
export const MATURITY_STACK_LABEL =
  'API, database, worker, and product UI provisioned (docs/11 featureset tracked in-repo)'



export const MARKETING_PRODUCTS: MarketingProduct[] = [

  {

    productKey: 'nexarr',

    displayName: 'NexArr',

    tagline: 'Control plane for identity, tenants, entitlements, and launch',

    owns: PRODUCT_OWNERSHIP.nexarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.nexarr.doesNotOwn,

    icon: ShieldCheck,

    sortOrder: 0,

    category: 'control-plane',

    maturity: 'v1-operational',

    maturityLabel: 'API, database, worker, and suite UI (NexArr control plane)',

  },

  {

    productKey: 'staffarr',

    displayName: 'StaffArr',

    tagline: 'Workforce backbone — who someone is and whether they are ready',

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

    tagline: 'Training workflow, evidence, and qualification proof',

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

    tagline: 'Assets, inspections, and maintenance execution',

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

    tagline: 'Routes, dispatch, and transportation execution',

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

    tagline: 'Vendors, purchasing, receiving, and inventory',

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

    tagline: 'Controlled vocabulary, keys, mappings, and rule context',

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

    tagline: 'Field inbox and mobile task surfaces',

    owns: PRODUCT_OWNERSHIP.companion.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.companion.doesNotOwn,

    icon: Inbox,

    sortOrder: 70,

    category: 'field',

    maturity: 'v1-partial',

    maturityLabel: 'Field inbox and deep links; authority stays in product APIs',

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


