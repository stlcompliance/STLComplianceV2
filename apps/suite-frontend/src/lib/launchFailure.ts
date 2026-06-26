import type { LaunchContextResponse } from '../api/types'

export interface LaunchFailureCopy {
  title: string
  message: string
  guidance: string
  severity: 'warning' | 'error'
}

export interface LaunchFailureDisplay {
  title: string
  message: string
  guidance: string
  normalizedCode: string | null
  rawCode: string | null
}

const DENIAL_COPY: Record<string, LaunchFailureCopy> = {
  tenant_suspended: {
    title: 'Tenant is not active',
    message: 'Your organization workspace is suspended or inactive.',
    guidance: 'Contact your tenant administrator or STL Compliance support to restore access.',
    severity: 'error',
  },
  product_unavailable: {
    title: 'Product unavailable',
    message: 'This product cannot be launched from your current tenant context.',
    guidance: 'Confirm your tenant membership, product status, and permissions, then try again.',
    severity: 'warning',
  },
  launch_destination_inactive: {
    title: 'Launch destination inactive',
    message: 'This launch destination is not currently in an available operating state.',
    guidance: 'Review tenant status in NexArr, the product destination status, and destination product permissions, then try again.',
    severity: 'warning',
  },
  platform_admin_required: {
    title: 'Platform administrator required',
    message: 'This product is restricted to NexArr platform administrators.',
    guidance: 'Launch it with a platform administrator account, or ask a platform administrator to access it for you.',
    severity: 'warning',
  },
  profile_missing: {
    title: 'Launch profile missing',
    message: 'NexArr does not have active launch settings for this product.',
    guidance: 'Platform administrators can configure the product launch URL and callback allowlist.',
    severity: 'error',
  },
  callback_not_allowed: {
    title: 'Callback URL not allowed',
    message: 'The suite callback URL is not on the allowed list for this product.',
    guidance: 'Platform administrators must add this suite origin to the product callback allowlist.',
    severity: 'error',
  },
  'launch.denied': {
    title: 'Launch not permitted',
    message: 'NexArr blocked this product launch for your current tenant context.',
    guidance: 'Review launch diagnostics or contact your administrator.',
    severity: 'warning',
  },
  'launch.profile_missing': {
    title: 'Launch profile missing',
    message: 'Launch settings are not configured for this product.',
    guidance: 'Platform administrators can register the product base URL in the launch registry.',
    severity: 'error',
  },
  'launch.callback_not_allowed': {
    title: 'Callback URL not allowed',
    message: 'The product callback URL was rejected by NexArr.',
    guidance: 'Platform administrators must update the callback allowlist for this product.',
    severity: 'error',
  },
}

const DEFAULT_COPY: LaunchFailureCopy = {
  title: 'Launch not permitted',
  message: 'NexArr could not complete the launch for this product.',
  guidance: 'Try again later or contact your administrator if the problem continues.',
  severity: 'warning',
}

const PRODUCT_UNAVAILABLE_ALIASES = new Set([
  'product_not_available',
  'launch.product_unavailable',
  'availability_revoked',
  'launch.availability_revoked',
  'handoff.not_available',
  'not_available',
])

const LAUNCH_DESTINATION_INACTIVE_ALIASES = new Set([
  'availability_inactive',
  'launch.availability_inactive',
])

export function normalizeLaunchFailureCode(code: string | null | undefined): string {
  const normalized = code?.trim().toLowerCase() ?? ''
  if (LAUNCH_DESTINATION_INACTIVE_ALIASES.has(normalized)) {
    return 'launch_destination_inactive'
  }
  if (PRODUCT_UNAVAILABLE_ALIASES.has(normalized)) {
    return 'product_unavailable'
  }
  return normalized
}

export function resolveLaunchFailureCopy(code: string | null | undefined): LaunchFailureCopy {
  const normalized = normalizeLaunchFailureCode(code)
  if (!normalized) {
    return DEFAULT_COPY
  }
  return DENIAL_COPY[normalized] ?? {
    ...DEFAULT_COPY,
    message: `${DEFAULT_COPY.message} (${normalized})`,
  }
}

export function formatLaunchFailureError(code: string | null | undefined): string {
  const copy = resolveLaunchFailureCopy(code)
  return copy.message
}

export function describeLaunchFailure(code: string | null | undefined): LaunchFailureDisplay | null {
  const rawCode = code?.trim().toLowerCase() ?? ''
  const normalizedCode = normalizeLaunchFailureCode(code)
  if (!normalizedCode) {
    return null
  }

  const copy = resolveLaunchFailureCopy(normalizedCode)
  return {
    title: copy.title,
    message: copy.message,
    guidance: copy.guidance,
    normalizedCode,
    rawCode:
      rawCode
      && rawCode !== normalizedCode
      && !PRODUCT_UNAVAILABLE_ALIASES.has(rawCode)
      && !LAUNCH_DESTINATION_INACTIVE_ALIASES.has(rawCode)
        ? rawCode
        : null,
  }
}

export function normalizeLaunchRemediationHint(
  hint: string | null | undefined,
  reasonCode: string | null | undefined,
): string | null {
  const trimmed = hint?.trim()
  const rawReasonCode = reasonCode?.trim().toLowerCase() ?? ''
  const normalizedReasonCode = normalizeLaunchFailureCode(reasonCode)

  if (rawReasonCode === 'not_available' || rawReasonCode === 'handoff.not_available') {
    return 'Confirm the tenant is active, then review the destination product status and local permissions.'
  }

  if (
    normalizedReasonCode === 'launch_destination_inactive' ||
    normalizedReasonCode === 'product_unavailable'
  ) {
    return resolveLaunchFailureCopy(normalizedReasonCode).guidance
  }

  if (!trimmed) {
    return null
  }

  if (
    trimmed.toLowerCase()
    === 'activate or reactivate the tenant launch availability for the requested product.'
  ) {
    return 'Confirm the tenant is active, then review the destination product status and local permissions.'
  }

  return trimmed
}

export function buildLaunchFailureFromContext(context: LaunchContextResponse): LaunchFailureCopy | null {
  if (context.canLaunch) {
    return null
  }
  return resolveLaunchFailureCopy(context.denialReasonCode)
}
