import { useMemo, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import {
  ApiErrorCallout,
  ControlledSelect,
  getErrorMessage,
  type PickerOption,
} from '@stl/shared-ui'

import { getPartyRegistryMetadata } from '../../api/client'
import type { ExternalPartyResponse } from '../../api/types'
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

function formatAddress(party: ExternalPartyResponse | null): string {
  if (!party) return 'Not recorded'
  const parts = [
    party.addressLine1,
    party.locality,
    party.regionCode,
    party.postalCode,
    party.countryCode,
  ].filter(Boolean)
  return parts.length > 0 ? parts.join(', ') : 'Not recorded'
}

function formatServiceCoverage(serviceTypes: string[] | undefined): string {
  if (!serviceTypes || serviceTypes.length === 0) return 'Not recorded'
  return serviceTypes.map((serviceType) => humanize(serviceType)).join(', ')
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

function sortSupplierDirectory(parties: ExternalPartyResponse[]): ExternalPartyResponse[] {
  const roots = parties
    .filter((party) => !party.parentPartyId)
    .sort((left, right) => left.displayName.localeCompare(right.displayName))
  const childrenByParent = new Map<string, ExternalPartyResponse[]>()
  for (const party of parties.filter((item) => item.parentPartyId)) {
    const siblings = childrenByParent.get(party.parentPartyId!) ?? []
    siblings.push(party)
    childrenByParent.set(party.parentPartyId!, siblings)
  }
  for (const siblings of childrenByParent.values()) {
    siblings.sort((left, right) => left.displayName.localeCompare(right.displayName))
  }

  const ordered: ExternalPartyResponse[] = []
  for (const root of roots) {
    ordered.push(root)
    ordered.push(...(childrenByParent.get(root.partyId) ?? []))
  }

  const orphanChildren = parties
    .filter((party) => party.parentPartyId && !parties.some((candidate) => candidate.partyId === party.parentPartyId))
    .sort((left, right) => left.displayName.localeCompare(right.displayName))
  for (const orphan of orphanChildren) {
    if (!ordered.some((candidate) => candidate.partyId === orphan.partyId)) {
      ordered.push(orphan)
    }
  }

  return ordered
}

function SupplierDirectoryWorkspace({ state: s, mode }: { state: SupplyArrWorkspaceState; mode: SuppliersViewMode }) {
  const metadataQuery = useQuery({
    queryKey: ['supplyarr-supplier-directory-metadata', s.accessToken],
    queryFn: () => getPartyRegistryMetadata(s.accessToken),
    enabled: Boolean(s.accessToken && s.canReadParties),
  })

  const parties = useMemo(
    () => sortSupplierDirectory(s.suppliersQuery.data ?? []),
    [s.suppliersQuery.data],
  )
  const [selectedSupplierId, setSelectedSupplierId] = useState('')
  const selectedSupplier = useMemo(
    () => parties.find((party) => party.partyId === selectedSupplierId)
      ?? parties.find((party) => !party.parentPartyId)
      ?? parties[0]
      ?? null,
    [parties, selectedSupplierId],
  )
  const childUnits = useMemo(
    () => parties.filter((party) => party.parentPartyId === selectedSupplier?.partyId),
    [parties, selectedSupplier?.partyId],
  )
  const rootSupplierOptions = parties
    .filter((party) => !party.parentPartyId)
    .map<PickerOption>((party) => ({ value: party.partyId, label: party.displayName }))
  const unitKindOptions = (metadataQuery.data?.unitKindOptions ?? fallbackUnitKindOptions)
    .map<PickerOption>((option) => ({ value: option.value, label: option.label }))
  const serviceTypeOptions = (metadataQuery.data?.serviceTypeOptions ?? fallbackServiceTypeOptions)
    .map<PickerOption>((option) => ({ value: option.value, label: option.label }))
  const selectedPartyContracts = (s.contractsQuery.data ?? []).filter(
    (contract) => contract.vendorPartyId === selectedSupplier?.partyId,
  )
  const linkedItems = (s.partsQuery.data ?? []).flatMap((part) =>
    part.vendorLinks
      .filter((link) => link.partyId === selectedSupplier?.partyId)
      .map((link) => ({ part, link })),
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
            Manage supplier identities and their sub-units in one hierarchy. Service coverage and site context live on each supplier unit so sourcing, maintenance, and purchasing can work from the same supplier record.
          </p>
        </div>
        <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] px-4 py-3 text-sm text-[var(--color-text-secondary)]">
          {parties.filter((party) => !party.parentPartyId).length} supplier identities
          {' · '}
          {parties.filter((party) => party.parentPartyId).length} sub-units
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
            {!s.suppliersQuery.isLoading && parties.length === 0 ? (
              <div className="mt-4">
                <EmptyPanel
                  title="No suppliers yet"
                  description="Create a supplier identity first, then add sub-units for location-specific sourcing, maintenance, or service coverage."
                />
              </div>
            ) : null}
            <div className="mt-4 space-y-2">
              {parties.map((party) => {
                const active = selectedSupplier?.partyId === party.partyId
                const isChild = Boolean(party.parentPartyId)
                return (
                  <button
                    key={party.partyId}
                    type="button"
                    className={`w-full rounded-2xl border px-4 py-3 text-left transition ${
                      active
                        ? 'border-[var(--color-accent-border)] bg-[var(--color-accent-soft)]'
                        : 'border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] hover:border-[var(--color-accent-border)]'
                    }`}
                    onClick={() => setSelectedSupplierId(party.partyId)}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div className={isChild ? 'pl-5' : ''}>
                        <p className="text-sm font-semibold text-[var(--color-text-primary)]">
                          {isChild ? 'Sub-unit · ' : 'Identity · '}
                          {party.displayName}
                        </p>
                        <p className="mt-1 text-xs text-[var(--color-text-secondary)]">
                          {party.parentPartyDisplayName ? `${party.parentPartyDisplayName} · ` : ''}
                          {formatServiceCoverage(party.serviceTypes)}
                        </p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatAddress(party)}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-xs font-medium text-[var(--color-text-secondary)]">{humanize(party.approvalStatus)}</p>
                        <p className="mt-1 text-xs text-[var(--color-text-muted)]">{humanize(party.status)}</p>
                      </div>
                    </div>
                  </button>
                )
              })}
            </div>
          </div>

          <div className="rounded-2xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-surface)] p-4">
            <h2 className="text-lg font-semibold text-[var(--color-text-primary)]">
              {mode === 'create' ? 'Create supplier unit' : 'Add supplier or sub-unit'}
            </h2>
            <p className="mt-1 text-sm text-[var(--color-text-secondary)]">
              Start with a supplier identity, then create sub-units for regional branches, dealer locations, maintenance shops, or other site-specific sourcing nodes.
            </p>

            <div className="mt-4 grid gap-3">
              <ControlledSelect
                label="Record kind"
                value={s.supplierUnitKind}
                onChange={s.setSupplierUnitKind}
                options={unitKindOptions}
                emptyLabel="Select record kind"
              />
              {s.supplierUnitKind === 'sub_unit' ? (
                <ControlledSelect
                  label="Parent supplier identity"
                  value={s.supplierParentPartyId}
                  onChange={s.setSupplierParentPartyId}
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
                  Address line
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
                  || (s.supplierUnitKind === 'sub_unit' && !s.supplierParentPartyId)
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
                      s.updatePartyApprovalMutation.mutate({
                        route: 'suppliers',
                        partyId: selectedSupplier.partyId,
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
                      s.updatePartyStatusMutation.mutate({
                        route: 'suppliers',
                        partyId: selectedSupplier.partyId,
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
                <DetailCard label="Supplier key" value={selectedSupplier.partyKey} />
                <DetailCard label="Legal name" value={selectedSupplier.legalName || selectedSupplier.displayName} />
                <DetailCard label="Parent supplier" value={selectedSupplier.parentPartyDisplayName || 'Root supplier identity'} />
                <DetailCard label="Service coverage" value={formatServiceCoverage(selectedSupplier.serviceTypes)} />
                <DetailCard label="Address" value={formatAddress(selectedSupplier)} />
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
                <div className="mt-4 space-y-3">
                  {childUnits.length > 0 ? childUnits.map((party) => (
                    <div key={party.partyId} className="rounded-xl border border-[var(--color-border-subtle)] bg-[var(--color-bg-control)] p-4">
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="font-semibold text-[var(--color-text-primary)]">{party.displayName}</p>
                          <p className="mt-1 text-sm text-[var(--color-text-secondary)]">{formatServiceCoverage(party.serviceTypes)}</p>
                          <p className="mt-1 text-xs text-[var(--color-text-muted)]">{formatAddress(party)}</p>
                        </div>
                        <button
                          type="button"
                          className="text-sm font-medium text-[var(--color-accent)]"
                          onClick={() => setSelectedSupplierId(party.partyId)}
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
                  {selectedPartyContracts.length > 0 ? selectedPartyContracts.slice(0, 4).map((contract) => (
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

export function PartiesSection({ state: s }: Props) {
  const location = useLocation()
  const mode: SuppliersViewMode = location.pathname.startsWith('/suppliers/create')
    ? 'create'
    : location.pathname.startsWith('/suppliers/details')
      ? 'details'
      : 'drawer'

  return <SupplierDirectoryWorkspace state={s} mode={mode} />
}
