import type { LaunchContextResponse } from '../api/types'

export interface LaunchFailureCopy {
  title: string
  message: string
  guidance: string
  severity: 'warning' | 'error'
}

const DENIAL_COPY: Record<string, LaunchFailureCopy> = {
  tenant_suspended: {
    title: 'Tenant is not active',
    message: 'Your organization workspace is suspended or inactive.',
    guidance: 'Contact your tenant administrator or STL Compliance support to restore access.',
    severity: 'error',
  },
  not_entitled: {
    title: 'Product not entitled',
    message: 'Your account does not include access to this product for the current tenant.',
    guidance: 'Ask a tenant administrator to grant the product entitlement, then try again.',
    severity: 'warning',
  },
  entitlement_inactive: {
    title: 'Entitlement inactive',
    message: 'This product entitlement exists but is not active for your tenant.',
    guidance: 'A tenant administrator can reactivate the entitlement from platform administration.',
    severity: 'warning',
  },
  profile_missing: {
    title: 'Launch profile missing',
    message: 'NexArr does not have an active launch profile for this product.',
    guidance: 'Platform administrators can configure the product launch URL and callback allowlist.',
    severity: 'error',
  },
  callback_not_allowed: {
    title: 'Callback URL not allowed',
    message: 'The suite callback URL is not on the product launch allowlist.',
    guidance: 'Platform administrators must add this suite origin to the product callback allowlist.',
    severity: 'error',
  },
  'launch.denied': {
    title: 'Launch not permitted',
    message: 'NexArr blocked this product launch for your current tenant context.',
    guidance: 'Review entitlements and launch diagnostics, or contact your administrator.',
    severity: 'warning',
  },
  'launch.profile_missing': {
    title: 'Launch profile missing',
    message: 'No launch profile is configured for this product.',
    guidance: 'Platform administrators can register the product base URL in the launch registry.',
    severity: 'error',
  },
  'launch.callback_not_allowed': {
    title: 'Callback URL not allowed',
    message: 'The handoff callback URL was rejected by NexArr.',
    guidance: 'Platform administrators must update the callback allowlist for this product.',
    severity: 'error',
  },
}

const DEFAULT_COPY: LaunchFailureCopy = {
  title: 'Launch not permitted',
  message: 'NexArr could not authorize a launch handoff for this product.',
  guidance: 'Try again later or contact your administrator if the problem continues.',
  severity: 'warning',
}

export function normalizeLaunchFailureCode(code: string | null | undefined): string {
  return code?.trim().toLowerCase() ?? ''
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

export function buildLaunchFailureFromContext(context: LaunchContextResponse): LaunchFailureCopy | null {
  if (context.canLaunch) {
    return null
  }
  return resolveLaunchFailureCopy(context.denialReasonCode)
}
