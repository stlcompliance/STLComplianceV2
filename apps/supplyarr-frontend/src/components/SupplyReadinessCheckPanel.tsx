import { useMemo, useState } from 'react'

import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useQuery } from '@tanstack/react-query'

import {
  getPartSupplyReadiness,
  getProcurementPathReadiness,
  getSupplierReadiness,
} from '../api/client'
import type { SupplierResponse, PartResponse } from '../api/types'
import {
  formatSupplierIdentitySummary,
  formatSupplierOperationalContext,
  humanizeSupplierUnitKind,
} from '../utils/supplierPresentation'

interface SupplyReadinessCheckPanelProps {
  accessToken: string
  canRead: boolean
  parts: PartResponse[]
  suppliers: SupplierResponse[]
}

function readinessBadgeClass(status: string): string {
  return status === 'ready'
    ? 'bg-emerald-500/20 text-emerald-200'
    : 'bg-rose-500/20 text-rose-200'
}

function formatSupplierUnitLabel(supplier: SupplierResponse): string {
  return [
    humanizeSupplierUnitKind(supplier.unitKind),
    formatSupplierIdentitySummary({
      displayName: supplier.displayName,
      supplierKey: supplier.supplierKey,
      parentSupplierDisplayName: supplier.parentSupplierDisplayName,
      supplierUnitKind: supplier.unitKind,
    }),
    formatSupplierOperationalContext({
      supplierServiceTypes: supplier.serviceTypes,
      addressLine1: supplier.addressLine1,
      locality: supplier.locality,
      regionCode: supplier.regionCode,
      postalCode: supplier.postalCode,
    }),
  ]
    .filter(Boolean)
    .join(' · ')
}

export function SupplyReadinessCheckPanel({
  accessToken,
  canRead,
  parts,
  suppliers,
}: SupplyReadinessCheckPanelProps) {
  const [checkMode, setCheckMode] = useState<'part' | 'supplier' | 'path'>('part')
  const [selectedPartId, setSelectedPartId] = useState('')
  const [selectedSupplierUnitId, setSelectedSupplierUnitId] = useState('')
  const [requestedQuantity, setRequestedQuantity] = useState('')

  const quantity =
    requestedQuantity.trim() === '' ? undefined : Number.parseFloat(requestedQuantity)
  const partOptions = useMemo<PickerOption[]>(
    () =>
      parts.map((part) => ({
        value: part.partId,
        label: `${part.partKey} · ${part.displayName}`,
      })),
    [parts],
  )
  const selectedPartOption = useMemo<PickerOption | undefined>(
    () => partOptions.find((option) => option.value === selectedPartId),
    [partOptions, selectedPartId],
  )
  const supplierUnitOptions = useMemo<PickerOption[]>(
    () =>
      suppliers.map((supplier) => ({
        value: supplier.supplierId,
        label: formatSupplierUnitLabel(supplier),
      })),
    [suppliers],
  )
  const selectedSupplierUnitOption = useMemo<PickerOption | undefined>(
    () => supplierUnitOptions.find((option) => option.value === selectedSupplierUnitId),
    [selectedSupplierUnitId, supplierUnitOptions],
  )

  const partQuery = useQuery({
    queryKey: ['supplyarr-part-readiness', accessToken, selectedPartId, quantity],
    queryFn: () => getPartSupplyReadiness(accessToken, selectedPartId, quantity),
    enabled: canRead && checkMode === 'part' && Boolean(selectedPartId),
  })

  const supplierUnitQuery = useQuery({
    queryKey: ['supplyarr-supplier-unit-readiness', accessToken, selectedSupplierUnitId],
    queryFn: () => getSupplierReadiness(accessToken, selectedSupplierUnitId),
    enabled: canRead && checkMode === 'supplier' && Boolean(selectedSupplierUnitId),
  })

  const pathQuery = useQuery({
    queryKey: [
      'supplyarr-procurement-path-readiness',
      accessToken,
      selectedPartId,
      selectedSupplierUnitId,
      quantity,
    ],
    queryFn: () =>
      getProcurementPathReadiness(accessToken, selectedPartId, selectedSupplierUnitId, quantity),
    enabled:
      canRead && checkMode === 'path' && Boolean(selectedPartId) && Boolean(selectedSupplierUnitId),
  })

  if (!canRead) {
    return null
  }

  const activeResult =
    checkMode === 'part'
      ? partQuery.data
      : checkMode === 'supplier'
        ? supplierUnitQuery.data
        : pathQuery.data

  const isLoading =
    checkMode === 'part'
      ? partQuery.isLoading
      : checkMode === 'supplier'
        ? supplierUnitQuery.isLoading
        : pathQuery.isLoading

  const blockers = activeResult?.blockers ?? []

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="supply-readiness-check-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Readiness check</h2>
      <p className="mt-1 text-sm text-slate-400">
        Evaluate part availability, supplier approval, and procurement path blockers with stable reason
        codes for related workflows. Supplier checks help confirm which branch or dealer location is ready to source from.
      </p>

      <div className="mt-4 flex flex-wrap gap-2">
        {(['part', 'supplier', 'path'] as const).map((mode) => (
          <button
            key={mode}
            type="button"
            className={`rounded px-3 py-1 text-sm ${
              checkMode === mode
                ? 'bg-sky-700 text-white'
                : 'border border-slate-600 text-slate-300'
            }`}
            onClick={() => setCheckMode(mode)}
          >
            {mode === 'part' ? 'Part' : mode === 'supplier' ? 'Supplier identity or sub-unit' : 'Procurement path'}
          </button>
        ))}
      </div>

      <div className="mt-4 grid gap-4 md:grid-cols-2">
        {(checkMode === 'part' || checkMode === 'path') && (
          <StaticSearchPicker
            id="readiness-check-part"
            label="Part"
            value={selectedPartId}
            onChange={setSelectedPartId}
            options={partOptions}
            selectedOption={selectedPartOption}
            placeholder="Search parts…"
            testId="readiness-check-part-picker"
          />
        )}

        {(checkMode === 'supplier' || checkMode === 'path') && (
          <StaticSearchPicker
            id="readiness-check-supplier-unit"
            label="Supplier identity or sub-unit"
            value={selectedSupplierUnitId}
            onChange={setSelectedSupplierUnitId}
            options={supplierUnitOptions}
            selectedOption={selectedSupplierUnitOption}
            placeholder="Search supplier identities or sub-units…"
            testId="readiness-check-supplier-unit-picker"
          />
        )}

        {(checkMode === 'part' || checkMode === 'path') && (
          <label htmlFor="readiness-check-qty" className="block text-sm text-slate-400">
            Requested quantity (optional)
            <input
              id="readiness-check-qty"
              type="number"
              min="0"
              step="any"
              className="mt-1 w-full rounded border border-slate-700 bg-slate-950 px-2 py-1 text-white"
              value={requestedQuantity}
              onChange={(event) => setRequestedQuantity(event.target.value)}
            />
          </label>
        )}
      </div>

      {isLoading && <p className="mt-4 text-sm text-[var(--color-text-muted)]">Evaluating readiness…</p>}

      {activeResult && (
        <div className="mt-4 rounded-md border border-slate-800 bg-slate-950/60 p-4">
          <div className="flex flex-wrap items-center gap-2">
            <span
              className={`rounded px-2 py-0.5 text-xs font-medium ${readinessBadgeClass(activeResult.readinessStatus)}`}
            >
              {activeResult.readinessStatus}
            </span>
            {'partKey' in activeResult && (
              <span className="text-sm text-slate-200">
                {activeResult.partKey}
                {'supplierKey' in activeResult && activeResult.supplierKey
                  ? ` → ${activeResult.supplierKey}`
                  : ''}
              </span>
            )}
            {'supplierKey' in activeResult &&
              !('partKey' in activeResult && (activeResult as { partKey?: string }).partKey) && (
                <span className="text-sm text-slate-200">{activeResult.supplierKey}</span>
              )}
          </div>

          {'availability' in activeResult && (
            <p className="mt-2 text-sm text-slate-400">
              Available {activeResult.availability.quantityAvailable} · on hand{' '}
              {activeResult.availability.quantityOnHand} · reserved{' '}
              {activeResult.availability.quantityReserved}
            </p>
          )}

          {blockers.length === 0 ? (
            <p className="mt-3 text-sm text-emerald-300">No blockers detected.</p>
          ) : (
            <ul className="mt-3 divide-y divide-slate-800 text-sm">
              {blockers.map((blocker) => (
                <li key={`${blocker.reasonCode}-${blocker.sourceEntityId}`} className="py-2">
                  <code className="text-xs text-amber-300">{blocker.reasonCode}</code>
                  <p className="text-slate-200">{blocker.message}</p>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </section>
  )
}
