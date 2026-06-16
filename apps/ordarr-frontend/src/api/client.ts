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
  callbackUrl: string | null
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
  handoffState: string
  completionState: string
  financialPacketState: string
  summary: string
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

export interface OrdArrCompletionPacket {
  packetId: string
  orderNumber?: string | null
  packetType: string
  status: string
  recordRefs: ProductObjectReference[]
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
  featuredOrders: OrdArrOrderSummary[]
  recentActivity: OrdArrActivity[]
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

export async function redeemHandoff(handoffCode: string): Promise<OrdArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/v1/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
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

export async function listOrders(accessToken: string): Promise<OrdArrOrderSummary[]> {
  const response = await fetch(`${apiBase}/api/v1/workspace/orders`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OrdArrOrderSummary[]>(response, 'Failed to load orders')
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
