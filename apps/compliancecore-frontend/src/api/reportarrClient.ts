const apiBase = import.meta.env.VITE_REPORTARR_API_BASE ?? ''

export type ReportArrReportDefinitionResponse = {
  reportDefinitionId: string
  reportNumber: string
  reportKey: string
  title: string
  description: string
  reportType: string
  status: string
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `${fallbackMessage} (${response.status})`)
  }

  return (await response.json()) as T
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

export async function listReportDefinitions(accessToken: string): Promise<ReportArrReportDefinitionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/integrations/report-definitions`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReportArrReportDefinitionResponse[]>(response, 'Failed to load report definitions')
}
