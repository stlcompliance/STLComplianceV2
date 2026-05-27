export interface AuthTokenResponse {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
  sessionId: string
  userId: string
  tenantId: string
}

export interface LoginRequest {
  email: string
  password: string
  tenantId: string | null
}

export interface MeResponse {
  userId: string
  email: string
  displayName: string
  isPlatformAdmin: boolean
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  entitlements: string[]
}

export interface NavigationItem {
  productKey: string
  displayName: string
  routePath: string
  sortOrder: number
}

export interface NavigationResponse {
  tenantId: string
  products: NavigationItem[]
}

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

export interface ApiErrorBody {
  code?: string
  message?: string
}

export class NexarrApiError extends Error {
  readonly status: number
  readonly code?: string

  constructor(status: number, message: string, code?: string) {
    super(message)
    this.name = 'NexarrApiError'
    this.status = status
    this.code = code
  }
}
