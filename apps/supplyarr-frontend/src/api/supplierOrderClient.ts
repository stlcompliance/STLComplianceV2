import type {
  CreateSupplierOrderBrokerDecisionRequest,
  CreateSupplierOrderMagicLinkResponse,
  CreateSupplierOrderRequest,
  RegisterSupplierOrderDocumentRequest,
  SendSupplierOrderResponse,
  SplitSupplierOrderRequest,
  SplitSupplierOrderResponse,
  SupplierOrderListItemResponse,
  SupplierOrderMetadataResponse,
  SupplierOrderPortalResponse,
  SupplierOrderResponse,
  SupplierOrderSettingsResponse,
  SupplierOrderStatusUpdateResponse,
  UpdateSupplierOrderRequest,
  UpdateSupplierOrderStatusRequest,
  UpsertSupplierOrderSettingsRequest,
} from './types'

const apiBase = import.meta.env.VITE_SUPPLYARR_API_BASE ?? ''
const supplierOrdersApiPath = `${apiBase}/api/v1/supplier-orders`
const supplierOrderSettingsApiPath = `${apiBase}/api/v1/supplier-order-settings`
const supplierOrderPortalApiPath = `${apiBase}/api/v1/supplier-access/orders`

function normalizeSupplierOrderListItem(raw: SupplierOrderListItemResponse): SupplierOrderListItemResponse {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplierOrder(raw: SupplierOrderResponse): SupplierOrderResponse {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplierOrderPortal(raw: SupplierOrderPortalResponse): SupplierOrderPortalResponse {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

class SupplyArrSupplierOrderApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'SupplyArrSupplierOrderApiError'
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

async function toApiError(response: Response, fallbackMessage: string): Promise<SupplyArrSupplierOrderApiError> {
  const body = await response.text()
  const message = extractProblemMessage(body) || body || `${fallbackMessage} (${response.status})`
  return new SupplyArrSupplierOrderApiError(message, response.status, body)
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

export async function getSupplierOrders(
  accessToken: string,
  options?: { status?: string; supplierId?: string },
): Promise<SupplierOrderListItemResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) {
    search.set('status', options.status)
  }
  if (options?.supplierId) {
    search.set('supplierId', options.supplierId)
  }

  const suffix = search.size > 0 ? `?${search.toString()}` : ''
  const response = await fetch(`${supplierOrdersApiPath}${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return (await parseJsonResponse<SupplierOrderListItemResponse[]>(response, 'Failed to load supplier orders'))
    .map(normalizeSupplierOrderListItem)
}

export async function getSupplierOrderMetadata(accessToken: string): Promise<SupplierOrderMetadataResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/metadata`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOrderMetadataResponse>(response, 'Failed to load supplier-order metadata')
}

export async function getSupplierOrder(
  accessToken: string,
  supplierOrderId: string,
): Promise<SupplierOrderResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierOrder(
    await parseJsonResponse<SupplierOrderResponse>(response, 'Failed to load supplier order'),
  )
}

export async function createSupplierOrder(
  accessToken: string,
  payload: CreateSupplierOrderRequest,
): Promise<SupplierOrderResponse> {
  const response = await fetch(supplierOrdersApiPath, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeSupplierOrder(
    await parseJsonResponse<SupplierOrderResponse>(response, 'Failed to create supplier order'),
  )
}

export async function updateSupplierOrder(
  accessToken: string,
  supplierOrderId: string,
  payload: UpdateSupplierOrderRequest,
): Promise<SupplierOrderResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeSupplierOrder(
    await parseJsonResponse<SupplierOrderResponse>(response, 'Failed to update supplier order'),
  )
}

export async function sendSupplierOrderToSupplier(
  accessToken: string,
  supplierOrderId: string,
): Promise<SendSupplierOrderResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}/send-to-supplier`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SendSupplierOrderResponse>(response, 'Failed to send supplier order')
}

export async function submitSupplierOrderStatus(
  accessToken: string,
  supplierOrderId: string,
  payload: UpdateSupplierOrderStatusRequest,
): Promise<SupplierOrderResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}/status`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeSupplierOrder(
    await parseJsonResponse<SupplierOrderResponse>(response, 'Failed to update supplier-order status'),
  )
}

export async function getSupplierOrderHistory(
  accessToken: string,
  supplierOrderId: string,
): Promise<SupplierOrderStatusUpdateResponse[]> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}/status-history`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOrderStatusUpdateResponse[]>(response, 'Failed to load supplier-order history')
}

export async function createSupplierOrderMagicLink(
  accessToken: string,
  supplierOrderId: string,
): Promise<CreateSupplierOrderMagicLinkResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}/magic-link`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CreateSupplierOrderMagicLinkResponse>(response, 'Failed to create magic link')
}

export async function registerSupplierOrderDocument(
  accessToken: string,
  supplierOrderId: string,
  payload: RegisterSupplierOrderDocumentRequest,
): Promise<SupplierOrderResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}/documents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeSupplierOrder(
    await parseJsonResponse<SupplierOrderResponse>(response, 'Failed to register supplier-order document'),
  )
}

export async function createSupplierOrderBrokerDecision(
  accessToken: string,
  supplierOrderId: string,
  payload: CreateSupplierOrderBrokerDecisionRequest,
): Promise<unknown> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}/partial-decision`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<unknown>(response, 'Failed to record supplier partial decision')
}

export async function splitSupplierOrder(
  accessToken: string,
  supplierOrderId: string,
  payload: SplitSupplierOrderRequest,
): Promise<SplitSupplierOrderResponse> {
  const response = await fetch(`${supplierOrdersApiPath}/${supplierOrderId}/split`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SplitSupplierOrderResponse>(response, 'Failed to split supplier order')
}

export async function getSupplierOrderSettings(accessToken: string): Promise<SupplierOrderSettingsResponse> {
  const response = await fetch(supplierOrderSettingsApiPath, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOrderSettingsResponse>(response, 'Failed to load supplier-order settings')
}

export async function upsertSupplierOrderSettings(
  accessToken: string,
  payload: UpsertSupplierOrderSettingsRequest,
): Promise<SupplierOrderSettingsResponse> {
  const response = await fetch(supplierOrderSettingsApiPath, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SupplierOrderSettingsResponse>(response, 'Failed to save supplier-order settings')
}

export async function getSupplierAccessOrder(token: string): Promise<SupplierOrderPortalResponse> {
  const response = await fetch(`${supplierOrderPortalApiPath}/${encodeURIComponent(token)}`)
  return normalizeSupplierOrderPortal(
    await parseJsonResponse<SupplierOrderPortalResponse>(response, 'Failed to load supplier portal order'),
  )
}

export async function submitSupplierAccessOrderStatus(
  token: string,
  payload: UpdateSupplierOrderStatusRequest,
): Promise<SupplierOrderPortalResponse> {
  const response = await fetch(`${supplierOrderPortalApiPath}/${encodeURIComponent(token)}/status`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return normalizeSupplierOrderPortal(
    await parseJsonResponse<SupplierOrderPortalResponse>(response, 'Failed to submit supplier portal status'),
  )
}

export async function registerSupplierAccessOrderDocument(
  token: string,
  payload: RegisterSupplierOrderDocumentRequest,
): Promise<SupplierOrderPortalResponse> {
  const response = await fetch(`${supplierOrderPortalApiPath}/${encodeURIComponent(token)}/documents`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return normalizeSupplierOrderPortal(
    await parseJsonResponse<SupplierOrderPortalResponse>(response, 'Failed to register supplier portal document'),
  )
}
