import type {
  ReportArrAcknowledgeAlertRequest,
  ReportArrAlertResponse,
  ReportArrAuditPackageResponse,
  ReportArrAuditScopeResponse,
  ReportArrCalculateKpiRequest,
  ReportArrCancelReportRunRequest,
  ReportArrCreateAuditPackageRequest,
  ReportArrLockAuditPackageRequest,
  ReportArrCreateDashboardRequest,
  ReportArrCreateDatasetRequest,
  ReportArrCreateExportRequest,
  ReportArrCreateReportDefinitionRequest,
  ReportArrCreateReportRunRequest,
  ReportArrCreateReportScheduleRequest,
  ReportArrDashboardResponse,
  ReportArrDashboardAccessPolicyResponse,
  ReportArrDashboardFilterResponse,
  ReportArrDashboardWidgetResponse,
  ReportArrDatasetResponse,
  ReportArrDatasetFieldResponse,
  ReportArrDatasetLineageResponse,
  ReportArrExportJobResponse,
  ReportArrHandoffSessionResponse,
  ReportArrKpiDefinitionResponse,
  ReportArrKpiValueResponse,
  ReportArrIngestionCursorResponse,
  ReportArrMeResponse,
  ReportArrMetricDefinitionResponse,
  ReportArrMetricValueResponse,
  ReportArrDrilldownDefinitionResponse,
  ReportArrReadModelResponse,
  ReportArrReadModelRecordResponse,
  ReportArrRefreshDatasetRequest,
  ReportArrRefreshJobResponse,
  ReportArrReportDefinitionResponse,
  ReportArrReportAccessPolicyResponse,
  ReportArrReportParameterResponse,
  ReportArrReportSectionResponse,
  ReportArrReportRunResponse,
  ReportArrReportScheduleResponse,
  ReportArrReportRecipientResponse,
  ReportArrResolveAlertRequest,
  ReportArrWidgetVisualizationSettingsResponse,
  ReportArrSessionBootstrapResponse,
  ReportArrSourceConnectorResponse,
  ReportArrSourceEventReceiptResponse,
  ReportArrSummaryResponse,
  ReportArrUpdateDashboardRequest,
  ReportArrUpdateReportDefinitionRequest,
  ReportArrUpdateReportScheduleRequest,
  ReportArrIntegrationEventRequest,
  ReportArrAnalyticsSnapshotResponse,
  ReportArrExceptionQueryResponse,
  ReportArrExceptionResultResponse,
  ReportArrTrendAnalysisResponse,
} from './types'

const apiBase = import.meta.env.VITE_REPORTARR_API_BASE ?? ''

class ReportArrApiError extends Error {
  constructor(message: string, public readonly status: number) {
    super(message)
    this.name = 'ReportArrApiError'
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new ReportArrApiError(body || `${fallbackMessage} (${response.status})`, response.status)
  }

  return (await response.json()) as T
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function getJson<T>(path: string, accessToken: string): Promise<T> {
  return parseJsonResponse<T>(
    await fetch(`${apiBase}${path}`, { headers: authHeaders(accessToken) }),
    `Failed to load ${path}`,
  )
}

async function sendJson<T>(
  path: string,
  accessToken: string,
  method: 'POST' | 'PATCH',
  body: unknown,
): Promise<T> {
  return parseJsonResponse<T>(
    await fetch(`${apiBase}${path}`, {
      method,
      headers: authHeaders(accessToken),
      body: JSON.stringify(body),
    }),
    `Failed to send ${path}`,
  )
}

export async function redeemHandoff(handoffCode: string): Promise<ReportArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<ReportArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getSessionBootstrap(accessToken: string): Promise<ReportArrSessionBootstrapResponse> {
  return getJson<ReportArrSessionBootstrapResponse>('/api/session', accessToken)
}

export async function getMe(accessToken: string): Promise<ReportArrMeResponse> {
  return getJson<ReportArrMeResponse>('/api/me', accessToken)
}

export async function getWorkspaceSummary(accessToken: string): Promise<ReportArrSummaryResponse> {
  return getJson<ReportArrSummaryResponse>('/api/v1/workspace/summary', accessToken)
}

export async function listDatasets(accessToken: string): Promise<ReportArrDatasetResponse[]> {
  return getJson<ReportArrDatasetResponse[]>('/api/v1/integrations/datasets', accessToken)
}

export async function getDataset(
  accessToken: string,
  datasetId: string,
): Promise<ReportArrDatasetResponse> {
  return getJson<ReportArrDatasetResponse>(
    `/api/v1/integrations/datasets/${encodeURIComponent(datasetId)}`,
    accessToken,
  )
}

export async function listDatasetFields(accessToken: string): Promise<ReportArrDatasetFieldResponse[]> {
  return getJson<ReportArrDatasetFieldResponse[]>('/api/v1/workspace/dataset-fields', accessToken)
}

export async function createDataset(
  accessToken: string,
  body: ReportArrCreateDatasetRequest,
): Promise<ReportArrDatasetResponse> {
  return sendJson<ReportArrDatasetResponse>('/api/v1/integrations/datasets', accessToken, 'POST', body)
}

export async function refreshDataset(
  accessToken: string,
  datasetId: string,
  body: ReportArrRefreshDatasetRequest,
): Promise<ReportArrRefreshJobResponse> {
  return sendJson<ReportArrRefreshJobResponse>(
    `/api/v1/integrations/datasets/${encodeURIComponent(datasetId)}/refresh`,
    accessToken,
    'POST',
    body,
  )
}

export async function listDashboards(accessToken: string): Promise<ReportArrDashboardResponse[]> {
  return getJson<ReportArrDashboardResponse[]>('/api/v1/integrations/dashboards', accessToken)
}

export async function getDashboard(accessToken: string, dashboardId: string): Promise<ReportArrDashboardResponse> {
  return getJson<ReportArrDashboardResponse>(
    `/api/v1/integrations/dashboards/${encodeURIComponent(dashboardId)}`,
    accessToken,
  )
}

export async function listDashboardAccessPolicies(accessToken: string): Promise<ReportArrDashboardAccessPolicyResponse[]> {
  return getJson<ReportArrDashboardAccessPolicyResponse[]>('/api/v1/workspace/dashboard-access-policies', accessToken)
}

export async function listDashboardFilters(accessToken: string): Promise<ReportArrDashboardFilterResponse[]> {
  return getJson<ReportArrDashboardFilterResponse[]>('/api/v1/workspace/dashboard-filters', accessToken)
}

export async function listDrilldowns(accessToken: string): Promise<ReportArrDrilldownDefinitionResponse[]> {
  return getJson<ReportArrDrilldownDefinitionResponse[]>('/api/v1/workspace/drilldowns', accessToken)
}

export async function createDashboard(
  accessToken: string,
  body: ReportArrCreateDashboardRequest,
): Promise<ReportArrDashboardResponse> {
  return sendJson<ReportArrDashboardResponse>('/api/v1/integrations/dashboards', accessToken, 'POST', body)
}

export async function updateDashboard(
  accessToken: string,
  dashboardId: string,
  body: ReportArrUpdateDashboardRequest,
): Promise<ReportArrDashboardResponse> {
  return sendJson<ReportArrDashboardResponse>(
    `/api/v1/integrations/dashboards/${encodeURIComponent(dashboardId)}`,
    accessToken,
    'PATCH',
    body,
  )
}

export async function renderWidget(accessToken: string, widgetId: string): Promise<Record<string, unknown>> {
  return getJson<Record<string, unknown>>(`/api/v1/integrations/widgets/${encodeURIComponent(widgetId)}/render`, accessToken)
}

export async function listReportDefinitions(accessToken: string): Promise<ReportArrReportDefinitionResponse[]> {
  return getJson<ReportArrReportDefinitionResponse[]>('/api/v1/integrations/report-definitions', accessToken)
}

export async function getReportDefinition(accessToken: string, reportDefinitionId: string): Promise<ReportArrReportDefinitionResponse> {
  return getJson<ReportArrReportDefinitionResponse>(
    `/api/v1/integrations/report-definitions/${encodeURIComponent(reportDefinitionId)}`,
    accessToken,
  )
}

export async function listReportAccessPolicies(accessToken: string): Promise<ReportArrReportAccessPolicyResponse[]> {
  return getJson<ReportArrReportAccessPolicyResponse[]>('/api/v1/workspace/report-access-policies', accessToken)
}

export async function createReportDefinition(
  accessToken: string,
  body: ReportArrCreateReportDefinitionRequest,
): Promise<ReportArrReportDefinitionResponse> {
  return sendJson<ReportArrReportDefinitionResponse>('/api/v1/integrations/report-definitions', accessToken, 'POST', body)
}

export async function updateReportDefinition(
  accessToken: string,
  reportDefinitionId: string,
  body: ReportArrUpdateReportDefinitionRequest,
): Promise<ReportArrReportDefinitionResponse> {
  return sendJson<ReportArrReportDefinitionResponse>(
    `/api/v1/integrations/report-definitions/${encodeURIComponent(reportDefinitionId)}`,
    accessToken,
    'PATCH',
    body,
  )
}

export async function createReportRun(
  accessToken: string,
  body: ReportArrCreateReportRunRequest,
): Promise<ReportArrReportRunResponse> {
  return sendJson<ReportArrReportRunResponse>('/api/v1/integrations/report-runs', accessToken, 'POST', body)
}

export async function getReportRun(accessToken: string, reportRunId: string): Promise<ReportArrReportRunResponse> {
  return getJson<ReportArrReportRunResponse>(
    `/api/v1/integrations/report-runs/${encodeURIComponent(reportRunId)}`,
    accessToken,
  )
}

export async function cancelReportRun(
  accessToken: string,
  reportRunId: string,
  body: ReportArrCancelReportRunRequest,
): Promise<ReportArrReportRunResponse> {
  return sendJson<ReportArrReportRunResponse>(
    `/api/v1/integrations/report-runs/${encodeURIComponent(reportRunId)}/cancel`,
    accessToken,
    'POST',
    body,
  )
}

export async function listReportSchedules(accessToken: string): Promise<ReportArrReportScheduleResponse[]> {
  return getJson<ReportArrReportScheduleResponse[]>('/api/v1/integrations/report-schedules', accessToken)
}

export async function listReportRecipients(accessToken: string): Promise<ReportArrReportRecipientResponse[]> {
  return getJson<ReportArrReportRecipientResponse[]>('/api/v1/workspace/report-recipients', accessToken)
}

export async function listReportParameters(accessToken: string): Promise<ReportArrReportParameterResponse[]> {
  return getJson<ReportArrReportParameterResponse[]>('/api/v1/workspace/report-parameters', accessToken)
}

export async function listReportSections(accessToken: string): Promise<ReportArrReportSectionResponse[]> {
  return getJson<ReportArrReportSectionResponse[]>('/api/v1/workspace/report-sections', accessToken)
}

export async function listReportRuns(accessToken: string): Promise<ReportArrReportRunResponse[]> {
  return getJson<ReportArrReportRunResponse[]>('/api/v1/integrations/report-runs', accessToken)
}

export async function createReportSchedule(
  accessToken: string,
  body: ReportArrCreateReportScheduleRequest,
): Promise<ReportArrReportScheduleResponse> {
  return sendJson<ReportArrReportScheduleResponse>('/api/v1/integrations/report-schedules', accessToken, 'POST', body)
}

export async function updateReportSchedule(
  accessToken: string,
  scheduleId: string,
  body: ReportArrUpdateReportScheduleRequest,
): Promise<ReportArrReportScheduleResponse> {
  return sendJson<ReportArrReportScheduleResponse>(
    `/api/v1/integrations/report-schedules/${encodeURIComponent(scheduleId)}`,
    accessToken,
    'PATCH',
    body,
  )
}

export async function createExport(
  accessToken: string,
  body: ReportArrCreateExportRequest,
): Promise<ReportArrExportJobResponse> {
  return sendJson<ReportArrExportJobResponse>('/api/v1/integrations/exports', accessToken, 'POST', body)
}

export async function listExportJobs(accessToken: string): Promise<ReportArrExportJobResponse[]> {
  return getJson<ReportArrExportJobResponse[]>('/api/v1/integrations/exports', accessToken)
}

export async function getExportJob(accessToken: string, exportJobId: string): Promise<ReportArrExportJobResponse> {
  return getJson<ReportArrExportJobResponse>(`/api/v1/integrations/exports/${encodeURIComponent(exportJobId)}`, accessToken)
}

export async function listKpis(accessToken: string): Promise<ReportArrKpiDefinitionResponse[]> {
  return getJson<ReportArrKpiDefinitionResponse[]>('/api/v1/integrations/kpis', accessToken)
}

export async function getKpi(accessToken: string, kpiId: string): Promise<ReportArrKpiDefinitionResponse> {
  return getJson<ReportArrKpiDefinitionResponse>(`/api/v1/integrations/kpis/${encodeURIComponent(kpiId)}`, accessToken)
}

export async function listKpiValues(accessToken: string): Promise<ReportArrKpiValueResponse[]> {
  return getJson<ReportArrKpiValueResponse[]>('/api/v1/workspace/kpi-values', accessToken)
}

export async function listMetrics(accessToken: string): Promise<ReportArrMetricDefinitionResponse[]> {
  return getJson<ReportArrMetricDefinitionResponse[]>('/api/v1/integrations/metrics', accessToken)
}

export async function listMetricValues(accessToken: string): Promise<ReportArrMetricValueResponse[]> {
  return getJson<ReportArrMetricValueResponse[]>('/api/v1/workspace/metric-values', accessToken)
}

export async function listAnalyticsSnapshots(accessToken: string): Promise<ReportArrAnalyticsSnapshotResponse[]> {
  return getJson<ReportArrAnalyticsSnapshotResponse[]>('/api/v1/workspace/analytics-snapshots', accessToken)
}

export async function listTrendAnalyses(accessToken: string): Promise<ReportArrTrendAnalysisResponse[]> {
  return getJson<ReportArrTrendAnalysisResponse[]>('/api/v1/workspace/trend-analyses', accessToken)
}

export async function listExceptionQueries(accessToken: string): Promise<ReportArrExceptionQueryResponse[]> {
  return getJson<ReportArrExceptionQueryResponse[]>('/api/v1/workspace/exception-queries', accessToken)
}

export async function listExceptionResults(accessToken: string): Promise<ReportArrExceptionResultResponse[]> {
  return getJson<ReportArrExceptionResultResponse[]>('/api/v1/workspace/exception-results', accessToken)
}

export async function calculateKpi(
  accessToken: string,
  kpiId: string,
  body: ReportArrCalculateKpiRequest,
): Promise<ReportArrKpiValueResponse> {
  return sendJson<ReportArrKpiValueResponse>(
    `/api/v1/integrations/kpis/${encodeURIComponent(kpiId)}/calculate`,
    accessToken,
    'POST',
    body,
  )
}

export async function listAlerts(accessToken: string): Promise<ReportArrAlertResponse[]> {
  return getJson<ReportArrAlertResponse[]>('/api/v1/integrations/alerts', accessToken)
}

export async function acknowledgeAlert(
  accessToken: string,
  alertId: string,
  body: ReportArrAcknowledgeAlertRequest,
): Promise<ReportArrAlertResponse> {
  return sendJson<ReportArrAlertResponse>(
    `/api/v1/integrations/alerts/${encodeURIComponent(alertId)}/acknowledge`,
    accessToken,
    'POST',
    body,
  )
}

export async function resolveAlert(
  accessToken: string,
  alertId: string,
  body: ReportArrResolveAlertRequest,
): Promise<ReportArrAlertResponse> {
  return sendJson<ReportArrAlertResponse>(
    `/api/v1/integrations/alerts/${encodeURIComponent(alertId)}/resolve`,
    accessToken,
    'POST',
    body,
  )
}

export async function listAuditPackages(accessToken: string): Promise<ReportArrAuditPackageResponse[]> {
  return getJson<ReportArrAuditPackageResponse[]>('/api/v1/integrations/audit-packages', accessToken)
}

export async function getAuditPackage(
  accessToken: string,
  auditReportPackageId: string,
): Promise<ReportArrAuditPackageResponse> {
  return getJson<ReportArrAuditPackageResponse>(
    `/api/v1/integrations/audit-packages/${encodeURIComponent(auditReportPackageId)}`,
    accessToken,
  )
}

export async function listAuditScopes(accessToken: string): Promise<ReportArrAuditScopeResponse[]> {
  return getJson<ReportArrAuditScopeResponse[]>('/api/v1/workspace/audit-scopes', accessToken)
}

export async function createAuditPackage(
  accessToken: string,
  body: ReportArrCreateAuditPackageRequest,
): Promise<ReportArrAuditPackageResponse> {
  return sendJson<ReportArrAuditPackageResponse>('/api/v1/integrations/audit-packages', accessToken, 'POST', body)
}

export async function lockAuditPackage(
  accessToken: string,
  auditPackageId: string,
  body: ReportArrLockAuditPackageRequest,
): Promise<ReportArrAuditPackageResponse> {
  return sendJson<ReportArrAuditPackageResponse>(
    `/api/v1/integrations/audit-packages/${encodeURIComponent(auditPackageId)}/lock`,
    accessToken,
    'POST',
    body,
  )
}

export async function listSourceConnectors(accessToken: string): Promise<ReportArrSourceConnectorResponse[]> {
  return getJson<ReportArrSourceConnectorResponse[]>('/api/v1/workspace/source-connectors', accessToken)
}

export async function listIngestionCursors(accessToken: string): Promise<ReportArrIngestionCursorResponse[]> {
  return getJson<ReportArrIngestionCursorResponse[]>('/api/v1/workspace/ingestion-cursors', accessToken)
}

export async function listWidgets(accessToken: string): Promise<ReportArrDashboardWidgetResponse[]> {
  return getJson<ReportArrDashboardWidgetResponse[]>('/api/v1/integrations/widgets', accessToken)
}

export async function listWidgetVisualizations(accessToken: string): Promise<ReportArrWidgetVisualizationSettingsResponse[]> {
  return getJson<ReportArrWidgetVisualizationSettingsResponse[]>('/api/v1/integrations/widget-visualizations', accessToken)
}

export async function listReadModels(accessToken: string): Promise<ReportArrReadModelResponse[]> {
  return getJson<ReportArrReadModelResponse[]>('/api/v1/integrations/read-models', accessToken)
}

export async function getReadModel(accessToken: string, readModelId: string): Promise<ReportArrReadModelResponse> {
  return getJson<ReportArrReadModelResponse>(`/api/v1/integrations/read-models/${encodeURIComponent(readModelId)}`, accessToken)
}

export async function listReadModelRecords(accessToken: string): Promise<ReportArrReadModelRecordResponse[]> {
  return getJson<ReportArrReadModelRecordResponse[]>('/api/v1/workspace/read-model-records', accessToken)
}

export async function listDatasetLineage(accessToken: string): Promise<ReportArrDatasetLineageResponse[]> {
  return getJson<ReportArrDatasetLineageResponse[]>('/api/v1/workspace/dataset-lineage', accessToken)
}

export async function rebuildReadModel(
  accessToken: string,
  readModelId: string,
  requestedByPersonId: string,
): Promise<ReportArrRefreshJobResponse> {
  return sendJson<ReportArrRefreshJobResponse>(
    `/api/v1/integrations/read-models/${encodeURIComponent(readModelId)}/rebuild`,
    accessToken,
    'POST',
    { requestedByPersonId },
  )
}

export async function listRefreshJobs(accessToken: string): Promise<ReportArrRefreshJobResponse[]> {
  return getJson<ReportArrRefreshJobResponse[]>('/api/v1/workspace/refresh-jobs', accessToken)
}

export async function listSourceEvents(accessToken: string): Promise<ReportArrSourceEventReceiptResponse[]> {
  return getJson<ReportArrSourceEventReceiptResponse[]>('/api/v1/integrations/source-events', accessToken)
}

export async function receiveEvent(
  accessToken: string,
  body: ReportArrIntegrationEventRequest,
): Promise<ReportArrSourceEventReceiptResponse> {
  return sendJson<ReportArrSourceEventReceiptResponse>('/api/v1/integrations/events', accessToken, 'POST', body)
}

export async function receiveEventBatch(
  accessToken: string,
  body: { events: ReportArrIntegrationEventRequest[] },
): Promise<{ received: number; receipts: ReportArrSourceEventReceiptResponse[] }> {
  return sendJson<{ received: number; receipts: ReportArrSourceEventReceiptResponse[] }>(
    '/api/v1/integrations/events/batch',
    accessToken,
    'POST',
    body,
  )
}
