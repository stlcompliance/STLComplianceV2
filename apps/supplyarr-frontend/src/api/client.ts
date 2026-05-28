import type {
  CreateInventoryBinRequest,
  CreateInventoryLocationRequest,
  CreatePartCatalogRequest,
  CreatePartRequest,
  CreatePartVendorLinkRequest,
  CreateTypedExternalPartyRequest,
  ExternalPartyResponse,
  HandoffSessionResponse,
  InventoryBinResponse,
  InventoryLocationResponse,
  PartCatalogResponse,
  PartResponse,
  PartStockLevelResponse,
  PartVendorLinkResponse,
  SupplyArrMeResponse,
  UpsertPartStockLevelRequest,
  CreatePurchaseOrderFromPurchaseRequestRequest,
  CreatePurchaseRequestRequest,
  BackorderResponse,
  CancelBackorderRequest,
  CancelVendorReturnRequest,
  CreateBackorderFromPurchaseOrderLineRequest,
  CreateVendorReturnFromPurchaseOrderLineRequest,
  CreateVendorReturnFromStockRequest,
  VendorReturnResponse,
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
  ProcurementCoordinationDashboardResponse,
  ProcurementCoordinationSettingsResponse,
  UpsertProcurementCoordinationSettingsRequest,
  PendingProcurementCoordinationResponse,
  ProcurementCoordinationRunsResponse,
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

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new SupplyArrApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/handoff/redeem`, {
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
  const response = await fetch(`${apiBase}/api/inventory/locations`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InventoryLocationResponse[]>(response, 'Failed to load inventory locations')
}

export async function getInventoryBins(
  accessToken: string,
  locationId: string,
): Promise<InventoryBinResponse[]> {
  const response = await fetch(`${apiBase}/api/inventory/locations/${locationId}/bins`, {
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
  const url = query ? `${apiBase}/api/inventory/stock?${query}` : `${apiBase}/api/inventory/stock`
  const response = await fetch(url, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartStockLevelResponse[]>(response, 'Failed to load stock levels')
}

export async function createInventoryLocation(
  accessToken: string,
  request: CreateInventoryLocationRequest,
): Promise<InventoryLocationResponse> {
  const response = await fetch(`${apiBase}/api/inventory/locations`, {
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
  const response = await fetch(`${apiBase}/api/inventory/locations/${locationId}/bins`, {
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
  const response = await fetch(`${apiBase}/api/inventory/stock`, {
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
  const response = await fetch(`${apiBase}/api/purchase-requests${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseRequestResponse[]>(response, 'Failed to load purchase requests')
}

export async function createPurchaseRequest(
  accessToken: string,
  request: CreatePurchaseRequestRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/purchase-requests`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to create purchase request')
}

export async function submitPurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/purchase-requests/${purchaseRequestId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to submit purchase request')
}

export async function approvePurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/purchase-requests/${purchaseRequestId}/approve`, {
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
  const response = await fetch(`${apiBase}/api/purchase-requests/${purchaseRequestId}/reject`, {
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
  const response = await fetch(`${apiBase}/api/purchase-orders${query}`, {
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
    `${apiBase}/api/purchase-orders/from-purchase-request/${purchaseRequestId}`,
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
  const response = await fetch(`${apiBase}/api/purchase-orders/${purchaseOrderId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to approve purchase order')
}

export async function issuePurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(`${apiBase}/api/purchase-orders/${purchaseOrderId}/issue`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to issue purchase order')
}

export async function getReceivingReceipt(
  accessToken: string,
  receivingReceiptId: string,
): Promise<ReceivingReceiptResponse> {
  const response = await fetch(`${apiBase}/api/receiving/${receivingReceiptId}`, {
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
  const response = await fetch(`${apiBase}/api/receiving${query}`, {
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
    `${apiBase}/api/receiving/from-purchase-order/${purchaseOrderId}`,
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
  const response = await fetch(`${apiBase}/api/receiving/${receivingReceiptId}/post`, {
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
    `${apiBase}/api/receiving/${receivingReceiptId}/lines/${lineId}`,
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
    `${apiBase}/api/receiving/${receivingReceiptId}/lines/${lineId}/exceptions`,
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
    `${apiBase}/api/receiving/exceptions/${receivingExceptionId}/resolve`,
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
  const response = await fetch(`${apiBase}/api/backorders${query}`, {
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
    `${apiBase}/api/backorders/from-purchase-order-line/${purchaseOrderLineId}`,
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
  const response = await fetch(`${apiBase}/api/backorders/${backorderId}/fulfill`, {
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
  const response = await fetch(`${apiBase}/api/backorders/${backorderId}/cancel`, {
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
  const response = await fetch(`${apiBase}/api/returns${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorReturnResponse[]>(response, 'Failed to load vendor returns')
}

export async function createVendorReturnFromStock(
  accessToken: string,
  request: CreateVendorReturnFromStockRequest,
): Promise<VendorReturnResponse> {
  const response = await fetch(`${apiBase}/api/returns/from-stock`, {
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
    `${apiBase}/api/returns/from-purchase-order-line/${purchaseOrderLineId}`,
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
  const response = await fetch(`${apiBase}/api/returns/${returnId}/post`, {
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
  const response = await fetch(`${apiBase}/api/returns/${returnId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<VendorReturnResponse>(response, 'Failed to cancel vendor return')
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
  const response = await fetch(`${apiBase}/api/pricing-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PricingSnapshotResponse[]>(response, 'Failed to load pricing snapshots')
}

export async function createPricingSnapshot(
  accessToken: string,
  request: CreatePricingSnapshotRequest,
): Promise<PricingSnapshotResponse> {
  const response = await fetch(`${apiBase}/api/pricing-snapshots`, {
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
  const response = await fetch(`${apiBase}/api/lead-time-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LeadTimeSnapshotResponse[]>(response, 'Failed to load lead-time snapshots')
}

export async function createLeadTimeSnapshot(
  accessToken: string,
  request: CreateLeadTimeSnapshotRequest,
): Promise<LeadTimeSnapshotResponse> {
  const response = await fetch(`${apiBase}/api/lead-time-snapshots`, {
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
  const response = await fetch(`${apiBase}/api/availability-snapshots${query}`, {
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
  const response = await fetch(`${apiBase}/api/availability-snapshots`, {
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
  const response = await fetch(`${apiBase}/api/reorder-evaluation`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReorderEvaluationResponse>(response, 'Failed to load reorder evaluation')
}

export async function upsertPartReorderPolicy(
  accessToken: string,
  partId: string,
  request: UpsertPartReorderPolicyRequest,
): Promise<PartReorderPolicyResponse> {
  const response = await fetch(`${apiBase}/api/reorder-evaluation/parts/${partId}/policy`, {
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
  const response = await fetch(`${apiBase}/api/reorder-evaluation/create-purchase-request`, {
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
  const response = await fetch(`${apiBase}/api/demand-refs${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrDemandRefResponse[]>(response, 'Failed to load demand references')
}

export async function createPurchaseRequestFromDemandRef(
  accessToken: string,
  demandRefId: string,
  request: CreatePurchaseRequestFromDemandRefRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/demand-refs/${demandRefId}/create-purchase-request`, {
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
  const response = await fetch(`${apiBase}/api/notification-settings`, {
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
  const response = await fetch(`${apiBase}/api/notification-settings`, {
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
  const response = await fetch(`${apiBase}/api/notification-settings/dispatches?${search}`, {
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
  const response = await fetch(`${apiBase}/api/price-snapshot-settings`, {
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
  const response = await fetch(`${apiBase}/api/price-snapshot-settings`, {
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
  const response = await fetch(`${apiBase}/api/price-snapshot-settings/pending`, {
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
  const response = await fetch(`${apiBase}/api/price-snapshot-settings/runs?${search}`, {
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
  const response = await fetch(`${apiBase}/api/lead-time-snapshot-settings`, {
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
  const response = await fetch(`${apiBase}/api/lead-time-snapshot-settings`, {
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
  const response = await fetch(`${apiBase}/api/lead-time-snapshot-settings/pending`, {
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
  const response = await fetch(`${apiBase}/api/lead-time-snapshot-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LeadTimeSnapshotRunsResponse>(
    response,
    'Failed to load lead-time snapshot runs',
  )
}

export async function getProcurementCoordinationDashboard(
  accessToken: string,
  activeOnly = true,
): Promise<ProcurementCoordinationDashboardResponse> {
  const search = new URLSearchParams({ activeOnly: String(activeOnly) })
  const response = await fetch(`${apiBase}/api/procurement-coordination?${search}`, {
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
  const response = await fetch(`${apiBase}/api/procurement-coordination-settings`, {
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
  const response = await fetch(`${apiBase}/api/procurement-coordination-settings`, {
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
  const response = await fetch(`${apiBase}/api/procurement-coordination-settings/pending`, {
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
  const response = await fetch(`${apiBase}/api/procurement-coordination-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementCoordinationRunsResponse>(
    response,
    'Failed to load procurement coordination runs',
  )
}
