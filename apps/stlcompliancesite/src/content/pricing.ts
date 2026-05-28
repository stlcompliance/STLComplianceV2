export type LicensingPillar = {
  id: string
  title: string
  body: string
}

export type EntitlementExample = {
  productKey: string
  displayName: string
  summary: string
}

/** Public licensing narrative — not a price list or checkout surface. */
export const LICENSING_PILLARS: LicensingPillar[] = [
  {
    id: 'tenant',
    title: 'Tenant-scoped suite access',
    body: 'Every customer organization is a NexArr tenant. Identity, licensing metadata, and product entitlements are enforced on the control plane before any operational product API runs.',
  },
  {
    id: 'entitlements',
    title: 'Per-product entitlements',
    body: 'StaffArr, TrainArr, MaintainArr, RoutArr, SupplyArr, Compliance Core, and Companion are licensed independently per tenant. Entitlements determine which product UIs and APIs a user may launch — not this marketing site.',
  },
  {
    id: 'nexarr',
    title: 'NexArr is always in the path',
    body: 'Login, session authority, service clients, service tokens, and launch handoffs are NexArr responsibilities. Operational products never grant access on their own.',
  },
  {
    id: 'honesty',
    title: 'No checkout on this site',
    body: 'STLComplianceSite does not process payments, issue licenses, or change entitlements. Quotes, contracts, and provisioning are handled through your STL Compliance agreement and NexArr platform administration.',
  },
]

export const ENTITLEMENT_EXAMPLES: EntitlementExample[] = [
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    summary: 'Workforce directory, readiness, certifications, and incidents when entitled.',
  },
  {
    productKey: 'trainarr',
    displayName: 'TrainArr',
    summary: 'Training assignments, evidence, signoffs, and qualification proof when entitled.',
  },
  {
    productKey: 'maintainarr',
    displayName: 'MaintainArr',
    summary: 'Assets, inspections, work orders, and maintenance history when entitled.',
  },
  {
    productKey: 'routarr',
    displayName: 'RoutArr',
    summary: 'Routes, dispatch, transportation execution, and DVIR surfaces when entitled.',
  },
  {
    productKey: 'supplyarr',
    displayName: 'SupplyArr',
    summary: 'Vendors, purchasing, receiving, and inventory when entitled.',
  },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    summary: 'Controlled vocabulary, rule packs, mappings, and evaluation context when entitled.',
  },
  {
    productKey: 'companion',
    displayName: 'Companion',
    summary: 'Field inbox aggregation and deep links into entitled products when entitled.',
  },
]

export const PRICING_DISCLAIMER =
  'Illustrative packaging only. Actual scope, limits, and commercial terms are defined in your agreement with STL Compliance. List prices are not published on this marketing site.'
