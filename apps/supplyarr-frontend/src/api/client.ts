import type {
  CreateInventoryBinRequest,
  CreateInventoryLocationRequest,
  CreatePartCatalogRequest,
  CreatePartRequest,
  CreatePartVendorLinkRequest,
  CreatePartyContactRequest,
  CreateTypedExternalPartyRequest,
  ExternalPartyResponse,
  PartyRegistryRoute,
  UpdateExternalPartyApprovalStatusRequest,
  UpdateExternalPartyRequest,
  UpdateExternalPartyStatusRequest,
  HandoffSessionResponse,
  InventoryBinResponse,
  InventoryLocationResponse,
  PartCatalogResponse,
  PartResponse,
  PartStockLevelResponse,
  StockReservationResponse,
  CreateStockReservationRequest,
  ReleaseStockReservationRequest,
  PartVendorLinkResponse,
  SupplyArrMeResponse,
  SupplyArrSessionBootstrapResponse,
  UpsertPartStockLevelRequest,
  CreatePurchaseOrderFromPurchaseRequestRequest,
  CreatePurchaseRequestRequest,
  BackorderResponse,
  CancelBackorderRequest,
  CancelPurchaseOrderRequest,
  CancelVendorReturnRequest,
  CreateBackorderFromPurchaseOrderLineRequest,
  CreateVendorReturnFromPurchaseOrderLineRequest,
  CreateVendorReturnFromStockRequest,
  VendorReturnResponse,
  WarrantyClaimResponse,
  CreateWarrantyClaimRequest,
  UpdateWarrantyClaimRequest,
  SubmitWarrantyClaimRequest,
  RecordWarrantyClaimVendorResponseRequest,
  CloseWarrantyClaimRequest,
  DenyWarrantyClaimRequest,
  CancelWarrantyClaimRequest,
  CreateReceivingExceptionRequest,
  CreateReceivingReceiptFromPurchaseOrderRequest,
  PurchaseOrderResponse,
  PurchaseRequestResponse,
  ReceivingExceptionResponse,
  ReceivingReceiptResponse,
  RejectPurchaseRequestRequest,
  UpdateReceivingReceiptLineRequest,
  PricingSnapshotResponse,
  CreatePricingSnapshotRequest,
  LeadTimeSnapshotResponse,
  CreateLeadTimeSnapshotRequest,
  AvailabilitySnapshotResponse,
  CreateAvailabilitySnapshotRequest,
  ReorderEvaluationResponse,
  UpsertPartReorderPolicyRequest,
  PartReorderPolicyResponse,
  CreatePurchaseRequestFromReorderRequest,
  MaintainArrDemandRefResponse,
  CreatePurchaseRequestFromDemandRefRequest,
  ProcurementNotificationSettingsResponse,
  UpsertProcurementNotificationSettingsRequest,
  ProcurementNotificationDispatchesResponse,
  PriceSnapshotSettingsResponse,
  UpsertPriceSnapshotSettingsRequest,
  PendingPriceSnapshotCapturesResponse,
  PriceSnapshotRunsResponse,
  LeadTimeSnapshotSettingsResponse,
  UpsertLeadTimeSnapshotSettingsRequest,
  PendingLeadTimeSnapshotCapturesResponse,
  LeadTimeSnapshotRunsResponse,
  AvailabilitySnapshotSettingsResponse,
  UpsertAvailabilitySnapshotSettingsRequest,
  PendingAvailabilitySnapshotCapturesResponse,
  AvailabilitySnapshotRunsResponse,
  ProcurementCoordinationDashboardResponse,
  ProcurementCoordinationSettingsResponse,
  UpsertProcurementCoordinationSettingsRequest,
  PendingProcurementCoordinationResponse,
  ProcurementCoordinationRunsResponse,
  ApprovalReminderSettingsResponse,
  UpsertApprovalReminderSettingsRequest,
  PendingApprovalRemindersResponse,
  ApprovalReminderRunsResponse,
  ApprovalRemindersDashboardResponse,
  ProcurementExceptionEscalationSettingsResponse,
  UpsertProcurementExceptionEscalationSettingsRequest,
  PendingProcurementExceptionEscalationsResponse,
  ProcurementExceptionEscalationRunsResponse,
  ProcurementExceptionEscalationEventsResponse,
  DemandProcessingSettingsResponse,
  UpsertDemandProcessingSettingsRequest,
  PendingDemandProcessingResponse,
  DemandProcessingRunsResponse,
  DemandProcessingDashboardResponse,
  DemandProcessingDetailResponse,
  DemandProcessingOperatorActionResponse,
  SupplyReadinessDashboardResponse,
  PartSupplyReadinessResponse,
  VendorSupplyReadinessResponse,
  ProcurementPathReadinessResponse,
  IntegrationEventSettingsResponse,
  UpsertIntegrationEventSettingsRequest,
  IntegrationEventsListResponse,
  RfqResponse,
  RfqQuoteComparisonResponse,
  VendorQuoteResponse,
  CreatePurchaseRequestFromRfqResponse,
  SupplierOnboardingResponse,
  SupplierOnboardingDocumentRequirementsResponse,
  VendorRestrictionResponse,
  CreateVendorRestrictionRequest,
  LiftVendorRestrictionRequest,
  VendorRestrictionEnforcementResponse,
  SupplierIncidentResponse,
  CreateSupplierIncidentRequest,
  ResolveSupplierIncidentRequest,
  CancelSupplierIncidentRequest,
  ReopenSupplierIncidentRequest,
  ApplySupplierIncidentProcurementRestrictionRequest,
  ProcurementExceptionResponse,
  ProcurementExceptionResolutionTemplateResponse,
  CreateProcurementExceptionRequest,
  AssignProcurementExceptionRequest,
  LinkProcurementExceptionActionsRequest,
  ResolveProcurementExceptionRequest,
  RequestProcurementExceptionWaiveRequest,
  RejectProcurementExceptionWaiveRequest,
  CloseProcurementExceptionRequest,
  CancelProcurementExceptionRequest,
  ReopenProcurementExceptionRequest,
  ProcurementApprovalAuthorityMirrorResponse,
  PartyComplianceDocumentResponse,
  EmergencyPurchaseResponse,
  IssueEmergencyPurchaseOrderResponse,
  VendorReportSummaryResponse,
  VendorReportDetailResponse,
  PartsInventoryReportSummaryResponse,
  PartsInventoryPartDetailResponse,
  PartsInventoryLocationDetailResponse,
  PurchasingReportSummaryResponse,
  PurchasingPurchaseRequestDetailResponse,
  PurchasingPurchaseOrderDetailResponse,
  ComplianceReportSummaryResponse,
  CompliancePartyDetailResponse,
  ForgivingSearchResponse,
  AuditHistoryListResponse,
} from './types'

const apiBase = import.meta.env.VITE_SUPPLYARR_API_BASE ?? ''

export class SupplyArrApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'SupplyArrApiError'
  }
}

type ProblemDetailsLike = {
  title?: string
  detail?: string
  errors?: Record<string, string[] | string>
}

function extractProblemDetailsMessage(body: string): string | null {
  if (!body.trim()) {
    return null
  }

  try {
    const parsed = JSON.parse(body) as ProblemDetailsLike
    const parts: string[] = []

    if (typeof parsed.title === 'string' && parsed.title.trim()) {
      parts.push(parsed.title.trim())
    }

    if (typeof parsed.detail === 'string' && parsed.detail.trim()) {
      parts.push(parsed.detail.trim())
    }

    const errorEntries = parsed.errors ? Object.entries(parsed.errors) : []
    if (errorEntries.length > 0) {
      const flattened = errorEntries
        .flatMap(([field, value]) => {
          const values = Array.isArray(value) ? value : [value]
          return values
            .map((message) => String(message).trim())
            .filter(Boolean)
            .map((message) => `${field}: ${message}`)
        })
      if (flattened.length > 0) {
        parts.push(flattened.join('; '))
      }
    }

    return parts.length > 0 ? parts.join(' - ') : null
  } catch {
    return null
  }
}

async function toApiError(response: Response, fallbackMessage: string): Promise<SupplyArrApiError> {
  const body = await response.text()
  const parsedMessage = extractProblemDetailsMessage(body)
  const message = parsedMessage || body || `${fallbackMessage} (${response.status})`
  return new SupplyArrApiError(message, response.status, body)
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    throw await toApiError(response, fallbackMessage)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<HandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getMe(accessToken: string): Promise<SupplyArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplyArrMeResponse>(response, 'Failed to load profile')
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<SupplyArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplyArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function getVendors(accessToken: string): Promise<ExternalPartyResponse[]> {
  const response = await fetch(`${apiBase}/api/vendors`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalPartyResponse[]>(response, 'Failed to load vendors')
}

export async function getSuppliers(accessToken: string): Promise<ExternalPartyResponse[]> {
  const response = await fetch(`${apiBase}/api/suppliers`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalPartyResponse[]>(response, 'Failed to load suppliers')
}

export async function getDealers(accessToken: string): Promise<ExternalPartyResponse[]> {
  const response = await fetch(`${apiBase}/api/dealers`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalPartyResponse[]>(response, 'Failed to load dealers')
}

export async function createVendor(
  accessToken: string,
  request: CreateTypedExternalPartyRequest,
): Promise<ExternalPartyResponse> {
  const response = await fetch(`${apiBase}/api/vendors`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ExternalPartyResponse>(response, 'Failed to create vendor')
}

export async function createSupplier(
  accessToken: string,
  request: CreateTypedExternalPartyRequest,
): Promise<ExternalPartyResponse> {
  const response = await fetch(`${apiBase}/api/suppliers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ExternalPartyResponse>(response, 'Failed to create supplier')
}

export async function createDealer(
  accessToken: string,
  request: CreateTypedExternalPartyRequest,
): Promise<ExternalPartyResponse> {
  const response = await fetch(`${apiBase}/api/dealers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ExternalPartyResponse>(response, 'Failed to create dealer')
}

export async function updateParty(
  accessToken: string,
  route: PartyRegistryRoute,
  partyId: string,
  request: UpdateExternalPartyRequest,
): Promise<ExternalPartyResponse> {
  const response = await fetch(`${apiBase}/api/${route}/${partyId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ExternalPartyResponse>(response, 'Failed to update party')
}

export async function updatePartyApprovalStatus(
  accessToken: string,
  route: PartyRegistryRoute,
  partyId: string,
  request: UpdateExternalPartyApprovalStatusRequest,
): Promise<ExternalPartyResponse> {
  const response = await fetch(`${apiBase}/api/${route}/${partyId}/approval-status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ExternalPartyResponse>(response, 'Failed to update approval status')
}

export async function updatePartyStatus(
  accessToken: string,
  route: PartyRegistryRoute,
  partyId: string,
  request: UpdateExternalPartyStatusRequest,
): Promise<ExternalPartyResponse> {
  const response = await fetch(`${apiBase}/api/${route}/${partyId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ExternalPartyResponse>(response, 'Failed to update party status')
}

export async function createPartyContact(
  accessToken: string,
  route: PartyRegistryRoute,
  partyId: string,
  request: CreatePartyContactRequest,
): Promise<ExternalPartyResponse> {
  const response = await fetch(`${apiBase}/api/${route}/${partyId}/contacts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  if (!response.ok) {
    return parseJsonResponse<ExternalPartyResponse>(response, 'Failed to add party contact')
  }

  const listResponse = await fetch(`${apiBase}/api/${route}/${partyId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalPartyResponse>(listResponse, 'Failed to reload party after contact add')
}

export async function getPartCatalogs(accessToken: string): Promise<PartCatalogResponse[]> {
  const response = await fetch(`${apiBase}/api/catalogs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartCatalogResponse[]>(response, 'Failed to load part catalogs')
}

export async function getParts(accessToken: string): Promise<PartResponse[]> {
  const response = await fetch(`${apiBase}/api/parts`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartResponse[]>(response, 'Failed to load parts')
}

export async function createPartCatalog(
  accessToken: string,
  request: CreatePartCatalogRequest,
): Promise<PartCatalogResponse> {
  const response = await fetch(`${apiBase}/api/catalogs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartCatalogResponse>(response, 'Failed to create part catalog')
}

export async function createPart(
  accessToken: string,
  request: CreatePartRequest,
): Promise<PartResponse> {
  const response = await fetch(`${apiBase}/api/parts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartResponse>(response, 'Failed to create part')
}

export async function createPartVendorLink(
  accessToken: string,
  partId: string,
  request: CreatePartVendorLinkRequest,
): Promise<PartVendorLinkResponse> {
  const response = await fetch(`${apiBase}/api/parts/${partId}/vendor-links`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartVendorLinkResponse>(response, 'Failed to link vendor to part')
}

export async function getInventoryLocations(
  accessToken: string,
): Promise<InventoryLocationResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/inventory/locations`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InventoryLocationResponse[]>(response, 'Failed to load inventory locations')
}

export async function getInventoryBins(
  accessToken: string,
  locationId: string,
): Promise<InventoryBinResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/inventory/locations/${locationId}/bins`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InventoryBinResponse[]>(response, 'Failed to load inventory bins')
}

export async function getPartStockLevels(
  accessToken: string,
  params?: { locationId?: string; partId?: string },
): Promise<PartStockLevelResponse[]> {
  const search = new URLSearchParams()
  if (params?.locationId) {
    search.set('locationId', params.locationId)
  }
  if (params?.partId) {
    search.set('partId', params.partId)
  }
  const query = search.toString()
  const url = query ? `${apiBase}/api/v1/inventory/stock?${query}` : `${apiBase}/api/v1/inventory/stock`
  const response = await fetch(url, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartStockLevelResponse[]>(response, 'Failed to load stock levels')
}

export async function getStockReservations(
  accessToken: string,
  options?: { status?: string; partId?: string; binId?: string },
): Promise<StockReservationResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  if (options?.binId) {
    params.set('binId', options.binId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/inventory/reservations${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StockReservationResponse[]>(response, 'Failed to load stock reservations')
}

export async function createStockReservation(
  accessToken: string,
  request: CreateStockReservationRequest,
): Promise<StockReservationResponse> {
  const response = await fetch(`${apiBase}/api/v1/inventory/reservations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StockReservationResponse>(response, 'Failed to create stock reservation')
}

export async function releaseStockReservation(
  accessToken: string,
  reservationId: string,
  request: ReleaseStockReservationRequest,
): Promise<StockReservationResponse> {
  const response = await fetch(`${apiBase}/api/v1/inventory/reservations/${reservationId}/release`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<StockReservationResponse>(response, 'Failed to release stock reservation')
}

export async function fulfillStockReservation(
  accessToken: string,
  reservationId: string,
): Promise<StockReservationResponse> {
  const response = await fetch(`${apiBase}/api/v1/inventory/reservations/${reservationId}/fulfill`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<StockReservationResponse>(response, 'Failed to fulfill stock reservation')
}

export async function createInventoryLocation(
  accessToken: string,
  request: CreateInventoryLocationRequest,
): Promise<InventoryLocationResponse> {
  const response = await fetch(`${apiBase}/api/v1/inventory/locations`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<InventoryLocationResponse>(response, 'Failed to create inventory location')
}

export async function createInventoryBin(
  accessToken: string,
  locationId: string,
  request: CreateInventoryBinRequest,
): Promise<InventoryBinResponse> {
  const response = await fetch(`${apiBase}/api/v1/inventory/locations/${locationId}/bins`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<InventoryBinResponse>(response, 'Failed to create inventory bin')
}

export async function upsertPartStockLevel(
  accessToken: string,
  request: UpsertPartStockLevelRequest,
): Promise<PartStockLevelResponse> {
  const response = await fetch(`${apiBase}/api/v1/inventory/stock`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartStockLevelResponse>(response, 'Failed to save stock level')
}

export async function getPurchaseRequests(
  accessToken: string,
  status?: string,
): Promise<PurchaseRequestResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/purchase-requests${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseRequestResponse[]>(response, 'Failed to load purchase requests')
}

export async function createPurchaseRequest(
  accessToken: string,
  request: CreatePurchaseRequestRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-requests`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to create purchase request')
}

export async function getRfqs(accessToken: string, status?: string): Promise<RfqResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/rfqs${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RfqResponse[]>(response, 'Failed to load RFQs')
}

export async function getRfq(accessToken: string, rfqId: string): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RfqResponse>(response, 'Failed to load RFQ')
}

export async function createRfq(
  accessToken: string,
  payload: {
    rfqKey: string
    title: string
    notes: string
    lines?: { partId: string; quantityRequested: number; notes: string }[]
  },
): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<RfqResponse>(response, 'Failed to create RFQ')
}

export async function submitRfq(accessToken: string, rfqId: string): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RfqResponse>(response, 'Failed to submit RFQ')
}

export async function inviteRfqVendors(
  accessToken: string,
  rfqId: string,
  vendorPartyIds: string[],
): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/invite-vendors`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ vendorPartyIds }),
  })
  return parseJsonResponse<RfqResponse>(response, 'Failed to invite vendors')
}

export async function createVendorQuote(
  accessToken: string,
  rfqId: string,
  payload: { vendorPartyId: string; quoteKey: string; currencyCode: string; notes: string },
): Promise<VendorQuoteResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quotes`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorQuoteResponse>(response, 'Failed to create vendor quote')
}

export async function upsertVendorQuoteLine(
  accessToken: string,
  rfqId: string,
  vendorQuoteId: string,
  payload: {
    rfqLineId: string
    unitPrice: number
    quantityQuoted: number
    leadTimeDays?: number | null
    notes: string
  },
): Promise<VendorQuoteResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quotes/${vendorQuoteId}/lines`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorQuoteResponse>(response, 'Failed to save quote line')
}

export async function submitVendorQuote(
  accessToken: string,
  rfqId: string,
  vendorQuoteId: string,
): Promise<VendorQuoteResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quotes/${vendorQuoteId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorQuoteResponse>(response, 'Failed to submit vendor quote')
}

export async function getRfqQuoteComparison(
  accessToken: string,
  rfqId: string,
): Promise<RfqQuoteComparisonResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quote-comparison`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RfqQuoteComparisonResponse>(response, 'Failed to load quote comparison')
}

export async function selectRfqVendorQuote(
  accessToken: string,
  rfqId: string,
  vendorQuoteId: string,
): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/select-quote`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ vendorQuoteId }),
  })
  return parseJsonResponse<RfqResponse>(response, 'Failed to select vendor quote')
}

export async function createPurchaseRequestFromRfq(
  accessToken: string,
  rfqId: string,
  payload: { requestKey: string; title?: string; notes?: string },
): Promise<CreatePurchaseRequestFromRfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/create-purchase-request`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<CreatePurchaseRequestFromRfqResponse>(
    response,
    'Failed to create purchase request from RFQ',
  )
}

export async function submitPurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-requests/${purchaseRequestId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to submit purchase request')
}

export async function approvePurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-requests/${purchaseRequestId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to approve purchase request')
}

export async function rejectPurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
  request: RejectPurchaseRequestRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-requests/${purchaseRequestId}/reject`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to reject purchase request')
}

export async function getPurchaseOrders(
  accessToken: string,
  status?: string,
): Promise<PurchaseOrderResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/purchase-orders${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseOrderResponse[]>(response, 'Failed to load purchase orders')
}

export async function createPurchaseOrderFromPurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
  request: CreatePurchaseOrderFromPurchaseRequestRequest,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/purchase-orders/from-purchase-request/${purchaseRequestId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<PurchaseOrderResponse>(
    response,
    'Failed to create purchase order from purchase request',
  )
}

export async function approvePurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-orders/${purchaseOrderId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to approve purchase order')
}

export async function issuePurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-orders/${purchaseOrderId}/issue`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to issue purchase order')
}

export async function cancelPurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
  request: CancelPurchaseOrderRequest,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-orders/${purchaseOrderId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to cancel purchase order')
}

export async function getReceivingReceipt(
  accessToken: string,
  receivingReceiptId: string,
): Promise<ReceivingReceiptResponse> {
  const response = await fetch(`${apiBase}/api/v1/receiving/${receivingReceiptId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReceivingReceiptResponse>(response, 'Failed to load receiving receipt')
}

export async function getReceivingReceipts(
  accessToken: string,
  options?: { status?: string; purchaseOrderId?: string },
): Promise<ReceivingReceiptResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.purchaseOrderId) {
    params.set('purchaseOrderId', options.purchaseOrderId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/receiving${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReceivingReceiptResponse[]>(response, 'Failed to load receiving receipts')
}

export async function createReceivingReceiptFromPurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
  request: CreateReceivingReceiptFromPurchaseOrderRequest,
): Promise<ReceivingReceiptResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/receiving/from-purchase-order/${purchaseOrderId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<ReceivingReceiptResponse>(
    response,
    'Failed to create receiving receipt',
  )
}

export async function postReceivingReceipt(
  accessToken: string,
  receivingReceiptId: string,
): Promise<ReceivingReceiptResponse> {
  const response = await fetch(`${apiBase}/api/v1/receiving/${receivingReceiptId}/post`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReceivingReceiptResponse>(response, 'Failed to post receiving receipt')
}

export async function updateReceivingReceiptLine(
  accessToken: string,
  receivingReceiptId: string,
  lineId: string,
  request: UpdateReceivingReceiptLineRequest,
): Promise<ReceivingReceiptResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/receiving/${receivingReceiptId}/lines/${lineId}`,
    {
      method: 'PUT',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<ReceivingReceiptResponse>(
    response,
    'Failed to update receiving receipt line',
  )
}

export async function createReceivingException(
  accessToken: string,
  receivingReceiptId: string,
  lineId: string,
  request: CreateReceivingExceptionRequest,
): Promise<ReceivingExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/receiving/${receivingReceiptId}/lines/${lineId}/exceptions`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<ReceivingExceptionResponse>(
    response,
    'Failed to record receiving exception',
  )
}

export async function resolveReceivingException(
  accessToken: string,
  receivingExceptionId: string,
): Promise<ReceivingExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/receiving/exceptions/${receivingExceptionId}/resolve`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ReceivingExceptionResponse>(
    response,
    'Failed to resolve receiving exception',
  )
}

export async function getBackorders(
  accessToken: string,
  options?: { status?: string; purchaseOrderId?: string; partId?: string },
): Promise<BackorderResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.purchaseOrderId) {
    params.set('purchaseOrderId', options.purchaseOrderId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/backorders${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BackorderResponse[]>(response, 'Failed to load backorders')
}

export async function createBackorderFromPurchaseOrderLine(
  accessToken: string,
  purchaseOrderLineId: string,
  request: CreateBackorderFromPurchaseOrderLineRequest,
): Promise<BackorderResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/backorders/from-purchase-order-line/${purchaseOrderLineId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<BackorderResponse>(response, 'Failed to create backorder')
}

export async function fulfillBackorder(
  accessToken: string,
  backorderId: string,
): Promise<BackorderResponse> {
  const response = await fetch(`${apiBase}/api/v1/backorders/${backorderId}/fulfill`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BackorderResponse>(response, 'Failed to fulfill backorder')
}

export async function cancelBackorder(
  accessToken: string,
  backorderId: string,
  request: CancelBackorderRequest,
): Promise<BackorderResponse> {
  const response = await fetch(`${apiBase}/api/v1/backorders/${backorderId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<BackorderResponse>(response, 'Failed to cancel backorder')
}

export async function getVendorReturns(
  accessToken: string,
  options?: {
    status?: string
    vendorPartyId?: string
    purchaseOrderId?: string
    partId?: string
  },
): Promise<VendorReturnResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.vendorPartyId) {
    params.set('vendorPartyId', options.vendorPartyId)
  }
  if (options?.purchaseOrderId) {
    params.set('purchaseOrderId', options.purchaseOrderId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/returns${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorReturnResponse[]>(response, 'Failed to load vendor returns')
}

export async function createVendorReturnFromStock(
  accessToken: string,
  request: CreateVendorReturnFromStockRequest,
): Promise<VendorReturnResponse> {
  const response = await fetch(`${apiBase}/api/v1/returns/from-stock`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<VendorReturnResponse>(response, 'Failed to create vendor return')
}

export async function createVendorReturnFromPurchaseOrderLine(
  accessToken: string,
  purchaseOrderLineId: string,
  request: CreateVendorReturnFromPurchaseOrderLineRequest,
): Promise<VendorReturnResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/returns/from-purchase-order-line/${purchaseOrderLineId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<VendorReturnResponse>(response, 'Failed to create vendor return')
}

export async function postVendorReturn(
  accessToken: string,
  returnId: string,
): Promise<VendorReturnResponse> {
  const response = await fetch(`${apiBase}/api/v1/returns/${returnId}/post`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorReturnResponse>(response, 'Failed to post vendor return')
}

export async function cancelVendorReturn(
  accessToken: string,
  returnId: string,
  request: CancelVendorReturnRequest,
): Promise<VendorReturnResponse> {
  const response = await fetch(`${apiBase}/api/v1/returns/${returnId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<VendorReturnResponse>(response, 'Failed to cancel vendor return')
}

export async function listWarrantyClaims(
  accessToken: string,
  options?: {
    status?: string
    vendorPartyId?: string
    partId?: string
    purchaseOrderId?: string
  },
): Promise<WarrantyClaimResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) params.set('status', options.status)
  if (options?.vendorPartyId) params.set('vendorPartyId', options.vendorPartyId)
  if (options?.partId) params.set('partId', options.partId)
  if (options?.purchaseOrderId) params.set('purchaseOrderId', options.purchaseOrderId)
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/warranty-claims${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WarrantyClaimResponse[]>(response, 'Failed to load warranty claims')
}

export async function createWarrantyClaim(
  accessToken: string,
  request: CreateWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to create warranty claim')
}

export async function updateWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: UpdateWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to update warranty claim')
}

export async function submitWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: SubmitWarrantyClaimRequest = {},
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to submit warranty claim')
}

export async function recordWarrantyClaimVendorResponse(
  accessToken: string,
  warrantyClaimId: string,
  request: RecordWarrantyClaimVendorResponseRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/record-vendor-response`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<WarrantyClaimResponse>(
    response,
    'Failed to record warranty claim vendor response',
  )
}

export async function closeWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: CloseWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/close`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to close warranty claim')
}

export async function denyWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: DenyWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/deny`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to deny warranty claim')
}

export async function cancelWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: CancelWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to cancel warranty claim')
}

export async function getPricingSnapshots(
  accessToken: string,
  options?: {
    partVendorLinkId?: string
    partId?: string
    vendorPartyId?: string
    asOf?: string
  },
): Promise<PricingSnapshotResponse[]> {
  const params = new URLSearchParams()
  if (options?.partVendorLinkId) {
    params.set('partVendorLinkId', options.partVendorLinkId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  if (options?.vendorPartyId) {
    params.set('vendorPartyId', options.vendorPartyId)
  }
  if (options?.asOf) {
    params.set('asOf', options.asOf)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/pricing-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PricingSnapshotResponse[]>(response, 'Failed to load pricing snapshots')
}

export async function createPricingSnapshot(
  accessToken: string,
  request: CreatePricingSnapshotRequest,
): Promise<PricingSnapshotResponse> {
  const response = await fetch(`${apiBase}/api/v1/pricing-snapshots`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PricingSnapshotResponse>(response, 'Failed to create pricing snapshot')
}

export async function getLeadTimeSnapshots(
  accessToken: string,
  options?: {
    partVendorLinkId?: string
    partId?: string
    vendorPartyId?: string
    asOf?: string
  },
): Promise<LeadTimeSnapshotResponse[]> {
  const params = new URLSearchParams()
  if (options?.partVendorLinkId) {
    params.set('partVendorLinkId', options.partVendorLinkId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  if (options?.vendorPartyId) {
    params.set('vendorPartyId', options.vendorPartyId)
  }
  if (options?.asOf) {
    params.set('asOf', options.asOf)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LeadTimeSnapshotResponse[]>(response, 'Failed to load lead-time snapshots')
}

export async function createLeadTimeSnapshot(
  accessToken: string,
  request: CreateLeadTimeSnapshotRequest,
): Promise<LeadTimeSnapshotResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshots`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<LeadTimeSnapshotResponse>(response, 'Failed to create lead-time snapshot')
}

export async function getAvailabilitySnapshots(
  accessToken: string,
  options?: {
    partVendorLinkId?: string
    partId?: string
    vendorPartyId?: string
    asOf?: string
  },
): Promise<AvailabilitySnapshotResponse[]> {
  const params = new URLSearchParams()
  if (options?.partVendorLinkId) {
    params.set('partVendorLinkId', options.partVendorLinkId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  if (options?.vendorPartyId) {
    params.set('vendorPartyId', options.vendorPartyId)
  }
  if (options?.asOf) {
    params.set('asOf', options.asOf)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/availability-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AvailabilitySnapshotResponse[]>(
    response,
    'Failed to load availability snapshots',
  )
}

export async function createAvailabilitySnapshot(
  accessToken: string,
  request: CreateAvailabilitySnapshotRequest,
): Promise<AvailabilitySnapshotResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshots`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<AvailabilitySnapshotResponse>(
    response,
    'Failed to create availability snapshot',
  )
}

export async function getReorderEvaluation(accessToken: string): Promise<ReorderEvaluationResponse> {
  const response = await fetch(`${apiBase}/api/v1/reorder-evaluation`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReorderEvaluationResponse>(response, 'Failed to load reorder evaluation')
}

export async function upsertPartReorderPolicy(
  accessToken: string,
  partId: string,
  request: UpsertPartReorderPolicyRequest,
): Promise<PartReorderPolicyResponse> {
  const response = await fetch(`${apiBase}/api/v1/reorder-evaluation/parts/${partId}/policy`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartReorderPolicyResponse>(response, 'Failed to save reorder policy')
}

export async function createPurchaseRequestFromReorder(
  accessToken: string,
  request: CreatePurchaseRequestFromReorderRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/reorder-evaluation/create-purchase-request`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseRequestResponse>(
    response,
    'Failed to create purchase request from reorder suggestions',
  )
}

export async function getDemandRefs(
  accessToken: string,
  options?: { status?: string },
): Promise<MaintainArrDemandRefResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) {
    search.set('status', options.status)
  }
  const suffix = search.size > 0 ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/demand-refs${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrDemandRefResponse[]>(response, 'Failed to load demand references')
}

export async function getDemandRef(
  accessToken: string,
  demandRefId: string,
): Promise<MaintainArrDemandRefResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-refs/${demandRefId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrDemandRefResponse>(response, 'Failed to load demand reference')
}

export async function createPurchaseRequestFromDemandRef(
  accessToken: string,
  demandRefId: string,
  request: CreatePurchaseRequestFromDemandRefRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-refs/${demandRefId}/create-purchase-request`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseRequestResponse>(
    response,
    'Failed to create purchase request from demand reference',
  )
}

export async function getProcurementNotificationSettings(
  accessToken: string,
): Promise<ProcurementNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/notification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementNotificationSettingsResponse>(
    response,
    'Failed to load notification settings',
  )
}

export async function upsertProcurementNotificationSettings(
  accessToken: string,
  payload: UpsertProcurementNotificationSettingsRequest,
): Promise<ProcurementNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/notification-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementNotificationSettingsResponse>(
    response,
    'Failed to save notification settings',
  )
}

export async function getProcurementNotificationDispatches(
  accessToken: string,
  limit = 20,
): Promise<ProcurementNotificationDispatchesResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/notification-settings/dispatches?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementNotificationDispatchesResponse>(
    response,
    'Failed to load notification dispatches',
  )
}

export async function getPriceSnapshotSettings(
  accessToken: string,
): Promise<PriceSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PriceSnapshotSettingsResponse>(
    response,
    'Failed to load price snapshot settings',
  )
}

export async function upsertPriceSnapshotSettings(
  accessToken: string,
  payload: UpsertPriceSnapshotSettingsRequest,
): Promise<PriceSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PriceSnapshotSettingsResponse>(
    response,
    'Failed to save price snapshot settings',
  )
}

export async function getPendingPriceSnapshotCaptures(
  accessToken: string,
): Promise<PendingPriceSnapshotCapturesResponse> {
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingPriceSnapshotCapturesResponse>(
    response,
    'Failed to load pending price snapshot captures',
  )
}

export async function getPriceSnapshotRuns(
  accessToken: string,
  limit = 5,
): Promise<PriceSnapshotRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PriceSnapshotRunsResponse>(
    response,
    'Failed to load price snapshot runs',
  )
}

export async function getLeadTimeSnapshotSettings(
  accessToken: string,
): Promise<LeadTimeSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LeadTimeSnapshotSettingsResponse>(
    response,
    'Failed to load lead-time snapshot settings',
  )
}

export async function upsertLeadTimeSnapshotSettings(
  accessToken: string,
  payload: UpsertLeadTimeSnapshotSettingsRequest,
): Promise<LeadTimeSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<LeadTimeSnapshotSettingsResponse>(
    response,
    'Failed to save lead-time snapshot settings',
  )
}

export async function getPendingLeadTimeSnapshotCaptures(
  accessToken: string,
): Promise<PendingLeadTimeSnapshotCapturesResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingLeadTimeSnapshotCapturesResponse>(
    response,
    'Failed to load pending lead-time snapshot captures',
  )
}

export async function getLeadTimeSnapshotRuns(
  accessToken: string,
  limit = 5,
): Promise<LeadTimeSnapshotRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LeadTimeSnapshotRunsResponse>(
    response,
    'Failed to load lead-time snapshot runs',
  )
}

export async function getAvailabilitySnapshotSettings(
  accessToken: string,
): Promise<AvailabilitySnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AvailabilitySnapshotSettingsResponse>(
    response,
    'Failed to load availability snapshot settings',
  )
}

export async function upsertAvailabilitySnapshotSettings(
  accessToken: string,
  payload: UpsertAvailabilitySnapshotSettingsRequest,
): Promise<AvailabilitySnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AvailabilitySnapshotSettingsResponse>(
    response,
    'Failed to save availability snapshot settings',
  )
}

export async function getPendingAvailabilitySnapshotCaptures(
  accessToken: string,
): Promise<PendingAvailabilitySnapshotCapturesResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingAvailabilitySnapshotCapturesResponse>(
    response,
    'Failed to load pending availability snapshot captures',
  )
}

export async function getAvailabilitySnapshotRuns(
  accessToken: string,
  limit = 10,
): Promise<AvailabilitySnapshotRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AvailabilitySnapshotRunsResponse>(
    response,
    'Failed to load availability snapshot runs',
  )
}

export async function getProcurementCoordinationDashboard(
  accessToken: string,
  activeOnly = true,
): Promise<ProcurementCoordinationDashboardResponse> {
  const search = new URLSearchParams({ activeOnly: String(activeOnly) })
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementCoordinationDashboardResponse>(
    response,
    'Failed to load procurement coordination dashboard',
  )
}

export async function getProcurementCoordinationSettings(
  accessToken: string,
): Promise<ProcurementCoordinationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementCoordinationSettingsResponse>(
    response,
    'Failed to load procurement coordination settings',
  )
}

export async function upsertProcurementCoordinationSettings(
  accessToken: string,
  payload: UpsertProcurementCoordinationSettingsRequest,
): Promise<ProcurementCoordinationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementCoordinationSettingsResponse>(
    response,
    'Failed to save procurement coordination settings',
  )
}

export async function getPendingProcurementCoordination(
  accessToken: string,
): Promise<PendingProcurementCoordinationResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingProcurementCoordinationResponse>(
    response,
    'Failed to load pending procurement coordination',
  )
}

export async function getProcurementCoordinationRuns(
  accessToken: string,
  limit = 5,
): Promise<ProcurementCoordinationRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementCoordinationRunsResponse>(
    response,
    'Failed to load procurement coordination runs',
  )
}

export async function getApprovalRemindersDashboard(
  accessToken: string,
  includeUpcoming = false,
): Promise<ApprovalRemindersDashboardResponse> {
  const search = new URLSearchParams({ includeUpcoming: String(includeUpcoming) })
  const response = await fetch(`${apiBase}/api/v1/approval-reminders?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ApprovalRemindersDashboardResponse>(
    response,
    'Failed to load approval reminders dashboard',
  )
}

export async function getApprovalReminderSettings(
  accessToken: string,
): Promise<ApprovalReminderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ApprovalReminderSettingsResponse>(
    response,
    'Failed to load approval reminder settings',
  )
}

export async function upsertApprovalReminderSettings(
  accessToken: string,
  payload: UpsertApprovalReminderSettingsRequest,
): Promise<ApprovalReminderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ApprovalReminderSettingsResponse>(
    response,
    'Failed to save approval reminder settings',
  )
}

export async function getPendingApprovalReminders(
  accessToken: string,
): Promise<PendingApprovalRemindersResponse> {
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingApprovalRemindersResponse>(
    response,
    'Failed to load pending approval reminders',
  )
}

export async function getApprovalReminderRuns(
  accessToken: string,
  limit = 5,
): Promise<ApprovalReminderRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ApprovalReminderRunsResponse>(
    response,
    'Failed to load approval reminder runs',
  )
}

export async function getProcurementExceptionEscalationSettings(
  accessToken: string,
): Promise<ProcurementExceptionEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exception-escalation-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementExceptionEscalationSettingsResponse>(
    response,
    'Failed to load procurement exception escalation settings',
  )
}

export async function upsertProcurementExceptionEscalationSettings(
  accessToken: string,
  payload: UpsertProcurementExceptionEscalationSettingsRequest,
): Promise<ProcurementExceptionEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exception-escalation-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionEscalationSettingsResponse>(
    response,
    'Failed to save procurement exception escalation settings',
  )
}

export async function getPendingProcurementExceptionEscalations(
  accessToken: string,
): Promise<PendingProcurementExceptionEscalationsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exception-escalation-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingProcurementExceptionEscalationsResponse>(
    response,
    'Failed to load pending procurement exception escalations',
  )
}

export async function getProcurementExceptionEscalationRuns(
  accessToken: string,
  limit = 5,
): Promise<ProcurementExceptionEscalationRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exception-escalation-settings/runs?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ProcurementExceptionEscalationRunsResponse>(
    response,
    'Failed to load procurement exception escalation runs',
  )
}

export async function getProcurementExceptionEscalationEvents(
  accessToken: string,
  limit = 10,
): Promise<ProcurementExceptionEscalationEventsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exception-escalation-settings/events?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ProcurementExceptionEscalationEventsResponse>(
    response,
    'Failed to load procurement exception escalation events',
  )
}

export async function getDemandProcessingDashboard(
  accessToken: string,
): Promise<DemandProcessingDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingDashboardResponse>(
    response,
    'Failed to load demand processing dashboard',
  )
}

export async function getDemandProcessingDetail(
  accessToken: string,
  demandRefId: string,
): Promise<DemandProcessingDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing/${demandRefId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingDetailResponse>(
    response,
    'Failed to load demand processing detail',
  )
}

export async function retryDemandProcessing(
  accessToken: string,
  demandRefId: string,
): Promise<DemandProcessingOperatorActionResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing/${demandRefId}/retry-processing`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingOperatorActionResponse>(
    response,
    'Failed to retry demand processing',
  )
}

export async function createDemandProcessingPrDraft(
  accessToken: string,
  demandRefId: string,
): Promise<DemandProcessingOperatorActionResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing/${demandRefId}/create-pr-draft`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingOperatorActionResponse>(
    response,
    'Failed to create purchase request draft',
  )
}

export async function getSupplyReadinessDashboard(
  accessToken: string,
): Promise<SupplyReadinessDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/supply-readiness/dashboard`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplyReadinessDashboardResponse>(
    response,
    'Failed to load supply readiness dashboard',
  )
}

export async function getPartSupplyReadiness(
  accessToken: string,
  partId: string,
  quantity?: number,
): Promise<PartSupplyReadinessResponse> {
  const params = new URLSearchParams()
  if (quantity !== undefined) {
    params.set('quantity', String(quantity))
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/v1/supply-readiness/parts/${partId}${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PartSupplyReadinessResponse>(
    response,
    'Failed to load part supply readiness',
  )
}

export async function getVendorSupplyReadiness(
  accessToken: string,
  externalPartyId: string,
): Promise<VendorSupplyReadinessResponse> {
  const response = await fetch(`${apiBase}/api/v1/supply-readiness/vendors/${externalPartyId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorSupplyReadinessResponse>(
    response,
    'Failed to load vendor supply readiness',
  )
}

export async function getProcurementPathReadiness(
  accessToken: string,
  partId: string,
  externalPartyId: string,
  quantity?: number,
): Promise<ProcurementPathReadinessResponse> {
  const params = new URLSearchParams({ partId, externalPartyId })
  if (quantity !== undefined) {
    params.set('quantity', String(quantity))
  }
  const response = await fetch(`${apiBase}/api/v1/supply-readiness/procurement-path?${params}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementPathReadinessResponse>(
    response,
    'Failed to load procurement path readiness',
  )
}

export async function getDemandProcessingSettings(
  accessToken: string,
): Promise<DemandProcessingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingSettingsResponse>(
    response,
    'Failed to load demand processing settings',
  )
}

export async function upsertDemandProcessingSettings(
  accessToken: string,
  payload: UpsertDemandProcessingSettingsRequest,
): Promise<DemandProcessingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DemandProcessingSettingsResponse>(
    response,
    'Failed to save demand processing settings',
  )
}

export async function getPendingDemandProcessing(
  accessToken: string,
): Promise<PendingDemandProcessingResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingDemandProcessingResponse>(
    response,
    'Failed to load pending demand processing',
  )
}

export async function getDemandProcessingRuns(
  accessToken: string,
  limit = 5,
): Promise<DemandProcessingRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingRunsResponse>(
    response,
    'Failed to load demand processing runs',
  )
}

export async function getIntegrationEventSettings(
  accessToken: string,
): Promise<IntegrationEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationEventSettingsResponse>(
    response,
    'Failed to load integration event settings',
  )
}

export async function upsertIntegrationEventSettings(
  accessToken: string,
  payload: UpsertIntegrationEventSettingsRequest,
): Promise<IntegrationEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<IntegrationEventSettingsResponse>(
    response,
    'Failed to save integration event settings',
  )
}

export async function getIntegrationEventOutbox(
  accessToken: string,
  limit = 25,
): Promise<IntegrationEventsListResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings/outbox?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationEventsListResponse>(
    response,
    'Failed to load integration outbox events',
  )
}

export async function getIntegrationEventInbox(
  accessToken: string,
  limit = 25,
): Promise<IntegrationEventsListResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings/inbox?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationEventsListResponse>(
    response,
    'Failed to load integration inbox events',
  )
}

export async function getVendorReportSummary(
  accessToken: string,
  options?: { approvalStatus?: string; activeOnly?: boolean },
): Promise<VendorReportSummaryResponse> {
  const search = new URLSearchParams()
  if (options?.approvalStatus) {
    search.set('approvalStatus', options.approvalStatus)
  }
  if (options?.activeOnly) {
    search.set('activeOnly', 'true')
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/vendors/summary${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<VendorReportSummaryResponse>(
    response,
    'Failed to load vendor report summary',
  )
}

export async function getVendorReportDetail(
  accessToken: string,
  vendorPartyId: string,
): Promise<VendorReportDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/reports/vendors/${vendorPartyId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorReportDetailResponse>(
    response,
    'Failed to load vendor report detail',
  )
}

export async function exportVendorReportSummaryCsv(
  accessToken: string,
  options?: { approvalStatus?: string; activeOnly?: boolean },
): Promise<Blob> {
  const search = new URLSearchParams()
  if (options?.approvalStatus) {
    search.set('approvalStatus', options.approvalStatus)
  }
  if (options?.activeOnly) {
    search.set('activeOnly', 'true')
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/vendors/summary/export${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to export vendor report summary')
  }
  return response.blob()
}

export async function getPartsInventoryReportSummary(
  accessToken: string,
  options?: {
    activePartsOnly?: boolean
    belowReorderOnly?: boolean
    inventoryLocationId?: string
  },
): Promise<PartsInventoryReportSummaryResponse> {
  const search = new URLSearchParams()
  if (options?.activePartsOnly) {
    search.set('activePartsOnly', 'true')
  }
  if (options?.belowReorderOnly) {
    search.set('belowReorderOnly', 'true')
  }
  if (options?.inventoryLocationId) {
    search.set('inventoryLocationId', options.inventoryLocationId)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/parts-inventory/summary${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PartsInventoryReportSummaryResponse>(
    response,
    'Failed to load parts and inventory report summary',
  )
}

export async function getPartsInventoryPartDetail(
  accessToken: string,
  partId: string,
): Promise<PartsInventoryPartDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/reports/parts-inventory/parts/${partId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartsInventoryPartDetailResponse>(
    response,
    'Failed to load part inventory detail',
  )
}

export async function getPartsInventoryLocationDetail(
  accessToken: string,
  inventoryLocationId: string,
): Promise<PartsInventoryLocationDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/reports/parts-inventory/locations/${inventoryLocationId}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PartsInventoryLocationDetailResponse>(
    response,
    'Failed to load location inventory detail',
  )
}

export async function exportPartsInventoryReportSummaryCsv(
  accessToken: string,
  options?: {
    activePartsOnly?: boolean
    belowReorderOnly?: boolean
    inventoryLocationId?: string
  },
): Promise<Blob> {
  const search = new URLSearchParams()
  if (options?.activePartsOnly) {
    search.set('activePartsOnly', 'true')
  }
  if (options?.belowReorderOnly) {
    search.set('belowReorderOnly', 'true')
  }
  if (options?.inventoryLocationId) {
    search.set('inventoryLocationId', options.inventoryLocationId)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/parts-inventory/summary/export${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to export parts and inventory report')
  }
  return response.blob()
}

export async function getPurchasingReportSummary(
  accessToken: string,
  options?: { openDocumentsOnly?: boolean; vendorPartyId?: string },
): Promise<PurchasingReportSummaryResponse> {
  const search = new URLSearchParams()
  if (options?.openDocumentsOnly) {
    search.set('openDocumentsOnly', 'true')
  }
  if (options?.vendorPartyId) {
    search.set('vendorPartyId', options.vendorPartyId)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/purchasing/summary${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PurchasingReportSummaryResponse>(
    response,
    'Failed to load purchasing report summary',
  )
}

export async function getPurchasingPurchaseRequestDetail(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchasingPurchaseRequestDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/reports/purchasing/purchase-requests/${purchaseRequestId}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PurchasingPurchaseRequestDetailResponse>(
    response,
    'Failed to load purchase request report detail',
  )
}

export async function getPurchasingPurchaseOrderDetail(
  accessToken: string,
  purchaseOrderId: string,
): Promise<PurchasingPurchaseOrderDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/reports/purchasing/purchase-orders/${purchaseOrderId}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PurchasingPurchaseOrderDetailResponse>(
    response,
    'Failed to load purchase order report detail',
  )
}

export async function exportPurchasingReportSummaryCsv(
  accessToken: string,
  options?: { openDocumentsOnly?: boolean; vendorPartyId?: string },
): Promise<Blob> {
  const search = new URLSearchParams()
  if (options?.openDocumentsOnly) {
    search.set('openDocumentsOnly', 'true')
  }
  if (options?.vendorPartyId) {
    search.set('vendorPartyId', options.vendorPartyId)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/purchasing/summary/export${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to export purchasing report')
  }
  return response.blob()
}

export async function getComplianceReportSummary(
  accessToken: string,
  options?: { attentionOnly?: boolean; partyType?: string; externalPartyId?: string },
): Promise<ComplianceReportSummaryResponse> {
  const search = new URLSearchParams()
  if (options?.attentionOnly) {
    search.set('attentionOnly', 'true')
  }
  if (options?.partyType) {
    search.set('partyType', options.partyType)
  }
  if (options?.externalPartyId) {
    search.set('externalPartyId', options.externalPartyId)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/compliance/summary${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<ComplianceReportSummaryResponse>(
    response,
    'Failed to load compliance report summary',
  )
}

export async function getCompliancePartyDetail(
  accessToken: string,
  externalPartyId: string,
): Promise<CompliancePartyDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/reports/compliance/parties/${externalPartyId}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<CompliancePartyDetailResponse>(
    response,
    'Failed to load compliance party detail',
  )
}

export async function listAuditHistory(
  accessToken: string,
  options?: {
    limit?: number
    cursor?: string
    action?: string
    targetType?: string
    targetId?: string
    actorUserId?: string
    result?: string
    fromOccurredAt?: string
    toOccurredAt?: string
  },
): Promise<AuditHistoryListResponse> {
  const search = new URLSearchParams()
  if (options?.limit) {
    search.set('limit', String(options.limit))
  }
  if (options?.cursor) {
    search.set('cursor', options.cursor)
  }
  if (options?.action) {
    search.set('action', options.action)
  }
  if (options?.targetType) {
    search.set('targetType', options.targetType)
  }
  if (options?.targetId) {
    search.set('targetId', options.targetId)
  }
  if (options?.actorUserId) {
    search.set('actorUserId', options.actorUserId)
  }
  if (options?.result) {
    search.set('result', options.result)
  }
  if (options?.fromOccurredAt) {
    search.set('fromOccurredAt', options.fromOccurredAt)
  }
  if (options?.toOccurredAt) {
    search.set('toOccurredAt', options.toOccurredAt)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/audit-history${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<AuditHistoryListResponse>(response, 'Failed to load audit history')
}

export async function forgivingSearch(
  accessToken: string,
  options: { q: string; limit?: number },
): Promise<ForgivingSearchResponse> {
  const search = new URLSearchParams({ q: options.q })
  if (options.limit) {
    search.set('limit', String(options.limit))
  }
  const response = await fetch(`${apiBase}/api/v1/search/forgiving?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ForgivingSearchResponse>(response, 'Failed to run forgiving search')
}

export async function getEmergencyPurchases(
  accessToken: string,
  status?: string,
): Promise<EmergencyPurchaseResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/emergency-purchases${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmergencyPurchaseResponse[]>(response, 'Failed to load emergency purchases')
}

export async function listPendingEmergencyPurchases(
  accessToken: string,
): Promise<EmergencyPurchaseResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/emergency-purchases/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EmergencyPurchaseResponse[]>(
    response,
    'Failed to load pending emergency purchases',
  )
}

export async function createEmergencyPurchase(
  accessToken: string,
  payload: {
    requestKey: string
    title: string
    emergencyReason: string
    vendorPartyId: string
    notes: string
    lines: { partId: string; quantityRequested: number; notes: string }[]
  },
): Promise<EmergencyPurchaseResponse> {
  const response = await fetch(`${apiBase}/api/v1/emergency-purchases`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<EmergencyPurchaseResponse>(response, 'Failed to create emergency purchase')
}

export async function expeditedSubmitEmergencyPurchase(
  accessToken: string,
  purchaseRequestId: string,
  notes?: string,
): Promise<EmergencyPurchaseResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/emergency-purchases/${purchaseRequestId}/expedited-submit`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ notes: notes ?? null }),
    },
  )
  return parseJsonResponse<EmergencyPurchaseResponse>(
    response,
    'Failed to expedited-submit emergency purchase',
  )
}

export async function managerOverrideApproveEmergencyPurchase(
  accessToken: string,
  purchaseRequestId: string,
  justification: string,
): Promise<EmergencyPurchaseResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/emergency-purchases/${purchaseRequestId}/manager-override-approve`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ justification }),
    },
  )
  return parseJsonResponse<EmergencyPurchaseResponse>(
    response,
    'Failed to manager-override approve emergency purchase',
  )
}

export async function issueEmergencyPurchaseOrder(
  accessToken: string,
  purchaseRequestId: string,
  orderKey: string,
): Promise<IssueEmergencyPurchaseOrderResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/emergency-purchases/${purchaseRequestId}/issue-purchase-order`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ orderKey, title: null, notes: null }),
    },
  )
  return parseJsonResponse<IssueEmergencyPurchaseOrderResponse>(
    response,
    'Failed to issue emergency purchase order',
  )
}

export async function getSupplierOnboardingDocumentRequirements(
  accessToken: string,
): Promise<SupplierOnboardingDocumentRequirementsResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/document-requirements`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOnboardingDocumentRequirementsResponse>(
    response,
    'Failed to load onboarding document requirements',
  )
}

export async function listPendingSupplierOnboarding(
  accessToken: string,
): Promise<SupplierOnboardingResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOnboardingResponse[]>(
    response,
    'Failed to load pending supplier onboarding',
  )
}

export async function startSupplierOnboarding(
  accessToken: string,
  externalPartyId: string,
  notes?: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/start`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ externalPartyId, notes: notes ?? null }),
  })
  return parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to start supplier onboarding')
}

export async function getSupplierOnboardingByParty(
  accessToken: string,
  partyId: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/parties/${partyId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to load supplier onboarding')
}

export async function submitSupplierOnboarding(
  accessToken: string,
  partyId: string,
  notes?: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/parties/${partyId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ notes: notes ?? null }),
  })
  return parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to submit supplier onboarding')
}

export async function approveSupplierOnboarding(
  accessToken: string,
  partyId: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/parties/${partyId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to approve supplier onboarding')
}

export async function rejectSupplierOnboarding(
  accessToken: string,
  partyId: string,
  reason: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/parties/${partyId}/reject`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ reason }),
  })
  return parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to reject supplier onboarding')
}

export async function listPartyComplianceDocuments(
  accessToken: string,
  partyId: string,
): Promise<PartyComplianceDocumentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/parties/${partyId}/compliance-documents`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartyComplianceDocumentResponse[]>(
    response,
    'Failed to load party compliance documents',
  )
}

export async function registerPartyComplianceDocument(
  accessToken: string,
  partyId: string,
  payload: {
    documentKey: string
    documentTypeKey: string
    title: string
    expiresAt?: string | null
    effectiveAt?: string | null
    fileName: string
    contentType: string
    sizeBytes: number
    notes: string
  },
): Promise<PartyComplianceDocumentResponse> {
  const response = await fetch(`${apiBase}/api/v1/parties/${partyId}/compliance-documents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PartyComplianceDocumentResponse>(
    response,
    'Failed to register compliance document',
  )
}

export async function approvePartyComplianceDocument(
  accessToken: string,
  partyId: string,
  documentId: string,
): Promise<PartyComplianceDocumentResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/parties/${partyId}/compliance-documents/${documentId}/approve`,
    { method: 'POST', headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PartyComplianceDocumentResponse>(
    response,
    'Failed to approve compliance document',
  )
}

export async function exportComplianceReportSummaryCsv(
  accessToken: string,
  options?: { attentionOnly?: boolean; partyType?: string; externalPartyId?: string },
): Promise<Blob> {
  const search = new URLSearchParams()
  if (options?.attentionOnly) {
    search.set('attentionOnly', 'true')
  }
  if (options?.partyType) {
    search.set('partyType', options.partyType)
  }
  if (options?.externalPartyId) {
    search.set('externalPartyId', options.externalPartyId)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/reports/compliance/summary/export${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to export compliance report')
  }
  return response.blob()
}

export async function listVendorRestrictions(
  accessToken: string,
  options?: { status?: string } | string,
): Promise<VendorRestrictionResponse[]> {
  const status = typeof options === 'string' ? options : options?.status
  const search = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/vendor-restrictions${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorRestrictionResponse[]>(response, 'Failed to load vendor restrictions')
}

export async function listPartyVendorRestrictions(
  accessToken: string,
  partyId: string,
): Promise<VendorRestrictionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/parties/${partyId}/vendor-restrictions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorRestrictionResponse[]>(
    response,
    'Failed to load party vendor restrictions',
  )
}

export async function getPartyVendorRestrictionEnforcement(
  accessToken: string,
  partyId: string,
): Promise<VendorRestrictionEnforcementResponse> {
  const response = await fetch(`${apiBase}/api/v1/parties/${partyId}/vendor-restrictions/enforcement`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorRestrictionEnforcementResponse>(
    response,
    'Failed to load vendor restriction enforcement',
  )
}

export async function createPartyVendorRestriction(
  accessToken: string,
  partyId: string,
  payload: CreateVendorRestrictionRequest,
): Promise<VendorRestrictionResponse> {
  const response = await fetch(`${apiBase}/api/v1/parties/${partyId}/vendor-restrictions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorRestrictionResponse>(response, 'Failed to create vendor restriction')
}

export async function liftVendorRestriction(
  accessToken: string,
  restrictionId: string,
  payload: LiftVendorRestrictionRequest,
): Promise<VendorRestrictionResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-restrictions/${restrictionId}/lift`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorRestrictionResponse>(response, 'Failed to lift vendor restriction')
}

export async function listSupplierIncidents(
  accessToken: string,
  options?: { status?: string; externalPartyId?: string; severity?: string },
): Promise<SupplierIncidentResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) search.set('status', options.status)
  if (options?.externalPartyId) search.set('externalPartyId', options.externalPartyId)
  if (options?.severity) search.set('severity', options.severity)
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierIncidentResponse[]>(response, 'Failed to load supplier incidents')
}

export async function listPartySupplierIncidents(
  accessToken: string,
  partyId: string,
): Promise<SupplierIncidentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/parties/${partyId}/supplier-incidents`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierIncidentResponse[]>(
    response,
    'Failed to load party supplier incidents',
  )
}

export async function createSupplierIncident(
  accessToken: string,
  payload: CreateSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SupplierIncidentResponse>(response, 'Failed to create supplier incident')
}

export async function startSupplierIncidentInvestigation(
  accessToken: string,
  incidentId: string,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/start-investigation`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierIncidentResponse>(response, 'Failed to start investigation')
}

export async function resolveSupplierIncident(
  accessToken: string,
  incidentId: string,
  payload: ResolveSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SupplierIncidentResponse>(response, 'Failed to resolve supplier incident')
}

export async function closeSupplierIncident(
  accessToken: string,
  incidentId: string,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/close`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ resolutionNotes: null }),
  })
  return parseJsonResponse<SupplierIncidentResponse>(response, 'Failed to close supplier incident')
}

export async function cancelSupplierIncident(
  accessToken: string,
  incidentId: string,
  payload: CancelSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SupplierIncidentResponse>(response, 'Failed to cancel supplier incident')
}

export async function reopenSupplierIncident(
  accessToken: string,
  incidentId: string,
  payload: ReopenSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/reopen`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SupplierIncidentResponse>(response, 'Failed to reopen supplier incident')
}

export async function applySupplierIncidentProcurementRestriction(
  accessToken: string,
  incidentId: string,
  payload: ApplySupplierIncidentProcurementRestrictionRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/supplier-incidents/${incidentId}/apply-procurement-restriction`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  return parseJsonResponse<SupplierIncidentResponse>(
    response,
    'Failed to apply procurement restriction from incident',
  )
}

export async function listProcurementExceptionResolutionTemplates(
  accessToken: string,
): Promise<ProcurementExceptionResolutionTemplateResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/resolution-templates`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementExceptionResolutionTemplateResponse[]>(
    response,
    'Failed to load procurement exception resolution templates',
  )
}

export async function listProcurementExceptions(
  accessToken: string,
  options?: {
    status?: string
    subjectType?: string
    subjectId?: string
    overdueOnly?: boolean
  },
): Promise<ProcurementExceptionResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) search.set('status', options.status)
  if (options?.subjectType) search.set('subjectType', options.subjectType)
  if (options?.subjectId) search.set('subjectId', options.subjectId)
  if (options?.overdueOnly) search.set('overdueOnly', 'true')
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementExceptionResponse[]>(
    response,
    'Failed to load procurement exceptions',
  )
}

export async function listSubjectProcurementExceptions(
  accessToken: string,
  subjectType: 'purchase_request' | 'purchase_order' | 'rfq',
  subjectId: string,
): Promise<ProcurementExceptionResponse[]> {
  const route =
    subjectType === 'purchase_request'
      ? `/api/v1/purchase-requests/${subjectId}/procurement-exceptions`
      : subjectType === 'purchase_order'
        ? `/api/v1/purchase-orders/${subjectId}/procurement-exceptions`
        : `/api/v1/rfqs/${subjectId}/procurement-exceptions`
  const response = await fetch(`${apiBase}${route}`, { headers: authHeaders(accessToken) })
  return parseJsonResponse<ProcurementExceptionResponse[]>(
    response,
    'Failed to load subject procurement exceptions',
  )
}

export async function createSubjectProcurementException(
  accessToken: string,
  subjectType: 'purchase_request' | 'purchase_order' | 'rfq',
  subjectId: string,
  payload: CreateProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const route =
    subjectType === 'purchase_request'
      ? `/api/v1/purchase-requests/${subjectId}/procurement-exceptions`
      : subjectType === 'purchase_order'
        ? `/api/v1/purchase-orders/${subjectId}/procurement-exceptions`
        : `/api/v1/rfqs/${subjectId}/procurement-exceptions`
  const response = await fetch(`${apiBase}${route}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to create procurement exception',
  )
}

export async function assignProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: AssignProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/assign`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to assign procurement exception',
  )
}

export async function linkProcurementExceptionActions(
  accessToken: string,
  exceptionId: string,
  payload: LinkProcurementExceptionActionsRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/link-actions`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to link procurement exception actions',
  )
}

export async function startProcurementExceptionInvestigation(
  accessToken: string,
  exceptionId: string,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/start-investigation`,
    { method: 'POST', headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to start procurement exception investigation',
  )
}

export async function resolveProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: ResolveProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to resolve procurement exception',
  )
}

export async function requestProcurementExceptionWaive(
  accessToken: string,
  exceptionId: string,
  payload: RequestProcurementExceptionWaiveRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/request-waive`,
    { method: 'POST', headers: authHeaders(accessToken), body: JSON.stringify(payload) },
  )
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to request procurement exception waive',
  )
}

export async function approveProcurementExceptionWaive(
  accessToken: string,
  exceptionId: string,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/approve-waive`,
    { method: 'POST', headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to approve procurement exception waive',
  )
}

export async function rejectProcurementExceptionWaive(
  accessToken: string,
  exceptionId: string,
  payload: RejectProcurementExceptionWaiveRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/reject-waive`,
    { method: 'POST', headers: authHeaders(accessToken), body: JSON.stringify(payload) },
  )
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to reject procurement exception waive',
  )
}

export async function closeProcurementException(
  accessToken: string,
  exceptionId: string,
  payload?: CloseProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/close`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload ?? { resolutionNotes: null }),
  })
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to close procurement exception',
  )
}

export async function cancelProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: CancelProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to cancel procurement exception',
  )
}

export async function reopenProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: ReopenProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/reopen`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to reopen procurement exception',
  )
}

export async function getProcurementApprovalAuthority(
  accessToken: string,
): Promise<ProcurementApprovalAuthorityMirrorResponse> {
  const response = await fetch(`${apiBase}/api/me/procurement-approval-authority`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementApprovalAuthorityMirrorResponse>(
    response,
    'Failed to load StaffArr procurement approval authority',
  )
}
