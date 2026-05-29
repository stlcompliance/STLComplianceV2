import {
  clearAuthSession,
  isAccessTokenExpired,
  loadAuthSession,
  saveAuthSession,
  toStoredSession,
  type StoredAuthSession,
} from '../auth/authStorage'
import { getNexarrApiBaseUrl } from './nexarrBaseUrl'
import type {
  ApiErrorBody,
  AuthTokenResponse,
  HandoffCreatedResponse,
  LaunchContextResponse,
  LaunchDiagnosticsResponse,
  LoginRequest,
  EntitlementSummary,
  MeResponse,
  NavigationResponse,
  TenantSummary,
  PagedResult,
  PlatformAdminDashboardResponse,
  ProductOverviewRow,
  PlatformAuditPackageExportPreview,
  PlatformAuditPackageExportSummary,
  PlatformAuditPackageFilterOptions,
  PlatformAuditPackageGenerationJob,
  PlatformAuditPackageManifest,
  PlatformAuditPackageScope,
  PlatformAuditEventTimelineItem,
  ServiceTokenCleanupRunsResponse,
  ServiceTokenCleanupSettings,
  EntitlementReconciliationSettings,
  EntitlementReconciliationRunsResponse,
  PendingEntitlementReconciliationResponse,
  TenantLifecycleSettings,
  TenantLifecycleRunsResponse,
  PendingTenantLifecycleResponse,
  PlatformLifecycleOverviewResponse,
  PlatformWorkerHealthOrchestrationStatusResponse,
  TriggerEntitlementReconciliationOrchestrationResponse,
  TriggerServiceTokenCleanupOrchestrationResponse,
  TriggerTenantLifecycleOrchestrationResponse,
  TenantOverviewRow,
} from './types'
import { NexarrApiError } from './types'

type TokenProvider = () => string | null
type SessionUpdater = (session: StoredAuthSession) => void

let accessTokenProvider: TokenProvider = () => loadAuthSession()?.accessToken ?? null
let onSessionUpdated: SessionUpdater = saveAuthSession

export function configureNexarrClient(options: {
  getAccessToken?: TokenProvider
  onSessionUpdated?: SessionUpdater
}): void {
  if (options.getAccessToken) {
    accessTokenProvider = options.getAccessToken
  }
  if (options.onSessionUpdated) {
    onSessionUpdated = options.onSessionUpdated
  }
}

function apiUrl(path: string): string {
  const base = getNexarrApiBaseUrl()
  return `${base}${path}`
}

async function parseError(response: Response): Promise<InstanceType<typeof NexarrApiError>> {
  let body: ApiErrorBody | undefined
  try {
    body = (await response.json()) as ApiErrorBody
  } catch {
    body = undefined
  }
  return new NexarrApiError(
    response.status,
    body?.message ?? response.statusText,
    body?.code,
  )
}

async function fetchWithAuth(
  path: string,
  init: RequestInit = {},
  retryOnUnauthorized = true,
): Promise<Response> {
  const headers = new Headers(init.headers)
  if (!headers.has('Content-Type') && init.body) {
    headers.set('Content-Type', 'application/json')
  }

  const token = accessTokenProvider()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(apiUrl(path), { ...init, headers })

  if (response.status === 401 && retryOnUnauthorized) {
    const renewed = await tryRenewSession()
    if (renewed) {
      return fetchWithAuth(path, init, false)
    }
  }

  return response
}

async function tryRenewSession(): Promise<boolean> {
  const session = loadAuthSession()
  if (!session?.refreshToken) {
    return false
  }

  const response = await fetch(apiUrl('/api/auth/renew'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: session.refreshToken }),
  })

  if (!response.ok) {
    clearAuthSession()
    return false
  }

  const tokens = (await response.json()) as AuthTokenResponse
  const updated = toStoredSession(tokens)
  saveAuthSession(updated)
  onSessionUpdated(updated)
  return true
}

async function ensureValidAccessToken(): Promise<boolean> {
  const session = loadAuthSession()
  if (!session) {
    return false
  }
  if (!isAccessTokenExpired(session)) {
    return true
  }
  return tryRenewSession()
}

export async function login(request: LoginRequest): Promise<StoredAuthSession> {
  const response = await fetch(apiUrl('/api/auth/login'), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw await parseError(response)
  }

  const tokens = (await response.json()) as AuthTokenResponse
  const session = toStoredSession(tokens)
  saveAuthSession(session)
  onSessionUpdated(session)
  return session
}

export async function logout(): Promise<void> {
  const session = loadAuthSession()
  if (session?.refreshToken) {
    await fetch(apiUrl('/api/auth/logout'), {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken: session.refreshToken }),
    }).catch(() => undefined)
  }
  clearAuthSession()
}

export async function getMe(): Promise<MeResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/me')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as MeResponse
}

export async function getNavigation(): Promise<NavigationResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/me/navigation')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as NavigationResponse
}

export async function getMyEntitlements(): Promise<EntitlementSummary[]> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/me/entitlements')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as EntitlementSummary[]
}

export async function getMyTenants(): Promise<TenantSummary[]> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/me/tenants')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as TenantSummary[]
}

export async function getLaunchContext(productKey: string): Promise<LaunchContextResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/launch/context?productKey=${encodeURIComponent(productKey)}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as LaunchContextResponse
}

export async function createHandoff(
  productKey: string,
  callbackUrl: string,
): Promise<HandoffCreatedResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/launch/handoff', {
    method: 'POST',
    body: JSON.stringify({ productKey, callbackUrl }),
  })
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as HandoffCreatedResponse
}

export async function getPlatformAdminDashboard(): Promise<PlatformAdminDashboardResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/dashboard')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformAdminDashboardResponse
}

export async function getPlatformLifecycleOverview(): Promise<PlatformLifecycleOverviewResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/platform-lifecycle/overview')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformLifecycleOverviewResponse
}

export async function getPlatformWorkerHealthOrchestration(): Promise<PlatformWorkerHealthOrchestrationStatusResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/worker-health-orchestration')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformWorkerHealthOrchestrationStatusResponse
}

export async function triggerPlatformServiceTokenCleanup(): Promise<TriggerServiceTokenCleanupOrchestrationResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    '/api/platform-admin/worker-health-orchestration/trigger-service-token-cleanup',
    { method: 'POST' },
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as TriggerServiceTokenCleanupOrchestrationResponse
}

export async function triggerPlatformEntitlementReconciliation(): Promise<TriggerEntitlementReconciliationOrchestrationResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    '/api/platform-admin/worker-health-orchestration/trigger-entitlement-reconciliation',
    { method: 'POST' },
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as TriggerEntitlementReconciliationOrchestrationResponse
}

export async function triggerPlatformTenantLifecycle(): Promise<TriggerTenantLifecycleOrchestrationResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    '/api/platform-admin/worker-health-orchestration/trigger-tenant-lifecycle',
    { method: 'POST' },
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as TriggerTenantLifecycleOrchestrationResponse
}

export async function getPlatformAdminLaunchDiagnostics(
  params: { tenantId?: string; productKey?: string; page?: number; pageSize?: number } = {},
): Promise<LaunchDiagnosticsResponse> {
  await ensureValidAccessToken()
  const search = new URLSearchParams()
  if (params.tenantId) {
    search.set('tenantId', params.tenantId)
  }
  if (params.productKey) {
    search.set('productKey', params.productKey)
  }
  if (params.page) {
    search.set('page', String(params.page))
  }
  if (params.pageSize) {
    search.set('pageSize', String(params.pageSize))
  }
  const qs = search.toString()
  const response = await fetchWithAuth(
    `/api/platform-admin/launch-diagnostics${qs ? `?${qs}` : ''}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as LaunchDiagnosticsResponse
}

export async function getPlatformAdminTenantOverview(
  page = 1,
  pageSize = 50,
): Promise<PagedResult<TenantOverviewRow>> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/overview/tenants?page=${page}&pageSize=${pageSize}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PagedResult<TenantOverviewRow>
}

export async function getPlatformAdminProductOverview(): Promise<ProductOverviewRow[]> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/overview/products')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as ProductOverviewRow[]
}

function buildPlatformAuditPackageQuery(
  options?: PlatformAuditPackageScope & {
    format?: string
    page?: number
    pageSize?: number
  },
): string {
  const search = new URLSearchParams()
  if (options?.format) {
    search.set('format', options.format)
  }
  if (options?.from) {
    search.set('from', options.from)
  }
  if (options?.to) {
    search.set('to', options.to)
  }
  if (options?.tenantId) {
    search.set('tenantId', options.tenantId)
  }
  if (options?.action) {
    search.set('action', options.action)
  }
  if (options?.result) {
    search.set('result', options.result)
  }
  if (options?.targetType) {
    search.set('targetType', options.targetType)
  }
  if (options?.actorUserId) {
    search.set('actorUserId', options.actorUserId)
  }
  if (options?.productKey) {
    search.set('productKey', options.productKey)
  }
  if (options?.page) {
    search.set('page', String(options.page))
  }
  if (options?.pageSize) {
    search.set('pageSize', String(options.pageSize))
  }
  const qs = search.toString()
  return qs ? `?${qs}` : ''
}

export async function getPlatformAuditPackageManifest(): Promise<PlatformAuditPackageManifest> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/audit-packages/manifest')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformAuditPackageManifest
}

export async function getPlatformAuditPackageFilterOptions(
  options?: Pick<PlatformAuditPackageScope, 'tenantId'>,
): Promise<PlatformAuditPackageFilterOptions> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/audit-packages/filter-options${buildPlatformAuditPackageQuery(options)}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformAuditPackageFilterOptions
}

export async function getPlatformAuditPackageExportSummary(
  options?: PlatformAuditPackageScope,
): Promise<PlatformAuditPackageExportSummary> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/audit-packages/summary${buildPlatformAuditPackageQuery(options)}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformAuditPackageExportSummary
}

export async function getPlatformAuditPackageTimeline(
  options?: PlatformAuditPackageScope & { page?: number; pageSize?: number },
): Promise<PagedResult<PlatformAuditEventTimelineItem>> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/audit-packages/timeline${buildPlatformAuditPackageQuery(options)}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PagedResult<PlatformAuditEventTimelineItem>
}

export async function exportPlatformAuditPackageZip(
  options?: PlatformAuditPackageScope,
): Promise<Blob> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/audit-packages/export${buildPlatformAuditPackageQuery(options)}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return response.blob()
}

export async function exportPlatformAuditPackageCsv(
  options?: PlatformAuditPackageScope,
): Promise<Blob> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/audit-packages/export${buildPlatformAuditPackageQuery({ ...options, format: 'csv' })}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return response.blob()
}

export async function exportPlatformAuditPackageJson(
  options?: PlatformAuditPackageScope,
): Promise<PlatformAuditPackageExportPreview> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/audit-packages/export${buildPlatformAuditPackageQuery({ ...options, format: 'json' })}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformAuditPackageExportPreview
}

export async function createPlatformAuditPackageGenerationJob(
  body: PlatformAuditPackageScope & { format: string },
): Promise<PlatformAuditPackageGenerationJob> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/audit-packages/jobs', {
    method: 'POST',
    body: JSON.stringify({
      format: body.format,
      from: body.from,
      to: body.to,
      tenantId: body.tenantId,
      action: body.action,
      result: body.result,
      targetType: body.targetType,
      actorUserId: body.actorUserId,
      productKey: body.productKey,
    }),
  })
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformAuditPackageGenerationJob
}

export async function getPlatformAuditPackageGenerationJob(
  jobId: string,
): Promise<PlatformAuditPackageGenerationJob> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(`/api/platform-admin/audit-packages/jobs/${jobId}`)
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PlatformAuditPackageGenerationJob
}

export async function downloadPlatformAuditPackageGenerationJob(jobId: string): Promise<Blob> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/audit-packages/jobs/${jobId}/download`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return response.blob()
}

export async function getServiceTokenCleanupSettings(): Promise<ServiceTokenCleanupSettings> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/service-token-cleanup/settings')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as ServiceTokenCleanupSettings
}

export async function upsertServiceTokenCleanupSettings(
  payload: Pick<ServiceTokenCleanupSettings, 'isEnabled' | 'retentionDaysAfterExpiry' | 'retentionDaysAfterRevoke'>,
): Promise<ServiceTokenCleanupSettings> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/service-token-cleanup/settings', {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as ServiceTokenCleanupSettings
}

export async function getServiceTokenCleanupRuns(limit = 8): Promise<ServiceTokenCleanupRunsResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/service-token-cleanup/runs?limit=${limit}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as ServiceTokenCleanupRunsResponse
}

export async function getEntitlementReconciliationSettings(): Promise<EntitlementReconciliationSettings> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/entitlement-reconciliation/settings')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as EntitlementReconciliationSettings
}

export async function upsertEntitlementReconciliationSettings(
  payload: Pick<
    EntitlementReconciliationSettings,
    'isEnabled' | 'autoGrantFromLicense' | 'autoRevokeStaleEntitlements'
  >,
): Promise<EntitlementReconciliationSettings> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/entitlement-reconciliation/settings', {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as EntitlementReconciliationSettings
}

export async function getEntitlementReconciliationRuns(
  limit = 8,
): Promise<EntitlementReconciliationRunsResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/entitlement-reconciliation/runs?limit=${limit}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as EntitlementReconciliationRunsResponse
}

export async function getEntitlementReconciliationPending(
  batchSize = 20,
): Promise<PendingEntitlementReconciliationResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/entitlement-reconciliation/pending?batchSize=${batchSize}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PendingEntitlementReconciliationResponse
}

export async function getTenantLifecycleSettings(): Promise<TenantLifecycleSettings> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/tenant-lifecycle/settings')
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as TenantLifecycleSettings
}

export async function upsertTenantLifecycleSettings(
  payload: Pick<
    TenantLifecycleSettings,
    | 'isEnabled'
    | 'autoSuspendWhenNoValidLicense'
    | 'suspendGraceDaysAfterLastLicenseExpiry'
    | 'autoReactivateWhenValidLicense'
    | 'revokeSessionsOnSuspend'
  >,
): Promise<TenantLifecycleSettings> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth('/api/platform-admin/tenant-lifecycle/settings', {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as TenantLifecycleSettings
}

export async function getTenantLifecycleRuns(limit = 8): Promise<TenantLifecycleRunsResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(`/api/platform-admin/tenant-lifecycle/runs?limit=${limit}`)
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as TenantLifecycleRunsResponse
}

export async function getTenantLifecyclePending(
  batchSize = 20,
): Promise<PendingTenantLifecycleResponse> {
  await ensureValidAccessToken()
  const response = await fetchWithAuth(
    `/api/platform-admin/tenant-lifecycle/pending?batchSize=${batchSize}`,
  )
  if (!response.ok) {
    throw await parseError(response)
  }
  return (await response.json()) as PendingTenantLifecycleResponse
}
