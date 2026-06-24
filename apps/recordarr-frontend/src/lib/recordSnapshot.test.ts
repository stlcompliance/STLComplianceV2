import { describe, expect, it } from 'vitest'

import { buildRecordSnapshotSummary, buildRecordTechnicalDetails } from './recordSnapshot'
import type { RecordArrRecord } from '../api/client'

const sampleRecord: RecordArrRecord = {
  recordId: 'record-123',
  recordNumber: 'REC-2026-001',
  title: 'Shipment manifest',
  description: 'Outbound manifest for June run',
  recordType: 'document',
  documentClass: 'operations',
  documentType: 'manifest',
  documentSubtype: 'outbound',
  status: 'active',
  classification: 'internal',
  sourceProduct: 'loadarr',
  sourceObjectType: 'shipment',
  sourceObjectId: 'source-987',
  sourceObjectDisplayName: 'Load 987',
  ownerPersonId: 'person-42',
  uploadedByPersonId: 'person-77',
  uploadedAt: '2026-06-10T10:00:00Z',
  effectiveAt: '2026-06-10T10:00:00Z',
  expiresAt: null,
  archivedAt: null,
  purgedAt: null,
  currentFileName: 'manifest.pdf',
  currentMimeType: 'application/pdf',
  versionNumber: 3,
  tags: ['priority', 'dispatch'],
  currentFileRef: 'file-ref-1',
  fileRefs: ['file-ref-1'],
  currentVersionRef: 'version-ref-1',
  sourceObjectRefs: ['source-ref-1'],
  metadataRefs: ['meta-ref-1'],
  versionRefs: ['version-ref-1'],
  ocrResultRefs: ['ocr-ref-1'],
  extractionResultRefs: ['extract-ref-1'],
  evidenceMappingRefs: ['evidence-ref-1'],
  packageRefs: ['pkg-ref-1'],
  retentionPolicyRef: 'retention-policy-1',
  retentionStatusRef: 'retention-status-1',
  legalHoldRefs: ['hold-ref-1'],
  accessPolicyRef: 'access-policy-1',
  complianceRefs: ['compliance-ref-1'],
  auditTrail: [
    {
      auditTrailEntryId: 'audit-1',
      action: 'created',
      actorPersonId: 'person-42',
      occurredAt: '2026-06-10T10:00:00Z',
      details: 'Created from test fixture',
    },
  ],
  recordRef: {
    recordarrRecordId: 'record-arr-1',
    recordNumberSnapshot: 'REC-2026-001',
    titleSnapshot: 'Shipment manifest',
    recordTypeSnapshot: 'document',
    documentClassSnapshot: 'operations',
    documentTypeSnapshot: 'manifest',
    documentSubtypeSnapshot: 'outbound',
    statusSnapshot: 'active',
    classificationSnapshot: 'internal',
    versionSnapshot: 3,
    expiresAtSnapshot: null,
    retentionStatusSnapshot: 'active',
    lastResolvedAt: '2026-06-10T10:00:00Z',
  },
}

describe('recordSnapshot helpers', () => {
  it('keeps the summary human readable and separates technical identifiers', () => {
    const summary = buildRecordSnapshotSummary(sampleRecord, 'Jordan Lee')
    const technical = buildRecordTechnicalDetails(sampleRecord)

    expect(summary.find((entry) => entry.label === 'Owner')?.value).toBe('Jordan Lee')
    expect(summary.find((entry) => entry.label === 'Record reference')?.value).toBe('REC-2026-001 · Shipment manifest')
    expect(summary.some((entry) => entry.value.includes('person-42'))).toBe(false)
    expect(technical.find((entry) => entry.label === 'Owner person ID')?.value).toBe('person-42')
    expect(technical.find((entry) => entry.label === 'Record reference ID')?.value).toBe('record-arr-1')
  })
})
