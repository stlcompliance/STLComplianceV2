import type {
  CustomArrCreateCustomerRequest,
  CustomArrCrmOverview,
  CustomArrCrmRecord,
  CustomArrCustomerDetail,
  CustomArrCustomerSummary,
  CustomArrRequirementCatalogItem,
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
