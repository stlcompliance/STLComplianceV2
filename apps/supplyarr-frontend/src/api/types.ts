export interface HandoffSessionResponse {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
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
  hasSupplyArrEntitlement: boolean
  entitlements: string[]
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
  createdAt: string
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
  reorderPoint: number | null
  reorderQuantity: number | null
  manufacturerAliases: unknown[]
  vendorLinks: PartVendorLinkResponse[]
  createdAt: string
  updatedAt: string
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
}

export interface CreatePartVendorLinkRequest {
  partyId: string
  vendorPartNumber: string
  isPreferred: boolean
}

export interface InventoryLocationResponse {
  locationId: string
  locationKey: string
  name: string
  locationType: string
  addressLine: string
  status: string
  binCount: number
  createdAt: string
  updatedAt: string
}

export interface InventoryBinResponse {
  binId: string
  locationId: string
  locationKey: string
  binKey: string
  name: string
  status: string
  createdAt: string
  updatedAt: string
}

export interface PartStockLevelResponse {
  stockLevelId: string
  partId: string
  partKey: string
  partDisplayName: string
  binId: string
  binKey: string
  binName: string
  locationId: string
  locationKey: string
  locationName: string
  quantityOnHand: number
  quantityReserved: number
  quantityAvailable: number
  createdAt: string
  updatedAt: string
}

export interface CreateInventoryLocationRequest {
  locationKey: string
  name: string
  locationType: string
  addressLine: string
}

export interface CreateInventoryBinRequest {
  binKey: string
  name: string
}

export interface UpsertPartStockLevelRequest {
  partId: string
  binId: string
  quantityOnHand: number
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
  lines: PurchaseRequestLineResponse[]
  createdAt: string
  updatedAt: string
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

export interface CreatePurchaseOrderFromPurchaseRequestRequest {
  orderKey: string
  title?: string | null
  notes?: string | null
}

export interface CancelPurchaseOrderRequest {
  reason: string
}

export interface ReceivingExceptionResponse {
  receivingExceptionId: string
  receivingReceiptId: string
  receivingReceiptLineId: string
  lineNumber: number
  partKey: string
  exceptionType: string
  quantity: number
  notes: string
  status: string
  createdByUserId: string
  resolvedByUserId: string | null
  resolvedAt: string | null
  createdAt: string
  updatedAt: string
}

export interface ReceivingReceiptLineResponse {
  lineId: string
  lineNumber: number
  purchaseOrderLineId: string
  partId: string
  partKey: string
  partDisplayName: string
  quantityExpected: number
  quantityReceived: number
  quantityOrdered: number
  quantityPreviouslyReceived: number
  quantityRemainingOnOrder: number
  exceptions: ReceivingExceptionResponse[]
  createdAt: string
  updatedAt: string
}

export interface ReceivingReceiptResponse {
  receivingReceiptId: string
  receiptKey: string
  status: string
  purchaseOrderId: string
  purchaseOrderKey: string
  inventoryBinId: string
  binKey: string
  binName: string
  inventoryLocationId: string
  locationKey: string
  locationName: string
  notes: string
  createdByUserId: string
  postedAt: string | null
  postedByUserId: string | null
  lines: ReceivingReceiptLineResponse[]
  exceptions: ReceivingExceptionResponse[]
  createdAt: string
  updatedAt: string
}

export interface CreateReceivingReceiptFromPurchaseOrderRequest {
  receiptKey: string
  inventoryBinId: string
  notes?: string | null
}

export interface UpdateReceivingReceiptLineRequest {
  quantityReceived: number
}

export interface CreateReceivingExceptionRequest {
  exceptionType: string
  quantity: number
  notes?: string | null
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
