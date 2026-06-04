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
}

export type Nonconformance = ListItem & {
  description: string
  nonconformanceType: string
  category: string
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  customerImpact: string | null
  supplierImpact: string | null
  safetyImpact: string | null
  complianceImpact: string | null
  recurrenceFlag: boolean
  repeatOfNonconformanceRef: string | null
  dueAt: string | null
}

export type QualityHold = ListItem & {
  description: string
  holdType: string
  holdScope: string
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  holdReason: string | null
  releaseReason: string | null
  rejectionReason: string | null
  conditionalReleaseTerms: string | null
  quantityHeld: number | null
  unitOfMeasure: string | null
  lotNumber: string | null
  serialNumber: string | null
  placedAt: string | null
  placedByPersonId: string | null
  releasedAt: string | null
  releasedByPersonId: string | null
  expiresAt: string | null
}

export type Capa = ListItem & {
  description: string
  capaType: string
  sourceType: string
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  sponsorPersonId: string | null
  rootCauseSummary: string | null
  dueAt: string | null
  relatedNonconformanceRefs: string[]
  relatedAuditFindingRefs: string[]
}

export type Audit = ListItem & {
  description: string
  auditType: string
  auditScope: string | null
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  auditorPersonIds: string[]
  leadAuditorPersonId: string | null
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
}

export type Finding = ListItem & {
  description: string
  findingType: string
  recordRefs: string[]
  closedAt: string | null
  closedByPersonId: string | null
  closureSummary: string | null
  auditRef: string | null
  nonconformanceRef: string | null
  capaRef: string | null
  dueAt: string | null
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
  recordRefs: string[]
}

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

export const assurarrApi = {
  getDashboard: () => getJson<DashboardResponse>('/api/v1/dashboard'),
  listNonconformances: () => getJson<Nonconformance[]>('/api/v1/nonconformances'),
  createNonconformance: (body: CreateBase & { nonconformanceType: string; category: string; recurrenceFlag?: boolean; dueAt?: string }) =>
    sendJson<Nonconformance>('/api/v1/nonconformances', 'POST', {
      ...body,
      recurrenceFlag: body.recurrenceFlag ?? false,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
    }),
  updateNonconformanceStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Nonconformance>(`/api/v1/nonconformances/${id}/status`, 'PATCH', { status, closureSummary }),
  listHolds: () => getJson<QualityHold[]>('/api/v1/holds'),
  createHold: (body: CreateBase & { holdType: string; holdScope: string; holdReason?: string; quantityHeld?: number; unitOfMeasure?: string; lotNumber?: string; serialNumber?: string; expiresAt?: string }) =>
    sendJson<QualityHold>('/api/v1/holds', 'POST', {
      ...body,
      expiresAt: body.expiresAt ? new Date(body.expiresAt).toISOString() : null,
    }),
  updateHoldStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<QualityHold>(`/api/v1/holds/${id}/status`, 'PATCH', { status, closureSummary }),
  listCapas: () => getJson<Capa[]>('/api/v1/capas'),
  createCapa: (body: CreateBase & { capaType: string; sourceType: string; sponsorPersonId?: string; rootCauseSummary?: string; dueAt?: string; relatedNonconformanceRefs?: string[]; relatedAuditFindingRefs?: string[] }) =>
    sendJson<Capa>('/api/v1/capas', 'POST', {
      ...body,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
      relatedNonconformanceRefs: body.relatedNonconformanceRefs ?? [],
      relatedAuditFindingRefs: body.relatedAuditFindingRefs ?? [],
    }),
  updateCapaStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Capa>(`/api/v1/capas/${id}/status`, 'PATCH', { status, closureSummary }),
  listAudits: () => getJson<Audit[]>('/api/v1/audits'),
  createAudit: (body: CreateBase & { auditType: string; auditScope?: string; auditorPersonIds?: string[]; leadAuditorPersonId?: string; staffArrSiteId?: string; staffArrLocationId?: string; supplierRef?: string; customerRef?: string; plannedStartAt?: string; plannedEndAt?: string; checklistRefs?: string[] }) =>
    sendJson<Audit>('/api/v1/audits', 'POST', {
      ...body,
      auditorPersonIds: body.auditorPersonIds ?? [],
      checklistRefs: body.checklistRefs ?? [],
      plannedStartAt: body.plannedStartAt ? new Date(body.plannedStartAt).toISOString() : null,
      plannedEndAt: body.plannedEndAt ? new Date(body.plannedEndAt).toISOString() : null,
    }),
  updateAuditStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Audit>(`/api/v1/audits/${id}/status`, 'PATCH', { status, closureSummary }),
  listFindings: () => getJson<Finding[]>('/api/v1/findings'),
  createFinding: (body: CreateBase & { findingType: string; auditRef?: string; nonconformanceRef?: string; capaRef?: string; dueAt?: string }) =>
    sendJson<Finding>('/api/v1/findings', 'POST', {
      ...body,
      dueAt: body.dueAt ? new Date(body.dueAt).toISOString() : null,
    }),
  updateFindingStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<Finding>(`/api/v1/findings/${id}/status`, 'PATCH', { status, closureSummary }),
  listSnapshots: () => getJson<StatusSnapshot[]>('/api/v1/status-snapshots'),
  createSnapshot: (body: CreateBase & { targetProduct: string; targetObjectRef: string; qualityStatus: string; activeHoldRefs?: string[]; openNonconformanceRefs?: string[]; openCapaRefs?: string[]; openFindingRefs?: string[]; expiresAt?: string }) =>
    sendJson<StatusSnapshot>('/api/v1/status-snapshots', 'POST', {
      ...body,
      activeHoldRefs: body.activeHoldRefs ?? [],
      openNonconformanceRefs: body.openNonconformanceRefs ?? [],
      openCapaRefs: body.openCapaRefs ?? [],
      openFindingRefs: body.openFindingRefs ?? [],
      expiresAt: body.expiresAt ? new Date(body.expiresAt).toISOString() : null,
    }),
  listScorecards: () => getJson<Scorecard[]>('/api/v1/scorecards'),
  createScorecard: (body: CreateBase & { targetType: string; targetRef: string; periodStart: string; periodEnd: string; overallScore?: number; qualityStatus: string; trend: string; metricRefs?: string[] }) =>
    sendJson<Scorecard>('/api/v1/scorecards', 'POST', {
      ...body,
      periodStart: new Date(body.periodStart).toISOString(),
      periodEnd: new Date(body.periodEnd).toISOString(),
      metricRefs: body.metricRefs ?? [],
    }),
  listQualityReviews: () => getJson<QualityReview[]>('/api/v1/integrations/quality-reviews'),
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
  createQualityRelease: (body: CreateBase & { holdRef: string; releaseType: string; requestedByPersonId?: string; requestedAt?: string; conditions?: string; expirationAt?: string; evidenceRecordRefs?: string[]; notes?: string }) =>
    sendJson<QualityRelease>('/api/v1/integrations/quality-releases', 'POST', {
      ...body,
      requestedAt: body.requestedAt ? new Date(body.requestedAt).toISOString() : null,
      expirationAt: body.expirationAt ? new Date(body.expirationAt).toISOString() : null,
      evidenceRecordRefs: body.evidenceRecordRefs ?? [],
    }),
  updateQualityReleaseStatus: (id: string, status: string, closureSummary?: string) =>
    sendJson<QualityRelease>(`/api/v1/integrations/quality-releases/${id}/status`, 'PATCH', { status, closureSummary }),
}
