export type ProductAiAssistantMessageRequest = {
  sessionId?: string | null
  productKey: string
  surface: string
  route: string
  category?: string
  message: string
  pageContext?: Record<string, unknown>
  allowedBehaviors?: string[]
}

export type ProductAiAssistantMessageResponse = {
  sessionId: string
  messageId: string
  outcome: string
  answer: string
  errorCode?: string | null
  safeMessage?: string | null
  requiredReviewReasons?: string[]
}

export class ProductAiAssistanceError extends Error {
  readonly status: number
  readonly body: string

  constructor(message: string, status: number, body: string) {
    super(message)
    this.name = 'ProductAiAssistanceError'
    this.status = status
    this.body = body
  }
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

function extractErrorMessage(body: string): string | null {
  if (!body.trim()) return null

  try {
    const parsed = JSON.parse(body) as { message?: string; detail?: string; title?: string }
    return parsed.message ?? parsed.detail ?? parsed.title ?? null
  } catch {
    return null
  }
}

export async function sendProductAiAssistantMessage(
  apiBase: string,
  accessToken: string,
  payload: ProductAiAssistantMessageRequest,
): Promise<ProductAiAssistantMessageResponse> {
  const response = await fetch(`${apiBase}/api/v1/ai/assistant/messages`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })

  if (!response.ok) {
    const body = await response.text()
    throw new ProductAiAssistanceError(
      extractErrorMessage(body) ?? body ?? `AI assistance failed (${response.status})`,
      response.status,
      body,
    )
  }

  return (await response.json()) as ProductAiAssistantMessageResponse
}
