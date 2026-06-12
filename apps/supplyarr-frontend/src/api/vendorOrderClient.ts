import type {
  CreateVendorOrderBrokerDecisionRequest,
  CreateVendorOrderMagicLinkResponse,
  CreateVendorOrderRequest,
  RegisterVendorOrderDocumentRequest,
  SendVendorOrderResponse,
  SplitVendorOrderRequest,
  SplitVendorOrderResponse,
  UpdateVendorOrderRequest,
  UpdateVendorOrderStatusRequest,
  UpsertVendorOrderSettingsRequest,
  VendorOrderListItemResponse,
  VendorOrderMetadataResponse,
  VendorOrderPortalResponse,
  VendorOrderResponse,
  VendorOrderSettingsResponse,
  VendorOrderStatusUpdateResponse,
} from './types'

const apiBase = import.meta.env.VITE_SUPPLYARR_API_BASE ?? ''

class SupplyArrVendorOrderApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'SupplyArrVendorOrderApiError'
  }
}

type ProblemDetailsLike = {
  title?: string
  detail?: string
  errors?: Record<string, string[] | string>
}

function extractProblemMessage(body: string): string | null {
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

    const flattened = Object.entries(parsed.errors ?? {})
      .flatMap(([field, value]) => (Array.isArray(value) ? value : [value]).map((message) => `${field}: ${String(message).trim()}`))
      .filter((value) => value.length > 0)

    if (flattened.length > 0) {
      parts.push(flattened.join('; '))
    }

    return parts.length > 0 ? parts.join(' - ') : null
  } catch {
    return null
  }
}

async function toApiError(response: Response, fallbackMessage: string): Promise<SupplyArrVendorOrderApiError> {
  const body = await response.text()
  const message = extractProblemMessage(body) || body || `${fallbackMessage} (${response.status})`
  return new SupplyArrVendorOrderApiError(message, response.status, body)
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

export async function getVendorOrders(
  accessToken: string,
  options?: { status?: string; vendorId?: string },
): Promise<VendorOrderListItemResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) {
    search.set('status', options.status)
  }
  if (options?.vendorId) {
    search.set('vendorId', options.vendorId)
  }

  const suffix = search.size > 0 ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/vendor-orders${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorOrderListItemResponse[]>(response, 'Failed to load vendor orders')
}

export async function getVendorOrderMetadata(accessToken: string): Promise<VendorOrderMetadataResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/metadata`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorOrderMetadataResponse>(response, 'Failed to load vendor-order metadata')
}

export async function getVendorOrder(
  accessToken: string,
  vendorOrderId: string,
): Promise<VendorOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorOrderResponse>(response, 'Failed to load vendor order')
}

export async function createVendorOrder(
  accessToken: string,
  payload: CreateVendorOrderRequest,
): Promise<VendorOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorOrderResponse>(response, 'Failed to create vendor order')
}

export async function updateVendorOrder(
  accessToken: string,
  vendorOrderId: string,
  payload: UpdateVendorOrderRequest,
): Promise<VendorOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorOrderResponse>(response, 'Failed to update vendor order')
}

export async function sendVendorOrderToVendor(
  accessToken: string,
  vendorOrderId: string,
): Promise<SendVendorOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}/send-to-vendor`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SendVendorOrderResponse>(response, 'Failed to send vendor order')
}

export async function submitVendorOrderStatus(
  accessToken: string,
  vendorOrderId: string,
  payload: UpdateVendorOrderStatusRequest,
): Promise<VendorOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}/status`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorOrderResponse>(response, 'Failed to update vendor-order status')
}

export async function getVendorOrderHistory(
  accessToken: string,
  vendorOrderId: string,
): Promise<VendorOrderStatusUpdateResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}/status-history`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorOrderStatusUpdateResponse[]>(response, 'Failed to load vendor-order history')
}

export async function createVendorOrderMagicLink(
  accessToken: string,
  vendorOrderId: string,
): Promise<CreateVendorOrderMagicLinkResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}/magic-link`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CreateVendorOrderMagicLinkResponse>(response, 'Failed to create magic link')
}

export async function registerVendorOrderDocument(
  accessToken: string,
  vendorOrderId: string,
  payload: RegisterVendorOrderDocumentRequest,
): Promise<VendorOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}/documents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorOrderResponse>(response, 'Failed to register vendor-order document')
}

export async function createVendorOrderBrokerDecision(
  accessToken: string,
  vendorOrderId: string,
  payload: CreateVendorOrderBrokerDecisionRequest,
): Promise<unknown> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}/partial-decision`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<unknown>(response, 'Failed to record vendor partial decision')
}

export async function splitVendorOrder(
  accessToken: string,
  vendorOrderId: string,
  payload: SplitVendorOrderRequest,
): Promise<SplitVendorOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-orders/${vendorOrderId}/split`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SplitVendorOrderResponse>(response, 'Failed to split vendor order')
}

export async function getVendorOrderSettings(accessToken: string): Promise<VendorOrderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-order-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<VendorOrderSettingsResponse>(response, 'Failed to load vendor-order settings')
}

export async function upsertVendorOrderSettings(
  accessToken: string,
  payload: UpsertVendorOrderSettingsRequest,
): Promise<VendorOrderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-order-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorOrderSettingsResponse>(response, 'Failed to save vendor-order settings')
}

export async function getVendorAccessOrder(token: string): Promise<VendorOrderPortalResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-access/orders/${encodeURIComponent(token)}`)
  return parseJsonResponse<VendorOrderPortalResponse>(response, 'Failed to load vendor portal order')
}

export async function submitVendorAccessOrderStatus(
  token: string,
  payload: UpdateVendorOrderStatusRequest,
): Promise<VendorOrderPortalResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-access/orders/${encodeURIComponent(token)}/status`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorOrderPortalResponse>(response, 'Failed to submit vendor portal status')
}

export async function registerVendorAccessOrderDocument(
  token: string,
  payload: RegisterVendorOrderDocumentRequest,
): Promise<VendorOrderPortalResponse> {
  const response = await fetch(`${apiBase}/api/v1/vendor-access/orders/${encodeURIComponent(token)}/documents`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<VendorOrderPortalResponse>(response, 'Failed to register vendor portal document')
}
