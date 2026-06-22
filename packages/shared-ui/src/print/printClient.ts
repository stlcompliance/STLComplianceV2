import type {
  ArchiveOfficialResponse,
  BrowserPrintLogRequest,
  BrowserPrintLogResponse,
  PrintDocumentRequest,
  PrintHistoryResponse,
  PrintPreviewResponse,
  PrintTemplateCatalogResponse,
  ReprintRequest,
} from './types'

export class PrintClientError extends Error {
  readonly status: number
  readonly body: string

  constructor(
    message: string,
    status: number,
    body: string,
  ) {
    super(message)
    this.name = 'PrintClientError'
    this.status = status
    this.body = body
  }
}

function trimBase(apiBase: string): string {
  return apiBase.trim().replace(/\/$/, '')
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new PrintClientError(body || fallbackMessage, response.status, body)
  }

  return (await response.json()) as T
}

function parseFileNameFromDisposition(value: string | null): string | null {
  if (!value) {
    return null
  }

  const utf8Match = value.match(/filename\*=UTF-8''([^;]+)/i)
  if (utf8Match?.[1]) {
    return decodeURIComponent(utf8Match[1])
  }

  const quotedMatch = value.match(/filename="([^"]+)"/i)
  if (quotedMatch?.[1]) {
    return quotedMatch[1]
  }

  const plainMatch = value.match(/filename=([^;]+)/i)
  return plainMatch?.[1]?.trim() ?? null
}

async function parseFileResponse(
  response: Response,
  fallbackMessage: string,
): Promise<{ blob: Blob; fileName: string | null; contentType: string | null }> {
  if (!response.ok) {
    const body = await response.text()
    throw new PrintClientError(body || fallbackMessage, response.status, body)
  }

  return {
    blob: await response.blob(),
    fileName: parseFileNameFromDisposition(response.headers.get('Content-Disposition')),
    contentType: response.headers.get('Content-Type'),
  }
}

export async function getPrintTemplates(
  apiBase: string,
  accessToken: string,
): Promise<PrintTemplateCatalogResponse> {
  const response = await fetch(`${trimBase(apiBase)}/api/v1/print/templates`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PrintTemplateCatalogResponse>(response, 'Failed to load print templates')
}

export async function logBrowserPrint(
  apiBase: string,
  accessToken: string,
  request: BrowserPrintLogRequest,
): Promise<BrowserPrintLogResponse> {
  const response = await fetch(`${trimBase(apiBase)}/api/v1/print/browser-print-log`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<BrowserPrintLogResponse>(
    response,
    'Failed to log browser print request',
  )
}

export async function getPrintHistory(
  apiBase: string,
  accessToken: string,
  options: { sourceEntityType: string; sourceEntityId: string; limit?: number },
): Promise<PrintHistoryResponse> {
  const search = new URLSearchParams({
    sourceEntityType: options.sourceEntityType,
    sourceEntityId: options.sourceEntityId,
  })
  if (options.limit) {
    search.set('limit', String(options.limit))
  }

  const response = await fetch(`${trimBase(apiBase)}/api/v1/print/history?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PrintHistoryResponse>(response, 'Failed to load print history')
}

export async function previewPrintDocument(
  apiBase: string,
  accessToken: string,
  request: PrintDocumentRequest,
): Promise<PrintPreviewResponse> {
  const response = await fetch(`${trimBase(apiBase)}/api/v1/print/preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PrintPreviewResponse>(response, 'Failed to generate print preview')
}

export async function downloadPrintPdf(
  apiBase: string,
  accessToken: string,
  request: PrintDocumentRequest,
): Promise<{ blob: Blob; fileName: string | null; contentType: string | null }> {
  const response = await fetch(`${trimBase(apiBase)}/api/v1/print/pdf`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseFileResponse(response, 'Failed to generate PDF')
}

export async function archiveOfficialCopy(
  apiBase: string,
  accessToken: string,
  request: PrintDocumentRequest,
): Promise<ArchiveOfficialResponse> {
  const response = await fetch(`${trimBase(apiBase)}/api/v1/print/archive`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<ArchiveOfficialResponse>(response, 'Failed to archive official copy')
}

export async function logReprint(
  apiBase: string,
  accessToken: string,
  request: ReprintRequest,
): Promise<BrowserPrintLogResponse> {
  const response = await fetch(`${trimBase(apiBase)}/api/v1/print/reprint`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<BrowserPrintLogResponse>(response, 'Failed to record reprint request')
}
