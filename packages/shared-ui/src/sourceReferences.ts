import { IMPLEMENTED_PRODUCT_OWNERSHIP, normalizeProductKey } from './productOwnershipManifest'
import type { PickerOption } from './forms/pickerTypes'

export type SourceReferenceOption = PickerOption & {
  sourceProduct: string
  sourceObjectType: string
  sourceObjectId: string
  sourceObjectDisplayName: string
}

export const SUITE_SOURCE_PRODUCT_OPTIONS: PickerOption[] = IMPLEMENTED_PRODUCT_OWNERSHIP.map((product) => ({
  value: product.productKey,
  label: product.displayName,
}))

export const SUITE_SOURCE_REFERENCE_OPTIONS: SourceReferenceOption[] = [
  {
    value: 'loadarr:receiving_session:RR-24018',
    label: 'RR-24018 receiving session - LoadArr',
    sourceProduct: 'loadarr',
    sourceObjectType: 'receiving_session',
    sourceObjectId: 'RR-24018',
    sourceObjectDisplayName: 'RR-24018 receiving session',
  },
  {
    value: 'loadarr:inventory_lot:LOT-991',
    label: 'LOT-991 inventory lot - LoadArr',
    sourceProduct: 'loadarr',
    sourceObjectType: 'inventory_lot',
    sourceObjectId: 'LOT-991',
    sourceObjectDisplayName: 'LOT-991 inventory lot',
  },
  {
    value: 'loadarr:inventory_hold:HOLD-000001',
    label: 'HOLD-000001 inventory hold - LoadArr',
    sourceProduct: 'loadarr',
    sourceObjectType: 'inventory_hold',
    sourceObjectId: 'HOLD-000001',
    sourceObjectDisplayName: 'HOLD-000001 inventory hold',
  },
  {
    value: 'assurarr:nonconformance:NCR-000001',
    label: 'NCR-000001 nonconformance - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'nonconformance',
    sourceObjectId: 'NCR-000001',
    sourceObjectDisplayName: 'NCR-000001 nonconformance',
  },
  {
    value: 'assurarr:quality_hold:HOLD-000001',
    label: 'HOLD-000001 quality hold - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'quality_hold',
    sourceObjectId: 'HOLD-000001',
    sourceObjectDisplayName: 'HOLD-000001 quality hold',
  },
  {
    value: 'assurarr:capa:CAPA-000001',
    label: 'CAPA-000001 corrective action - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'capa',
    sourceObjectId: 'CAPA-000001',
    sourceObjectDisplayName: 'CAPA-000001 corrective action',
  },
  {
    value: 'assurarr:supplier_quality_issue:SQA-000001',
    label: 'SQA-000001 supplier quality issue - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'supplier_quality_issue',
    sourceObjectId: 'SQA-000001',
    sourceObjectDisplayName: 'SQA-000001 supplier quality issue',
  },
  {
    value: 'assurarr:scar:SCAR-000001',
    label: 'SCAR-000001 supplier corrective action - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'scar',
    sourceObjectId: 'SCAR-000001',
    sourceObjectDisplayName: 'SCAR-000001 supplier corrective action',
  },
  {
    value: 'routarr:trip:trip-7781',
    label: 'Trip 7781 - RoutArr',
    sourceProduct: 'routarr',
    sourceObjectType: 'trip',
    sourceObjectId: 'trip-7781',
    sourceObjectDisplayName: 'Trip 7781',
  },
  {
    value: 'routarr:route:route-4472',
    label: 'Route 4472 - RoutArr',
    sourceProduct: 'routarr',
    sourceObjectType: 'route',
    sourceObjectId: 'route-4472',
    sourceObjectDisplayName: 'Route 4472',
  },
  {
    value: 'maintainarr:work_order:WO-1042',
    label: 'WO-1042 work order - MaintainArr',
    sourceProduct: 'maintainarr',
    sourceObjectType: 'work_order',
    sourceObjectId: 'WO-1042',
    sourceObjectDisplayName: 'WO-1042 work order',
  },
  {
    value: 'maintainarr:asset:asset-trk-1042',
    label: 'TRK-1042 asset - MaintainArr',
    sourceProduct: 'maintainarr',
    sourceObjectType: 'asset',
    sourceObjectId: 'asset-trk-1042',
    sourceObjectDisplayName: 'TRK-1042 asset',
  },
  {
    value: 'supplyarr:purchase_order:PO-10492',
    label: 'PO-10492 purchase order - SupplyArr',
    sourceProduct: 'supplyarr',
    sourceObjectType: 'purchase_order',
    sourceObjectId: 'PO-10492',
    sourceObjectDisplayName: 'PO-10492 purchase order',
  },
  {
    value: 'supplyarr:supplier:supplier-midwest-fleet',
    label: 'Midwest Fleet Parts supplier - SupplyArr',
    sourceProduct: 'supplyarr',
    sourceObjectType: 'supplier',
    sourceObjectId: 'supplier-midwest-fleet',
    sourceObjectDisplayName: 'Midwest Fleet Parts',
  },
  {
    value: 'customarr:customer:CUS-1001',
    label: 'CUS-1001 customer - CustomArr',
    sourceProduct: 'customarr',
    sourceObjectType: 'customer',
    sourceObjectId: 'CUS-1001',
    sourceObjectDisplayName: 'CUS-1001 customer',
  },
  {
    value: 'customarr:case:CASE-1001',
    label: 'CASE-1001 customer case - CustomArr',
    sourceProduct: 'customarr',
    sourceObjectType: 'case',
    sourceObjectId: 'CASE-1001',
    sourceObjectDisplayName: 'CASE-1001 customer case',
  },
  {
    value: 'ordarr:order:ORD-1001',
    label: 'ORD-1001 order - OrdArr',
    sourceProduct: 'ordarr',
    sourceObjectType: 'order',
    sourceObjectId: 'ORD-1001',
    sourceObjectDisplayName: 'ORD-1001 order',
  },
  {
    value: 'staffarr:person:person-quality-manager',
    label: 'Quality manager person - StaffArr',
    sourceProduct: 'staffarr',
    sourceObjectType: 'person',
    sourceObjectId: 'person-quality-manager',
    sourceObjectDisplayName: 'Quality manager',
  },
  {
    value: 'trainarr:assignment:TRN-2026-001',
    label: 'TRN-2026-001 assignment - TrainArr',
    sourceProduct: 'trainarr',
    sourceObjectType: 'assignment',
    sourceObjectId: 'TRN-2026-001',
    sourceObjectDisplayName: 'TRN-2026-001 assignment',
  },
  {
    value: 'recordarr:record:REC-000001',
    label: 'REC-000001 record - RecordArr',
    sourceProduct: 'recordarr',
    sourceObjectType: 'record',
    sourceObjectId: 'REC-000001',
    sourceObjectDisplayName: 'REC-000001 record',
  },
  {
    value: 'compliancecore:requirement:REQ-FMCSA-DQF',
    label: 'REQ-FMCSA-DQF evidence requirement - Compliance Core',
    sourceProduct: 'compliancecore',
    sourceObjectType: 'requirement',
    sourceObjectId: 'REQ-FMCSA-DQF',
    sourceObjectDisplayName: 'REQ-FMCSA-DQF evidence requirement',
  },
  {
    value: 'reportarr:dataset:dispatch_performance',
    label: 'Dispatch performance dataset - ReportArr',
    sourceProduct: 'reportarr',
    sourceObjectType: 'dataset',
    sourceObjectId: 'dispatch_performance',
    sourceObjectDisplayName: 'Dispatch performance dataset',
  },
  {
    value: 'reportarr:report:accident_register',
    label: 'Accident register report - ReportArr',
    sourceProduct: 'reportarr',
    sourceObjectType: 'report',
    sourceObjectId: 'accident_register',
    sourceObjectDisplayName: 'Accident register report',
  },
]

export const FACT_SOURCE_REFERENCE_OPTIONS: SourceReferenceOption[] = [
  {
    value: 'staffarr:record_type:person_application',
    label: 'Person application record type - StaffArr',
    sourceProduct: 'staffarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'person_application',
    sourceObjectDisplayName: 'Person application record type',
  },
  {
    value: 'recordarr:document_type:application',
    label: 'Application document type - RecordArr',
    sourceProduct: 'recordarr',
    sourceObjectType: 'document_type',
    sourceObjectId: 'application',
    sourceObjectDisplayName: 'Application document type',
  },
  {
    value: 'loadarr:record_type:receiving_session',
    label: 'Receiving session record type - LoadArr',
    sourceProduct: 'loadarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'receiving_session',
    sourceObjectDisplayName: 'Receiving session record type',
  },
  {
    value: 'loadarr:record_type:inventory_lot',
    label: 'Inventory lot record type - LoadArr',
    sourceProduct: 'loadarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'inventory_lot',
    sourceObjectDisplayName: 'Inventory lot record type',
  },
  {
    value: 'loadarr:record_type:inventory_hold',
    label: 'Inventory hold record type - LoadArr',
    sourceProduct: 'loadarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'inventory_hold',
    sourceObjectDisplayName: 'Inventory hold record type',
  },
  {
    value: 'assurarr:record_type:nonconformance',
    label: 'Nonconformance record type - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'nonconformance',
    sourceObjectDisplayName: 'Nonconformance record type',
  },
  {
    value: 'assurarr:record_type:quality_hold',
    label: 'Quality hold record type - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'quality_hold',
    sourceObjectDisplayName: 'Quality hold record type',
  },
  {
    value: 'assurarr:record_type:capa',
    label: 'CAPA record type - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'capa',
    sourceObjectDisplayName: 'CAPA record type',
  },
  {
    value: 'assurarr:record_type:supplier_quality_issue',
    label: 'Supplier quality issue record type - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'supplier_quality_issue',
    sourceObjectDisplayName: 'Supplier quality issue record type',
  },
  {
    value: 'assurarr:record_type:scar',
    label: 'Supplier corrective action record type - AssurArr',
    sourceProduct: 'assurarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'scar',
    sourceObjectDisplayName: 'Supplier corrective action record type',
  },
  {
    value: 'routarr:record_type:trip',
    label: 'Trip record type - RoutArr',
    sourceProduct: 'routarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'trip',
    sourceObjectDisplayName: 'Trip record type',
  },
  {
    value: 'routarr:record_type:route',
    label: 'Route record type - RoutArr',
    sourceProduct: 'routarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'route',
    sourceObjectDisplayName: 'Route record type',
  },
  {
    value: 'maintainarr:record_type:work_order',
    label: 'Work order record type - MaintainArr',
    sourceProduct: 'maintainarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'work_order',
    sourceObjectDisplayName: 'Work order record type',
  },
  {
    value: 'maintainarr:record_type:asset',
    label: 'Asset record type - MaintainArr',
    sourceProduct: 'maintainarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'asset',
    sourceObjectDisplayName: 'Asset record type',
  },
  {
    value: 'maintainarr:record_type:inspection',
    label: 'Inspection record type - MaintainArr',
    sourceProduct: 'maintainarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'inspection',
    sourceObjectDisplayName: 'Inspection record type',
  },
  {
    value: 'supplyarr:record_type:purchase_order',
    label: 'Purchase order record type - SupplyArr',
    sourceProduct: 'supplyarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'purchase_order',
    sourceObjectDisplayName: 'Purchase order record type',
  },
  {
    value: 'supplyarr:record_type:supplier',
    label: 'Supplier record type - SupplyArr',
    sourceProduct: 'supplyarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'supplier',
    sourceObjectDisplayName: 'Supplier record type',
  },
  {
    value: 'customarr:record_type:customer',
    label: 'Customer record type - CustomArr',
    sourceProduct: 'customarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'customer',
    sourceObjectDisplayName: 'Customer record type',
  },
  {
    value: 'customarr:record_type:case',
    label: 'Customer case record type - CustomArr',
    sourceProduct: 'customarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'case',
    sourceObjectDisplayName: 'Customer case record type',
  },
  {
    value: 'ordarr:record_type:order',
    label: 'Order record type - OrdArr',
    sourceProduct: 'ordarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'order',
    sourceObjectDisplayName: 'Order record type',
  },
  {
    value: 'trainarr:record_type:assignment',
    label: 'Assignment record type - TrainArr',
    sourceProduct: 'trainarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'assignment',
    sourceObjectDisplayName: 'Assignment record type',
  },
  {
    value: 'recordarr:record_type:record',
    label: 'Record type - RecordArr',
    sourceProduct: 'recordarr',
    sourceObjectType: 'record_type',
    sourceObjectId: 'record',
    sourceObjectDisplayName: 'Record type',
  },
  {
    value: 'compliancecore:record_type:requirement',
    label: 'Requirement record type - Compliance Core',
    sourceProduct: 'compliancecore',
    sourceObjectType: 'record_type',
    sourceObjectId: 'requirement',
    sourceObjectDisplayName: 'Requirement record type',
  },
  {
    value: 'compliancecore:calculation:fact_coverage',
    label: 'Fact coverage calculation - Compliance Core',
    sourceProduct: 'compliancecore',
    sourceObjectType: 'calculation',
    sourceObjectId: 'fact_coverage',
    sourceObjectDisplayName: 'Fact coverage calculation',
  },
  {
    value: 'reportarr:report:rpt-001',
    label: 'Executive summary report - ReportArr',
    sourceProduct: 'reportarr',
    sourceObjectType: 'report',
    sourceObjectId: 'rpt-001',
    sourceObjectDisplayName: 'Executive summary report',
  },
  {
    value: 'reportarr:report:rpt-002',
    label: 'Compliance audit readiness report - ReportArr',
    sourceProduct: 'reportarr',
    sourceObjectType: 'report',
    sourceObjectId: 'rpt-002',
    sourceObjectDisplayName: 'Compliance audit readiness report',
  },
  {
    value: 'reportarr:report:rpt-003',
    label: 'Dispatch performance pack report - ReportArr',
    sourceProduct: 'reportarr',
    sourceObjectType: 'report',
    sourceObjectId: 'rpt-003',
    sourceObjectDisplayName: 'Dispatch performance pack report',
  },
]

export function listSourceReferenceOptions(sourceProduct?: string | null): SourceReferenceOption[] {
  const normalized = sourceProduct ? normalizeProductKey(sourceProduct) : ''
  if (!normalized) {
    return SUITE_SOURCE_REFERENCE_OPTIONS
  }

  return SUITE_SOURCE_REFERENCE_OPTIONS.filter(
    (option) => normalizeProductKey(option.sourceProduct) === normalized,
  )
}

export function listFactSourceReferenceOptions(sourceProduct?: string | null): SourceReferenceOption[] {
  const normalized = sourceProduct ? normalizeProductKey(sourceProduct) : ''
  if (!normalized) {
    return FACT_SOURCE_REFERENCE_OPTIONS
  }

  return FACT_SOURCE_REFERENCE_OPTIONS.filter(
    (option) => normalizeProductKey(option.sourceProduct) === normalized,
  )
}

export function getSourceReferenceOption(value?: string | null): SourceReferenceOption | undefined {
  if (!value) {
    return undefined
  }

  return SUITE_SOURCE_REFERENCE_OPTIONS.find((option) => option.value === value)
}

export function buildSourceObjectRef(
  sourceProduct?: string | null,
  sourceObjectType?: string | null,
  sourceObjectId?: string | null,
): string {
  if (!sourceProduct || !sourceObjectType || !sourceObjectId) {
    return ''
  }

  return `${normalizeProductKey(sourceProduct)}:${sourceObjectType.trim()}:${sourceObjectId.trim()}`
}
