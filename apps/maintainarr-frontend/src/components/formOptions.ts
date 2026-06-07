import type { PickerOption } from '@stl/shared-ui'

export const WORK_ORDER_EVIDENCE_TYPE_OPTIONS: PickerOption[] = [
  { value: 'before_photo', label: 'Before photo' },
  { value: 'after_photo', label: 'After photo' },
  { value: 'completion_photo', label: 'Completion photo' },
  { value: 'signature', label: 'Signature' },
  { value: 'document', label: 'Document' },
  { value: 'inspection_photo', label: 'Inspection photo' },
  { value: 'receipt', label: 'Receipt' },
  { value: 'work_order_note', label: 'Work order note' },
]

export const WORK_ORDER_LABOR_TYPE_OPTIONS: PickerOption[] = [
  { value: 'diagnostic', label: 'Diagnostic' },
  { value: 'repair', label: 'Repair' },
  { value: 'inspection', label: 'Inspection' },
  { value: 'testing', label: 'Testing' },
  { value: 'calibration', label: 'Calibration' },
  { value: 'cleanup', label: 'Cleanup' },
  { value: 'admin', label: 'Administrative' },
  { value: 'travel', label: 'Travel' },
  { value: 'vendor_coordination', label: 'Vendor coordination (labor)' },
  { value: 'waiting', label: 'Waiting' },
  { value: 'overtime', label: 'Overtime (legacy)' },
  { value: 'regular', label: 'Regular (legacy)' },
]

export const DEFECT_EVIDENCE_TYPE_OPTIONS: PickerOption[] = [
  { value: 'defect_photo', label: 'Defect photo' },
  { value: 'damage_photo', label: 'Damage photo' },
  { value: 'inspection_photo', label: 'Inspection photo' },
  { value: 'document', label: 'Document' },
  { value: 'signature', label: 'Signature' },
]

export const INSPECTION_EVIDENCE_TYPE_OPTIONS: PickerOption[] = [
  { value: 'inspection_photo', label: 'Inspection photo' },
  { value: 'failed_item_photo', label: 'Failed item photo' },
  { value: 'measurement_photo', label: 'Measurement photo' },
  { value: 'document', label: 'Document' },
  { value: 'signature', label: 'Signature' },
]

export const PARTS_DEMAND_UOM_OPTIONS: PickerOption[] = [
  { value: 'each', label: 'Each' },
  { value: 'box', label: 'Box' },
  { value: 'case', label: 'Case' },
  { value: 'ft', label: 'Feet' },
  { value: 'in', label: 'Inches' },
  { value: 'gal', label: 'Gallons' },
  { value: 'lb', label: 'Pounds' },
  { value: 'kg', label: 'Kilograms' },
  { value: 'l', label: 'Liters' },
  { value: 'ml', label: 'Milliliters' },
]
