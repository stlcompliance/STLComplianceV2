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
