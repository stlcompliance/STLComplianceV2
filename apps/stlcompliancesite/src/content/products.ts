import type { LucideIcon } from 'lucide-react'
import {
  ClipboardCheck,
  GraduationCap,
  Inbox,
  Landmark,
  PackageSearch,
  Route,
  ShieldCheck,
  ClipboardList,
  FileText,
  Users,
  DatabaseZap,
  Wrench,
  Warehouse,
} from 'lucide-react'
import { normalizeProductKey } from '@stl/shared-ui'

import { productPath } from '../lib/publicRoutes'
import { PRODUCT_OWNERSHIP } from './ownershipBoundaries'

export type ProductCategoryKey =
  | 'platform'
  | 'workforce'
  | 'operations'
  | 'compliance'
  | 'records'
  | 'field'

export type CapabilityKey =
  | 'secureAccess'
  | 'workforce'
  | 'training'
  | 'maintenance'
  | 'dispatch'
  | 'customer'
  | 'orders'
  | 'supply'
  | 'warehouse'
  | 'complianceRules'
  | 'auditEvidence'
  | 'fieldInbox'

export type CapabilityLevel = 'primary' | 'connected' | 'none'

export type MarketingProduct = {
  productKey: string
  displayName: string
  tagline: string
  overview: string
  owns: string
  doesNotOwn: string
  primaryWorkflows: string[]
  recordsManaged: string[]
  readinessChecks: string[]
  evidenceOutputs: string[]
  handoffs: string[]
  checklist: Record<CapabilityKey, CapabilityLevel>
  connectedReasons: Partial<Record<CapabilityKey, string>>
  icon: LucideIcon
  sortOrder: number
  category: ProductCategoryKey
  brandImageSrc: string
  brandAccentClass: string
}

export const PRODUCT_CATEGORY_LABELS: Record<ProductCategoryKey, string> = {
  platform: 'Platform and access',
  workforce: 'Workforce and readiness',
  operations: 'Daily operations',
  compliance: 'Compliance proof',
  records: 'Evidence, records, and reporting',
  field: 'Field work',
}

export const CAPABILITY_LABELS: Record<CapabilityKey, string> = {
  secureAccess: 'Secure access',
  workforce: 'Workforce records',
  training: 'Training and qualifications',
  maintenance: 'Maintenance and inspections',
  dispatch: 'Routes and dispatch',
  customer: 'Customer records',
  orders: 'Orders and requests',
  supply: 'Vendors and purchasing',
  warehouse: 'Warehouse and inventory',
  complianceRules: 'Rules and checks',
  auditEvidence: 'Audit evidence',
  fieldInbox: 'Field inbox',
}

export const CAPABILITY_ORDER: CapabilityKey[] = [
  'secureAccess',
  'workforce',
  'training',
  'maintenance',
  'dispatch',
  'customer',
  'orders',
  'supply',
  'warehouse',
  'complianceRules',
  'auditEvidence',
  'fieldInbox',
]

const noneChecklist: Record<CapabilityKey, CapabilityLevel> = {
  secureAccess: 'none',
  workforce: 'none',
  training: 'none',
  maintenance: 'none',
  dispatch: 'none',
  customer: 'none',
  orders: 'none',
  supply: 'none',
  warehouse: 'none',
  complianceRules: 'none',
  auditEvidence: 'none',
  fieldInbox: 'none',
}

function checklist(
  primary: CapabilityKey[],
  connected: CapabilityKey[] = [],
): Record<CapabilityKey, CapabilityLevel> {
  return {
    ...noneChecklist,
    ...Object.fromEntries(connected.map((key) => [key, 'connected'])),
    ...Object.fromEntries(primary.map((key) => [key, 'primary'])),
  } as Record<CapabilityKey, CapabilityLevel>
}

function reasons(entries: Partial<Record<CapabilityKey, string>>): Partial<Record<CapabilityKey, string>> {
  return entries
}

const nonMarketingProductKeys = new Set(['nexarr'])

const allMarketingProducts: MarketingProduct[] = [
  {
    productKey: 'nexarr',
    displayName: 'NexArr',
    tagline: 'The secure front door for one-suite access and entitlement.',
    overview:
      'NexArr is the secure entry point for STL Compliance. It handles tenant setup, identities, and entitlement so teams are in the right place from the first click.',
    owns: PRODUCT_OWNERSHIP.nexarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.nexarr.doesNotOwn,
    primaryWorkflows: [
      'Owns tenant login, platform identity, and access launch.',
      'Controls which products a tenant can use and how teams are onboarded.',
      'Publishes security and entitlement context used by all suite products.',
    ],
    recordsManaged: [
      'Tenants',
      'People access',
      'Membership',
      'Product entitlements',
      'Service tokens',
      'Platform sessions',
    ],
    readinessChecks: [
      'Checks secure login and entitlement before product launch.',
      'Ensures platform-level authorization is valid before operations begin.',
      'Supports break-glass and secure admin pathways where required.',
    ],
    evidenceOutputs: [
      'Access audit events',
      'Session launch records',
      'Product entitlement snapshots',
      'Platform admin changes',
    ],
    handoffs: [
      'Hands users into products only after identity and entitlement checks pass.',
      'Provides authentication context used by operational workflows.',
      'Keeps access rules separate from product execution records.',
    ],
    checklist: checklist(['secureAccess'], ['workforce', 'training', 'maintenance', 'dispatch', 'supply', 'warehouse', 'complianceRules', 'auditEvidence']),
    connectedReasons: reasons({
      secureAccess: 'one suite login',
      workforce: 'shares user identity context',
      complianceRules: 'enables controlled rule access',
      auditEvidence: 'records access activity',
    }),
    icon: ShieldCheck,
    sortOrder: 5,
    category: 'platform',
    brandImageSrc: '/brand/stl-fullcolor.png',
    brandAccentClass: 'from-sky-500/20 to-cyan-400/10',
  },
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    tagline: 'People, sites, roles, permissions, incidents, and readiness context in one place.',
    overview:
      'StaffArr is the workforce system of record for frontline operations. It keeps people, sites, departments, roles, permissions, readiness signals, incidents, and personnel history connected to the operational work those people are allowed to perform.',
    owns: PRODUCT_OWNERSHIP.staffarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.staffarr.doesNotOwn,
    primaryWorkflows: [
      'Maintain employee profiles, site assignments, departments, roles, and permissions.',
      'Track readiness status, personnel history, and workforce incidents.',
      'Use TrainArr qualification publication, incident context, and operational requirements to help supervisors understand who is eligible for work.',
    ],
    recordsManaged: [
      'People',
      'Sites',
      'Departments',
      'Roles and permissions',
      'Incidents',
      'Readiness history',
    ],
    readinessChecks: [
      'Shows whether required certifications or qualifications are current.',
      'Connects TrainArr qualification proof back to people and readiness.',
      'Supports workforce eligibility decisions before assignment, dispatch, or operational handoff.',
    ],
    evidenceOutputs: [
      'Personnel history',
      'Incident records',
      'Readiness snapshots',
      'People export and audit package inputs',
    ],
    handoffs: [
      'Receives training and qualification proof from TrainArr.',
      'Provides person, role, site, and readiness context to operational products.',
      'Feeds people-related evidence into audit and reporting workflows.',
    ],
    checklist: checklist(['workforce', 'auditEvidence'], ['secureAccess', 'training', 'complianceRules']),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      training: 'receives qualification proof',
      complianceRules: 'feeds people facts',
    }),
    icon: Users,
    sortOrder: 10,
    category: 'workforce',
    brandImageSrc: '/brand/staffarr-fullcolor.png',
    brandAccentClass: 'from-purple-500/20 to-violet-400/10',
  },
  {
    productKey: 'trainarr',
    displayName: 'TrainArr',
    tagline: 'Training, signoffs, evaluations, certificates, and qualification proof.',
    overview:
      'TrainArr manages training programs from publication through assignment, evidence capture, evaluation, signoff, completion, recertification, and qualification proof. It turns learning activity into operational readiness signals instead of leaving training as a disconnected LMS record.',
    owns: PRODUCT_OWNERSHIP.trainarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.trainarr.doesNotOwn,
    primaryWorkflows: [
      'Create programs, versions, steps, completion rules, and assignment requirements.',
      'Assign training, collect evidence, run evaluations, capture acknowledgements, and record signoffs.',
      'Issue certificates, calculate qualifications, schedule recertification, and preserve training history.',
    ],
    recordsManaged: [
      'Training programs',
      'Program versions',
      'Assignments',
      'Steps',
      'Evidence',
      'Evaluations',
      'Signoffs',
      'Certificates',
      'Qualifications',
    ],
    readinessChecks: [
      'Requires completion rules, evaluations, acknowledgements, and signoffs before training closes.',
      'Calculates whether a person holds the qualification required for work.',
      'Supports retraining and recertification when qualification proof expires or rules change.',
    ],
    evidenceOutputs: [
      'Assignment history',
      'Captured evidence',
      'Evaluation results',
      'Signoff timeline',
      'Certificates',
      'Qualification records',
      'Training audit packages',
    ],
    handoffs: [
      'Sends qualification proof and training blockers to StaffArr.',
      'Publishes training-related requirements and evidence into Compliance Core context.',
      'Creates material or remediation demand signals for connected operations when training reveals a need.',
    ],
    checklist: checklist(['training', 'auditEvidence'], ['secureAccess', 'workforce', 'complianceRules', 'fieldInbox']),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      workforce: 'assigns real people',
      complianceRules: 'maps training to rules',
      fieldInbox: 'sends worker tasks',
    }),
    icon: GraduationCap,
    sortOrder: 20,
    category: 'workforce',
    brandImageSrc: '/brand/trainarr-fullcolor.png',
    brandAccentClass: 'from-orange-500/20 to-amber-400/10',
  },
  {
    productKey: 'maintainarr',
    displayName: 'MaintainArr',
    tagline: 'Assets, inspections, defects, work orders, repairs, and readiness.',
    overview:
      'MaintainArr controls the asset side of readiness. It tracks equipment, inspections, defects, preventive maintenance, work orders, repairs, labor notes, parts usage, and asset release so operations can see whether equipment is ready before work starts.',
    owns: PRODUCT_OWNERSHIP.maintainarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.maintainarr.doesNotOwn,
    primaryWorkflows: [
      'Manage assets, inspection programs, defect capture, work orders, repair notes, and preventive maintenance.',
      'Track asset condition, repair activity, labor notes, part usage, and readiness decisions.',
      'Connect maintenance outcomes to dispatch, supply, compliance, and audit records.',
    ],
    recordsManaged: [
      'Assets',
      'Inspections',
      'Defects',
      'Work orders',
      'Repairs',
      'Labor notes',
      'Part usage',
      'Preventive maintenance schedules',
    ],
    readinessChecks: [
      'Checks technician qualifications for job types that require specific training or certification.',
      'Blocks or flags asset use when inspections, defects, or repairs make equipment unready.',
      'Connects part usage and supply availability to maintenance execution.',
      'Preserves inspection and repair proof for compliance review.',
    ],
    evidenceOutputs: [
      'Inspection results',
      'Defect history',
      'Work order history',
      'Repair notes',
      'Parts usage',
      'Asset readiness records',
      'Maintenance audit package inputs',
    ],
    handoffs: [
      'Uses StaffArr and TrainArr readiness when repair, inspection, or work order tasks require qualified people.',
      'Sends part demand to SupplyArr and stock context to LoadArr when maintenance needs materials.',
      'Provides asset readiness context to RoutArr before vehicle or equipment assignment.',
      'Provides inspection and repair evidence to Compliance Core.',
    ],
    checklist: checklist(
      ['maintenance', 'auditEvidence'],
      ['secureAccess', 'workforce', 'training', 'supply', 'warehouse', 'dispatch', 'complianceRules'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      workforce: 'qualified technician',
      training: 'job qualification',
      supply: 'parts demand',
      warehouse: 'stock availability',
      dispatch: 'asset release',
      complianceRules: 'inspection rules',
    }),
    icon: Wrench,
    sortOrder: 30,
    category: 'operations',
    brandImageSrc: '/brand/maintainarr-fullcolor.png',
    brandAccentClass: 'from-emerald-500/20 to-teal-400/10',
  },
  {
    productKey: 'routarr',
    displayName: 'RoutArr',
    tagline: 'Routes, dispatch, drivers, vehicles, trip status, and exceptions.',
    overview:
      'RoutArr manages route and dispatch execution. It keeps trips, drivers, vehicles, stops, status changes, proof capture, inspection status snapshots, exceptions, and dispatch history tied to workforce and asset readiness so teams can see whether a trip should proceed.',
    owns: PRODUCT_OWNERSHIP.routarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.routarr.doesNotOwn,
    primaryWorkflows: [
      'Create trips and routes, assign drivers and vehicles, and manage dispatch status.',
      'Track stops, trip progress, exceptions, driver portal actions, and route history.',
      'Check driver, vehicle, inspection, and compliance signals before or during dispatch.',
    ],
    recordsManaged: [
      'Routes',
      'Trips',
      'Stops',
      'Drivers',
      'Vehicles',
      'Assignments',
      'Trip status events',
      'Dispatch exceptions',
      'Inspection references',
    ],
    readinessChecks: [
      'Checks whether the assigned driver, vehicle, load, inventory state, and route conditions are fit for the trip.',
      'Supports trip completion controls tied to inspection, proof, or delivery requirements.',
      'Connects route execution to compliance gates and notification events.',
    ],
    evidenceOutputs: [
      'Trip history',
      'Dispatch status timeline',
      'Inspection status snapshots',
      'Delivery or route evidence',
      'Exception history',
      'Dispatch notification records',
    ],
    handoffs: [
      'Uses StaffArr and TrainArr readiness before driver assignment.',
      'Uses MaintainArr asset readiness before vehicle assignment.',
      'Uses SupplyArr and LoadArr context for demand, shipment, load, stock, and inventory status.',
      'Publishes trip and dispatch evidence to Compliance Core.',
    ],
    checklist: checklist(
      ['dispatch', 'auditEvidence'],
      ['secureAccess', 'workforce', 'training', 'maintenance', 'supply', 'warehouse', 'complianceRules', 'fieldInbox'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      workforce: 'driver assignment',
      training: 'driver qualification',
      maintenance: 'vehicle readiness',
      supply: 'shipment demand',
      warehouse: 'load/stock status',
      complianceRules: 'dispatch gates',
      fieldInbox: 'driver tasks',
    }),
    icon: Route,
    sortOrder: 40,
    category: 'operations',
    brandImageSrc: '/brand/routarr-fullcolor.png',
    brandAccentClass: 'from-blue-500/20 to-cyan-400/10',
  },
  {
    productKey: 'supplyarr',
    displayName: 'SupplyArr',
    tagline: 'Vendors, suppliers, items, purchasing, approvals, and procurement records.',
    overview:
      'SupplyArr manages vendors, suppliers, parts, purchasing, approvals, pricing snapshots, lead times, vendor restrictions, procurement exceptions, and procurement readiness. It makes purchasing and supplier evidence visible to the work that depends on it while LoadArr owns warehouse receiving and stock execution.',
    owns: PRODUCT_OWNERSHIP.supplyarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.supplyarr.doesNotOwn,
    primaryWorkflows: [
      'Maintain vendors, suppliers, parts, supply contracts, purchase requests, purchase orders, and approvals.',
      'Track price snapshots, lead times, supplier incidents, restrictions, warranty claims, and procurement exceptions.',
      'Coordinate demand from maintenance, routes, training, staff, or warehouse operations and hand warehouse execution to LoadArr.',
    ],
    recordsManaged: [
      'Vendors',
      'Suppliers',
      'Parts',
      'Purchase requests',
      'Purchase orders',
      'Approvals',
      'Contracts',
      'Supplier incidents',
      'Warranty claims',
      'Vendor restrictions',
    ],
    readinessChecks: [
      'Checks whether parts, vendors, approvals, or purchasing exceptions affect operational readiness.',
      'Tracks restricted vendors and supplier incidents before procurement decisions.',
      'Publishes commercial and procurement context so LoadArr can execute receiving and stock movement.',
    ],
    evidenceOutputs: [
      'Approval history',
      'Vendor and supplier records',
      'Pricing and lead-time snapshots',
      'Procurement exception history',
      'Warranty and return records',
    ],
    handoffs: [
      'Receives demand from MaintainArr, RoutArr, TrainArr, StaffArr, and LoadArr.',
      'Provides vendor, item, availability snapshot, and procurement evidence back to operational workflows.',
      'Hands physical receiving, stock movement, reservations, and warehouse execution to LoadArr.',
      'Feeds vendor, purchasing, and receiving evidence into Compliance Core.',
    ],
    checklist: checklist(
      ['supply', 'auditEvidence'],
      ['secureAccess', 'workforce', 'maintenance', 'dispatch', 'warehouse', 'complianceRules'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      workforce: 'approval authority',
      maintenance: 'parts demand',
      dispatch: 'route demand',
      warehouse: 'receiving/stock',
      complianceRules: 'vendor rules',
    }),
    icon: PackageSearch,
    sortOrder: 50,
    category: 'operations',
    brandImageSrc: '/brand/supplyarr-fullcolor.png',
    brandAccentClass: 'from-green-500/20 to-lime-400/10',
  },
  {
    productKey: 'customarr',
    displayName: 'CustomArr',
    tagline: 'Customer accounts, contacts, locations, requirements, and onboarding context.',
    overview:
      'CustomArr owns customer relationship truth for operations: accounts, contacts, locations, onboarding status, preferences, access requirements, restrictions, and customer-specific service expectations. It keeps customer context separate from order lifecycle, dispatch execution, and accounting systems.',
    owns: PRODUCT_OWNERSHIP.customarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.customarr.doesNotOwn,
    primaryWorkflows: [
      'Create and maintain customer accounts, contacts, locations, hierarchy, notes, and preferences.',
      'Track customer onboarding, account status, service eligibility, access requirements, and contact authorization.',
      'Publish customer context to orders, routes, quality cases, reports, and compliance workflows without owning their execution.',
    ],
    recordsManaged: [
      'Customer accounts',
      'Customer locations',
      'Customer contacts',
      'Authorized contact records',
      'Customer onboarding',
      'Customer requirements',
      'Portal relationship context',
      'Customer risk and hold status',
    ],
    readinessChecks: [
      'Separates customer lifecycle status from service eligibility before work is accepted.',
      'Shows whether a location, contact, or customer-specific access requirement needs review.',
      'Keeps enforceable requirements distinct from informational access requirements.',
    ],
    evidenceOutputs: [
      'Customer onboarding history',
      'Contact authorization history',
      'Requirement and restriction snapshots',
      'Customer communication trail',
    ],
    handoffs: [
      'Provides customer account and location context to OrdArr before order or request orchestration.',
      'Provides customer snapshots to RoutArr, AssurArr, ReportArr, and Compliance Core when they need customer context.',
      'Keeps sales leads and accounting execution outside CustomArr unless a future owned CRM or finance integration is introduced.',
    ],
    checklist: checklist(
      ['customer'],
      ['secureAccess', 'orders', 'dispatch', 'supply', 'complianceRules', 'auditEvidence'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses portal identity context',
      orders: 'feeds order/customer context',
      dispatch: 'location and contact snapshots',
      complianceRules: 'customer requirements',
      auditEvidence: 'authorization history',
    }),
    icon: Users,
    sortOrder: 55,
    category: 'operations',
    brandImageSrc: '/brand/stl-fullcolor.png',
    brandAccentClass: 'from-cyan-500/20 to-sky-400/10',
  },
  {
    productKey: 'ordarr',
    displayName: 'OrdArr',
    tagline: 'Order and request orchestration across products without taking over execution truth.',
    overview:
      'OrdArr coordinates what a customer or internal operation requested, which products need to act, and when the work is complete enough for financial handoff. It owns the parent order/request lifecycle and packets, while execution products own the work records they perform.',
    owns: PRODUCT_OWNERSHIP.ordarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.ordarr.doesNotOwn,
    primaryWorkflows: [
      'Create parent orders and requests that explain why product work is happening.',
      'Coordinate handoffs to execution products and track lifecycle state without copying their records.',
      'Prepare completion, invoice-ready, and bill-ready packets for downstream finance systems.',
    ],
    recordsManaged: [
      'Orders',
      'Requests',
      'Order lifecycle status',
      'Product handoffs',
      'Completion packets',
      'Invoice-ready packets',
      'Bill-ready packets',
      'Order audit events',
    ],
    readinessChecks: [
      'Checks whether required customer context, product handoffs, and completion signals are present.',
      'Tracks which execution product owns each open piece of work.',
      'Prevents financial handoff until required completion evidence has been assembled.',
    ],
    evidenceOutputs: [
      'Order status timeline',
      'Handoff timeline',
      'Completion packet summary',
      'Invoice-ready packet summary',
      'Bill-ready packet summary',
    ],
    handoffs: [
      'Consumes customer truth from CustomArr.',
      'Hands execution work to MaintainArr, RoutArr, SupplyArr, LoadArr, AssurArr, and other product owners as needed.',
      'Uses RecordArr file links and Compliance Core evidence meaning without becoming their source of truth.',
    ],
    checklist: checklist(
      ['orders'],
      ['secureAccess', 'customer', 'maintenance', 'dispatch', 'supply', 'warehouse', 'complianceRules', 'auditEvidence'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      customer: 'requires customer context',
      maintenance: 'coordinates work orders',
      dispatch: 'coordinates trips',
      supply: 'coordinates procurement needs',
      warehouse: 'coordinates fulfillment state',
      complianceRules: 'completion requirements',
      auditEvidence: 'packet proof',
    }),
    icon: ClipboardList,
    sortOrder: 57,
    category: 'operations',
    brandImageSrc: '/brand/stl-fullcolor.png',
    brandAccentClass: 'from-fuchsia-500/20 to-rose-400/10',
  },
  {
    productKey: 'loadarr',
    displayName: 'LoadArr',
    tagline: 'Warehouse operations, receiving, stock movement, picking, counts, and inventory proof.',
    overview:
      'LoadArr manages warehouse execution and inventory proof. It covers locations, receiving, stock movement, reservations, picking, shipments, adjustments, cycle counts, and inventory history so warehouse work is traceable and connected to supply and dispatch needs.',
    owns: PRODUCT_OWNERSHIP.loadarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.loadarr.doesNotOwn,
    primaryWorkflows: [
      'Manage warehouse locations, inventory movement, receiving, picking, reservations, shipments, and adjustments.',
      'Run cycle counts and preserve inventory history for operational and audit review.',
      'Connect warehouse availability to purchasing, route, maintenance, and shipment work.',
    ],
    recordsManaged: [
      'Warehouse locations',
      'Stock balances',
      'Reservations',
      'Receiving events',
      'Picks',
      'Shipments',
      'Adjustments',
      'Cycle counts',
      'Inventory history',
    ],
    readinessChecks: [
      'Checks whether stock is available, reserved, picked, received, or adjusted.',
      'Preserves count and movement history so inventory proof does not live only in spreadsheets.',
      'Connects warehouse movement to upstream purchasing and downstream route or maintenance work.',
    ],
    evidenceOutputs: [
      'Stock movement history',
      'Receiving proof',
      'Pick and shipment records',
      'Adjustment history',
      'Cycle count records',
      'Inventory audit inputs',
    ],
    handoffs: [
      'Uses SupplyArr purchasing and receiving context.',
      'Provides stock, reservation, pick, load, and shipment context to RoutArr and MaintainArr.',
      'Feeds inventory evidence into Compliance Core when rules or audits require it.',
    ],
    checklist: checklist(
      ['warehouse', 'auditEvidence'],
      ['secureAccess', 'supply', 'dispatch', 'maintenance', 'complianceRules'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      supply: 'receiving/PO context',
      dispatch: 'load/ship status',
      maintenance: 'parts stock',
      complianceRules: 'inventory proof',
    }),
    icon: Warehouse,
    sortOrder: 60,
    category: 'operations',
    brandImageSrc: '/brand/loadarr-fullcolor.png',
    brandAccentClass: 'from-cyan-500/20 to-blue-400/10',
  },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    tagline: 'Rules, evidence expectations, citations, and compliance checks.',
    overview:
      'Compliance Core is the rules and evidence layer for the suite. It manages rule packs, citations, vocabulary, evidence expectations, findings, checks, and audit packaging so compliance proof is tied to the work that created it.',
    owns: PRODUCT_OWNERSHIP.compliancecore.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.compliancecore.doesNotOwn,
    primaryWorkflows: [
      'Maintain regulatory requirements, citations, rule packs, evidence expectations, and approved wording.',
      'Evaluate operational facts from product workflows against compliance rules.',
      'Prepare findings, evidence views, and audit package material across connected products.',
    ],
    recordsManaged: [
      'Rule packs',
      'Citations',
      'Evidence expectations',
      'Compliance checks',
      'Findings',
      'Vocabulary and aliases',
      'Audit package jobs',
    ],
    readinessChecks: [
      'Checks whether product facts satisfy rule and evidence expectations.',
      'Connects findings to the product records that need correction or proof.',
      'Supports compliance gates before operational work proceeds.',
    ],
    evidenceOutputs: [
      'Rule evaluation results',
      'Finding history',
      'Evidence maps',
      'Citation references',
      'Audit package exports',
      'Delivery orchestration records',
    ],
    handoffs: [
      'Receives facts and evidence from StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, and LoadArr.',
      'Sends rule context, gates, and findings back into product workflows.',
      'Supports audit package generation for compliance review.',
    ],
    checklist: checklist(
      ['complianceRules', 'auditEvidence'],
      ['secureAccess', 'workforce', 'training', 'maintenance', 'dispatch', 'supply', 'warehouse'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      workforce: 'people facts',
      training: 'qualification proof',
      maintenance: 'inspection facts',
      dispatch: 'trip facts',
      supply: 'vendor facts',
      warehouse: 'inventory facts',
    }),
    icon: ClipboardCheck,
    sortOrder: 70,
    category: 'compliance',
    brandImageSrc: '/brand/compliancecore-fullcolor.png',
    brandAccentClass: 'from-indigo-500/20 to-sky-400/10',
  },
  {
    productKey: 'fieldcompanion',
    displayName: 'Field Companion',
    tagline: 'A focused field inbox for tasks, messages, and quick handoffs.',
    overview:
      'Field Companion gives frontline workers a focused inbox for tasks, messages, and quick handoffs into the product where action belongs. It is not the system of record for every workflow; it is the practical field surface that helps people find and complete the right work faster.',
    owns: PRODUCT_OWNERSHIP.fieldcompanion.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.fieldcompanion.doesNotOwn,
    primaryWorkflows: [
      'Show field tasks and messages in one focused inbox.',
      'Route workers into the product workspace that owns the underlying record.',
      'Keep quick handoffs visible without duplicating operational systems of record.',
    ],
    recordsManaged: ['Inbox items', 'Task handoffs', 'Message context', 'Product launch targets'],
    readinessChecks: [
      'Surfaces tasks that need worker attention from connected products.',
      'Keeps field action tied to the product record that owns the work.',
      'Reduces missed handoffs between office workflows and field execution.',
    ],
    evidenceOutputs: [
      'Task acknowledgement context',
      'Message and handoff history',
      'Navigation trail into product records',
    ],
    handoffs: [
      'Routes users into TrainArr, MaintainArr, RoutArr, SupplyArr, LoadArr, StaffArr, or Compliance Core as needed.',
      'Returns field attention to the workflow owner instead of copying the record.',
    ],
    checklist: checklist(
      ['fieldInbox'],
      ['secureAccess', 'training', 'maintenance', 'dispatch', 'supply', 'warehouse', 'auditEvidence'],
    ),
    connectedReasons: reasons({
      secureAccess: 'uses suite identity',
      training: 'training tasks',
      maintenance: 'work order tasks',
      dispatch: 'driver tasks',
      supply: 'field requests',
      warehouse: 'warehouse tasks',
      auditEvidence: 'task proof',
    }),
    icon: Inbox,
    sortOrder: 80,
    category: 'field',
    brandImageSrc: '/brand/fieldcompanion-icon.svg',
    brandAccentClass: 'from-slate-500/20 to-teal-400/10',
  },
  {
    productKey: 'recordarr',
    displayName: 'RecordArr',
    tagline: 'The document and retention layer that connects proof to work.',
    overview:
      'RecordArr stores records your teams need to prove their work happened and keeps document versions, access, and retention organized so support and evidence retrieval stay reliable.',
    owns: PRODUCT_OWNERSHIP.recordarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.recordarr.doesNotOwn,
    primaryWorkflows: [
      'Stores policies, SOPs, certificates, and operational documents.',
      'Manages retention rules, version control, and legal hold status.',
      'Supports secure access history and document package handoff.',
    ],
    recordsManaged: [
      'Controlled documents',
      'Record versions',
      'SOP and policy files',
      'Evidence packages',
      'Retention schedules',
      'Legal holds',
    ],
    readinessChecks: [
      'Tracks document status and version before work evidence moves forward.',
      'Keeps retention and evidence timing visible across operations.',
      'Records document access and acknowledgment history.',
    ],
    evidenceOutputs: [
      'Evidence file libraries',
      'Record packages',
      'Retention and hold logs',
      'Controlled document audit trail',
    ],
    handoffs: [
      'Stores proof from all products for review and audit readiness.',
      'Supports Compliance Core with evidence references.',
      'Provides reports and legal evidence attachments to customers and operations teams.',
    ],
    checklist: checklist(['auditEvidence'], ['complianceRules', 'secureAccess', 'training', 'maintenance', 'dispatch', 'supply', 'warehouse']),
    connectedReasons: reasons({
      auditEvidence: 'captures documents and proof',
      complianceRules: 'supports evidence mapping',
      secureAccess: 'controls document access',
    }),
    icon: FileText,
    sortOrder: 85,
    category: 'records',
    brandImageSrc: '/brand/stl-fullcolor.png',
    brandAccentClass: 'from-emerald-500/20 to-green-400/10',
  },
  {
    productKey: 'reportarr',
    displayName: 'ReportArr',
    tagline: 'Cross-suite reporting and dashboarding for people, operations, and readiness.',
    overview:
      'ReportArr connects operational signals into plain summaries and recurring reports so leadership sees what is ready, what is behind, and where attention is needed.',
    owns: PRODUCT_OWNERSHIP.reportarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.reportarr.doesNotOwn,
    primaryWorkflows: [
      'Builds cross-product dashboards and scheduled reporting.',
      'Tracks KPI snapshots and exportable summaries.',
      'Provides repeatable reporting views for operations and compliance meetings.',
    ],
    recordsManaged: [
      'Report definitions',
      'KPI models',
      'Scheduled runs',
      'Dashboard filters',
      'Execution history',
      'Report outputs',
    ],
    readinessChecks: [
      'Summarizes readiness outcomes from connected products.',
      'Shows missed follow-up signals and delayed work statuses.',
      'Highlights evidence and ownership state for decision meetings.',
    ],
    evidenceOutputs: [
      'Cross-suite readiness reports',
      'Compliance and audit status summaries',
      'Export packs for leadership reviews',
      'Trend snapshots',
    ],
    handoffs: [
      'Consumes operational events and presents them in digestible views.',
      'Supports teams in deciding where to assign follow-up.',
      'Keeps evidence visibility closer to the operating team.',
    ],
    checklist: checklist(['auditEvidence'], ['complianceRules', 'workforce', 'training', 'maintenance', 'dispatch', 'supply', 'warehouse']),
    connectedReasons: reasons({
      complianceRules: 'adds rule context',
      auditEvidence: 'turns evidence into executive visibility',
      workforce: 'shows people readiness trends',
      maintenance: 'highlights equipment risk',
    }),
    icon: DatabaseZap,
    sortOrder: 90,
    category: 'records',
    brandImageSrc: '/brand/stl-fullcolor.png',
    brandAccentClass: 'from-cyan-500/20 to-teal-400/10',
  },
  {
    productKey: 'assurarr',
    displayName: 'AssurArr',
    tagline: 'Nonconformance, quality, CAPA, and release controls for operations.',
    overview:
      'AssurArr helps teams handle exceptions with practical assurance workflows: case creation, action plans, holds, and evidence of what was fixed and verified.',
    owns: PRODUCT_OWNERSHIP.assurarr.owns,
    doesNotOwn: PRODUCT_OWNERSHIP.assurarr.doesNotOwn,
    primaryWorkflows: [
      'Tracks nonconformance reports, findings, and corrective actions.',
      'Supports investigation, CAPA plan creation, and effectiveness checks.',
      'Manages holds and release decisions with accountability.',
    ],
    recordsManaged: [
      'Nonconformance reports',
      'CAPA cases',
      'Corrective actions',
      'Findings and holds',
      'Escalation records',
      'Supplier and customer deviations',
    ],
    readinessChecks: [
      'Connects root cause and containment to open work.',
      'Tracks whether corrective actions are complete and verified.',
      'Flags operational impacts before work or release resumes.',
    ],
    evidenceOutputs: [
      'Assurance case timelines',
      'CAPA evidence packages',
      'Release and hold records',
      'Escalation summaries',
    ],
    handoffs: [
      'Works with originating product for direct corrective execution.',
      'Escalates training or personnel follow-up when needed.',
      'Uses RecordArr for evidence attachments and audit traceability.',
    ],
    checklist: checklist(['auditEvidence'], ['training', 'maintenance', 'dispatch', 'supply', 'warehouse', 'complianceRules']),
    connectedReasons: reasons({
      training: 'triggers qualification retraining when needed',
      complianceRules: 'links incidents to rule outcomes',
      maintenance: 'coordinates corrective execution',
      dispatch: 'manages release/hold impacts',
    }),
    icon: Landmark,
    sortOrder: 95,
    category: 'compliance',
    brandImageSrc: '/brand/stl-fullcolor.png',
    brandAccentClass: 'from-amber-500/20 to-orange-400/10',
  },
]

export const MARKETING_PRODUCTS: MarketingProduct[] = allMarketingProducts.filter(
  (product) => !nonMarketingProductKeys.has(product.productKey),
)

export function getMarketingProduct(productKey: string): MarketingProduct | undefined {
  const normalized = normalizeProductKey(productKey)
  return MARKETING_PRODUCTS.find((p) => p.productKey === normalized)
}

export function productPagePath(productKey: string): string {
  return productPath(normalizeProductKey(productKey))
}

export function productsByCategory(category: ProductCategoryKey): MarketingProduct[] {
  return MARKETING_PRODUCTS.filter((p) => p.category === category).sort(
    (a, b) => a.sortOrder - b.sortOrder,
  )
}

export const PRODUCT_CATEGORY_ORDER: ProductCategoryKey[] = [
  'platform',
  'workforce',
  'operations',
  'compliance',
  'records',
  'field',
]
