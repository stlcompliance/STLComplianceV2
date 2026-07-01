import { buildSemanticKey, slugifyKey, type PickerOption } from '@stl/shared-ui'
import { formatSupplierIdentitySummary, humanizeSupplierUnitKind } from '../utils/supplierPresentation'

export const CURRENCY_OPTIONS: PickerOption[] = [
  { value: 'USD', label: 'USD — US Dollar' },
  { value: 'CAD', label: 'CAD — Canadian Dollar' },
  { value: 'EUR', label: 'EUR — Euro' },
  { value: 'GBP', label: 'GBP — British Pound' },
  { value: 'MXN', label: 'MXN — Mexican Peso' },
]

export const UOM_OPTIONS: PickerOption[] = [
  { value: 'each', label: 'Each' },
  { value: 'box', label: 'Box' },
  { value: 'case', label: 'Case' },
  { value: 'ft', label: 'Foot (ft)' },
  { value: 'in', label: 'Inch (in)' },
  { value: 'gal', label: 'Gallon (gal)' },
  { value: 'lb', label: 'Pound (lb)' },
  { value: 'kg', label: 'Kilogram (kg)' },
  { value: 'l', label: 'Liter (l)' },
  { value: 'ml', label: 'Milliliter (ml)' },
]

export const LOCATION_TYPE_OPTIONS: PickerOption[] = [
  { value: 'warehouse', label: 'Warehouse' },
  { value: 'site', label: 'Site' },
]

/** Workflow/reporting reason codes — API stores combined code + notes in a single reason string. */
export const PROCUREMENT_REJECTION_REASON_OPTIONS: PickerOption[] = [
  { value: 'budget_exceeded', label: 'Budget exceeded' },
  { value: 'wrong_part', label: 'Wrong part or specification' },
  { value: 'duplicate_request', label: 'Duplicate request' },
  { value: 'supplier_unavailable', label: 'Supplier unavailable' },
  { value: 'policy_exception', label: 'Policy exception' },
  { value: 'other', label: 'Other (explain in notes)' },
]

export const PROCUREMENT_CANCEL_REASON_OPTIONS: PickerOption[] = [
  { value: 'no_longer_needed', label: 'No longer needed' },
  { value: 'sourced_elsewhere', label: 'Sourced elsewhere' },
  { value: 'supplier_cancelled', label: 'Supplier cancelled' },
  { value: 'data_entry_error', label: 'Data entry error' },
  { value: 'other', label: 'Other (explain in notes)' },
]

export const WARRANTY_DENIAL_REASON_OPTIONS: PickerOption[] = [
  { value: 'out_of_warranty', label: 'Out of warranty period' },
  { value: 'not_covered', label: 'Not covered by policy' },
  { value: 'insufficient_documentation', label: 'Insufficient documentation' },
  { value: 'misuse_or_damage', label: 'Misuse or damage' },
  { value: 'other', label: 'Other (explain in notes)' },
]

export const EMERGENCY_PURCHASE_REASON_OPTIONS: PickerOption[] = [
  { value: 'safety_critical', label: 'Safety critical' },
  { value: 'production_down', label: 'Production or fleet down' },
  { value: 'regulatory_deadline', label: 'Regulatory deadline' },
  { value: 'sole_source', label: 'Sole source / no substitute' },
  { value: 'other', label: 'Other (explain in notes)' },
]

export interface SupplierUnitPickerSource {
  supplierId: string
  displayName: string
  supplierKey: string
  parentSupplierDisplayName?: string | null
  unitKind?: string | null
}

export function formatProcurementReason(code: string, notes: string): string {
  const trimmedCode = code.trim()
  const trimmedNotes = notes.trim()
  if (!trimmedCode) {
    return trimmedNotes
  }
  if (!trimmedNotes) {
    return trimmedCode
  }
  return `${trimmedCode}: ${trimmedNotes}`
}

type ResolveGeneratedKeyOptions = {
  domain?: string
  kind?: string
  aliases?: readonly string[]
  maxLength?: number
  existingKeys?: readonly string[]
}

export function resolveGeneratedKey(
  sourceLabel: string,
  options: ResolveGeneratedKeyOptions = {},
): string {
  const { domain, kind, aliases, maxLength, existingKeys } = options
  if (domain && kind) {
    return buildSemanticKey({
      domain,
      kind,
      title: sourceLabel,
      aliases,
      maxLength,
      existingKeys: existingKeys ?? [],
    })
  }

  return slugifyKey(sourceLabel)
}

export function keyCollisionWarning(key: string, existingKeys: readonly string[]): string | null {
  if (!key) {
    return null
  }
  const normalized = key.toLowerCase()
  if (existingKeys.some((existing) => existing.toLowerCase() === normalized)) {
    return `Key "${key}" is already in use.`
  }
  return null
}

export function distinctCategoryOptions(parts: { categoryKey: string }[]): PickerOption[] {
  const keys = new Set<string>()
  for (const part of parts) {
    const key = part.categoryKey?.trim()
    if (key) {
      keys.add(key)
    }
  }
  if (!keys.has('general')) {
    keys.add('general')
  }
  return [...keys].sort().map((value) => ({ value, label: value }))
}

export function toSupplierUnitPickerOptions(
  suppliers: SupplierUnitPickerSource[],
): PickerOption[] {
  return suppliers.map((supplier) => ({
    value: supplier.supplierId,
    label: `${humanizeSupplierUnitKind(supplier.unitKind)} · ${formatSupplierIdentitySummary({
      displayName: supplier.displayName,
      supplierKey: supplier.supplierKey,
      parentSupplierDisplayName: supplier.parentSupplierDisplayName,
      supplierUnitKind: supplier.unitKind,
    })}`,
  }))
}

export function toPartPickerOptions(
  parts: { partId: string; displayName: string; partKey: string }[],
): PickerOption[] {
  return parts.map((part) => ({
    value: part.partId,
    label: `${part.displayName} (${part.partKey})`,
  }))
}

export function toCatalogPickerOptions(
  catalogs: { catalogId: string; name: string }[],
): PickerOption[] {
  return catalogs.map((catalog) => ({
    value: catalog.catalogId,
    label: catalog.name,
  }))
}

export function toLocationPickerOptions(
  locations: { locationId: string; name: string; locationKey: string }[],
): PickerOption[] {
  return locations.map((location) => ({
    value: location.locationId,
    label: `${location.name} (${location.locationKey})`,
  }))
}

export function toBinPickerOptions(
  bins: { binId: string; binKey: string; locationKey: string; name: string }[],
): PickerOption[] {
  return bins.map((bin) => ({
    value: bin.binId,
    label: `${bin.locationKey}/${bin.binKey} — ${bin.name}`,
  }))
}
