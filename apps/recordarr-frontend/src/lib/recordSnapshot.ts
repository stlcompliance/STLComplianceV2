import type { RecordArrRecord } from '../api/client'

export type RecordSnapshotEntry = {
  label: string
  value: string
}

function joinValues(values: readonly string[]) {
  return values.filter((value) => value.trim().length > 0).join(', ')
}

export function buildRecordSnapshotSummary(record: RecordArrRecord, ownerDisplayName: string): RecordSnapshotEntry[] {
  return [
    { label: 'Record', value: `${record.recordNumber} · ${record.title}` },
    { label: 'Source', value: `${record.sourceProduct} · ${record.sourceObjectDisplayName}` },
    { label: 'Source type', value: record.sourceObjectType },
    { label: 'Owner', value: ownerDisplayName || 'Not recorded' },
    { label: 'Current file', value: `${record.currentFileName} (${record.currentMimeType})` },
    { label: 'Version', value: `v${record.versionNumber}` },
    { label: 'Audit trail', value: `${record.auditTrail.length} entr${record.auditTrail.length === 1 ? 'y' : 'ies'}` },
    {
      label: 'Record reference',
      value: record.recordRef ? `${record.recordRef.recordNumberSnapshot} · ${record.recordRef.titleSnapshot}` : 'n/a',
    },
    {
      label: 'Retention',
      value: record.recordRef?.retentionStatusSnapshot ?? 'n/a',
    },
  ]
}

export function buildRecordTechnicalDetails(record: RecordArrRecord): RecordSnapshotEntry[] {
  return [
    { label: 'Record ID', value: record.recordId },
    { label: 'Source object ID', value: record.sourceObjectId },
    { label: 'Owner person ID', value: record.ownerPersonId },
    { label: 'Uploaded by person ID', value: record.uploadedByPersonId ?? 'Not recorded' },
    { label: 'Current file ref', value: record.currentFileRef },
    { label: 'Current version ref', value: record.currentVersionRef },
    { label: 'Source object refs', value: joinValues(record.sourceObjectRefs) || 'none' },
    { label: 'Metadata refs', value: joinValues(record.metadataRefs) || 'none' },
    { label: 'Version refs', value: joinValues(record.versionRefs) || 'none' },
    { label: 'OCR result refs', value: joinValues(record.ocrResultRefs) || 'none' },
    { label: 'Extraction result refs', value: joinValues(record.extractionResultRefs) || 'none' },
    { label: 'Evidence mapping refs', value: joinValues(record.evidenceMappingRefs) || 'none' },
    { label: 'Package refs', value: joinValues(record.packageRefs) || 'none' },
    { label: 'Legal hold refs', value: joinValues(record.legalHoldRefs) || 'none' },
    { label: 'Access policy ref', value: record.accessPolicyRef ?? 'n/a' },
    { label: 'Compliance refs', value: joinValues(record.complianceRefs) || 'none' },
    { label: 'Record reference ID', value: record.recordRef?.recordarrRecordId ?? 'n/a' },
  ]
}
