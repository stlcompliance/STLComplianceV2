import type {
  EntitlementSummary,
  MeResponse,
  NavigationItem,
  TenantSummary,
} from '../api/types'
import type { StoredAuthSession } from '../auth/authStorage'
import { getSuiteProductCatalogEntry, normalizeProductKey } from '@stl/shared-ui'
import { hasProductEntitlement, isInSuiteProduct, isPlatformAdmin } from './permissions'

export type DashboardActionKind = 'warning' | 'action' | 'info'

export interface DashboardAction {
  id: string
  kind: DashboardActionKind
  title: string
  description?: string
  href?: string
  productKey?: string
}

export interface QuickLaunchProduct {
  productKey: string
  displayName: string
  routePath: string
  sortOrder: number
  inSuite: boolean
  entitled: boolean
  launchable: boolean
}

export interface SessionSummary {
  sessionId: string
  userId: string
  tenantId: string
  accessExpiresAt: string
  refreshExpiresAt: string
  accessExpiresInMinutes: number | null
  isAccessExpiringSoon: boolean
}

const ACTIVE_TENANT_STATUS = 'active'
const ACCESS_EXPIRY_WARN_MINUTES = 15

function hasEnabledLaunchSurface(product: NavigationItem): boolean {
  return product.surfaces.some(
    (surface) =>
      surface.isEnabled &&
      (surface.surfaceKey.trim().toLowerCase() === 'launch' ||
        surface.relativePath.trim().toLowerCase() === 'launch'),
  )
}

export function findCurrentTenant(
  tenants: readonly TenantSummary[],
  tenantId: string,
): TenantSummary | undefined {
  return tenants.find((t) => t.tenantId === tenantId)
}

export function isTenantActive(tenant: TenantSummary | undefined): boolean {
  return tenant?.status.trim().toLowerCase() === ACTIVE_TENANT_STATUS
}

export function buildQuickLaunchProducts(
  navigationProducts: readonly NavigationItem[],
  entitlements: readonly string[],
): QuickLaunchProduct[] {
  return [...navigationProducts]
    .filter((product) => hasProductEntitlement(entitlements, product.productKey))
    .map((product) => {
      const normalized = normalizeProductKey(product.productKey)
      const catalogEntry = getSuiteProductCatalogEntry(normalized)
      return {
        productKey: normalized,
        displayName: catalogEntry?.displayName ?? product.displayName,
        routePath: product.routePath,
        sortOrder: catalogEntry?.sortOrder ?? product.sortOrder,
        inSuite: isInSuiteProduct(normalized),
        entitled: true,
        launchable: hasEnabledLaunchSurface(product),
      }
    })
    .filter((product) => Boolean(getSuiteProductCatalogEntry(product.productKey)))
    .sort((a, b) => a.sortOrder - b.sortOrder)
}

export function summarizeSession(session: StoredAuthSession): SessionSummary {
  const accessExpiresAt = session.accessTokenExpiresAt
  const accessMs = Date.parse(accessExpiresAt)
  const accessExpiresInMinutes = Number.isNaN(accessMs)
    ? null
    : Math.max(0, Math.round((accessMs - Date.now()) / 60_000))

  return {
    sessionId: session.sessionId,
    userId: session.userId,
    tenantId: session.tenantId,
    accessExpiresAt,
    refreshExpiresAt: session.refreshTokenExpiresAt,
    accessExpiresInMinutes,
    isAccessExpiringSoon:
      accessExpiresInMinutes !== null &&
      accessExpiresInMinutes <= ACCESS_EXPIRY_WARN_MINUTES,
  }
}

export function buildWhatINeedActions(input: {
  me: MeResponse
  tenants: readonly TenantSummary[]
  entitlements: readonly EntitlementSummary[]
  navigationProducts: readonly NavigationItem[]
}): DashboardAction[] {
  const actions: DashboardAction[] = []
  const currentTenant = findCurrentTenant(input.tenants, input.me.tenantId)

  if (currentTenant && !isTenantActive(currentTenant)) {
    actions.push({
      id: 'tenant-not-active',
      kind: 'warning',
      title: `Tenant "${currentTenant.displayName}" is ${currentTenant.status}`,
      description:
        'Product access may be limited until your organization is active again. Contact your administrator.',
    })
  }

  if (input.entitlements.length === 0 && !isPlatformAdmin(input.me)) {
    actions.push({
      id: 'no-entitlements',
      kind: 'warning',
      title: 'No active product entitlements',
      description: 'Ask a tenant administrator to grant access to the products you need.',
    })
  }

  if (input.tenants.length > 1) {
    actions.push({
      id: 'multi-tenant',
      kind: 'info',
      title: `You belong to ${input.tenants.length} tenants`,
      description: `Current workspace: ${input.me.tenantDisplayName} (${input.me.tenantSlug}). Sign out and sign in with another tenant to switch.`,
    })
  }

  for (const product of buildQuickLaunchProducts(input.navigationProducts, input.me.entitlements)) {
    if (product.inSuite) {
      actions.push({
        id: `hub-${product.productKey}`,
        kind: 'action',
        title: `Open ${product.displayName}`,
        description: 'Manage identity and platform settings in-suite.',
        href: product.routePath,
        productKey: product.productKey,
      })
      continue
    }

    actions.push({
      id: `${product.launchable ? 'launch' : 'open'}-${product.productKey}`,
      kind: 'action',
      title: `${product.launchable ? 'Launch' : 'Open'} ${product.displayName}`,
      description: product.launchable
        ? 'Opens the product app via NexArr handoff when launch is permitted.'
        : 'Opens the suite overview for this product.',
      href: product.routePath,
      productKey: product.launchable ? product.productKey : undefined,
    })
  }

  if (isPlatformAdmin(input.me)) {
    actions.push({
      id: 'platform-admin',
      kind: 'action',
      title: 'Review platform administration',
      description: 'Tenants, launch diagnostics, and suite-wide health from the control plane.',
      href: '/app/platform-admin',
    })
  }

  return actions
}
