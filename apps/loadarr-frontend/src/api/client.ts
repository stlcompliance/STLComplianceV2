export interface LoadArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasLoadArrEntitlement: boolean
  entitlements: string[]
}

export interface LoadArrHandoffSessionResponse {
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
}

export interface LoadArrPermissionCatalogItemResponse {
  productKey: string
  permissionKey: string
  label: string
  description: string | null
  scope: string
  sensitivity: string
  status: string
}

export interface LoadArrPermissionCatalogResponse {
  permissions: LoadArrPermissionCatalogItemResponse[]
}

export interface LoadArrRouteSurfaceListResponse<TItem> {
  items: TItem[]
  total: number
}

export interface LoadArrRouteSurfaceRecordBase {
  id: string
  status?: string
}

export interface LoadArrExpectedReceiptResponse extends LoadArrRouteSurfaceRecordBase {
  expectedReceiptNumber: string
  sourceProductKey: string
  sourceObjectType: string
  sourceObjectId: string
  supplierNameSnapshot: string
  staffarrSiteOrgUnitId: string
  staffarrSiteNameSnapshot: string
  warehouseLocationId: string
  locationNameSnapshot: string
  supplyarrItemId: string
  itemNameSnapshot: string
  expectedQuantity: number
  receivedQuantity: number
  unitOfMeasure: string
  expectedAtUtc: string
  lastUpdatedAtUtc: string
  receivingSessionId: string | null
  signals: string[]
}

export type LoadArrRouteSurfaceCollectionKey =
  | 'expectedReceipts'
  | 'dockAppointments'
  | 'putawayTasks'
  | 'reservations'
  | 'picking'
  | 'staging'
  | 'shipping'
  | 'loadouts'
  | 'exceptions'
  | 'exceptionReceiving'
  | 'exceptionInventoryHolds'
  | 'exceptionQuarantine'
  | 'exceptionPendingQualityReview'
  | 'supplyPoReceipts'
  | 'supplyVendorReturns'
  | 'supplyBackorders'
  | 'supplyReorderSignals'
  | 'setupLocationRules'
  | 'setupItemReferences'
  | 'setupInventoryPolicies'
  | 'setupDevicesLabels'
  | 'recordsStockLedger'
  | 'recordsReceivingHistory'
  | 'recordsMovementHistory'
  | 'recordsCountHistory'
  | 'recordsAdjustmentHistory'

export type LoadArrRouteSurfaceDetailCollectionKey = Exclude<
  LoadArrRouteSurfaceCollectionKey,
  | 'exceptionReceiving'
  | 'exceptionInventoryHolds'
  | 'exceptionQuarantine'
  | 'exceptionPendingQualityReview'
>

export type LoadArrRouteSurfaceQuery = Record<string, string | number | boolean | null | undefined>

export const loadArrRouteSurfaceCollectionPaths = {
  expectedReceipts: '/api/v1/loadarr/expected-receipts',
  dockAppointments: '/api/v1/loadarr/dock-appointments',
  putawayTasks: '/api/v1/loadarr/putaway-tasks',
  reservations: '/api/v1/loadarr/reservations',
  picking: '/api/v1/loadarr/picking',
  staging: '/api/v1/loadarr/staging',
  shipping: '/api/v1/loadarr/shipping',
  loadouts: '/api/v1/loadarr/loadouts',
  exceptions: '/api/v1/loadarr/exceptions',
  exceptionReceiving: '/api/v1/loadarr/exceptions/receiving',
  exceptionInventoryHolds: '/api/v1/loadarr/exceptions/inventory-holds',
  exceptionQuarantine: '/api/v1/loadarr/exceptions/quarantine',
  exceptionPendingQualityReview: '/api/v1/loadarr/exceptions/pending-quality-review',
  supplyPoReceipts: '/api/v1/loadarr/supply-coordination/po-receipts',
  supplyVendorReturns: '/api/v1/loadarr/supply-coordination/vendor-returns',
  supplyBackorders: '/api/v1/loadarr/supply-coordination/backorders',
  supplyReorderSignals: '/api/v1/loadarr/supply-coordination/reorder-signals',
  setupLocationRules: '/api/v1/loadarr/setup/location-rules',
  setupItemReferences: '/api/v1/loadarr/setup/item-references',
  setupInventoryPolicies: '/api/v1/loadarr/setup/inventory-policies',
  setupDevicesLabels: '/api/v1/loadarr/setup/devices-labels',
  recordsStockLedger: '/api/v1/loadarr/records/stock-ledger',
  recordsReceivingHistory: '/api/v1/loadarr/records/receiving-history',
  recordsMovementHistory: '/api/v1/loadarr/records/movement-history',
  recordsCountHistory: '/api/v1/loadarr/records/count-history',
  recordsAdjustmentHistory: '/api/v1/loadarr/records/adjustment-history',
} satisfies Record<LoadArrRouteSurfaceCollectionKey, string>

const apiBase = import.meta.env.VITE_LOADARR_API_BASE ?? ''

export class LoadArrApiError extends Error {
  readonly status: number
  readonly body: string

  constructor(message: string, status: number, body: string) {
    super(message)
    this.name = 'LoadArrApiError'
    this.status = status
    this.body = body
  }
}

function authHeaders(accessToken: string): Headers {
  return createLoadArrHeaders(accessToken, {
    'Content-Type': 'application/json',
  })
}

export function resolveLoadArrApiUrl(path: string): string {
  return `${apiBase}${path}`
}

export function createLoadArrHeaders(accessToken?: string, headers?: HeadersInit): Headers {
  const requestHeaders = new Headers(headers)
  if (accessToken) {
    requestHeaders.set('Authorization', `Bearer ${accessToken}`)
  }

  return requestHeaders
}

export function loadArrFetch(
  path: string,
  accessToken?: string,
  init: RequestInit = {},
): Promise<Response> {
  return fetch(resolveLoadArrApiUrl(path), {
    ...init,
    headers: createLoadArrHeaders(accessToken, init.headers),
  })
}

function withQuery(path: string, query?: LoadArrRouteSurfaceQuery): string {
  if (!query) {
    return path
  }

  const params = new URLSearchParams()
  for (const [key, value] of Object.entries(query)) {
    if (value !== null && value !== undefined && value !== '') {
      params.set(key, String(value))
    }
  }

  const queryString = params.toString()
  return queryString ? `${path}?${queryString}` : path
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new LoadArrApiError(
      body || `${fallbackMessage} (${response.status})`,
      response.status,
      body,
    )
  }

  return (await response.json()) as T
}

export async function getLoadArrRouteSurfaceCollection<
  TItem extends LoadArrRouteSurfaceRecordBase,
>(
  accessToken: string,
  collectionKey: LoadArrRouteSurfaceCollectionKey,
  query?: LoadArrRouteSurfaceQuery,
): Promise<LoadArrRouteSurfaceListResponse<TItem>> {
  const response = await loadArrFetch(
    withQuery(loadArrRouteSurfaceCollectionPaths[collectionKey], query),
    accessToken,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<LoadArrRouteSurfaceListResponse<TItem>>(
    response,
    'Failed to load LoadArr route surface collection',
  )
}

export async function getLoadArrRouteSurfaceRecord<
  TItem extends LoadArrRouteSurfaceRecordBase,
>(
  accessToken: string,
  collectionKey: LoadArrRouteSurfaceDetailCollectionKey,
  recordId: string,
): Promise<TItem> {
  const path = `${loadArrRouteSurfaceCollectionPaths[collectionKey]}/${encodeURIComponent(recordId)}`
  const response = await loadArrFetch(path, accessToken, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TItem>(response, 'Failed to load LoadArr route surface record')
}

export async function getLoadArrExpectedReceipts(
  accessToken: string,
  query?: LoadArrRouteSurfaceQuery,
): Promise<LoadArrRouteSurfaceListResponse<LoadArrExpectedReceiptResponse>> {
  return getLoadArrRouteSurfaceCollection<LoadArrExpectedReceiptResponse>(
    accessToken,
    'expectedReceipts',
    query,
  )
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<LoadArrSessionBootstrapResponse> {
  const response = await loadArrFetch('/api/session', accessToken, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LoadArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function redeemHandoff(handoffCode: string): Promise<LoadArrHandoffSessionResponse> {
  const response = await loadArrFetch('/api/auth/nexarr/redeem', undefined, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<LoadArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getLoadArrPermissionCatalog(
  accessToken: string,
): Promise<LoadArrPermissionCatalogResponse> {
  const response = await loadArrFetch('/api/v1/admin/permissions', accessToken, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LoadArrPermissionCatalogResponse>(
    response,
    'Failed to load LoadArr permission catalog',
  )
}
