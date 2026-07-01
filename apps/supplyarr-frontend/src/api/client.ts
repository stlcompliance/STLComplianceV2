import type {
  CreatePartCatalogRequest,
  CreatePartRequest,
  CreatePartSourceRequest,
  CreatePartSupplierLinkRequest,
  CreateSupplierContactRequest,
  CreateSupplierRequest,
  SupplierResponse,
  SupplierDirectoryMetadataResponse,
  UpdateSupplierApprovalStatusRequest,
  UpdateSupplierRequest,
  UpdateSupplierStatusRequest,
  HandoffSessionResponse,
  PartCatalogResponse,
  PartResponse,
  PartSourceResponse,
  SubstitutionItemResponse,
  OutboundShipmentResponse,
  CreateOutboundShipmentRequest,
  PartSupplierLinkResponse,
  SupplierCatalogApiSyncRequest,
  SupplierCatalogApiSyncResponse,
  ContractsCsvImportRequest,
  ContractsCsvImportResponse,
  SupplyContractResponse,
  SupplyArrMeResponse,
  SupplyArrSessionBootstrapResponse,
  CreatePurchaseOrderFromPurchaseRequestRequest,
  CreatePurchaseRequestRequest,
  BackorderResponse,
  CancelBackorderRequest,
  CancelPurchaseOrderRequest,
  CancelSupplierReturnRequest,
  CreateBackorderFromPurchaseOrderLineRequest,
  CreateSupplierReturnFromPurchaseOrderLineRequest,
  CreateSupplierReturnFromStockRequest,
  CreateSupplierWarrantyClaimRequest,
  SupplierReturnResponse,
  WarrantyClaimResponse,
  UpdateWarrantyClaimRequest,
  SubmitWarrantyClaimRequest,
  RecordWarrantyClaimSupplierResponseRequest,
  CloseWarrantyClaimRequest,
  DenyWarrantyClaimRequest,
  CancelWarrantyClaimRequest,
  PurchaseOrderResponse,
  PurchaseRequestResponse,
  RejectPurchaseRequestRequest,
  PricingSnapshotResponse,
  CreatePricingSnapshotRequest,
  LeadTimeSnapshotResponse,
  CreateLeadTimeSnapshotRequest,
  AvailabilitySnapshotResponse,
  CreateAvailabilitySnapshotRequest,
  ReorderEvaluationResponse,
  ReorderSuggestionResponse,
  UpsertPartReorderPolicyRequest,
  PartReorderPolicyResponse,
  CreatePurchaseRequestFromReorderRequest,
  MaintainArrDemandRefResponse,
  CreatePurchaseRequestFromDemandRefRequest,
  ProcurementNotificationSettingsResponse,
  UpsertProcurementNotificationSettingsRequest,
  ProcurementNotificationDispatchesResponse,
  PriceSnapshotSettingsResponse,
  UpsertPriceSnapshotSettingsRequest,
  PendingPriceSnapshotCapturesResponse,
  PriceSnapshotRunsResponse,
  LeadTimeSnapshotSettingsResponse,
  UpsertLeadTimeSnapshotSettingsRequest,
  PendingLeadTimeSnapshotCapturesResponse,
  LeadTimeSnapshotRunsResponse,
  AvailabilitySnapshotSettingsResponse,
  UpsertAvailabilitySnapshotSettingsRequest,
  PendingAvailabilitySnapshotCapturesResponse,
  AvailabilitySnapshotRunsResponse,
  ProcurementCoordinationDashboardResponse,
  ProcurementCoordinationSummaryResponse,
  ProcurementCoordinationSettingsResponse,
  UpsertProcurementCoordinationSettingsRequest,
  PendingProcurementCoordinationResponse,
  ProcurementCoordinationRunsResponse,
  ApprovalReminderSettingsResponse,
  UpsertApprovalReminderSettingsRequest,
  PendingApprovalRemindersResponse,
  ApprovalReminderRunsResponse,
  ApprovalRemindersDashboardResponse,
  ProcurementExceptionEscalationSettingsResponse,
  UpsertProcurementExceptionEscalationSettingsRequest,
  PendingProcurementExceptionEscalationsResponse,
  PendingProcurementExceptionAutoClosesResponse,
  ProcurementExceptionEscalationRunsResponse,
  ProcurementExceptionEscalationEventsResponse,
  DemandProcessingSettingsResponse,
  UpsertDemandProcessingSettingsRequest,
  PendingDemandProcessingResponse,
  DemandProcessingRunsResponse,
  DemandProcessingDashboardResponse,
  DemandProcessingDetailResponse,
  DemandProcessingOperatorActionResponse,
  SupplyReadinessDashboardResponse,
  PartSupplyReadinessResponse,
  SupplierSupplyReadinessResponse,
  ProcurementPathReadinessResponse,
  IntegrationEventSettingsResponse,
  UpsertIntegrationEventSettingsRequest,
  IntegrationEventsListResponse,
  RfqResponse,
  RfqSupplierInvitationResponse,
  RfqQuoteComparisonResponse,
  SupplierPortalCreateQuoteRequest,
  SupplierPortalRfqResponse,
  SupplierQuoteResponse,
  SupplierEmailInboxListResponse,
  IngestSupplierEmailInboxRequest,
  IngestSupplierEmailInboxResponse,
  CreatePurchaseRequestFromRfqResponse,
  SupplierOnboardingResponse,
  SupplierOnboardingDocumentRequirementsResponse,
  SupplierRestrictionResponse,
  CreateSupplierRestrictionRequest,
  LiftSupplierRestrictionRequest,
  SupplierRestrictionEnforcementResponse,
  SupplierIncidentResponse,
  CreateSupplierIncidentRequest,
  ResolveSupplierIncidentRequest,
  CancelSupplierIncidentRequest,
  ReopenSupplierIncidentRequest,
  ApplySupplierIncidentProcurementRestrictionRequest,
  ProcurementExceptionResponse,
  ProcurementExceptionResolutionTemplateResponse,
  CreateProcurementExceptionRequest,
  AssignProcurementExceptionRequest,
  LinkProcurementExceptionActionsRequest,
  ResolveProcurementExceptionRequest,
  RequestProcurementExceptionWaiveRequest,
  RejectProcurementExceptionWaiveRequest,
  CloseProcurementExceptionRequest,
  CancelProcurementExceptionRequest,
  ReopenProcurementExceptionRequest,
  ProcurementApprovalAuthorityMirrorResponse,
  SupplierComplianceDocumentResponse,
  EmergencyPurchaseResponse,
  CreateEmergencyPurchaseRequest,
  IssueEmergencyPurchaseOrderResponse,
  ForgivingSearchResponse,
  AuditHistoryListResponse,
  SupplierReportSummaryItem,
  SupplierReportSummaryResponse,
  SupplierReportDetailResponse,
  SupplierComplianceReportSummaryResponse,
  SupplierComplianceDetailResponse,
  PartsInventoryReportSummaryResponse,
  PartsInventoryPartSummaryItem,
  PartsInventoryPartDetailResponse,
  PartsInventoryLocationDetailResponse,
  PurchasingReportSummaryResponse,
  PurchasingDocumentSummaryItem,
  PurchasingPurchaseRequestDetailResponse,
  PurchasingPurchaseOrderDetailResponse,
} from './types'
import type { ProductImportHistoryEntry, ProductImportManifest } from '@stl/shared-ui'

const apiBase = import.meta.env.VITE_SUPPLYARR_API_BASE ?? ''

type RawSupplierResponse = SupplierResponse
type RawSupplierSupplyReadinessResponse = SupplierSupplyReadinessResponse
type RawProcurementPathReadinessResponse = ProcurementPathReadinessResponse
type RawSupplierRestrictionResponse = SupplierRestrictionResponse
type RawSupplierRestrictionEnforcementResponse = SupplierRestrictionEnforcementResponse
type RawPricingSnapshotResponse = PricingSnapshotResponse
type RawLeadTimeSnapshotResponse = LeadTimeSnapshotResponse
type RawAvailabilitySnapshotResponse = AvailabilitySnapshotResponse
type RawSupplierOnboardingResponse = SupplierOnboardingResponse
type RawSupplierComplianceDocumentResponse = SupplierComplianceDocumentResponse
type RawSupplierIncidentResponse = SupplierIncidentResponse

export class SupplyArrApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly body: string,
  ) {
    super(message)
    this.name = 'SupplyArrApiError'
  }
}

type ProblemDetailsLike = {
  title?: string
  detail?: string
  errors?: Record<string, string[] | string>
}

type SupplierIdentityLike = {
  supplierId?: string | null
  supplierKey?: string | null
  supplierDisplayName?: string | null
}

function resolveSupplierId(raw: SupplierIdentityLike): string | null {
  return raw.supplierId ?? null
}

function resolveSupplierKey(raw: SupplierIdentityLike): string | null {
  return raw.supplierKey ?? null
}

function resolveSupplierDisplayName(raw: SupplierIdentityLike): string | null {
  return raw.supplierDisplayName ?? null
}

function serializeSupplierReference<T extends {
  supplierId?: string | null
  supplierUnitId?: string | null
}>(request: T): T & { supplierId?: string | null } {
  return {
    ...request,
    supplierId: request.supplierUnitId ?? request.supplierId ?? null,
  }
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

async function toApiError(response: Response, fallbackMessage: string): Promise<SupplyArrApiError> {
  const body = await response.text()
  const parsedMessage = extractProblemDetailsMessage(body)
  const message = parsedMessage || body || `${fallbackMessage} (${response.status})`
  return new SupplyArrApiError(message, response.status, body)
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

function normalizeSupplierResponse(raw: RawSupplierResponse): SupplierResponse {
  return {
    supplierId: raw.supplierId,
    supplierKey: raw.supplierKey,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    unitKind: raw.unitKind,
    displayName: raw.displayName,
    legalName: raw.legalName,
    taxIdentifier: raw.taxIdentifier,
    approvalStatus: raw.approvalStatus,
    status: raw.status,
    notes: raw.notes,
    serviceTypes: raw.serviceTypes ?? [],
    addressLine1: raw.addressLine1,
    addressLine2: raw.addressLine2,
    locality: raw.locality,
    regionCode: raw.regionCode,
    postalCode: raw.postalCode,
    countryCode: raw.countryCode,
    childUnitCount: raw.childUnitCount ?? 0,
    contacts: raw.contacts ?? [],
    createdAt: raw.createdAt,
    updatedAt: raw.updatedAt,
  }
}

function normalizeSupplierResponses(raw: RawSupplierResponse[]): SupplierResponse[] {
  return raw.map(normalizeSupplierResponse)
}

function normalizeSupplierOnboarding(raw: RawSupplierOnboardingResponse): SupplierOnboardingResponse {
  return {
    ...raw,
    supplierId: raw.supplierId,
    supplierKey: raw.supplierKey,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
  }
}

function normalizeSupplierOnboardings(raw: RawSupplierOnboardingResponse[]): SupplierOnboardingResponse[] {
  return raw.map(normalizeSupplierOnboarding)
}

function normalizeSupplierComplianceDocument(raw: RawSupplierComplianceDocumentResponse): SupplierComplianceDocumentResponse {
  return {
    ...raw,
    supplierId: raw.supplierId,
    supplierKey: raw.supplierKey,
    supplierDisplayName: raw.supplierDisplayName,
  }
}

function normalizeSupplierComplianceDocuments(raw: RawSupplierComplianceDocumentResponse[]): SupplierComplianceDocumentResponse[] {
  return raw.map(normalizeSupplierComplianceDocument)
}

function normalizeSupplierComplianceDetail(raw: SupplierComplianceDetailResponse): SupplierComplianceDetailResponse {
  return {
    ...raw,
    summary: {
      ...raw.summary,
      parentSupplierId: raw.summary.parentSupplierId ?? null,
      parentSupplierDisplayName: raw.summary.parentSupplierDisplayName ?? null,
      supplierUnitKind: raw.summary.supplierUnitKind ?? 'identity',
      supplierServiceTypes: raw.summary.supplierServiceTypes ?? [],
    },
  }
}

function serializeSupplierRequest(request: CreateSupplierRequest | UpdateSupplierRequest) {
  return {
    ...request,
    parentSupplierId: request.parentSupplierId ?? null,
  }
}

function normalizeSupplierReadiness(raw: RawSupplierSupplyReadinessResponse): SupplierSupplyReadinessResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierKey: resolveSupplierKey(raw)!,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeProcurementPathReadiness(raw: RawProcurementPathReadinessResponse): ProcurementPathReadinessResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierKey: resolveSupplierKey(raw)!,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplierRestriction(raw: RawSupplierRestrictionResponse): SupplierRestrictionResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierKey: resolveSupplierKey(raw)!,
    supplierDisplayName: resolveSupplierDisplayName(raw)!,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplierRestrictions(raw: RawSupplierRestrictionResponse[]): SupplierRestrictionResponse[] {
  return raw.map(normalizeSupplierRestriction)
}

function normalizeSupplierRestrictionEnforcement(raw: RawSupplierRestrictionEnforcementResponse): SupplierRestrictionEnforcementResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplierIncident(raw: RawSupplierIncidentResponse): SupplierIncidentResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierKey: resolveSupplierKey(raw)!,
    supplierDisplayName: resolveSupplierDisplayName(raw)!,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplierIncidents(raw: RawSupplierIncidentResponse[]): SupplierIncidentResponse[] {
  return raw.map(normalizeSupplierIncident)
}

function normalizePartSupplierLink(raw: PartSupplierLinkResponse): PartSupplierLinkResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierKey: resolveSupplierKey(raw)!,
    supplierDisplayName: resolveSupplierDisplayName(raw)!,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizePartResponse(raw: PartResponse): PartResponse {
  return {
    ...raw,
    supplierLinks: raw.supplierLinks.map(normalizePartSupplierLink),
  }
}

function normalizePurchaseRequestResponse(raw: PurchaseRequestResponse): PurchaseRequestResponse {
  const supplierId = resolveSupplierId(raw)
  return {
    ...raw,
    supplierId,
    supplierKey: resolveSupplierKey(raw),
    supplierDisplayName: resolveSupplierDisplayName(raw),
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? (supplierId ? 'identity' : null),
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeEmergencyPurchaseResponse(raw: EmergencyPurchaseResponse): EmergencyPurchaseResponse {
  const supplierId = resolveSupplierId(raw)
  return {
    ...raw,
    supplierId: supplierId ?? undefined,
    supplierKey: resolveSupplierKey(raw) ?? undefined,
    supplierDisplayName: resolveSupplierDisplayName(raw) ?? undefined,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? (supplierId ? 'identity' : null),
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeEmergencyPurchaseResponses(raw: EmergencyPurchaseResponse[]): EmergencyPurchaseResponse[] {
  return raw.map(normalizeEmergencyPurchaseResponse)
}

function normalizeProcurementCoordinationSummary(
  raw: ProcurementCoordinationSummaryResponse,
): ProcurementCoordinationSummaryResponse {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? (raw.supplierId ? 'identity' : null),
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeProcurementExceptionResponse(
  raw: ProcurementExceptionResponse,
): ProcurementExceptionResponse {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? (raw.supplierId ? 'identity' : null),
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizePurchaseOrderResponse(raw: PurchaseOrderResponse): PurchaseOrderResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierKey: resolveSupplierKey(raw)!,
    supplierDisplayName: resolveSupplierDisplayName(raw)!,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplyContractResponse(raw: SupplyContractResponse): SupplyContractResponse {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw)!,
    supplierKey: resolveSupplierKey(raw)!,
    supplierDisplayName: resolveSupplierDisplayName(raw)!,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplyContractResponses(raw: SupplyContractResponse[]): SupplyContractResponse[] {
  return raw.map(normalizeSupplyContractResponse)
}

function normalizePurchasingDocumentSummaryItem(raw: PurchasingDocumentSummaryItem): PurchasingDocumentSummaryItem {
  const supplierId = resolveSupplierId(raw)
  const hasSupplierIdentity = Boolean(
    supplierId ?? resolveSupplierDisplayName(raw),
  )

  return {
    ...raw,
    supplierId,
    supplierKey: resolveSupplierKey(raw),
    supplierDisplayName: resolveSupplierDisplayName(raw) ?? '',
    supplierUnitKind: raw.supplierUnitKind ?? (hasSupplierIdentity ? 'identity' : null),
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizePurchasingReportSummary(
  raw: PurchasingReportSummaryResponse,
): PurchasingReportSummaryResponse {
  return {
    ...raw,
    analytics: {
      ...raw.analytics,
      supplierDocumentExpiringSoonCount:
        raw.analytics.supplierDocumentExpiringSoonCount ?? 0,
      blockedSupplierCount: raw.analytics.blockedSupplierCount ?? 0,
    },
    documents: raw.documents.map(normalizePurchasingDocumentSummaryItem),
  }
}

function normalizePurchasingPurchaseRequestDetail(
  raw: PurchasingPurchaseRequestDetailResponse,
): PurchasingPurchaseRequestDetailResponse {
  return {
    ...raw,
    summary: normalizePurchasingDocumentSummaryItem(raw.summary),
  }
}

function normalizePurchasingPurchaseOrderDetail(
  raw: PurchasingPurchaseOrderDetailResponse,
): PurchasingPurchaseOrderDetailResponse {
  return {
    ...raw,
    summary: normalizePurchasingDocumentSummaryItem(raw.summary),
  }
}

function normalizeRfqInvitation(raw: RfqSupplierInvitationResponse): RfqSupplierInvitationResponse {
  return {
    ...raw,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeSupplierQuote(raw: SupplierQuoteResponse): SupplierQuoteResponse {
  return {
    ...raw,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeRfq(raw: RfqResponse): RfqResponse {
  return {
    ...raw,
    awardedSupplierUnitKind: raw.awardedSupplierUnitKind ?? null,
    awardedSupplierServiceTypes: raw.awardedSupplierServiceTypes ?? [],
    invitations: raw.invitations.map(normalizeRfqInvitation),
    quotes: raw.quotes.map(normalizeSupplierQuote),
  }
}

function normalizeSupplierPortalRfq(raw: SupplierPortalRfqResponse): SupplierPortalRfqResponse {
  return {
    ...raw,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeRfqQuoteComparison(raw: RfqQuoteComparisonResponse): RfqQuoteComparisonResponse {
  return {
    ...raw,
    lines: raw.lines.map((line) => ({
      ...line,
      quotes: line.quotes.map((quote) => ({
        ...quote,
        supplierUnitKind: quote.supplierUnitKind ?? 'identity',
        supplierServiceTypes: quote.supplierServiceTypes ?? [],
      })),
    })),
    quoteSummaries: raw.quoteSummaries.map((quote) => ({
      ...quote,
      supplierUnitKind: quote.supplierUnitKind ?? 'identity',
      supplierServiceTypes: quote.supplierServiceTypes ?? [],
    })),
  }
}

function normalizeSupplierReturn(raw: SupplierReturnResponse): SupplierReturnResponse {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeWarrantyClaim(raw: WarrantyClaimResponse): WarrantyClaimResponse {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizePricingSnapshot(raw: RawPricingSnapshotResponse): PricingSnapshotResponse {
  return {
    ...raw,
    supplierId: raw.supplierId,
    supplierKey: raw.supplierKey,
    supplierDisplayName: raw.supplierDisplayName,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeLeadTimeSnapshot(raw: RawLeadTimeSnapshotResponse): LeadTimeSnapshotResponse {
  return {
    ...raw,
    supplierId: raw.supplierId,
    supplierKey: raw.supplierKey,
    supplierDisplayName: raw.supplierDisplayName,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeAvailabilitySnapshot(raw: RawAvailabilitySnapshotResponse): AvailabilitySnapshotResponse {
  return {
    ...raw,
    supplierId: raw.supplierId,
    supplierKey: raw.supplierKey,
    supplierDisplayName: raw.supplierDisplayName,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizePendingSnapshotCaptureItem<
  T extends {
    partSupplierLinkId: string
    partId: string
    partKey: string
    partDisplayName: string
    supplierPartNumber: string
    lastCapturedAt: string | null
  } & SupplierIdentityLike
>(
  raw: T,
): T & { supplierId: string; supplierKey: string; supplierDisplayName: string } {
  return {
    ...raw,
    supplierId: resolveSupplierId(raw) ?? '',
    supplierKey: resolveSupplierKey(raw) ?? '',
    supplierDisplayName: resolveSupplierDisplayName(raw) ?? '',
  }
}

function normalizeApprovalReminderSummary(
  raw: ApprovalReminderSummaryResponse & SupplierIdentityLike,
): ApprovalReminderSummaryResponse {
  const hasSupplierIdentity = Boolean(
    resolveSupplierId(raw) ?? resolveSupplierDisplayName(raw) ?? resolveSupplierKey(raw),
  )

  return {
    ...raw,
    supplierId: resolveSupplierId(raw),
    supplierKey: resolveSupplierKey(raw),
    supplierDisplayName: resolveSupplierDisplayName(raw),
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? (hasSupplierIdentity ? 'identity' : null),
    supplierServiceTypes: raw.supplierServiceTypes ?? [],
  }
}

function normalizeApprovalRemindersDashboard(
  raw: ApprovalRemindersDashboardResponse,
): ApprovalRemindersDashboardResponse {
  return {
    ...raw,
    items: raw.items.map((item) => normalizeApprovalReminderSummary(item as ApprovalReminderSummaryResponse & SupplierIdentityLike)),
  }
}

function normalizeReorderSuggestion(raw: ReorderSuggestionResponse): ReorderSuggestionResponse {
  return {
    ...raw,
    preferredSupplierId: raw.preferredSupplierId ?? null,
    preferredSupplierKey: raw.preferredSupplierKey ?? null,
    preferredSupplierDisplayName: raw.preferredSupplierDisplayName ?? null,
  }
}

function normalizePartsInventoryPartSummary(
  raw: PartsInventoryPartSummaryItem,
): PartsInventoryPartSummaryItem {
  return {
    ...raw,
    supplierLinkCount: raw.supplierLinkCount ?? 0,
  }
}

function normalizePartsInventoryPartDetail(
  raw: PartsInventoryPartDetailResponse,
): PartsInventoryPartDetailResponse {
  return {
    ...raw,
    summary: normalizePartsInventoryPartSummary(raw.summary),
    supplierLinks: raw.supplierLinks.map((link) => ({
      ...link,
      supplierUnitKind: link.supplierUnitKind ?? 'identity',
      supplierServiceTypes: link.supplierServiceTypes ?? [],
      parentSupplierId: link.parentSupplierId ?? null,
      parentSupplierDisplayName: link.parentSupplierDisplayName ?? null,
    })),
  }
}

function normalizePartsInventoryReportSummary(
  raw: PartsInventoryReportSummaryResponse,
): PartsInventoryReportSummaryResponse {
  return {
    ...raw,
    parts: raw.parts.map(normalizePartsInventoryPartSummary),
  }
}

function normalizeReorderEvaluation(raw: ReorderEvaluationResponse): ReorderEvaluationResponse {
  return {
    ...raw,
    suggestions: raw.suggestions.map(normalizeReorderSuggestion),
  }
}

function normalizeSupplierReportSummaryItem(raw: SupplierReportSummaryItem): SupplierReportSummaryItem {
  return {
    ...raw,
    parentSupplierId: raw.parentSupplierId ?? null,
    parentSupplierDisplayName: raw.parentSupplierDisplayName ?? null,
    supplierUnitKind: raw.supplierUnitKind ?? 'identity',
    supplierServiceTypes: Array.isArray(raw.supplierServiceTypes) ? raw.supplierServiceTypes : [],
  }
}

function normalizeSupplierReportSummary(raw: SupplierReportSummaryResponse): SupplierReportSummaryResponse {
  return {
    ...raw,
    suppliers: raw.suppliers.map(normalizeSupplierReportSummaryItem),
  }
}

function normalizeSupplierReportDetail(raw: SupplierReportDetailResponse): SupplierReportDetailResponse {
  const normalizedSummary = normalizeSupplierReportSummaryItem(raw.summary)

  return {
    ...raw,
    summary: normalizedSummary,
    partLinks: raw.partLinks.map((partLink) => ({
      ...partLink,
      supplierId: normalizedSummary.supplierId,
      supplierKey: normalizedSummary.supplierKey,
      supplierDisplayName: normalizedSummary.supplierDisplayName,
    })),
  }
}

async function downloadExportBlob(
  accessToken: string,
  path: string,
  errorMessage: string,
): Promise<Blob> {
  const response = await fetch(`${apiBase}${path}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!response.ok) {
    throw await toApiError(response, errorMessage)
  }
  return response.blob()
}

export async function redeemHandoff(handoffCode: string): Promise<HandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<HandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getMe(accessToken: string): Promise<SupplyArrMeResponse> {
  const response = await fetch(`${apiBase}/api/me`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplyArrMeResponse>(response, 'Failed to load profile')
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<SupplyArrSessionBootstrapResponse> {
  const response = await fetch(`${apiBase}/api/session`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplyArrSessionBootstrapResponse>(
    response,
    'Failed to load session bootstrap',
  )
}

export async function getSupplierDirectory(accessToken: string): Promise<SupplierResponse[]> {
  const response = await fetch(`${apiBase}/api/suppliers`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierResponse[]>(response, 'Failed to load supplier directory')
  return normalizeSupplierResponses(raw)
}

export async function getSuppliers(accessToken: string): Promise<SupplierResponse[]> {
  return getSupplierDirectory(accessToken)
}

export async function getSupplierDirectoryMetadata(accessToken: string): Promise<SupplierDirectoryMetadataResponse> {
  const response = await fetch(`${apiBase}/api/v1/suppliers/metadata`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierDirectoryMetadataResponse>(
    response,
    'Failed to load supplier directory metadata',
  )
}

export async function createSupplier(
  accessToken: string,
  request: CreateSupplierRequest,
): Promise<SupplierResponse> {
  const response = await fetch(`${apiBase}/api/suppliers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(serializeSupplierRequest(request)),
  })
  const raw = await parseJsonResponse<RawSupplierResponse>(response, 'Failed to create supplier')
  return normalizeSupplierResponse(raw)
}

export async function updateSupplier(
  accessToken: string,
  supplierId: string,
  request: UpdateSupplierRequest,
): Promise<SupplierResponse> {
  const response = await fetch(`${apiBase}/api/suppliers/${supplierId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(serializeSupplierRequest(request)),
  })
  const raw = await parseJsonResponse<RawSupplierResponse>(response, 'Failed to update supplier')
  return normalizeSupplierResponse(raw)
}

export async function updateSupplierApprovalStatus(
  accessToken: string,
  supplierId: string,
  request: UpdateSupplierApprovalStatusRequest,
): Promise<SupplierResponse> {
  const response = await fetch(`${apiBase}/api/suppliers/${supplierId}/approval-status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  const raw = await parseJsonResponse<RawSupplierResponse>(response, 'Failed to update supplier approval status')
  return normalizeSupplierResponse(raw)
}

export async function updateSupplierStatus(
  accessToken: string,
  supplierId: string,
  request: UpdateSupplierStatusRequest,
): Promise<SupplierResponse> {
  const response = await fetch(`${apiBase}/api/suppliers/${supplierId}/status`, {
    method: 'PATCH',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  const raw = await parseJsonResponse<RawSupplierResponse>(response, 'Failed to update supplier status')
  return normalizeSupplierResponse(raw)
}

export async function createSupplierContact(
  accessToken: string,
  supplierId: string,
  request: CreateSupplierContactRequest,
): Promise<SupplierResponse> {
  const response = await fetch(`${apiBase}/api/suppliers/${supplierId}/contacts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  if (!response.ok) {
    const raw = await parseJsonResponse<RawSupplierResponse>(response, 'Failed to add supplier contact')
    return normalizeSupplierResponse(raw)
  }

  const listResponse = await fetch(`${apiBase}/api/suppliers/${supplierId}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierResponse>(listResponse, 'Failed to reload supplier after contact add')
  return normalizeSupplierResponse(raw)
}

export async function getPartCatalogs(accessToken: string): Promise<PartCatalogResponse[]> {
  const response = await fetch(`${apiBase}/api/catalogs`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartCatalogResponse[]>(response, 'Failed to load part catalogs')
}

export async function getParts(accessToken: string): Promise<PartResponse[]> {
  const response = await fetch(`${apiBase}/api/parts`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<PartResponse[]>(response, 'Failed to load parts')
  return raw.map(normalizePartResponse)
}

export async function syncSupplierCatalogApi(
  accessToken: string,
  request: SupplierCatalogApiSyncRequest,
): Promise<SupplierCatalogApiSyncResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-catalogs/sync`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({
      ...request,
      supplierKey: resolveSupplierKey(request),
    }),
  })
  return parseJsonResponse<SupplierCatalogApiSyncResponse>(response, 'Failed to sync supplier catalog API feed')
}

export async function getSubstitutions(
  accessToken: string,
  partId?: string,
): Promise<SubstitutionItemResponse[]> {
  const query = partId ? `?partId=${encodeURIComponent(partId)}` : ''
  const response = await fetch(`${apiBase}/api/v1/substitutions${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SubstitutionItemResponse[]>(response, 'Failed to load substitutions')
}

export async function createPartCatalog(
  accessToken: string,
  request: CreatePartCatalogRequest,
): Promise<PartCatalogResponse> {
  const response = await fetch(`${apiBase}/api/catalogs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartCatalogResponse>(response, 'Failed to create part catalog')
}

export async function createPart(
  accessToken: string,
  request: CreatePartRequest,
): Promise<PartResponse> {
  const response = await fetch(`${apiBase}/api/parts`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizePartResponse(await parseJsonResponse<PartResponse>(response, 'Failed to create part'))
}

export async function createPartSource(
  accessToken: string,
  partId: string,
  request: CreatePartSourceRequest,
): Promise<PartSourceResponse> {
  const response = await fetch(`${apiBase}/api/parts/${partId}/sources`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartSourceResponse>(response, 'Failed to add part source')
}

export async function createPartSupplierLink(
  accessToken: string,
  partId: string,
  request: CreatePartSupplierLinkRequest,
): Promise<PartSupplierLinkResponse> {
  const payload = serializeSupplierReference(request)
  const response = await fetch(`${apiBase}/api/parts/${partId}/supplier-links`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizePartSupplierLink(
    await parseJsonResponse<PartSupplierLinkResponse>(response, 'Failed to link supplier to part'),
  )
}

export async function getOutboundShipments(accessToken: string): Promise<OutboundShipmentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/wms/outbound-shipments`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<OutboundShipmentResponse[]>(response, 'Failed to load outbound shipments')
}

export async function createOutboundShipment(
  accessToken: string,
  request: CreateOutboundShipmentRequest,
): Promise<OutboundShipmentResponse> {
  const response = await fetch(`${apiBase}/api/v1/wms/outbound-shipments`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<OutboundShipmentResponse>(response, 'Failed to create outbound shipment')
}

export async function getPurchaseRequests(
  accessToken: string,
  status?: string,
): Promise<PurchaseRequestResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/purchase-requests${query}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<PurchaseRequestResponse[]>(response, 'Failed to load purchase requests')
  return raw.map(normalizePurchaseRequestResponse)
}

export async function createPurchaseRequest(
  accessToken: string,
  request: CreatePurchaseRequestRequest,
): Promise<PurchaseRequestResponse> {
  const payload = serializeSupplierReference(request)
  const response = await fetch(`${apiBase}/api/v1/purchase-requests`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizePurchaseRequestResponse(
    await parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to create purchase request'),
  )
}

export async function getRfqs(accessToken: string, status?: string): Promise<RfqResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/rfqs${query}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RfqResponse[]>(response, 'Failed to load RFQs')
  return raw.map(normalizeRfq)
}

export async function getRfq(accessToken: string, rfqId: string): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeRfq(await parseJsonResponse<RfqResponse>(response, 'Failed to load RFQ'))
}

const supplierPortalApiPath = `${apiBase}/api/v1/supplier-portal`

export async function getSupplierPortalRfq(
  rfqId: string,
  accessCode: string,
): Promise<SupplierPortalRfqResponse> {
  const query = `?accessCode=${encodeURIComponent(accessCode)}`
  const response = await fetch(`${supplierPortalApiPath}/rfqs/${rfqId}${query}`)
  return normalizeSupplierPortalRfq(
    await parseJsonResponse<SupplierPortalRfqResponse>(response, 'Failed to load supplier portal RFQ'),
  )
}

export async function createSupplierPortalQuote(
  rfqId: string,
  accessCode: string,
  payload: SupplierPortalCreateQuoteRequest,
): Promise<SupplierQuoteResponse> {
  const query = `?accessCode=${encodeURIComponent(accessCode)}`
  const response = await fetch(`${supplierPortalApiPath}/rfqs/${rfqId}/quotes${query}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return normalizeSupplierQuote(
    await parseJsonResponse<SupplierQuoteResponse>(response, 'Failed to create supplier portal quote'),
  )
}

export async function upsertSupplierPortalQuoteLine(
  rfqId: string,
  supplierQuoteId: string,
  accessCode: string,
  payload: {
    rfqLineId: string
    unitPrice: number
    quantityQuoted: number
    leadTimeDays?: number | null
    notes: string
  },
): Promise<SupplierQuoteResponse> {
  const query = `?accessCode=${encodeURIComponent(accessCode)}`
  const response = await fetch(
    `${supplierPortalApiPath}/rfqs/${rfqId}/quotes/${supplierQuoteId}/lines${query}`,
    {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload),
    },
  )
  return normalizeSupplierQuote(
    await parseJsonResponse<SupplierQuoteResponse>(response, 'Failed to save supplier portal quote line'),
  )
}

export async function submitSupplierPortalQuote(
  rfqId: string,
  supplierQuoteId: string,
  accessCode: string,
): Promise<SupplierQuoteResponse> {
  const query = `?accessCode=${encodeURIComponent(accessCode)}`
  const response = await fetch(
    `${supplierPortalApiPath}/rfqs/${rfqId}/quotes/${supplierQuoteId}/submit${query}`,
    { method: 'POST' },
  )
  return normalizeSupplierQuote(
    await parseJsonResponse<SupplierQuoteResponse>(response, 'Failed to submit supplier portal quote'),
  )
}

export async function getSupplierEmailInbox(
  accessToken: string,
  limit = 25,
): Promise<SupplierEmailInboxListResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-email-inbox?limit=${encodeURIComponent(limit)}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierEmailInboxListResponse>(
    response,
    'Failed to load supplier email inbox',
  )
}

export async function ingestSupplierEmailInbox(
  accessToken: string,
  payload: IngestSupplierEmailInboxRequest,
): Promise<IngestSupplierEmailInboxResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-email-inbox`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<IngestSupplierEmailInboxResponse>(
    response,
    'Failed to ingest supplier email',
  )
}

export async function createRfq(
  accessToken: string,
  payload: {
    rfqKey: string
    title: string
    notes: string
    lines?: { partId: string; quantityRequested: number; notes: string }[]
  },
): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeRfq(await parseJsonResponse<RfqResponse>(response, 'Failed to create RFQ'))
}

export async function submitRfq(accessToken: string, rfqId: string): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizeRfq(await parseJsonResponse<RfqResponse>(response, 'Failed to submit RFQ'))
}

export async function inviteRfqSuppliers(
  accessToken: string,
  rfqId: string,
  supplierIds: string[],
): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/invite-suppliers`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ supplierIds }),
  })
  return normalizeRfq(await parseJsonResponse<RfqResponse>(response, 'Failed to invite suppliers'))
}

export async function createSupplierQuote(
  accessToken: string,
  rfqId: string,
  payload: { supplierId: string; quoteKey: string; currencyCode: string; notes: string },
): Promise<SupplierQuoteResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quotes`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeSupplierQuote(
    await parseJsonResponse<SupplierQuoteResponse>(response, 'Failed to create supplier quote'),
  )
}

export async function upsertSupplierQuoteLine(
  accessToken: string,
  rfqId: string,
  supplierQuoteId: string,
  payload: {
    rfqLineId: string
    unitPrice: number
    quantityQuoted: number
    leadTimeDays?: number | null
    notes: string
  },
): Promise<SupplierQuoteResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quotes/${supplierQuoteId}/lines`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeSupplierQuote(
    await parseJsonResponse<SupplierQuoteResponse>(response, 'Failed to save supplier quote line'),
  )
}

export async function submitSupplierQuote(
  accessToken: string,
  rfqId: string,
  supplierQuoteId: string,
): Promise<SupplierQuoteResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quotes/${supplierQuoteId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierQuote(
    await parseJsonResponse<SupplierQuoteResponse>(response, 'Failed to submit supplier quote'),
  )
}

export async function getRfqQuoteComparison(
  accessToken: string,
  rfqId: string,
): Promise<RfqQuoteComparisonResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/quote-comparison`, {
    headers: authHeaders(accessToken),
  })
  return normalizeRfqQuoteComparison(
    await parseJsonResponse<RfqQuoteComparisonResponse>(response, 'Failed to load quote comparison'),
  )
}

export async function selectRfqSupplierQuote(
  accessToken: string,
  rfqId: string,
  supplierQuoteId: string,
): Promise<RfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/select-quote`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ supplierQuoteId }),
  })
  return normalizeRfq(await parseJsonResponse<RfqResponse>(response, 'Failed to select supplier quote'))
}

export async function createPurchaseRequestFromRfq(
  accessToken: string,
  rfqId: string,
  payload: { requestKey: string; title?: string; notes?: string },
): Promise<CreatePurchaseRequestFromRfqResponse> {
  const response = await fetch(`${apiBase}/api/v1/rfqs/${rfqId}/create-purchase-request`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  const raw = await parseJsonResponse<CreatePurchaseRequestFromRfqResponse>(
    response,
    'Failed to create purchase request from RFQ',
  )
  return {
    ...raw,
    purchaseRequest: normalizePurchaseRequestResponse(raw.purchaseRequest),
  }
}

export async function submitPurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-requests/${purchaseRequestId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizePurchaseRequestResponse(
    await parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to submit purchase request'),
  )
}

export async function approvePurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-requests/${purchaseRequestId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizePurchaseRequestResponse(
    await parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to approve purchase request'),
  )
}

export async function rejectPurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
  request: RejectPurchaseRequestRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-requests/${purchaseRequestId}/reject`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizePurchaseRequestResponse(
    await parseJsonResponse<PurchaseRequestResponse>(response, 'Failed to reject purchase request'),
  )
}

export async function getPurchaseOrders(
  accessToken: string,
  status?: string,
): Promise<PurchaseOrderResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/purchase-orders${query}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<PurchaseOrderResponse[]>(response, 'Failed to load purchase orders')
  return raw.map(normalizePurchaseOrderResponse)
}

export async function getContractRecords(
  accessToken: string,
  options?: { supplierId?: string; status?: string; limit?: number },
): Promise<SupplyContractResponse[]> {
  const params = new URLSearchParams()
  if (options?.supplierId) {
    params.set('supplierId', options.supplierId)
  }
  if (options?.status) {
    params.set('status', options.status)
  }
  if (typeof options?.limit === 'number') {
    params.set('limit', String(options.limit))
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/contracts/records${query}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplyContractResponses(
    await parseJsonResponse<SupplyContractResponse[]>(response, 'Failed to load contract records'),
  )
}

export async function importContractsCsv(
  accessToken: string,
  request: ContractsCsvImportRequest,
): Promise<ContractsCsvImportResponse> {
  const response = await fetch(`${apiBase}/api/v1/imports/contracts-csv`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })

  const body = await response.text()
  if (!body) {
    if (!response.ok) {
      throw new SupplyArrApiError('Failed to import contracts CSV', response.status, body)
    }
    throw new SupplyArrApiError('Contracts CSV import returned an empty response.', response.status, body)
  }

  try {
    return JSON.parse(body) as ContractsCsvImportResponse
  } catch {
    throw new SupplyArrApiError(
      body || 'Failed to import contracts CSV',
      response.status,
      body,
    )
  }
}

export interface GenericCsvImportIssue {
  lineNumber: number
  code: string
  message: string
}

export interface GenericCsvImportResponse {
  importType: string
  dryRun: boolean
  succeeded: boolean
  rowsRead: number
  issues: GenericCsvImportIssue[]
  metrics: Array<{ key: string; value: number }>
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
  importType?: string,
  limit = 25,
): Promise<ProductImportHistoryEntry[]> {
  const params = new URLSearchParams()
  params.set('limit', String(limit))
  if (importType) {
    params.set('importType', importType)
  }

  const response = await fetch(`${apiBase}/api/v1/imports/history?${params.toString()}`, {
    headers: authHeaders(accessToken),
  })
  const payload = await parseJsonResponse<{
    items: Array<{
      importHistoryId: string
      importType: string
      dryRun: boolean
      succeeded: boolean
      rowsRead: number
      issueCount: number
      actorUserId?: string | null
      occurredAt: string
    }>
  }>(response, 'Failed to load import history')

  return payload.items.map((item) => {
    const successCount = Math.max(item.rowsRead - item.issueCount, 0)
    return {
      importHistoryId: item.importHistoryId,
      importTypeKey: item.importType,
      displayName: item.importType
        .replace(/_csv$/i, '')
        .split('_')
        .filter(Boolean)
        .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
        .join(' '),
      status: item.succeeded ? (item.dryRun ? 'validated' : 'completed') : 'issues_found',
      dryRun: item.dryRun,
      rowCount: item.rowsRead,
      successCount,
      errorCount: item.issueCount,
      actorUserId: item.actorUserId,
      occurredAt: item.occurredAt,
      summary: item.dryRun
        ? `Validated ${successCount} of ${item.rowsRead} rows.`
        : `Processed ${successCount} of ${item.rowsRead} rows.`,
    }
  })
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

export async function runGenericCsvImport(
  accessToken: string,
  importTypeKey: string,
  request: { csv: string; dryRun: boolean; fileName?: string | null },
): Promise<GenericCsvImportResponse> {
  const routeKey = importTypeKey.replace(/_/g, '-')
  const response = await fetch(`${apiBase}/api/v1/imports/${routeKey}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })

  const body = await response.text()
  if (!body) {
    throw new SupplyArrApiError('Import returned an empty response.', response.status, body)
  }
  if (!response.ok) {
    throw new SupplyArrApiError(body, response.status, body)
  }

  let parsed: Record<string, unknown>
  try {
    parsed = JSON.parse(body) as Record<string, unknown>
  } catch {
    throw new SupplyArrApiError('Import returned invalid JSON.', response.status, body)
  }

  const metrics = Object.entries(parsed)
    .filter(([key, value]) => typeof value === 'number' && key !== 'rowsRead')
    .map(([key, value]) => ({ key, value: value as number }))

  const issues = Array.isArray(parsed.issues)
    ? parsed.issues.map((issue) => {
        const item = issue as {
          lineNumber?: unknown
          code?: unknown
          message?: unknown
        }
        return {
          lineNumber: Number(item.lineNumber ?? 0),
          code: String(item.code ?? 'import.issue'),
          message: String(item.message ?? 'Import issue'),
        }
      })
    : []

  return {
    importType: String(parsed.importType ?? importTypeKey),
    dryRun: Boolean(parsed.dryRun),
    succeeded: Boolean(parsed.succeeded),
    rowsRead: Number(parsed.rowsRead ?? 0),
    issues,
    metrics,
  }
}

export async function createPurchaseOrderFromPurchaseRequest(
  accessToken: string,
  purchaseRequestId: string,
  request: CreatePurchaseOrderFromPurchaseRequestRequest,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/purchase-orders/from-purchase-request/${purchaseRequestId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return normalizePurchaseOrderResponse(
    await parseJsonResponse<PurchaseOrderResponse>(
      response,
      'Failed to create purchase order from purchase request',
    ),
  )
}

export async function approvePurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-orders/${purchaseOrderId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizePurchaseOrderResponse(
    await parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to approve purchase order'),
  )
}

export async function issuePurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-orders/${purchaseOrderId}/issue`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizePurchaseOrderResponse(
    await parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to issue purchase order'),
  )
}

export async function cancelPurchaseOrder(
  accessToken: string,
  purchaseOrderId: string,
  request: CancelPurchaseOrderRequest,
): Promise<PurchaseOrderResponse> {
  const response = await fetch(`${apiBase}/api/v1/purchase-orders/${purchaseOrderId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizePurchaseOrderResponse(
    await parseJsonResponse<PurchaseOrderResponse>(response, 'Failed to cancel purchase order'),
  )
}

export async function getBackorders(
  accessToken: string,
  options?: { status?: string; purchaseOrderId?: string; partId?: string },
): Promise<BackorderResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.purchaseOrderId) {
    params.set('purchaseOrderId', options.purchaseOrderId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/backorders${query}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BackorderResponse[]>(response, 'Failed to load backorders')
}

export async function createBackorderFromPurchaseOrderLine(
  accessToken: string,
  purchaseOrderLineId: string,
  request: CreateBackorderFromPurchaseOrderLineRequest,
): Promise<BackorderResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/backorders/from-purchase-order-line/${purchaseOrderLineId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return parseJsonResponse<BackorderResponse>(response, 'Failed to create backorder')
}

export async function fulfillBackorder(
  accessToken: string,
  backorderId: string,
): Promise<BackorderResponse> {
  const response = await fetch(`${apiBase}/api/v1/backorders/${backorderId}/fulfill`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<BackorderResponse>(response, 'Failed to fulfill backorder')
}

export async function cancelBackorder(
  accessToken: string,
  backorderId: string,
  request: CancelBackorderRequest,
): Promise<BackorderResponse> {
  const response = await fetch(`${apiBase}/api/v1/backorders/${backorderId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<BackorderResponse>(response, 'Failed to cancel backorder')
}

export async function getSupplierReturns(
  accessToken: string,
  options?: {
    status?: string
    supplierId?: string
    purchaseOrderId?: string
    partId?: string
  },
): Promise<SupplierReturnResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) {
    params.set('status', options.status)
  }
  if (options?.supplierId) {
    params.set('supplierId', options.supplierId)
  }
  if (options?.purchaseOrderId) {
    params.set('purchaseOrderId', options.purchaseOrderId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/returns${query}`, {
    headers: authHeaders(accessToken),
  })
  const returns = await parseJsonResponse<SupplierReturnResponse[]>(response, 'Failed to load supplier returns')
  return returns.map(normalizeSupplierReturn)
}

export async function createSupplierReturnFromStock(
  accessToken: string,
  request: CreateSupplierReturnFromStockRequest,
): Promise<SupplierReturnResponse> {
  const response = await fetch(`${apiBase}/api/v1/returns/from-stock`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(serializeSupplierReference(request)),
  })
  return normalizeSupplierReturn(
    await parseJsonResponse<SupplierReturnResponse>(response, 'Failed to create supplier return'),
  )
}

export async function createSupplierReturnFromPurchaseOrderLine(
  accessToken: string,
  purchaseOrderLineId: string,
  request: CreateSupplierReturnFromPurchaseOrderLineRequest,
): Promise<SupplierReturnResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/returns/from-purchase-order-line/${purchaseOrderLineId}`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return normalizeSupplierReturn(
    await parseJsonResponse<SupplierReturnResponse>(response, 'Failed to create supplier return'),
  )
}

export async function postSupplierReturn(
  accessToken: string,
  returnId: string,
): Promise<SupplierReturnResponse> {
  const response = await fetch(`${apiBase}/api/v1/returns/${returnId}/post`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierReturn(
    await parseJsonResponse<SupplierReturnResponse>(response, 'Failed to post supplier return'),
  )
}

export async function cancelSupplierReturn(
  accessToken: string,
  returnId: string,
  request: CancelSupplierReturnRequest,
): Promise<SupplierReturnResponse> {
  const response = await fetch(`${apiBase}/api/v1/returns/${returnId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeSupplierReturn(
    await parseJsonResponse<SupplierReturnResponse>(response, 'Failed to cancel supplier return'),
  )
}

export async function listSupplierWarrantyClaims(
  accessToken: string,
  options?: {
    status?: string
    supplierId?: string
    partId?: string
    purchaseOrderId?: string
  },
): Promise<WarrantyClaimResponse[]> {
  const params = new URLSearchParams()
  if (options?.status) params.set('status', options.status)
  if (options?.supplierId) params.set('supplierId', options.supplierId)
  if (options?.partId) params.set('partId', options.partId)
  if (options?.purchaseOrderId) params.set('purchaseOrderId', options.purchaseOrderId)
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/warranty-claims${query}`, {
    headers: authHeaders(accessToken),
  })
  const claims = await parseJsonResponse<WarrantyClaimResponse[]>(response, 'Failed to load warranty claims')
  return claims.map(normalizeWarrantyClaim)
}

export async function createSupplierWarrantyClaim(
  accessToken: string,
  request: CreateSupplierWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(serializeSupplierReference(request)),
  })
  return normalizeWarrantyClaim(
    await parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to create warranty claim'),
  )
}

export async function updateWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: UpdateWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeWarrantyClaim(
    await parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to update warranty claim'),
  )
}

export async function submitWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: SubmitWarrantyClaimRequest = {},
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeWarrantyClaim(
    await parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to submit warranty claim'),
  )
}

export async function recordWarrantyClaimSupplierResponse(
  accessToken: string,
  warrantyClaimId: string,
  request: RecordWarrantyClaimSupplierResponseRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/record-supplier-response`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(request),
    },
  )
  return normalizeWarrantyClaim(
    await parseJsonResponse<WarrantyClaimResponse>(
      response,
      'Failed to record warranty claim supplier response',
    ),
  )
}

export async function closeWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: CloseWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/close`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeWarrantyClaim(
    await parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to close warranty claim'),
  )
}

export async function denyWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: DenyWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/deny`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeWarrantyClaim(
    await parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to deny warranty claim'),
  )
}

export async function cancelWarrantyClaim(
  accessToken: string,
  warrantyClaimId: string,
  request: CancelWarrantyClaimRequest,
): Promise<WarrantyClaimResponse> {
  const response = await fetch(`${apiBase}/api/v1/warranty-claims/${warrantyClaimId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeWarrantyClaim(
    await parseJsonResponse<WarrantyClaimResponse>(response, 'Failed to cancel warranty claim'),
  )
}

export async function getPricingSnapshots(
  accessToken: string,
  options?: {
    partSupplierLinkId?: string
    partId?: string
    supplierId?: string
    asOf?: string
  },
): Promise<PricingSnapshotResponse[]> {
  const params = new URLSearchParams()
  if (options?.partSupplierLinkId) {
    params.set('partSupplierLinkId', options.partSupplierLinkId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  if (options?.supplierId) {
    params.set('supplierId', options.supplierId)
  }
  if (options?.asOf) {
    params.set('asOf', options.asOf)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/pricing-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  const snapshots = await parseJsonResponse<RawPricingSnapshotResponse[]>(response, 'Failed to load pricing snapshots')
  return snapshots.map(normalizePricingSnapshot)
}

export async function createPricingSnapshot(
  accessToken: string,
  request: CreatePricingSnapshotRequest,
): Promise<PricingSnapshotResponse> {
  const response = await fetch(`${apiBase}/api/v1/pricing-snapshots`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizePricingSnapshot(
    await parseJsonResponse<RawPricingSnapshotResponse>(response, 'Failed to create pricing snapshot'),
  )
}

export async function getLeadTimeSnapshots(
  accessToken: string,
  options?: {
    partSupplierLinkId?: string
    partId?: string
    supplierId?: string
    asOf?: string
  },
): Promise<LeadTimeSnapshotResponse[]> {
  const params = new URLSearchParams()
  if (options?.partSupplierLinkId) {
    params.set('partSupplierLinkId', options.partSupplierLinkId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  if (options?.supplierId) {
    params.set('supplierId', options.supplierId)
  }
  if (options?.asOf) {
    params.set('asOf', options.asOf)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  const snapshots = await parseJsonResponse<RawLeadTimeSnapshotResponse[]>(response, 'Failed to load lead-time snapshots')
  return snapshots.map(normalizeLeadTimeSnapshot)
}

export async function createLeadTimeSnapshot(
  accessToken: string,
  request: CreateLeadTimeSnapshotRequest,
): Promise<LeadTimeSnapshotResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshots`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeLeadTimeSnapshot(
    await parseJsonResponse<RawLeadTimeSnapshotResponse>(response, 'Failed to create lead-time snapshot'),
  )
}

export async function getAvailabilitySnapshots(
  accessToken: string,
  options?: {
    partSupplierLinkId?: string
    partId?: string
    supplierId?: string
    asOf?: string
  },
): Promise<AvailabilitySnapshotResponse[]> {
  const params = new URLSearchParams()
  if (options?.partSupplierLinkId) {
    params.set('partSupplierLinkId', options.partSupplierLinkId)
  }
  if (options?.partId) {
    params.set('partId', options.partId)
  }
  if (options?.supplierId) {
    params.set('supplierId', options.supplierId)
  }
  if (options?.asOf) {
    params.set('asOf', options.asOf)
  }
  const query = params.toString() ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/availability-snapshots${query}`, {
    headers: authHeaders(accessToken),
  })
  const snapshots = await parseJsonResponse<RawAvailabilitySnapshotResponse[]>(
    response,
    'Failed to load availability snapshots',
  )
  return snapshots.map(normalizeAvailabilitySnapshot)
}

export async function createAvailabilitySnapshot(
  accessToken: string,
  request: CreateAvailabilitySnapshotRequest,
): Promise<AvailabilitySnapshotResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshots`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return normalizeAvailabilitySnapshot(
    await parseJsonResponse<RawAvailabilitySnapshotResponse>(
      response,
      'Failed to create availability snapshot',
    ),
  )
}

export async function getReorderEvaluation(accessToken: string): Promise<ReorderEvaluationResponse> {
  const response = await fetch(`${apiBase}/api/v1/reorder-evaluation`, {
    headers: authHeaders(accessToken),
  })
  return normalizeReorderEvaluation(
    await parseJsonResponse<ReorderEvaluationResponse>(response, 'Failed to load reorder evaluation'),
  )
}

export async function upsertPartReorderPolicy(
  accessToken: string,
  partId: string,
  request: UpsertPartReorderPolicyRequest,
): Promise<PartReorderPolicyResponse> {
  const response = await fetch(`${apiBase}/api/v1/reorder-evaluation/parts/${partId}/policy`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PartReorderPolicyResponse>(response, 'Failed to save reorder policy')
}

export async function createPurchaseRequestFromReorder(
  accessToken: string,
  request: CreatePurchaseRequestFromReorderRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/reorder-evaluation/create-purchase-request`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseRequestResponse>(
    response,
    'Failed to create purchase request from reorder suggestions',
  )
}

export async function getDemandRefs(
  accessToken: string,
  options?: { status?: string },
): Promise<MaintainArrDemandRefResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) {
    search.set('status', options.status)
  }
  const suffix = search.size > 0 ? `?${search.toString()}` : ''
  const response = await fetch(`${apiBase}/api/v1/demand-refs${suffix}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrDemandRefResponse[]>(response, 'Failed to load demand references')
}

export async function getDemandRef(
  accessToken: string,
  demandRefId: string,
): Promise<MaintainArrDemandRefResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-refs/${demandRefId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<MaintainArrDemandRefResponse>(response, 'Failed to load demand reference')
}

export async function createPurchaseRequestFromDemandRef(
  accessToken: string,
  demandRefId: string,
  request: CreatePurchaseRequestFromDemandRefRequest,
): Promise<PurchaseRequestResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-refs/${demandRefId}/create-purchase-request`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(request),
  })
  return parseJsonResponse<PurchaseRequestResponse>(
    response,
    'Failed to create purchase request from demand reference',
  )
}

export async function getProcurementNotificationSettings(
  accessToken: string,
): Promise<ProcurementNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/notification-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementNotificationSettingsResponse>(
    response,
    'Failed to load notification settings',
  )
}

export async function upsertProcurementNotificationSettings(
  accessToken: string,
  payload: UpsertProcurementNotificationSettingsRequest,
): Promise<ProcurementNotificationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/notification-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementNotificationSettingsResponse>(
    response,
    'Failed to save notification settings',
  )
}

export async function getProcurementNotificationDispatches(
  accessToken: string,
  limit = 20,
): Promise<ProcurementNotificationDispatchesResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/notification-settings/dispatches?${search}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<ProcurementNotificationDispatchesResponse>(
    response,
    'Failed to load notification dispatches',
  )
  return { ...raw, items: raw.items }
}

export async function getPriceSnapshotSettings(
  accessToken: string,
): Promise<PriceSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PriceSnapshotSettingsResponse>(
    response,
    'Failed to load price snapshot settings',
  )
}

export async function upsertPriceSnapshotSettings(
  accessToken: string,
  payload: UpsertPriceSnapshotSettingsRequest,
): Promise<PriceSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<PriceSnapshotSettingsResponse>(
    response,
    'Failed to save price snapshot settings',
  )
}

export async function getPendingPriceSnapshotCaptures(
  accessToken: string,
): Promise<PendingPriceSnapshotCapturesResponse> {
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<PendingPriceSnapshotCapturesResponse>(
    response,
    'Failed to load pending price snapshot captures',
  )
  return {
    ...raw,
    items: raw.items.map((item) => normalizePendingSnapshotCaptureItem(item)),
  }
}

export async function getPriceSnapshotRuns(
  accessToken: string,
  limit = 5,
): Promise<PriceSnapshotRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/price-snapshot-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PriceSnapshotRunsResponse>(
    response,
    'Failed to load price snapshot runs',
  )
}

export async function getLeadTimeSnapshotSettings(
  accessToken: string,
): Promise<LeadTimeSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LeadTimeSnapshotSettingsResponse>(
    response,
    'Failed to load lead-time snapshot settings',
  )
}

export async function upsertLeadTimeSnapshotSettings(
  accessToken: string,
  payload: UpsertLeadTimeSnapshotSettingsRequest,
): Promise<LeadTimeSnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<LeadTimeSnapshotSettingsResponse>(
    response,
    'Failed to save lead-time snapshot settings',
  )
}

export async function getPendingLeadTimeSnapshotCaptures(
  accessToken: string,
): Promise<PendingLeadTimeSnapshotCapturesResponse> {
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<PendingLeadTimeSnapshotCapturesResponse>(
    response,
    'Failed to load pending lead-time snapshot captures',
  )
  return {
    ...raw,
    items: raw.items.map((item) => normalizePendingSnapshotCaptureItem(item)),
  }
}

export async function getLeadTimeSnapshotRuns(
  accessToken: string,
  limit = 5,
): Promise<LeadTimeSnapshotRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/lead-time-snapshot-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<LeadTimeSnapshotRunsResponse>(
    response,
    'Failed to load lead-time snapshot runs',
  )
}

export async function getAvailabilitySnapshotSettings(
  accessToken: string,
): Promise<AvailabilitySnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AvailabilitySnapshotSettingsResponse>(
    response,
    'Failed to load availability snapshot settings',
  )
}

export async function upsertAvailabilitySnapshotSettings(
  accessToken: string,
  payload: UpsertAvailabilitySnapshotSettingsRequest,
): Promise<AvailabilitySnapshotSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<AvailabilitySnapshotSettingsResponse>(
    response,
    'Failed to save availability snapshot settings',
  )
}

export async function getPendingAvailabilitySnapshotCaptures(
  accessToken: string,
): Promise<PendingAvailabilitySnapshotCapturesResponse> {
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<PendingAvailabilitySnapshotCapturesResponse>(
    response,
    'Failed to load pending availability snapshot captures',
  )
  return {
    ...raw,
    items: raw.items.map((item) => normalizePendingSnapshotCaptureItem(item)),
  }
}

export async function getAvailabilitySnapshotRuns(
  accessToken: string,
  limit = 10,
): Promise<AvailabilitySnapshotRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/availability-snapshot-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<AvailabilitySnapshotRunsResponse>(
    response,
    'Failed to load availability snapshot runs',
  )
}

export async function getProcurementCoordinationDashboard(
  accessToken: string,
  activeOnly = true,
): Promise<ProcurementCoordinationDashboardResponse> {
  const search = new URLSearchParams({ activeOnly: String(activeOnly) })
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination?${search}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<ProcurementCoordinationDashboardResponse>(
    response,
    'Failed to load procurement coordination dashboard',
  )
  return {
    ...raw,
    items: raw.items.map(normalizeProcurementCoordinationSummary),
  }
}

export async function getProcurementCoordinationSettings(
  accessToken: string,
): Promise<ProcurementCoordinationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementCoordinationSettingsResponse>(
    response,
    'Failed to load procurement coordination settings',
  )
}

export async function upsertProcurementCoordinationSettings(
  accessToken: string,
  payload: UpsertProcurementCoordinationSettingsRequest,
): Promise<ProcurementCoordinationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementCoordinationSettingsResponse>(
    response,
    'Failed to save procurement coordination settings',
  )
}

export async function getPendingProcurementCoordination(
  accessToken: string,
): Promise<PendingProcurementCoordinationResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingProcurementCoordinationResponse>(
    response,
    'Failed to load pending procurement coordination',
  )
}

export async function getProcurementCoordinationRuns(
  accessToken: string,
  limit = 5,
): Promise<ProcurementCoordinationRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/procurement-coordination-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementCoordinationRunsResponse>(
    response,
    'Failed to load procurement coordination runs',
  )
}

export async function getApprovalRemindersDashboard(
  accessToken: string,
  includeUpcoming = false,
): Promise<ApprovalRemindersDashboardResponse> {
  const search = new URLSearchParams({ includeUpcoming: String(includeUpcoming) })
  const response = await fetch(`${apiBase}/api/v1/approval-reminders?${search}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<ApprovalRemindersDashboardResponse>(
    response,
    'Failed to load approval reminders dashboard',
  )
  return normalizeApprovalRemindersDashboard(raw)
}

export async function getApprovalReminderSettings(
  accessToken: string,
): Promise<ApprovalReminderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ApprovalReminderSettingsResponse>(
    response,
    'Failed to load approval reminder settings',
  )
}

export async function upsertApprovalReminderSettings(
  accessToken: string,
  payload: UpsertApprovalReminderSettingsRequest,
): Promise<ApprovalReminderSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ApprovalReminderSettingsResponse>(
    response,
    'Failed to save approval reminder settings',
  )
}

export async function getPendingApprovalReminders(
  accessToken: string,
): Promise<PendingApprovalRemindersResponse> {
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingApprovalRemindersResponse>(
    response,
    'Failed to load pending approval reminders',
  )
}

export async function getApprovalReminderRuns(
  accessToken: string,
  limit = 5,
): Promise<ApprovalReminderRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/approval-reminder-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ApprovalReminderRunsResponse>(
    response,
    'Failed to load approval reminder runs',
  )
}

export async function getProcurementExceptionEscalationSettings(
  accessToken: string,
): Promise<ProcurementExceptionEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exception-escalation-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementExceptionEscalationSettingsResponse>(
    response,
    'Failed to load procurement exception escalation settings',
  )
}

export async function upsertProcurementExceptionEscalationSettings(
  accessToken: string,
  payload: UpsertProcurementExceptionEscalationSettingsRequest,
): Promise<ProcurementExceptionEscalationSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exception-escalation-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<ProcurementExceptionEscalationSettingsResponse>(
    response,
    'Failed to save procurement exception escalation settings',
  )
}

export async function getPendingProcurementExceptionEscalations(
  accessToken: string,
): Promise<PendingProcurementExceptionEscalationsResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exception-escalation-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingProcurementExceptionEscalationsResponse>(
    response,
    'Failed to load pending procurement exception escalations',
  )
}

export async function getPendingProcurementExceptionAutoCloses(
  accessToken: string,
): Promise<PendingProcurementExceptionAutoClosesResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exception-escalation-settings/auto-close/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingProcurementExceptionAutoClosesResponse>(
    response,
    'Failed to load pending procurement exception auto-closes',
  )
}

export async function getProcurementExceptionEscalationRuns(
  accessToken: string,
  limit = 5,
): Promise<ProcurementExceptionEscalationRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exception-escalation-settings/runs?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ProcurementExceptionEscalationRunsResponse>(
    response,
    'Failed to load procurement exception escalation runs',
  )
}

export async function getProcurementExceptionEscalationEvents(
  accessToken: string,
  limit = 10,
): Promise<ProcurementExceptionEscalationEventsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exception-escalation-settings/events?${search}`,
    {
      headers: authHeaders(accessToken),
    },
  )
  return parseJsonResponse<ProcurementExceptionEscalationEventsResponse>(
    response,
    'Failed to load procurement exception escalation events',
  )
}

export async function getDemandProcessingDashboard(
  accessToken: string,
): Promise<DemandProcessingDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingDashboardResponse>(
    response,
    'Failed to load demand processing dashboard',
  )
}

export async function getDemandProcessingDetail(
  accessToken: string,
  demandRefId: string,
): Promise<DemandProcessingDetailResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing/${demandRefId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingDetailResponse>(
    response,
    'Failed to load demand processing detail',
  )
}

export async function retryDemandProcessing(
  accessToken: string,
  demandRefId: string,
): Promise<DemandProcessingOperatorActionResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing/${demandRefId}/retry-processing`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingOperatorActionResponse>(
    response,
    'Failed to retry demand processing',
  )
}

export async function createDemandProcessingPrDraft(
  accessToken: string,
  demandRefId: string,
): Promise<DemandProcessingOperatorActionResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing/${demandRefId}/create-pr-draft`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingOperatorActionResponse>(
    response,
    'Failed to create purchase request draft',
  )
}

export async function getSupplyReadinessDashboard(
  accessToken: string,
): Promise<SupplyReadinessDashboardResponse> {
  const response = await fetch(`${apiBase}/api/v1/supply-readiness/dashboard`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplyReadinessDashboardResponse>(
    response,
    'Failed to load supply readiness dashboard',
  )
}

export async function getPartSupplyReadiness(
  accessToken: string,
  partId: string,
  quantity?: number,
): Promise<PartSupplyReadinessResponse> {
  const params = new URLSearchParams()
  if (quantity !== undefined) {
    params.set('quantity', String(quantity))
  }
  const query = params.toString()
  const response = await fetch(
    `${apiBase}/api/v1/supply-readiness/parts/${partId}${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<PartSupplyReadinessResponse>(
    response,
    'Failed to load part supply readiness',
  )
}

export async function getSupplierReadiness(
  accessToken: string,
  supplierId: string,
): Promise<SupplierSupplyReadinessResponse> {
  const response = await fetch(`${apiBase}/api/v1/supply-readiness/suppliers/${supplierId}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierSupplyReadinessResponse>(response, 'Failed to load supplier readiness')
  return normalizeSupplierReadiness(raw)
}

export async function getProcurementPathReadiness(
  accessToken: string,
  partId: string,
  supplierId: string,
  quantity?: number,
): Promise<ProcurementPathReadinessResponse> {
  const params = new URLSearchParams({ partId, supplierId })
  if (quantity !== undefined) {
    params.set('quantity', String(quantity))
  }
  const response = await fetch(`${apiBase}/api/v1/supply-readiness/procurement-path?${params}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawProcurementPathReadinessResponse>(response, 'Failed to load procurement path readiness')
  return normalizeProcurementPathReadiness(raw)
}

export async function getDemandProcessingSettings(
  accessToken: string,
): Promise<DemandProcessingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingSettingsResponse>(
    response,
    'Failed to load demand processing settings',
  )
}

export async function upsertDemandProcessingSettings(
  accessToken: string,
  payload: UpsertDemandProcessingSettingsRequest,
): Promise<DemandProcessingSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<DemandProcessingSettingsResponse>(
    response,
    'Failed to save demand processing settings',
  )
}

export async function getPendingDemandProcessing(
  accessToken: string,
): Promise<PendingDemandProcessingResponse> {
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings/pending`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PendingDemandProcessingResponse>(
    response,
    'Failed to load pending demand processing',
  )
}

export async function getDemandProcessingRuns(
  accessToken: string,
  limit = 5,
): Promise<DemandProcessingRunsResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/demand-processing-settings/runs?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<DemandProcessingRunsResponse>(
    response,
    'Failed to load demand processing runs',
  )
}

export async function getIntegrationEventSettings(
  accessToken: string,
): Promise<IntegrationEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationEventSettingsResponse>(
    response,
    'Failed to load integration event settings',
  )
}

export async function upsertIntegrationEventSettings(
  accessToken: string,
  payload: UpsertIntegrationEventSettingsRequest,
): Promise<IntegrationEventSettingsResponse> {
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings`, {
    method: 'PUT',
    headers: { ...authHeaders(accessToken), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  })
  return parseJsonResponse<IntegrationEventSettingsResponse>(
    response,
    'Failed to save integration event settings',
  )
}

export async function getIntegrationEventOutbox(
  accessToken: string,
  limit = 25,
): Promise<IntegrationEventsListResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings/outbox?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationEventsListResponse>(
    response,
    'Failed to load integration outbox events',
  )
}

export async function getIntegrationEventInbox(
  accessToken: string,
  limit = 25,
): Promise<IntegrationEventsListResponse> {
  const search = new URLSearchParams({ limit: String(limit) })
  const response = await fetch(`${apiBase}/api/v1/integration-event-settings/inbox?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<IntegrationEventsListResponse>(
    response,
    'Failed to load integration inbox events',
  )
}

export async function listAuditHistory(
  accessToken: string,
  options?: {
    limit?: number
    cursor?: string
    action?: string
    targetType?: string
    targetId?: string
    actorUserId?: string
    result?: string
    fromOccurredAt?: string
    toOccurredAt?: string
  },
): Promise<AuditHistoryListResponse> {
  const search = new URLSearchParams()
  if (options?.limit) {
    search.set('limit', String(options.limit))
  }
  if (options?.cursor) {
    search.set('cursor', options.cursor)
  }
  if (options?.action) {
    search.set('action', options.action)
  }
  if (options?.targetType) {
    search.set('targetType', options.targetType)
  }
  if (options?.targetId) {
    search.set('targetId', options.targetId)
  }
  if (options?.actorUserId) {
    search.set('actorUserId', options.actorUserId)
  }
  if (options?.result) {
    search.set('result', options.result)
  }
  if (options?.fromOccurredAt) {
    search.set('fromOccurredAt', options.fromOccurredAt)
  }
  if (options?.toOccurredAt) {
    search.set('toOccurredAt', options.toOccurredAt)
  }
  const query = search.toString()
  const response = await fetch(
    `${apiBase}/api/v1/audit-history${query ? `?${query}` : ''}`,
    { headers: authHeaders(accessToken) },
  )
  return parseJsonResponse<AuditHistoryListResponse>(response, 'Failed to load audit history')
}

export async function forgivingSearch(
  accessToken: string,
  options: { q: string; limit?: number },
): Promise<ForgivingSearchResponse> {
  const search = new URLSearchParams({ q: options.q })
  if (options.limit) {
    search.set('limit', String(options.limit))
  }
  const response = await fetch(`${apiBase}/api/v1/search/forgiving?${search}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ForgivingSearchResponse>(response, 'Failed to run forgiving search')
}

export async function getEmergencyPurchases(
  accessToken: string,
  status?: string,
): Promise<EmergencyPurchaseResponse[]> {
  const query = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/emergency-purchases${query}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeEmergencyPurchaseResponses(
    await parseJsonResponse<EmergencyPurchaseResponse[]>(response, 'Failed to load emergency purchases'),
  )
}

export async function listPendingEmergencyPurchases(
  accessToken: string,
): Promise<EmergencyPurchaseResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/emergency-purchases/pending`, {
    headers: authHeaders(accessToken),
  })
  return normalizeEmergencyPurchaseResponses(
    await parseJsonResponse<EmergencyPurchaseResponse[]>(
      response,
      'Failed to load pending emergency purchases',
    ),
  )
}

export async function createEmergencyPurchase(
  accessToken: string,
  payload: CreateEmergencyPurchaseRequest,
): Promise<EmergencyPurchaseResponse> {
  const response = await fetch(`${apiBase}/api/v1/emergency-purchases`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeEmergencyPurchaseResponse(
    await parseJsonResponse<EmergencyPurchaseResponse>(response, 'Failed to create emergency purchase'),
  )
}

export async function expeditedSubmitEmergencyPurchase(
  accessToken: string,
  purchaseRequestId: string,
  notes?: string,
): Promise<EmergencyPurchaseResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/emergency-purchases/${purchaseRequestId}/expedited-submit`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ notes: notes ?? null }),
    },
  )
  return normalizeEmergencyPurchaseResponse(
    await parseJsonResponse<EmergencyPurchaseResponse>(
      response,
      'Failed to expedited-submit emergency purchase',
    ),
  )
}

export async function managerOverrideApproveEmergencyPurchase(
  accessToken: string,
  purchaseRequestId: string,
  justification: string,
): Promise<EmergencyPurchaseResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/emergency-purchases/${purchaseRequestId}/manager-override-approve`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ justification }),
    },
  )
  return normalizeEmergencyPurchaseResponse(
    await parseJsonResponse<EmergencyPurchaseResponse>(
      response,
      'Failed to manager-override approve emergency purchase',
    ),
  )
}

export async function issueEmergencyPurchaseOrder(
  accessToken: string,
  purchaseRequestId: string,
  orderKey: string,
): Promise<IssueEmergencyPurchaseOrderResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/emergency-purchases/${purchaseRequestId}/issue-purchase-order`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify({ orderKey, title: null, notes: null }),
    },
  )
  const raw = await parseJsonResponse<IssueEmergencyPurchaseOrderResponse>(
    response,
    'Failed to issue emergency purchase order',
  )
  return {
    ...raw,
    emergencyPurchase: raw.emergencyPurchase
      ? normalizeEmergencyPurchaseResponse(raw.emergencyPurchase)
      : (raw.emergencyPurchase as IssueEmergencyPurchaseOrderResponse['emergencyPurchase']),
    purchaseOrder: raw.purchaseOrder
      ? normalizePurchaseOrderResponse(raw.purchaseOrder)
      : (raw.purchaseOrder as IssueEmergencyPurchaseOrderResponse['purchaseOrder']),
  }
}

export async function getSupplierOnboardingDocumentRequirements(
  accessToken: string,
): Promise<SupplierOnboardingDocumentRequirementsResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/document-requirements`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<SupplierOnboardingDocumentRequirementsResponse>(
    response,
    'Failed to load onboarding document requirements',
  )
}

export async function listPendingSupplierOnboarding(
  accessToken: string,
): Promise<SupplierOnboardingResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/pending`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierOnboardings(
    await parseJsonResponse<SupplierOnboardingResponse[]>(
      response,
      'Failed to load pending supplier onboarding',
    ),
  )
}

export async function startSupplierOnboarding(
  accessToken: string,
  supplierId: string,
  notes?: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/start`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ supplierId, notes: notes ?? null }),
  })
  return normalizeSupplierOnboarding(
    await parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to start supplier onboarding'),
  )
}

export async function getSupplierOnboarding(
  accessToken: string,
  supplierId: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/suppliers/${supplierId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierOnboarding(
    await parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to load supplier onboarding'),
  )
}

export async function submitSupplierOnboarding(
  accessToken: string,
  supplierId: string,
  notes?: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/suppliers/${supplierId}/submit`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ notes: notes ?? null }),
  })
  return normalizeSupplierOnboarding(
    await parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to submit supplier onboarding'),
  )
}

export async function approveSupplierOnboarding(
  accessToken: string,
  supplierId: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/suppliers/${supplierId}/approve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierOnboarding(
    await parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to approve supplier onboarding'),
  )
}

export async function rejectSupplierOnboarding(
  accessToken: string,
  supplierId: string,
  reason: string,
): Promise<SupplierOnboardingResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-onboarding/suppliers/${supplierId}/reject`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ reason }),
  })
  return normalizeSupplierOnboarding(
    await parseJsonResponse<SupplierOnboardingResponse>(response, 'Failed to reject supplier onboarding'),
  )
}

export async function listSupplierComplianceDocuments(
  accessToken: string,
  supplierId: string,
): Promise<SupplierComplianceDocumentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/suppliers/${supplierId}/compliance-documents`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierComplianceDocuments(
    await parseJsonResponse<SupplierComplianceDocumentResponse[]>(
      response,
      'Failed to load supplier compliance documents',
    ),
  )
}

export async function registerSupplierComplianceDocument(
  accessToken: string,
  supplierId: string,
  payload: {
    documentKey: string
    documentTypeKey: string
    title: string
    expiresAt?: string | null
    effectiveAt?: string | null
    fileName: string
    contentType: string
    sizeBytes: number
    notes: string
    contentBase64?: string | null
  },
): Promise<SupplierComplianceDocumentResponse> {
  const response = await fetch(`${apiBase}/api/v1/suppliers/${supplierId}/compliance-documents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeSupplierComplianceDocument(
    await parseJsonResponse<SupplierComplianceDocumentResponse>(
      response,
      'Failed to register compliance document',
    ),
  )
}

export async function approveSupplierComplianceDocument(
  accessToken: string,
  supplierId: string,
  documentId: string,
): Promise<SupplierComplianceDocumentResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/suppliers/${supplierId}/compliance-documents/${documentId}/approve`,
    { method: 'POST', headers: authHeaders(accessToken) },
  )
  return normalizeSupplierComplianceDocument(
    await parseJsonResponse<SupplierComplianceDocumentResponse>(
      response,
      'Failed to approve compliance document',
    ),
  )
}

export async function listSupplierRestrictions(
  accessToken: string,
  options?: { status?: string } | string,
): Promise<SupplierRestrictionResponse[]> {
  const status = typeof options === 'string' ? options : options?.status
  const search = status ? `?status=${encodeURIComponent(status)}` : ''
  const response = await fetch(`${apiBase}/api/v1/supplier-restrictions${search}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierRestrictionResponse[]>(response, 'Failed to load supplier restrictions')
  return normalizeSupplierRestrictions(raw)
}

export async function listSupplierRestrictionsBySupplier(
  accessToken: string,
  supplierId: string,
): Promise<SupplierRestrictionResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/suppliers/${supplierId}/restrictions`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierRestrictionResponse[]>(response, 'Failed to load supplier restrictions')
  return normalizeSupplierRestrictions(raw)
}

export async function getSupplierRestrictionEnforcement(
  accessToken: string,
  supplierId: string,
): Promise<SupplierRestrictionEnforcementResponse> {
  const response = await fetch(`${apiBase}/api/v1/suppliers/${supplierId}/restrictions/enforcement`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierRestrictionEnforcementResponse>(response, 'Failed to load supplier restriction enforcement')
  return normalizeSupplierRestrictionEnforcement(raw)
}

export async function createSupplierRestriction(
  accessToken: string,
  supplierId: string,
  payload: CreateSupplierRestrictionRequest,
): Promise<SupplierRestrictionResponse> {
  const response = await fetch(`${apiBase}/api/v1/suppliers/${supplierId}/restrictions`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  const raw = await parseJsonResponse<RawSupplierRestrictionResponse>(response, 'Failed to create supplier restriction')
  return normalizeSupplierRestriction(raw)
}

export async function liftSupplierRestriction(
  accessToken: string,
  restrictionId: string,
  payload: LiftSupplierRestrictionRequest,
): Promise<SupplierRestrictionResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-restrictions/${restrictionId}/lift`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  const raw = await parseJsonResponse<RawSupplierRestrictionResponse>(response, 'Failed to lift supplier restriction')
  return normalizeSupplierRestriction(raw)
}

export async function listRestrictionsForSupplier(
  accessToken: string,
  supplierId: string,
): Promise<SupplierRestrictionResponse[]> {
  return await listSupplierRestrictionsBySupplier(accessToken, supplierId)
}

export async function listSupplierIncidents(
  accessToken: string,
  options?: { status?: string; supplierId?: string; severity?: string },
): Promise<SupplierIncidentResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) search.set('status', options.status)
  if (options?.supplierId) search.set('supplierId', options.supplierId)
  if (options?.severity) search.set('severity', options.severity)
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse[]>(response, 'Failed to load supplier incidents')
  return normalizeSupplierIncidents(raw)
}

export async function listSupplierIncidentsForSupplier(
  accessToken: string,
  supplierId: string,
): Promise<SupplierIncidentResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/suppliers/${supplierId}/supplier-incidents`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse[]>(response, 'Failed to load supplier incidents for supplier record')
  return normalizeSupplierIncidents(raw)
}

export const getSupplierOnboardingBySupplier = getSupplierOnboarding
export const submitSupplierOnboardingForSupplier = submitSupplierOnboarding
export const approveSupplierOnboardingForSupplier = approveSupplierOnboarding
export const rejectSupplierOnboardingForSupplier = rejectSupplierOnboarding

export async function createSupplierIncident(
  accessToken: string,
  payload: CreateSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse>(response, 'Failed to create supplier incident')
  return normalizeSupplierIncident(raw)
}

export async function startSupplierIncidentInvestigation(
  accessToken: string,
  incidentId: string,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/start-investigation`, {
    method: 'POST',
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse>(response, 'Failed to start investigation')
  return normalizeSupplierIncident(raw)
}

export async function resolveSupplierIncident(
  accessToken: string,
  incidentId: string,
  payload: ResolveSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse>(response, 'Failed to resolve supplier incident')
  return normalizeSupplierIncident(raw)
}

export async function closeSupplierIncident(
  accessToken: string,
  incidentId: string,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/close`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify({ resolutionNotes: null }),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse>(response, 'Failed to close supplier incident')
  return normalizeSupplierIncident(raw)
}

export async function cancelSupplierIncident(
  accessToken: string,
  incidentId: string,
  payload: CancelSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse>(response, 'Failed to cancel supplier incident')
  return normalizeSupplierIncident(raw)
}

export async function reopenSupplierIncident(
  accessToken: string,
  incidentId: string,
  payload: ReopenSupplierIncidentRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(`${apiBase}/api/v1/supplier-incidents/${incidentId}/reopen`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  const raw = await parseJsonResponse<RawSupplierIncidentResponse>(response, 'Failed to reopen supplier incident')
  return normalizeSupplierIncident(raw)
}

export async function applySupplierIncidentProcurementRestriction(
  accessToken: string,
  incidentId: string,
  payload: ApplySupplierIncidentProcurementRestrictionRequest,
): Promise<SupplierIncidentResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/supplier-incidents/${incidentId}/apply-procurement-restriction`,
    {
      method: 'POST',
      headers: authHeaders(accessToken),
      body: JSON.stringify(payload),
    },
  )
  const raw = await parseJsonResponse<RawSupplierIncidentResponse>(
    response,
    'Failed to apply procurement restriction from incident',
  )
  return normalizeSupplierIncident(raw)
}

export async function listProcurementExceptionResolutionTemplates(
  accessToken: string,
): Promise<ProcurementExceptionResolutionTemplateResponse[]> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/resolution-templates`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementExceptionResolutionTemplateResponse[]>(
    response,
    'Failed to load procurement exception resolution templates',
  )
}

export async function listProcurementExceptions(
  accessToken: string,
  options?: {
    status?: string
    subjectType?: string
    subjectId?: string
    overdueOnly?: boolean
  },
): Promise<ProcurementExceptionResponse[]> {
  const search = new URLSearchParams()
  if (options?.status) search.set('status', options.status)
  if (options?.subjectType) search.set('subjectType', options.subjectType)
  if (options?.subjectId) search.set('subjectId', options.subjectId)
  if (options?.overdueOnly) search.set('overdueOnly', 'true')
  const query = search.toString()
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions${query ? `?${query}` : ''}`, {
    headers: authHeaders(accessToken),
  })
  return (await parseJsonResponse<ProcurementExceptionResponse[]>(
    response,
    'Failed to load procurement exceptions',
  )).map(normalizeProcurementExceptionResponse)
}

export async function listSubjectProcurementExceptions(
  accessToken: string,
  subjectType: 'purchase_request' | 'purchase_order' | 'rfq',
  subjectId: string,
): Promise<ProcurementExceptionResponse[]> {
  const route =
    subjectType === 'purchase_request'
      ? `/api/v1/purchase-requests/${subjectId}/procurement-exceptions`
      : subjectType === 'purchase_order'
        ? `/api/v1/purchase-orders/${subjectId}/procurement-exceptions`
        : `/api/v1/rfqs/${subjectId}/procurement-exceptions`
  const response = await fetch(`${apiBase}${route}`, { headers: authHeaders(accessToken) })
  return (await parseJsonResponse<ProcurementExceptionResponse[]>(
    response,
    'Failed to load subject procurement exceptions',
  )).map(normalizeProcurementExceptionResponse)
}

export async function createSubjectProcurementException(
  accessToken: string,
  subjectType: 'purchase_request' | 'purchase_order' | 'rfq',
  subjectId: string,
  payload: CreateProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const route =
    subjectType === 'purchase_request'
      ? `/api/v1/purchase-requests/${subjectId}/procurement-exceptions`
      : subjectType === 'purchase_order'
        ? `/api/v1/purchase-orders/${subjectId}/procurement-exceptions`
        : `/api/v1/rfqs/${subjectId}/procurement-exceptions`
  const response = await fetch(`${apiBase}${route}`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to create procurement exception',
  ))
}

export async function assignProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: AssignProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/assign`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to assign procurement exception',
  ))
}

export async function linkProcurementExceptionActions(
  accessToken: string,
  exceptionId: string,
  payload: LinkProcurementExceptionActionsRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/link-actions`, {
    method: 'PUT',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to link procurement exception actions',
  ))
}

export async function startProcurementExceptionInvestigation(
  accessToken: string,
  exceptionId: string,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/start-investigation`,
    { method: 'POST', headers: authHeaders(accessToken) },
  )
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to start procurement exception investigation',
  ))
}

export async function resolveProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: ResolveProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/resolve`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to resolve procurement exception',
  ))
}

export async function requestProcurementExceptionWaive(
  accessToken: string,
  exceptionId: string,
  payload: RequestProcurementExceptionWaiveRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/request-waive`,
    { method: 'POST', headers: authHeaders(accessToken), body: JSON.stringify(payload) },
  )
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to request procurement exception waive',
  ))
}

export async function approveProcurementExceptionWaive(
  accessToken: string,
  exceptionId: string,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/approve-waive`,
    { method: 'POST', headers: authHeaders(accessToken) },
  )
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to approve procurement exception waive',
  ))
}

export async function rejectProcurementExceptionWaive(
  accessToken: string,
  exceptionId: string,
  payload: RejectProcurementExceptionWaiveRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(
    `${apiBase}/api/v1/procurement-exceptions/${exceptionId}/reject-waive`,
    { method: 'POST', headers: authHeaders(accessToken), body: JSON.stringify(payload) },
  )
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to reject procurement exception waive',
  ))
}

export async function closeProcurementException(
  accessToken: string,
  exceptionId: string,
  payload?: CloseProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/close`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload ?? { resolutionNotes: null }),
  })
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to close procurement exception',
  ))
}

export async function cancelProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: CancelProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/cancel`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to cancel procurement exception',
  ))
}

export async function reopenProcurementException(
  accessToken: string,
  exceptionId: string,
  payload: ReopenProcurementExceptionRequest,
): Promise<ProcurementExceptionResponse> {
  const response = await fetch(`${apiBase}/api/v1/procurement-exceptions/${exceptionId}/reopen`, {
    method: 'POST',
    headers: authHeaders(accessToken),
    body: JSON.stringify(payload),
  })
  return normalizeProcurementExceptionResponse(await parseJsonResponse<ProcurementExceptionResponse>(
    response,
    'Failed to reopen procurement exception',
  ))
}

export async function getProcurementApprovalAuthority(
  accessToken: string,
): Promise<ProcurementApprovalAuthorityMirrorResponse> {
  const response = await fetch(`${apiBase}/api/me/procurement-approval-authority`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<ProcurementApprovalAuthorityMirrorResponse>(
    response,
    'Failed to load StaffArr procurement approval authority',
  )
}

export async function getSupplierReportSummary(
  accessToken: string,
  options?: { approvalStatus?: string; activeOnly?: boolean },
): Promise<SupplierReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.approvalStatus) params.set('approvalStatus', options.approvalStatus)
  if (options?.activeOnly) params.set('activeOnly', 'true')
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/suppliers/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierReportSummary(
    await parseJsonResponse<SupplierReportSummaryResponse>(
      response,
      'Failed to load supplier report summary',
    ),
  )
}

export async function getSupplierReportDetail(
  accessToken: string,
  supplierId: string,
): Promise<SupplierReportDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/suppliers/${supplierId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierReportDetail(
    await parseJsonResponse<SupplierReportDetailResponse>(
      response,
      'Failed to load supplier report detail',
    ),
  )
}

export function exportSupplierReportSummaryCsv(
  accessToken: string,
  options?: { approvalStatus?: string; activeOnly?: boolean },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.approvalStatus) params.set('approvalStatus', options.approvalStatus)
  if (options?.activeOnly) params.set('activeOnly', 'true')
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(
    accessToken,
    `/api/reports/suppliers/summary/export${query}`,
    'Supplier report export failed',
  )
}

export async function getComplianceReportSummary(
  accessToken: string,
  options?: {
    attentionOnly?: boolean
    supplierId?: string
    reviewStatus?: string
  },
): Promise<SupplierComplianceReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.attentionOnly) params.set('attentionOnly', 'true')
  if (options?.supplierId) params.set('supplierId', options.supplierId)
  if (options?.reviewStatus) params.set('reviewStatus', options.reviewStatus)
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/compliance/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  const raw = await parseJsonResponse<SupplierComplianceReportSummaryResponse>(
    response,
    'Failed to load compliance report summary',
  )
  return {
    generatedAt: raw.generatedAt,
    totals: {
      supplierCount: raw.totals.supplierCount,
      documentCount: raw.totals.documentCount,
      expiredCount: raw.totals.expiredCount,
      expiringSoonCount: raw.totals.expiringSoonCount,
      reviewPendingCount: raw.totals.reviewPendingCount,
      approvedCount: raw.totals.approvedCount,
      rejectedCount: raw.totals.rejectedCount,
    },
    suppliers: raw.suppliers.map((supplier) => ({
      ...supplier,
      parentSupplierId: supplier.parentSupplierId ?? null,
      parentSupplierDisplayName: supplier.parentSupplierDisplayName ?? null,
      supplierUnitKind: supplier.supplierUnitKind ?? 'identity',
      supplierServiceTypes: supplier.supplierServiceTypes ?? [],
    })),
    documents: raw.documents.map((document) => ({
      ...document,
    })),
  }
}

export async function getComplianceSupplierDetail(
  accessToken: string,
  supplierId: string,
): Promise<SupplierComplianceDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/compliance/suppliers/${supplierId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizeSupplierComplianceDetail(
    await parseJsonResponse<SupplierComplianceDetailResponse>(
      response,
      'Failed to load supplier compliance detail',
    ),
  )
}

export function exportComplianceReportSummaryCsv(
  accessToken: string,
  options?: {
    attentionOnly?: boolean
    supplierId?: string
    reviewStatus?: string
  },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.attentionOnly) params.set('attentionOnly', 'true')
  if (options?.supplierId) params.set('supplierId', options.supplierId)
  if (options?.reviewStatus) params.set('reviewStatus', options.reviewStatus)
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(
    accessToken,
    `/api/reports/compliance/summary/export${query}`,
    'Compliance report export failed',
  )
}

export async function getPartsInventoryReportSummary(
  accessToken: string,
  options?: {
    partStatus?: string
    activePartsOnly?: boolean
    belowReorderOnly?: boolean
    inventoryLocationId?: string
  },
): Promise<PartsInventoryReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.partStatus) params.set('partStatus', options.partStatus)
  if (options?.activePartsOnly) params.set('activePartsOnly', 'true')
  if (options?.belowReorderOnly) params.set('belowReorderOnly', 'true')
  if (options?.inventoryLocationId) params.set('inventoryLocationId', options.inventoryLocationId)
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/parts-inventory/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return normalizePartsInventoryReportSummary(
    await parseJsonResponse<PartsInventoryReportSummaryResponse>(
      response,
      'Failed to load parts inventory report summary',
    ),
  )
}

export async function getPartsInventoryPartDetail(
  accessToken: string,
  partId: string,
): Promise<PartsInventoryPartDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/parts-inventory/parts/${partId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizePartsInventoryPartDetail(
    await parseJsonResponse<PartsInventoryPartDetailResponse>(
      response,
      'Failed to load parts inventory part detail',
    ),
  )
}

export async function getPartsInventoryLocationDetail(
  accessToken: string,
  inventoryLocationId: string,
): Promise<PartsInventoryLocationDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/parts-inventory/locations/${inventoryLocationId}`, {
    headers: authHeaders(accessToken),
  })
  return parseJsonResponse<PartsInventoryLocationDetailResponse>(
    response,
    'Failed to load parts inventory location detail',
  )
}

export function exportPartsInventoryReportSummaryCsv(
  accessToken: string,
  options?: {
    partStatus?: string
    activePartsOnly?: boolean
    belowReorderOnly?: boolean
    inventoryLocationId?: string
  },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.partStatus) params.set('partStatus', options.partStatus)
  if (options?.activePartsOnly) params.set('activePartsOnly', 'true')
  if (options?.belowReorderOnly) params.set('belowReorderOnly', 'true')
  if (options?.inventoryLocationId) params.set('inventoryLocationId', options.inventoryLocationId)
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(
    accessToken,
    `/api/reports/parts-inventory/summary/export${query}`,
    'Parts inventory report export failed',
  )
}

export async function getPurchasingReportSummary(
  accessToken: string,
  options?: { openDocumentsOnly?: boolean; supplierId?: string },
): Promise<PurchasingReportSummaryResponse> {
  const params = new URLSearchParams()
  if (options?.openDocumentsOnly) params.set('openDocumentsOnly', 'true')
  if (options?.supplierId) params.set('supplierId', options.supplierId)
  const query = params.size > 0 ? `?${params.toString()}` : ''
  const response = await fetch(`${apiBase}/api/reports/purchasing/summary${query}`, {
    headers: authHeaders(accessToken),
  })
  return normalizePurchasingReportSummary(
    await parseJsonResponse<PurchasingReportSummaryResponse>(
      response,
      'Failed to load purchasing report summary',
    ),
  )
}

export async function getPurchasingPurchaseRequestDetail(
  accessToken: string,
  purchaseRequestId: string,
): Promise<PurchasingPurchaseRequestDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/purchasing/purchase-requests/${purchaseRequestId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizePurchasingPurchaseRequestDetail(
    await parseJsonResponse<PurchasingPurchaseRequestDetailResponse>(
      response,
      'Failed to load purchasing purchase request detail',
    ),
  )
}

export async function getPurchasingPurchaseOrderDetail(
  accessToken: string,
  purchaseOrderId: string,
): Promise<PurchasingPurchaseOrderDetailResponse> {
  const response = await fetch(`${apiBase}/api/reports/purchasing/purchase-orders/${purchaseOrderId}`, {
    headers: authHeaders(accessToken),
  })
  return normalizePurchasingPurchaseOrderDetail(
    await parseJsonResponse<PurchasingPurchaseOrderDetailResponse>(
      response,
      'Failed to load purchasing purchase order detail',
    ),
  )
}

export function exportPurchasingReportSummaryCsv(
  accessToken: string,
  options?: { openDocumentsOnly?: boolean; supplierId?: string },
): Promise<Blob> {
  const params = new URLSearchParams()
  if (options?.openDocumentsOnly) params.set('openDocumentsOnly', 'true')
  if (options?.supplierId) params.set('supplierId', options.supplierId)
  const query = params.size > 0 ? `?${params.toString()}` : ''
  return downloadExportBlob(
    accessToken,
    `/api/reports/purchasing/summary/export${query}`,
    'Purchasing report export failed',
  )
}
