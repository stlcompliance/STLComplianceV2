/** Canonical product scope copy for public product pages. */

export type ProductOwnershipCopy = {
  owns: string
  doesNotOwn: string
}

export const PRODUCT_OWNERSHIP: Record<string, ProductOwnershipCopy> = {
  staffarr: {
    owns:
      'Employee records, sites, departments, roles, permissions, incidents, certifications, readiness, and personnel history.',
    doesNotOwn:
      'Detailed training courses, maintenance work, dispatch work, purchasing, inventory movement, or rule libraries.',
  },
  trainarr: {
    owns:
      'Training programs, assignments, required steps, tests, evaluations, signoffs, certificates, retraining, and qualification proof.',
    doesNotOwn:
      'Core employee records, maintenance work, dispatch work, purchasing, inventory, or company login.',
  },
  maintainarr: {
    owns:
      'Assets, inspections, defects, preventive maintenance, work orders, repairs, labor notes, part usage, and asset readiness.',
    doesNotOwn:
      'Employee records, training programs, route dispatch, vendor approval, or purchasing decisions.',
  },
  routarr: {
    owns:
      'Route planning, dispatch, driver and vehicle assignment, trip progress, stop tracking, exceptions, inspections, and route history.',
    doesNotOwn:
      'Employee records, training programs, repair work, parts purchasing, or regulatory rule libraries.',
  },
  supplyarr: {
    owns:
      'Vendors, customers, parts, purchase requests, approvals, receiving, pricing snapshots, lead times, and supply records.',
    doesNotOwn:
      'Employee records, training records, repair execution, dispatch execution, or warehouse movement history.',
  },
  loadarr: {
    owns:
      'Warehouse locations, receiving, stock movement, reservations, picking, shipments, adjustments, cycle counts, and inventory history.',
    doesNotOwn:
      'Vendor approval, purchase negotiation, employee records, training records, repair execution, or route dispatch.',
  },
  compliancecore: {
    owns:
      'Regulatory requirements, evidence expectations, citations, approved wording, rule packs, and compliance checks.',
    doesNotOwn:
      'The daily work records created by employees, trainers, mechanics, dispatchers, buyers, or warehouse teams.',
  },
  companion: {
    owns:
      'A simple field inbox that brings tasks and messages together and sends workers to the right product when action is needed.',
    doesNotOwn:
      'The original training, maintenance, dispatch, purchasing, inventory, or compliance records.',
  },
}

export const COMPLIANCE_CORE_EDUCATION = {
  headline: 'Rules and proof, connected to real work',
  lead:
    'Compliance Core helps the suite understand which rules apply, what proof matters, and where evidence should come from. The work still happens in the product built for that job.',
  bullets: [
    'A trainer, mechanic, dispatcher, or buyer keeps working in the tool made for their workflow.',
    'Compliance Core helps connect that work to rules, citations, evidence expectations, and audit questions.',
    'This keeps compliance proof tied to what actually happened instead of buried in side documents.',
  ],
} as const
