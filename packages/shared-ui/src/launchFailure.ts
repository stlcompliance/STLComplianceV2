type ApiLikeError = {
  status?: number
  body?: string
  message?: string
}

type ParsedApiError = {
  code?: string
  message?: string
}

const ACCESS_UNAVAILABLE_CODES = new Set([
  'handoff.not_entitled',
  'handoff.not_available',
  'launch.availability_revoked',
  'launch.entitlement_revoked',
])

function parseApiError(body?: string): ParsedApiError {
  if (!body) {
    return {}
  }

  try {
    const parsed = JSON.parse(body) as ParsedApiError
    return parsed
  } catch {
    return {}
  }
}

export function resolveNexArrLaunchFailureMessage(productName: string, error: unknown): string {
  const candidate = error as ApiLikeError
  const status = candidate?.status
  const apiError = parseApiError(candidate?.body)
  const code = apiError.code ?? ''
  const message = apiError.message ?? candidate?.message ?? 'Handoff failed'

  if (code === 'handoff.code_missing' || code === 'launch.handoff_missing') {
    return `Missing handoff code. Launch ${productName} from the suite.`
  }

  if (
    code === 'launch.handoff_invalid'
    || code === 'launch.handoff_expired'
    || code === 'launch.handoff_already_redeemed'
  ) {
    return 'The handoff code is invalid, expired, or already used. Relaunch from the suite.'
  }

  if (code === 'availability_inactive' || ACCESS_UNAVAILABLE_CODES.has(code)) {
    return `${productName} is unavailable for your current tenant context.`
  }

  if (code === 'auth.platform_admin_required') {
    return `${productName} requires platform administrator access in NexArr.`
  }

  if (
    code === 'auth.session_revoked'
    || code === 'auth.session_expired'
    || code === 'launch.session_revoked'
    || code === 'launch.session_expired'
  ) {
    return 'Your NexArr session has ended. Sign in again and relaunch from the suite.'
  }

  if (code === 'handoff.product_mismatch' || code === 'launch.callback_not_allowed') {
    return `Invalid callback for ${productName}. Relaunch from NexArr.`
  }

  if (code === 'launch.profile_missing' || code === 'product.not_found') {
    return `${productName} is unavailable right now. Contact a platform administrator.`
  }

  if (code === 'auth.tenant_forbidden') {
    return 'This launch does not match your current tenant context. Select the correct tenant in NexArr and relaunch.'
  }

  if (status === 403) {
    return `${productName} is unavailable for your current tenant context.`
  }

  if (status === 401) {
    return 'The handoff code is invalid or expired. Relaunch from the suite.'
  }

  return message
}
