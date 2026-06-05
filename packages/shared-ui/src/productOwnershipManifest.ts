export type ProductOwnershipManifestEntry = {
  productKey: string
  displayName: string
  routeSlug: string
  legacyProductKeys?: readonly string[]
  sortOrder: number
  catalogDescription: string
  owns: string
  doesNotOwn: string
}

export const IMPLEMENTED_PRODUCT_OWNERSHIP: readonly ProductOwnershipManifestEntry[] = [
  {
    productKey: 'nexarr',
    displayName: 'NexArr',
    routeSlug: 'nexarr',
    sortOrder: 0,
    catalogDescription: 'Suite access, entitlement, and launch control',
    owns:
      'Platform login, tenant identity, tenant membership, product entitlement, product launch, service tokens, and platform access audit events.',
    doesNotOwn:
      'Product-specific permissions, personnel records, customer records, vendor records, inventory, dispatch, maintenance, reports, or financial execution.',
  },
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    routeSlug: 'staffarr',
    sortOrder: 10,
    catalogDescription: 'People, org, locations, and readiness projections',
    owns:
      'People, workers, org units, internal sites, operational locations, permission assignments, role assignments, work authority context, person status, delegation, and personnel history.',
    doesNotOwn:
      'Training programs, certification definitions, certification issuance, maintenance work, dispatch work, inventory work, payroll execution, or accounting.',
  },
  {
    productKey: 'trainarr',
    displayName: 'TrainArr',
    routeSlug: 'trainarr',
    sortOrder: 20,
    catalogDescription: 'Training, certifications, and qualification lifecycle',
    owns:
      'Training programs, modules, assignments, signoffs, practical evaluations, remediation, qualification rules, certification issuance, renewal, expiration, revocation, and qualification publication to StaffArr.',
    doesNotOwn:
      'Person master records, job or position master records, incident master records, maintenance work, dispatch work, inventory work, payroll, or accounting.',
  },
  {
    productKey: 'maintainarr',
    displayName: 'MaintainArr',
    routeSlug: 'maintainarr',
    sortOrder: 30,
    catalogDescription: 'Assets, inspections, defects, and maintenance execution',
    owns:
      'Asset master records, asset condition, inspections, defects, work orders, repairs, maintenance labor capture, downtime, maintenance evidence, warranty tracking, recall tracking, parts demand requests, and maintenance compliance records.',
    doesNotOwn:
      'Inventory stock ledger, warehouse bins as inventory truth, vendor master, purchase approvals, staff location identity, dispatch execution, customer relationship, or the financial asset ledger.',
  },
  {
    productKey: 'routarr',
    displayName: 'RoutArr',
    routeSlug: 'routarr',
    sortOrder: 40,
    catalogDescription: 'Dispatch, trips, proof, and transportation execution',
    owns:
      'Routes, trips, dispatch plans, driver assignments, vehicle assignments, stop sequence, pickup execution, delivery execution, trip status, ETA and status updates, transportation exceptions, proof of pickup, proof of delivery, and dispatch audit trail.',
    doesNotOwn:
      'Driver person master records, driver certifications, vehicle maintenance truth, inventory truth, warehouse receiving truth, customer master records, financial freight billing, ELD hardware, or telematics hardware.',
  },
  {
    productKey: 'supplyarr',
    displayName: 'SupplyArr',
    routeSlug: 'supplyarr',
    sortOrder: 50,
    catalogDescription: 'Vendors, suppliers, items, and procurement context',
    owns:
      'Vendor master records, supplier master records, supplier contacts, supplier documents, item, part, and material master data, preferred suppliers, price snapshots, lead-time snapshots, purchase requests, RFQs, operational purchasing approvals, purchase intent, procurement status, and external vendor mappings.',
    doesNotOwn:
      'Inventory balances, stock ledger, warehouse movement, receiving execution, payment execution, accounts payable, tax, banking, or the general ledger.',
  },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    routeSlug: 'compliancecore',
    sortOrder: 60,
    catalogDescription: 'Rules, evidence requirements, and regulatory intelligence',
    owns:
      'Governing body catalogs, rulepacks, regulations, applicability logic, evidence requirements, exemptions, exceptions, compliance interpretations, audit package logic, evidence classification, compliance gap analysis, and rule-to-product mapping.',
    doesNotOwn:
      'Operational execution, stored document files, training execution, inventory execution, dispatch execution, maintenance execution, vendor relationships, customer relationships, accounting, or legal advice as a substitute for counsel.',
  },
  {
    productKey: 'loadarr',
    displayName: 'LoadArr',
    routeSlug: 'loadarr',
    sortOrder: 65,
    catalogDescription: 'Warehouse receiving, inventory, and stock movement',
    owns:
      'Expected receipts, receiving workflow, dock receiving queue, putaway, inventory balances, stock ledger, warehouse tasks, reservations, picks, issues, returns, cycle counts, inventory adjustments, inventory holds, quarantine status, lot tracking, serial tracking, bin stock, fulfillment status, and inventory availability APIs.',
    doesNotOwn:
      'StaffArr location identity, vendor master, item commercial ownership, purchase approvals, customer master, dispatch execution, maintenance work orders, the financial inventory valuation ledger, or dedicated scanner and hardware ownership.',
  },
  {
    productKey: 'recordarr',
    displayName: 'RecordArr',
    routeSlug: 'recordarr',
    sortOrder: 66,
    catalogDescription: 'Documents, records, retention, and controlled files',
    owns:
      'Document storage, record metadata, file versions, controlled documents, policies, SOPs, templates, effective dates, expiration dates, retention schedules, legal holds, document approvals, read-and-acknowledge records, evidence file storage, and document access history.',
    doesNotOwn:
      'Compliance rule interpretation, operational product records, customer master, vendor master, training programs, inventory, dispatch, maintenance, or accounting.',
  },
  {
    productKey: 'reportarr',
    displayName: 'ReportArr',
    routeSlug: 'reportarr',
    sortOrder: 66.5,
    catalogDescription: 'Dashboards, analytics, exports, and report delivery',
    owns:
      'Cross-product dashboards, report definitions, scheduled reports, KPI views, executive summaries, audit readiness dashboards, compliance posture dashboards, inventory health reporting, dispatch performance reporting, vendor performance reporting, export packages, report subscriptions, and snapshot or report history.',
    doesNotOwn:
      'Operational source-of-truth records, source data correction, compliance interpretation, product execution, the financial ledger, or stored documents outside rendered report artifacts.',
  },
  {
    productKey: 'assurarr',
    displayName: 'AssurArr',
    routeSlug: 'assurarr',
    sortOrder: 67,
    catalogDescription: 'Quality, nonconformance, CAPA, and release decisions',
    owns:
      'Nonconformance reports, assurance cases, corrective actions, preventive actions, root cause analysis, containment actions, deviation records, quality holds as business decisions, release approvals, effectiveness checks, recurrence tracking, and CAPA evidence packages.',
    doesNotOwn:
      'Inventory ledger, warehouse physical movement, employee discipline, training execution, maintenance repair execution, regulatory interpretation, customer master, vendor master, or accounting.',
  },
  {
    productKey: 'fieldcompanion',
    displayName: 'Field Companion',
    routeSlug: 'field-companion',
    legacyProductKeys: ['companion'],
    sortOrder: 70,
    catalogDescription: 'Field inbox, capture, and mobile execution surfaces',
    owns:
      'Mobile task inbox, product switching, guided execution screens, photo capture, document capture, signature capture, secure no-login upload flows, offline-capable field actions, inspection execution UI, delivery confirmation UI, incident self-reporting UI, and push or in-app task surfaces.',
    doesNotOwn:
      'Final operational records, ELD replacement, scanner hardware replacement, accounting, source-of-truth product data, or product-specific business rules.',
  },
] as const

export const IMPLEMENTED_PRODUCT_KEYS = IMPLEMENTED_PRODUCT_OWNERSHIP.map(
  (entry) => entry.productKey,
)

export function normalizeProductKey(productKey: string): string {
  const normalized = productKey.trim().toLowerCase().replace(/[-_]/g, '')
  return normalized === 'companion' ? 'fieldcompanion' : normalized
}

export function toLegacyProductKey(productKey: string): string {
  const normalized = normalizeProductKey(productKey)
  return normalized === 'fieldcompanion' ? 'companion' : normalized
}

export function getProductRouteSlug(productKey: string): string {
  const normalized = normalizeProductKey(productKey)
  return (
    IMPLEMENTED_PRODUCT_OWNERSHIP.find((entry) => entry.productKey === normalized)?.routeSlug ??
    normalized
  )
}

export function getProductOwnershipManifestEntry(
  productKey: string,
): ProductOwnershipManifestEntry | undefined {
  const normalized = normalizeProductKey(productKey)
  return IMPLEMENTED_PRODUCT_OWNERSHIP.find((entry) => entry.productKey === normalized)
}
