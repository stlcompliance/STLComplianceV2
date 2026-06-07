export type DashboardCard = {
  key: string
  title: string
  description: string
  count: number
  tone: string
}

export type TimelineEvent = {
  id: string
  subjectType: string
  subjectId: string
  eventType: string
  details: string | null
  occurredAt: string
}

export type DashboardResponse = {
  generatedAt: string
  cards: DashboardCard[]
  recentEvents: TimelineEvent[]
}

export type ListItem = {
  id: string
  number: string
  title: string
  status: string
  severity: string
  sourceProduct: string | null
  sourceObjectRef: string | null
  affectedObjectRefs: string[]
  ownerPersonId: string | null
  createdAt: string
  updatedAt: string
  createdByPersonId: string
  updatedByPersonId: string
}

export type Nonconformance = ListItem & {
  description: string
  nonconformanceType: string
  category: string
  discoveredAt: string | null
  discoveredByPersonId: string | null
  staffArrSiteId: string | null
  staffArrLocationId: string | null
  recordRefs: string[]
  containmentRefs: string[]
  holdRefs: string[]
  affectedItemRefs: string[]
  affectedAssetRefs: string[]
  affectedOrderRefs: string[]
  affectedSupplierRefs: string[]
  affectedCustomerRefs: string[]
  affectedShipmentRefs: string[]
  dispositionRefs: string[]
  capaRefs: string[]
  complianceRefs: string[]
  financialImpactSnapshot: string | null
  auditTrail: string[]
  eventLog: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  customerImpact: string | null
  supplierImpact: string | null
  safetyImpact: string | null
  complianceImpact: string | null
  recurrenceFlag: boolean
  repeatOfNonconformanceRef: string | null
  rootCauseRef: string | null
  blockerRefs: string[]
  dueAt: string | null
}

export type QualityHold = ListItem & {
  description: string
  holdType: string
  holdScope: string
  sourceNonconformanceRef: string | null
  staffArrSiteId: string | null
  staffArrLocationId: string | null
  recordRefs: string[]
  auditTrail: string[]
  eventLog: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  holdReason: string | null
  releaseReason: string | null
  rejectionReason: string | null
  conditionalReleaseTerms: string | null
  releaseRequirements: string[]
  releaseApprovalRefs: string[]
  quantityHeld: number | null
  unitOfMeasure: string | null
  lotNumber: string | null
  serialNumber: string | null
  placedAt: string | null
  placedByPersonId: string | null
  releasedAt: string | null
  releasedByPersonId: string | null
  rejectedAt: string | null
  rejectedByPersonId: string | null
  expiresAt: string | null
}

export type Capa = ListItem & {
  description: string
  capaType: string
  sourceType: string
  staffArrSiteId: string | null
  staffArrLocationId: string | null
  sourceRefs: string[]
  recordRefs: string[]
  actionPlanRefs: string[]
  verificationPlanRef: string | null
  relatedCustomerComplaintRefs: string[]
  relatedSupplierIssueRefs: string[]
  complianceRefs: string[]
  auditTrail: string[]
  eventLog: string[]
  openedAt: string | null
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  sponsorPersonId: string | null
  rootCauseSummary: string | null
  dueAt: string | null
  relatedNonconformanceRefs: string[]
  relatedAuditFindingRefs: string[]
  effectivenessVerificationRefs: string[]
}

export type CapaAction = {
  id: string
  number: string
  capaId: string
  title: string
  description: string
  status: string
  actionType: string
  assignedPersonId: string | null
  assignedTeamRef: string | null
  sourceProductActionRef: string | null
  targetProduct: string
  targetObjectRef: string | null
  dueAt: string | null
  startedAt: string | null
  completedAt: string | null
  completedByPersonId: string | null
  verificationRequired: boolean
  verifiedAt: string | null
  verifiedByPersonId: string | null
  evidenceRecordRefs: string[]
  blockerRefs: string[]
  notes: string | null
  createdAt: string
  updatedAt: string
}

export type CapaActionBlocker = {
  id: string
  number: string
  capaActionId: string
  blockerType: string
  sourceProduct: string | null
  sourceObjectRef: string | null
  title: string
  description: string
  status: string
  createdAt: string
  resolvedAt: string | null
  resolvedByPersonId: string | null
}

export type VerificationPlan = {
  id: string
  number: string
  capaId: string
  title: string
  description: string
  verificationType: string
  successCriteria: string
  sampleSize: number | null
  observationPeriodDays: number | null
  requiredEvidenceTypes: string[]
  responsiblePersonId: string | null
  plannedVerificationAt: string | null
  status: string
  createdAt: string
  updatedAt: string
}

export type EffectivenessVerification = {
  id: string
  number: string
  capaId: string
  verificationPlanId: string | null
  status: string
  performedByPersonId: string | null
  performedAt: string | null
  resultSummary: string | null
  evidenceRecordRefs: string[]
  metricResults: string[]
  recurrenceFound: boolean
  followUpRequired: boolean
  reopenedCapaRef: string | null
  createdAt: string
  updatedAt: string
}

export type Audit = ListItem & {
  description: string
  auditType: string
  auditScope: string | null
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  standardRefs: string[]
  complianceRefs: string[]
  auditorPersonIds: string[]
  leadAuditorPersonId: string | null
  auditeeRefs: string[]
  staffArrSiteId: string | null
  staffArrLocationId: string | null
  supplierRef: string | null
  customerRef: string | null
  plannedStartAt: string | null
  plannedEndAt: string | null
  actualStartAt: string | null
  actualEndAt: string | null
  checklistRefs: string[]
  findingRefs: string[]
  auditTrail: string[]
}

export type AuditChecklist = {
  id: string
  number: string
  auditId: string
  title: string
  description: string
  status: string
  itemRefs: string[]
  createdAt: string
  updatedAt: string
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
}

export type AuditChecklistItem = {
  id: string
  number: string
  checklistId: string
  sequence: number
  prompt: string
  helpText: string | null
  requirementRef: string | null
  responseType: string
  required: boolean
  responseValue: string | null
  result: string | null
  findingCreated: boolean
  findingRef: string | null
  evidenceRecordRefs: string[]
  answeredAt: string | null
  answeredByPersonId: string | null
  createdAt: string
  updatedAt: string
}

export type Finding = ListItem & {
  description: string
  findingType: string
  recordRefs: string[]
  sourceRequirementRef: string | null
  evidenceRecordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  auditRef: string | null
  nonconformanceRef: string | null
  capaRef: string | null
  dueAt: string | null
}

export type RootCauseAnalysis = {
  id: string
  number: string
  nonconformanceId: string
  title: string
  description: string
  status: string
  method: string
  primaryCauseCategory: string
  sourceProduct: string | null
  sourceObjectRef: string | null
  affectedObjectRefs: string[]
  ownerPersonId: string | null
  recordRefs: string[]
  createdAt: string
  updatedAt: string
  rootCauseSummary: string | null
  contributingFactors: string[]
  analyzedByPersonId: string | null
  completedAt: string | null
  evidenceRecordRefs: string[]
}

export type StatusSnapshot = ListItem & {
  description: string
  targetProduct: string
  targetObjectRef: string
  qualityStatus: string
  activeHoldRefs: string[]
  openNonconformanceRefs: string[]
  openCapaRefs: string[]
  openFindingRefs: string[]
  lastReviewedAt: string | null
  reviewedByPersonId: string | null
  expiresAt: string | null
  notes: string | null
  recordRefs: string[]
  eventLog: string[]
}

export type QualityStatus = StatusSnapshot

export type Scorecard = ListItem & {
  description: string
  targetType: string
  targetRef: string
  periodStart: string
  periodEnd: string
  overallScore: number | null
  qualityStatus: string
  trend: string
  generatedAt: string
  generatedBy: string
  reviewedByPersonId: string | null
  reviewedAt: string | null
  metricRefs: string[]
  eventLog: string[]
}

export type QualityMetric = {
  id: string
  scorecardId: string
  metricKey: string
  title: string
  description: string
  category: string
  value: number | null
  numerator: number | null
  denominator: number | null
  unit: string | null
  targetValue: number | null
  warningThreshold: number | null
  criticalThreshold: number | null
  status: string
  sourceProductRefs: string[]
  createdAt: string
  updatedAt: string
}

export type QualityRiskProfile = {
  id: string
  targetType: string
  targetRef: string
  riskLevel: string
  riskFactors: string[]
  openIssueCount: number
  repeatIssueCount: number
  criticalIssueCount: number
  lastIncidentAt: string | null
  mitigationActions: string[]
  reviewedAt: string | null
  reviewedByPersonId: string | null
  eventLog: string[]
  createdAt: string
  updatedAt: string
}

export type QualityReview = ListItem & {
  reviewType: string
  sourceReviewRef: string | null
  reviewerPersonId: string | null
  requestedAt: string | null
  dueAt: string | null
  decisionAt: string | null
  decisionReason: string | null
  requiredEvidenceRefs: string[]
  submittedEvidenceRefs: string[]
  notes: string | null
  eventLog: string[]
}

export type QualityRelease = ListItem & {
  holdRef: string
  releaseType: string
  requestedByPersonId: string | null
  requestedAt: string | null
  approvedByPersonId: string | null
  approvedAt: string | null
  executedAt: string | null
  conditions: string | null
  expirationAt: string | null
  evidenceRecordRefs: string[]
  notes: string | null
  eventLog: string[]
}

export type ContainmentAction = ListItem & {
  actionType: string
  nonconformanceRef: string | null
  assignedPersonId: string | null
  assignedTeamRef: string | null
  sourceProductActionRef: string | null
  dueAt: string | null
  startedAt: string | null
  completedAt: string | null
  completedByPersonId: string | null
  verificationRequired: boolean
  verifiedByPersonId: string | null
  verifiedAt: string | null
  evidenceRecordRefs: string[]
  notes: string | null
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
}

export type Disposition = ListItem & {
  dispositionType: string
  nonconformanceRef: string | null
  decisionByPersonId: string | null
  decisionAt: string | null
  approvedByPersonId: string | null
  approvedAt: string | null
  rationale: string | null
  requiredActions: string[]
  executionProduct: string | null
  executionObjectRef: string | null
  evidenceRecordRefs: string[]
  notes: string | null
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
}

export type SupplierQualityIssue = ListItem & {
  issueType: string
  affectedObjectRefs: string[]
  affectedReceiptRefs: string[]
  affectedPurchaseOrderRefs: string[]
  affectedItemRefs: string[]
  supplierRef: string | null
  nonconformanceRef: string | null
  scarRef: string | null
  holdRefs: string[]
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  openedAt: string | null
}

export type SupplierCorrectiveActionRequest = ListItem & {
  affectedObjectRefs: string[]
  supplierRef: string | null
  sourceNonconformanceRef: string | null
  sourceCapaRef: string | null
  requestedByPersonId: string | null
  requestedAt: string | null
  supplierDueAt: string | null
  supplierResponseRecordRefs: string[]
  reviewPersonId: string | null
  reviewedAt: string | null
  reviewDecision: string | null
  followUpCapaRef: string | null
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
}

export type CustomerComplaintQualityCase = ListItem & {
  complaintType: string
  affectedObjectRefs: string[]
  affectedOrderRefs: string[]
  affectedShipmentRefs: string[]
  affectedItemRefs: string[]
  affectedAssetRefs: string[]
  customerRef: string | null
  customerContactSnapshot: string | null
  customerLocationRef: string | null
  nonconformanceRef: string | null
  holdRefs: string[]
  capaRefs: string[]
  customerResponseRecordRefs: string[]
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  receivedAt: string | null
  receivedByPersonId: string | null
  customerResponseDueAt: string | null
}

type CreateBase = {
  title: string
  description: string
  severity: string
  sourceProduct?: string
  sourceObjectRef?: string
  affectedObjectRefs: string[]
  ownerPersonId?: string
}

const apiBase = import.meta.env.VITE_ASSURARR_API_BASE ?? ''

async function readJson<T>(response: Response, fallbackMessage: string): Promise<T> {
  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || `${fallbackMessage} (${response.status})`)
  }

  return (await response.json()) as T
}

async function getJson<T>(path: string): Promise<T> {
  return readJson<T>(await fetch(`${apiBase}${path}`), `Request failed for ${path}`)
}

async function sendJson<T>(path: string, method: 'POST' | 'PATCH', body: unknown): Promise<T> {
  return readJson<T>(
    await fetch(`${apiBase}${path}`, {
      method,
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }),
    `Request failed for ${path}`,
  )
}

function splitLines(value?: string): string[] {
  if (!value) return []
  return value
    .split(/[,;\n]/)
    .map((item) => item.trim())
    .filter(Boolean)
}

export const assurarrApi = {
  getDashboard: () => getJson<DashboardResponse>('/api/v1/dashboard'),
  listNonconformances: () => getJson<Nonconformance[]>('/api/v1/nonconformances'),
  getNonconformance: (id: string) => getJson<Nonconformance>(`/api/v1/nonconformances/${id}`),
  createNonconformance: (body: CreateBase & { nonconformanceType: string; category: string; recurrenceFlag?: boolean; blockerRefs?: string[]; dueAt?: string; discoveredAt?: string; discoveredByPersonId?: string; staffArrSiteId?: string; staffArrLocationId?: string; containmentRefs?: string[]; holdRefs?: string[]; affectedItemRefs?: string[]; affectedAssetRefs?: string[]; affectedOrderRefs?: string[]; affectedSupplierRefs?: string[]; affectedCustomerRefs?: string[]; affectedShipmentRefs?: string[]; dispositionRefs?: string[]; capaRefs?: string[]; complianceRefs?: string[]; financialImpactSnapshot?: string }) =>
    sendJson<Nonconformance>('/api/v1/nonconformances', 'POST', {
      ...body,
      recurrenceFlag: body.recurrenceFlag ?? false,
      blockerRefs: body.blockerRefs ?? [],
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
      discoveredAt: body.discoveredAt ? new Date(body.discoveredAt).toISOString() : null,
      discoveredByPersonId: body.discoveredByPersonId ?? null,
      staffArrSiteId: body.staffArrSiteId ?? null,
      staffArrLocationId: body.staffArrLocationId ?? null,
      containmentRefs: body.containmentRefs ?? [],
      holdRefs: body.holdRefs ?? [],
      affectedItemRefs: body.affectedItemRefs ?? [],
      affectedAssetRefs: body.affectedAssetRefs ?? [],
      affectedOrderRefs: body.affectedOrderRefs ?? [],
      affectedSupplierRefs: body.affectedSupplierRefs ?? [],
      affectedCustomerRefs: body.affectedCustomerRefs ?? [],
      affectedShipmentRefs: body.affectedShipmentRefs ?? [],
      dispositionRefs: body.dispositionRefs ?? [],
      capaRefs: body.capaRefs ?? [],
      complianceRefs: body.complianceRefs ?? [],
      financialImpactSnapshot: body.financialImpactSnapshot ?? null,
    }),
  updateNonconformanceStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Nonconformance>(`/api/v1/nonconformances/${id}/status`, 'PATCH', { status, closureSummary }),
  listRootCauseAnalyses: (nonconformanceId: string) =>
    getJson<RootCauseAnalysis[]>(`/api/v1/nonconformances/${nonconformanceId}/root-cause-analyses`),
  getRootCauseAnalysis: (nonconformanceId: string, rootCauseId: string) =>
    getJson<RootCauseAnalysis>(`/api/v1/nonconformances/${nonconformanceId}/root-cause-analyses/${rootCauseId}`),
  createRootCauseAnalysis: (
    body: {
      title: string
      description: string
      nonconformanceId: string
      status: string
      method: string
      primaryCauseCategory: string
      sourceProduct?: string
      sourceObjectRef?: string
      affectedObjectRefs?: string[]
      ownerPersonId?: string
      recordRefs?: string[]
      rootCauseSummary?: string
      contributingFactors?: string[]
      analyzedByPersonId?: string
      completedAt?: string
      evidenceRecordRefs?: string[]
    },
  ) =>
    sendJson<RootCauseAnalysis>('/api/v1/integrations/root-cause-analyses', 'POST', {
      ...body,
      affectedObjectRefs: body.affectedObjectRefs ?? [],
      recordRefs: body.recordRefs ?? [],
      contributingFactors: body.contributingFactors ?? [],
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
      completedAt: body.completedAt ? new Date(body.completedAt).toISOString() : null,
    }),
  listHolds: () => getJson<QualityHold[]>('/api/v1/integrations/holds'),
  getHold: (id: string) => getJson<QualityHold>(`/api/v1/integrations/holds/${id}`),
  createHold: (body: CreateBase & { holdType: string; holdScope: string; sourceNonconformanceRef?: string; holdReason?: string; quantityHeld?: number; unitOfMeasure?: string; lotNumber?: string; serialNumber?: string; expiresAt?: string; staffArrSiteId?: string; staffArrLocationId?: string; placedByPersonId?: string }) =>
    sendJson<QualityHold>('/api/v1/holds', 'POST', {
      ...body,
      sourceNonconformanceRef: body.sourceNonconformanceRef ?? null,
      expiresAt: body.expiresAt ? new Date(body.expiresAt).toISOString() : null,
      staffArrSiteId: body.staffArrSiteId ?? null,
      staffArrLocationId: body.staffArrLocationId ?? null,
      placedByPersonId: body.placedByPersonId ?? null,
    }),
  updateHoldStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<QualityHold>(`/api/v1/holds/${id}/status`, 'PATCH', { status, closureSummary }),
  requestHoldRelease: (holdId: string, body: CreateBase & { holdRef: string; releaseType: string; requestedByPersonId?: string; requestedAt?: string; conditions?: string; expirationAt?: string; evidenceRecordRefs?: string; notes?: string }) =>
    sendJson<QualityRelease>(`/api/v1/integrations/holds/${holdId}/release-requests`, 'POST', {
      ...body,
      sourceProduct: body.sourceProduct || null,
      sourceObjectRef: body.sourceObjectRef || null,
      affectedObjectRefs: body.affectedObjectRefs,
      ownerPersonId: body.ownerPersonId || null,
      requestedByPersonId: body.requestedByPersonId || null,
      requestedAt: body.requestedAt || null,
      expirationAt: body.expirationAt || null,
      evidenceRecordRefs: splitLines(body.evidenceRecordRefs),
      notes: body.notes || null,
    }),
  approveHoldRelease: (holdId: string, closureSummary?: string) =>
    sendJson<QualityRelease>(`/api/v1/integrations/holds/${holdId}/release`, 'POST', { status: 'executed', closureSummary }),
  rejectHoldRelease: (holdId: string, closureSummary?: string) =>
    sendJson<QualityRelease>(`/api/v1/integrations/holds/${holdId}/reject`, 'POST', { status: 'rejected', closureSummary }),
  listCapas: () => getJson<Capa[]>('/api/v1/integrations/capas'),
  getCapa: (id: string) => getJson<Capa>(`/api/v1/integrations/capas/${id}`),
  createCapa: (body: CreateBase & { capaType: string; sourceType: string; sponsorPersonId?: string; rootCauseSummary?: string; dueAt?: string; relatedNonconformanceRefs?: string[]; relatedAuditFindingRefs?: string[]; effectivenessVerificationRefs?: string[]; staffArrSiteId?: string; staffArrLocationId?: string; sourceRefs?: string[]; recordRefs?: string[]; actionPlanRefs?: string[]; verificationPlanRef?: string; relatedCustomerComplaintRefs?: string[]; relatedSupplierIssueRefs?: string[]; complianceRefs?: string[]; openedAt?: string }) =>
    sendJson<Capa>('/api/v1/integrations/capas', 'POST', {
      ...body,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
      relatedNonconformanceRefs: body.relatedNonconformanceRefs ?? [],
      relatedAuditFindingRefs: body.relatedAuditFindingRefs ?? [],
      effectivenessVerificationRefs: body.effectivenessVerificationRefs ?? [],
      staffArrSiteId: body.staffArrSiteId || null,
      staffArrLocationId: body.staffArrLocationId || null,
      sourceRefs: body.sourceRefs ?? [],
      recordRefs: body.recordRefs ?? [],
      actionPlanRefs: body.actionPlanRefs ?? [],
      verificationPlanRef: body.verificationPlanRef || null,
      relatedCustomerComplaintRefs: body.relatedCustomerComplaintRefs ?? [],
      relatedSupplierIssueRefs: body.relatedSupplierIssueRefs ?? [],
      complianceRefs: body.complianceRefs ?? [],
      openedAt: body.openedAt ? new Date(body.openedAt).toISOString() : null,
    }),
  updateCapaStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Capa>(`/api/v1/capas/${id}/status`, 'PATCH', { status, closureSummary }),
  getCapaAction: (capaId: string, actionId: string) => getJson<CapaAction>(`/api/v1/capas/${capaId}/actions/${actionId}`),
  listCapaActions: (capaId: string) => getJson<CapaAction[]>(`/api/v1/capas/${capaId}/actions`),
  createCapaAction: (capaId: string, body: { title: string; description: string; actionType: string; assignedPersonId?: string; assignedTeamRef?: string; sourceProductActionRef?: string; targetProduct: string; targetObjectRef?: string; dueAt?: string; verificationRequired?: boolean; evidenceRecordRefs?: string[]; blockerRefs?: string[]; notes?: string }) =>
    sendJson<CapaAction>(`/api/v1/integrations/capas/${capaId}/actions`, 'POST', {
      ...body,
      verificationRequired: body.verificationRequired ?? true,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
      blockerRefs: body.blockerRefs ?? [],
    }),
  updateCapaActionStatus: (capaId: string, actionId: string, body: { status: string; completedByPersonId?: string; completedAt?: string; verifiedByPersonId?: string; verifiedAt?: string; closureSummary?: string }) =>
    sendJson<CapaAction>(`/api/v1/capas/${capaId}/actions/${actionId}/status`, 'PATCH', {
      ...body,
      completedAt: body.completedAt ? new Date(body.completedAt).toISOString() : null,
      verifiedAt: body.verifiedAt ? new Date(body.verifiedAt).toISOString() : null,
    }),
  getCapaActionBlocker: (capaId: string, actionId: string, blockerId: string) =>
    getJson<CapaActionBlocker>(`/api/v1/capas/${capaId}/actions/${actionId}/blockers/${blockerId}`),
  listCapaActionBlockers: (capaId: string, actionId: string) => getJson<CapaActionBlocker[]>(`/api/v1/capas/${capaId}/actions/${actionId}/blockers`),
  createCapaActionBlocker: (capaId: string, actionId: string, body: { blockerType: string; sourceProduct?: string; sourceObjectRef?: string; title: string; description: string }) =>
    sendJson<CapaActionBlocker>(`/api/v1/capas/${capaId}/actions/${actionId}/blockers`, 'POST', body),
  updateCapaActionBlockerStatus: (capaId: string, actionId: string, blockerId: string, status: string, resolvedByPersonId?: string, resolvedAt?: string) =>
    sendJson<CapaActionBlocker>(`/api/v1/capas/${capaId}/actions/${actionId}/blockers/${blockerId}/status`, 'PATCH', {
      status,
      resolvedByPersonId: resolvedByPersonId ?? null,
      resolvedAt: resolvedAt ? new Date(resolvedAt).toISOString() : null,
    }),
  getVerificationPlan: (capaId: string, verificationPlanId: string) =>
    getJson<VerificationPlan>(`/api/v1/capas/${capaId}/verification-plans/${verificationPlanId}`),
  listVerificationPlans: (capaId: string) => getJson<VerificationPlan[]>(`/api/v1/capas/${capaId}/verification-plans`),
  createVerificationPlan: (capaId: string, body: { title: string; description: string; verificationType: string; successCriteria: string; sampleSize?: number; observationPeriodDays?: number; requiredEvidenceTypes?: string[]; responsiblePersonId?: string; plannedVerificationAt?: string }) =>
    sendJson<VerificationPlan>(`/api/v1/integrations/capas/${capaId}/verification`, 'POST', {
      ...body,
      requiredEvidenceTypes: body.requiredEvidenceTypes ?? [],
      plannedVerificationAt: body.plannedVerificationAt ? new Date(body.plannedVerificationAt).toISOString() : null,
    }),
  updateVerificationPlanStatus: (capaId: string, verificationPlanId: string, status: string, closureSummary?: string) =>
    sendJson<VerificationPlan>(`/api/v1/capas/${capaId}/verification-plans/${verificationPlanId}/status`, 'PATCH', { status, closureSummary }),
  getEffectivenessVerification: (capaId: string, verificationId: string) =>
    getJson<EffectivenessVerification>(`/api/v1/capas/${capaId}/effectiveness-verifications/${verificationId}`),
  listEffectivenessVerifications: (capaId: string) => getJson<EffectivenessVerification[]>(`/api/v1/capas/${capaId}/effectiveness-verifications`),
  createEffectivenessVerification: (
    capaId: string,
    body: {
      verificationPlanId?: string
      status: string
      performedByPersonId?: string
      performedAt?: string
      resultSummary?: string
      evidenceRecordRefs?: string[]
      metricResults?: string[]
      recurrenceFound?: boolean
      followUpRequired?: boolean
      reopenedCapaRef?: string
    },
  ) =>
    sendJson<EffectivenessVerification>(`/api/v1/capas/${capaId}/effectiveness-verifications`, 'POST', {
      ...body,
      verificationPlanId: body.verificationPlanId ?? null,
      performedByPersonId: body.performedByPersonId ?? null,
      performedAt: body.performedAt ? new Date(body.performedAt).toISOString() : null,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
      metricResults: body.metricResults ?? [],
      recurrenceFound: body.recurrenceFound ?? false,
      followUpRequired: body.followUpRequired ?? false,
      reopenedCapaRef: body.reopenedCapaRef ?? null,
    }),
  updateEffectivenessVerificationStatus: (
    capaId: string,
    verificationId: string,
    status: string,
    body?: { resultSummary?: string; recurrenceFound?: boolean; followUpRequired?: boolean; reopenedCapaRef?: string },
  ) =>
    sendJson<EffectivenessVerification>(`/api/v1/capas/${capaId}/effectiveness-verifications/${verificationId}/status`, 'PATCH', {
      status,
      resultSummary: body?.resultSummary ?? null,
      recurrenceFound: body?.recurrenceFound ?? null,
      followUpRequired: body?.followUpRequired ?? null,
      reopenedCapaRef: body?.reopenedCapaRef ?? null,
    }),
  listAudits: () => getJson<Audit[]>('/api/v1/audits'),
  getAudit: (id: string) => getJson<Audit>(`/api/v1/audits/${id}`),
  createAudit: (body: CreateBase & { auditType: string; auditScope?: string; auditorPersonIds?: string[]; leadAuditorPersonId?: string; staffArrSiteId?: string; staffArrLocationId?: string; supplierRef?: string; customerRef?: string; plannedStartAt?: string; plannedEndAt?: string; checklistRefs?: string[]; standardRefs?: string[]; complianceRefs?: string[]; auditeeRefs?: string[]; actualStartAt?: string; actualEndAt?: string }) =>
    sendJson<Audit>('/api/v1/audits', 'POST', {
      ...body,
      auditorPersonIds: body.auditorPersonIds ?? [],
      checklistRefs: body.checklistRefs ?? [],
      standardRefs: body.standardRefs ?? [],
      complianceRefs: body.complianceRefs ?? [],
      auditeeRefs: body.auditeeRefs ?? [],
      plannedStartAt: body.plannedStartAt ? new Date(body.plannedStartAt).toISOString() : null,
      plannedEndAt: body.plannedEndAt ? new Date(body.plannedEndAt).toISOString() : null,
      actualStartAt: body.actualStartAt ? new Date(body.actualStartAt).toISOString() : null,
      actualEndAt: body.actualEndAt ? new Date(body.actualEndAt).toISOString() : null,
    }),
  updateAuditStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Audit>(`/api/v1/audits/${id}/status`, 'PATCH', { status, closureSummary }),
  listAuditChecklists: (auditId: string) => getJson<AuditChecklist[]>(`/api/v1/audits/${auditId}/checklists`),
  getAuditChecklist: (auditId: string, checklistId: string) => getJson<AuditChecklist>(`/api/v1/audits/${auditId}/checklists/${checklistId}`),
  createAuditChecklist: (auditId: string, body: { title: string; description: string; status?: string }) =>
    sendJson<AuditChecklist>(`/api/v1/audits/${auditId}/checklists`, 'POST', {
      ...body,
      status: body.status ?? 'draft',
    }),
  updateAuditChecklistStatus: (auditId: string, checklistId: string, status: string, closureSummary?: string) =>
    sendJson<AuditChecklist>(`/api/v1/audits/${auditId}/checklists/${checklistId}/status`, 'PATCH', { status, closureSummary }),
  listAuditChecklistItems: (auditId: string, checklistId: string) =>
    getJson<AuditChecklistItem[]>(`/api/v1/audits/${auditId}/checklists/${checklistId}/items`),
  getAuditChecklistItem: (auditId: string, checklistId: string, itemId: string) =>
    getJson<AuditChecklistItem>(`/api/v1/audits/${auditId}/checklists/${checklistId}/items/${itemId}`),
  createAuditChecklistItem: (
    auditId: string,
    checklistId: string,
    body: {
      sequence: number
      prompt: string
      helpText?: string
      requirementRef?: string
      responseType: string
      required: boolean
      responseValue?: string
      result?: string
      findingCreated?: boolean
      findingRef?: string
      evidenceRecordRefs?: string[]
      answeredByPersonId?: string
      answeredAt?: string
    },
  ) =>
    sendJson<AuditChecklistItem>(`/api/v1/audits/${auditId}/checklists/${checklistId}/items`, 'POST', {
      ...body,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
      findingCreated: body.findingCreated ?? false,
      answeredAt: body.answeredAt ? new Date(body.answeredAt).toISOString() : null,
    }),
  updateAuditChecklistItemResponse: (
    auditId: string,
    checklistId: string,
    itemId: string,
    body: {
      responseValue?: string
      result?: string
      findingCreated?: boolean
      findingRef?: string
      evidenceRecordRefs?: string[]
      answeredByPersonId?: string
      answeredAt?: string
    },
  ) =>
    sendJson<AuditChecklistItem>(`/api/v1/audits/${auditId}/checklists/${checklistId}/items/${itemId}/response`, 'PATCH', {
      ...body,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
      findingCreated: body.findingCreated ?? false,
      answeredAt: body.answeredAt ? new Date(body.answeredAt).toISOString() : null,
    }),
  listFindings: () => getJson<Finding[]>('/api/v1/findings'),
  getFinding: (id: string) => getJson<Finding>(`/api/v1/findings/${id}`),
  createFinding: (body: CreateBase & { findingType: string; sourceRequirementRef?: string; auditRef?: string; nonconformanceRef?: string; capaRef?: string; dueAt?: string; evidenceRecordRefs?: string[] }) =>
    sendJson<Finding>('/api/v1/findings', 'POST', {
      ...body,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
      sourceRequirementRef: body.sourceRequirementRef || null,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
    }),
  updateFindingStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Finding>(`/api/v1/findings/${id}/status`, 'PATCH', { status, closureSummary }),
  listSnapshots: () => getJson<StatusSnapshot[]>('/api/v1/status-snapshots'),
  createSnapshot: (body: CreateBase & { targetProduct: string; targetObjectRef: string; qualityStatus: string; activeHoldRefs?: string[]; openNonconformanceRefs?: string[]; openCapaRefs?: string[]; openFindingRefs?: string[]; expiresAt?: string; notes?: string }) =>
    sendJson<StatusSnapshot>('/api/v1/status-snapshots', 'POST', {
      ...body,
      activeHoldRefs: body.activeHoldRefs ?? [],
      openNonconformanceRefs: body.openNonconformanceRefs ?? [],
      openCapaRefs: body.openCapaRefs ?? [],
      openFindingRefs: body.openFindingRefs ?? [],
      expiresAt: body.expiresAt ? new Date(body.expiresAt).toISOString() : null,
      notes: body.notes || null,
    }),
  getSnapshot: (id: string) => getJson<StatusSnapshot>(`/api/v1/status-snapshots/${id}`),
  listQualityStatus: () => getJson<QualityStatus[]>('/api/v1/integrations/quality-status'),
  getQualityStatus: (targetProduct: string, targetObjectId: string) => getJson<QualityStatus>(`/api/v1/integrations/quality-status/${targetProduct}/${targetObjectId}`),
  createQualityStatusCheck: (body: CreateBase & { targetProduct: string; targetObjectRef: string; qualityStatus: string; activeHoldRefs?: string[]; openNonconformanceRefs?: string[]; openCapaRefs?: string[]; openFindingRefs?: string[]; expiresAt?: string }) =>
    sendJson<QualityStatus>('/api/v1/integrations/quality-status-checks', 'POST', {
      ...body,
      activeHoldRefs: body.activeHoldRefs ?? [],
      openNonconformanceRefs: body.openNonconformanceRefs ?? [],
      openCapaRefs: body.openCapaRefs ?? [],
      openFindingRefs: body.openFindingRefs ?? [],
      expiresAt: body.expiresAt ? new Date(body.expiresAt).toISOString() : null,
    }),
  listScorecards: () => getJson<Scorecard[]>('/api/v1/integrations/scorecards'),
  getScorecard: (id: string) => getJson<Scorecard>(`/api/v1/scorecards/${id}`),
  createScorecard: (body: CreateBase & { targetType: string; targetRef: string; periodStart: string; periodEnd: string; overallScore?: number; qualityStatus: string; trend: string; metricRefs?: string[] }) =>
    sendJson<Scorecard>('/api/v1/scorecards', 'POST', {
      ...body,
      periodStart: new Date(body.periodStart).toISOString(),
      periodEnd: new Date(body.periodEnd).toISOString(),
      metricRefs: body.metricRefs ?? [],
    }),
  reviewScorecard: (id: string, body: { reviewedByPersonId?: string | null; reviewedAt?: string | null }) =>
    sendJson<Scorecard>(`/api/v1/integrations/scorecards/${id}/review`, 'POST', {
      reviewedByPersonId: body.reviewedByPersonId ?? null,
      reviewedAt: body.reviewedAt ? new Date(body.reviewedAt).toISOString() : null,
    }),
  getQualityMetric: (scorecardId: string, metricId: string) =>
    getJson<QualityMetric>(`/api/v1/scorecards/${scorecardId}/metrics/${metricId}`),
  listQualityMetrics: (scorecardId: string) => getJson<QualityMetric[]>(`/api/v1/scorecards/${scorecardId}/metrics`),
  createQualityMetric: (scorecardId: string, body: { metricKey: string; title: string; description: string; category: string; value?: number | null; numerator?: number | null; denominator?: number | null; unit?: string | null; targetValue?: number | null; warningThreshold?: number | null; criticalThreshold?: number | null; status: string; sourceProductRefs?: string[] }) =>
    sendJson<QualityMetric>(`/api/v1/scorecards/${scorecardId}/metrics`, 'POST', {
      ...body,
      sourceProductRefs: body.sourceProductRefs ?? [],
    }),
  listRiskProfiles: () => getJson<QualityRiskProfile[]>('/api/v1/integrations/risk-profiles'),
  getRiskProfile: (id: string) => getJson<QualityRiskProfile>(`/api/v1/integrations/risk-profiles/${id}`),
  createRiskProfile: (body: { targetType: string; targetRef: string; riskLevel: string; riskFactors?: string[]; openIssueCount: number; repeatIssueCount: number; criticalIssueCount: number; lastIncidentAt?: string | null; mitigationActions?: string[]; reviewedAt?: string | null; reviewedByPersonId?: string | null }) =>
    sendJson<QualityRiskProfile>('/api/v1/integrations/risk-profiles', 'POST', {
      ...body,
      riskFactors: body.riskFactors ?? [],
      mitigationActions: body.mitigationActions ?? [],
      lastIncidentAt: body.lastIncidentAt ? new Date(body.lastIncidentAt).toISOString() : null,
      reviewedAt: body.reviewedAt ? new Date(body.reviewedAt).toISOString() : null,
    }),
  listQualityReviews: () => getJson<QualityReview[]>('/api/v1/integrations/quality-reviews'),
  getQualityReview: (id: string) => getJson<QualityReview>(`/api/v1/integrations/quality-reviews/${id}`),
  createQualityReview: (body: CreateBase & { reviewType: string; sourceReviewRef?: string; reviewerPersonId?: string; requestedAt?: string; dueAt?: string; decisionReason?: string; requiredEvidenceRefs?: string[]; submittedEvidenceRefs?: string[]; notes?: string }) =>
    sendJson<QualityReview>('/api/v1/integrations/quality-reviews', 'POST', {
      ...body,
      requestedAt: body.requestedAt ? new Date(body.requestedAt).toISOString() : null,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
      requiredEvidenceRefs: body.requiredEvidenceRefs ?? [],
      submittedEvidenceRefs: body.submittedEvidenceRefs ?? [],
    }),
  updateQualityReviewStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<QualityReview>(`/api/v1/integrations/quality-reviews/${id}/status`, 'PATCH', { status, closureSummary }),
  listQualityReleases: () => getJson<QualityRelease[]>('/api/v1/integrations/quality-releases'),
  getQualityRelease: (id: string) => getJson<QualityRelease>(`/api/v1/integrations/quality-releases/${id}`),
  createQualityRelease: (body: CreateBase & { holdRef: string; releaseType: string; requestedByPersonId?: string; requestedAt?: string; conditions?: string; expirationAt?: string; evidenceRecordRefs?: string[]; notes?: string }) =>
    sendJson<QualityRelease>('/api/v1/integrations/quality-releases', 'POST', {
      ...body,
      requestedAt: body.requestedAt ? new Date(body.requestedAt).toISOString() : null,
      expirationAt: body.expirationAt ? new Date(body.expirationAt).toISOString() : null,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
    }),
  updateQualityReleaseStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<QualityRelease>(`/api/v1/integrations/quality-releases/${id}/status`, 'PATCH', { status, closureSummary }),
  listContainmentActions: () => getJson<ContainmentAction[]>('/api/v1/integrations/containment-actions'),
  createContainmentAction: (body: CreateBase & { actionType: string; nonconformanceRef?: string; assignedPersonId?: string; assignedTeamRef?: string; sourceProductActionRef?: string; dueAt?: string; verificationRequired?: boolean; evidenceRecordRefs?: string[]; notes?: string }) =>
    sendJson<ContainmentAction>('/api/v1/integrations/containment-actions', 'POST', {
      ...body,
      verificationRequired: body.verificationRequired ?? true,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
    }),
  updateContainmentActionStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<ContainmentAction>(`/api/v1/integrations/containment-actions/${id}/status`, 'PATCH', { status, closureSummary }),
  getContainmentAction: (id: string) => getJson<ContainmentAction>(`/api/v1/integrations/containment-actions/${id}`),
  listDispositions: () => getJson<Disposition[]>('/api/v1/integrations/dispositions'),
  createDisposition: (body: CreateBase & { dispositionType: string; nonconformanceRef?: string; decisionByPersonId?: string; decisionAt?: string; approvedByPersonId?: string; approvedAt?: string; rationale?: string; requiredActions?: string[]; executionProduct?: string; executionObjectRef?: string; evidenceRecordRefs?: string[]; notes?: string }) =>
    sendJson<Disposition>('/api/v1/integrations/dispositions', 'POST', {
      ...body,
      decisionAt: body.decisionAt ? new Date(body.decisionAt).toISOString() : null,
      approvedAt: body.approvedAt ? new Date(body.approvedAt).toISOString() : null,
      requiredActions: body.requiredActions ?? [],
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
    }),
  updateDispositionStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Disposition>(`/api/v1/integrations/dispositions/${id}/status`, 'PATCH', { status, closureSummary }),
  getDisposition: (id: string) => getJson<Disposition>(`/api/v1/integrations/dispositions/${id}`),
  listSupplierQualityIssues: () => getJson<SupplierQualityIssue[]>('/api/v1/integrations/supplier-quality-issues'),
  getSupplierQualityIssue: (id: string) => getJson<SupplierQualityIssue>(`/api/v1/integrations/supplier-quality-issues/${id}`),
  createSupplierQualityIssue: (body: CreateBase & { issueType: string; affectedReceiptRefs?: string[]; affectedPurchaseOrderRefs?: string[]; affectedItemRefs?: string[]; supplierRef?: string; nonconformanceRef?: string; scarRef?: string; holdRefs?: string[]; recordRefs?: string[]; openedAt?: string }) =>
    sendJson<SupplierQualityIssue>('/api/v1/integrations/supplier-quality-issues', 'POST', {
      ...body,
      affectedReceiptRefs: body.affectedReceiptRefs ?? [],
      affectedPurchaseOrderRefs: body.affectedPurchaseOrderRefs ?? [],
      affectedItemRefs: body.affectedItemRefs ?? [],
      holdRefs: body.holdRefs ?? [],
      recordRefs: body.recordRefs ?? [],
      openedAt: body.openedAt ? new Date(body.openedAt).toISOString() : null,
    }),
  updateSupplierQualityIssueStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<SupplierQualityIssue>(`/api/v1/integrations/supplier-quality-issues/${id}/status`, 'PATCH', { status, closureSummary }),
  listScars: () => getJson<SupplierCorrectiveActionRequest[]>('/api/v1/integrations/scars'),
  getScar: (id: string) => getJson<SupplierCorrectiveActionRequest>(`/api/v1/integrations/scars/${id}`),
  createScar: (body: CreateBase & { supplierRef?: string; sourceNonconformanceRef?: string; sourceCapaRef?: string; requestedByPersonId?: string; requestedAt?: string; supplierDueAt?: string; supplierResponseRecordRefs?: string[]; reviewPersonId?: string; reviewedAt?: string; reviewDecision?: string; followUpCapaRef?: string; recordRefs?: string[] }) =>
    sendJson<SupplierCorrectiveActionRequest>('/api/v1/integrations/scars', 'POST', {
      ...body,
      affectedObjectRefs: body.affectedObjectRefs ?? [],
      requestedAt: body.requestedAt ? new Date(body.requestedAt).toISOString() : null,
      supplierDueAt: body.supplierDueAt ? new Date(body.supplierDueAt).toISOString() : null,
      supplierResponseRecordRefs: body.supplierResponseRecordRefs ?? [],
      reviewedAt: body.reviewedAt ? new Date(body.reviewedAt).toISOString() : null,
      recordRefs: body.recordRefs ?? [],
    }),
  updateScarStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<SupplierCorrectiveActionRequest>(`/api/v1/integrations/scars/${id}/status`, 'PATCH', { status, closureSummary }),
  listCustomerComplaintQualityCases: () => getJson<CustomerComplaintQualityCase[]>('/api/v1/integrations/customer-complaint-quality-cases'),
  getCustomerComplaintQualityCase: (id: string) => getJson<CustomerComplaintQualityCase>(`/api/v1/integrations/customer-complaint-quality-cases/${id}`),
  createCustomerComplaintQualityCase: (body: CreateBase & { complaintType: string; affectedOrderRefs?: string[]; affectedShipmentRefs?: string[]; affectedItemRefs?: string[]; affectedAssetRefs?: string[]; customerRef?: string; customerContactSnapshot?: string; customerLocationRef?: string; nonconformanceRef?: string; holdRefs?: string[]; capaRefs?: string[]; customerResponseRecordRefs?: string[]; recordRefs?: string[]; receivedAt?: string; receivedByPersonId?: string; customerResponseDueAt?: string }) =>
    sendJson<CustomerComplaintQualityCase>('/api/v1/integrations/customer-complaint-quality-cases', 'POST', {
      ...body,
      affectedOrderRefs: body.affectedOrderRefs ?? [],
      affectedShipmentRefs: body.affectedShipmentRefs ?? [],
      affectedItemRefs: body.affectedItemRefs ?? [],
      affectedAssetRefs: body.affectedAssetRefs ?? [],
      holdRefs: body.holdRefs ?? [],
      capaRefs: body.capaRefs ?? [],
      customerResponseRecordRefs: body.customerResponseRecordRefs ?? [],
      recordRefs: body.recordRefs ?? [],
      receivedAt: body.receivedAt ? new Date(body.receivedAt).toISOString() : null,
      customerResponseDueAt: body.customerResponseDueAt ? new Date(body.customerResponseDueAt).toISOString() : null,
    }),
  updateCustomerComplaintQualityCaseStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<CustomerComplaintQualityCase>(`/api/v1/integrations/customer-complaint-quality-cases/${id}/status`, 'PATCH', { status, closureSummary }),
}
