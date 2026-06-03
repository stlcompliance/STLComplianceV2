import { useMemo, useState } from 'react'

import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useQuery } from '@tanstack/react-query'

import {
  getPartSupplyReadiness,
  getProcurementPathReadiness,
  getVendorSupplyReadiness,
} from '../api/client'
import type { ExternalPartyResponse, PartResponse } from '../api/types'

interface SupplyReadinessCheckPanelProps {
  accessToken: string
  canRead: boolean
  parts: PartResponse[]
  vendors: ExternalPartyResponse[]
}

function readinessBadgeClass(status: string): string {
  return status === 'ready'
    ? 'bg-emerald-500/20 text-emerald-200'
    : 'bg-rose-500/20 text-rose-200'
}

export function SupplyReadinessCheckPanel({
  accessToken,
  canRead,
  parts,
  vendors,
}: SupplyReadinessCheckPanelProps) {
  const [checkMode, setCheckMode] = useState<'part' | 'vendor' | 'path'>('part')
  const [selectedPartId, setSelectedPartId] = useState('')
  const [selectedVendorId, setSelectedVendorId] = useState('')
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
  const vendorOptions = useMemo<PickerOption[]>(
    () =>
      vendors.map((vendor) => ({
        value: vendor.partyId,
        label: `${vendor.partyKey} · ${vendor.displayName}`,
      })),
    [vendors],
  )
  const selectedVendorOption = useMemo<PickerOption | undefined>(
    () => vendorOptions.find((option) => option.value === selectedVendorId),
    [selectedVendorId, vendorOptions],
  )

  const partQuery = useQuery({
    queryKey: ['supplyarr-part-readiness', accessToken, selectedPartId, quantity],
    queryFn: () => getPartSupplyReadiness(accessToken, selectedPartId, quantity),
    enabled: canRead && checkMode === 'part' && Boolean(selectedPartId),
  })

  const vendorQuery = useQuery({
    queryKey: ['supplyarr-vendor-readiness', accessToken, selectedVendorId],
    queryFn: () => getVendorSupplyReadiness(accessToken, selectedVendorId),
    enabled: canRead && checkMode === 'vendor' && Boolean(selectedVendorId),
  })

  const pathQuery = useQuery({
    queryKey: [
      'supplyarr-procurement-path-readiness',
      accessToken,
      selectedPartId,
      selectedVendorId,
      quantity,
    ],
    queryFn: () =>
      getProcurementPathReadiness(accessToken, selectedPartId, selectedVendorId, quantity),
    enabled:
      canRead && checkMode === 'path' && Boolean(selectedPartId) && Boolean(selectedVendorId),
  })

  if (!canRead) {
    return null
  }

  const activeResult =
    checkMode === 'part'
      ? partQuery.data
      : checkMode === 'vendor'
        ? vendorQuery.data
        : pathQuery.data

  const isLoading =
    checkMode === 'part'
      ? partQuery.isLoading
      : checkMode === 'vendor'
        ? vendorQuery.isLoading
        : pathQuery.isLoading

  const blockers = activeResult?.blockers ?? []

  return (
    <section
      className="rounded-xl border border-slate-700 bg-slate-900/80 p-5 lg:col-span-2"
      data-testid="supply-readiness-check-panel"
    >
      <h2 className="text-lg font-semibold text-slate-50">Readiness check</h2>
      <p className="mt-1 text-sm text-slate-400">
        Evaluate part availability, vendor approval, and procurement path blockers with stable reason
        codes for cross-product consumers.
      </p>

      <div className="mt-4 flex flex-wrap gap-2">
        {(['part', 'vendor', 'path'] as const).map((mode) => (
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
            {mode === 'part' ? 'Part' : mode === 'vendor' ? 'Vendor' : 'Procurement path'}
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

        {(checkMode === 'vendor' || checkMode === 'path') && (
          <StaticSearchPicker
            id="readiness-check-vendor"
            label="Vendor or supplier"
            value={selectedVendorId}
            onChange={setSelectedVendorId}
            options={vendorOptions}
            selectedOption={selectedVendorOption}
            placeholder="Search vendors…"
            testId="readiness-check-vendor-picker"
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

      {isLoading && <p className="mt-4 text-sm text-slate-500">Evaluating readiness…</p>}

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
                {'partyKey' in activeResult && activeResult.partyKey
                  ? ` → ${activeResult.partyKey}`
                  : ''}
              </span>
            )}
            {'partyKey' in activeResult &&
              !('partKey' in activeResult && (activeResult as { partKey?: string }).partKey) && (
                <span className="text-sm text-slate-200">{activeResult.partyKey}</span>
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
