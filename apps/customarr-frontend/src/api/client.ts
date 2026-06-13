import type {
  CustomArrCreateCustomerRequest,
  CustomArrCustomerDetail,
  CustomArrCustomerSummary,
  CustomArrRequirementCatalogItem,
  CustomArrWorkspaceSession,
} from '../demoData'

const apiBase = import.meta.env.VITE_CUSTOMARR_API_BASE ?? ''

export interface CustomArrSessionBootstrapResponse {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasCustomArrEntitlement: boolean
  entitlements: string[]
}

export interface CustomArrHandoffSessionResponse {
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

export interface CustomArrDashboardResponse {
  generatedAt: string
  customerCount: number
  activeCustomerCount: number
  onboardingCustomerCount: number
  watchListCustomerCount: number
  contactCount: number
  siteCount: number
  requirementCount: number
  featuredCustomers: CustomArrCustomerSummary[]
  recentActivity: Array<{
    activityId: string
    customerId: string
    customerNumber: string
    message: string
    occurredAt: string
    kind: string
  }>
}

class CustomArrApiError extends Error {
  constructor(message: string, readonly status: number) {
    super(message)
    this.name = 'CustomArrApiError'
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
    throw new CustomArrApiError(body || `${fallbackMessage} (${response.status})`, response.status)
  }

  return (await response.json()) as T
}

export async function getSessionBootstrap(accessToken: string): Promise<CustomArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/v1/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrSessionBootstrapResponse>(response, 'Failed to load session bootstrap')
}

export async function redeemHandoff(handoffCode: string): Promise<CustomArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/v1/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<CustomArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getDashboard(accessToken: string): Promise<CustomArrDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/workspace/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrDashboardResponse>(response, 'Failed to load dashboard')
}

export async function listCustomers(accessToken: string, search?: string): Promise<CustomArrCustomerDetail[]> {
  const searchParams = new URLSearchParams()
  if (search?.trim()) {
    searchParams.set('search', search.trim())
  }
  const response = await fetch(`${apiBase}/api/v1/workspace/customers${searchParams.toString() ? `?${searchParams}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrCustomerDetail[]>(response, 'Failed to load customers')
}

export async function getCustomer(accessToken: string, customerId: string): Promise<CustomArrCustomerDetail> {
  const response = await fetch(`${apiBase}/api/v1/workspace/customers/${customerId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrCustomerDetail>(response, 'Failed to load customer')
}

export async function createCustomer(
  accessToken: string,
  body: CustomArrCreateCustomerRequest,
): Promise<CustomArrCustomerDetail> {
  const response = await fetch(`${apiBase}/api/v1/workspace/customers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(body),
  })
  return parseJsonResponse<CustomArrCustomerDetail>(response, 'Failed to create customer')
}

export async function listRequirements(accessToken: string): Promise<CustomArrRequirementCatalogItem[]> {
  const response = await fetch(`${apiBase}/api/v1/workspace/requirements`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CustomArrRequirementCatalogItem[]>(response, 'Failed to load requirement catalog')
}

export function resolveDemoWorkspaceSession(): CustomArrWorkspaceSession {
  return {
    userDisplayName: 'Demo Admin',
    tenantDisplayName: 'CustomArr Demo Tenant',
    tenantSlug: 'demo-tenant',
  }
}
