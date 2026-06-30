import { useMemo, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  ControlledSelect,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'

import { getSupplierDirectoryMetadata } from '../../api/client'
import type { SupplierResponse } from '../../api/types'
import type { SupplyArrWorkspaceState } from '../useSupplyArrWorkspaceState'

type Props = { state: SupplyArrWorkspaceState }
type SuppliersViewMode = 'drawer' | 'details' | 'create'

const fallbackUnitKindOptions: PickerOption[] = [
  { value: 'identity', label: 'Supplier identity' },
  { value: 'sub_unit', label: 'Supplier sub-unit' },
]

const fallbackServiceTypeOptions: PickerOption[] = [
  { value: 'products', label: 'Products' },
  { value: 'parts', label: 'Parts' },
  { value: 'maintenance', label: 'Maintenance' },
  { value: 'repair', label: 'Repair' },
  { value: 'warranty', label: 'Warranty' },
  { value: 'field_service', label: 'Field service' },
  { value: 'logistics', label: 'Logistics' },
]

function humanize(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  return value.replace(/[_-]+/g, ' ').replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatAddress(supplier: SupplierResponse | null): string {
  if (!supplier) return 'Not recorded'
  const parts = [
    supplier.addressLine1,
    supplier.locality,
    supplier.regionCode,
    supplier.postalCode,
    supplier.countryCode,
  ].filter(Boolean)
  return parts.length > 0 ? parts.join(', ') : 'Not recorded'
}

function formatServiceCoverage(serviceTypes: string[] | undefined): string {
  if (!serviceTypes || serviceTypes.length === 0) return 'Not recorded'
  return serviceTypes.map((serviceType) => humanize(serviceType)).join(', ')
}

function describeSupplierUseCase(serviceTypes: string[] | undefined): string {
  const normalized = new Set((serviceTypes ?? []).map((value) => value.toLowerCase()))
  const cues: string[] = []

  if (normalized.has('products') || normalized.has('parts')) {
    cues.push('Stock and parts sourcing')
  }
  if (normalized.has('maintenance') || normalized.has('repair') || normalized.has('field_service')) {
    cues.push('Maintenance and service dispatch')
  }
  if (normalized.has('warranty')) {
    cues.push('Warranty recovery')
  }
  if (normalized.has('logistics')) {
    cues.push('Freight and transfer coordination')
  }

  return cues.length > 0 ? cues.join(' · ') : 'General supplier reference'
}

function formatTimestamp(value: string | null | undefined): string {
  if (!value) return 'Not recorded'
  const date = new Date(value)
  return Number.isNaN(date.getTime())
    ? 'Not recorded'
    : date.toLocaleDateString(undefined, { month: 'short', day: '2-digit', year: 'numeric' })
}

function EmptyPanel({ title, description }: { title: string; description: string }) {
  return (
    <div className="rounded-xl border border-dashed border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-4 py-5">
      <p className="text-sm font-semibold text-[var(--color-text-primary)]">{title}</p>
      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{description}</p>
    </div>
  )
}

function sortSupplierDirectory(suppliers: SupplierResponse[]): SupplierResponse[] {
  const roots = suppliers
    .filter((supplier) => !supplier.parentSupplierId)
    .sort((left, right) => left.displayName.localeCompare(right.displayName))
  const childrenByParent = new Map<string, SupplierResponse[]>()
  for (const supplier of suppliers.filter((item) => item.parentSupplierId)) {
    const siblings = childrenByParent.get(supplier.parentSupplierId!) ?? []
    siblings.push(supplier)
    childrenByParent.set(supplier.parentSupplierId!, siblings)
  }
  for (const siblings of childrenByParent.values()) {
    siblings.sort((left, right) => left.displayName.localeCompare(right.displayName))
  }

  const ordered: SupplierResponse[] = []
  for (const root of roots) {
    ordered.push(root)
    ordered.push(...(childrenByParent.get(root.supplierId!) ?? []))
  }

  const orphanChildren = suppliers
    .filter((supplier) => supplier.parentSupplierId && !suppliers.some((candidate) => candidate.supplierId === supplier.parentSupplierId))
    .sort((left, right) => left.displayName.localeCompare(right.displayName))
  for (const orphan of orphanChildren) {
    if (!ordered.some((candidate) => candidate.supplierId === orphan.supplierId)) {
      ordered.push(orphan)
    }
  }

  return ordered
}

function SupplierDirectoryWorkspace({ state: s, mode }: { state: SupplyArrWorkspaceState; mode: SuppliersViewMode }) {
  const metadataQuery = useQuery({
    queryKey: ['supplyarr-supplier-directory-metadata', s.accessToken],
    queryFn: () => getSupplierDirectoryMetadata(s.accessToken),
    enabled: Boolean(s.accessToken && s.canReadSuppliers),
  })

  const suppliers = useMemo(
    () => sortSupplierDirectory(s.suppliersQuery.data ?? []),
    [s.suppliersQuery.data],
  )
  const [selectedSupplierId, setSelectedSupplierId] = useState('')
  const selectedSupplier = useMemo(
    () => suppliers.find((supplier) => supplier.supplierId === selectedSupplierId)
      ?? suppliers.find((supplier) => !supplier.parentSupplierId)
      ?? suppliers[0]
      ?? null,
    [suppliers, selectedSupplierId],
  )
  const childUnits = useMemo(
    () => suppliers.filter((supplier) => supplier.parentSupplierId === selectedSupplier?.supplierId),
    [suppliers, selectedSupplier?.supplierId],
  )
  const rootSupplierOptions = suppliers
    .filter((supplier) => !supplier.parentSupplierId)
    .map<PickerOption>((supplier) => ({ value: supplier.supplierId, label: supplier.displayName }))
  const unitKindOptions = (metadataQuery.data?.unitKindOptions ?? fallbackUnitKindOptions)
    .map<PickerOption>((option) => ({ value: option.value, label: option.label }))
  const serviceTypeOptions = (metadataQuery.data?.serviceTypeOptions ?? fallbackServiceTypeOptions)
    .map<PickerOption>((option) => ({ value: option.value, label: option.label }))
  const selectedSupplierContracts = (s.contractsQuery.data ?? []).filter(
    (contract) => (contract.supplierId ?? contract.vendorPartyId) === selectedSupplier?.supplierId,
  )
  const linkedItems = (s.partsQuery.data ?? []).flatMap((part) =>
    part.vendorLinks
      .filter((link) => link.supplierId === selectedSupplier?.supplierId)
      .map((link) => ({ part, link })),
  )
  const siblingUnits = useMemo(
    () => selectedSupplier?.parentSupplierId
      ? suppliers.filter((supplier) => supplier.parentSupplierId === selectedSupplier.parentSupplierId && supplier.supplierId !== selectedSupplier.supplierId)
      : [],
    [selectedSupplier?.parentSupplierId, selectedSupplier?.supplierId, suppliers],
  )
  const selectedServiceValues = s.supplierServiceTypes
    .split(',')
    .map((value) => value.trim())
    .filter(Boolean)

  return (
    <div className="space-y-6" data-testid="supplyarr-supplier-directory">
      <header className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-semibold text-[var(--color-text-primary)]">Supplier directory</h1>
          <p className="mt-1 max-w-3xl text-sm text-[var(--color-text-secondary)]">
            Manage supplier identities and their sub-units in one hierarchy. Service coverage and supplier-location context live on each supplier identity or sub-unit so sourcing, maintenance, and purchasing can work from the same supplier record, while internal site truth stays in StaffArr.
          </p>
        </div>
        <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-4 py-3 text-sm text-[var(--color-text-secondary)]">
          {suppliers.filter((supplier) => !supplier.parentSupplierId).length} supplier identities
          {' · '}
          {suppliers.filter((supplier) => supplier.parentSupplierId).length} sub-units
        </div>
      </header>

      <div className="grid gap-6 xl:grid-cols-[minmax(320px,420px)_minmax(0,1fr)]">
        <section className="space-y-4">
          <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <div className="flex items-center justify-between gap-3">
              <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">Directory</h2>
              <span className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                Root first
              </span>
            </div>

            {s.suppliersQuery.isLoading ? (
              <p className="mt-4 text-sm text-[var(--color-text-secondary)]">Loading suppliers…</p>
            ) : null}
            {s.suppliersQuery.isError ? (
              <ApiErrorCallout
                className="mt-4"
                message={getErrorMessage(s.suppliersQuery.error, 'Failed to load supplier directory.')}
                onRetry={() => void s.suppliersQuery.refetch()}
                retryLabel="Retry"
              />
            ) : null}
            {!s.suppliersQuery.isLoading && suppliers.length === 0 ? (
              <div className="mt-4">
                <EmptyPanel
                  title="No suppliers yet"
                  description="Create a supplier identity first, then add sub-units for location-specific sourcing, maintenance, or service coverage."
                />
              </div>
            ) : null}
            <div className="mt-4 space-y-2">
              {suppliers.map((supplier) => {
                const active = selectedSupplier?.supplierId === supplier.supplierId
                const isChild = Boolean(supplier.parentSupplierId)
                return (
                  <button
                    key={supplier.supplierId}
                    type="button"
                    className={`w-full rounded-2xl border px-4 py-3 text-left transition ${
                      active
                        ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)]'
                        : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] hover:border-[var(--color-accent-border)]'
                    }`}
                    onClick={() => setSelectedSupplierId(supplier.supplierId!)}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div className={isChild ? 'pl-5' : ''}>
                        <p className="text-sm font-semibold text-[var(--color-text-primary)]">
                          {isChild ? 'Sub-unit · ' : 'Identity · '}
                          {supplier.displayName}
                        </p>
                        <p className="mt-1 text-xs text-[var(--color-text-secondary)]">
                          {supplier.parentSupplierDisplayName ? `${supplier.parentSupplierDisplayName} · ` : ''}
                          {formatServiceCoverage(supplier.serviceTypes)}
                        </p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatAddress(supplier)}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-xs font-medium text-[var(--color-text-secondary)]">{humanize(supplier.approvalStatus)}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{humanize(supplier.status)}</p>
                      </div>
                    </div>
                  </button>
                )
              })}
            </div>
          </div>

          <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">
              {mode === 'create' ? 'Create supplier identity or sub-unit' : 'Add supplier or sub-unit'}
            </h2>
            <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
              Start with a supplier identity, then create sub-units for regional branches, service locations, maintenance shops, or other site-specific sourcing nodes.
            </p>

            <div className="mt-4 grid gap-3">
              <ControlledSelect
                label="Supplier level"
                value={s.supplierUnitKind}
                onChange={s.setSupplierUnitKind}
                options={unitKindOptions}
                emptyLabel="Select supplier level"
              />
              {s.supplierUnitKind === 'sub_unit' ? (
                <ControlledSelect
                  label="Parent supplier identity"
                  value={s.supplierParentUnitId}
                  onChange={s.setSupplierParentUnitId}
                  options={rootSupplierOptions}
                  emptyLabel="Select supplier identity"
                />
              ) : null}
              <label className="text-sm font-medium text-[var(--color-text-primary)]">
                Supplier key
                <input
                  className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  value={s.supplierKey}
                  onChange={(event) => s.setSupplierKey(event.target.value)}
                  placeholder="midwest-fleet"
                />
              </label>
              <label className="text-sm font-medium text-[var(--color-text-primary)]">
                Display name
                <input
                  className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  value={s.supplierName}
                  onChange={(event) => s.setSupplierName(event.target.value)}
                  placeholder={s.supplierUnitKind === 'sub_unit' ? 'Midwest Fleet - Kansas City' : 'Midwest Fleet'}
                />
              </label>
              <label className="text-sm font-medium text-[var(--color-text-primary)]">
                Legal name
                <input
                  className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  value={s.supplierLegalName}
                  onChange={(event) => s.setSupplierLegalName(event.target.value)}
                  placeholder="Midwest Fleet LLC"
                />
              </label>
              <label className="text-sm font-medium text-[var(--color-text-primary)]">
                Service coverage
                <select
                  multiple
                  className="mt-1 min-h-32 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  value={selectedServiceValues}
                  onChange={(event) => {
                    const values = Array.from(event.target.selectedOptions).map((option) => option.value)
                    s.setSupplierServiceTypes(values.join(','))
                  }}
                >
                  {serviceTypeOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
              <div className="grid gap-3 md:grid-cols-2">
                    <label className="text-sm font-medium text-[var(--color-text-primary)]">
                  Supplier address
                  <input
                    className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                    value={s.supplierAddressLine1}
                    onChange={(event) => s.setSupplierAddressLine1(event.target.value)}
                    placeholder="1200 Westport Rd"
                  />
                </label>
                <label className="text-sm font-medium text-[var(--color-text-primary)]">
                  City
                  <input
                    className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                    value={s.supplierLocality}
                    onChange={(event) => s.setSupplierLocality(event.target.value)}
                    placeholder="Kansas City"
                  />
                </label>
              </div>
              <div className="grid gap-3 md:grid-cols-3">
                <label className="text-sm font-medium text-[var(--color-text-primary)]">
                  Region
                  <input
                    className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                    value={s.supplierRegionCode}
                    onChange={(event) => s.setSupplierRegionCode(event.target.value)}
                    placeholder="MO"
                  />
                </label>
                <label className="text-sm font-medium text-[var(--color-text-primary)]">
                  Postal code
                  <input
                    className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                    value={s.supplierPostalCode}
                    onChange={(event) => s.setSupplierPostalCode(event.target.value)}
                    placeholder="64111"
                  />
                </label>
                <label className="text-sm font-medium text-[var(--color-text-primary)]">
                  Country
                  <input
                    className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                    value={s.supplierCountryCode}
                    onChange={(event) => s.setSupplierCountryCode(event.target.value)}
                    placeholder="US"
                  />
                </label>
              </div>
              <label className="text-sm font-medium text-[var(--color-text-primary)]">
                Tax identifier
                <input
                  className="mt-1 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  value={s.supplierTaxId}
                  onChange={(event) => s.setSupplierTaxId(event.target.value)}
                  placeholder="12-3456789"
                />
              </label>
              <label className="text-sm font-medium text-[var(--color-text-primary)]">
                Notes
                <textarea
                  className="mt-1 min-h-24 w-full rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)]"
                  value={s.supplierNotes}
                  onChange={(event) => s.setSupplierNotes(event.target.value)}
                  placeholder="Use notes for sourcing context or unit-specific operating details."
                />
              </label>
              <button
                type="button"
                className="rounded-xl bg-[var(--color-accent)] px-4 py-3 text-sm font-semibold text-[var(--color-on-accent)] hover:bg-[var(--color-accent-hover)] disabled:opacity-60"
                disabled={
                  s.createSupplierMutation.isPending
                  || !s.supplierKey.trim()
                  || !s.supplierName.trim()
                  || (s.supplierUnitKind === 'sub_unit' && !s.supplierParentUnitId)
                }
                onClick={() => s.createSupplierMutation.mutate()}
              >
                {s.createSupplierMutation.isPending ? 'Saving supplier…' : s.supplierUnitKind === 'sub_unit' ? 'Create sub-unit' : 'Create supplier identity'}
              </button>
              {s.createSupplierMutation.isError ? (
                <ApiErrorCallout
                  message={getErrorMessage(s.createSupplierMutation.error, 'Failed to create supplier record.')}
                />
              ) : null}
            </div>
          </div>
        </section>

        <section className="space-y-4" data-testid="supplyarr-supplier-profile">
          <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">Supplier snapshot</p>
                <h2 className="mt-2 text-2xl font-semibold text-[var(--color-text-primary)]">
                  {selectedSupplier?.displayName ?? 'No supplier selected'}
                </h2>
                <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                  {selectedSupplier
                    ? `${humanize(selectedSupplier.unitKind)} · ${formatServiceCoverage(selectedSupplier.serviceTypes)}`
                    : 'Choose a supplier identity or sub-unit from the directory.'}
                </p>
              </div>
              {selectedSupplier ? (
                <div className="flex gap-2">
                  <button
                    type="button"
                    className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)]"
                    onClick={() =>
                      s.updateSupplierApprovalMutation.mutate({
                        supplierId: selectedSupplier.supplierId,
                        approvalStatus: selectedSupplier.approvalStatus === 'approved' ? 'pending' : 'approved',
                      })
                    }
                  >
                    {selectedSupplier.approvalStatus === 'approved' ? 'Mark review' : 'Approve'}
                  </button>
                  <button
                    type="button"
                    className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] px-3 py-2 text-sm text-[var(--color-text-primary)] hover:border-[var(--color-accent-border)]"
                    onClick={() =>
                      s.updateSupplierStatusMutation.mutate({
                        supplierId: selectedSupplier.supplierId,
                        status: selectedSupplier.status === 'active' ? 'inactive' : 'active',
                      })
                    }
                  >
                    {selectedSupplier.status === 'active' ? 'Deactivate' : 'Activate'}
                  </button>
                </div>
              ) : null}
            </div>

            {!selectedSupplier ? (
              <div className="mt-4">
                <EmptyPanel
                  title="No supplier selected"
                  description="Select a supplier identity or sub-unit from the directory to inspect hierarchy, sourcing fit, and site context."
                />
              </div>
            ) : (
              <div className="mt-5 grid gap-4 md:grid-cols-2 xl:grid-cols-3">
                <DetailCard label="Supplier key" value={selectedSupplier.supplierKey} />
                <DetailCard label="Legal name" value={selectedSupplier.legalName || selectedSupplier.displayName} />
                <DetailCard label="Parent supplier" value={selectedSupplier.parentSupplierDisplayName || 'Root supplier identity'} />
                <DetailCard label="Service coverage" value={formatServiceCoverage(selectedSupplier.serviceTypes)} />
                <DetailCard label="Supplier location" value={formatAddress(selectedSupplier)} />
                <DetailCard label="Sub-units" value={String(selectedSupplier.childUnitCount ?? childUnits.length)} />
                <DetailCard label="Approval state" value={humanize(selectedSupplier.approvalStatus)} />
                <DetailCard label="Lifecycle status" value={humanize(selectedSupplier.status)} />
                <DetailCard label="Updated" value={formatTimestamp(selectedSupplier.updatedAt)} />
              </div>
            )}
          </div>

          {selectedSupplier ? (
            <div className="grid gap-4 xl:grid-cols-2">
                <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
                  <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Sub-units</h3>
                  <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                    Location-specific supplier nodes for sourcing, service, or maintenance coverage.
                </p>
                <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                  Internal tenant sites are selected later from StaffArr when a workflow needs a company-owned origin, destination, or receiving location.
                </p>
                <div className="mt-4 space-y-3">
                  {childUnits.length > 0 ? childUnits.map((supplier) => (
                    <div key={supplier.supplierId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="font-semibold text-[var(--color-text-primary)]">{supplier.displayName}</p>
                          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{formatServiceCoverage(supplier.serviceTypes)}</p>
                          <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatAddress(supplier)}</p>
                        </div>
                        <button
                          type="button"
                          className="text-sm font-medium text-[var(--color-accent)]"
                          onClick={() => setSelectedSupplierId(supplier.supplierId!)}
                        >
                          View
                        </button>
                      </div>
                    </div>
                  )) : (
                    <EmptyPanel
                      title="No sub-units yet"
                      description="Add regional branches or site-level supplier nodes so sourcing can prefer the closest capable location."
                    />
                  )}
                </div>
              </section>

              <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
                <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Sourcing readiness</h3>
                <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                  Use supplier identities as umbrella records and sub-units as the location-aware sourcing nodes for parts, maintenance, and service routing.
                </p>
                <div className="mt-4 grid gap-3 md:grid-cols-2">
                  <DetailCard label="Best fit" value={describeSupplierUseCase(selectedSupplier.serviceTypes)} />
                  <DetailCard
                    label="Location strategy"
                    value={
                      selectedSupplier.parentSupplierId
                        ? 'Use this sub-unit when the closest capable supplier location matters.'
                        : 'Use this identity when sourcing can route across multiple supplier locations.'
                    }
                  />
                  <DetailCard label="Linked items in scope" value={String(linkedItems.length)} />
                  <DetailCard label="Contracts in scope" value={String(selectedSupplierContracts.length)} />
                  <DetailCard
                    label="Nearby supplier nodes"
                    value={
                      selectedSupplier.parentSupplierId
                        ? `${siblingUnits.length} sibling sub-units under ${selectedSupplier.parentSupplierDisplayName ?? 'the parent supplier'}`
                        : `${childUnits.length} direct sub-units available for location-specific routing`
                    }
                  />
                  <DetailCard
                    label="Internal-site ownership"
                    value="StaffArr owns tenant sites, docks, and yards used as internal origins, destinations, and receiving points."
                  />
                </div>
              </section>

              <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
                <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Primary contacts</h3>
                <div className="mt-4 space-y-3">
                  {selectedSupplier.contacts.length > 0 ? selectedSupplier.contacts.map((contact) => (
                    <div key={contact.contactId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                      <p className="font-semibold text-[var(--color-text-primary)]">
                        {contact.contactName}
                        {contact.isPrimary ? ' · Primary' : ''}
                      </p>
                      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{contact.roleLabel || 'Contact'}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {[contact.email, contact.phone].filter(Boolean).join(' · ') || 'No direct contact details'}
                      </p>
                    </div>
                  )) : (
                    <EmptyPanel title="No contacts recorded" description="Add contacts to each supplier identity or sub-unit as procurement ownership becomes clearer." />
                  )}
                </div>
              </section>

              <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
                <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Linked items</h3>
                <div className="mt-4 space-y-3">
                  {linkedItems.length > 0 ? linkedItems.slice(0, 5).map(({ part, link }) => (
                    <div key={link.linkId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                      <p className="font-semibold text-[var(--color-text-primary)]">{part.displayName}</p>
                      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
                        {link.vendorPartNumber || 'Supplier part not recorded'} · {link.catalogLeadTimeDays ?? 'Untracked'} day lead time
                      </p>
                    </div>
                  )) : (
                    <EmptyPanel title="No linked items" description="Attach parts and catalog lines to a specific supplier identity or sub-unit for location-aware sourcing." />
                  )}
                </div>
              </section>

              <section className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-5">
                <h3 className="text-lg font-semibold text-[var(--color-text-primary)]">Contracts & terms</h3>
                <div className="mt-4 space-y-3">
                  {selectedSupplierContracts.length > 0 ? selectedSupplierContracts.slice(0, 4).map((contract) => (
                    <div key={contract.contractId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                      <p className="font-semibold text-[var(--color-text-primary)]">{contract.contractKey}</p>
                      <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{contract.title}</p>
                      <p className="mt-1 text-xs text-[var(--color-text-muted)]">
                        {contract.paymentTerms || 'Terms not recorded'} · {contract.freightTerms || 'Freight terms not recorded'}
                      </p>
                    </div>
                  )) : (
                    <EmptyPanel title="No contracts on file" description="Contracts will appear here once they are linked to this supplier identity or sub-unit." />
                  )}
                </div>
              </section>
            </div>
          ) : null}
        </section>
      </div>
    </div>
  )
}

function DetailCard({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
      <p className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</p>
      <p className="mt-2 text-sm font-semibold text-[var(--color-text-primary)]">{value}</p>
    </div>
  )
}

export function SuppliersSection({ state: s }: Props) {
  const location = useLocation()
  const mode: SuppliersViewMode = location.pathname.startsWith('/suppliers/create')
    ? 'create'
    : location.pathname.startsWith('/suppliers/details')
      ? 'details'
      : 'drawer'

  return <SupplierDirectoryWorkspace state={s} mode={mode} />
}
