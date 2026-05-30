import type { PickerOption } from '@stl/shared-ui'

export const MATERIAL_DEMAND_UOM_OPTIONS: PickerOption[] = [
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

export const EVIDENCE_TYPE_OPTIONS: PickerOption[] = [
  { value: 'completion_certificate', label: 'Completion certificate' },
  { value: 'evaluation_sheet', label: 'Evaluation sheet' },
  { value: 'signoff_form', label: 'Signoff form' },
  { value: 'practical_demo', label: 'Practical demonstration' },
  { value: 'attendance_roster', label: 'Attendance roster' },
]
