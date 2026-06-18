export type CustomArrCustomerStatus = 'lead' | 'prospect' | 'qualified' | 'onboarding' | 'active' | 'suspended' | 'watch' | 'on_hold' | 'inactive' | 'archived' | 'blocked' | 'lost'
export type CustomArrCustomerTier = 'business' | 'individual' | 'government' | 'internal' | 'broker' | 'carrier' | 'strategic' | 'core' | 'standard' | 'shipper' | 'consignee' | 'broker_customer' | 'distributor' | 'manufacturer' | 'retailer'
export type CustomArrRequirementStatus = 'complete' | 'pending' | 'watch'
export type CustomArrActivityKind = 'created' | 'updated' | 'status' | 'review'

export interface CustomArrWorkspaceSession {
  userDisplayName: string
  tenantDisplayName: string
  tenantSlug: string
}

export interface CustomArrCustomerContact {
  contactId: string
  name: string
  role: string
  email: string
  phone: string
  isPrimary: boolean
  personId?: string | null
  firstName?: string
  lastName?: string
  displayName?: string
  title?: string | null
  department?: string | null
  businessEmail?: string
  primaryPhone?: string
  mobilePhone?: string | null
  phoneExtension?: string | null
  preferredContactMethodKey?: string
  preferredLanguageKey?: string | null
  timezone?: string | null
  primary?: boolean
  isBillingContact?: boolean
  isOrderingContact?: boolean
  isShippingContact?: boolean
  isEmergencyContact?: boolean
  portalAccessEnabled?: boolean
  portalRoleKey?: string | null
  statusKey?: string
  lastVerifiedAt?: string | null
}

export interface CustomArrCustomerLocation {
  locationId: string
  label: string
  type: 'billing' | 'shipping' | 'service'
  city: string
  state: string
  addressId?: string
  addressTypeKey?: string
  locationName?: string
  attentionTo?: string | null
  line1?: string
  line2?: string | null
  addressCity?: string
  stateProvince?: string
  postalCode?: string
  countryCode?: string
  latitude?: number | null
  longitude?: number | null
  timezone?: string | null
  deliveryInstructions?: string | null
  appointmentRequired?: boolean
  receivingHours?: string | null
  dockDoorNotes?: string | null
  accessRestrictions?: string | null
  isDefaultBilling?: boolean
  isDefaultShipping?: boolean
  isDefaultService?: boolean
  statusKey?: string
}

export interface CustomArrRequirementProgress {
  requirementKey: string
  title: string
  owner: string
  status: CustomArrRequirementStatus
  dueAt: string | null
  requirementId?: string
  requirementTypeKey?: string
  requirementName?: string
  description?: string
  requiredBeforeKey?: string
  recordArrDocumentId?: string | null
  complianceCoreRuleRef?: string | null
  statusKey?: string
  effectiveDate?: string | null
  expirationDate?: string | null
  reviewedByPersonId?: string | null
  reviewedAt?: string | null
  waiverReason?: string | null
  waivedByPersonId?: string | null
  ownerTeam?: string
}

export interface CustomArrActivityItem {
  activityId: string
  kind: CustomArrActivityKind
  message: string
  occurredAt: string
  sourceProductKey?: string
  actorPersonId?: string | null
}

export interface CustomArrCustomerIdentifier {
  identifierId: string
  identifierTypeKey: string
  identifierValue: string
  jurisdictionKey: string | null
  issuingAuthority: string | null
  effectiveDate: string | null
  expirationDate: string | null
  verificationStatusKey: string
  recordArrDocumentId: string | null
}

export interface CustomArrCustomerBillingProfile {
  billingProfileId: string
  billingContactId: string | null
  billingAddressId: string | null
  paymentTermsKey: string
  invoiceDeliveryMethodKey: string
  billingEmail: string | null
  purchaseOrderRequired: boolean
  taxExempt: boolean
  taxExemptionRecordId: string | null
  currencyCode: string
  creditStatusKey: string
  creditLimit: number | null
  externalAccountingCustomerRef: string | null
}

export interface CustomArrPortalSettings {
  portalEnabled: boolean
  portalDisplayName: string | null
  allowPortalOrderCreate: boolean
  allowPortalDocumentUpload: boolean
  allowPortalStatusView: boolean
  defaultPortalContactId: string | null
  portalInviteStatusKey: string
  portalTermsAcceptedAt: string | null
  portalTermsAcceptedByPersonId: string | null
  portalNotes: string | null
}

export interface CustomArrOperationalPreferences {
  defaultOrderTypeKey: string | null
  defaultServiceLevelKey: string | null
  defaultPickupAddressId: string | null
  defaultDeliveryAddressId: string | null
  defaultContactId: string | null
  requiresAppointment: boolean
  requiresProofOfDelivery: boolean
  requiresCustomerReference: boolean
  customerReferenceLabel: string | null
  defaultInstructions: string | null
  restrictedServiceNotes: string | null
  notificationPreferenceKey: string | null
  orderConfirmationRequired: boolean
}

export interface CustomArrCustomerExternalRef {
  externalRefId: string
  systemKey: string
  externalId: string
  externalCode: string | null
  lastSyncedAt: string | null
  syncStatusKey: string
}

export interface CustomArrCustomerRelationship {
  relationshipId: string
  relatedCustomerId: string
  relatedCustomerName: string | null
  relationshipTypeKey: string
  effectiveDate: string | null
  endDate: string | null
}

export interface CustomArrCustomerCustomFieldValue {
  fieldValueId: string
  fieldDefinitionId: string
  valueText: string | null
  valueNumber: number | null
  valueBoolean: boolean | null
  valueDate: string | null
  valueOptionKey: string | null
  effectiveDate: string | null
  sourceKey: string
  lastVerifiedAt: string | null
  updatedByPersonId: string | null
}

export interface CustomArrCustomerSummary {
  customerId: string
  tenantId?: string
  customerNumber: string
  customerCode?: string
  legalName: string
  displayName?: string
  dbaName?: string | null
  customerTypeKey?: string
  statusKey?: CustomArrCustomerStatus
  tradeName: string
  status: CustomArrCustomerStatus
  tier: CustomArrCustomerTier
  segment: string
  ownerPersonId: string
  parentCustomerId: string | null
  parentCustomerName: string | null
  primaryContactName: string
  primaryContactEmail: string
  siteCount: number
  contactCount: number
  requirementCount: number
  holdStatus: string
  lastActivityAt: string
  updatedAt: string
  primaryContactId?: string | null
  primaryBillingAddressId?: string | null
  primaryShippingAddressId?: string | null
  primaryServiceAddressId?: string | null
  assignedTeamId?: string | null
  customerSinceDate?: string | null
  sourceKey?: string
  tags?: string[]
  billingAddress?: string
  shippingAddress?: string
  paymentTerms?: string
  riskRating?: string
}

export interface CustomArrCustomerDetail extends CustomArrCustomerSummary {
  hierarchyPath: string[]
  billingAddress: string
  shippingAddress: string
  taxId: string
  paymentTerms: string
  riskRating: string
  notes: string[]
  contacts: CustomArrCustomerContact[]
  locations: CustomArrCustomerLocation[]
  addresses?: CustomArrCustomerLocation[]
  identifiers?: CustomArrCustomerIdentifier[]
  billingProfiles?: CustomArrCustomerBillingProfile[]
  portalSettings?: CustomArrPortalSettings
  operationalPreferences?: CustomArrOperationalPreferences
  requirements: CustomArrRequirementProgress[]
  externalRefs?: CustomArrCustomerExternalRef[]
  relationships?: CustomArrCustomerRelationship[]
  customFieldValues?: CustomArrCustomerCustomFieldValue[]
  activity: CustomArrActivityItem[]
  accountOwnerPersonId?: string | null
  createdAt?: string
  createdByPersonId?: string | null
  updatedByPersonId?: string | null
  archivedAt?: string | null
  archivedByPersonId?: string | null
  rowVersion?: number
}

export interface CustomArrRequirementCatalogItem {
  requirementKey: string
  title: string
  description: string
  status: CustomArrRequirementStatus
  ownerTeam: string
  appliesTo: string[]
}

export interface CustomArrCrmOverview {
  generatedAt: string
  accountCount: number
  leadCount: number
  opportunityCount: number
  proposalCount: number
  agreementCount: number
  openCaseCount: number
  openTaskCount: number
  blockedEligibilityCount: number
}

export interface CustomArrCrmRecord {
  module: string
  id: string
  number: string
  customerId: string | null
  customerName: string | null
  title: string
  statusKey: string
  ownerPersonId: string | null
  secondaryStatusKey: string | null
  value: number | null
  dueAt: string | null
  updatedAt: string | null
  summary: string | null
  sourceProductKey: string
  freshness: string
}

export interface CustomArrCreateCustomerRequest {
  legalName: string
  tradeName: string
  status: CustomArrCustomerStatus
  tier: CustomArrCustomerTier
  segment: string
  ownerPersonId: string
  parentCustomerId: string
  primaryContactName: string
  primaryContactEmail: string
  primaryContactPhone: string
  billingCity: string
  billingState: string
  shippingCity: string
  shippingState: string
  notes: string
  displayName?: string
  dbaName?: string
  customerTypeKey?: string
  statusKey?: string
  accountOwnerPersonId?: string
  assignedTeamId?: string
  customerSinceDate?: string | null
  sourceKey?: string
  tags?: string[]
  portalEnabled?: boolean
  portalDisplayName?: string
  paymentTermsKey?: string
  defaultOrderTypeKey?: string
  defaultServiceLevelKey?: string
  requiresAppointment?: boolean
  requiresProofOfDelivery?: boolean
  requiresCustomerReference?: boolean
  customerReferenceLabel?: string
  defaultInstructions?: string
  notificationPreferenceKey?: string
}

export const demoWorkspaceSession: CustomArrWorkspaceSession = {
  userDisplayName: 'Demo Admin',
  tenantDisplayName: 'CustomArr Demo Tenant',
  tenantSlug: 'demo-tenant',
}

export const demoRequirementCatalog: CustomArrRequirementCatalogItem[] = [
  {
    requirementKey: 'cert-insurance',
    title: 'Certificate of insurance',
    description: 'Current COI on file with liability and cargo coverage.',
    status: 'complete',
    ownerTeam: 'Risk',
    appliesTo: ['strategic', 'core'],
  },
  {
    requirementKey: 'tax-w9',
    title: 'Tax registration / W-9',
    description: 'Tax identity and remittance details verified before activation.',
    status: 'complete',
    ownerTeam: 'Finance',
    appliesTo: ['strategic', 'core', 'standard'],
  },
  {
    requirementKey: 'code-of-conduct',
    title: 'Supplier code of conduct',
    description: 'Signed commitment to the customer code of conduct and operating standards.',
    status: 'watch',
    ownerTeam: 'Procurement',
    appliesTo: ['strategic', 'core'],
  },
  {
    requirementKey: 'billing-terms',
    title: 'Billing terms acknowledgement',
    description: 'Confirmed billing schedule, payment terms, and invoicing contact.',
    status: 'complete',
    ownerTeam: 'Accounts receivable',
    appliesTo: ['strategic', 'core', 'standard'],
  },
  {
    requirementKey: 'e-invoicing',
    title: 'E-invoicing readiness',
    description: 'E-invoice endpoint and remittance workflow are confirmed for launch.',
    status: 'pending',
    ownerTeam: 'Operations',
    appliesTo: ['strategic'],
  },
]

export const demoCrmOverview: CustomArrCrmOverview = {
  generatedAt: '2026-06-17T15:00:00Z',
  accountCount: 3,
  leadCount: 3,
  opportunityCount: 3,
  proposalCount: 2,
  agreementCount: 2,
  openCaseCount: 3,
  openTaskCount: 5,
  blockedEligibilityCount: 1,
}

export const demoCrmRecords: Record<string, CustomArrCrmRecord[]> = {
  accounts: [
    { module: 'customer', id: 'cust-1001', number: 'CUS-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Acme Freight', statusKey: 'active', ownerPersonId: 'person-101', secondaryStatusKey: 'eligible', value: null, dueAt: null, updatedAt: '2026-06-11T13:05:00Z', summary: 'Strategic customer with recurring service expansion.', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'customer', id: 'cust-1002', number: 'CUS-1002', customerId: 'cust-1002', customerName: 'Northwind Components', title: 'Northwind Components', statusKey: 'onboarding', ownerPersonId: 'person-102', secondaryStatusKey: 'pending_review', value: null, dueAt: null, updatedAt: '2026-06-11T09:30:00Z', summary: 'Core industrial supply account in onboarding.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  locations: [
    { module: 'customer_location', id: 'loc-1001', number: 'DAL-HQ', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'HQ', statusKey: 'active', ownerPersonId: null, secondaryStatusKey: 'billing', value: null, dueAt: null, updatedAt: '2026-06-11T13:05:00Z', summary: '410 Harbor Ave, Dallas, TX', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'customer_location', id: 'loc-2001', number: 'COL-PLANT', customerId: 'cust-1002', customerName: 'Northwind Components', title: 'Primary plant', statusKey: 'active', ownerPersonId: null, secondaryStatusKey: 'service', value: null, dueAt: null, updatedAt: '2026-06-11T09:30:00Z', summary: 'Appointment required; dock access during receiving hours.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  contacts: [
    { module: 'customer_contact', id: 'ct-1001', number: 'ct-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Maria Jensen', statusKey: 'active', ownerPersonId: null, secondaryStatusKey: 'portal_enabled', value: null, dueAt: null, updatedAt: '2026-06-10T15:40:00Z', summary: 'Ordering, billing, portal admin', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'customer_contact', id: 'ct-2001', number: 'ct-2001', customerId: 'cust-1002', customerName: 'Northwind Components', title: 'Harper Lane', statusKey: 'active', ownerPersonId: null, secondaryStatusKey: 'email', value: null, dueAt: null, updatedAt: '2026-06-11T09:30:00Z', summary: 'Commercial lead and onboarding sponsor', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  leads: [
    { module: 'lead', id: 'lead-1001', number: 'LEAD-1001', customerId: null, customerName: null, title: 'Harbor View Cold Chain', statusKey: 'qualified', ownerPersonId: 'person-101', secondaryStatusKey: 'referral', value: 84, dueAt: '2026-06-20T15:00:00Z', updatedAt: '2026-06-16T15:00:00Z', summary: 'Warehouse and transportation interest.', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'lead', id: 'lead-1002', number: 'LEAD-1002', customerId: null, customerName: null, title: 'Metro Builders Supply', statusKey: 'new', ownerPersonId: 'person-103', secondaryStatusKey: 'web', value: 58, dueAt: '2026-06-21T15:00:00Z', updatedAt: '2026-06-15T15:00:00Z', summary: 'Regional distribution inquiry.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  opportunities: [
    { module: 'opportunity', id: 'opp-1001', number: 'OPP-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Recurring service expansion', statusKey: 'open', ownerPersonId: 'person-101', secondaryStatusKey: 'proposal', value: 125000, dueAt: '2026-07-08T15:00:00Z', updatedAt: '2026-06-17T15:00:00Z', summary: 'Transportation and warehouse coordination package.', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'opportunity', id: 'opp-1002', number: 'OPP-1002', customerId: 'cust-1002', customerName: 'Northwind Components', title: 'Onboarding launch services', statusKey: 'open', ownerPersonId: 'person-102', secondaryStatusKey: 'discovery', value: 48000, dueAt: '2026-07-01T15:00:00Z', updatedAt: '2026-06-14T15:00:00Z', summary: 'Customer launch and requirement readiness.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  proposals: [
    { module: 'proposal', id: 'prop-1001', number: 'PROP-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Proposal v1', statusKey: 'sent', ownerPersonId: 'person-101', secondaryStatusKey: 'pending', value: null, dueAt: '2026-07-17T15:00:00Z', updatedAt: '2026-06-13T15:00:00Z', summary: 'Recurring coordination pricing and standard terms snapshot.', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'proposal', id: 'prop-1002', number: 'PROP-1002', customerId: 'cust-1002', customerName: 'Northwind Components', title: 'Proposal v2', statusKey: 'draft', ownerPersonId: 'person-102', secondaryStatusKey: 'pending', value: null, dueAt: '2026-07-05T15:00:00Z', updatedAt: '2026-06-16T15:00:00Z', summary: 'Launch-service package awaiting internal approval.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  agreements: [
    { module: 'agreement', id: 'agr-1001', number: 'AGR-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Master service agreement', statusKey: 'active', ownerPersonId: 'person-101', secondaryStatusKey: 'master_service_agreement', value: null, dueAt: '2027-04-17T15:00:00Z', updatedAt: '2026-06-17T15:00:00Z', summary: 'Customer services, portal terms, and documentation expectations.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  cases: [
    { module: 'case', id: 'case-1001', number: 'CASE-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Portal notification preference review', statusKey: 'in_progress', ownerPersonId: 'person-101', secondaryStatusKey: 'normal', value: null, dueAt: '2026-06-24T15:00:00Z', updatedAt: '2026-06-16T15:00:00Z', summary: 'Confirm status notifications route to operations and billing contacts.', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'case', id: 'case-1002', number: 'CASE-1002', customerId: 'cust-1003', customerName: 'South Ridge Logistics', title: 'Document refresh escalation', statusKey: 'new', ownerPersonId: 'person-103', secondaryStatusKey: 'high', value: null, dueAt: '2026-06-19T15:00:00Z', updatedAt: '2026-06-17T15:00:00Z', summary: 'Customer needs updated requirement evidence before next order handoff.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  activities: [
    { module: 'activity', id: 'act-1001', number: 'act-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Proposal review call', statusKey: 'call', ownerPersonId: 'person-101', secondaryStatusKey: 'customer_call', value: null, dueAt: null, updatedAt: '2026-06-16T18:00:00Z', summary: 'Customer requested revised service assumptions.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  tasks: [
    { module: 'task', id: 'task-1001', number: 'TASK-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Follow up on proposal response', statusKey: 'open', ownerPersonId: 'person-101', secondaryStatusKey: 'high', value: null, dueAt: '2026-06-19T15:00:00Z', updatedAt: '2026-06-17T15:00:00Z', summary: 'Call operations sponsor and capture response.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  'portal-access': [
    { module: 'portal_access', id: 'portal-1001', number: 'portal-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'ct-1001', statusKey: 'active', ownerPersonId: 'person-101', secondaryStatusKey: 'customer_admin', value: null, dueAt: null, updatedAt: '2026-06-17T15:00:00Z', summary: 'NexArr external identity linked for portal order and status access.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  eligibility: [
    { module: 'eligibility', id: 'elig-1001', number: 'elig-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'ordarr.order.create', statusKey: 'eligible', ownerPersonId: 'person-101', secondaryStatusKey: 'customarr', value: null, dueAt: null, updatedAt: '2026-06-17T15:00:00Z', summary: 'Customer is eligible for order handoff.', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'eligibility', id: 'elig-1002', number: 'elig-1002', customerId: 'cust-1003', customerName: 'South Ridge Logistics', title: 'ordarr.order.create', statusKey: 'blocked', ownerPersonId: 'person-103', secondaryStatusKey: 'customarr', value: null, dueAt: null, updatedAt: '2026-06-16T15:00:00Z', summary: 'Document refresh requirement blocks order handoff.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  onboarding: [
    { module: 'onboarding', id: 'onb-1001', number: 'ONB-1001', customerId: 'cust-1002', customerName: 'Northwind Components', title: 'new_customer', statusKey: 'in_review', ownerPersonId: 'person-102', secondaryStatusKey: null, value: null, dueAt: '2026-06-28T15:00:00Z', updatedAt: '2026-06-17T15:00:00Z', summary: 'Billing terms and e-invoicing remain open.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  health: [
    { module: 'health', id: 'health-1001', number: 'health-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Acme Freight health', statusKey: 'green', ownerPersonId: 'person-101', secondaryStatusKey: 'low', value: 88, dueAt: '2026-09-17T15:00:00Z', updatedAt: '2026-06-17T15:00:00Z', summary: 'Healthy relationship with expansion upside.', sourceProductKey: 'customarr', freshness: 'live' },
    { module: 'health', id: 'health-1003', number: 'health-1003', customerId: 'cust-1003', customerName: 'South Ridge Logistics', title: 'South Ridge Logistics health', statusKey: 'yellow', ownerPersonId: 'person-103', secondaryStatusKey: 'medium', value: 61, dueAt: '2026-07-17T15:00:00Z', updatedAt: '2026-06-17T15:00:00Z', summary: 'Watch item due to missing requirement evidence.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  imports: [
    { module: 'import', id: 'imp-1001', number: 'imp-1001', customerId: null, customerName: null, title: 'customer-refresh.csv', statusKey: 'reviewed', ownerPersonId: 'person-999', secondaryStatusKey: 'csv', value: 42, dueAt: null, updatedAt: '2026-06-16T15:00:00Z', summary: 'Two duplicate candidates routed to merge review.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  'merge-review': [
    { module: 'merge', id: 'mrg-1001', number: 'mrg-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Merge review', statusKey: 'proposed', ownerPersonId: 'person-999', secondaryStatusKey: 'manual_review', value: 1, dueAt: null, updatedAt: '2026-06-16T15:00:00Z', summary: 'Potential duplicate from imported customer list.', sourceProductKey: 'customarr', freshness: 'live' },
  ],
  'integration-references': [
    { module: 'integration_reference', id: 'xref-1001', number: 'qb-CUS-1001', customerId: 'cust-1001', customerName: 'Acme Freight', title: 'Acme Freight Systems LLC', statusKey: 'active', ownerPersonId: null, secondaryStatusKey: 'quickbooks', value: null, dueAt: null, updatedAt: '2026-06-17T15:00:00Z', summary: 'customer in quickbooks', sourceProductKey: 'customarr', freshness: 'live' },
  ],
}

const buildRequirements = (count: number, prefix: string): CustomArrRequirementProgress[] =>
  demoRequirementCatalog.slice(0, count).map((item, index) => ({
    requirementKey: `${prefix}-${item.requirementKey}`,
    title: item.title,
    owner: item.ownerTeam,
    status: index === 0 ? 'complete' : index === 1 ? 'complete' : index === 2 ? 'watch' : 'pending',
    dueAt: index === 4 ? '2026-07-15T17:00:00Z' : null,
  }))

export const demoCustomersSeed: CustomArrCustomerDetail[] = [
  {
    customerId: 'cust-1001',
    customerNumber: 'CUS-1001',
    legalName: 'Acme Freight Systems LLC',
    tradeName: 'Acme Freight',
    status: 'active',
    tier: 'strategic',
    segment: 'Enterprise logistics',
    ownerPersonId: 'person-101',
    parentCustomerId: null,
    parentCustomerName: null,
    primaryContactName: 'Maria Jensen',
    primaryContactEmail: 'maria.jensen@acmefreight.example',
    siteCount: 4,
    contactCount: 3,
    requirementCount: 5,
    holdStatus: 'clear',
    lastActivityAt: '2026-06-10T15:40:00Z',
    updatedAt: '2026-06-11T13:05:00Z',
    hierarchyPath: ['Acme Freight Systems LLC'],
    billingAddress: '410 Harbor Ave, Dallas, TX 75201',
    shippingAddress: '410 Harbor Ave, Dallas, TX 75201',
    taxId: 'TX-45-9988123',
    paymentTerms: 'Net 30',
    riskRating: 'low',
    notes: ['Strategic customer with quarterly review cadence.', 'Primary onboarding and contract data is complete.'],
    contacts: [
      { contactId: 'ct-1001', name: 'Maria Jensen', role: 'Primary procurement contact', email: 'maria.jensen@acmefreight.example', phone: '+1 (972) 555-0191', isPrimary: true },
      { contactId: 'ct-1002', name: 'Derek Holt', role: 'Accounts payable', email: 'derek.holt@acmefreight.example', phone: '+1 (972) 555-0192', isPrimary: false },
      { contactId: 'ct-1003', name: 'Nina Rao', role: 'Operations manager', email: 'nina.rao@acmefreight.example', phone: '+1 (972) 555-0193', isPrimary: false },
    ],
    locations: [
      { locationId: 'loc-1001', label: 'HQ', type: 'billing', city: 'Dallas', state: 'TX' },
      { locationId: 'loc-1002', label: 'Cross dock', type: 'shipping', city: 'Fort Worth', state: 'TX' },
      { locationId: 'loc-1003', label: 'Service center', type: 'service', city: 'Arlington', state: 'TX' },
    ],
    requirements: buildRequirements(5, 'acme'),
    activity: [
      { activityId: 'act-1001', kind: 'status', message: 'Moved from onboarding to active.', occurredAt: '2026-06-11T13:05:00Z' },
      { activityId: 'act-1002', kind: 'updated', message: 'Primary contact and remittance address confirmed.', occurredAt: '2026-06-10T15:40:00Z' },
    ],
  },
  {
    customerId: 'cust-1002',
    customerNumber: 'CUS-1002',
    legalName: 'Northwind Components Inc.',
    tradeName: 'Northwind Components',
    status: 'onboarding',
    tier: 'core',
    segment: 'Industrial supply',
    ownerPersonId: 'person-102',
    parentCustomerId: 'cust-1001',
    parentCustomerName: 'Acme Freight Systems LLC',
    primaryContactName: 'Harper Lane',
    primaryContactEmail: 'harper.lane@northwind.example',
    siteCount: 2,
    contactCount: 2,
    requirementCount: 4,
    holdStatus: 'watch',
    lastActivityAt: '2026-06-09T11:15:00Z',
    updatedAt: '2026-06-11T09:30:00Z',
    hierarchyPath: ['Acme Freight Systems LLC', 'Northwind Components Inc.'],
    billingAddress: '88 Foundry Rd, Columbus, OH 43085',
    shippingAddress: '88 Foundry Rd, Columbus, OH 43085',
    taxId: 'OH-29-1188221',
    paymentTerms: 'Net 45',
    riskRating: 'medium',
    notes: ['Pending e-invoicing confirmation.', 'Requires final legal review before activation.'],
    contacts: [
      { contactId: 'ct-2001', name: 'Harper Lane', role: 'Commercial lead', email: 'harper.lane@northwind.example', phone: '+1 (614) 555-0110', isPrimary: true },
      { contactId: 'ct-2002', name: 'Soren Patel', role: 'Billing specialist', email: 'soren.patel@northwind.example', phone: '+1 (614) 555-0111', isPrimary: false },
    ],
    locations: [
      { locationId: 'loc-2001', label: 'Primary plant', type: 'service', city: 'Columbus', state: 'OH' },
      { locationId: 'loc-2002', label: 'Distribution yard', type: 'shipping', city: 'Newark', state: 'OH' },
    ],
    requirements: buildRequirements(4, 'northwind'),
    activity: [
      { activityId: 'act-2001', kind: 'created', message: 'Customer record created by onboarding operations.', occurredAt: '2026-06-11T09:30:00Z' },
      { activityId: 'act-2002', kind: 'review', message: 'Billing terms pending finance signoff.', occurredAt: '2026-06-09T11:15:00Z' },
    ],
  },
  {
    customerId: 'cust-1003',
    customerNumber: 'CUS-1003',
    legalName: 'South Ridge Logistics Partners',
    tradeName: 'South Ridge Logistics',
    status: 'watch',
    tier: 'standard',
    segment: 'Regional transportation',
    ownerPersonId: 'person-103',
    parentCustomerId: null,
    parentCustomerName: null,
    primaryContactName: 'Elena Park',
    primaryContactEmail: 'elena.park@southridge.example',
    siteCount: 1,
    contactCount: 2,
    requirementCount: 3,
    holdStatus: 'review',
    lastActivityAt: '2026-06-08T18:15:00Z',
    updatedAt: '2026-06-10T10:45:00Z',
    hierarchyPath: ['South Ridge Logistics Partners'],
    billingAddress: '2100 Market St, Memphis, TN 38103',
    shippingAddress: '2100 Market St, Memphis, TN 38103',
    taxId: 'TN-81-9044117',
    paymentTerms: 'Prepaid',
    riskRating: 'elevated',
    notes: ['Watch list due to delayed document refresh.', 'Requires ownership reassignment after transition.'],
    contacts: [
      { contactId: 'ct-3001', name: 'Elena Park', role: 'Customer success', email: 'elena.park@southridge.example', phone: '+1 (901) 555-0122', isPrimary: true },
      { contactId: 'ct-3002', name: 'Jordan Price', role: 'Finance contact', email: 'jordan.price@southridge.example', phone: '+1 (901) 555-0123', isPrimary: false },
    ],
    locations: [
      { locationId: 'loc-3001', label: 'Operations office', type: 'service', city: 'Memphis', state: 'TN' },
    ],
    requirements: buildRequirements(3, 'southridge'),
    activity: [
      { activityId: 'act-3001', kind: 'status', message: 'Moved to watch after document refresh delay.', occurredAt: '2026-06-08T18:15:00Z' },
      { activityId: 'act-3002', kind: 'updated', message: 'Ownership review assigned to customer operations.', occurredAt: '2026-06-10T10:45:00Z' },
    ],
  },
]

export function cloneCustomers(customers: CustomArrCustomerDetail[]): CustomArrCustomerDetail[] {
  return customers.map((customer) => ({
    ...customer,
    hierarchyPath: [...customer.hierarchyPath],
    notes: [...customer.notes],
    contacts: customer.contacts.map((contact) => ({ ...contact })),
    locations: customer.locations.map((location) => ({ ...location })),
    requirements: customer.requirements.map((requirement) => ({ ...requirement })),
    activity: customer.activity.map((activity) => ({ ...activity })),
  }))
}
