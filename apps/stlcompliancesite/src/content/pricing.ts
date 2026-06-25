export type LicensingPillar = {
  id: string
  title: string
  body: string
}

export type AccessExample = {
  productKey: string
  displayName: string
  summary: string
}

export const LICENSING_PILLARS: LicensingPillar[] = [
  {
    id: 'tenant',
    title: 'One customer account',
    body: 'Each customer gets a secure company space for its users, products, and access settings.',
  },
  {
    id: 'access',
    title: 'Choose the products you need',
    body: 'Customers can start with the products that match their work, then add more as the operation grows.',
  },
  {
    id: 'secure-sign-in',
    title: 'Secure sign-in is included',
    body: 'Secure suite access gives users one entry point instead of separate logins for every operational product.',
  },
  {
    id: 'honesty',
    title: 'No surprise checkout',
    body: 'Pricing is scoped with your team based on products, operational scale, and compliance complexity.',
  },
]

export const ACCESS_EXAMPLES: AccessExample[] = [
  {
    productKey: 'staffarr',
    displayName: 'StaffArr',
    summary: 'Workforce directory, readiness, certifications, and incidents.',
  },
  {
    productKey: 'trainarr',
    displayName: 'TrainArr',
    summary: 'Training assignments, evidence, signoffs, and qualification proof.',
  },
  {
    productKey: 'maintainarr',
    displayName: 'MaintainArr',
    summary: 'Assets, inspections, work orders, and maintenance history.',
  },
  {
    productKey: 'routarr',
    displayName: 'RoutArr',
    summary: 'Routes, dispatch, transportation execution, and inspections.',
  },
  {
    productKey: 'supplyarr',
    displayName: 'SupplyArr',
    summary: 'Vendors, purchasing, approvals, receiving, and supply records.',
  },
  {
    productKey: 'customarr',
    displayName: 'CustomArr',
    summary: 'Customer accounts, contacts, locations, requirements, and onboarding context.',
  },
  {
    productKey: 'ordarr',
    displayName: 'OrdArr',
    summary: 'Order and request lifecycle, product handoffs, and completion packets.',
  },
  {
    productKey: 'loadarr',
    displayName: 'LoadArr',
    summary: 'Warehouse operations, stock movement, picking, counts, and inventory history.',
  },
  {
    productKey: 'compliancecore',
    displayName: 'Compliance Core',
    summary: 'Rules, evidence mapping, citations, and compliance checks.',
  },
  {
    productKey: 'fieldcompanion',
    displayName: 'Field Companion',
    summary: 'Field inbox, task handoffs, and quick product navigation.',
  },
  {
    productKey: 'nexarr',
    displayName: 'NexArr',
    summary: 'Secure suite entry, tenant login, and product launch.',
  },
  {
    productKey: 'recordarr',
    displayName: 'RecordArr',
    summary: 'Document storage, retention, and controlled evidence record management.',
  },
  {
    productKey: 'reportarr',
    displayName: 'ReportArr',
    summary: 'Dashboards, reports, and operational trend visibility.',
  },
  {
    productKey: 'assurarr',
    displayName: 'AssurArr',
    summary: 'Nonconformance, CAPA, and assurance actions with effectivity tracking.',
  },
]

export const PRICING_DISCLAIMER =
  'Actual scope, limits, and commercial terms are defined with your STL Compliance team. List prices are not published on this site.'
