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
    return typeof parsed.message === 'string' ? parsed : null
  } catch {
    return null
  }
}

export function companionPlainReason(error: unknown, fallback: string): string {
  if (error && typeof error === 'object' && 'body' in error) {
    const body = (error as { body?: string }).body
    if (typeof body === 'string') {
      const parsed = parseApiErrorBody(body)
      if (parsed?.message) {
        return parsed.message
      }
    }
  }

  if (error instanceof Error && error.message.trim()) {
    const parsed = parseApiErrorBody(error.message)
    if (parsed?.message) {
      return parsed.message
    }

    return error.message
  }

  return fallback
}
