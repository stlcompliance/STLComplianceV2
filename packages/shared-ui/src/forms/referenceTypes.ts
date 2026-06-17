export type CrossProductReference = {
  ownerProductKey: string
  referenceType: string
  referenceId: string
  displayLabelSnapshot: string
  secondaryLabelSnapshot?: string | null
  statusSnapshot?: string | null
  ownerVersion?: string | null
  createdVia: string
}

export type ReferenceTypeDescriptor = {
  ownerProductKey: string
  referenceType: string
  label: string
  canSearch: boolean
  canQuickCreate: boolean
  quickCreatePermission?: string | null
  description?: string | null
}

export type ReferenceSearchRequest = {
  referenceType: string
  query?: string | null
  limit?: number
  filters?: Record<string, string>
}

export type ReferenceSummaryResponse = {
  ownerProductKey: string
  referenceType: string
  referenceId: string
  displayLabel: string
  secondaryLabel?: string | null
  status?: string | null
  ownerVersion?: string | null
  detailPath?: string | null
  metadata?: Record<string, string> | null
}

export type ReferenceSearchResponse = {
  results: ReferenceSummaryResponse[]
}

export type QuickCreateOptionDescriptor = {
  value: string
  label: string
}

export type QuickCreateFieldDescriptor = {
  key: string
  label: string
  fieldType: 'text' | 'email' | 'tel' | 'textarea' | 'select' | string
  required: boolean
  placeholder?: string | null
  defaultValue?: string | null
  options?: QuickCreateOptionDescriptor[] | null
}

export type QuickCreateSchemaResponse = {
  ownerProductKey: string
  referenceType: string
  allowed: boolean
  managedByLabel: string
  permissionKey?: string | null
  disabledReason?: string | null
  fields?: QuickCreateFieldDescriptor[] | null
}

export type QuickCreateRequest = {
  referenceType: string
  values: Record<string, string>
  duplicateDisposition?: string | null
}

export type DuplicateCandidateResponse = {
  referenceId: string
  displayLabel: string
  secondaryLabel?: string | null
  status?: string | null
  matchReason: string
  confidence: number
}

export type QuickCreateResponse = {
  reference?: CrossProductReference | null
  duplicateCandidates: DuplicateCandidateResponse[]
  created: boolean
  reviewStatus?: string | null
  message?: string | null
}

export function referenceSummaryToSnapshot(
  summary: ReferenceSummaryResponse,
  createdVia = 'selected',
): CrossProductReference {
  return {
    ownerProductKey: summary.ownerProductKey,
    referenceType: summary.referenceType,
    referenceId: summary.referenceId,
    displayLabelSnapshot: summary.displayLabel,
    secondaryLabelSnapshot: summary.secondaryLabel,
    statusSnapshot: summary.status,
    ownerVersion: summary.ownerVersion,
    createdVia,
  }
}

export function referenceSnapshotToSummary(
  value: CrossProductReference,
): ReferenceSummaryResponse {
  return {
    ownerProductKey: value.ownerProductKey,
    referenceType: value.referenceType,
    referenceId: value.referenceId,
    displayLabel: value.displayLabelSnapshot,
    secondaryLabel: value.secondaryLabelSnapshot,
    status: value.statusSnapshot,
    ownerVersion: value.ownerVersion,
  }
}
