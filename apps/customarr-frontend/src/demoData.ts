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
