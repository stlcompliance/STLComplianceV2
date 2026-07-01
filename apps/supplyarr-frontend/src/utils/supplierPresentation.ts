export interface SupplierPresentationLike {
  supplierId?: string | null
  supplierDisplayName?: string | null
  displayName?: string | null
  supplierKey?: string | null
  parentSupplierDisplayName?: string | null
  supplierUnitKind?: string | null
  supplierServiceTypes?: string[] | null
  supplierAddressLine1?: string | null
  supplierLocality?: string | null
  supplierRegionCode?: string | null
  supplierPostalCode?: string | null
  addressLine1?: string | null
  locality?: string | null
  regionCode?: string | null
  postalCode?: string | null
}

export function resolveSupplierDisplayName(value: SupplierPresentationLike): string {
  return (
    value.supplierDisplayName
    ?? value.displayName
    ?? 'Unknown supplier'
  )
}

export function resolveSupplierId(value: SupplierPresentationLike): string | null {
  return value.supplierId ?? null
}

export function resolveSupplierKey(value: SupplierPresentationLike): string | null {
  return value.supplierKey ?? null
}

export function formatSupplierSummary(value: SupplierPresentationLike): string {
  const displayName = resolveSupplierDisplayName(value)
  const supplierKey = resolveSupplierKey(value)
  return supplierKey ? `${displayName} (${supplierKey})` : displayName
}

export function formatSupplierIdentitySummary(value: SupplierPresentationLike): string {
  const displayName = formatSupplierIdentityLabel(value)
  const supplierKey = resolveSupplierKey(value)
  return supplierKey ? `${displayName} (${supplierKey})` : displayName
}

export function humanizeSupplierUnitKind(value: string | null | undefined): string {
  if (!value) {
    return 'Supplier record'
  }

  if (value === 'identity') {
    return 'Parent supplier'
  }

  if (value === 'sub_unit') {
    return 'Sub-unit'
  }

  return value
    .split('_')
    .filter(Boolean)
    .map((segment) => segment.charAt(0).toUpperCase() + segment.slice(1))
    .join(' ')
}

export function formatSupplierServiceTypes(values: string[] | null | undefined): string {
  if (!values || values.length === 0) {
    return 'Services not listed'
  }

  return values
    .map((value) =>
      value
        .split('_')
        .filter(Boolean)
        .map((segment) => segment.charAt(0).toUpperCase() + segment.slice(1))
        .join(' '),
    )
    .join(', ')
}

export function formatSupplierIdentityLabel(value: SupplierPresentationLike): string {
  const displayName = resolveSupplierDisplayName(value)
  if (value.supplierUnitKind === 'sub_unit' && value.parentSupplierDisplayName) {
    return `${value.parentSupplierDisplayName} · ${displayName}`
  }
  return displayName
}

export function formatSupplierLocation(value: SupplierPresentationLike): string | null {
  const segments = [
    value.supplierAddressLine1 ?? value.addressLine1,
    value.supplierLocality ?? value.locality,
    value.supplierRegionCode ?? value.regionCode,
    value.supplierPostalCode ?? value.postalCode,
  ]
    .map((segment) => segment?.trim())
    .filter(Boolean)

  return segments.length > 0 ? segments.join(', ') : null
}

export function formatSupplierOperationalContext(value: SupplierPresentationLike): string {
  const location = formatSupplierLocation(value)
  const services =
    value.supplierServiceTypes && value.supplierServiceTypes.length > 0
      ? formatSupplierServiceTypes(value.supplierServiceTypes)
      : null

  if (location && services) {
    return `${location} · ${services}`
  }

  return location ?? services ?? 'No supplier location or service context available'
}
