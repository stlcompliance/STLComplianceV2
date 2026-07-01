import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useMemo, useState } from 'react'

import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import {
  createSupplierRestriction,
  getSupplierRestrictionEnforcement,
  liftSupplierRestriction,
  listRestrictionsForSupplier,
  listSupplierRestrictions,
} from '../api/client'
import type { SupplierResponse } from '../api/types'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'
import {
  formatSupplierIdentitySummary,
  formatSupplierOperationalContext,
  humanizeSupplierUnitKind,
  resolveSupplierId,
} from '../utils/supplierPresentation'

const SCOPE_OPTIONS = [
  { value: 'purchase_requests', label: 'Purchase requests' },
  { value: 'purchase_orders', label: 'Purchase orders' },
  { value: 'rfq_invitations', label: 'RFQ invitations' },
  { value: 'receiving', label: 'Receiving' },
  { value: 'all_procurement', label: 'All procurement' },
] as const

interface SupplierRestrictionsPanelProps {
  accessToken: string
  canManage: boolean
  restrictableSuppliers: SupplierResponse[]
}

export function SupplierRestrictionsPanel({
  accessToken,
  canManage,
  restrictableSuppliers,
}: SupplierRestrictionsPanelProps) {
  const queryClient = useQueryClient()
  const [selectedSupplierId, setSelectedSupplierId] = useState('')
  const [restrictionKey, setRestrictionKey] = useState('')
  const [reason, setReason] = useState('')
  const [selectedScopes, setSelectedScopes] = useState<string[]>(['all_procurement'])

  const activeQuery = useQuery({
    queryKey: ['supplyarr-supplier-restrictions', accessToken, 'active'],
    queryFn: () => listSupplierRestrictions(accessToken, 'active'),
    enabled: canManage,
  })

  const supplierRestrictionsQuery = useQuery({
    queryKey: ['supplyarr-supplier-restrictions', accessToken, 'by-supplier', selectedSupplierId],
    queryFn: () => listRestrictionsForSupplier(accessToken, selectedSupplierId),
    enabled: Boolean(selectedSupplierId),
  })

  const enforcementQuery = useQuery({
    queryKey: ['supplyarr-supplier-restriction-enforcement', accessToken, selectedSupplierId],
    queryFn: () => getSupplierRestrictionEnforcement(accessToken, selectedSupplierId),
    enabled: Boolean(selectedSupplierId),
  })

  const selectedSupplier = useMemo(
    () => restrictableSuppliers.find((supplier) => resolveSupplierId(supplier) === selectedSupplierId),
    [restrictableSuppliers, selectedSupplierId],
  )
  const supplierOptions = useMemo<PickerOption[]>(
    () =>
      restrictableSuppliers.map((supplier) => ({
        value: supplier.supplierId,
        label: `${formatSupplierIdentitySummary({
          supplierDisplayName: supplier.displayName,
          supplierKey: supplier.supplierKey,
          parentSupplierDisplayName: supplier.parentSupplierDisplayName,
          supplierUnitKind: supplier.unitKind,
        })} · ${humanizeSupplierUnitKind(supplier.unitKind)}`,
      })),
    [restrictableSuppliers],
  )
  const selectedSupplierOption = useMemo<PickerOption | undefined>(
    () => supplierOptions.find((option) => option.value === selectedSupplierId),
    [supplierOptions, selectedSupplierId],
  )

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: ['supplyarr-supplier-restrictions', accessToken] })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-supplier-restrictions', accessToken, 'by-supplier', selectedSupplierId],
    })
    void queryClient.invalidateQueries({
      queryKey: ['supplyarr-supplier-restriction-enforcement', accessToken, selectedSupplierId],
    })
  }

  const createMutation = useMutation({
    mutationFn: () =>
      createSupplierRestriction(accessToken, selectedSupplierId, {
        restrictionKey,
        scopes: selectedScopes,
        reason,
      }),
    onSuccess: () => {
      setRestrictionKey('')
      setReason('')
      invalidate()
    },
  })

  const liftMutation = useMutation({
    mutationFn: (restrictionId: string) =>
      liftSupplierRestriction(accessToken, restrictionId, { liftNotes: 'Lifted from supplier directory' }),
    onSuccess: invalidate,
  })

  if (!canManage) {
    return null
  }

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="supplier-restrictions-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Supplier restrictions</h2>
      <p className="mt-1 text-sm text-slate-400">
        Block procurement activity by scope for supplier identities or sub-units. Enforcement applies to purchase
        requests, purchase orders, RFQ invitations, and receiving.
      </p>

      {activeQuery.data && (
        <p className="mt-3 text-sm text-[var(--color-text-muted)]">
          {activeQuery.data.length} active restriction{activeQuery.data.length === 1 ? '' : 's'} tenant-wide
        </p>
      )}

      <div className="mt-4 grid gap-4 md:grid-cols-2">
        <StaticSearchPicker
          id="supplier-restriction-supplier"
          label="Supplier identity or sub-unit"
          value={selectedSupplierId}
          onChange={setSelectedSupplierId}
          options={supplierOptions}
          selectedOption={selectedSupplierOption}
          placeholder="Search supplier identities or sub-units…"
          testId="supplier-restriction-supplier-picker"
        />
        {selectedSupplier ? (
          <p className="md:col-span-2 text-xs text-[var(--color-text-muted)]">
            {formatSupplierIdentitySummary({
              supplierDisplayName: selectedSupplier.displayName,
              supplierKey: selectedSupplier.supplierKey,
              parentSupplierDisplayName: selectedSupplier.parentSupplierDisplayName,
              supplierUnitKind: selectedSupplier.unitKind,
            })}{' '}
            · {humanizeSupplierUnitKind(selectedSupplier.unitKind)} ·{' '}
            {formatSupplierOperationalContext({
              supplierServiceTypes: selectedSupplier.serviceTypes,
              addressLine1: selectedSupplier.addressLine1,
              locality: selectedSupplier.locality,
              regionCode: selectedSupplier.regionCode,
              postalCode: selectedSupplier.postalCode,
            })}
          </p>
        ) : null}

        {selectedSupplier && enforcementQuery.data && (
          <div className="md:col-span-2 rounded-md border border-slate-800 p-3 text-sm">
            <span
              className={
                enforcementQuery.data.isBlocked
                  ? 'text-rose-300'
                  : 'text-emerald-300'
              }
            >
              {enforcementQuery.data.isBlocked ? 'Blocked' : 'Clear'} for procurement
            </span>
            {enforcementQuery.data.activeScopes.length > 0 && (
              <p className="mt-1 text-[var(--color-text-muted)]">
                Active scopes: {enforcementQuery.data.activeScopes.join(', ')}
              </p>
            )}
            {enforcementQuery.data.blockReason && (
              <p className="mt-1 text-slate-400">{enforcementQuery.data.blockReason}</p>
            )}
          </div>
        )}

        {selectedSupplierId && (
          <>
            <GeneratedKeyFieldGroup
              sourceLabel={`${selectedSupplier?.displayName ?? ''} ${selectedScopes.join(' ')} restriction`}
              existingKeys={supplierRestrictionsQuery.data?.map((restriction) => restriction.restrictionKey) ?? []}
              onKeyChange={setRestrictionKey}
              domain="supplier"
              kind="restriction"
              maxLength={128}
              label="Restriction key"
              disabled={createMutation.isPending}
            />
            <label htmlFor="supplier-restriction-reason" className="block text-sm text-slate-400 md:col-span-2">
              Restriction reason
              <textarea
                id="supplier-restriction-reason"
                className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
                rows={2}
                value={reason}
                onChange={(event) => setReason(event.target.value)}
              />
            </label>
            <fieldset className="md:col-span-2">
              <legend className="text-sm font-medium text-slate-300">Scopes</legend>
              <div className="mt-2 flex flex-wrap gap-3">
                {SCOPE_OPTIONS.map((scope) => (
                  <label key={scope.value} htmlFor={`supplier-restriction-scope-${scope.value}`} className="flex items-center gap-2 text-sm text-slate-400">
                    <input
                      id={`supplier-restriction-scope-${scope.value}`}
                      type="checkbox"
                      checked={selectedScopes.includes(scope.value)}
                      onChange={(event) => {
                        if (event.target.checked) {
                          setSelectedScopes((prev) => [...prev, scope.value])
                        } else {
                          setSelectedScopes((prev) => prev.filter((x) => x !== scope.value))
                        }
                      }}
                    />
                    {scope.label}
                  </label>
                ))}
              </div>
            </fieldset>
            <button
              type="button"
              className="rounded bg-rose-700 px-3 py-1.5 text-sm text-white disabled:opacity-50 md:col-span-2 md:w-fit"
              disabled={
                createMutation.isPending
                || !restrictionKey.trim()
                || !reason.trim()
                || selectedScopes.length === 0
              }
              onClick={() => createMutation.mutate()}
            >
              Apply restriction
            </button>
          </>
        )}
      </div>

      {supplierRestrictionsQuery.data && supplierRestrictionsQuery.data.length > 0 && (
        <ul className="mt-4 divide-y divide-slate-800 rounded-md border border-slate-800 text-sm">
          {supplierRestrictionsQuery.data.map((item) => (
            <li key={item.restrictionId} className="flex flex-wrap items-center justify-between gap-2 px-3 py-3">
              <div>
                <div className="font-medium text-slate-100">
                  {item.restrictionKey} · {item.status}
                </div>
                <div className="text-xs text-[var(--color-text-muted)]">{item.scopes.join(', ')}</div>
                <p className="mt-1 text-slate-400">{item.reason}</p>
              </div>
              {item.status === 'active' && (
                <button
                  type="button"
                  className="rounded border border-slate-600 px-2 py-1 text-xs text-slate-200 disabled:opacity-50"
                  disabled={liftMutation.isPending}
                  onClick={() => liftMutation.mutate(item.restrictionId)}
                >
                  Lift
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
