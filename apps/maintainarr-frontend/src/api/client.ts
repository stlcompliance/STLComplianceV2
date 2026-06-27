import type {
  AssetClassResponse,
  AssetResponse,
  AssetSearchResponse,
  AssetTypeResponse,
  CreateAssetClassRequest,
  CreateAssetRequest,
  AssetUpsertV1Request,
  AssetFieldContextResponse,
  AssetInstalledComponentResponse,
  CatalogResponse,
  ReferenceOptionResponse,
  FieldsetResponse,
  CreateAssetTypeRequest,
  CreateInspectionChecklistItemRequest,
  CreateInspectionTemplateCategoryRequest,
  CreateInspectionTemplateRequest,
  InspectionTemplatePreviewResponse,
  InspectionTemplateValidationResponse,
  HandoffSessionResponse,
  InspectionRunDetailResponse,
  InspectionRunEvidenceResponse,
  AssetMeterResponse,
  CreateAssetMeterRequest,
  CreateDefectRequest,
  CreateDefectsFromInspectionRunRequest,
  CreateDefectsFromInspectionRunResponse,
  CreateMaintainArrEvidenceRequest,
  DefectDraftPreviewResponse,
  DefectDetailResponse,
  DefectDuplicateMatchResponse,
  DefectEvidenceResponse,
  DefectSummaryResponse,
  DefectSubmissionResponse,
  DefectValidationResponse,
  MeterPmForecastResponse,
  MeterReadingResponse,
  RecordMeterReadingRequest,
  InspectionRunSummaryResponse,
  InspectionTemplateDetailResponse,
  InspectionTemplateSummaryResponse,
  PublishInspectionTemplateRequest,
  MaintainArrMeResponse,
  MaintainArrSessionBootstrapResponse,
  AssetReadinessResponse,
  AssetReadinessSummaryResponse,
  AssetReadinessHistoryResponse,
  AssetReservationResponse,
  CreateAssetReservationRequest,
  ReservationActionRequest,
  AssetTelematicsIngestionResponse,
  AssetExternalIntelligenceOverviewResponse,
  AssetEnrichmentSnapshotResponse,
  AssetEnrichmentSuggestionResponse,
  AssetRecallSnapshotResponse,
  AssetComplaintSignalResponse,
  ExternalAssetIdentifierResponse,
  ExternalIntelligenceProviderSummaryResponse,
  ExternalProviderHealthResponse,
  RecallProviderSummaryResponse,
  RecallProviderHealthResponse,
  RecallCampaignResponse,
  AssetRecallCaseResponse,
  RecallDashboardResponse,
  RecallVehicleSearchRequest,
  RecallCampaignSearchRequest,
  CreateRecallCampaignRequest,
  UpdateRecallCampaignRequest,
  VerifyAssetRecallRequest,
  DismissAssetRecallRequest,
  ReleaseRecallHoldRequest,
  ExternalVinDecodeBatchRequest,
  ExternalVinDecodeBatchItemResponse,
  ExternalVinDecodeRequest,
  ExternalVinDecodeResponse,
  AuditPackageExportResponse,
  AuditPackageGenerationJobResponse,
  AuditPackageManifestResponse,
  MaintenanceNotificationDispatchesResponse,
  MaintenanceNotificationSettingsResponse,
  MaintainArrTenantSettingsAuditResponse,
  MaintainArrTenantSettingsResponse,
  ResetMaintainArrTenantSettingsRequest,
  UpsertMaintainArrTenantSettingsRequest,
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
  DowntimeTrackingSettingsResponse,
  UpsertDowntimeTrackingSettingsRequest,
  PendingAssetDowntimeSyncResponse,
  AssetDowntimeSyncRunsResponse,
  AssetDowntimeEventResponse,
  CreateManualDowntimeEventRequest,
  CloseDowntimeEventRequest,
  UpdateDowntimeEventReasonRequest,
  AssetAvailabilityResponse,
  FleetAvailabilityResponse,
  MaintenancePlatformEventSettingsResponse,
  UpsertMaintenancePlatformEventSettingsRequest,
  MaintenancePlatformOutboxEventsResponse,
  MaintenancePlatformEventProcessingRunsResponse,
  MaintenanceHistorySummaryResponse,
  MaintenanceHistoryRollupSettingsResponse,
  UpsertMaintenanceHistoryRollupSettingsRequest,
  PmDueScanSettingsResponse,
  UpsertPmDueScanSettingsRequest,
  PendingPmDueResponse,
  PmDueScanRunsResponse,
  TriggerPmDueScanResponse,
  PendingMaintenanceHistoryRollupsResponse,
  MaintenanceHistoryRollupRunsResponse,
  MaintenanceHistoryEntryResponse,
  UpsertMaintenanceNotificationSettingsRequest,
  PagedResult,
  PmProgramDetailResponse,
  PmProgramSummaryResponse,
  CreatePmProgramRequest,
  UpdatePmProgramRequest,
  ActivatePmProgramRequest,
  PmProgramScopePreviewResponse,
  PmProgramDuePreviewResponse,
  ReplacePmProgramSchedulesRequest,
  PmScheduleResponse,
  StartInspectionRunRequest,
  SubmitInspectionRunAnswersRequest,
  PauseInspectionRunRequest,
  ResumeInspectionRunRequest,
  TechnicianRefListResponse,
  UpsertTechnicianRefRequest,
  TechnicianRefResponse,
  InspectionVoiceGuidanceResponse,
  NormalizeVoiceNumericResponse,
  UpdateDefectStatusRequest,
  UpsertDefectDraftRequest,
  SubmitDefectRequest,
  UpdateInspectionChecklistItemRequest,
  UpdateInspectionTemplateCategoryRequest,
  UpdateInspectionTemplateRequest,
  UpdateInspectionTemplateStatusRequest,
  RetireInspectionTemplateRequest,
  WorkOrderDetailResponse,
  WorkOrderDuplicateMatchResponse,
  WorkOrderPreviewResponse,
  WorkOrderValidationResponse,
  WorkOrderSummaryResponse,
  CreateWorkOrderRequest,
  CreateWorkOrderFromDefectRequest,
  UpdateWorkOrderStatusRequest,
  WorkOrderTaskLineResponse,
  CreateWorkOrderTaskLineRequest,
  WorkOrderLaborEntryResponse,
  CreateWorkOrderLaborEntryRequest,
  UpdateWorkOrderLaborEntryStatusRequest,
  WorkOrderEvidenceResponse,
  WorkOrderCommentResponse,
  WorkOrderTimelineEventResponse,
  CreateWorkOrderEvidenceRequest,
  CreateWorkOrderCommentRequest,
  WorkOrderPartsDemandLineResponse,
  CreateWorkOrderPartsDemandLineRequest,
  PublishWorkOrderPartsDemandRequest,
  PublishWorkOrderPartsDemandResponse,
  WorkOrderPartsDemandStatusEventResponse,
  MaintenancePartsKitLineResponse,
  MaintenancePartsKitListResponse,
  MaintenancePartsKitPreviewRequest,
  MaintenancePartsKitPreviewResponse,
  MaintenancePartsKitValidationResponse,
  MaintenancePartsKitResponse,
  CreateMaintenancePartsKitRequest,
  UpdateMaintenancePartsKitRequest,
  UpdateMaintenancePartsKitStatusRequest,
  CreateMaintenancePartsKitLineRequest,
  UpdateMaintenancePartsKitLineRequest,
  MaintenanceVendorWorkListResponse,
  MaintenanceVendorWorkResponse,
  MaintenanceVendorWorkPortalResponse,
  UpsertMaintenanceVendorWorkRequest,
  UpdateMaintenanceVendorWorkPortalRequest,
  WorkOrderSupplyReadinessResponse,
  MaintenanceReportSummaryResponse,
  MaintenanceReportAssetDetailResponse,
  MaintenanceReportWorkOrderDetailResponse,
  ExecutiveReportSummaryResponse,
  ComplianceReportSummaryResponse,
  AssetBulkImportRequest,
  AssetBulkImportResponse,
  SchedulingBoardResponse,
  SchedulingMutationResponse,
  SchedulingRequest,
  SchedulingResourceLaneResponse,
  EntityExportManifestResponse,
} from './types'
import type { ProductImportHistoryEntry, ProductImportManifest } from '@stl/shared-ui'

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

type ProblemDetailsLike = {
  title?: string
  detail?: string
  errors?: Record<string, string[] | string>
}

function extractProblemDetailsMessage(body: string): string | null {
  if (!body.trim()) {
    return null
  }

  try {
    const parsed = JSON.parse(body) as ProblemDetailsLike
    const parts: string[] = []

    if (typeof parsed.title === 'string' && parsed.title.trim()) {
      parts.push(parsed.title.trim())
    }

    if (typeof parsed.detail === 'string' && parsed.detail.trim()) {
      parts.push(parsed.detail.trim())
    }

    const errorEntries = parsed.errors ? Object.entries(parsed.errors) : []
    if (errorEntries.length > 0) {
      const flattened = errorEntries
        .flatMap(([field, value]) => {
          const values = Array.isArray(value) ? value : [value]
          return values
            .map((message) => String(message).trim())
            .filter(Boolean)
            .map((message) => `${field}: ${message}`)
        })
      if (flattened.length > 0) {
        parts.push(flattened.join('; '))
      }
    }

    return parts.length > 0 ? parts.join(' - ') : null
  } catch {
    return null
  }
}

async function toApiError(
  response: Response,
  fallbackMessage: string,
): Promise<MaintainArrApiError> {
  const body = await response.text()
  const parsedMessage = extractProblemDetailsMessage(body)
  const message =
    response.status === 401
      ? `${fallbackMessage}: your MaintainArr session expired before this request was accepted. Launch MaintainArr from the suite again, then retry.`
      : parsedMessage || body || `${fallbackMessage} (${response.status})`
  return new MaintainArrApiError(message, response.status, body)
}

function authHeaders(accessToken: string): HeadersInit {
  return {
    Authorization: `Bearer ${accessToken}`,
    'Content-Type': 'application/json',
  }
}

async function parseJsonResponse<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    throw await toApiError(response, fallbackMessage)
  }

  return (await response.json()) as T
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
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

export async function getSessionBootstrap(
  accessToken: string,
): Promise<MaintainArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
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

export async function getAsset(accessToken: string, assetId: string): Promise<AssetResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetResponse>(response, 'Failed to load asset')
}

export async function searchAssets(
  accessToken: string,
  query?: string,
  limit = 25,
): Promise<AssetSearchResponse[]> {
  const search = new URLSearchParams()
  if (query && query.trim()) {
    search.set('query', query.trim())
  }
  search.set('limit', String(limit))
  const response = await fetch(`${apiBase}/api/v1/assets/search?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetSearchResponse[]>(response, 'Failed to search assets')
}

export async function createAsset(accessToken: string, payload: CreateAssetRequest): Promise<AssetResponse> {
  const response = await fetch(`${apiBase}/api/assets`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetResponse>(response, 'Failed to create asset')
}

export async function createAssetControlledV1(accessToken: string, payload: AssetUpsertV1Request): Promise<AssetResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetResponse>(response, 'Failed to create controlled asset')
}

export async function updateAssetControlledV1(accessToken: string, assetId: string, payload: AssetUpsertV1Request): Promise<AssetResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetResponse>(response, 'Failed to update controlled asset')
}

export async function getAssetFieldContext(accessToken: string, assetId: string): Promise<AssetFieldContextResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/field-context`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetFieldContextResponse>(response, 'Failed to load asset field context')
}

export async function getAssetInstalledComponents(
  accessToken: string,
  assetId: string,
): Promise<AssetInstalledComponentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/components`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetInstalledComponentResponse[]>(
    response,
    'Failed to load asset components',
  )
}

export async function getAssetCreateFieldset(accessToken: string): Promise<FieldsetResponse> {
  const response = await fetch(`${apiBase}/api/v1/fieldsets/assets/create`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldsetResponse>(response, 'Failed to load asset create fieldset')
}

export async function getAssetEditFieldset(accessToken: string, assetId: string): Promise<FieldsetResponse> {
  const response = await fetch(`${apiBase}/api/v1/fieldsets/assets/${assetId}/edit`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldsetResponse>(response, 'Failed to load asset edit fieldset')
}

export async function getWorkOrderCreateFieldset(accessToken: string): Promise<FieldsetResponse> {
  const response = await fetch(`${apiBase}/api/v1/fieldsets/work-orders/create`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldsetResponse>(response, 'Failed to load work order create fieldset')
}

export async function getDefectCreateFieldset(accessToken: string): Promise<FieldsetResponse> {
  const response = await fetch(`${apiBase}/api/v1/fieldsets/defects/create`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldsetResponse>(response, 'Failed to load defect create fieldset')
}

export async function getInspectionTemplateCreateFieldset(accessToken: string): Promise<FieldsetResponse> {
  const response = await fetch(`${apiBase}/api/v1/fieldsets/inspection-templates/create`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FieldsetResponse>(response, 'Failed to load inspection template create fieldset')
}

export async function getCatalogs(accessToken: string, keys?: string[]): Promise<CatalogResponse[]> {
  const query = keys && keys.length > 0 ? `?keys=${encodeURIComponent(keys.join(','))}` : ''
  const response = await fetch(`${apiBase}/api/v1/catalogs${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<CatalogResponse[]>(response, 'Failed to load catalogs')
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
  const response = await fetch(`${apiBase}/api/v1/pm-programs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmProgramSummaryResponse[]>(response, 'Failed to load PM programs')
}

export async function getPmProgram(
  accessToken: string,
  pmProgramId: string,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/pm-programs/${pmProgramId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to load PM program')
}

export async function createPmProgram(
  accessToken: string,
  payload: CreatePmProgramRequest,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/pm-programs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to create PM program')
}

export async function updatePmProgram(
  accessToken: string,
  pmProgramId: string,
  payload: UpdatePmProgramRequest,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/pm-programs/${pmProgramId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to update PM program')
}

export async function replacePmProgramSchedules(
  accessToken: string,
  pmProgramId: string,
  payload: ReplacePmProgramSchedulesRequest,
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/pm-programs/${pmProgramId}/schedules`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to assign PM schedules')
}

export async function activatePmProgram(
  accessToken: string,
  pmProgramId: string,
  payload: ActivatePmProgramRequest = {},
): Promise<PmProgramDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/pm-programs/${pmProgramId}/activate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramDetailResponse>(response, 'Failed to activate PM program')
}

export async function previewPmProgramScope(
  accessToken: string,
  payload: CreatePmProgramRequest,
): Promise<PmProgramScopePreviewResponse> {
  const response = await fetch(`${apiBase}/api/v1/pm-programs/preview-scope`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramScopePreviewResponse>(response, 'Failed to preview PM program scope')
}

export async function previewPmProgramDue(
  accessToken: string,
  payload: CreatePmProgramRequest,
): Promise<PmProgramDuePreviewResponse> {
  const response = await fetch(`${apiBase}/api/v1/pm-programs/preview-due`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmProgramDuePreviewResponse>(response, 'Failed to preview PM program due logic')
}

async function getReferenceOptions(
  accessToken: string,
  path: string,
): Promise<ReferenceOptionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/references/${path}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReferenceOptionResponse[]>(response, `Failed to load ${path} references`)
}

export async function getSites(accessToken: string): Promise<ReferenceOptionResponse[]> {
  return getReferenceOptions(accessToken, 'sites')
}

export async function getDepartments(accessToken: string): Promise<ReferenceOptionResponse[]> {
  return getReferenceOptions(accessToken, 'departments')
}

export async function getTeams(accessToken: string): Promise<ReferenceOptionResponse[]> {
  return getReferenceOptions(accessToken, 'teams')
}

export async function getPeople(accessToken: string): Promise<ReferenceOptionResponse[]> {
  return getReferenceOptions(accessToken, 'people')
}

export async function getParts(accessToken: string): Promise<ReferenceOptionResponse[]> {
  return getReferenceOptions(accessToken, 'parts')
}

export async function getComplianceCoreCatalogOptions(
  accessToken: string,
  catalogKey: string,
): Promise<ReferenceOptionResponse[]> {
  return getReferenceOptions(accessToken, `compliance-core/catalogs/${encodeURIComponent(catalogKey)}`)
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

export async function updateInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
  payload: UpdateInspectionTemplateRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to update inspection template')
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

export async function validateInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
): Promise<InspectionTemplateValidationResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/validate`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionTemplateValidationResponse>(response, 'Failed to validate inspection template')
}

export async function previewInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
): Promise<InspectionTemplatePreviewResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/preview`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionTemplatePreviewResponse>(response, 'Failed to preview inspection template')
}

export async function publishInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
  payload: PublishInspectionTemplateRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/publish`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to publish inspection template')
}

export async function retireInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
  payload: RetireInspectionTemplateRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/retire`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to retire inspection template')
}

export async function updateInspectionTemplateStatus(
  accessToken: string,
  inspectionTemplateId: string,
  payload: UpdateInspectionTemplateStatusRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to update inspection template status')
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
    throw await toApiError(response, 'Failed to create category')
  }
  await response.json()
  return getInspectionTemplate(accessToken, inspectionTemplateId)
}

export async function updateInspectionTemplateCategory(
  accessToken: string,
  inspectionTemplateId: string,
  categoryId: string,
  payload: UpdateInspectionTemplateCategoryRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/categories/${categoryId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to update category')
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
    throw await toApiError(response, 'Failed to create checklist item')
  }
  await response.json()
  return getInspectionTemplate(accessToken, inspectionTemplateId)
}

export async function updateInspectionChecklistItem(
  accessToken: string,
  inspectionTemplateId: string,
  checklistItemId: string,
  payload: UpdateInspectionChecklistItemRequest,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/checklist-items/${checklistItemId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to update checklist item')
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
  return updateInspectionTemplateStatus(accessToken, inspectionTemplateId, { status: 'active' })
}

export async function cloneInspectionTemplate(
  accessToken: string,
  inspectionTemplateId: string,
): Promise<InspectionTemplateDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspection-templates/${inspectionTemplateId}/clone`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionTemplateDetailResponse>(response, 'Failed to clone inspection template')
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

export async function pauseInspectionRun(
  accessToken: string,
  inspectionRunId: string,
  payload: PauseInspectionRunRequest,
): Promise<InspectionRunDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/pause`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionRunDetailResponse>(response, 'Failed to pause inspection run')
}

export async function resumeInspectionRun(
  accessToken: string,
  inspectionRunId: string,
  payload: ResumeInspectionRunRequest,
): Promise<InspectionRunDetailResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/resume`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionRunDetailResponse>(response, 'Failed to resume inspection run')
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

export async function getInspectionVoiceGuidance(
  accessToken: string,
  inspectionRunId: string,
): Promise<InspectionVoiceGuidanceResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/voice-guidance`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionVoiceGuidanceResponse>(response, 'Failed to load inspection voice guidance')
}

export async function normalizeInspectionVoiceNumeric(
  accessToken: string,
  transcript: string,
): Promise<NormalizeVoiceNumericResponse> {
  const response = await fetch(`${apiBase}/api/inspections/voice/normalize-numeric`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ transcript }),
  })
  return parseJsonResponse<NormalizeVoiceNumericResponse>(response, 'Failed to normalize voice numeric answer')
}

export async function getTechnicianRefs(accessToken: string): Promise<TechnicianRefListResponse> {
  const response = await fetch(`${apiBase}/api/technician-refs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TechnicianRefListResponse>(response, 'Failed to load technician references')
}

export async function upsertTechnicianRef(
  accessToken: string,
  payload: UpsertTechnicianRefRequest,
): Promise<TechnicianRefResponse> {
  const response = await fetch(`${apiBase}/api/technician-refs`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<TechnicianRefResponse>(response, 'Failed to upsert technician reference')
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

export async function createDefectDraft(
  accessToken: string,
  payload: UpsertDefectDraftRequest,
): Promise<DefectDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/defects/drafts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DefectDetailResponse>(response, 'Failed to create defect draft')
}

export async function updateDefectDraft(
  accessToken: string,
  defectId: string,
  payload: UpsertDefectDraftRequest,
): Promise<DefectDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/defects/${defectId}/draft`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DefectDetailResponse>(response, 'Failed to update defect draft')
}

export async function validateDefectDraft(
  accessToken: string,
  defectId: string,
): Promise<DefectValidationResponse> {
  const response = await fetch(`${apiBase}/api/v1/defects/${defectId}/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectValidationResponse>(response, 'Failed to validate defect draft')
}

export async function checkDuplicateDefectDraft(
  accessToken: string,
  defectId: string,
): Promise<DefectDuplicateMatchResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/defects/${defectId}/duplicates`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectDuplicateMatchResponse[]>(response, 'Failed to check defect duplicates')
}

export async function previewDefectDraft(
  accessToken: string,
  defectId: string,
): Promise<DefectDraftPreviewResponse> {
  const response = await fetch(`${apiBase}/api/v1/defects/${defectId}/preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectDraftPreviewResponse>(response, 'Failed to preview defect draft')
}

export async function submitDefectDraft(
  accessToken: string,
  defectId: string,
  payload: SubmitDefectRequest,
): Promise<DefectSubmissionResponse> {
  const response = await fetch(`${apiBase}/api/v1/defects/${defectId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DefectSubmissionResponse>(response, 'Failed to submit defect draft')
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

export async function getDefectEvidence(
  accessToken: string,
  defectId: string,
): Promise<DefectEvidenceResponse[]> {
  const response = await fetch(`${apiBase}/api/defects/${defectId}/evidence`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DefectEvidenceResponse[]>(response, 'Failed to load defect evidence')
}

export async function uploadDefectEvidence(
  accessToken: string,
  defectId: string,
  payload: CreateMaintainArrEvidenceRequest,
): Promise<DefectEvidenceResponse> {
  const response = await fetch(`${apiBase}/api/defects/${defectId}/evidence`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DefectEvidenceResponse>(response, 'Failed to upload defect evidence')
}

export async function getInspectionRunEvidence(
  accessToken: string,
  inspectionRunId: string,
): Promise<InspectionRunEvidenceResponse[]> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/evidence`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<InspectionRunEvidenceResponse[]>(
    response,
    'Failed to load inspection run evidence',
  )
}

export async function uploadInspectionRunEvidence(
  accessToken: string,
  inspectionRunId: string,
  payload: CreateMaintainArrEvidenceRequest,
): Promise<InspectionRunEvidenceResponse> {
  const response = await fetch(`${apiBase}/api/inspections/${inspectionRunId}/evidence`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<InspectionRunEvidenceResponse>(
    response,
    'Failed to upload inspection run evidence',
  )
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

export async function createWorkOrderDraft(
  accessToken: string,
  payload: CreateWorkOrderRequest,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/drafts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to create work order draft')
}

export async function updateWorkOrderDraft(
  accessToken: string,
  workOrderId: string,
  payload: CreateWorkOrderRequest,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/draft`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to update work order draft')
}

export async function validateWorkOrderDraft(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderValidationResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderValidationResponse>(response, 'Failed to validate work order draft')
}

export async function previewWorkOrderDraft(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderPreviewResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderPreviewResponse>(response, 'Failed to preview work order draft')
}

export async function checkDuplicateWorkOrderDraft(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderDuplicateMatchResponse[]> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/duplicates`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderDuplicateMatchResponse[]>(response, 'Failed to check work order duplicates')
}

export async function openWorkOrderDraft(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/open`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to open work order draft')
}

export async function scheduleWorkOrderDraft(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/schedule`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to schedule work order draft')
}

export async function startWorkOrderDraft(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/start`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to start work order draft')
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

export async function updateWorkOrderLaborStatus(
  accessToken: string,
  workOrderId: string,
  laborEntryId: string,
  payload: UpdateWorkOrderLaborEntryStatusRequest,
): Promise<WorkOrderLaborEntryResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/labor/${laborEntryId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderLaborEntryResponse>(response, 'Failed to update work order labor status')
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

export async function getWorkOrderComments(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderCommentResponse[]> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/comments`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderCommentResponse[]>(response, 'Failed to load work order comments')
}

export async function createWorkOrderComment(
  accessToken: string,
  workOrderId: string,
  payload: CreateWorkOrderCommentRequest,
): Promise<WorkOrderCommentResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/comments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<WorkOrderCommentResponse>(response, 'Failed to create work order comment')
}

export async function getWorkOrderTimeline(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderTimelineEventResponse[]> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/timeline`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderTimelineEventResponse[]>(response, 'Failed to load work order timeline')
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

export async function getWorkOrderPartsDemandStatusEvents(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderPartsDemandStatusEventResponse[]> {
  const response = await fetch(
    `${apiBase}/api/work-orders/${workOrderId}/parts-demand/status-events`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<WorkOrderPartsDemandStatusEventResponse[]>(
    response,
    'Failed to load work order parts demand status events',
  )
}

export async function getMaintenancePartsKits(
  accessToken: string,
): Promise<MaintenancePartsKitListResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePartsKitListResponse>(response, 'Failed to load maintenance parts kits')
}

export async function getMaintenancePartsKit(
  accessToken: string,
  partsKitId: string,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(response, 'Failed to load maintenance parts kit')
}

export async function validateMaintenancePartsKit(
  accessToken: string,
  payload: MaintenancePartsKitPreviewRequest,
): Promise<MaintenancePartsKitValidationResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePartsKitValidationResponse>(
    response,
    'Failed to validate maintenance parts kit',
  )
}

export async function previewMaintenancePartsKit(
  accessToken: string,
  payload: MaintenancePartsKitPreviewRequest,
): Promise<MaintenancePartsKitPreviewResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/preview`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePartsKitPreviewResponse>(
    response,
    'Failed to preview maintenance parts kit',
  )
}

export async function createMaintenancePartsKit(
  accessToken: string,
  payload: CreateMaintenancePartsKitRequest,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(response, 'Failed to create maintenance parts kit')
}

export async function updateMaintenancePartsKit(
  accessToken: string,
  partsKitId: string,
  payload: UpdateMaintenancePartsKitRequest,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(response, 'Failed to update maintenance parts kit')
}

export async function updateMaintenancePartsKitStatus(
  accessToken: string,
  partsKitId: string,
  payload: UpdateMaintenancePartsKitStatusRequest,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(response, 'Failed to update maintenance parts kit status')
}

export async function submitMaintenancePartsKitForApproval(
  accessToken: string,
  partsKitId: string,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/submit-approval`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(
    response,
    'Failed to submit maintenance parts kit for approval',
  )
}

export async function activateMaintenancePartsKit(
  accessToken: string,
  partsKitId: string,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/activate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(
    response,
    'Failed to activate maintenance parts kit',
  )
}

export async function retireMaintenancePartsKit(
  accessToken: string,
  partsKitId: string,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/retire`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(
    response,
    'Failed to retire maintenance parts kit',
  )
}

export async function cloneMaintenancePartsKit(
  accessToken: string,
  partsKitId: string,
): Promise<MaintenancePartsKitResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/clone`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePartsKitResponse>(response, 'Failed to clone maintenance parts kit')
}

export async function createMaintenancePartsKitLine(
  accessToken: string,
  partsKitId: string,
  payload: CreateMaintenancePartsKitLineRequest,
): Promise<MaintenancePartsKitLineResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/lines`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePartsKitLineResponse>(response, 'Failed to create maintenance parts kit line')
}

export async function updateMaintenancePartsKitLine(
  accessToken: string,
  partsKitId: string,
  lineId: string,
  payload: UpdateMaintenancePartsKitLineRequest,
): Promise<MaintenancePartsKitLineResponse> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/lines/${lineId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePartsKitLineResponse>(response, 'Failed to update maintenance parts kit line')
}

export async function deleteMaintenancePartsKitLine(
  accessToken: string,
  partsKitId: string,
  lineId: string,
): Promise<void> {
  const response = await fetch(`${apiBase}/api/v1/maintenance-parts-kits/${partsKitId}/lines/${lineId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Failed to delete maintenance parts kit line')
  }
}

export async function getMaintenanceVendorWork(
  accessToken: string,
  workOrderId: string,
): Promise<MaintenanceVendorWorkListResponse> {
  const response = await fetch(`${apiBase}/api/v1/work-orders/${workOrderId}/vendor-work`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceVendorWorkListResponse>(response, 'Failed to load vendor work')
}

export async function upsertMaintenanceVendorWork(
  accessToken: string,
  workOrderId: string,
  payload: UpsertMaintenanceVendorWorkRequest,
): Promise<MaintenanceVendorWorkResponse> {
  const response = await fetch(`${apiBase}/api/v1/work-orders/${workOrderId}/vendor-work`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenanceVendorWorkResponse>(response, 'Failed to save vendor work')
}

export async function issueMaintenanceVendorWorkPortalAccess(
  accessToken: string,
  workOrderId: string,
  vendorWorkId: string,
): Promise<MaintenanceVendorWorkResponse> {
  const response = await fetch(`${apiBase}/api/v1/work-orders/${workOrderId}/vendor-work/${vendorWorkId}/portal-access`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceVendorWorkResponse>(response, 'Failed to issue vendor work portal access')
}

export async function revokeMaintenanceVendorWorkPortalAccess(
  accessToken: string,
  workOrderId: string,
  vendorWorkId: string,
): Promise<MaintenanceVendorWorkResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/work-orders/${workOrderId}/vendor-work/${vendorWorkId}/portal-access/revoke`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<MaintenanceVendorWorkResponse>(response, 'Failed to revoke vendor work portal access')
}

export async function getMaintenanceVendorWorkPortal(
  workOrderId: string,
  accessCode: string,
): Promise<MaintenanceVendorWorkPortalResponse> {
  const query = `?accessCode=${encodeURIComponent(accessCode)}`
  const response = await fetch(`${apiBase}/api/v1/vendor-portal/work-orders/${workOrderId}${query}`)
  return parseJsonResponse<MaintenanceVendorWorkPortalResponse>(response, 'Failed to load vendor work portal')
}

export async function updateMaintenanceVendorWorkPortal(
  workOrderId: string,
  accessCode: string,
  payload: UpdateMaintenanceVendorWorkPortalRequest,
): Promise<MaintenanceVendorWorkPortalResponse> {
  const query = `?accessCode=${encodeURIComponent(accessCode)}`
  const response = await fetch(`${apiBase}/api/v1/vendor-portal/work-orders/${workOrderId}/status${query}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenanceVendorWorkPortalResponse>(response, 'Failed to update vendor work portal status')
}

export async function getWorkOrderSupplyReadiness(
  accessToken: string,
  workOrderId: string,
): Promise<WorkOrderSupplyReadinessResponse> {
  const response = await fetch(`${apiBase}/api/work-orders/${workOrderId}/supply-readiness`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<WorkOrderSupplyReadinessResponse>(
    response,
    'Failed to load work order supply readiness',
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
  const response = await fetch(`${apiBase}/api/v1/readiness?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetReadinessResponse>(response, 'Failed to load asset readiness')
}

export async function getAssetReadinessFleet(
  accessToken: string,
): Promise<AssetReadinessSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/readiness`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetReadinessSummaryResponse[]>(response, 'Failed to load asset readiness fleet')
}

export async function getAssetReadinessHistory(
  accessToken: string,
  assetId: string,
  limit = 15,
): Promise<AssetReadinessHistoryResponse> {
  const search = new URLSearchParams({
    assetId,
    limit: String(limit),
  })
  const response = await fetch(`${apiBase}/api/v1/readiness/history?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetReadinessHistoryResponse>(response, 'Failed to load asset readiness history')
}

export type AssetReservationAction =
  | 'approve'
  | 'reserve'
  | 'checkout'
  | 'start-use'
  | 'return'
  | 'inspect'
  | 'close'
  | 'cancel'
  | 'no-show'

export async function getAssetReservations(
  accessToken: string,
  filters?: {
    assetId?: string
    status?: string
    activeOnly?: boolean
    limit?: number
  },
): Promise<AssetReservationResponse[]> {
  const search = new URLSearchParams()
  if (filters?.assetId && filters.assetId.trim()) {
    search.set('assetId', filters.assetId.trim())
  }
  if (filters?.status && filters.status.trim()) {
    search.set('status', filters.status.trim())
  }
  if (filters?.activeOnly != null) {
    search.set('activeOnly', String(filters.activeOnly))
  }
  if (filters?.limit != null) {
    search.set('limit', String(filters.limit))
  }
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reservations${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetReservationResponse[]>(response, 'Failed to load asset reservations')
}

export async function getAssetReservation(
  accessToken: string,
  reservationId: string,
): Promise<AssetReservationResponse> {
  const response = await fetch(`${apiBase}/api/reservations/${reservationId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetReservationResponse>(response, 'Failed to load asset reservation')
}

export async function createAssetReservation(
  accessToken: string,
  assetId: string,
  payload: CreateAssetReservationRequest,
): Promise<AssetReservationResponse> {
  const search = new URLSearchParams({ assetId })
  const response = await fetch(`${apiBase}/api/reservations?${search.toString()}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetReservationResponse>(response, 'Failed to create asset reservation')
}

export async function updateAssetReservation(
  accessToken: string,
  reservationId: string,
  action: AssetReservationAction,
  payload: ReservationActionRequest = { notes: null, chargeNotes: null, meterReading: null, occurredAt: null },
): Promise<AssetReservationResponse> {
  const response = await fetch(`${apiBase}/api/reservations/${reservationId}/${action}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetReservationResponse>(
    response,
    `Failed to ${action.replace(/-/g, ' ')} asset reservation`,
  )
}

export async function getAssetTelematicsIngestion(
  accessToken: string,
  assetId: string,
  limit = 8,
): Promise<AssetTelematicsIngestionResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/telematics-ingestion?${search.toString()}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetTelematicsIngestionResponse>(
    response,
    'Failed to load asset telematics ingestion history',
  )
}

export async function getAssetExternalIntelligenceOverview(
  accessToken: string,
  assetId: string,
): Promise<AssetExternalIntelligenceOverviewResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/external-intelligence`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetExternalIntelligenceOverviewResponse>(
    response,
    'Failed to load asset external intelligence',
  )
}

export async function getAssetExternalIntelligenceIdentifiers(
  accessToken: string,
  assetId: string,
): Promise<ExternalAssetIdentifierResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/external-intelligence/identifiers`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalAssetIdentifierResponse[]>(
    response,
    'Failed to load asset external intelligence identifiers',
  )
}

export async function getAssetExternalIntelligenceSnapshots(
  accessToken: string,
  assetId: string,
  snapshotType?: string | null,
): Promise<AssetEnrichmentSnapshotResponse[]> {
  const search = new URLSearchParams()
  if (snapshotType && snapshotType.trim()) {
    search.set('snapshotType', snapshotType.trim())
  }
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/external-intelligence/snapshots${query}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AssetEnrichmentSnapshotResponse[]>(
    response,
    'Failed to load asset external intelligence snapshots',
  )
}

export async function getAssetExternalIntelligenceSuggestions(
  accessToken: string,
  assetId: string,
  status?: string | null,
): Promise<AssetEnrichmentSuggestionResponse[]> {
  const search = new URLSearchParams()
  if (status && status.trim()) {
    search.set('status', status.trim())
  }
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/external-intelligence/suggestions${query}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AssetEnrichmentSuggestionResponse[]>(
    response,
    'Failed to load asset external intelligence suggestions',
  )
}

export async function getAssetExternalIntelligenceRecalls(
  accessToken: string,
  assetId: string,
  status?: string | null,
): Promise<AssetRecallSnapshotResponse[]> {
  const search = new URLSearchParams()
  if (status && status.trim()) {
    search.set('status', status.trim())
  }
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/external-intelligence/recalls${query}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AssetRecallSnapshotResponse[]>(
    response,
    'Failed to load asset external intelligence recalls',
  )
}

export async function getAssetExternalIntelligenceComplaints(
  accessToken: string,
  assetId: string,
): Promise<AssetComplaintSignalResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/external-intelligence/complaints`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetComplaintSignalResponse[]>(
    response,
    'Failed to load asset external intelligence complaints',
  )
}

export async function refreshAssetExternalIntelligence(
  accessToken: string,
  assetId: string,
): Promise<AssetExternalIntelligenceOverviewResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/external-intelligence/refresh`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetExternalIntelligenceOverviewResponse>(
    response,
    'Failed to refresh asset external intelligence',
  )
}

export async function acceptAssetExternalIntelligenceSuggestion(
  accessToken: string,
  assetId: string,
  suggestionId: string,
): Promise<AssetEnrichmentSuggestionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/external-intelligence/suggestions/${suggestionId}/accept`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AssetEnrichmentSuggestionResponse>(
    response,
    'Failed to accept external intelligence suggestion',
  )
}

export async function rejectAssetExternalIntelligenceSuggestion(
  accessToken: string,
  assetId: string,
  suggestionId: string,
): Promise<AssetEnrichmentSuggestionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/external-intelligence/suggestions/${suggestionId}/reject`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<AssetEnrichmentSuggestionResponse>(
    response,
    'Failed to reject external intelligence suggestion',
  )
}

export async function createAssetRecallWorkOrder(
  accessToken: string,
  assetId: string,
  recallId: string,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/external-intelligence/recalls/${recallId}/work-order`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to create recall work order')
}

export async function getRecallProviders(
  accessToken: string,
): Promise<RecallProviderSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/recalls/providers`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecallProviderSummaryResponse[]>(
    response,
    'Failed to load recall providers',
  )
}

export async function getRecallProviderHealth(
  accessToken: string,
): Promise<RecallProviderHealthResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/recalls/providers/health`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecallProviderHealthResponse[]>(
    response,
    'Failed to load recall provider health',
  )
}

export async function getRecallDashboard(
  accessToken: string,
): Promise<RecallDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/recalls/dashboard`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecallDashboardResponse>(response, 'Failed to load recall dashboard')
}

export async function listRecallCampaigns(
  accessToken: string,
  filters?: {
    sourceProvider?: string | null
    status?: string | null
    campaignNumber?: string | null
    component?: string | null
    limit?: number | null
    offset?: number | null
  },
): Promise<RecallCampaignResponse[]> {
  const search = new URLSearchParams()
  if (filters?.sourceProvider && filters.sourceProvider.trim()) {
    search.set('sourceProvider', filters.sourceProvider.trim())
  }
  if (filters?.status && filters.status.trim()) {
    search.set('status', filters.status.trim())
  }
  if (filters?.campaignNumber && filters.campaignNumber.trim()) {
    search.set('campaignNumber', filters.campaignNumber.trim())
  }
  if (filters?.component && filters.component.trim()) {
    search.set('component', filters.component.trim())
  }
  if (filters?.limit != null) {
    search.set('limit', String(filters.limit))
  }
  if (filters?.offset != null) {
    search.set('offset', String(filters.offset))
  }
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/recalls/campaigns${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecallCampaignResponse[]>(response, 'Failed to load recall campaigns')
}

export async function getRecallCampaign(
  accessToken: string,
  campaignId: string,
): Promise<RecallCampaignResponse> {
  const response = await fetch(`${apiBase}/api/v1/recalls/campaigns/${campaignId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<RecallCampaignResponse>(response, 'Failed to load recall campaign')
}

export async function searchRecallCampaignsByVehicle(
  accessToken: string,
  request: RecallVehicleSearchRequest,
): Promise<RecallCampaignResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/recalls/campaigns/search/vehicle`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecallCampaignResponse[]>(
    response,
    'Failed to search recalls by vehicle',
  )
}

export async function searchRecallCampaignByNumber(
  accessToken: string,
  request: RecallCampaignSearchRequest,
): Promise<RecallCampaignResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/recalls/campaigns/search/campaign-number`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecallCampaignResponse[]>(
    response,
    'Failed to search recall by campaign number',
  )
}

export async function createRecallCampaign(
  accessToken: string,
  request: CreateRecallCampaignRequest,
): Promise<RecallCampaignResponse> {
  const response = await fetch(`${apiBase}/api/v1/recalls/campaigns`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecallCampaignResponse>(response, 'Failed to create recall campaign')
}

export async function updateRecallCampaign(
  accessToken: string,
  campaignId: string,
  request: UpdateRecallCampaignRequest,
): Promise<RecallCampaignResponse> {
  const response = await fetch(`${apiBase}/api/v1/recalls/campaigns/${campaignId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<RecallCampaignResponse>(response, 'Failed to update recall campaign')
}

export async function listAssetRecallCases(
  accessToken: string,
  assetId: string,
  filters?: {
    status?: string | null
    sourceProvider?: string | null
    campaignNumber?: string | null
    component?: string | null
    limit?: number | null
    offset?: number | null
  },
): Promise<AssetRecallCaseResponse[]> {
  const search = new URLSearchParams()
  if (filters?.status && filters.status.trim()) {
    search.set('status', filters.status.trim())
  }
  if (filters?.sourceProvider && filters.sourceProvider.trim()) {
    search.set('sourceProvider', filters.sourceProvider.trim())
  }
  if (filters?.campaignNumber && filters.campaignNumber.trim()) {
    search.set('campaignNumber', filters.campaignNumber.trim())
  }
  if (filters?.component && filters.component.trim()) {
    search.set('component', filters.component.trim())
  }
  if (filters?.limit != null) {
    search.set('limit', String(filters.limit))
  }
  if (filters?.offset != null) {
    search.set('offset', String(filters.offset))
  }
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/recalls${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetRecallCaseResponse[]>(
    response,
    'Failed to load asset recalls',
  )
}

export async function refreshAssetRecallCases(
  accessToken: string,
  assetId: string,
): Promise<AssetRecallCaseResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/recalls/refresh`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetRecallCaseResponse[]>(
    response,
    'Failed to refresh asset recalls',
  )
}

export async function verifyAssetRecall(
  accessToken: string,
  assetId: string,
  caseId: string,
  request: VerifyAssetRecallRequest,
): Promise<AssetRecallCaseResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/recalls/cases/${caseId}/verify`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<AssetRecallCaseResponse>(response, 'Failed to verify asset recall')
}

export async function dismissAssetRecall(
  accessToken: string,
  assetId: string,
  caseId: string,
  request: DismissAssetRecallRequest,
): Promise<AssetRecallCaseResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/recalls/cases/${caseId}/dismiss`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<AssetRecallCaseResponse>(response, 'Failed to dismiss asset recall')
}

export async function createAssetRecallHold(
  accessToken: string,
  assetId: string,
  caseId: string,
): Promise<AssetRecallCaseResponse> {
  const response = await fetch(`${apiBase}/api/v1/assets/${assetId}/recalls/cases/${caseId}/hold`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetRecallCaseResponse>(response, 'Failed to create asset recall hold')
}

export async function releaseAssetRecallHold(
  accessToken: string,
  assetId: string,
  caseId: string,
  request: ReleaseRecallHoldRequest,
): Promise<AssetRecallCaseResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/recalls/cases/${caseId}/hold/release`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<AssetRecallCaseResponse>(response, 'Failed to release asset recall hold')
}

export async function createAssetRecallCaseWorkOrder(
  accessToken: string,
  assetId: string,
  caseId: string,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/recalls/cases/${caseId}/work-order`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<WorkOrderDetailResponse>(response, 'Failed to create asset recall work order')
}

export async function createAssetRecallCaseInspectionItem(
  accessToken: string,
  assetId: string,
  caseId: string,
): Promise<WorkOrderDetailResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/assets/${assetId}/recalls/cases/${caseId}/inspection-item`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<WorkOrderDetailResponse>(
    response,
    'Failed to create asset recall inspection item',
  )
}

export async function getExternalIntelligenceProviders(
  accessToken: string,
): Promise<ExternalIntelligenceProviderSummaryResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/external-intelligence/providers`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalIntelligenceProviderSummaryResponse[]>(
    response,
    'Failed to load external intelligence providers',
  )
}

export async function getExternalIntelligenceProviderHealth(
  accessToken: string,
): Promise<ExternalProviderHealthResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/external-intelligence/providers/health`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExternalProviderHealthResponse[]>(
    response,
    'Failed to load external intelligence provider health',
  )
}

export async function decodeVin(
  accessToken: string,
  payload: ExternalVinDecodeRequest,
  assetId?: string | null,
  persist = false,
): Promise<ExternalVinDecodeResponse> {
  const search = new URLSearchParams()
  if (assetId && assetId.trim()) {
    search.set('assetId', assetId.trim())
  }
  if (persist) {
    search.set('persist', 'true')
  }
  const query = search.toString() ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/external-intelligence/vin/decode${query}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ExternalVinDecodeResponse>(response, 'Failed to decode VIN')
}

export async function decodeVinBatch(
  accessToken: string,
  payload: ExternalVinDecodeBatchRequest,
): Promise<ExternalVinDecodeBatchItemResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/external-intelligence/vin/batch-decode`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ExternalVinDecodeBatchItemResponse[]>(
    response,
    'Failed to decode VIN batch',
  )
}

export async function getExternalIntelligenceMakes(
  accessToken: string,
): Promise<ReferenceOptionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/external-intelligence/references/makes`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ReferenceOptionResponse[]>(
    response,
    'Failed to load external intelligence makes',
  )
}

export async function getExternalIntelligenceManufacturers(
  accessToken: string,
  manufacturerType?: string | null,
  page = 1,
): Promise<ReferenceOptionResponse[]> {
  const search = new URLSearchParams({ page: String(page) })
  if (manufacturerType && manufacturerType.trim()) {
    search.set('manufacturerType', manufacturerType.trim())
  }
  const response = await fetch(
    `${apiBase}/api/v1/external-intelligence/references/manufacturers?${search.toString()}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ReferenceOptionResponse[]>(
    response,
    'Failed to load external intelligence manufacturers',
  )
}

export async function getExternalIntelligenceModels(
  accessToken: string,
  make: string,
  modelYear?: number | null,
  vehicleType?: string | null,
): Promise<ReferenceOptionResponse[]> {
  const search = new URLSearchParams({ make })
  if (modelYear != null) {
    search.set('modelYear', String(modelYear))
  }
  if (vehicleType && vehicleType.trim()) {
    search.set('vehicleType', vehicleType.trim())
  }
  const response = await fetch(
    `${apiBase}/api/v1/external-intelligence/references/models?${search.toString()}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ReferenceOptionResponse[]>(
    response,
    'Failed to load external intelligence models',
  )
}

export async function getExternalIntelligenceEquipmentPlantCodes(
  accessToken: string,
  year: number,
  equipmentType?: string | null,
  reportType?: string | null,
): Promise<ReferenceOptionResponse[]> {
  const search = new URLSearchParams({ year: String(year) })
  if (equipmentType && equipmentType.trim()) {
    search.set('equipmentType', equipmentType.trim())
  }
  if (reportType && reportType.trim()) {
    search.set('reportType', reportType.trim())
  }
  const response = await fetch(
    `${apiBase}/api/v1/external-intelligence/references/equipment-plant-codes?${search.toString()}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ReferenceOptionResponse[]>(
    response,
    'Failed to load external intelligence equipment plant codes',
  )
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

export async function getPmDueScanSettings(
  accessToken: string,
): Promise<PmDueScanSettingsResponse> {
  const response = await fetch(`${apiBase}/api/pm-due-scan-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmDueScanSettingsResponse>(response, 'Failed to load PM due scan settings')
}

export async function upsertPmDueScanSettings(
  accessToken: string,
  payload: UpsertPmDueScanSettingsRequest,
): Promise<PmDueScanSettingsResponse> {
  const response = await fetch(`${apiBase}/api/pm-due-scan-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PmDueScanSettingsResponse>(response, 'Failed to save PM due scan settings')
}

export async function getPendingPmDueScan(
  accessToken: string,
): Promise<PendingPmDueResponse> {
  const response = await fetch(`${apiBase}/api/pm-due-scan-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingPmDueResponse>(response, 'Failed to load pending PM due scan items')
}

export async function getPmDueScanRuns(
  accessToken: string,
  limit = 5,
): Promise<PmDueScanRunsResponse> {
  const response = await fetch(`${apiBase}/api/pm-due-scan-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PmDueScanRunsResponse>(response, 'Failed to load PM due scan runs')
}

export async function triggerPmDueScan(accessToken: string): Promise<TriggerPmDueScanResponse> {
  const response = await fetch(`${apiBase}/api/pm-due-scan-settings/trigger`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<TriggerPmDueScanResponse>(response, 'Failed to trigger PM due scan')
}

function buildAuditPackageQuery(
  options?: import('./types').AuditPackageScope & { format?: string; page?: number; pageSize?: number },
): string {
  const params = new URLSearchParams()
  if (options?.format) {
    params.set('format', options.format)
  }
  if (options?.from) {
    params.set('from', `${options.from}T00:00:00.000Z`)
  }
  if (options?.to) {
    params.set('to', `${options.to}T23:59:59.999Z`)
  }
  if (options?.action) {
    params.set('action', options.action)
  }
  if (options?.result) {
    params.set('result', options.result)
  }
  if (options?.targetType) {
    params.set('targetType', options.targetType)
  }
  if (options?.actorUserId) {
    params.set('actorUserId', options.actorUserId)
  }
  if (options?.page) {
    params.set('page', String(options.page))
  }
  if (options?.pageSize) {
    params.set('pageSize', String(options.pageSize))
  }
  const query = params.toString()
  return query ? `?${query}` : ''
}

function auditPackageJobBody(scope: import('./types').AuditPackageScope & { format: string }) {
  return {
    format: scope.format,
    from: scope.from ? `${scope.from}T00:00:00.000Z` : undefined,
    to: scope.to ? `${scope.to}T23:59:59.999Z` : undefined,
    action: scope.action,
    result: scope.result,
    targetType: scope.targetType,
    actorUserId: scope.actorUserId,
  }
}

export async function getAuditPackageManifest(
  accessToken: string,
): Promise<AuditPackageManifestResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AuditPackageManifestResponse>(response, 'Failed to load audit package manifest')
}

export async function getAuditPackageFilterOptions(
  accessToken: string,
): Promise<import('./types').AuditPackageFilterOptions> {
  const response = await fetch(`${apiBase}/api/audit-packages/filter-options`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<import('./types').AuditPackageFilterOptions>(
    response,
    'Failed to load audit filter options',
  )
}

export async function getAuditPackageExportSummary(
  accessToken: string,
  scope?: import('./types').AuditPackageScope,
): Promise<import('./types').AuditPackageExportSummary> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/summary${buildAuditPackageQuery(scope)}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<import('./types').AuditPackageExportSummary>(
    response,
    'Failed to load audit export summary',
  )
}

export async function getAuditPackageTimeline(
  accessToken: string,
  scope?: import('./types').AuditPackageScope & { page?: number; pageSize?: number },
): Promise<import('./types').PagedAuditTimeline> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/timeline${buildAuditPackageQuery({ ...scope, page: scope?.page ?? 1, pageSize: scope?.pageSize ?? 15 })}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<import('./types').PagedAuditTimeline>(
    response,
    'Failed to load audit timeline',
  )
}

export async function exportAuditPackageZip(
  accessToken: string,
  options?: import('./types').AuditPackageScope,
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery(options)}`,
    {
      headers: { Authorization: `Bearer ${accessToken}` },
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Audit package export failed')
  }
  return response.blob()
}

export async function exportAuditPackageCsv(
  accessToken: string,
  options?: import('./types').AuditPackageScope,
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/audit-packages/export${buildAuditPackageQuery({ ...options, format: 'csv' })}`,
    { headers: { Authorization: `Bearer ${accessToken}` } },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Audit CSV export failed')
  }
  return response.blob()
}

export async function exportAuditPackageJson(
  accessToken: string,
  options?: import('./types').AuditPackageScope,
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
  options: import('./types').AuditPackageScope & { format: 'zip' | 'json' },
): Promise<AuditPackageGenerationJobResponse> {
  const response = await fetch(`${apiBase}/api/audit-packages/jobs`, {
    method: 'POST',
    headers: {
      ...authHeaders(accessToken),
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(auditPackageJobBody(options)),
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
    throw await toApiError(response, 'Audit package download failed')
  }
  return response.blob()
}

export async function getMaintenanceReportSummary(
  accessToken: string,
  options?: { lifecycleStatus?: string },
): Promise<MaintenanceReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.lifecycleStatus) {
    params.set('lifecycleStatus', options.lifecycleStatus)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/reports/maintenance/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceReportSummaryResponse>(
    response,
    'Failed to load maintenance report summary',
  )
}

export async function getMaintenanceReportAssetDetail(
  accessToken: string,
  assetId: string,
): Promise<MaintenanceReportAssetDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/reports/maintenance/assets/${assetId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceReportAssetDetailResponse>(
    response,
    'Failed to load maintenance report asset detail',
  )
}

export async function getMaintenanceReportWorkOrderDetail(
  accessToken: string,
  workOrderId: string,
): Promise<MaintenanceReportWorkOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/reports/maintenance/work-orders/${workOrderId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenanceReportWorkOrderDetailResponse>(
    response,
    'Failed to load maintenance report work order detail',
  )
}

export async function exportMaintenanceReportSummaryCsv(
  accessToken: string,
  options?: { lifecycleStatus?: string },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.lifecycleStatus) {
    params.set('lifecycleStatus', options.lifecycleStatus)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/reports/maintenance/summary/export${query}`, {
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Maintenance report export failed')
  }
  return response.blob()
}

export async function getExecutiveReportSummary(
  accessToken: string,
): Promise<ExecutiveReportSummaryResponse> {
  const response = await fetch(`${apiBase}/api/v1/reports/executive/summary`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ExecutiveReportSummaryResponse>(
    response,
    'Failed to load executive report summary',
  )
}

export async function exportExecutiveReportSummaryCsv(accessToken: string): Promise<Blob> {
  const response = await fetch(`${apiBase}/api/v1/reports/executive/summary/export`, {
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Executive report export failed')
  }
  return response.blob()
}

export async function getComplianceReportSummary(
  accessToken: string,
  options?: { attentionOnly?: boolean; siteRef?: string },
): Promise<ComplianceReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.attentionOnly) {
    params.set('attentionOnly', 'true')
  }
  if (options?.siteRef) {
    params.set('siteRef', options.siteRef)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/reports/compliance/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ComplianceReportSummaryResponse>(
    response,
    'Failed to load compliance report summary',
  )
}

export async function exportComplianceReportSummaryCsv(
  accessToken: string,
  options?: { attentionOnly?: boolean; siteRef?: string },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.attentionOnly) {
    params.set('attentionOnly', 'true')
  }
  if (options?.siteRef) {
    params.set('siteRef', options.siteRef)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/reports/compliance/summary/export${query}`, {
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, 'Compliance report export failed')
  }
  return response.blob()
}

export async function validateAssetImport(
  accessToken: string,
  request: AssetBulkImportRequest,
): Promise<AssetBulkImportResponse> {
  const response = await fetch(`${apiBase}/api/v1/imports/assets/validate`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<AssetBulkImportResponse>(response, 'Failed to validate asset import')
}

export async function commitAssetImport(
  accessToken: string,
  request: AssetBulkImportRequest,
): Promise<AssetBulkImportResponse> {
  const response = await fetch(`${apiBase}/api/v1/imports/assets/commit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<AssetBulkImportResponse>(response, 'Failed to commit asset import')
}

export async function listImportManifests(
  accessToken: string,
): Promise<ProductImportManifest[]> {
  const response = await fetch(`${apiBase}/api/v1/imports/manifests`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProductImportManifest[]>(response, 'Failed to load import manifests')
}

export async function listImportHistory(
  accessToken: string,
  limit = 25,
): Promise<ProductImportHistoryEntry[]> {
  const response = await fetch(`${apiBase}/api/v1/imports/history?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  const payload = await parseJsonResponse<{ items: ProductImportHistoryEntry[] }>(
    response,
    'Failed to load import history',
  )
  return payload.items
}

export async function downloadImportTemplate(
  accessToken: string,
  importTypeKey: string,
): Promise<Blob> {
  const response = await fetch(
    `${apiBase}/api/v1/imports/manifests/${encodeURIComponent(importTypeKey)}/template`,
    {
      headers: authHeaders(accessToken),
    },
  )
  if (!response.ok) {
    throw await toApiError(response, 'Failed to download import template')
  }
  return response.blob()
}

export async function getEntityExportManifest(
  accessToken: string,
): Promise<EntityExportManifestResponse> {
  const response = await fetch(`${apiBase}/api/exports/manifest`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<EntityExportManifestResponse>(
    response,
    'Failed to load export manifest',
  )
}

async function downloadExportBlob(
  accessToken: string,
  path: string,
  errorLabel: string,
): Promise<Blob> {
  const response = await fetch(`${apiBase}${path}`, {
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    throw await toApiError(response, errorLabel)
  }
  return response.blob()
}

export async function exportAssetsCsv(
  accessToken: string,
  options?: { lifecycleStatus?: string },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.lifecycleStatus) {
    params.set('lifecycleStatus', options.lifecycleStatus)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(accessToken, `/api/exports/assets${query}`, 'Asset export failed')
}

export async function exportWorkOrdersCsv(
  accessToken: string,
  options?: { status?: string; assetId?: string },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.assetId) {
    params.set('assetId', options.assetId)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(accessToken, `/api/exports/work-orders${query}`, 'Work order export failed')
}

export async function exportInspectionRunsCsv(
  accessToken: string,
  options?: { status?: string; assetId?: string },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.assetId) {
    params.set('assetId', options.assetId)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(
    accessToken,
    `/api/exports/inspection-runs${query}`,
    'Inspection run export failed',
  )
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

export async function getMaintainArrTenantSettings(
  accessToken: string,
): Promise<MaintainArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/settings/tenant/maintainarr`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrTenantSettingsResponse>(
    response,
    'Failed to load MaintainArr tenant settings',
  )
}

export async function updateMaintainArrTenantSettings(
  accessToken: string,
  payload: UpsertMaintainArrTenantSettingsRequest,
): Promise<MaintainArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/settings/tenant/maintainarr`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintainArrTenantSettingsResponse>(
    response,
    'Failed to save MaintainArr tenant settings',
  )
}

export async function resetMaintainArrTenantSettings(
  accessToken: string,
  payload: ResetMaintainArrTenantSettingsRequest = {},
): Promise<MaintainArrTenantSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/settings/tenant/maintainarr/reset-to-defaults`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintainArrTenantSettingsResponse>(
    response,
    'Failed to reset MaintainArr tenant settings',
  )
}

export async function getMaintainArrTenantSettingsAudit(
  accessToken: string,
  limit = 25,
): Promise<MaintainArrTenantSettingsAuditResponse> {
  const response = await fetch(`${apiBase}/api/v1/settings/tenant/maintainarr/audit?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrTenantSettingsAuditResponse>(
    response,
    'Failed to load MaintainArr settings history',
  )
}

export async function getDowntimeTrackingSettings(
  accessToken: string,
): Promise<DowntimeTrackingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/downtime-tracking-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DowntimeTrackingSettingsResponse>(
    response,
    'Failed to load downtime tracking settings',
  )
}

export async function upsertDowntimeTrackingSettings(
  accessToken: string,
  payload: UpsertDowntimeTrackingSettingsRequest,
): Promise<DowntimeTrackingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/downtime-tracking-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DowntimeTrackingSettingsResponse>(
    response,
    'Failed to save downtime tracking settings',
  )
}

export async function getPendingDowntimeSync(
  accessToken: string,
): Promise<PendingAssetDowntimeSyncResponse> {
  const response = await fetch(`${apiBase}/api/downtime-tracking-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingAssetDowntimeSyncResponse>(
    response,
    'Failed to load pending downtime sync',
  )
}

export async function getDowntimeSyncRuns(
  accessToken: string,
  limit = 5,
): Promise<AssetDowntimeSyncRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/downtime-tracking-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetDowntimeSyncRunsResponse>(
    response,
    'Failed to load downtime sync runs',
  )
}

export async function listDowntimeEvents(
  accessToken: string,
  options?: { assetId?: string; activeOnly?: boolean; limit?: number },
): Promise<AssetDowntimeEventResponse[]> {
  const params = new URLSearchParams()
  if (options?.assetId) {
    params.set('assetId', options.assetId)
  }
  if (options?.activeOnly) {
    params.set('activeOnly', 'true')
  }
  if (options?.limit) {
    params.set('limit', String(options.limit))
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/downtime/events${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetDowntimeEventResponse[]>(response, 'Failed to load downtime events')
}

export async function createManualDowntimeEvent(
  accessToken: string,
  payload: CreateManualDowntimeEventRequest,
): Promise<AssetDowntimeEventResponse> {
  const response = await fetch(`${apiBase}/api/downtime/events`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetDowntimeEventResponse>(
    response,
    'Failed to create downtime event',
  )
}

export async function updateDowntimeEventReason(
  accessToken: string,
  eventId: string,
  payload: UpdateDowntimeEventReasonRequest,
): Promise<AssetDowntimeEventResponse> {
  const response = await fetch(`${apiBase}/api/downtime/events/${eventId}/reason`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetDowntimeEventResponse>(
    response,
    'Failed to update downtime event reason',
  )
}

export async function closeDowntimeEvent(
  accessToken: string,
  eventId: string,
  payload: CloseDowntimeEventRequest = {},
): Promise<AssetDowntimeEventResponse> {
  const response = await fetch(`${apiBase}/api/downtime/events/${eventId}/close`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AssetDowntimeEventResponse>(
    response,
    'Failed to close downtime event',
  )
}

export async function getFleetAvailability(
  accessToken: string,
): Promise<FleetAvailabilityResponse> {
  const response = await fetch(`${apiBase}/api/downtime/availability/fleet`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<FleetAvailabilityResponse>(
    response,
    'Failed to load fleet availability',
  )
}

export async function getAssetAvailability(
  accessToken: string,
  assetId: string,
): Promise<AssetAvailabilityResponse> {
  const response = await fetch(`${apiBase}/api/downtime/availability/assets/${assetId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AssetAvailabilityResponse>(
    response,
    'Failed to load asset availability',
  )
}

export async function getMaintenancePlatformEventSettings(
  accessToken: string,
): Promise<MaintenancePlatformEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/platform-event-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePlatformEventSettingsResponse>(
    response,
    'Failed to load platform event settings',
  )
}

export async function upsertMaintenancePlatformEventSettings(
  accessToken: string,
  payload: UpsertMaintenancePlatformEventSettingsRequest,
): Promise<MaintenancePlatformEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/platform-event-settings`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<MaintenancePlatformEventSettingsResponse>(
    response,
    'Failed to save platform event settings',
  )
}

export async function getMaintenancePlatformOutboxEvents(
  accessToken: string,
  limit = 10,
): Promise<MaintenancePlatformOutboxEventsResponse> {
  const response = await fetch(`${apiBase}/api/platform-event-settings/outbox?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePlatformOutboxEventsResponse>(
    response,
    'Failed to load platform outbox events',
  )
}

export async function getMaintenancePlatformEventProcessingRuns(
  accessToken: string,
  limit = 5,
): Promise<MaintenancePlatformEventProcessingRunsResponse> {
  const response = await fetch(`${apiBase}/api/platform-event-settings/runs?limit=${limit}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintenancePlatformEventProcessingRunsResponse>(
    response,
    'Failed to load platform event processing runs',
  )
}

export async function getMaintenanceParts(
  accessToken: string,
  options?: { search?: string; status?: string },
): Promise<import('./types').MaintenancePartResponse[]> {
  const params = new URLSearchParams()
  if (options?.search) {
    params.set('search', options.search)
  }
  if (options?.status) {
    params.set('status', options.status)
  }
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/parts${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<import('./types').MaintenancePartResponse[]>(
    response,
    'Failed to load maintenance part profiles',
  )
}

export async function getMaintenancePart(
  accessToken: string,
  partId: string,
): Promise<import('./types').MaintenancePartResponse> {
  const response = await fetch(`${apiBase}/api/v1/parts/${partId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<import('./types').MaintenancePartResponse>(
    response,
    'Failed to load maintenance part profile',
  )
}

export async function createMaintenancePart(
  accessToken: string,
  payload: import('./types').CreateMaintenancePartRequest,
): Promise<import('./types').MaintenancePartResponse> {
  const response = await fetch(`${apiBase}/api/v1/parts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<import('./types').MaintenancePartResponse>(
    response,
    'Failed to create maintenance part profile',
  )
}

export async function updateMaintenancePart(
  accessToken: string,
  partId: string,
  payload: import('./types').UpdateMaintenancePartRequest,
): Promise<import('./types').MaintenancePartResponse> {
  const response = await fetch(`${apiBase}/api/v1/parts/${partId}`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<import('./types').MaintenancePartResponse>(
    response,
    'Failed to update maintenance part profile',
  )
}

export async function archiveMaintenancePart(
  accessToken: string,
  partId: string,
): Promise<import('./types').MaintenancePartResponse> {
  const response = await fetch(`${apiBase}/api/v1/parts/${partId}`, {
    method: 'DELETE',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<import('./types').MaintenancePartResponse>(
    response,
    'Failed to archive maintenance part profile',
  )
}

export async function getSchedulingUnscheduled(
  accessToken: string,
): Promise<SchedulingBoardResponse> {
  const response = await fetch(`${apiBase}/api/v1/scheduling/unscheduled`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SchedulingBoardResponse>(response, 'Failed to load unscheduled work')
}

export async function getSchedulingScheduled(
  accessToken: string,
): Promise<SchedulingBoardResponse> {
  const response = await fetch(`${apiBase}/api/v1/scheduling/scheduled`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SchedulingBoardResponse>(response, 'Failed to load scheduled work')
}

export async function getSchedulingResources(
  accessToken: string,
): Promise<SchedulingResourceLaneResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/scheduling/resources`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SchedulingResourceLaneResponse[]>(response, 'Failed to load scheduling resources')
}

async function postSchedulingMutation(
  accessToken: string,
  path: string,
  payload: SchedulingRequest,
  fallbackMessage: string,
): Promise<SchedulingMutationResponse> {
  const response = await fetch(`${apiBase}${path}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<SchedulingMutationResponse>(response, fallbackMessage)
}

export async function scheduleWork(
  accessToken: string,
  payload: SchedulingRequest,
): Promise<SchedulingMutationResponse> {
  return postSchedulingMutation(
    accessToken,
    '/api/v1/scheduling/schedule',
    payload,
    'Failed to schedule work',
  )
}

export async function rescheduleWork(
  accessToken: string,
  payload: SchedulingRequest,
): Promise<SchedulingMutationResponse> {
  return postSchedulingMutation(
    accessToken,
    '/api/v1/scheduling/reschedule',
    payload,
    'Failed to reschedule work',
  )
}

export async function unscheduleWork(
  accessToken: string,
  payload: SchedulingRequest,
): Promise<SchedulingMutationResponse> {
  return postSchedulingMutation(
    accessToken,
    '/api/v1/scheduling/unschedule',
    payload,
    'Failed to unschedule work',
  )
}

export async function cancelScheduledWork(
  accessToken: string,
  payload: SchedulingRequest,
): Promise<SchedulingMutationResponse> {
  return postSchedulingMutation(
    accessToken,
    '/api/v1/scheduling/cancel',
    payload,
    'Failed to cancel scheduled work',
  )
}

export async function completeScheduledWork(
  accessToken: string,
  payload: SchedulingRequest,
): Promise<SchedulingMutationResponse> {
  return postSchedulingMutation(
    accessToken,
    '/api/v1/scheduling/complete',
    payload,
    'Failed to complete scheduled work',
  )
}
