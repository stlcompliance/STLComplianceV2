import type {
  QuickCreateRequest,
  QuickCreateResponse,
  QuickCreateSchemaResponse,
  ReferenceSearchRequest,
  ReferenceSearchResponse,
  ReferenceSummaryResponse,
  ReferenceTypeDescriptor,
} from './referenceTypes'

export type ReferenceProviderClientOptions = {
  baseUrl: string
  getHeaders?: () => HeadersInit | Promise<HeadersInit>
  fetcher?: typeof fetch
}

export class ReferenceProviderClient {
  private readonly baseUrl: string
  private readonly getHeaders?: () => HeadersInit | Promise<HeadersInit>
  private readonly fetcher: typeof fetch

  constructor({ baseUrl, getHeaders, fetcher = fetch }: ReferenceProviderClientOptions) {
    this.baseUrl = baseUrl.replace(/\/$/, '')
    this.getHeaders = getHeaders
    this.fetcher = fetcher
  }

  async listReferenceTypes(): Promise<ReferenceTypeDescriptor[]> {
    return this.request<ReferenceTypeDescriptor[]>('/api/v1/integrations/reference-types')
  }

  async searchReferences(request: ReferenceSearchRequest): Promise<ReferenceSearchResponse> {
    return this.request<ReferenceSearchResponse>('/api/v1/integrations/references/search', {
      method: 'POST',
      body: JSON.stringify(request),
    })
  }

  async getSummary(referenceType: string, id: string): Promise<ReferenceSummaryResponse> {
    return this.request<ReferenceSummaryResponse>(
      `/api/v1/integrations/references/${encodeURIComponent(referenceType)}/${encodeURIComponent(id)}/summary`,
    )
  }

  async getQuickCreateSchema(referenceType: string): Promise<QuickCreateSchemaResponse> {
    return this.request<QuickCreateSchemaResponse>(
      `/api/v1/integrations/references/${encodeURIComponent(referenceType)}/quick-create-schema`,
    )
  }

  async quickCreate(
    referenceType: string,
    request: QuickCreateRequest,
  ): Promise<QuickCreateResponse> {
    return this.request<QuickCreateResponse>(
      `/api/v1/integrations/references/${encodeURIComponent(referenceType)}/quick-create`,
      {
        method: 'POST',
        headers: {
          'Idempotency-Key': createIdempotencyKey(referenceType),
        },
        body: JSON.stringify(request),
      },
    )
  }

  private async request<T>(path: string, init: RequestInit = {}): Promise<T> {
    const headers = new Headers(await this.getHeaders?.())
    new Headers(init.headers).forEach((value, key) => headers.set(key, value))
    if (init.body && !headers.has('Content-Type')) {
      headers.set('Content-Type', 'application/json')
    }

    const response = await this.fetcher(`${this.baseUrl}${path}`, {
      ...init,
      headers,
    })

    if (!response.ok) {
      let message = response.statusText
      try {
        const body = (await response.json()) as { message?: string; title?: string }
        message = body.message ?? body.title ?? message
      } catch {
        // Keep the HTTP status text when the owner returns no JSON error body.
      }
      throw new Error(message || `Reference provider request failed (${response.status})`)
    }

    return (await response.json()) as T
  }
}

function createIdempotencyKey(referenceType: string) {
  const suffix =
    typeof crypto !== 'undefined' && 'randomUUID' in crypto
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(16).slice(2)}`
  return `reference-${referenceType}-${suffix}`
}
