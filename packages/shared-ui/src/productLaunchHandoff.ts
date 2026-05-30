import { normalizeProductKey } from './productCatalog'
import { resolveProductLaunchUrl } from './productLaunchUrls'

export interface LaunchContextResponse {
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  userId: string
  userEmail: string
  productKey: string
  productDisplayName: string
  baseLaunchUrl: string
  launchUrl: string
  canLaunch: boolean
  denialReasonCode: string | null
}

export interface HandoffCreatedResponse {
  handoffCode: string
  handoffId: string
  expiresAt: string
  launchUrl: string
}

export interface LaunchCatalogItemResponse {
  productKey: string
  displayName: string
  productStatus: string
  launchUrl: string
  isCurrentProduct: boolean
}

export interface LaunchCatalogResponse {
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  currentProductKey: string | null
  products: LaunchCatalogItemResponse[]
  generatedAt: string
}

export class ProductLaunchError extends Error {
  readonly status: number

  constructor(message: string, status: number) {
    super(message)
    this.status = status
    this.name = 'ProductLaunchError'
  }
}

async function parseLaunchResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new ProductLaunchError(body || `${fallbackMessage} (${response.status})`, response.status)
  }

  return (await response.json()) as T
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

export async function getLaunchContext(
  apiBase: string,
  accessToken: string,
  productKey: string,
): Promise<LaunchContextResponse> {
  const search = new URLSearchParams({ productKey })
  const response = await fetch(`${apiBase}/api/v1/launch/context?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseLaunchResponse<LaunchContextResponse>(response, 'Failed to load launch context')
}

export async function createProductHandoff(
  apiBase: string,
  accessToken: string,
  productKey: string,
  callbackUrl: string,
): Promise<HandoffCreatedResponse> {
  const response = await fetch(`${apiBase}/api/v1/launch/handoff`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ productKey, callbackUrl }),
  })
  return parseLaunchResponse<HandoffCreatedResponse>(response, 'Failed to create product handoff')
}

export async function getLaunchCatalog(
  apiBase: string,
  accessToken: string,
  currentProductKey: string,
): Promise<LaunchCatalogResponse> {
  const search = new URLSearchParams({ currentProductKey })
  const response = await fetch(`${apiBase}/api/v1/launch/catalog?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseLaunchResponse<LaunchCatalogResponse>(response, 'Failed to load launch catalog')
}

export function buildProductWorkspaceCallbackUrl(
  productKey: string,
  suiteHomeUrl: string,
  productLaunchUrls: Record<string, string>,
): string {
  return resolveProductLaunchUrl(productKey, suiteHomeUrl, productLaunchUrls)
}

export function formatProductLaunchError(error: unknown): string {
  if (error instanceof ProductLaunchError) {
    return error.message
  }
  if (error instanceof Error) {
    return error.message
  }
  return 'Product launch failed.'
}

export function isSameProductKey(left: string, right: string): boolean {
  return normalizeProductKey(left) === normalizeProductKey(right)
}
