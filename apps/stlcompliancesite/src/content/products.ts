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

export type MarketingProduct = {
  productKey: string
  displayName: string
  tagline: string
  owns: string
  doesNotOwn: string
  icon: LucideIcon
  sortOrder: number
}

export const MARKETING_PRODUCTS: MarketingProduct[] = [
  {
    productKey: 'nexarr',
    displayName: 'NexArr',
    tagline: 'Control plane for identity, tenants, entitlements, and launch',
    owns: 'Login, tenants, platform identity, product entitlement, licensing, service clients, service tokens, and launch authority.',
    doesNotOwn: 'Operational workforce, training, asset, dispatch, or supply workflows.',
    icon: ShieldCheck,
    sortOrder: 0,
  },
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    tagline: 'Workforce backbone — who someone is and whether they are ready',
    owns: 'People, org structure, permissions, certifications, readiness, incidents, and personnel history.',
    doesNotOwn: 'Training proof, asset maintenance, dispatch execution, or procurement records.',
    icon: Users,
    sortOrder: 10,
  },
  {
    productKey: 'trainarr',
    displayName: 'TrainArr',
    tagline: 'Training workflow, evidence, and qualification proof',
    owns: 'Training workflow, evidence, evaluations, signoffs, completions, retraining, recertification, and training-derived qualifications.',
    doesNotOwn: 'Person identity, org hierarchy, or readiness calculation truth (publishes to StaffArr).',
    icon: GraduationCap,
    sortOrder: 20,
  },
  {
    productKey: 'maintainarr',
    displayName: 'MaintainArr',
    tagline: 'Assets, inspections, and maintenance execution',
    owns: 'Assets, inspections, defects, work orders, preventive maintenance, maintenance history, and asset readiness.',
    doesNotOwn: 'Procurement, vendor catalogs, or parts inventory (SupplyArr).',
    icon: Wrench,
    sortOrder: 30,
  },
  {
    productKey: 'routarr',
    displayName: 'RoutArr',
    tagline: 'Routes, dispatch, and transportation execution',
    owns: 'Routes, trips, dispatch, driver assignment, transportation execution, DVIR surfaces, proof, exceptions, and route history.',
    doesNotOwn: 'Driver identity or qualification truth (StaffArr + TrainArr).',
    icon: Route,
    sortOrder: 40,
  },
  {
    productKey: 'supplyarr',
    displayName: 'SupplyArr',
    tagline: 'Vendors, purchasing, receiving, and inventory',
    owns: 'Vendors, dealers, suppliers, parts catalogs, purchasing, receiving, inventory, pricing snapshots, and lead-time snapshots.',
    doesNotOwn: 'Asset maintenance workflows or work orders (MaintainArr).',
    icon: PackageSearch,
    sortOrder: 50,
  },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    tagline: 'Controlled vocabulary, keys, mappings, and rule context',
    owns: 'Controlled vocabulary, regulatory keys, material keys, mappings, rule packs, SDS/HazCom references, and evaluation patterns.',
    doesNotOwn: 'Operational workflow actions or product-owned facts (products own facts and overrides).',
    icon: ClipboardCheck,
    sortOrder: 60,
  },
  {
    productKey: 'companion',
    displayName: 'Companion',
    tagline: 'Field inbox and mobile task surfaces',
    owns: 'Aggregated field inbox presentation and mobile-oriented task navigation (links into entitled products).',
    doesNotOwn: 'Business authority, tenant data, or product workflow state (each product API remains authoritative).',
    icon: Inbox,
    sortOrder: 70,
  },
]

export function getMarketingProduct(productKey: string): MarketingProduct | undefined {
  const normalized = productKey.trim().toLowerCase()
  return MARKETING_PRODUCTS.find((p) => p.productKey === normalized)
}

export function productPagePath(productKey: string): string {
  return `/products/${productKey.trim().toLowerCase()}`
}
