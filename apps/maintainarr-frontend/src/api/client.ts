import type {
  AssetClassResponse,
  AssetResponse,
  AssetTypeResponse,
  CreateAssetClassRequest,
  CreateAssetRequest,
  CreateAssetTypeRequest,
  CreateInspectionChecklistItemRequest,
  CreateInspectionTemplateCategoryRequest,
  CreateInspectionTemplateRequest,
  HandoffSessionResponse,
  InspectionRunDetailResponse,
  AssetMeterResponse,
  CreateAssetMeterRequest,
  CreateDefectRequest,
  CreateDefectsFromInspectionRunRequest,
  CreateDefectsFromInspectionRunResponse,
  DefectDetailResponse,
  DefectSummaryResponse,
  MeterPmForecastResponse,
  MeterReadingResponse,
  RecordMeterReadingRequest,
  InspectionRunSummaryResponse,
  InspectionTemplateDetailResponse,
  InspectionTemplateSummaryResponse,
  MaintainArrMeResponse,
  AssetReadinessResponse,
  AssetReadinessSummaryResponse,
  AuditPackageExportResponse,
  AuditPackageGenerationJobResponse,
  AuditPackageManifestResponse,
  MaintenanceNotificationDispatchesResponse,
  MaintenanceNotificationSettingsResponse,
  UpsertDefectEscalationSettingsRequest,
  DefectEscalationSettingsResponse,
  PendingDefectEscalationsResponse,
  DefectEscalationRunsResponse,
  DefectEscalationEventsResponse,
  AssetStatusRollupSettingsResponse,
  UpsertAssetStatusRollupSettingsRequest,
  PendingAssetStatusRollupsResponse,
  AssetStatusRollupRunsResponse,
  AssetStatusScopeRollupSummaryResponse,
  MaintenanceHistorySummaryResponse,
  MaintenanceHistoryRollupSettingsResponse,
  UpsertMaintenanceHistoryRollupSettingsRequest,
  PendingMaintenanceHistoryRollupsResponse,
  MaintenanceHistoryRollupRunsResponse,
  MaintenanceHistoryEntryResponse,
  UpsertMaintenanceNotificationSettingsRequest,
  PagedResult,
  PmProgramDetailResponse,
  PmProgramSummaryResponse,
  CreatePmProgramRequest,
  ReplacePmProgramSchedulesRequest,
  PmScheduleResponse,
  StartInspectionRunRequest,
  SubmitInspectionRunAnswersRequest,
  UpdateDefectStatusRequest,
  WorkOrderDetailResponse,
  WorkOrderSummaryResponse,
  CreateWorkOrderRequest,
  CreateWorkOrderFromDefectRequest,
  UpdateWorkOrderStatusRequest,
  WorkOrderTaskLineResponse,
  CreateWorkOrderTaskLineRequest,
  WorkOrderLaborEntryResponse,
  CreateWorkOrderLaborEntryRequest,
  WorkOrderEvidenceResponse,
  CreateWorkOrderEvidenceRequest,
  WorkOrderPartsDemandLineResponse,
  CreateWorkOrderPartsDemandLineRequest,
  PublishWorkOrderPartsDemandRequest,
  PublishWorkOrderPartsDemandResponse,
} from './types'

const apiBase = import.meta.env.VITE_MAINTAINARR_API_BASE ?? ''

export class MaintainArrApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'MaintainArrApiError'
  }
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
    throw new MaintainArrApiError(body || `${fallbackMessage} (${response.status})`, response.status, body)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/handoff/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<HandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getMe(accessToken: string): Promise<MaintainArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrMeResponse>(response, 'Failed to load profile')
}

export async function getAssetClasses(accessToken: string): Promise<AssetClassResponse[]> {
  const response = await fetch(`${apiBase}/api/asset-classes`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetClassResponse[]>(response, 'Failed to load asset classes')
}

export async function createAssetClass(
  accessToken: string,
  payload: CreateAssetClassRequest,
): Promise<AssetClassResponse> {
  const response = await fetch(`${apiBase}/api/asset-classes`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetClassResponse>(response, 'Failed to create asset class')
}

export async function getAssetTypes(accessToken: string): Promise<AssetTypeResponse[]> {
  const response = await fetch(`${apiBase}/api/asset-types`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetTypeResponse[]>(response, 'Failed to load asset types')
}

export async function createAssetType(
  accessToken: string,
  payload: CreateAssetTypeRequest,
): Promise<AssetTypeResponse> {
  const response = await fetch(`${apiBase}/api/asset-types`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetTypeResponse>(response, 'Failed to create asset type')
}

export async function getAssets(accessToken: string): Promise<AssetResponse[]> {
  const response = await fetch(`${apiBase}/api/assets`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetResponse[]>(response, 'Failed to load assets')
}

export async function createAsset(accessToken: string, payload: CreateAssetRequest): Promise<AssetResponse> {
  const response = await fetch(`${apiBase}/api/assets`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetResponse>(response, 'Failed to create asset')
}

export async function getDuePmSchedules(accessToken: string): Promise<PmScheduleResponse[]> {
  const response = await fetch(`${apiBase}/api/preventive-maintenance/due`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmScheduleResponse[]>(response, 'Failed to load due PM schedules')
}

export async function getPmSchedules(accessToken: string): Promise<PmScheduleResponse[]> {
  const response = await fetch(`${apiBase}/api/preventive-maintenance/schedules`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmScheduleResponse[]>(response, 'Failed to load PM schedules')
}

export async function getPmPrograms(accessToken: string): Promise<PmProgramSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/preventive-maintenance/programs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmProgramSummaryResponse[]>(response, 'Failed to load PM programs')
}

export async function getPmProgram(
  accessToken: string,
  pmProgramId: string,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/preventive-maintenance/programs/${pmProgramId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to load PM program')
}

export async function createPmProgram(
  accessToken: string,
  payload: CreatePmProgramRequest,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/preventive-maintenance/programs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to create PM program')
}

export async function replacePmProgramSchedules(
  accessToken: string,
  pmProgramId: string,
  payload: ReplacePmProgramSchedulesRequest,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/preventive-maintenance/programs/${pmProgramId}/schedules`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to assign PM schedules')
}

export async function activatePmProgram(
  accessToken: string,
  pmProgramId: string,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/preventive-maintenance/programs/${pmProgramId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ status: 'active' }),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to activate PM program')
}

export async function getInspectionTemplates(
  accessToken: string,
): Promise<InspectionTemplateSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/inspection-templates`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionTemplateSummaryResponse[]>(response, 'Failed to load inspection templates')
}

export async function getInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to load inspection template')
}

export async function createInspectionTemplate(
  accessToken: string,
  payload: CreateInspectionTemplateRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to create inspection template')
}

export async function createInspectionTemplateCategory(
  accessToken: string,
  inspectionTemplateId: string,
  payload: CreateInspectionTemplateCategoryRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/categories`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    const body = await response.text()
    throw new MaintainArrApiError(body || `Failed to create category (${response.status})`, response.status, body)
  }
  await response.json()
  return getInspectionTemplate(accessToken, inspectionTemplateId)
}

export async function createInspectionChecklistItem(
  accessToken: string,
  inspectionTemplateId: string,
  payload: CreateInspectionChecklistItemRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/checklist-items`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    const body = await response.text()
    throw new MaintainArrApiError(body || `Failed to create checklist item (${response.status})`, response.status, body)
  }
  await response.json()
  return getInspectionTemplate(accessToken, inspectionTemplateId)
}

export async function replaceInspectionTemplateAssetTypes(
  accessToken: string,
  inspectionTemplateId: string,
  assetTypeIds: string[],
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/asset-types`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ assetTypeIds }),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to link asset types')
}

export async function activateInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ status: 'active' }),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to activate inspection template')
}

export async function getInspectionRuns(accessToken: string): Promise<InspectionRunSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/inspections`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionRunSummaryResponse[]>(response, 'Failed to load inspection runs')
}

export async function getInspectionRun(
  accessToken: string,
  inspectionRunId: string,
): Promise<InspectionRunDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionRunDetailResponse>(response, 'Failed to load inspection run')
}

export async function startInspectionRun(
  accessToken: string,
  payload: StartInspectionRunRequest,
): Promise<InspectionRunDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspections`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionRunDetailResponse>(response, 'Failed to start inspection run')
}

export async function submitInspectionRunAnswers(
  accessToken: string,
  inspectionRunId: string,
  payload: SubmitInspectionRunAnswersRequest,
): Promise<InspectionRunDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/answers`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionRunDetailResponse>(response, 'Failed to submit inspection answers')
}

export async function completeInspectionRun(
  accessToken: string,
  inspectionRunId: string,
): Promise<InspectionRunDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/complete`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionRunDetailResponse>(response, 'Failed to complete inspection run')
}

export async function getDefects(
  accessToken: string,
  params?: { assetId?: string; inspectionRunId?: string; status?: string },
): Promise<DefectSummaryResponse[]> {
  const search = new URLSearchParams()
  if (params?.assetId) {
    search.set('assetId', params.assetId)
  }
  if (params?.inspectionRunId) {
    search.set('inspectionRunId', params.inspectionRunId)
  }
  if (params?.status) {
    search.set('status', params.status)
  }
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/defects${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectSummaryResponse[]>(response, 'Failed to load defects')
}

export async function getDefect(accessToken: string, defectId: string): Promise<DefectDetailResponse> {
  const response = await fetch(`${apiBase}/api/defects/${defectId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectDetailResponse>(response, 'Failed to load defect')
}

export async function createDefect(
  accessToken: string,
  payload: CreateDefectRequest,
): Promise<DefectDetailResponse> {
  const response = await fetch(`${apiBase}/api/defects`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DefectDetailResponse>(response, 'Failed to create defect')
}

export async function createDefectsFromInspectionRun(
  accessToken: string,
  inspectionRunId: string,
  payload: CreateDefectsFromInspectionRunRequest,
): Promise<CreateDefectsFromInspectionRunResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/defects`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<CreateDefectsFromInspectionRunResponse>(
    response,
    'Failed to create defects from inspection run',
  )
}

export async function updateDefectStatus(
  accessToken: string,
  defectId: string,
  payload: UpdateDefectStatusRequest,
): Promise<DefectDetailResponse> {
  const response = await fetch(`${apiBase}/api/defects/${defectId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DefectDetailResponse>(response, 'Failed to update defect status')
}

export async function getAssetMeters(accessToken: string, assetId: string): Promise<AssetMeterResponse[]> {
  const response = await fetch(`${apiBase}/api/assets/${assetId}/meters`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetMeterResponse[]>(response, 'Failed to load asset meters')
}

export async function createAssetMeter(
  accessToken: string,
  assetId: string,
  payload: CreateAssetMeterRequest,
): Promise<AssetMeterResponse> {
  const response = await fetch(`${apiBase}/api/assets/${assetId}/meters`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetMeterResponse>(response, 'Failed to create asset meter')
}

export async function getMeterReadings(
  accessToken: string,
  assetMeterId: string,
  limit?: number,
): Promise<MeterReadingResponse[]> {
  const query = limit ? `?limit=${limit}` : ''
  const response = await fetch(`${apiBase}/api/meters/${assetMeterId}/readings${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MeterReadingResponse[]>(response, 'Failed to load meter readings')
}

export async function recordMeterReading(
  accessToken: string,
  assetMeterId: string,
  payload: RecordMeterReadingRequest,
): Promise<MeterReadingResponse> {
  const response = await fetch(`${apiBase}/api/meters/${assetMeterId}/readings`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MeterReadingResponse>(response, 'Failed to record meter reading')
}

export async function getWorkOrders(
  accessToken: string,
  params?: { assetId?: string; defectId?: string; status?: string },
): Promise<WorkOrderSummaryResponse[]> {
  const search = new URLSearchParams()
  if (params?.assetId) {
    search.set('assetId', params.assetId)
  }
  if (params?.defectId) {
    search.set('defectId', params.defectId)
  }
  if (params?.status) {
    search.set('status', params.status)
  }
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/work-orders${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderSummaryResponse[]>(response, 'Failed to load work orders')
}

export async function getWorkOrder(accessToken: string, workOrderId: string): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to load work order')
}

export async function createWorkOrder(
  accessToken: string,
  payload: CreateWorkOrderRequest,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to create work order')
}

export async function createWorkOrderFromDefect(
  accessToken: string,
  defectId: string,
  payload: CreateWorkOrderFromDefectRequest,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/defects/${defectId}/work-orders`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to create work order from defect')
}

export async function updateWorkOrderStatus(
  accessToken: string,
  workOrderId: string,
  payload: UpdateWorkOrderStatusRequest,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to update work order status')
}

export async function getWorkOrderTasks(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderTaskLineResponse[]> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/tasks`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderTaskLineResponse[]>(response, 'Failed to load work order tasks')
}

export async function createWorkOrderTask(
  accessToken: string,
  workOrderId: string,
  payload: CreateWorkOrderTaskLineRequest,
): Promise<WorkOrderTaskLineResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/tasks`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderTaskLineResponse>(response, 'Failed to create work order task')
}

export async function getWorkOrderLabor(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderLaborEntryResponse[]> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/labor`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderLaborEntryResponse[]>(response, 'Failed to load work order labor')
}

export async function logWorkOrderLabor(
  accessToken: string,
  workOrderId: string,
  payload: CreateWorkOrderLaborEntryRequest,
): Promise<WorkOrderLaborEntryResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/labor`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderLaborEntryResponse>(response, 'Failed to log work order labor')
}

export async function getWorkOrderEvidence(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderEvidenceResponse[]> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/evidence`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderEvidenceResponse[]>(response, 'Failed to load work order evidence')
}

export async function uploadWorkOrderEvidence(
  accessToken: string,
  workOrderId: string,
  payload: CreateWorkOrderEvidenceRequest,
): Promise<WorkOrderEvidenceResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/evidence`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderEvidenceResponse>(response, 'Failed to upload work order evidence')
}

export async function getWorkOrderPartsDemand(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderPartsDemandLineResponse[]> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/parts-demand`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderPartsDemandLineResponse[]>(
    response,
    'Failed to load work order parts demand',
  )
}

export async function createWorkOrderPartsDemandLine(
  accessToken: string,
  workOrderId: string,
  payload: CreateWorkOrderPartsDemandLineRequest,
): Promise<WorkOrderPartsDemandLineResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/parts-demand`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderPartsDemandLineResponse>(
    response,
    'Failed to create work order parts demand line',
  )
}

export async function publishWorkOrderPartsDemand(
  accessToken: string,
  workOrderId: string,
  payload: PublishWorkOrderPartsDemandRequest = {},
): Promise<PublishWorkOrderPartsDemandResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/parts-demand/publish`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PublishWorkOrderPartsDemandResponse>(
    response,
    'Failed to publish work order parts demand',
  )
}

export async function getMeterPmForecast(
  accessToken: string,
  assetMeterId: string,
): Promise<MeterPmForecastResponse> {
  const response = await fetch(`${apiBase}/api/meters/${assetMeterId}/pm-forecast`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MeterPmForecastResponse>(response, 'Failed to load meter PM forecast')
}

export async function getMaintenanceHistory(
  accessToken: string,
  assetId: string,
  page = 1,
  pageSize = 50,
): Promise<PagedResult<MaintenanceHistoryEntryResponse>> {
  const search = new URLSearchParams({
    assetId,
    page: String(page),
    pageSize: String(pageSize),
  })
  const response = await fetch(`${apiBase}/api/maintenance-history?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PagedResult<MaintenanceHistoryEntryResponse>>(
    response,
    'Failed to load maintenance history',
  )
}

export async function getMaintenanceHistorySummary(
  accessToken: string,
  assetId: string,
): Promise<MaintenanceHistorySummaryResponse> {
  const search = new URLSearchParams({ assetId })
  const response = await fetch(`${apiBase}/api/maintenance-history/summary?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceHistorySummaryResponse>(
    response,
    'Failed to load maintenance history summary',
  )
}

export async function getAssetReadiness(
  accessToken: string,
  assetId: string,
): Promise<AssetReadinessResponse> {
  const search = new URLSearchParams({ assetId })
  const response = await fetch(`${apiBase}/api/asset-readiness?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetReadinessResponse>(response, 'Failed to load asset readiness')
}

export async function getAssetReadinessFleet(
  accessToken: string,
): Promise<AssetReadinessSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/asset-readiness`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetReadinessSummaryResponse[]>(response, 'Failed to load asset readiness fleet')
}

export async function getMaintenanceNotificationSettings(
  accessToken: string,
): Promise<MaintenanceNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/notification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceNotificationSettingsResponse>(
    response,
    'Failed to load notification settings',
  )
}

export async function upsertMaintenanceNotificationSettings(
  accessToken: string,
  payload: UpsertMaintenanceNotificationSettingsRequest,
): Promise<MaintenanceNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/notification-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenanceNotificationSettingsResponse>(
    response,
    'Failed to save notification settings',
  )
}

export async function getDefectEscalationSettings(
  accessToken: string,
): Promise<DefectEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/defect-escalation-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectEscalationSettingsResponse>(
    response,
    'Failed to load defect escalation settings',
  )
}

export async function upsertDefectEscalationSettings(
  accessToken: string,
  payload: UpsertDefectEscalationSettingsRequest,
): Promise<DefectEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/defect-escalation-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DefectEscalationSettingsResponse>(
    response,
    'Failed to save defect escalation settings',
  )
}

export async function getPendingDefectEscalations(
  accessToken: string,
): Promise<PendingDefectEscalationsResponse> {
  const response = await fetch(`${apiBase}/api/defect-escalation-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingDefectEscalationsResponse>(
    response,
    'Failed to load pending defect escalations',
  )
}

export async function getDefectEscalationRuns(
  accessToken: string,
  limit = 5,
): Promise<DefectEscalationRunsResponse> {
  const response = await fetch(`${apiBase}/api/defect-escalation-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectEscalationRunsResponse>(
    response,
    'Failed to load defect escalation runs',
  )
}

export async function getDefectEscalationEvents(
  accessToken: string,
  limit = 5,
): Promise<DefectEscalationEventsResponse> {
  const response = await fetch(`${apiBase}/api/defect-escalation-settings/events?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectEscalationEventsResponse>(
    response,
    'Failed to load defect escalation events',
  )
}

export async function getAssetStatusRollupSettings(
  accessToken: string,
): Promise<AssetStatusRollupSettingsResponse> {
  const response = await fetch(`${apiBase}/api/asset-status-rollup-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetStatusRollupSettingsResponse>(
    response,
    'Failed to load asset status rollup settings',
  )
}

export async function upsertAssetStatusRollupSettings(
  accessToken: string,
  payload: UpsertAssetStatusRollupSettingsRequest,
): Promise<AssetStatusRollupSettingsResponse> {
  const response = await fetch(`${apiBase}/api/asset-status-rollup-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetStatusRollupSettingsResponse>(
    response,
    'Failed to save asset status rollup settings',
  )
}

export async function getPendingAssetStatusRollups(
  accessToken: string,
): Promise<PendingAssetStatusRollupsResponse> {
  const response = await fetch(`${apiBase}/api/asset-status-rollup-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingAssetStatusRollupsResponse>(
    response,
    'Failed to load pending asset status rollups',
  )
}

export async function getAssetStatusRollupRuns(
  accessToken: string,
  limit = 5,
): Promise<AssetStatusRollupRunsResponse> {
  const response = await fetch(`${apiBase}/api/asset-status-rollup-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetStatusRollupRunsResponse>(
    response,
    'Failed to load asset status rollup runs',
  )
}

export async function getFleetAssetStatusRollup(
  accessToken: string,
): Promise<AssetStatusScopeRollupSummaryResponse> {
  const response = await fetch(`${apiBase}/api/asset-status-rollups/fleet`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetStatusScopeRollupSummaryResponse>(
    response,
    'Failed to load fleet asset status rollup',
  )
}

export async function getMaintenanceHistoryRollupSettings(
  accessToken: string,
): Promise<MaintenanceHistoryRollupSettingsResponse> {
  const response = await fetch(`${apiBase}/api/maintenance-history-rollup-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceHistoryRollupSettingsResponse>(
    response,
    'Failed to load maintenance history rollup settings',
  )
}

export async function upsertMaintenanceHistoryRollupSettings(
  accessToken: string,
  payload: UpsertMaintenanceHistoryRollupSettingsRequest,
): Promise<MaintenanceHistoryRollupSettingsResponse> {
  const response = await fetch(`${apiBase}/api/maintenance-history-rollup-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenanceHistoryRollupSettingsResponse>(
    response,
    'Failed to save maintenance history rollup settings',
  )
}

export async function getPendingMaintenanceHistoryRollups(
  accessToken: string,
): Promise<PendingMaintenanceHistoryRollupsResponse> {
  const response = await fetch(`${apiBase}/api/maintenance-history-rollup-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingMaintenanceHistoryRollupsResponse>(
    response,
    'Failed to load pending maintenance history rollups',
  )
}

export async function getMaintenanceHistoryRollupRuns(
  accessToken: string,
  limit = 5,
): Promise<MaintenanceHistoryRollupRunsResponse> {
  const response = await fetch(`${apiBase}/api/maintenance-history-rollup-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceHistoryRollupRunsResponse>(
    response,
    'Failed to load maintenance history rollup runs',
  )
}

function buildAuditPackageQuery(options?: { from?: string; to?: string; format?: string }): string {
  const params = new URLSearchParams()
  if (options?.from) {
    params.set('from', options.from)
  }
  if (options?.to) {
    params.set('to', options.to)
  }
  if (options?.format) {
    params.set('format', options.format)
  }
  const query = params.toString()
  return query ? `?${query}` : ''
}

function auditPackageDateBody(from?: string, to?: string): { from?: string; to?: string } {
  const body: { from?: string; to?: string } = {}
  if (from) {
    body.from = `${from}T00:00:00.000Z`
  }
  if (to) {
    body.to = `${to}T23:59:59.999Z`
  }
  return body
}

export async function getAuditPackageManifest(
  accessToken: string,
): Promise<AuditPackageManifestResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageManifestResponse>(response, 'Failed to load audit package manifest')
}

export async function exportAuditPackageZip(
  accessToken: string,
  options?: { from?: string; to?: string },
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery(options)}`,
    {
      headers: { Authorization: `Bearer ${accessToken}` },
    },
  )
  if (!response.ok) {
    const body = await response.text()
    throw new MaintainArrApiError(
      body || `Audit package export failed (${response.status})`,
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function exportAuditPackageJson(
  accessToken: string,
  options?: { from?: string; to?: string },
): Promise<AuditPackageExportResponse> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery({ ...options, format: 'json' })}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AuditPackageExportResponse>(response, 'Failed to export audit package JSON')
}

export async function createAuditPackageGenerationJob(
  accessToken: string,
  options: { format: 'zip' | 'json'; from?: string; to?: string },
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      format: options.format,
      ...auditPackageDateBody(options.from, options.to),
    }),
  })
  return parseJsonResponse<AuditPackageGenerationJobResponse>(
    response,
    'Failed to queue audit package generation job',
  )
}

export async function getAuditPackageGenerationJob(
  accessToken: string,
  jobId: string,
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs/${jobId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageGenerationJobResponse>(
    response,
    'Failed to load audit package generation job',
  )
}

export async function downloadAuditPackageGenerationJob(
  accessToken: string,
  jobId: string,
): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs/${jobId}/download`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    const body = await response.text()
    throw new MaintainArrApiError(
      body || `Audit package download failed (${response.status})`,
      response.status,
      body,
    )
  }
  return response.blob()
}

export async function getMaintenanceNotificationDispatches(
  accessToken: string,
  limit = 20,
): Promise<MaintenanceNotificationDispatchesResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/notification-settings/dispatches?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceNotificationDispatchesResponse>(
    response,
    'Failed to load notification dispatches',
  )
}
