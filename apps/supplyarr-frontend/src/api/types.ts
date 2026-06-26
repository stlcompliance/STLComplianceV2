export interface HandoffSessionResponse {
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
  launchableProductKeys: string[]
  themePreference?: string | null
  callbackUrl: string | null
}

export interface SupplyArrMeResponse {
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasSupplyArrAccess: boolean
  launchableProductKeys: string[]
}

export interface SupplyArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasSupplyArrAccess: boolean
  launchableProductKeys: string[]
}

export interface PartyContactResponse {
  contactId: string
  contactName: string
  email: string
  phone: string
  roleLabel: string
  isPrimary: boolean
  createdAt: string
}

export interface PartyRegistryCatalogOptionResponse {
  value: string
  label: string
}

export interface PartyRegistryMetadataResponse {
  approvalStatusOptions: PartyRegistryCatalogOptionResponse[]
  statusOptions: PartyRegistryCatalogOptionResponse[]
}

export interface ExternalPartyResponse {
  partyId: string
  partyKey: string
  partyType: 'vendor' | 'dealer' | 'supplier' | string
  displayName: string
  legalName: string
  taxIdentifier: string | null
  approvalStatus: string
  status: string
  notes: string
  contacts: PartyContactResponse[]
  createdAt: string
  updatedAt: string
}

export interface CreateTypedExternalPartyRequest {
  partyKey: string
  displayName: string
  legalName: string
  taxIdentifier?: string | null
  notes: string
}

export interface UpdateExternalPartyRequest {
  displayName: string
  legalName: string
  taxIdentifier?: string | null
  notes: string
}

export interface UpdateExternalPartyApprovalStatusRequest {
  approvalStatus: string
}

export interface UpdateExternalPartyStatusRequest {
  status: string
}

export interface CreatePartyContactRequest {
  contactName: string
  email: string
  phone: string
  roleLabel: string
  isPrimary: boolean
}

export type PartyRegistryRoute = 'vendors' | 'suppliers' | 'dealers'

export interface PartCatalogResponse {
  catalogId: string
  catalogKey: string
  name: string
  description: string
  status: string
  createdAt: string
  updatedAt: string
}

export interface PartVendorLinkResponse {
  linkId: string
  partyId: string
  partyKey: string
  partyDisplayName: string
  vendorPartNumber: string
  isPreferred: boolean
  catalogUnitPrice: number | null
  catalogCurrencyCode: string | null
  catalogMinimumOrderQuantity: number | null
  catalogLeadTimeDays: number | null
  catalogQuantityAvailable: number | null
  catalogAvailabilityStatus: string | null
  createdAt: string
}

export interface PartSourceResponse {
  sourceId: string
  sourceType: string
  label: string
  notes: string
  createdAt: string
}

export interface VendorCatalogApiSyncItem {
  partKey: string
  vendorPartNumber: string
  isPreferred: boolean
  catalogUnitPrice: number | null
  catalogCurrencyCode: string | null
  catalogMinimumOrderQuantity: number | null
  catalogLeadTimeDays: number | null
  catalogQuantityAvailable: number | null
  catalogAvailabilityStatus: string | null
}

export interface VendorCatalogApiSyncRequest {
  vendorPartyKey: string
  dryRun: boolean
  items: VendorCatalogApiSyncItem[]
}

export interface VendorCatalogApiSyncIssue {
  itemNumber: number
  code: string
  message: string
}

export interface VendorCatalogApiSyncResponse {
  syncType: string
  dryRun: boolean
  success: boolean
  itemsRead: number
  itemsAccepted: number
  itemsApplied: number
  issues: VendorCatalogApiSyncIssue[]
}

export interface PartResponse {
  partId: string
  partKey: string
  catalogId: string | null
  catalogKey: string | null
  displayName: string
  description: string
  categoryKey: string
  unitOfMeasure: string
  manufacturerName: string
  manufacturerPartNumber: string
  status: string
  isTrackable?: boolean
  isStocked?: boolean
  requiresSerialLotTracking?: boolean
  reorderPoint: number | null
  reorderQuantity: number | null
  manufacturerAliases: unknown[]
  sources?: PartSourceResponse[]
  vendorLinks: PartVendorLinkResponse[]
  createdAt: string
  updatedAt: string
}

export interface SubstitutionItemResponse {
  partId: string
  partKey: string
  partDisplayName: string
  aliasId: string
  aliasKey: string
  manufacturerName: string
  manufacturerPartNumber: string
  createdAt: string
}

export interface CreatePartCatalogRequest {
  catalogKey: string
  name: string
  description: string
}

export interface CreatePartRequest {
  partKey: string
  catalogId: string | null
  displayName: string
  description: string
  categoryKey: string
  unitOfMeasure: string
  manufacturerName: string
  manufacturerPartNumber: string
  isTrackable?: boolean | null
  isStocked?: boolean | null
  requiresSerialLotTracking?: boolean
}

export interface CreatePartSourceRequest {
  sourceType: string
  label: string
  notes: string
}

export interface CreatePartVendorLinkRequest {
  partyId: string
  vendorPartNumber: string
  isPreferred: boolean
}

export interface OutboundShipmentLineResponse {
  shipmentLineId: string
  partId: string
  partKey: string
  partDisplayName: string
  fromBinId: string
  fromBinKey: string
  quantityRequested: number
  quantityReserved: number
  quantityPicked: number
  quantityShipped: number
  status: string
}

export interface OutboundShipmentResponse {
  shipmentId: string
  shipmentKey: string
  status: string
  shipVia: string
  destinationName: string
  destinationAddressSnapshot: string
  routarrShipmentIntentId: string | null
  routarrRouteId: string | null
  routarrStatus: string
  idempotencyKey: string
  lines: OutboundShipmentLineResponse[]
  createdAt: string
  updatedAt: string
}

export interface CreateOutboundShipmentLineRequest {
  partId: string
  fromBinId: string
  quantity: number
}

export interface CreateOutboundShipmentRequest {
  idempotencyKey: string
  shipmentKey: string
  shipVia: string
  destinationName: string
  destinationAddressSnapshot: string
  lines: CreateOutboundShipmentLineRequest[]
}

export interface PurchaseRequestLineResponse {
  lineId: string
  lineNumber: number
  partId: string
  partKey: string
  partDisplayName: string
  quantityRequested: number
  unitOfMeasure: string
  notes: string
  createdAt: string
  updatedAt: string
}

export interface PurchaseRequestResponse {
  purchaseRequestId: string
  requestKey: string
  title: string
  notes: string
  status: string
  vendorPartyId: string | null
  vendorPartyKey: string | null
  vendorDisplayName: string | null
  requestedByUserId: string
  submittedAt: string | null
  submittedByUserId: string | null
  approvedAt: string | null
  approvedByUserId: string | null
  rejectedAt: string | null
  rejectedByUserId: string | null
  rejectionReason: string
  isEmergency: boolean
  emergencyReason: string
  emergencyExpeditedAt: string | null
  managerOverrideApproved: boolean
  managerOverrideJustification: string
  managerOverrideApprovedAt: string | null
  lines: PurchaseRequestLineResponse[]
  createdAt: string
  updatedAt: string
}

export interface EmergencyPurchaseResponse {
  purchaseRequestId: string
  requestKey: string
  title: string
  notes: string
  status: string
  vendorPartyId: string | null
  vendorPartyKey: string | null
  vendorDisplayName: string | null
  emergencyReason: string
  emergencyExpeditedAt: string | null
  managerOverrideApproved: boolean
  managerOverrideJustification: string
  managerOverrideApprovedAt: string | null
  linkedPurchaseOrderId: string | null
  linkedPurchaseOrderKey: string | null
  lines: PurchaseRequestLineResponse[]
  createdAt: string
  updatedAt: string
}

export interface IssueEmergencyPurchaseOrderResponse {
  purchaseRequestId: string
  purchaseOrderId: string
  emergencyPurchase: EmergencyPurchaseResponse
  purchaseOrder: PurchaseOrderResponse
}

export interface CreatePurchaseRequestLineRequest {
  partId: string
  quantityRequested: number
  notes: string
}

export interface CreatePurchaseRequestRequest {
  requestKey: string
  title: string
  notes: string
  vendorPartyId: string | null
  lines?: CreatePurchaseRequestLineRequest[]
}

export interface RejectPurchaseRequestRequest {
  reason: string
}

export interface PurchaseOrderLineResponse {
  lineId: string
  lineNumber: number
  purchaseRequestLineId: string | null
  partId: string
  partKey: string
  partDisplayName: string
  quantityOrdered: number
  quantityReceived: number
  quantityRemaining: number
  unitOfMeasure: string
  notes: string
  createdAt: string
  updatedAt: string
}

export interface PurchaseOrderResponse {
  purchaseOrderId: string
  orderKey: string
  title: string
  notes: string
  status: string
  purchaseRequestId: string
  purchaseRequestKey: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  createdByUserId: string
  approvedAt: string | null
  approvedByUserId: string | null
  issuedAt: string | null
  issuedByUserId: string | null
  cancelledAt: string | null
  cancelledByUserId: string | null
  cancellationReason: string
  lines: PurchaseOrderLineResponse[]
  createdAt: string
  updatedAt: string
}

export interface ContractsCsvImportRequest {
  csv: string
  dryRun?: boolean
  fileName?: string | null
}

export interface ContractsCsvImportIssue {
  lineNumber: number
  code: string
  message: string
}

export interface ContractsCsvImportResponse {
  importType: string
  dryRun: boolean
  succeeded: boolean
  rowsRead: number
  contractsAccepted: number
  contractsCreated: number
  issues: ContractsCsvImportIssue[]
}

export interface SupplyContractResponse {
  contractId: string
  contractKey: string
  contractType: string
  title: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  effectiveAt: string
  expiresAt: string | null
  renewalAt: string | null
  paymentTerms: string
  freightTerms: string
  warrantyTerms: string
  minimumSpend: number | null
  serviceLevelAgreement: string
  approvalStatus: string
  status: string
  notes: string
  createdByUserId: string
  createdAt: string
  updatedAt: string
}

export interface CreatePurchaseOrderFromPurchaseRequestRequest {
  orderKey: string
  title?: string | null
  notes?: string | null
}

export interface CancelPurchaseOrderRequest {
  reason: string
}

export interface BackorderResponse {
  backorderId: string
  backorderKey: string
  status: string
  sourceType: string
  purchaseOrderId: string
  purchaseOrderKey: string
  purchaseOrderLineId: string
  purchaseOrderLineNumber: number
  purchaseRequestId: string | null
  purchaseRequestKey: string | null
  purchaseRequestLineId: string | null
  receivingReceiptId: string | null
  receivingReceiptKey: string | null
  receivingReceiptLineId: string | null
  partId: string
  partKey: string
  partDisplayName: string
  quantityBackordered: number
  quantityFulfilled: number
  quantityOpen: number
  expectedBy: string | null
  notes: string
  createdByUserId: string
  fulfilledByUserId: string | null
  fulfilledAt: string | null
  cancelledByUserId: string | null
  cancelledAt: string | null
  cancellationReason: string
  createdAt: string
  updatedAt: string
}

export interface CreateBackorderFromPurchaseOrderLineRequest {
  backorderKey: string
  quantityBackordered?: number | null
  expectedBy?: string | null
  notes?: string | null
}

export interface CancelBackorderRequest {
  reason: string
}

export interface VendorReturnLineResponse {
  lineId: string
  lineNumber: number
  partId: string
  partKey: string
  partDisplayName: string
  purchaseOrderLineId: string | null
  purchaseOrderLineNumber: number | null
  quantity: number
  notes: string
  createdAt: string
  updatedAt: string
}

export interface VendorReturnResponse {
  returnId: string
  returnKey: string
  status: string
  sourceType: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  purchaseOrderId: string | null
  purchaseOrderKey: string | null
  purchaseRequestId: string | null
  purchaseRequestKey: string | null
  inventoryBinId: string
  inventoryBinKey: string
  inventoryBinName: string
  inventoryLocationId: string
  inventoryLocationKey: string
  inventoryLocationName: string
  rmaNumber: string
  notes: string
  createdByUserId: string
  postedByUserId: string | null
  postedAt: string | null
  cancelledByUserId: string | null
  cancelledAt: string | null
  cancellationReason: string
  lines: VendorReturnLineResponse[]
  createdAt: string
  updatedAt: string
}

export interface CreateVendorReturnFromStockLineRequest {
  partId: string
  quantity: number
  notes?: string | null
}

export interface WarrantyClaimResponse {
  warrantyClaimId: string
  claimKey: string
  status: string
  claimType: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  partId: string
  partKey: string
  partDisplayName: string
  purchaseOrderId: string | null
  purchaseOrderKey: string | null
  purchaseOrderLineId: string | null
  receivingReceiptId: string | null
  receivingReceiptKey: string | null
  receivingReceiptLineId: string | null
  quantityClaimed: number
  problemDescription: string
  vendorRmaNumber: string
  vendorDisposition: string
  vendorResponseNotes: string
  closureNotes: string
  denialReason: string
  createdByUserId: string
  submittedByUserId: string | null
  submittedAt: string | null
  vendorRespondedByUserId: string | null
  vendorRespondedAt: string | null
  closedByUserId: string | null
  closedAt: string | null
  deniedByUserId: string | null
  deniedAt: string | null
  cancellationReason: string
  createdAt: string
  updatedAt: string
}

export interface CreateWarrantyClaimRequest {
  claimKey: string
  claimType: string
  vendorPartyId: string
  partId: string
  quantityClaimed: number
  problemDescription: string
  purchaseOrderId?: string | null
  purchaseOrderLineId?: string | null
  receivingReceiptId?: string | null
  receivingReceiptLineId?: string | null
  vendorRmaNumber?: string | null
}

export interface UpdateWarrantyClaimRequest {
  claimType: string
  quantityClaimed: number
  problemDescription: string
  purchaseOrderId?: string | null
  purchaseOrderLineId?: string | null
  receivingReceiptId?: string | null
  receivingReceiptLineId?: string | null
  vendorRmaNumber?: string | null
}

export interface SubmitWarrantyClaimRequest {
  notes?: string | null
}

export interface RecordWarrantyClaimVendorResponseRequest {
  vendorDisposition: string
  vendorResponseNotes: string
  vendorRmaNumber?: string | null
}

export interface CloseWarrantyClaimRequest {
  closureNotes: string
}

export interface DenyWarrantyClaimRequest {
  denialReason: string
}

export interface CancelWarrantyClaimRequest {
  reason: string
}

export interface CreateVendorReturnFromStockRequest {
  returnKey: string
  vendorPartyId: string
  inventoryBinId: string
  rmaNumber?: string | null
  notes?: string | null
  lines: CreateVendorReturnFromStockLineRequest[]
}

export interface CreateVendorReturnFromPurchaseOrderLineRequest {
  returnKey: string
  inventoryBinId: string
  quantity?: number | null
  rmaNumber?: string | null
  notes?: string | null
}

export interface CancelVendorReturnRequest {
  reason: string
}

export interface PricingSnapshotResponse {
  pricingSnapshotId: string
  snapshotKey: string
  partVendorLinkId: string
  partId: string
  partKey: string
  partDisplayName: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  vendorPartNumber: string
  unitPrice: number
  currencyCode: string
  minimumOrderQuantity: number | null
  effectiveFrom: string
  effectiveTo: string | null
  source: string
  notes: string
  isCurrent: boolean
  createdByUserId: string
  createdAt: string
  updatedAt: string
}

export interface CreatePricingSnapshotRequest {
  snapshotKey: string
  partVendorLinkId: string
  unitPrice: number
  currencyCode?: string | null
  minimumOrderQuantity?: number | null
  effectiveFrom?: string | null
  source?: string | null
  notes?: string | null
}

export interface LeadTimeSnapshotResponse {
  leadTimeSnapshotId: string
  snapshotKey: string
  partVendorLinkId: string
  partId: string
  partKey: string
  partDisplayName: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  vendorPartNumber: string
  leadTimeDays: number
  effectiveFrom: string
  effectiveTo: string | null
  source: string
  notes: string
  isCurrent: boolean
  createdByUserId: string
  createdAt: string
  updatedAt: string
}

export interface CreateLeadTimeSnapshotRequest {
  snapshotKey: string
  partVendorLinkId: string
  leadTimeDays: number
  effectiveFrom?: string | null
  source?: string | null
  notes?: string | null
}

export interface AvailabilitySnapshotResponse {
  availabilitySnapshotId: string
  snapshotKey: string
  partVendorLinkId: string
  partId: string
  partKey: string
  partDisplayName: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  vendorPartNumber: string
  quantityAvailable: number | null
  availabilityStatus: string
  effectiveFrom: string
  effectiveTo: string | null
  source: string
  notes: string
  isCurrent: boolean
  createdByUserId: string
  createdAt: string
  updatedAt: string
}

export interface CreateAvailabilitySnapshotRequest {
  snapshotKey: string
  partVendorLinkId: string
  quantityAvailable?: number | null
  availabilityStatus: string
  effectiveFrom?: string | null
  source?: string | null
  notes?: string | null
}

export interface ReorderSuggestionResponse {
  partId: string
  partKey: string
  displayName: string
  unitOfMeasure: string
  reorderPoint: number
  reorderQuantity: number | null
  quantityOnHand: number
  quantityReserved: number
  quantityAvailable: number
  suggestedOrderQuantity: number
  preferredVendorPartyId: string | null
  preferredVendorPartyKey: string | null
  preferredVendorDisplayName: string | null
  hasOpenPurchaseRequest: boolean
  skipReason: string | null
}

export interface ReorderEvaluationResponse {
  evaluatedAt: string
  suggestions: ReorderSuggestionResponse[]
}

export interface UpsertPartReorderPolicyRequest {
  reorderPoint: number | null
  reorderQuantity: number | null
}

export interface PartReorderPolicyResponse {
  partId: string
  partKey: string
  displayName: string
  reorderPoint: number | null
  reorderQuantity: number | null
  updatedAt: string
}

export interface CreatePurchaseRequestFromReorderRequest {
  requestKey: string
  title: string
  notes: string
  partIds: string[]
}

export interface MaintainArrDemandRefLineResponse {
  lineId: string
  lineNumber: number
  maintainarrDemandLineId: string
  partId: string | null
  partNumber: string
  description: string
  quantityRequested: number
  unitOfMeasure: string
  notes: string
}

export interface MaintainArrDemandRefResponse {
  demandRefId: string
  maintainarrPublicationId: string
  maintainarrWorkOrderId: string
  maintainarrWorkOrderNumber: string
  maintainarrAssetId: string
  title: string
  notes: string
  status: string
  procurementStatus: string
  purchaseRequestId: string | null
  purchaseOrderId: string | null
  lastStatusCallbackAt: string | null
  receivedAt: string
  updatedAt: string
  lines: MaintainArrDemandRefLineResponse[]
}

export interface CreatePurchaseRequestFromDemandRefRequest {
  requestKey: string
  title: string
  notes?: string | null
}

export interface ProcurementNotificationSettingsResponse {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnPurchaseRequestSubmitted: boolean
  notifyOnPurchaseRequestApproved: boolean
  notifyOnPurchaseOrderIssued: boolean
  notifyOnReceivingReceiptPosted: boolean
  updatedAt: string | null
}

export interface UpsertProcurementNotificationSettingsRequest {
  isEnabled: boolean
  notificationWebhookUrl: string | null
  notifyOnPurchaseRequestSubmitted: boolean
  notifyOnPurchaseRequestApproved: boolean
  notifyOnPurchaseOrderIssued: boolean
  notifyOnReceivingReceiptPosted: boolean
}

export interface ProcurementNotificationDispatchItem {
  notificationId: string
  eventKind: string
  dispatchStatus: string
  vendorPartyId: string | null
  relatedEntityType: string
  relatedEntityId: string
  webhookHost: string | null
  httpStatusCode: number | null
  errorMessage: string | null
  createdAt: string
  dispatchedAt: string | null
}

export interface ProcurementNotificationDispatchesResponse {
  items: ProcurementNotificationDispatchItem[]
}

export interface PriceSnapshotSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  updatedAt: string | null
}

export interface UpsertPriceSnapshotSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
}

export interface PendingPriceSnapshotCaptureItem {
  partVendorLinkId: string
  partId: string
  partKey: string
  partDisplayName: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  vendorPartNumber: string
  catalogUnitPrice: number
  catalogCurrencyCode: string
  catalogMinimumOrderQuantity: number | null
  currentUnitPrice: number | null
  currentCurrencyCode: string | null
  lastCapturedAt: string | null
}

export interface PendingPriceSnapshotCapturesResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingPriceSnapshotCaptureItem[]
}

export interface PriceSnapshotRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  capturedCount: number
  skippedCount: number
  createdAt: string
}

export interface PriceSnapshotRunsResponse {
  items: PriceSnapshotRunItem[]
}

export interface LeadTimeSnapshotSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  updatedAt: string | null
}

export interface UpsertLeadTimeSnapshotSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
}

export interface PendingLeadTimeSnapshotCaptureItem {
  partVendorLinkId: string
  partId: string
  partKey: string
  partDisplayName: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  vendorPartNumber: string
  catalogLeadTimeDays: number
  currentLeadTimeDays: number | null
  lastCapturedAt: string | null
}

export interface PendingLeadTimeSnapshotCapturesResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingLeadTimeSnapshotCaptureItem[]
}

export interface LeadTimeSnapshotRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  capturedCount: number
  skippedCount: number
  createdAt: string
}

export interface LeadTimeSnapshotRunsResponse {
  items: LeadTimeSnapshotRunItem[]
}

export interface AvailabilitySnapshotSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  updatedAt: string | null
}

export interface UpsertAvailabilitySnapshotSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
}

export interface PendingAvailabilitySnapshotCaptureItem {
  partVendorLinkId: string
  partId: string
  partKey: string
  partDisplayName: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  vendorPartNumber: string
  catalogQuantityAvailable: number | null
  catalogAvailabilityStatus: string | null
  currentQuantityAvailable: number | null
  currentAvailabilityStatus: string | null
  lastCapturedAt: string | null
}

export interface PendingAvailabilitySnapshotCapturesResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingAvailabilitySnapshotCaptureItem[]
}

export interface AvailabilitySnapshotRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  capturedCount: number
  skippedCount: number
  createdAt: string
}

export interface AvailabilitySnapshotRunsResponse {
  items: AvailabilitySnapshotRunItem[]
}

export interface ProcurementCoordinationSummaryResponse {
  coordinationRecordId: string
  subjectType: string
  subjectId: string
  documentKey: string
  title: string
  coordinationStage: string
  nextActionRequired: string
  purchaseRequestId: string | null
  purchaseOrderId: string | null
  vendorPartyId: string | null
  vendorDisplayName: string
  documentStatus: string
  lineCount: number
  quantityOrdered: number
  quantityReceived: number
  receiptProgressPercent: number | null
  isTerminal: boolean
  sourceUpdatedAt: string
  computedAt: string
  isMaterialized: boolean
}

export interface ProcurementCoordinationStageSummaryResponse {
  coordinationStage: string
  count: number
}

export interface ProcurementCoordinationDashboardResponse {
  activeCount: number
  terminalCount: number
  stageCounts: ProcurementCoordinationStageSummaryResponse[]
  items: ProcurementCoordinationSummaryResponse[]
}

export interface ProcurementCoordinationSettingsResponse {
  isEnabled: boolean
  stalenessHours: number
  updatedAt: string | null
}

export interface UpsertProcurementCoordinationSettingsRequest {
  isEnabled: boolean
  stalenessHours: number
}

export interface PendingProcurementCoordinationItem {
  subjectType: string
  subjectId: string
  documentKey: string
  title: string
  documentStatus: string
  sourceUpdatedAt: string
  lastComputedAt: string | null
}

export interface PendingProcurementCoordinationResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingProcurementCoordinationItem[]
}

export interface ProcurementCoordinationRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  refreshedCount: number
  skippedCount: number
  createdAt: string
}

export interface ProcurementCoordinationRunsResponse {
  items: ProcurementCoordinationRunItem[]
}

export interface ApprovalReminderSettingsResponse {
  isEnabled: boolean
  prReminderAfterHours: number
  poReminderAfterHours: number
  reminderCooldownHours: number
  maxRemindersPerSubject: number
  notifyOnPrApprovalReminder: boolean
  notifyOnPoApprovalReminder: boolean
  updatedAt: string | null
}

export interface UpsertApprovalReminderSettingsRequest {
  isEnabled: boolean
  prReminderAfterHours: number
  poReminderAfterHours: number
  reminderCooldownHours: number
  maxRemindersPerSubject: number
  notifyOnPrApprovalReminder: boolean
  notifyOnPoApprovalReminder: boolean
}

export interface PendingApprovalReminderItem {
  subjectType: string
  subjectId: string
  documentKey: string
  title: string
  documentStatus: string
  pendingSince: string
  lastReminderSentAt: string | null
  reminderCount: number
  hoursPending: number
  hoursUntilNextReminder: number
}

export interface PendingApprovalRemindersResponse {
  asOfUtc: string
  batchSize: number
  items: PendingApprovalReminderItem[]
}

export interface ApprovalReminderRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  remindersSentCount: number
  skippedCount: number
  createdAt: string
}

export interface ApprovalReminderRunsResponse {
  items: ApprovalReminderRunItem[]
}

export interface ProcurementExceptionEscalationSettingsResponse {
  isEnabled: boolean
  escalationCooldownHours: number
  maxEscalationsPerException: number
  notifyOnProcurementExceptionSlaEscalation: boolean
  autoCloseCompletedExceptionsEnabled: boolean
  autoCloseCompletedExceptionsAfterHours: number
  updatedAt: string | null
}

export interface UpsertProcurementExceptionEscalationSettingsRequest {
  isEnabled: boolean
  escalationCooldownHours: number
  maxEscalationsPerException: number
  notifyOnProcurementExceptionSlaEscalation: boolean
  autoCloseCompletedExceptionsEnabled: boolean
  autoCloseCompletedExceptionsAfterHours: number
}

export interface PendingProcurementExceptionEscalationItem {
  procurementExceptionId: string
  exceptionKey: string
  subjectType: string
  subjectId: string
  subjectKey: string
  title: string
  status: string
  slaDueAt: string | null
  escalationCount: number
  lastEscalatedAt: string | null
  hoursOverdue: number
  hoursUntilNextEscalation: number
}

export interface PendingProcurementExceptionEscalationsResponse {
  asOfUtc: string
  batchSize: number
  items: PendingProcurementExceptionEscalationItem[]
}

export interface ProcurementExceptionEscalationRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  escalatedCount: number
  skippedCount: number
  createdAt: string
}

export interface ProcurementExceptionEscalationRunsResponse {
  items: ProcurementExceptionEscalationRunItem[]
}

export interface ProcurementExceptionEscalationEventItem {
  eventId: string
  procurementExceptionId: string
  exceptionKey: string
  escalationLevel: number
  actionKind: string
  notificationDispatchId: string | null
  createdAt: string
}

export interface ProcurementExceptionEscalationEventsResponse {
  items: ProcurementExceptionEscalationEventItem[]
}

export interface PendingProcurementExceptionAutoCloseItem {
  procurementExceptionId: string
  exceptionKey: string
  subjectType: string
  subjectId: string
  subjectKey: string
  title: string
  status: string
  resolvedAt: string | null
  waivedAt: string | null
  completedAt: string | null
  hoursCompleted: number
  hoursUntilAutoClose: number
}

export interface PendingProcurementExceptionAutoClosesResponse {
  asOfUtc: string
  batchSize: number
  items: PendingProcurementExceptionAutoCloseItem[]
}

export interface ApprovalReminderSummaryResponse {
  reminderStateId: string
  subjectType: string
  subjectId: string
  documentKey: string
  title: string
  documentStatus: string
  vendorPartyId: string | null
  pendingSince: string
  lastReminderSentAt: string | null
  reminderCount: number
  hoursPending: number
  isOverdue: boolean
}

export interface ApprovalRemindersDashboardResponse {
  overdueCount: number
  pendingCount: number
  items: ApprovalReminderSummaryResponse[]
}

export interface DemandProcessingSettingsResponse {
  isEnabled: boolean
  autoCreatePrDraftWhenShort: boolean
  minHoursBeforeProcessing: number
  stalenessHours: number
  notifyOnPrDraftCreated: boolean
  processMaintainarrDemandRefs: boolean
  processRoutarrDemandRefs: boolean
  processTrainarrDemandRefs: boolean
  processStaffarrDemandRefs: boolean
  updatedAt: string | null
}

export interface IntegrationEventSettingsResponse {
  tenantId: string
  isEnabled: boolean
  maxAttempts: number
  retryIntervalMinutes: number
  updatedAt: string
}

export interface UpsertIntegrationEventSettingsRequest {
  isEnabled: boolean
  maxAttempts: number
  retryIntervalMinutes: number
}

export interface IntegrationEventItemResponse {
  eventId: string
  direction: string
  eventKind: string
  idempotencyKey: string
  sourceProduct: string | null
  relatedEntityType: string
  relatedEntityId: string | null
  processingStatus: string
  attemptCount: number
  errorMessage: string | null
  createdAt: string
  processedAt: string | null
}

export interface IntegrationEventsListResponse {
  items: IntegrationEventItemResponse[]
}

export interface RfqLineResponse {
  lineId: string
  lineNumber: number
  partId: string
  partKey: string
  partDisplayName: string
  quantityRequested: number
  unitOfMeasure: string
  notes: string
  createdAt: string
  updatedAt: string
}

export interface RfqVendorInvitationResponse {
  invitationId: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  status: string
  invitedAt: string
  portalAccessCodeIssuedAt: string
  portalAccessExpiresAt: string
  portalAccessCode: string
  portalUrl: string
}

export interface VendorQuoteLineResponse {
  quoteLineId: string
  rfqLineId: string
  rfqLineNumber: number
  partId: string
  partKey: string
  unitPrice: number
  quantityQuoted: number
  lineTotal: number
  leadTimeDays: number | null
  notes: string
}

export interface VendorQuoteResponse {
  vendorQuoteId: string
  rfqId: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  quoteKey: string
  status: string
  currencyCode: string
  totalAmount: number | null
  leadTimeDays: number | null
  notes: string
  submittedAt: string | null
  lines: VendorQuoteLineResponse[]
  createdAt: string
  updatedAt: string
}

export interface RfqResponse {
  rfqId: string
  rfqKey: string
  title: string
  notes: string
  status: string
  requestedByUserId: string
  submittedAt: string | null
  awardedVendorPartyId: string | null
  awardedVendorDisplayName: string | null
  selectedVendorQuoteId: string | null
  purchaseRequestId: string | null
  awardedAt: string | null
  lines: RfqLineResponse[]
  invitations: RfqVendorInvitationResponse[]
  quotes: VendorQuoteResponse[]
  createdAt: string
  updatedAt: string
}

export interface VendorPortalRfqLineResponse {
  rfqLineId: string
  lineNumber: number
  partId: string
  partKey: string
  partDisplayName: string
  quantityRequested: number
  unitOfMeasure: string
  notes: string
  quoteLineId: string | null
  unitPrice: number | null
  quantityQuoted: number | null
  leadTimeDays: number | null
  quoteNotes: string
}

export interface VendorPortalRfqResponse {
  rfqId: string
  rfqKey: string
  title: string
  notes: string
  status: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  invitationId: string
  invitationStatus: string
  invitedAt: string
  portalAccessExpiresAt: string
  vendorQuoteId: string | null
  quoteKey: string | null
  quoteStatus: string | null
  currencyCode: string | null
  totalAmount: number | null
  leadTimeDays: number | null
  quoteNotes: string | null
  submittedAt: string | null
  lines: VendorPortalRfqLineResponse[]
  createdAt: string
  updatedAt: string
}

export interface VendorOrderStatusUpdateResponse {
  statusUpdateId: string
  previousStatus: string | null
  newStatus: string
  orderedQuantitySnapshot: number
  quantityReady: number
  quantityRemaining: number
  estimatedReadyAt: string | null
  confirmedReadyAt: string | null
  pickupWindowStart: string | null
  pickupWindowEnd: string | null
  note: string | null
  exceptionReason: string | null
  source: string
  submittedByPersonId: string | null
  createdAt: string
}

export interface VendorOrderDocumentResponse {
  documentId: string
  documentType: string
  fileName: string
  contentType: string
  recordArrRecordId: string
  recordArrRecordNumberSnapshot: string
  recordArrFileId: string
  uploadedAt: string
}

export interface VendorOrderBrokerDecisionResponse {
  decisionId: string
  decisionType: string
  authorizedQuantity: number | null
  selectedTripId: string | null
  note: string | null
  decidedByPersonId: string | null
  createdAt: string
}

export interface VendorOrderCatalogOptionResponse {
  value: string
  label: string
  owner: string
  sourceOfTruth: string
}

export interface VendorOrderMetadataResponse {
  filterStatusOptions: VendorOrderCatalogOptionResponse[]
  internalStatusOptions: VendorOrderCatalogOptionResponse[]
  vendorPortalStatusOptions: VendorOrderCatalogOptionResponse[]
  documentTypeOptions: VendorOrderCatalogOptionResponse[]
  brokerDecisionTypeOptions: VendorOrderCatalogOptionResponse[]
}

export interface VendorOrderListItemResponse {
  vendorOrderId: string
  status: string
  vendorNameSnapshot: string
  itemDescription: string
  orderedQuantity: number
  quantityReady: number
  quantityRemaining: number
  quantityUom: string
  expectedReadyAt: string | null
  confirmedReadyAt: string | null
  parentVendorOrderId: string | null
  updatedAt: string
}

export interface VendorOrderResponse {
  vendorOrderId: string
  brokerOrderId: string | null
  brokerOrderNumberSnapshot: string | null
  vendorId: string
  vendorNameSnapshot: string
  vendorLocationId: string | null
  pickupLocationNameSnapshot: string | null
  pickupAddressSnapshot: string
  customerIdSnapshot: string | null
  deliveryLocationNameSnapshot: string | null
  deliveryAddressSnapshot: string | null
  itemDescription: string
  orderedQuantity: number
  quantityReady: number
  quantityRemaining: number
  quantityUom: string
  expectedReadyAt: string | null
  confirmedReadyAt: string | null
  pickupWindowStart: string | null
  pickupWindowEnd: string | null
  pickupInstructions: string | null
  status: string
  createdByPersonId: string | null
  parentVendorOrderId: string | null
  splitReason: string | null
  splitFromStatusUpdateId: string | null
  createdAt: string
  updatedAt: string
  cancelledAt: string | null
  closedAt: string | null
  documents: VendorOrderDocumentResponse[]
  brokerDecisions: VendorOrderBrokerDecisionResponse[]
  statusHistory: VendorOrderStatusUpdateResponse[]
}

export interface CreateVendorOrderRequest {
  brokerOrderId?: string | null
  brokerOrderNumberSnapshot?: string | null
  vendorId: string
  vendorLocationId?: string | null
  pickupLocationNameSnapshot?: string | null
  pickupAddressSnapshot: string
  customerIdSnapshot?: string | null
  deliveryLocationNameSnapshot?: string | null
  deliveryAddressSnapshot?: string | null
  itemDescription: string
  orderedQuantity: number
  quantityUom?: string | null
  expectedReadyAt?: string | null
  pickupWindowStart?: string | null
  pickupWindowEnd?: string | null
  pickupInstructions?: string | null
}

export interface UpdateVendorOrderRequest {
  brokerOrderNumberSnapshot?: string | null
  vendorLocationId?: string | null
  pickupLocationNameSnapshot?: string | null
  pickupAddressSnapshot: string
  customerIdSnapshot?: string | null
  deliveryLocationNameSnapshot?: string | null
  deliveryAddressSnapshot?: string | null
  itemDescription: string
  orderedQuantity: number
  quantityUom?: string | null
  expectedReadyAt?: string | null
  pickupWindowStart?: string | null
  pickupWindowEnd?: string | null
  pickupInstructions?: string | null
}

export interface UpdateVendorOrderStatusRequest {
  newStatus: string
  quantityReady?: number | null
  estimatedReadyAt?: string | null
  confirmedReadyAt?: string | null
  pickupWindowStart?: string | null
  pickupWindowEnd?: string | null
  note?: string | null
  exceptionReason?: string | null
  readyForPickupConfirmed: boolean
}

export interface SendVendorOrderResponse {
  vendorOrder: VendorOrderResponse
  magicLinkUrl: string
  expiresAt: string
}

export interface CreateVendorOrderMagicLinkResponse {
  magicLinkId: string
  token: string
  url: string
  expiresAt: string
}

export interface RegisterVendorOrderDocumentRequest {
  documentType: string
  fileName: string
  contentType: string
  storageProvider?: string | null
  storageKey?: string | null
  sizeBytes?: number | null
  pageCount?: number | null
  imageWidth?: number | null
  imageHeight?: number | null
  durationSeconds?: number | null
}

export interface VendorOrderPortalResponse {
  vendorOrderId: string
  status: string
  vendorNameSnapshot: string
  pickupLocationNameSnapshot: string
  pickupAddressSnapshot: string
  deliveryLocationNameSnapshot: string | null
  deliveryAddressSnapshot: string | null
  itemDescription: string
  orderedQuantity: number
  quantityReady: number
  quantityRemaining: number
  quantityUom: string
  expectedReadyAt: string | null
  confirmedReadyAt: string | null
  pickupWindowStart: string | null
  pickupWindowEnd: string | null
  pickupInstructions: string | null
  linkExpiresAt: string
  documents: VendorOrderDocumentResponse[]
  statusHistory: VendorOrderStatusUpdateResponse[]
  metadata: VendorOrderMetadataResponse
}

export interface CreateVendorOrderBrokerDecisionRequest {
  decisionType: string
  authorizedQuantity?: number | null
  selectedTripId?: string | null
  note?: string | null
}

export interface SplitVendorOrderRequest {
  selectedTripId?: string | null
  splitReason?: string | null
  remainingExpectedReadyAt?: string | null
  remainingPickupWindowStart?: string | null
  remainingPickupWindowEnd?: string | null
}

export interface SplitVendorOrderResponse {
  parentVendorOrder: VendorOrderResponse
  readyVendorOrder: VendorOrderResponse
  remainingVendorOrder: VendorOrderResponse
  remainingVendorOrderToken: string
  remainingVendorOrderUrl: string
}

export interface VendorOrderSettingsResponse {
  allowDestinationSummaryInVendorPortal: boolean
  magicLinkTtlHours: number
  updatedAt: string | null
}

export interface UpsertVendorOrderSettingsRequest {
  allowDestinationSummaryInVendorPortal: boolean
  magicLinkTtlHours: number
}

export interface RfqQuoteLineMetric {
  vendorQuoteId: string
  vendorPartyId: string
  vendorDisplayName: string
  quoteStatus: string
  unitPrice: number | null
  lineTotal: number | null
  leadTimeDays: number | null
  isLowestPrice: boolean
  isFastestLeadTime: boolean
}

export interface RfqLineComparisonRow {
  rfqLineId: string
  lineNumber: number
  partId: string
  partKey: string
  partDisplayName: string
  quantityRequested: number
  quotes: RfqQuoteLineMetric[]
}

export interface RfqQuoteSummary {
  vendorQuoteId: string
  vendorPartyId: string
  vendorDisplayName: string
  status: string
  totalAmount: number | null
  maxLeadTimeDays: number | null
  linesQuoted: number
  isSelected: boolean
}

export interface RfqQuoteComparisonResponse {
  rfqId: string
  rfqKey: string
  status: string
  lines: RfqLineComparisonRow[]
  quoteSummaries: RfqQuoteSummary[]
}

export interface CreatePurchaseRequestFromRfqResponse {
  rfqId: string
  purchaseRequestId: string
  purchaseRequest: PurchaseRequestResponse
}

export interface VendorPortalCreateQuoteRequest {
  quoteKey: string
  currencyCode: string
  notes: string
}

export interface OnboardingDocumentRequirementStatus {
  documentTypeKey: string
  label: string
  isRequired: boolean
  isSatisfied: boolean
  satisfyingDocumentId: string | null
  satisfyingReviewStatus: string | null
}

export interface VendorRestrictionResponse {
  restrictionId: string
  externalPartyId: string
  partyKey: string
  partyDisplayName: string
  partyType: string
  restrictionKey: string
  scopes: string[]
  reason: string
  status: string
  effectiveFrom: string
  effectiveUntil: string | null
  createdByUserId: string
  liftedByUserId: string | null
  liftedAt: string | null
  liftNotes: string | null
  createdAt: string
  updatedAt: string
}

export interface CreateVendorRestrictionRequest {
  restrictionKey: string
  scopes: string[]
  reason: string
  effectiveFrom?: string | null
  effectiveUntil?: string | null
}

export interface LiftVendorRestrictionRequest {
  liftNotes?: string | null
}

export interface VendorRestrictionEnforcementResponse {
  externalPartyId: string
  isBlocked: boolean
  blockReason: string | null
  activeScopes: string[]
}

export interface SupplierIncidentResponse {
  incidentId: string
  externalPartyId: string
  partyKey: string
  partyDisplayName: string
  partyType: string
  incidentKey: string
  title: string
  description: string
  incidentType: string
  severity: string
  status: string
  purchaseRequestId: string | null
  purchaseOrderId: string | null
  receivingReceiptId: string | null
  receivingExceptionId: string | null
  vendorRestrictionId: string | null
  reportedByUserId: string
  assignedToUserId: string | null
  resolutionNotes: string
  resolvedByUserId: string | null
  resolvedAt: string | null
  closedByUserId: string | null
  closedAt: string | null
  cancellationReason: string
  cancelledByUserId: string | null
  cancelledAt: string | null
  reopenedByUserId: string | null
  reopenedAt: string | null
  lastReopenReason: string
  reopenCount: number
  createdAt: string
  updatedAt: string
}

export interface CreateSupplierIncidentRequest {
  externalPartyId: string
  incidentKey: string
  title: string
  description: string
  incidentType: string
  severity: string
  purchaseRequestId?: string | null
  purchaseOrderId?: string | null
  receivingReceiptId?: string | null
  receivingExceptionId?: string | null
  assignedToUserId?: string | null
}

export interface ResolveSupplierIncidentRequest {
  resolutionNotes: string
}

export interface CancelSupplierIncidentRequest {
  reason: string
}

export interface ReopenSupplierIncidentRequest {
  reason: string
}

export interface ApplySupplierIncidentProcurementRestrictionRequest {
  restrictionKey: string
  scopes: string[]
  reason?: string | null
}

export interface ProcurementExceptionResponse {
  exceptionId: string
  exceptionKey: string
  subjectType: string
  subjectId: string
  subjectKey: string
  vendorPartyId: string | null
  vendorPartyKey: string | null
  vendorDisplayName: string | null
  exceptionCategory: string
  title: string
  description: string
  status: string
  resolutionNotes: string
  waiveJustification: string
  waiveRejectionReason: string
  createdByUserId: string
  assignedToUserId: string | null
  slaDueAt: string | null
  isSlaBreached: boolean
  resolutionTemplateKey: string
  linkedPurchaseRequestId: string | null
  linkedPurchaseRequestKey: string | null
  linkedPurchaseOrderId: string | null
  linkedPurchaseOrderKey: string | null
  waiveRequestedByUserId: string | null
  waiveRequestedAt: string | null
  waivedByUserId: string | null
  waivedAt: string | null
  resolvedAt: string | null
  closedAt: string | null
  cancelledAt: string | null
  cancellationReason: string
  reopenedByUserId: string | null
  reopenedAt: string | null
  lastReopenReason: string
  reopenCount: number
  createdAt: string
  updatedAt: string
}

export interface CreateProcurementExceptionRequest {
  exceptionKey: string
  exceptionCategory: string
  title: string
  description: string
  assignedToUserId?: string | null
  slaDueAt?: string | null
}

export interface ProcurementExceptionResolutionTemplateResponse {
  templateKey: string
  label: string
  defaultResolutionNotes: string
}

export interface AssignProcurementExceptionRequest {
  assignedToUserId: string
  slaDueAt?: string | null
}

export interface LinkProcurementExceptionActionsRequest {
  linkedPurchaseRequestId?: string | null
  linkedPurchaseOrderId?: string | null
}

export interface ResolveProcurementExceptionRequest {
  resolutionNotes: string
  resolutionTemplateKey?: string | null
}

export interface RequestProcurementExceptionWaiveRequest {
  waiveJustification: string
}

export interface RejectProcurementExceptionWaiveRequest {
  reason: string
}

export interface CloseProcurementExceptionRequest {
  resolutionNotes?: string | null
}

export interface CancelProcurementExceptionRequest {
  reason: string
}

export interface ReopenProcurementExceptionRequest {
  reason: string
}

export interface ProcurementApprovalAuthorityGrantMirror {
  permissionKey: string
  permissionName: string
  scopeType: string
  scopeValue: string | null
  roleKey: string
  roleName: string
}

export interface ProcurementApprovalAuthorityMirrorResponse {
  staffarrPersonId: string
  externalUserId: string
  canSubmitPurchaseRequests: boolean
  canApprovePurchaseRequests: boolean
  canIssuePurchaseOrders: boolean
  maxSubmitAmount: number | null
  maxApproveAmount: number | null
  maxIssueAmount: number | null
  orgUnitScopeIds: string[]
  grants: ProcurementApprovalAuthorityGrantMirror[]
  sourceComputedAt: string
  refreshedAt: string
  authoritySource: string
}

export interface SupplierOnboardingResponse {
  onboardingId: string
  externalPartyId: string
  partyKey: string
  partyType: string
  displayName: string
  onboardingStatus: string
  notes: string
  submittedAt: string | null
  reviewedAt: string | null
  rejectionReason: string
  documentRequirements: OnboardingDocumentRequirementStatus[]
  createdAt: string
  updatedAt: string
}

export interface OnboardingDocumentRequirementDefinition {
  documentTypeKey: string
  label: string
  isRequired: boolean
}

export interface SupplierOnboardingDocumentRequirementsResponse {
  requirements: OnboardingDocumentRequirementDefinition[]
}

export interface PartyComplianceDocumentResponse {
  documentId: string
  externalPartyId: string
  documentKey: string
  documentTypeKey: string
  title: string
  version: number
  reviewStatus: string
  expiresAt: string | null
  effectiveAt: string | null
  fileName: string
  contentType: string
  sizeBytes: number
  notes: string
  createdAt: string
  updatedAt: string
}

export interface UpsertDemandProcessingSettingsRequest {
  isEnabled: boolean
  autoCreatePrDraftWhenShort: boolean
  minHoursBeforeProcessing: number
  stalenessHours: number
  notifyOnPrDraftCreated: boolean
  processMaintainarrDemandRefs: boolean
  processRoutarrDemandRefs: boolean
  processTrainarrDemandRefs: boolean
  processStaffarrDemandRefs: boolean
}

export interface PendingDemandProcessingItem {
  demandRefId: string
  demandRefSource: string
  sourceRefKey: string
  title: string
  receivedAt: string
  lastProcessedAt: string | null
  lastProcessingOutcome: string | null
}

export interface PendingDemandProcessingResponse {
  asOfUtc: string
  stalenessHours: number
  batchSize: number
  items: PendingDemandProcessingItem[]
}

export interface DemandProcessingRunItem {
  runId: string
  asOfUtc: string
  candidatesFound: number
  processedCount: number
  prDraftsCreatedCount: number
  skippedCount: number
  createdAt: string
}

export interface DemandProcessingRunsResponse {
  items: DemandProcessingRunItem[]
}

export interface DemandProcessingSourceLinkResponse {
  productKey: string
  displayLabel: string
  referenceKey: string
}

export interface DemandProcessingSummaryResponse {
  processingStateId: string | null
  demandRefId: string
  demandRefSource: string
  sourceRefKey: string
  title: string
  demandRefStatus: string
  processingOutcome: string | null
  recommendedAction: string | null
  linesTotalCount: number | null
  linesCatalogCount: number | null
  linesShortCount: number | null
  purchaseRequestId: string | null
  lastProcessingMessage: string | null
  demandReceivedAt: string
  lastProcessedAt: string | null
  sourceLink: DemandProcessingSourceLinkResponse
}

export interface DemandProcessingLineSummary {
  lineId: string
  lineNumber: number
  partId: string | null
  partNumber: string
  quantityRequested: number
  quantityAvailable: number
  isShort: boolean
}

export interface DemandProcessingDetailResponse {
  summary: DemandProcessingSummaryResponse
  lines: DemandProcessingLineSummary[]
}

export interface DemandProcessingOperatorActionResponse {
  action: string
  result: {
    demandRefId: string
    demandRefSource: string
    sourceRefKey: string
    processingOutcome: string
    recommendedAction: string
    linesShortCount: number
    purchaseRequestId: string | null
    notificationDispatchId: string | null
  }
  detail: DemandProcessingDetailResponse
}

export interface DemandProcessingDashboardResponse {
  pendingCount: number
  stockShortCount: number
  stockAvailableCount: number
  prDraftedCount: number
  processedItems: DemandProcessingSummaryResponse[]
  pendingItems: DemandProcessingSummaryResponse[]
}

export interface SupplyReadinessTotalsResponse {
  activePartsCount: number
  partsBelowReorderCount: number
  stockLineCount: number
  totalQuantityOnHand: number
  totalQuantityReserved: number
  totalQuantityAvailable: number
  openBackorderCount: number
  openPurchaseRequestCount: number
  openPurchaseOrderCount: number
  issuedPurchaseOrderCount: number
  openDemandRefCount: number
  complianceAttentionCount: number
  activeVendorRestrictionCount: number
  activeProcurementExceptionCount: number
}

export interface SupplyReadinessDemandRefSourceCountResponse {
  source: string
  openCount: number
}

export interface SupplyReadinessAttentionItemResponse {
  category: string
  title: string
  detail: string
  status: string | null
  occurredAt: string | null
  relatedEntityType: string | null
  relatedEntityId: string | null
}

export interface SupplyReadinessPredictiveStockoutResponse {
  partId: string
  partKey: string
  displayName: string
  quantityAvailable: number
  openDemandQuantity: number
  openBackorderQuantity: number
  projectedQuantity: number
  shortageQuantity: number
  reorderPoint: number | null
  riskLevel: string
  reason: string
  sourceTimestamp: string | null
}

export interface SupplyReadinessDashboardResponse {
  generatedAt: string
  totals: SupplyReadinessTotalsResponse
  demandRefsBySource: SupplyReadinessDemandRefSourceCountResponse[]
  attentionItems: SupplyReadinessAttentionItemResponse[]
  predictiveStockoutItems: SupplyReadinessPredictiveStockoutResponse[]
}

export interface SupplyReadinessBlockerResponse {
  reasonCode: string
  message: string
  sourceEntityType: string
  sourceEntityId: string
  relatedEntityId: string | null
}

export interface SupplyReadinessAvailabilitySnapshotResponse {
  quantityOnHand: number
  quantityReserved: number
  quantityAvailable: number
  reorderPoint: number | null
  activeReservationCount: number
  openBackorderCount: number
}

export interface PartSupplyReadinessResponse {
  partId: string
  partKey: string
  displayName: string
  status: string
  readinessStatus: string
  readinessBasis: string
  calculatedAt: string
  blockers: SupplyReadinessBlockerResponse[]
  availability: SupplyReadinessAvailabilitySnapshotResponse
}

export interface VendorSupplyReadinessResponse {
  externalPartyId: string
  partyKey: string
  displayName: string
  partyType: string
  approvalStatus: string
  status: string
  readinessStatus: string
  readinessBasis: string
  calculatedAt: string
  blockers: SupplyReadinessBlockerResponse[]
}

export interface ProcurementPathReadinessResponse {
  partId: string
  partKey: string
  externalPartyId: string
  partyKey: string
  requestedQuantity: number | null
  readinessStatus: string
  readinessBasis: string
  calculatedAt: string
  blockers: SupplyReadinessBlockerResponse[]
}

export interface VendorApprovalStatusSummary {
  approvalStatus: string
  count: number
}

export interface VendorReportSummaryItem {
  vendorPartyId: string
  partyKey: string
  displayName: string
  approvalStatus: string
  status: string
  partVendorLinkCount: number
  preferredPartLinkCount: number
  openPurchaseRequestCount: number
  openPurchaseOrderCount: number
  issuedPurchaseOrderCount: number
  postedReceivingReceiptCount: number
  openBackorderCount: number
  openPurchaseOrderLineQuantity: number
  averageLeadTimeDays: number | null
  leadTimeSampleCount: number
  onTimeDeliveryRate: number | null
  onTimeDeliverySampleCount: number
  lastPurchaseOrderAt: string | null
  lastReceivingPostedAt: string | null
}

export interface VendorReportSummaryResponse {
  generatedAt: string
  approvalStatusCounts: VendorApprovalStatusSummary[]
  vendors: VendorReportSummaryItem[]
}

export interface PartsInventoryReportTotals {
  totalPartCount: number
  activePartCount: number
  locationCount: number
  binCount: number
  belowReorderPointCount: number
  zeroStockPartCount: number
  totalQuantityOnHand: number
  totalQuantityReserved: number
  totalQuantityAvailable: number
}

export interface PartsInventoryLocationSummaryItem {
  inventoryLocationId: string
  locationKey: string
  name: string
  status: string
  binCount: number
  partCountWithStock: number
  quantityOnHand: number
  quantityReserved: number
  quantityAvailable: number
}

export interface PartsInventoryPartSummaryItem {
  partId: string
  partKey: string
  displayName: string
  status: string
  categoryKey: string
  reorderPoint: number | null
  reorderQuantity: number | null
  quantityOnHand: number
  quantityReserved: number
  quantityAvailable: number
  belowReorderPoint: boolean
  vendorLinkCount: number
}

export interface PartsInventoryReportSummaryResponse {
  generatedAt: string
  totals: PartsInventoryReportTotals
  locations: PartsInventoryLocationSummaryItem[]
  parts: PartsInventoryPartSummaryItem[]
}

export interface PartsInventoryPartDetailResponse {
  summary: PartsInventoryPartSummaryItem
  stockByBin: Array<{
    partStockLevelId: string
    inventoryBinId: string
    binKey: string
    binName: string
    inventoryLocationId: string
    locationKey: string
    locationName: string
    quantityOnHand: number
    quantityReserved: number
    quantityAvailable: number
  }>
  vendorLinks: Array<{
    partVendorLinkId: string
    vendorPartyId: string
    vendorPartyKey: string
    vendorDisplayName: string
    vendorPartNumber: string
    isPreferred: boolean
  }>
}

export interface PartsInventoryLocationDetailResponse {
  summary: PartsInventoryLocationSummaryItem
  bins: Array<{
    inventoryBinId: string
    binKey: string
    binName: string
    status: string
    partCountWithStock: number
    quantityOnHand: number
    quantityReserved: number
  }>
  parts: Array<{
    partId: string
    partKey: string
    displayName: string
    quantityOnHand: number
    quantityReserved: number
    quantityAvailable: number
  }>
}

export interface VendorReportDetailResponse {
  summary: VendorReportSummaryItem
  recentPurchaseRequests: Array<{
    purchaseRequestId: string
    requestKey: string
    title: string
    status: string
    updatedAt: string
  }>
  recentPurchaseOrders: Array<{
    purchaseOrderId: string
    orderKey: string
    title: string
    status: string
    lineCount: number
    quantityOrdered: number
    quantityReceived: number
    updatedAt: string
  }>
  partLinks: Array<{
    partVendorLinkId: string
    partId: string
    partKey: string
    partDisplayName: string
    vendorPartNumber: string
    isPreferred: boolean
    catalogUnitPrice: number | null
    catalogAvailabilityStatus: string | null
  }>
}

export interface PurchasingStatusCount {
  status: string
  count: number
}

export interface PurchasingReportTotals {
  purchaseRequestCount: number
  openPurchaseRequestCount: number
  purchaseOrderCount: number
  openPurchaseOrderCount: number
  issuedPurchaseOrderCount: number
  draftReceivingReceiptCount: number
  postedReceivingReceiptCount: number
  openBackorderCount: number
  openPurchaseOrderLineQuantity: number
  purchaseOrderQuantityReceived: number
}

export interface PurchasingProcurementAnalytics {
  pendingPurchaseRequestCount: number
  emergencyPurchaseRequestCount: number
  activeProcurementExceptionCount: number
  openReceivingExceptionCount: number
  openWarrantyClaimCount: number
  vendorDocumentExpiringSoonCount: number
  blockedVendorCount: number
  averageLeadTimeDays: number | null
  estimatedSpendThisMonth: number
}

export interface PurchasingDocumentSummaryItem {
  documentType: 'purchase_request' | 'purchase_order' | string
  documentId: string
  documentKey: string
  title: string
  status: string
  vendorPartyId: string | null
  vendorDisplayName: string
  lineCount: number
  quantityOrdered: number
  quantityReceived: number
  updatedAt: string
}

export interface PurchasingReportSummaryResponse {
  generatedAt: string
  totals: PurchasingReportTotals
  analytics: PurchasingProcurementAnalytics
  purchaseRequestStatusCounts: PurchasingStatusCount[]
  purchaseOrderStatusCounts: PurchasingStatusCount[]
  documents: PurchasingDocumentSummaryItem[]
}

export interface PurchasingPurchaseRequestDetailResponse {
  summary: PurchasingDocumentSummaryItem
  lines: Array<{
    lineId: string
    lineNumber: number
    partId: string
    partKey: string
    partDisplayName: string
    quantityRequested: number
    unitOfMeasure: string
  }>
  linkedPurchaseOrderId: string | null
  linkedPurchaseOrderKey: string | null
}

export interface ComplianceReportTotals {
  partyCount: number
  documentCount: number
  expiredCount: number
  expiringSoonCount: number
  reviewPendingCount: number
  approvedCount: number
  rejectedCount: number
}

export interface CompliancePartySummaryItem {
  externalPartyId: string
  partyKey: string
  displayName: string
  partyType: string
  approvalStatus: string
  compliancePosture: string
  documentCount: number
  expiredCount: number
  expiringSoonCount: number
  reviewPendingCount: number
}

export interface ComplianceDocumentSummaryItem {
  documentId: string
  externalPartyId: string
  partyKey: string
  partyDisplayName: string
  partyType: string
  documentKey: string
  documentTypeKey: string
  title: string
  version: number
  reviewStatus: string
  effectiveStatus: string
  isExpired: boolean
  isExpiringSoon: boolean
  expiresAt: string | null
  updatedAt: string
}

export interface ComplianceReportSummaryResponse {
  generatedAt: string
  totals: ComplianceReportTotals
  parties: CompliancePartySummaryItem[]
  documents: ComplianceDocumentSummaryItem[]
}

export interface ForgivingSearchResultItem {
  entityType: string
  entityId: string
  primaryKey: string
  title: string
  subtitle: string
  deepLinkPath: string
  matchScore: number
}

export interface AuditHistoryItem {
  id: string
  actorUserId: string | null
  action: string
  targetType: string
  targetId: string | null
  result: string
  reasonCode: string | null
  correlationId: string
  occurredAt: string
}

export interface AuditHistoryListResponse {
  items: AuditHistoryItem[]
  nextCursor: string | null
  hasMore: boolean
}

export interface ForgivingSearchResponse {
  query: string
  normalizedQuery: string
  totalCount: number
  results: ForgivingSearchResultItem[]
}

export interface CompliancePartyDetailResponse {
  summary: CompliancePartySummaryItem
  documents: Array<{
    documentId: string
    documentKey: string
    documentTypeKey: string
    title: string
    version: number
    reviewStatus: string
    effectiveStatus: string
    isExpired: boolean
    isExpiringSoon: boolean
    expiresAt: string | null
    effectiveAt: string | null
    fileName: string
    contentType: string
    sizeBytes: number
    notes: string
    reviewedAt: string | null
    createdAt: string
    updatedAt: string
  }>
}

export interface PurchasingPurchaseOrderDetailResponse {
  summary: PurchasingDocumentSummaryItem
  lines: Array<{
    lineId: string
    lineNumber: number
    partId: string
    partKey: string
    partDisplayName: string
    quantityOrdered: number
    quantityReceived: number
    quantityRemaining: number
  }>
  receivingReceipts: Array<{
    receivingReceiptId: string
    receiptKey: string
    status: string
    postedAt: string | null
  }>
  backorders: Array<{
    backorderId: string
    backorderKey: string
    status: string
    quantityBackordered: number
    quantityFulfilled: number
  }>
}

export interface VendorEmailInboxMessageResponse {
  messageId: string
  messageKey: string
  messageKind: string
  senderEmail: string
  senderName: string
  subject: string
  bodyPreview: string
  matchStatus: string
  matchReason: string
  vendorPartyId: string | null
  vendorPartyKey: string | null
  vendorDisplayName: string | null
  linkedReferenceType: string | null
  linkedReferenceId: string | null
  linkedReferenceKey: string | null
  receivedAt: string
  createdAt: string
  updatedAt: string
  processedAt: string | null
}

export interface VendorEmailInboxListResponse {
  items: VendorEmailInboxMessageResponse[]
}

export interface IngestVendorEmailInboxRequest {
  messageKey: string
  messageKind: string
  senderEmail: string
  senderName: string
  subject: string
  body: string
  referenceKey?: string | null
}

export interface IngestVendorEmailInboxResponse {
  wasDuplicate: boolean
  message: VendorEmailInboxMessageResponse
}

