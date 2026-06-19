const apiBase = import.meta.env.VITE_ORDARR_API_BASE ?? ''

export interface ProductObjectReference {
  productKey: string
  objectType: string
  objectId: string
  objectNumber: string | null
}

export interface OrdArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasOrdArrEntitlement: boolean
  entitlements: string[]
}

export interface OrdArrHandoffSessionResponse {
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

export interface OrdArrOrderLine {
  orderLineId: string
  lineNumber: number
  lineType: string
  itemRef: ProductObjectReference | null
  description: string
  quantity: number
  unitOfMeasure: string
  requestedDate: string | null
  promisedDate: string | null
  unitPrice: number
  discount: number
  taxable: boolean
  allowSubstitution: boolean
  canCancel: boolean
  canReturn: boolean
  targetProductKey: string | null
  complianceFlag: string | null
  linkedDemandReference: string | null
  fulfillmentStatus: string
  allocationStatus: string
  createdAt: string
}

export interface OrdArrHold {
  holdId: string
  holdType: string
  reason: string
  ownerProductKey: string
  ownerPersonId: string
  releasePermission: string
  comment: string | null
  status: string
  createdAt: string
  releasedAt: string | null
  releasedByPersonId: string | null
}

export interface OrdArrTimelineEntry {
  timelineId: string
  eventType: string
  status: string
  message: string
  actorPersonId: string
  sourceProductKey: string
  occurredAt: string
}

export interface OrdArrReturn {
  returnId: string
  returnNumber: string
  returnType: string
  status: string
  reason: string
  quantity: number
  orderLineIds: string[]
  notes: string | null
  sourceReference: string | null
  createdAt: string
  updatedAt: string
}

export interface OrdArrCompletionPacket {
  packetId: string
  orderNumber?: string | null
  packetType: string
  status: string
  recordRefs: ProductObjectReference[]
}

export interface OrdArrHandoff {
  handoffId: string
  orderNumber?: string | null
  targetProductKey: string
  handoffType: string
  state: string
  summary: string
  requestedAt: string
}

export interface OrdArrOrderSummary {
  orderId: string
  orderNumber: string
  requestType: string
  lifecycleStatus: string
  customerRef: ProductObjectReference
  customerName: string
  ownerPersonId: string
  requestedAt: string
  updatedAt: string
  requestedWindowStart: string | null
  requestedWindowEnd: string | null
  promisedWindowStart: string | null
  promisedWindowEnd: string | null
  handoffState: string
  completionState: string
  financialPacketState: string
  summary: string
  sourceChannel: string
  orderType: string
  priority: string
  lineCount: number
  holdCount: number
  approvalState: string
  customerFacingStatus: string
  nextAction: string
}

export interface OrdArrOrderDetail extends OrdArrOrderSummary {
  buyerPoNumber: string | null
  billToRef: ProductObjectReference | null
  shipToRef: ProductObjectReference | null
  shippingMethodPreference: string | null
  paymentTerms: string | null
  customerNotes: string | null
  internalNotes: string | null
  sourceReference: string | null
  handoffs: OrdArrHandoff[]
  completionPackets: OrdArrCompletionPacket[]
  events: { eventId: string; eventType: string; message: string; occurredAt: string }[]
  lines: OrdArrOrderLine[]
  holds: OrdArrHold[]
  timeline: OrdArrTimelineEntry[]
  returns: OrdArrReturn[]
}

export interface OrdArrActivity {
  activityId: string
  orderId: string
  orderNumber: string
  eventType: string
  message: string
  occurredAt: string
}

export interface OrdArrDashboardResponse {
  generatedAt: string
  orderCount: number
  requestCount: number
  activeHandoffCount: number
  completionPacketCount: number
  invoiceReadyPacketCount: number
  billReadyPacketCount: number
  openOrderCount: number
  openHoldCount: number
  blockedOrderCount: number
  lateOrderCount: number
  returnCount: number
  featuredOrders: OrdArrOrderSummary[]
  recentActivity: OrdArrActivity[]
}

export interface OrdArrReportSummaryResponse {
  generatedAt: string
  orderCount: number
  openOrderCount: number
  closedOrderCount: number
  blockedOrderCount: number
  openHoldCount: number
  lateOrderCount: number
  lineCount: number
  returnedQuantity: number
  fillRatePercent: number
  onTimePercent: number
  activeHandoffCount: number
  returnCount: number
  completionPacketCount: number
  invoiceReadyPacketCount: number
  billReadyPacketCount: number
  featuredOrders: OrdArrOrderSummary[]
}

export interface OrdArrOrderLineRequest {
  lineType: string
  itemRef?: ProductObjectReference | null
  description: string
  quantity: number
  unitOfMeasure: string
  targetProductKey?: string | null
  requestedDate?: string | null
  promisedDate?: string | null
  unitPrice?: number
  discount?: number
  taxable?: boolean
  allowSubstitution?: boolean
  canCancel?: boolean
  canReturn?: boolean
  complianceFlag?: string | null
  linkedDemandReference?: string | null
}

export interface OrdArrCreateOrderRequest {
  customerRef: ProductObjectReference
  customerName: string
  requestType: string
  ownerPersonId: string
  summary: string
  requestedWindowStart?: string | null
  requestedWindowEnd?: string | null
  promisedWindowStart?: string | null
  promisedWindowEnd?: string | null
  fulfillmentProductKeys?: string[] | null
  sourceChannel?: string
  orderType?: string
  priority?: string
  buyerPoNumber?: string | null
  billToRef?: ProductObjectReference | null
  shipToRef?: ProductObjectReference | null
  shippingMethodPreference?: string | null
  paymentTerms?: string | null
  customerNotes?: string | null
  internalNotes?: string | null
  sourceReference?: string | null
  lines?: OrdArrOrderLineRequest[] | null
}

export interface OrdArrSubmitOrderRequest {
  comment?: string | null
}

export interface OrdArrHoldRequest {
  holdType: string
  reason: string
  ownerProductKey: string
  releasePermission?: string | null
  comment?: string | null
  ownerPersonId?: string | null
}

export interface OrdArrReleaseHoldRequest {
  comment?: string | null
  releasedByPersonId?: string | null
}

export interface OrdArrAcceptOrderRequest {
  promisedWindowStart?: string | null
  promisedWindowEnd?: string | null
  fulfillmentProductKeys?: string[] | null
  reason?: string | null
}

export interface OrdArrCancelOrderRequest {
  reason: string
}

export interface OrdArrReturnRequest {
  returnType: string
  reason: string
  quantity: number
  orderLineIds?: string[] | null
  notes?: string | null
  sourceReference?: string | null
}

export interface OrdArrReadinessResponse {
  orderId: string
  orderNumber: string
  isReady: boolean
  lifecycleStatus: string
  handoffState: string
  completionState: string
  financialPacketState: string
  blockingReasons: string[]
  meta: {
    tenantId: string
    sourceProduct: string
    resourceType: string
    resourceId: string
    freshness: string
    fetchedAt: string
  }
}

class OrdArrApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message)
    this.name = 'OrdArrApiError'
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
    throw new OrdArrApiError(body || `${fallbackMessage} (${response.status})`, response.status)
  }

  return (await response.json()) as T
}

async function parseMaybeJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T | null> {
  if (response.status === 404) {
    return null
  }

  return parseJsonResponse<T>(response, fallbackMessage)
}

function jsonBody<T>(value: T): string {
  return JSON.stringify(value)
}

export async function redeemHandoff(handoffCode: string): Promise<OrdArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/v1/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: jsonBody({ handoffCode }),
  })
  return parseJsonResponse<OrdArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getSessionBootstrap(accessToken: string): Promise<OrdArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/v1/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrSessionBootstrapResponse>(response, 'Failed to load session bootstrap')
}

export async function getDashboard(accessToken: string): Promise<OrdArrDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/workspace/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrDashboardResponse>(response, 'Failed to load dashboard')
}

export async function getReportSummary(accessToken: string): Promise<OrdArrReportSummaryResponse> {
  const response = await fetch(`${apiBase}/api/v1/workspace/reports/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrReportSummaryResponse>(response, 'Failed to load report summary')
}

export async function listOrders(accessToken: string): Promise<OrdArrOrderSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/workspace/orders`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrOrderSummary[]>(response, 'Failed to load orders')
}

export async function getOrder(accessToken: string, orderId: string): Promise<OrdArrOrderDetail | null> {
  const response = await fetch(`${apiBase}/api/v1/workspace/orders/${encodeURIComponent(orderId)}`, {
    headers: authHeaders(accessToken),
  })
  return parseMaybeJsonResponse<OrdArrOrderDetail>(response, 'Failed to load order detail')
}

export async function listOrderLines(accessToken: string, orderId: string): Promise<OrdArrOrderLine[]> {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/lines`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrOrderLine[]>(response, 'Failed to load order lines')
}

export async function listOrderHolds(accessToken: string, orderId: string): Promise<OrdArrHold[]> {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/holds`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrHold[]>(response, 'Failed to load order holds')
}

export async function listOrderTimeline(accessToken: string, orderId: string): Promise<OrdArrTimelineEntry[]> {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/timeline`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrTimelineEntry[]>(response, 'Failed to load order timeline')
}

export async function listOrderReturns(accessToken: string, orderId: string): Promise<OrdArrReturn[]> {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/returns`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrReturn[]>(response, 'Failed to load order returns')
}

export async function listHandoffs(accessToken: string): Promise<OrdArrHandoff[]> {
  const response = await fetch(`${apiBase}/api/v1/workspace/handoffs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrHandoff[]>(response, 'Failed to load handoffs')
}

export async function listCompletionPackets(accessToken: string): Promise<OrdArrCompletionPacket[]> {
  const response = await fetch(`${apiBase}/api/v1/workspace/completion-packets`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrCompletionPacket[]>(response, 'Failed to load completion packets')
}

export async function createOrder(accessToken: string, request: OrdArrCreateOrderRequest, idempotencyKey: string) {
  const response = await fetch(`${apiBase}/api/v1/orders`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': idempotencyKey,
    },
    body: jsonBody(request),
  })
  return parseJsonResponse<OrdArrOrderDetail>(response, 'Failed to create order')
}

export async function submitOrder(accessToken: string, orderId: string, request: OrdArrSubmitOrderRequest, idempotencyKey: string) {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/submit`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': idempotencyKey,
    },
    body: jsonBody(request),
  })
  return parseMaybeJsonResponse<OrdArrOrderDetail>(response, 'Failed to submit order')
}

export async function addOrderLine(accessToken: string, orderId: string, request: OrdArrOrderLineRequest, idempotencyKey: string) {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/lines`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': idempotencyKey,
    },
    body: jsonBody(request),
  })
  return parseMaybeJsonResponse<OrdArrOrderDetail>(response, 'Failed to add order line')
}

export async function addHold(accessToken: string, orderId: string, request: OrdArrHoldRequest, idempotencyKey: string) {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/holds`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': idempotencyKey,
    },
    body: jsonBody(request),
  })
  return parseMaybeJsonResponse<OrdArrOrderDetail>(response, 'Failed to add hold')
}

export async function releaseHold(
  accessToken: string,
  orderId: string,
  holdId: string,
  request: OrdArrReleaseHoldRequest,
  idempotencyKey: string,
) {
  const response = await fetch(
    `${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/holds/${encodeURIComponent(holdId)}/release`,
    {
      method: 'POST',
      headers: {
        ...authHeaders(accessToken),
        'Idempotency-Key': idempotencyKey,
      },
      body: jsonBody(request),
    },
  )
  return parseMaybeJsonResponse<OrdArrOrderDetail>(response, 'Failed to release hold')
}

export async function approveOrder(accessToken: string, orderId: string, request: OrdArrAcceptOrderRequest, idempotencyKey: string) {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/approve`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': idempotencyKey,
    },
    body: jsonBody(request),
  })
  return parseMaybeJsonResponse<OrdArrOrderDetail>(response, 'Failed to approve order')
}

export async function cancelOrder(accessToken: string, orderId: string, request: OrdArrCancelOrderRequest, idempotencyKey: string) {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/cancel`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': idempotencyKey,
    },
    body: jsonBody(request),
  })
  return parseMaybeJsonResponse<OrdArrOrderDetail>(response, 'Failed to cancel order')
}

export async function createReturn(accessToken: string, orderId: string, request: OrdArrReturnRequest, idempotencyKey: string) {
  const response = await fetch(`${apiBase}/api/v1/orders/${encodeURIComponent(orderId)}/returns`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Idempotency-Key': idempotencyKey,
    },
    body: jsonBody(request),
  })
  return parseMaybeJsonResponse<OrdArrReturn>(response, 'Failed to create return')
}
