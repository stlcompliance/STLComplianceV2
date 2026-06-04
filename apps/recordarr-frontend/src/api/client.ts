export type RecordArrHandoffSessionResponse = {
  accessToken: string
  accessTokenExpiresAt: string
  userId: string
  personId: string
  email: string
  displayName: string
  tenantId: string
  tenantSlug: string
  tenantDisplayName: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  entitlements: string[]
}

export type RecordArrSessionBootstrapResponse = {
  userId: string
  personId: string
  tenantId: string
  sessionId: string
  tenantRoleKey: string
  isPlatformAdmin: boolean
  productKey: string
  hasRecordArrEntitlement: boolean
  entitlements: string[]
}

export type RecordArrDashboardResponse = {
  generatedAt: string
  recordCount: number
  activeCount: number
  reviewCount: number
  uploadSessionCount: number
  packageCount: number
  controlledDocumentCount: number
  legalHoldCount: number
  recentRecords: RecordArrRecord[]
  openPackages: RecordArrPackage[]
  controlledDocuments: RecordArrControlledDocument[]
  legalHolds: RecordArrLegalHold[]
}

export type RecordArrRecord = {
  recordId: string
  recordNumber: string
  title: string
  description: string
  recordType: string
  documentType: string
  status: string
  classification: string
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
  sourceObjectDisplayName: string
  ownerPersonId: string
  uploadedByPersonId: string | null
  uploadedAt: string
  effectiveAt: string | null
  expiresAt: string | null
  currentFileName: string
  currentMimeType: string
  versionNumber: number
  tags: string[]
}

export type RecordArrUploadSession = {
  uploadSessionId: string
  uploadSessionNumber: string
  sessionType: string
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
  uploadPurpose: string
  status: string
  requiresDocumentScan: boolean
  requiresOcr: boolean
  requiresManualReview: boolean
  createdAt: string
  expiresAt: string
  completedAt: string | null
  revokedAt: string | null
  allowedMimeTypes: string[]
  maxUploads: number
  maxFileSizeBytes: number
  uploadedRecordRefs: string[]
}

export type RecordArrScanProcessing = {
  scanProcessingId: string
  recordId: string
  originalFileName: string
  status: string
  scanPurpose: string
  edgeCoordinates: string | null
  generatedPdfRecordRef: string | null
  ocrResultId: string | null
  extractionResultId: string | null
  confidenceScore: number
  processedAt: string | null
  failureReason: string | null
}

export type RecordArrOcrResult = {
  ocrResultId: string
  recordId: string
  fileId: string
  engine: string
  status: string
  language: string
  confidenceScore: number
  fullText: string
  extractedAt: string
  failureReason: string | null
}

export type RecordArrExtractedField = {
  extractedFieldId: string
  extractionResultId: string
  fieldKey: string
  label: string
  value: string
  valueType: string
  confidenceScore: number
  reviewStatus: string
  correctedValue: string | null
  correctedByPersonId: string | null
  correctedAt: string | null
}

export type RecordArrExtractionResult = {
  extractionResultId: string
  recordId: string
  extractionType: string
  status: string
  extractedFields: RecordArrExtractedField[]
  confidenceScore: number
  extractedAt: string
  reviewedByPersonId: string | null
  reviewedAt: string | null
  failureReason: string | null
}

export type RecordArrEvidenceMapping = {
  evidenceMappingId: string
  recordId: string
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
  complianceRequirementRef: string
  evidenceTypeKey: string
  status: string
  mappingSource: string
  confidenceScore: number
  confirmedByPersonId: string | null
  confirmedAt: string | null
  rejectedByPersonId: string | null
  rejectedAt: string | null
  rejectionReason: string | null
  notes: string | null
}

export type RecordArrPackage = {
  packageId: string
  packageNumber: string
  title: string
  packageType: string
  status: string
  sourceProduct: string
  sourceObjectRefs: string[]
  recordRefs: string[]
  manifestChecksum: string | null
  generatedPdfRecordRef: string | null
  generatedZipFileRef: string | null
  createdAt: string
  completedAt: string | null
  lockedAt: string | null
  archivedAt: string | null
  expiresAt: string | null
}

export type RecordArrPackageManifest = {
  manifestId: string
  packageId: string
  manifestVersion: number
  generatedAt: string
  recordEntries: RecordArrManifestEntry[]
  sourceObjectEntries: RecordArrManifestEntry[]
  requirementEntries: RecordArrManifestEntry[]
  checksum: string
  generatedByPersonId: string
}

export type RecordArrManifestEntry = {
  entryId: string
  entryType: string
  displayName: string
  sourceProduct: string | null
  sourceObjectRef: string | null
  recordRef: string | null
  complianceRequirementRef: string | null
  statusSnapshot: string | null
  checksum: string
}

export type RecordArrRetentionPolicy = {
  retentionPolicyId: string
  policyKey: string
  title: string
  description: string
  recordTypeApplicability: string
  documentTypeApplicability: string
  sourceProductApplicability: string
  retainFor: number
  retentionUnit: string
  retentionStartTrigger: string
  disposalAction: string
  legalHoldOverrides: boolean
  status: string
  createdAt: string
  updatedAt: string
}

export type RecordArrRetentionStatus = {
  retentionStatusId: string
  recordId: string
  retentionPolicyRef: string
  status: string
  retentionStartAt: string
  retentionExpiresAt: string | null
  nextReviewAt: string | null
  lastReviewedAt: string | null
  reviewedByPersonId: string | null
  disposalReviewRef: string | null
}

export type RecordArrDisposalReview = {
  disposalReviewId: string
  recordId: string
  retentionStatusRef: string
  proposedAction: string
  status: string
  requestedAt: string
  requestedByPersonId: string
  reviewedByPersonId: string | null
  reviewedAt: string | null
  decisionReason: string | null
  completedAt: string | null
}

export type RecordArrLegalHold = {
  legalHoldId: string
  holdNumber: string
  title: string
  description: string
  status: string
  holdType: string
  scopeRules: string[]
  recordRefs: string[]
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
  createdAt: string
  createdByPersonId: string
  activatedAt: string | null
  releasedAt: string | null
  releasedByPersonId: string | null
  releaseReason: string | null
}

export type RecordArrAuditTrailEntry = {
  auditTrailEntryId: string
  action: string
  actorPersonId: string
  occurredAt: string
  details: string
}

export type RecordArrControlledDocument = {
  controlledDocumentId: string
  documentNumber: string
  recordId: string
  title: string
  description: string
  controlledDocumentType: string
  status: string
  ownerPersonId: string
  departmentOrgUnitId: string
  staffarrSiteId: string
  currentVersionId: string
  reviewIntervalDays: number
  nextReviewAt: string | null
  effectiveAt: string | null
  expiresAt: string | null
  supersedesDocumentRef: string | null
  supersededByDocumentRef: string | null
  acknowledgementRequired: boolean
  relatedRecordRefs: string[]
  auditTrail: RecordArrAuditTrailEntry[]
}

export type RecordArrControlledDocumentVersion = {
  versionId: string
  controlledDocumentId: string
  versionNumber: number
  versionLabel: string
  status: string
  fileName: string
  createdAt: string
  createdByPersonId: string
  submittedForReviewAt: string | null
  approvedAt: string | null
  approvedByPersonId: string | null
  effectiveAt: string | null
  supersededAt: string | null
  changeSummary: string | null
  previousVersionRef: string | null
  nextVersionRef: string | null
}

export type RecordArrDocumentReview = {
  documentReviewId: string
  controlledDocumentId: string
  versionId: string
  reviewType: string
  status: string
  requestedByPersonId: string
  reviewerPersonId: string
  requestedAt: string
  dueAt: string | null
  reviewedAt: string | null
  decisionReason: string | null
  comments: string | null
}

export type RecordArrDocumentDistribution = {
  distributionId: string
  controlledDocumentId: string
  versionId: string
  distributionType: string
  targetRef: string
  status: string
  distributedAt: string | null
  acknowledgedAt: string | null
  acknowledgementRef: string | null
}

export type RecordArrDocumentAcknowledgement = {
  acknowledgementId: string
  controlledDocumentId: string
  versionId: string
  personId: string
  status: string
  acknowledgedAt: string | null
  signatureRecordRef: string | null
  attestationText: string | null
  dueAt: string | null
}

export type RecordArrAccessPolicy = {
  accessPolicyId: string
  recordId: string
  policyType: string
  status: string
  readRules: string[]
  writeRules: string[]
  downloadRules: string[]
  shareRules: string[]
  exportRules: string[]
  purgeRules: string[]
}

export type RecordArrAccessGrant = {
  accessGrantId: string
  recordId: string
  granteeType: string
  granteeRef: string
  permission: string
  status: string
  grantedByPersonId: string
  grantedAt: string
  expiresAt: string | null
  revokedAt: string | null
  revokeReason: string | null
}

export type RecordArrExternalShare = {
  externalShareId: string
  shareNumber: string
  recordId: string
  sharePurpose: string
  status: string
  recipientName: string
  recipientEmail: string
  allowedActions: string[]
  createdAt: string
  createdByPersonId: string
  expiresAt: string | null
  revokedAt: string | null
  revokedByPersonId: string | null
  lastAccessedAt: string | null
  accessCount: number
}

export type RecordArrRedaction = {
  redactionId: string
  sourceRecordId: string
  redactedRecordId: string
  redactionReason: string
  status: string
  redactedByPersonId: string
  redactedAt: string
  redactionRules: string[]
}

export type RecordArrAccessLog = {
  accessLogId: string
  recordId: string
  action: string
  result: string
  actorPersonId: string | null
  actorServiceClientId: string | null
  externalShareId: string | null
  occurredAt: string
  sourceIp: string | null
  userAgent: string | null
  reasonCode: string | null
}

const apiBase = import.meta.env.VITE_RECORDARR_API_BASE ?? ''

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

export async function redeemHandoff(handoffCode: string): Promise<RecordArrHandoffSessionResponse> {
  const response = await fetch(`${apiBase}/api/auth/nexarr/redeem`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ handoffCode }),
  })
  return parseJsonResponse<RecordArrHandoffSessionResponse>(response, 'Handoff redeem failed')
}

export async function getSessionBootstrap(
  accessToken: string,
): Promise<RecordArrSessionBootstrapResponse> {
  return getJson<RecordArrSessionBootstrapResponse>('/api/session', accessToken)
}

export async function getDashboard(accessToken: string): Promise<RecordArrDashboardResponse> {
  return getJson<RecordArrDashboardResponse>('/api/v1/workspace/summary', accessToken)
}

export async function listRecords(accessToken: string, search?: string): Promise<RecordArrRecord[]> {
  const path = search && search.trim().length > 0
    ? `/api/v1/workspace/records?search=${encodeURIComponent(search.trim())}`
    : '/api/v1/workspace/records'
  return getJson<RecordArrRecord[]>(path, accessToken)
}

export async function getRecord(accessToken: string, recordId: string): Promise<RecordArrRecord> {
  return getJson<RecordArrRecord>(`/api/v1/workspace/records/${encodeURIComponent(recordId)}`, accessToken)
}

export async function createRecord(
  accessToken: string,
  body: {
    title: string
    description: string
    recordType: string
    documentType: string
    sourceProduct: string
    sourceObjectType: string
    sourceObjectId: string
    sourceObjectDisplayName: string
    ownerPersonId: string
    uploadedByPersonId: string
    currentFileName: string
    currentMimeType: string
  },
): Promise<RecordArrRecord> {
  return sendJson<RecordArrRecord>('/api/v1/workspace/records', accessToken, 'POST', body)
}

export async function updateRecord(
  accessToken: string,
  recordId: string,
  body: { status: string; classification?: string | null; effectiveAt?: string | null; expiresAt?: string | null },
): Promise<RecordArrRecord> {
  return sendJson<RecordArrRecord>(`/api/v1/workspace/records/${encodeURIComponent(recordId)}`, accessToken, 'PATCH', body)
}

export async function listUploadSessions(accessToken: string): Promise<RecordArrUploadSession[]> {
  return getJson<RecordArrUploadSession[]>('/api/v1/workspace/upload-sessions', accessToken)
}

export async function createUploadSession(
  accessToken: string,
  body: {
    sourceProduct: string
    sourceObjectType: string
    sourceObjectId: string
    uploadPurpose: string
    requiresDocumentScan: boolean
    requiresOcr: boolean
    requiresManualReview: boolean
  },
): Promise<RecordArrUploadSession> {
  return sendJson<RecordArrUploadSession>('/api/v1/workspace/upload-sessions', accessToken, 'POST', body)
}

export async function listScans(accessToken: string): Promise<RecordArrScanProcessing[]> {
  return getJson<RecordArrScanProcessing[]>('/api/v1/workspace/document-scans', accessToken)
}

export async function createScan(
  accessToken: string,
  body: { recordId: string; originalFileName: string; scanPurpose: string },
): Promise<RecordArrScanProcessing> {
  return sendJson<RecordArrScanProcessing>('/api/v1/workspace/document-scans', accessToken, 'POST', body)
}

export async function applyManualCorrection(
  accessToken: string,
  scanProcessingId: string,
  body: { edgeCoordinates: string },
): Promise<RecordArrScanProcessing> {
  return sendJson<RecordArrScanProcessing>(
    `/api/v1/workspace/document-scans/${encodeURIComponent(scanProcessingId)}/manual-correction`,
    accessToken,
    'POST',
    body,
  )
}

export async function archiveRecord(accessToken: string, recordId: string, body: { actorPersonId: string }): Promise<RecordArrRecord> {
  return sendJson<RecordArrRecord>(`/api/v1/workspace/records/${encodeURIComponent(recordId)}/archive`, accessToken, 'POST', body)
}

export async function purgeRecord(accessToken: string, recordId: string, body: { actorPersonId: string }): Promise<RecordArrRecord> {
  return sendJson<RecordArrRecord>(`/api/v1/workspace/records/${encodeURIComponent(recordId)}/purge`, accessToken, 'POST', body)
}

export async function getOcrResult(accessToken: string, ocrResultId: string): Promise<RecordArrOcrResult> {
  return getJson<RecordArrOcrResult>(`/api/v1/workspace/ocr-results/${encodeURIComponent(ocrResultId)}`, accessToken)
}

export async function getExtractionResult(
  accessToken: string,
  extractionResultId: string,
): Promise<RecordArrExtractionResult> {
  return getJson<RecordArrExtractionResult>(
    `/api/v1/workspace/extraction-results/${encodeURIComponent(extractionResultId)}`,
    accessToken,
  )
}

export async function reviewExtractionResult(
  accessToken: string,
  extractionResultId: string,
  body: { reviewedByPersonId: string; status: string; failureReason?: string | null },
): Promise<RecordArrExtractionResult> {
  return sendJson<RecordArrExtractionResult>(
    `/api/v1/workspace/extraction-results/${encodeURIComponent(extractionResultId)}/review`,
    accessToken,
    'POST',
    body,
  )
}

export async function listEvidenceMappings(accessToken: string): Promise<RecordArrEvidenceMapping[]> {
  return getJson<RecordArrEvidenceMapping[]>('/api/v1/workspace/evidence-mappings', accessToken)
}

export async function createEvidenceMapping(
  accessToken: string,
  body: {
    recordId: string
    sourceProduct: string
    sourceObjectType: string
    sourceObjectId: string
    complianceRequirementRef: string
    evidenceTypeKey: string
    mappingSource: string
    confidenceScore: number
  },
): Promise<RecordArrEvidenceMapping> {
  return sendJson<RecordArrEvidenceMapping>('/api/v1/workspace/evidence-mappings', accessToken, 'POST', body)
}

export async function confirmEvidenceMapping(
  accessToken: string,
  mappingId: string,
  body: { confirmedByPersonId: string; notes?: string | null },
): Promise<RecordArrEvidenceMapping> {
  return sendJson<RecordArrEvidenceMapping>(
    `/api/v1/workspace/evidence-mappings/${encodeURIComponent(mappingId)}/confirm`,
    accessToken,
    'POST',
    body,
  )
}

export async function rejectEvidenceMapping(
  accessToken: string,
  mappingId: string,
  body: { rejectedByPersonId: string; rejectionReason: string; notes?: string | null },
): Promise<RecordArrEvidenceMapping> {
  return sendJson<RecordArrEvidenceMapping>(
    `/api/v1/workspace/evidence-mappings/${encodeURIComponent(mappingId)}/reject`,
    accessToken,
    'POST',
    body,
  )
}

export async function listPackages(accessToken: string): Promise<RecordArrPackage[]> {
  return getJson<RecordArrPackage[]>('/api/v1/workspace/record-packages', accessToken)
}

export async function createPackage(
  accessToken: string,
  body: { title: string; packageType: string; sourceProduct: string; sourceObjectRef: string; recordRef: string },
): Promise<RecordArrPackage> {
  return sendJson<RecordArrPackage>('/api/v1/workspace/record-packages', accessToken, 'POST', body)
}

export async function lockPackage(accessToken: string, packageId: string): Promise<RecordArrPackage> {
  return sendJson<RecordArrPackage>(
    `/api/v1/workspace/record-packages/${encodeURIComponent(packageId)}/lock`,
    accessToken,
    'POST',
    {},
  )
}

export async function getPackageManifest(accessToken: string, packageId: string): Promise<RecordArrPackageManifest> {
  return getJson<RecordArrPackageManifest>(
    `/api/v1/workspace/record-packages/${encodeURIComponent(packageId)}/manifest`,
    accessToken,
  )
}

export async function downloadPackage(accessToken: string, packageId: string): Promise<string> {
  const response = await fetch(`${apiBase}/api/v1/workspace/record-packages/${encodeURIComponent(packageId)}/download`, {
    headers: authHeaders(accessToken),
  })
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `Failed to download /api/v1/workspace/record-packages/${encodeURIComponent(packageId)}/download`)
  }
  return response.text()
}

export async function listRetentionPolicies(accessToken: string): Promise<RecordArrRetentionPolicy[]> {
  return getJson<RecordArrRetentionPolicy[]>('/api/v1/workspace/retention-policies', accessToken)
}

export async function getRetentionStatus(
  accessToken: string,
  recordId: string,
): Promise<RecordArrRetentionStatus> {
  return getJson<RecordArrRetentionStatus>(
    `/api/v1/workspace/records/${encodeURIComponent(recordId)}/retention-status`,
    accessToken,
  )
}

export async function listLegalHolds(accessToken: string): Promise<RecordArrLegalHold[]> {
  return getJson<RecordArrLegalHold[]>('/api/v1/workspace/legal-holds', accessToken)
}

export async function createLegalHold(
  accessToken: string,
  body: {
    title: string
    description: string
    holdType: string
    sourceProduct: string
    sourceObjectType: string
    sourceObjectId: string
    createdByPersonId: string
    scopeRules: string[]
    recordRefs: string[]
  },
): Promise<RecordArrLegalHold> {
  return sendJson<RecordArrLegalHold>('/api/v1/workspace/legal-holds', accessToken, 'POST', body)
}

export async function activateLegalHold(accessToken: string, legalHoldId: string): Promise<RecordArrLegalHold> {
  return sendJson<RecordArrLegalHold>(
    `/api/v1/workspace/legal-holds/${encodeURIComponent(legalHoldId)}/activate`,
    accessToken,
    'POST',
    {},
  )
}

export async function releaseLegalHold(
  accessToken: string,
  legalHoldId: string,
  body: { releasedByPersonId: string; releaseReason: string },
): Promise<RecordArrLegalHold> {
  return sendJson<RecordArrLegalHold>(
    `/api/v1/workspace/legal-holds/${encodeURIComponent(legalHoldId)}/release`,
    accessToken,
    'POST',
    body,
  )
}

export async function listControlledDocuments(accessToken: string): Promise<RecordArrControlledDocument[]> {
  return getJson<RecordArrControlledDocument[]>('/api/v1/workspace/controlled-documents', accessToken)
}

export async function getControlledDocument(
  accessToken: string,
  controlledDocumentId: string,
): Promise<RecordArrControlledDocument> {
  return getJson<RecordArrControlledDocument>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}`,
    accessToken,
  )
}

export async function createControlledDocument(
  accessToken: string,
  body: {
    title: string
    description: string
    controlledDocumentType: string
    ownerPersonId: string
    departmentOrgUnitId: string
    staffarrSiteId: string
    acknowledgementRequired: boolean
  },
): Promise<RecordArrControlledDocument> {
  return sendJson<RecordArrControlledDocument>(
    '/api/v1/workspace/controlled-documents',
    accessToken,
    'POST',
    body,
  )
}

export async function createDocumentVersion(
  accessToken: string,
  controlledDocumentId: string,
  body: { fileName: string; createdByPersonId: string; changeSummary?: string | null },
): Promise<RecordArrControlledDocumentVersion> {
  return sendJson<RecordArrControlledDocumentVersion>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/versions`,
    accessToken,
    'POST',
    body,
  )
}

export async function archiveControlledDocument(
  accessToken: string,
  controlledDocumentId: string,
  body: { updatedByPersonId: string },
): Promise<RecordArrControlledDocument> {
  return sendJson<RecordArrControlledDocument>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/archive`,
    accessToken,
    'POST',
    body,
  )
}

export async function obsoleteControlledDocument(
  accessToken: string,
  controlledDocumentId: string,
  body: { updatedByPersonId: string },
): Promise<RecordArrControlledDocument> {
  return sendJson<RecordArrControlledDocument>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/obsolete`,
    accessToken,
    'POST',
    body,
  )
}

export async function supersedeControlledDocument(
  accessToken: string,
  controlledDocumentId: string,
  body: { supersededByDocumentRef: string; supersededByPersonId: string },
): Promise<RecordArrControlledDocument> {
  return sendJson<RecordArrControlledDocument>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/supersede`,
    accessToken,
    'POST',
    body,
  )
}

export async function promoteDocumentVersion(
  accessToken: string,
  controlledDocumentId: string,
  versionId: string,
  body: { approvedByPersonId: string; effectiveAt?: string | null },
): Promise<RecordArrControlledDocumentVersion> {
  return sendJson<RecordArrControlledDocumentVersion>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/versions/${encodeURIComponent(versionId)}/promote`,
    accessToken,
    'POST',
    body,
  )
}

export async function createDocumentReview(
  accessToken: string,
  controlledDocumentId: string,
  body: {
    versionId: string
    reviewType: string
    requestedByPersonId: string
    reviewerPersonId: string
    dueAt?: string | null
  },
): Promise<RecordArrDocumentReview> {
  return sendJson<RecordArrDocumentReview>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/reviews`,
    accessToken,
    'POST',
    { ...body, dueAt: body.dueAt ?? null },
  )
}

export async function completeDocumentReview(
  accessToken: string,
  controlledDocumentId: string,
  reviewId: string,
  body: { status: string; decisionReason?: string | null; comments?: string | null },
): Promise<RecordArrDocumentReview> {
  return sendJson<RecordArrDocumentReview>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/reviews/${encodeURIComponent(reviewId)}/complete`,
    accessToken,
    'POST',
    body,
  )
}

export async function listDocumentVersions(
  accessToken: string,
  controlledDocumentId: string,
): Promise<RecordArrControlledDocumentVersion[]> {
  return getJson<RecordArrControlledDocumentVersion[]>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/versions`,
    accessToken,
  )
}

export async function listDocumentReviews(
  accessToken: string,
  controlledDocumentId: string,
): Promise<RecordArrDocumentReview[]> {
  return getJson<RecordArrDocumentReview[]>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/reviews`,
    accessToken,
  )
}

export async function listDocumentDistributions(
  accessToken: string,
  controlledDocumentId: string,
): Promise<RecordArrDocumentDistribution[]> {
  return getJson<RecordArrDocumentDistribution[]>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/distributions`,
    accessToken,
  )
}

export async function listDocumentAcknowledgements(
  accessToken: string,
  controlledDocumentId: string,
): Promise<RecordArrDocumentAcknowledgement[]> {
  return getJson<RecordArrDocumentAcknowledgement[]>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/acknowledgements`,
    accessToken,
  )
}

export async function createDocumentDistribution(
  accessToken: string,
  controlledDocumentId: string,
  body: { versionId: string; distributionType: string; targetRef: string },
): Promise<RecordArrDocumentDistribution> {
  return sendJson<RecordArrDocumentDistribution>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/distributions`,
    accessToken,
    'POST',
    body,
  )
}

export async function revokeDocumentDistribution(
  accessToken: string,
  controlledDocumentId: string,
  distributionId: string,
  body: { revokedByPersonId: string; revokeReason?: string | null },
): Promise<RecordArrDocumentDistribution> {
  return sendJson<RecordArrDocumentDistribution>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/distributions/${encodeURIComponent(distributionId)}/revoke`,
    accessToken,
    'POST',
    body,
  )
}

export async function expireDocumentDistribution(
  accessToken: string,
  controlledDocumentId: string,
  distributionId: string,
  body: { expiredByPersonId: string; expireReason?: string | null },
): Promise<RecordArrDocumentDistribution> {
  return sendJson<RecordArrDocumentDistribution>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/distributions/${encodeURIComponent(distributionId)}/expire`,
    accessToken,
    'POST',
    body,
  )
}

export async function createDocumentAcknowledgement(
  accessToken: string,
  controlledDocumentId: string,
  body: { versionId: string; personId: string; attestationText?: string | null; dueAt?: string | null },
): Promise<RecordArrDocumentAcknowledgement> {
  return sendJson<RecordArrDocumentAcknowledgement>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/acknowledgements`,
    accessToken,
    'POST',
    body,
  )
}

export async function completeDocumentAcknowledgement(
  accessToken: string,
  controlledDocumentId: string,
  acknowledgementId: string,
  body: { signatureRecordRef?: string | null },
): Promise<RecordArrDocumentAcknowledgement> {
  return sendJson<RecordArrDocumentAcknowledgement>(
    `/api/v1/workspace/controlled-documents/${encodeURIComponent(controlledDocumentId)}/acknowledgements/${encodeURIComponent(acknowledgementId)}/complete`,
    accessToken,
    'POST',
    body,
  )
}

export async function listAccessPolicies(accessToken: string): Promise<RecordArrAccessPolicy[]> {
  return getJson<RecordArrAccessPolicy[]>('/api/v1/workspace/access-policies', accessToken)
}

export async function listAccessGrants(accessToken: string): Promise<RecordArrAccessGrant[]> {
  return getJson<RecordArrAccessGrant[]>('/api/v1/workspace/access-grants', accessToken)
}

export async function createAccessGrant(
  accessToken: string,
  body: {
    recordId: string
    granteeType: string
    granteeRef: string
    permission: string
    grantedByPersonId: string
    expiresAt?: string | null
  },
): Promise<RecordArrAccessGrant> {
  return sendJson<RecordArrAccessGrant>('/api/v1/workspace/access-grants', accessToken, 'POST', body)
}

export async function revokeAccessGrant(
  accessToken: string,
  accessGrantId: string,
  body: { revokedByPersonId: string; revokeReason?: string | null },
): Promise<RecordArrAccessGrant> {
  return sendJson<RecordArrAccessGrant>(
    `/api/v1/workspace/access-grants/${encodeURIComponent(accessGrantId)}/revoke`,
    accessToken,
    'POST',
    body,
  )
}

export async function listExternalShares(accessToken: string): Promise<RecordArrExternalShare[]> {
  return getJson<RecordArrExternalShare[]>('/api/v1/workspace/external-shares', accessToken)
}

export async function createExternalShare(
  accessToken: string,
  body: {
    recordId: string
    recipientName: string
    recipientEmail: string
    sharePurpose: string
    allowedActions: string[]
    createdByPersonId: string
  },
): Promise<RecordArrExternalShare> {
  return sendJson<RecordArrExternalShare>('/api/v1/workspace/external-shares', accessToken, 'POST', body)
}

export async function revokeExternalShare(
  accessToken: string,
  externalShareId: string,
  body: { revokedByPersonId: string },
): Promise<RecordArrExternalShare> {
  return sendJson<RecordArrExternalShare>(
    `/api/v1/workspace/external-shares/${encodeURIComponent(externalShareId)}/revoke`,
    accessToken,
    'POST',
    body,
  )
}

export async function expireExternalShare(
  accessToken: string,
  externalShareId: string,
  body: { expiredByPersonId: string },
): Promise<RecordArrExternalShare> {
  return sendJson<RecordArrExternalShare>(
    `/api/v1/workspace/external-shares/${encodeURIComponent(externalShareId)}/expire`,
    accessToken,
    'POST',
    body,
  )
}

export async function listRedactions(accessToken: string): Promise<RecordArrRedaction[]> {
  return getJson<RecordArrRedaction[]>('/api/v1/workspace/redactions', accessToken)
}

export async function createRedaction(
  accessToken: string,
  body: {
    sourceRecordId: string
    redactedRecordId: string
    redactionReason: string
    redactedByPersonId: string
    redactionRules: string[]
  },
): Promise<RecordArrRedaction> {
  return sendJson<RecordArrRedaction>('/api/v1/workspace/redactions', accessToken, 'POST', body)
}

export async function listDisposalReviews(accessToken: string): Promise<RecordArrDisposalReview[]> {
  return getJson<RecordArrDisposalReview[]>('/api/v1/workspace/disposal-reviews', accessToken)
}

export async function createDisposalReview(
  accessToken: string,
  body: { recordId: string; retentionStatusRef: string; proposedAction: string; requestedByPersonId: string },
): Promise<RecordArrDisposalReview> {
  return sendJson<RecordArrDisposalReview>('/api/v1/workspace/disposal-reviews', accessToken, 'POST', body)
}

export async function completeDisposalReview(
  accessToken: string,
  disposalReviewId: string,
  body: { status: string; reviewedByPersonId?: string | null; decisionReason?: string | null },
): Promise<RecordArrDisposalReview> {
  return sendJson<RecordArrDisposalReview>(
    `/api/v1/workspace/disposal-reviews/${encodeURIComponent(disposalReviewId)}/complete`,
    accessToken,
    'POST',
    body,
  )
}

export async function listAccessLogs(accessToken: string): Promise<RecordArrAccessLog[]> {
  return getJson<RecordArrAccessLog[]>('/api/v1/workspace/access-logs', accessToken)
}
