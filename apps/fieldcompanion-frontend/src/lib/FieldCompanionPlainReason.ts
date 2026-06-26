import { reasonCodeToPlainMessage } from './FieldCompanionDeniedReasonCatalog'

export interface ApiErrorBody {
  code?: string
  message?: string
}

export function parseApiErrorBody(body: string): ApiErrorBody | null {
  if (!body.trim().startsWith('{')) {
    return null
  }

  try {
    const parsed = JSON.parse(body) as ApiErrorBody
    return typeof parsed.message === 'string' || typeof parsed.code === 'string' ? parsed : null
  } catch {
    return null
  }
}

function resolveParsedApiError(parsed: ApiErrorBody, fallback: string): string {
  if (parsed.message?.trim()) {
    return parsed.message.trim()
  }

  if (parsed.code?.trim()) {
    return reasonCodeToPlainMessage(parsed.code, fallback)
  }

  return fallback
}

export function FieldCompanionPlainReason(error: unknown, fallback: string): string {
  if (error && typeof error === 'object' && 'body' in error) {
    const body = (error as { body?: string }).body
    if (typeof body === 'string') {
      const parsed = parseApiErrorBody(body)
      if (parsed) {
        return resolveParsedApiError(parsed, fallback)
      }
    }
  }

  if (error instanceof Error && error.message.trim()) {
    const parsed = parseApiErrorBody(error.message)
    if (parsed) {
      return resolveParsedApiError(parsed, fallback)
    }

    if (PLAIN_REASON_CODE_PATTERN.test(error.message.trim())) {
      return reasonCodeToPlainMessage(error.message.trim(), fallback)
    }

    return error.message
  }

  return fallback
}

const PLAIN_REASON_CODE_PATTERN =
  /^(fieldcompanion\.|scan\.|launch\.|auth\.|tenant_|product_unavailable|not_available|availability_(inactive|revoked)|profile_|upstream_)/
