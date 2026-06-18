import type {
  CustomArrCreateCustomerRequest,
  CustomArrCrmOverview,
  CustomArrCrmRecord,
  CustomArrCustomerDetail,
  CustomArrCustomerSummary,
  CustomArrRequirementCatalogItem,
  CustomArrWorkspaceSession,
} from '../demoData'

const apiBase = import.meta.env.VITE_CUSTOMARR_API_BASE ?? ''

export interface CustomArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasCustomArrEntitlement: boolean
  entitlements: string[]
}

export interface CustomArrHandoffSessionResponse {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
  themePreference?: string | null
  callbackUrl: string | null
}

export interface CustomArrDashboardResponse {
  generatedAt: string
  customerCount: number
  activeCustomerCount: number
  onboardingCustomerCount: number
  watchListCustomerCount: number
  contactCount: number
  siteCount: number
  requirementCount: number
  featuredCustomers: CustomArrCustomerSummary[]
  recentActivity: Array<{
    activityId: string
    customerId: string
    customerNumber: string
    message: string
    occurredAt: string
    kind: string
  }>
}

export type CustomArrCrmModuleRoute =
  | 'accounts'
  | 'locations'
  | 'contacts'
  | 'leads'
  | 'opportunities'
  | 'proposals'
  | 'agreements'
  | 'cases'
  | 'activities'
  | 'tasks'
  | 'portal-access'
  | 'eligibility'
  | 'onboarding'
  | 'health'
  | 'imports'
  | 'merge-review'
  | 'integration-references'

export interface CustomerNumberingSettings {
  prefix: string
  sequenceName: string
  paddingLength: number
  nextNumber: number
  allowManualOverride: boolean
  manualOverrideRequiresPermission: boolean
  displayFormat: string
  uniquenessScope: string
  preview: string
}

export interface CustomerLifecycleStageItem {
  key: string
  label: string
  description: string
  sortOrder: number
  isInitial: boolean
  isActiveCustomerStage: boolean
  isTerminal: boolean
  blocksOrders: boolean
  blocksPortalAccess: boolean
  requiresApprovalToEnter: boolean
  requiresReasonToExit: boolean
  allowedNextStageKeys: string[]
  colorToken: string | null
  isSystemRequired: boolean
}

export interface CustomerLifecycleTransitionRuleItem {
  fromStageKey: string
  toStageKey: string
  requiresApproval: boolean
  requiredPermission: string | null
  requiredChecklistTemplateKey: string | null
  requiredReason: boolean
  blockIfOpenIssues: boolean
  blockIfExpiredRequiredDocuments: boolean
  blockIfMissingRequiredFields: boolean
}

export interface CustomerClassificationCatalogItem {
  catalogType: string
  key: string
  label: string
  description: string
  sortOrder: number
  isActive: boolean
  isDefault: boolean
  metadataKey: string | null
  metadataValue: string | null
}

export interface CustomerRequiredFieldRuleItem {
  customerTypeKey: string | null
  lifecycleStageKey: string | null
  fieldKey: string
  requirementLevel: 'hidden' | 'optional' | 'recommended' | 'required' | string
  validationMessage: string
  appliesToPortal: boolean
  appliesToInternalCreate: boolean
  appliesToInternalEdit: boolean
}

export interface CustomerContactRoleItem {
  key: string
  label: string
  description: string
  isRequiredForActiveCustomer: boolean
  requiresUniquePrimary: boolean
  allowsPortalAccess: boolean
  canReceiveOrderNotifications: boolean
  canReceiveBillingNotifications: boolean
  canReceiveComplianceNotifications: boolean
  sortOrder: number
  isActive: boolean
}

export interface CustomerAddressTypeItem {
  key: string
  label: string
  description: string
  isRequiredForActiveCustomer: boolean
  requiresValidation: boolean
  requiresGeocode: boolean
  usableForBilling: boolean
  usableForPickup: boolean
  usableForDelivery: boolean
  usableForService: boolean
  sortOrder: number
  isActive: boolean
}

export interface CustomerOwnerRuleItem {
  ruleName: string
  priority: number
  isActive: boolean
  customerTypeKey: string | null
  territoryKey: string | null
  industryKey: string | null
  sourceKey: string | null
  defaultOwnerType: string
  defaultOwnerRefId: string
  defaultOwnerNameSnapshot: string
  requiresOwnerForActiveCustomer: boolean
  requiresApprovalForReassignment: boolean
  approvalPermission: string | null
}

export interface CustomerOnboardingTemplateItem {
  key: string
  label: string
  description: string
  customerTypeKey: string | null
  industryKey: string | null
  priorityTierKey: string | null
  isDefault: boolean
  isActive: boolean
  blocksActivationUntilComplete: boolean
  sortOrder: number
}

export interface CustomerOnboardingChecklistItemTemplateItem {
  templateKey: string
  key: string
  label: string
  description: string
  itemType: string
  required: boolean
  sortOrder: number
  ownerType: string | null
  ownerRefId: string | null
  ownerNameSnapshot: string | null
  documentTypeKey: string | null
  complianceQuestionnaireKey: string | null
  blocksActivation: boolean
  blocksOrders: boolean
  blocksPortalAccess: boolean
}

export interface CustomerPortalTenantSettingsItem {
  portalEnabled: boolean
  inviteOnly: boolean
  selfRegistrationAllowed: boolean
  requireEmailVerification: boolean
  requireInternalApprovalForPortalUsers: boolean
  allowedEmailDomains: string[]
  supportContactName: string
  supportContactEmail: string
  supportContactPhone: string
  portalDisplayName: string
  logoRecordArrDocumentId: string | null
  allowedActions: {
    canViewProfile: boolean
    canRequestQuote: boolean
    canPlaceOrderRequest: boolean
    canUploadDocuments: boolean
    canSubmitIssue: boolean
    canViewOrderStatus: boolean
    canViewInvoicesSnapshot: boolean
  }
  defaultPortalContactRoleKey: string
  portalAdminContactRoleKey: string
}

export interface CustomerDocumentRequirementItem {
  key: string
  label: string
  description: string
  customerTypeKey: string | null
  lifecycleStageKey: string | null
  required: boolean
  expires: boolean
  expirationWarningDays: number | null
  recordArrDocumentTypeKey: string
  customerCanUpload: boolean
  visibleInPortal: boolean
  blocksActivation: boolean
  blocksOrders: boolean
  blocksPortalAccess: boolean
}

export interface CustomerDuplicateDetectionRuleItem {
  key: string
  label: string
  isActive: boolean
  priority: number
  matchField: string
  matchType: string
  weight: number
  autoBlockThreshold: number
  reviewThreshold: number
}

export interface CustomerIntegrationSettingsItem {
  erpSyncMode: string
  defaultConflictResolution: string
  emitEventsForDraftCustomers: boolean
  emitEventsForProspects: boolean
  emitEventsOnlyAfterActivation: boolean
  allowExternalCreate: boolean
  allowExternalUpdate: boolean
  requireReviewForExternalUpdate: boolean
}

export interface CustomerExternalIdSourceItem {
  key: string
  label: string
  sourceType: string
  required: boolean
  uniqueWithinTenant: boolean
  visibleInUi: boolean
  editableInUi: boolean
  isActive: boolean
}

export interface CustomerNotificationRuleItem {
  key: string
  label: string
  eventType: string
  isActive: boolean
  recipientType: string
  recipientRefId: string | null
  recipientNameSnapshot: string | null
  customerContactRoleKey: string | null
  delayMinutes: number
  escalationAfterMinutes: number | null
  templateKey: string | null
}

export interface CustomerCustomFieldDefinitionItem {
  key: string
  label: string
  description: string
  fieldType: string
  appliesToCustomerTypeKey: string | null
  appliesToLifecycleStageKey: string | null
  required: boolean
  visibleInPortal: boolean
  editableInPortal: boolean
  internalOnly: boolean
  sortOrder: number
  isActive: boolean
}

export interface CustomerCustomFieldOptionItem {
  fieldKey: string
  key: string
  label: string
  sortOrder: number
  isActive: boolean
}

export interface CustomArrTenantSettingsResponse {
  scope: string
  settingsVersion: number
  isActive: boolean
  effectiveFrom: string
  effectiveTo: string | null
  updatedAt: string
  numbering: CustomerNumberingSettings
  lifecycleStages: CustomerLifecycleStageItem[]
  transitionRules: CustomerLifecycleTransitionRuleItem[]
  classificationCatalogs: CustomerClassificationCatalogItem[]
  requiredFieldRules: CustomerRequiredFieldRuleItem[]
  contactRoles: CustomerContactRoleItem[]
  addressTypes: CustomerAddressTypeItem[]
  ownerRules: CustomerOwnerRuleItem[]
  onboardingTemplates: CustomerOnboardingTemplateItem[]
  onboardingChecklistItems: CustomerOnboardingChecklistItemTemplateItem[]
  portalSettings: CustomerPortalTenantSettingsItem
  documentRequirements: CustomerDocumentRequirementItem[]
  duplicateDetectionRules: CustomerDuplicateDetectionRuleItem[]
  integrationSettings: CustomerIntegrationSettingsItem
  externalIdSources: CustomerExternalIdSourceItem[]
  notificationRules: CustomerNotificationRuleItem[]
  customFieldDefinitions: CustomerCustomFieldDefinitionItem[]
  customFieldOptions: CustomerCustomFieldOptionItem[]
  warnings: Array<{ key: string; message: string }>
}

export interface CustomArrCustomerCreateMetadataResponse {
  customerNumberPreview: string
  initialLifecycleStageKey: string
  lifecycleStages: CustomerLifecycleStageItem[]
  classificationCatalogs: CustomerClassificationCatalogItem[]
  contactRoles: CustomerContactRoleItem[]
  addressTypes: CustomerAddressTypeItem[]
  requiredFieldRules: CustomerRequiredFieldRuleItem[]
  onboardingTemplates: CustomerOnboardingTemplateItem[]
  documentRequirements: CustomerDocumentRequirementItem[]
  customFieldDefinitions: CustomerCustomFieldDefinitionItem[]
  ownerRules: CustomerOwnerRuleItem[]
}

class CustomArrApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message)
    this.name = 'CustomArrApiError'
  }
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new CustomArrApiError(body || `${fallbackMessage} (${response.status})`, response.status)
  }

  return (await response.json()) as T
}

export async function getSessionBootstrap(accessToken: string): Promise<CustomArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/v1/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrSessionBootstrapResponse>(response, 'Failed to load session bootstrap')
}

export async function redeemHandoff(handoffCode: string): Promise<CustomArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/v1/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<CustomArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getDashboard(accessToken: string): Promise<CustomArrDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/workspace/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrDashboardResponse>(response, 'Failed to load dashboard')
}

export async function getCrmOverview(accessToken: string): Promise<CustomArrCrmOverview> {
  const response = await fetch(`${apiBase}/api/v1/workspace/crm-overview`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrCrmOverview>(response, 'Failed to load CRM overview')
}

export async function listCrmRecords(accessToken: string, module: CustomArrCrmModuleRoute): Promise<CustomArrCrmRecord[]> {
  const response = await fetch(`${apiBase}/api/v1/workspace/${module}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrCrmRecord[]>(response, `Failed to load ${module}`)
}

export async function listCustomers(accessToken: string, search?: string): Promise<CustomArrCustomerDetail[]> {
  const searchParams = new URLSearchParams()
  if (search?.trim()) {
    searchParams.set('search', search.trim())
  }
  const response = await fetch(`${apiBase}/api/v1/workspace/customers${searchParams.toString() ? `?${searchParams}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrCustomerDetail[]>(response, 'Failed to load customers')
}

export async function getCustomer(accessToken: string, customerId: string): Promise<CustomArrCustomerDetail> {
  const response = await fetch(`${apiBase}/api/v1/workspace/customers/${customerId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrCustomerDetail>(response, 'Failed to load customer')
}

export async function createCustomer(
  accessToken: string,
  body: CustomArrCreateCustomerRequest,
): Promise<CustomArrCustomerDetail> {
  const response = await fetch(`${apiBase}/api/v1/workspace/customers`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': `customarr-customer-${crypto.randomUUID()}`,
    },
    body: JSON.stringify(body),
  })
  return parseJsonResponse<CustomArrCustomerDetail>(response, 'Failed to create customer')
}

export async function listRequirements(accessToken: string): Promise<CustomArrRequirementCatalogItem[]> {
  const response = await fetch(`${apiBase}/api/v1/workspace/requirements`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrRequirementCatalogItem[]>(response, 'Failed to load requirement catalog')
}

export async function getTenantSettings(accessToken: string): Promise<CustomArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/customarr/tenant-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrTenantSettingsResponse>(response, 'Failed to load CustomArr tenant settings')
}

export async function updateTenantSettings(
  accessToken: string,
  body: CustomArrTenantSettingsResponse,
): Promise<CustomArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/customarr/tenant-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({
      numbering: body.numbering,
      lifecycleStages: body.lifecycleStages,
      transitionRules: body.transitionRules,
      classificationCatalogs: body.classificationCatalogs,
      requiredFieldRules: body.requiredFieldRules,
      contactRoles: body.contactRoles,
      addressTypes: body.addressTypes,
      ownerRules: body.ownerRules,
      onboardingTemplates: body.onboardingTemplates,
      onboardingChecklistItems: body.onboardingChecklistItems,
      portalSettings: body.portalSettings,
      documentRequirements: body.documentRequirements,
      duplicateDetectionRules: body.duplicateDetectionRules,
      integrationSettings: body.integrationSettings,
      externalIdSources: body.externalIdSources,
      notificationRules: body.notificationRules,
      customFieldDefinitions: body.customFieldDefinitions,
      customFieldOptions: body.customFieldOptions,
    }),
  })
  return parseJsonResponse<CustomArrTenantSettingsResponse>(response, 'Failed to update CustomArr tenant settings')
}

export async function getCustomerCreateMetadata(accessToken: string): Promise<CustomArrCustomerCreateMetadataResponse> {
  const response = await fetch(`${apiBase}/api/v1/customarr/customers/create-metadata`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrCustomerCreateMetadataResponse>(response, 'Failed to load customer create metadata')
}

export function resolveDemoWorkspaceSession(): CustomArrWorkspaceSession {
  return {
    userDisplayName: 'Demo Admin',
    tenantDisplayName: 'CustomArr Demo Tenant',
    tenantSlug: 'demo-tenant',
  }
}

export const demoTenantSettings: CustomArrTenantSettingsResponse = {
  scope: 'tenant',
  settingsVersion: 1,
  isActive: true,
  effectiveFrom: '2026-06-17T00:00:00Z',
  effectiveTo: null,
  updatedAt: '2026-06-17T00:00:00Z',
  numbering: {
    prefix: 'CUS',
    sequenceName: 'customarr_customer',
    paddingLength: 4,
    nextNumber: 1004,
    allowManualOverride: false,
    manualOverrideRequiresPermission: true,
    displayFormat: '{prefix}-{number}',
    uniquenessScope: 'tenant',
    preview: 'CUS-1004',
  },
  lifecycleStages: [
    { key: 'lead', label: 'Lead', description: 'Unqualified customer interest.', sortOrder: 10, isInitial: true, isActiveCustomerStage: false, isTerminal: false, blocksOrders: true, blocksPortalAccess: true, requiresApprovalToEnter: false, requiresReasonToExit: false, allowedNextStageKeys: ['prospect', 'lost'], colorToken: 'cyan', isSystemRequired: true },
    { key: 'prospect', label: 'Prospect', description: 'Potential customer being evaluated.', sortOrder: 20, isInitial: false, isActiveCustomerStage: false, isTerminal: false, blocksOrders: false, blocksPortalAccess: false, requiresApprovalToEnter: false, requiresReasonToExit: false, allowedNextStageKeys: ['qualified', 'lost'], colorToken: 'sky', isSystemRequired: true },
    { key: 'qualified', label: 'Qualified', description: 'Fit and need are confirmed.', sortOrder: 30, isInitial: false, isActiveCustomerStage: false, isTerminal: false, blocksOrders: false, blocksPortalAccess: false, requiresApprovalToEnter: false, requiresReasonToExit: false, allowedNextStageKeys: ['onboarding', 'lost'], colorToken: 'blue', isSystemRequired: true },
    { key: 'onboarding', label: 'Onboarding', description: 'Customer setup and requirements are in progress.', sortOrder: 40, isInitial: false, isActiveCustomerStage: false, isTerminal: false, blocksOrders: true, blocksPortalAccess: false, requiresApprovalToEnter: true, requiresReasonToExit: false, allowedNextStageKeys: ['active', 'suspended', 'inactive'], colorToken: 'amber', isSystemRequired: true },
    { key: 'active', label: 'Active', description: 'Customer can be used by downstream execution workflows.', sortOrder: 50, isInitial: false, isActiveCustomerStage: true, isTerminal: false, blocksOrders: false, blocksPortalAccess: false, requiresApprovalToEnter: true, requiresReasonToExit: false, allowedNextStageKeys: ['suspended', 'inactive'], colorToken: 'emerald', isSystemRequired: true },
    { key: 'suspended', label: 'Suspended', description: 'Customer is temporarily blocked from orders or portal access.', sortOrder: 60, isInitial: false, isActiveCustomerStage: false, isTerminal: false, blocksOrders: true, blocksPortalAccess: true, requiresApprovalToEnter: true, requiresReasonToExit: true, allowedNextStageKeys: ['active', 'inactive'], colorToken: 'rose', isSystemRequired: true },
    { key: 'inactive', label: 'Inactive', description: 'Customer is no longer active but remains retained.', sortOrder: 70, isInitial: false, isActiveCustomerStage: false, isTerminal: true, blocksOrders: true, blocksPortalAccess: true, requiresApprovalToEnter: false, requiresReasonToExit: true, allowedNextStageKeys: ['active'], colorToken: 'slate', isSystemRequired: true },
    { key: 'lost', label: 'Lost', description: 'Prospect did not convert.', sortOrder: 80, isInitial: false, isActiveCustomerStage: false, isTerminal: true, blocksOrders: true, blocksPortalAccess: true, requiresApprovalToEnter: false, requiresReasonToExit: true, allowedNextStageKeys: [], colorToken: 'slate', isSystemRequired: true },
  ],
  transitionRules: [
    { fromStageKey: 'qualified', toStageKey: 'onboarding', requiresApproval: true, requiredPermission: 'customarr.customer.transition', requiredChecklistTemplateKey: 'default_customer_onboarding', requiredReason: true, blockIfOpenIssues: false, blockIfExpiredRequiredDocuments: false, blockIfMissingRequiredFields: true },
    { fromStageKey: 'onboarding', toStageKey: 'active', requiresApproval: true, requiredPermission: 'customarr.customer.activate', requiredChecklistTemplateKey: 'default_customer_onboarding', requiredReason: false, blockIfOpenIssues: true, blockIfExpiredRequiredDocuments: true, blockIfMissingRequiredFields: true },
  ],
  classificationCatalogs: [
    { catalogType: 'customer_type', key: 'standard', label: 'Standard', description: '', sortOrder: 10, isActive: true, isDefault: true, metadataKey: null, metadataValue: null },
    { catalogType: 'customer_type', key: 'shipper', label: 'Shipper', description: '', sortOrder: 20, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'customer_type', key: 'consignee', label: 'Consignee', description: '', sortOrder: 30, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'customer_type', key: 'broker_customer', label: 'Broker Customer', description: '', sortOrder: 40, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'industry', key: 'general_business', label: 'General Business', description: '', sortOrder: 10, isActive: true, isDefault: true, metadataKey: null, metadataValue: null },
    { catalogType: 'priority_tier', key: 'standard', label: 'Standard', description: '', sortOrder: 10, isActive: true, isDefault: true, metadataKey: null, metadataValue: null },
    { catalogType: 'priority_tier', key: 'core', label: 'Core', description: '', sortOrder: 20, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'priority_tier', key: 'strategic', label: 'Strategic', description: '', sortOrder: 30, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'payment_terms', key: 'prepaid', label: 'Prepaid', description: '', sortOrder: 10, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'payment_terms', key: 'due_on_receipt', label: 'Due On Receipt', description: '', sortOrder: 20, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'payment_terms', key: 'net_15', label: 'Net 15', description: '', sortOrder: 30, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'payment_terms', key: 'net_30', label: 'Net 30', description: '', sortOrder: 40, isActive: true, isDefault: true, metadataKey: null, metadataValue: null },
    { catalogType: 'payment_terms', key: 'net_45', label: 'Net 45', description: '', sortOrder: 50, isActive: true, isDefault: false, metadataKey: null, metadataValue: null },
    { catalogType: 'credit_status', key: 'normal', label: 'Normal', description: '', sortOrder: 10, isActive: true, isDefault: true, metadataKey: null, metadataValue: null },
  ],
  requiredFieldRules: [
    { customerTypeKey: null, lifecycleStageKey: 'active', fieldKey: 'legalName', requirementLevel: 'required', validationMessage: 'Legal name is required before activation.', appliesToPortal: false, appliesToInternalCreate: true, appliesToInternalEdit: true },
    { customerTypeKey: null, lifecycleStageKey: 'active', fieldKey: 'customerType', requirementLevel: 'required', validationMessage: 'Customer type is required before activation.', appliesToPortal: false, appliesToInternalCreate: true, appliesToInternalEdit: true },
    { customerTypeKey: null, lifecycleStageKey: 'active', fieldKey: 'primaryContact', requirementLevel: 'required', validationMessage: 'At least one primary customer contact is required before activation.', appliesToPortal: false, appliesToInternalCreate: true, appliesToInternalEdit: true },
    { customerTypeKey: null, lifecycleStageKey: 'active', fieldKey: 'billingAddress', requirementLevel: 'required', validationMessage: 'A billing address is required before activation.', appliesToPortal: false, appliesToInternalCreate: true, appliesToInternalEdit: true },
    { customerTypeKey: null, lifecycleStageKey: 'active', fieldKey: 'accountOwner', requirementLevel: 'required', validationMessage: 'A StaffArr account owner reference is required before activation.', appliesToPortal: false, appliesToInternalCreate: true, appliesToInternalEdit: true },
  ],
  contactRoles: [
    { key: 'primary', label: 'Primary', description: 'Primary customer contact role.', isRequiredForActiveCustomer: true, requiresUniquePrimary: true, allowsPortalAccess: true, canReceiveOrderNotifications: true, canReceiveBillingNotifications: true, canReceiveComplianceNotifications: true, sortOrder: 10, isActive: true },
    { key: 'billing', label: 'Billing', description: 'Billing customer contact role.', isRequiredForActiveCustomer: true, requiresUniquePrimary: true, allowsPortalAccess: false, canReceiveOrderNotifications: false, canReceiveBillingNotifications: true, canReceiveComplianceNotifications: false, sortOrder: 20, isActive: true },
    { key: 'operations', label: 'Operations', description: 'Operations customer contact role.', isRequiredForActiveCustomer: false, requiresUniquePrimary: false, allowsPortalAccess: true, canReceiveOrderNotifications: true, canReceiveBillingNotifications: false, canReceiveComplianceNotifications: false, sortOrder: 40, isActive: true },
    { key: 'portal_admin', label: 'Portal Admin', description: 'Portal administrator contact role.', isRequiredForActiveCustomer: false, requiresUniquePrimary: true, allowsPortalAccess: true, canReceiveOrderNotifications: true, canReceiveBillingNotifications: true, canReceiveComplianceNotifications: true, sortOrder: 100, isActive: true },
  ],
  addressTypes: [
    { key: 'billing', label: 'Billing', description: 'Billing customer address type.', isRequiredForActiveCustomer: true, requiresValidation: true, requiresGeocode: false, usableForBilling: true, usableForPickup: false, usableForDelivery: false, usableForService: false, sortOrder: 20, isActive: true },
    { key: 'shipping', label: 'Shipping', description: 'Shipping customer address type.', isRequiredForActiveCustomer: false, requiresValidation: true, requiresGeocode: true, usableForBilling: false, usableForPickup: true, usableForDelivery: true, usableForService: true, sortOrder: 30, isActive: true },
    { key: 'pickup', label: 'Pickup', description: 'Pickup customer address type.', isRequiredForActiveCustomer: false, requiresValidation: true, requiresGeocode: true, usableForBilling: false, usableForPickup: true, usableForDelivery: false, usableForService: true, sortOrder: 40, isActive: true },
    { key: 'delivery', label: 'Delivery', description: 'Delivery customer address type.', isRequiredForActiveCustomer: false, requiresValidation: true, requiresGeocode: true, usableForBilling: false, usableForPickup: false, usableForDelivery: true, usableForService: true, sortOrder: 50, isActive: true },
    { key: 'service', label: 'Service', description: 'Service customer address type.', isRequiredForActiveCustomer: false, requiresValidation: true, requiresGeocode: true, usableForBilling: false, usableForPickup: true, usableForDelivery: true, usableForService: true, sortOrder: 60, isActive: true },
  ],
  ownerRules: [
    { ruleName: 'Require account owner for active customers', priority: 10, isActive: true, customerTypeKey: null, territoryKey: null, industryKey: null, sourceKey: null, defaultOwnerType: 'staffarr_person', defaultOwnerRefId: '', defaultOwnerNameSnapshot: 'Select a StaffArr account owner', requiresOwnerForActiveCustomer: true, requiresApprovalForReassignment: true, approvalPermission: 'customarr.customer.owner.reassign' },
  ],
  onboardingTemplates: [
    { key: 'default_customer_onboarding', label: 'Default Customer Onboarding', description: 'Default checklist for activating a customer.', customerTypeKey: null, industryKey: null, priorityTierKey: null, isDefault: true, isActive: true, blocksActivationUntilComplete: true, sortOrder: 10 },
  ],
  onboardingChecklistItems: [
    { templateKey: 'default_customer_onboarding', key: 'confirm_profile', label: 'Confirm customer profile', description: 'Confirm customer profile before activation.', itemType: 'task', required: true, sortOrder: 10, ownerType: null, ownerRefId: null, ownerNameSnapshot: null, documentTypeKey: null, complianceQuestionnaireKey: null, blocksActivation: true, blocksOrders: false, blocksPortalAccess: false },
    { templateKey: 'default_customer_onboarding', key: 'collect_required_documents', label: 'Collect required documents', description: 'Collect required documents before activation.', itemType: 'document', required: true, sortOrder: 30, ownerType: null, ownerRefId: null, ownerNameSnapshot: null, documentTypeKey: 'customer_document', complianceQuestionnaireKey: null, blocksActivation: true, blocksOrders: true, blocksPortalAccess: true },
  ],
  portalSettings: {
    portalEnabled: true,
    inviteOnly: true,
    selfRegistrationAllowed: false,
    requireEmailVerification: true,
    requireInternalApprovalForPortalUsers: true,
    allowedEmailDomains: [],
    supportContactName: 'Customer Success',
    supportContactEmail: 'support@example.com',
    supportContactPhone: '',
    portalDisplayName: 'Customer Portal',
    logoRecordArrDocumentId: null,
    allowedActions: { canViewProfile: true, canRequestQuote: true, canPlaceOrderRequest: false, canUploadDocuments: true, canSubmitIssue: true, canViewOrderStatus: true, canViewInvoicesSnapshot: false },
    defaultPortalContactRoleKey: 'primary',
    portalAdminContactRoleKey: 'portal_admin',
  },
  documentRequirements: [
    { key: 'certificate_of_insurance', label: 'Certificate Of Insurance', description: 'Certificate Of Insurance document requirement.', customerTypeKey: null, lifecycleStageKey: null, required: true, expires: true, expirationWarningDays: 30, recordArrDocumentTypeKey: 'customer_document.coi', customerCanUpload: true, visibleInPortal: true, blocksActivation: true, blocksOrders: true, blocksPortalAccess: false },
    { key: 'tax_registration', label: 'Tax Registration / W-9', description: 'Tax Registration / W-9 document requirement.', customerTypeKey: null, lifecycleStageKey: null, required: true, expires: false, expirationWarningDays: null, recordArrDocumentTypeKey: 'customer_document.tax', customerCanUpload: true, visibleInPortal: true, blocksActivation: true, blocksOrders: false, blocksPortalAccess: false },
  ],
  duplicateDetectionRules: [
    { key: 'external_id_exact', label: 'Exact External ID', isActive: true, priority: 10, matchField: 'externalId', matchType: 'external_id', weight: 100, autoBlockThreshold: 100, reviewThreshold: 70 },
    { key: 'tax_id_exact', label: 'Exact Tax ID', isActive: true, priority: 20, matchField: 'taxId', matchType: 'tax_id', weight: 100, autoBlockThreshold: 100, reviewThreshold: 70 },
    { key: 'email_domain_review', label: 'Email Domain Review', isActive: true, priority: 40, matchField: 'emailDomain', matchType: 'domain', weight: 30, autoBlockThreshold: 999, reviewThreshold: 60 },
  ],
  integrationSettings: { erpSyncMode: 'review_queue', defaultConflictResolution: 'manual_review', emitEventsForDraftCustomers: false, emitEventsForProspects: true, emitEventsOnlyAfterActivation: false, allowExternalCreate: false, allowExternalUpdate: false, requireReviewForExternalUpdate: true },
  externalIdSources: [
    { key: 'erp_customer', label: 'ERP Customer', sourceType: 'erp', required: false, uniqueWithinTenant: true, visibleInUi: true, editableInUi: true, isActive: true },
    { key: 'legacy_customer', label: 'Legacy Customer', sourceType: 'legacy', required: false, uniqueWithinTenant: true, visibleInUi: true, editableInUi: false, isActive: true },
  ],
  notificationRules: [
    { key: 'customer_created_owner', label: 'Customer Created', eventType: 'customer_created', isActive: true, recipientType: 'account_owner', recipientRefId: null, recipientNameSnapshot: null, customerContactRoleKey: null, delayMinutes: 0, escalationAfterMinutes: null, templateKey: null },
    { key: 'document_expiring_owner', label: 'Document Expiring', eventType: 'customer_document_expiring', isActive: true, recipientType: 'account_owner', recipientRefId: null, recipientNameSnapshot: null, customerContactRoleKey: null, delayMinutes: 0, escalationAfterMinutes: 1440, templateKey: null },
  ],
  customFieldDefinitions: [
    { key: 'customer_success_segment_note', label: 'Success Segment Note', description: 'Tenant-defined note used by customer success during onboarding reviews.', fieldType: 'text', appliesToCustomerTypeKey: null, appliesToLifecycleStageKey: null, required: false, visibleInPortal: false, editableInPortal: false, internalOnly: true, sortOrder: 10, isActive: true },
  ],
  customFieldOptions: [],
  warnings: [
    { key: 'active_customer_requirements', message: 'Changes to active-customer required fields can block activation and stage transitions until existing records are remapped or completed.' },
    { key: 'integration_writeback', message: 'External create, update, and bidirectional sync settings affect customer integrations and should be audited before enabling.' },
  ],
}
