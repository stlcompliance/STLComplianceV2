import { StaticSearchPicker, type PickerOption } from '@stl/shared-ui'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useMemo, useState } from 'react'

import { syncVendorCatalogApi } from '../api/client'
import type { PartResponse, VendorCatalogApiSyncItem, VendorCatalogApiSyncResponse } from '../api/types'
import { toPartyPickerOptions } from '../forms/controlledFormHelpers'

interface VendorCatalogApiPanelProps {
  accessToken: string
  canManage: boolean
  parts: PartResponse[]
  vendors: { partyId: string; displayName: string; partyKey: string }[]
}

type VendorCatalogEntryRow = {
  linkId: string
  partId: string
  partKey: string
  partDisplayName: string
  vendorPartyId: string
  vendorPartyKey: string
  vendorDisplayName: string
  vendorPartNumber: string
  isPreferred: boolean
  catalogUnitPrice: number | null
  catalogCurrencyCode: string | null
  catalogMinimumOrderQuantity: number | null
  catalogLeadTimeDays: number | null
  catalogQuantityAvailable: number | null
  catalogAvailabilityStatus: string | null
}

function formatMoney(value: number | null): string {
  if (value == null) return '—'
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: value >= 1000 ? 0 : 2,
  }).format(value)
}

export function VendorCatalogApiPanel({ accessToken, canManage, parts, vendors }: VendorCatalogApiPanelProps) {
  const queryClient = useQueryClient()
  const [selectedVendorId, setSelectedVendorId] = useState('')
  const [payloadJson, setPayloadJson] = useState('[]')
  const [dryRun, setDryRun] = useState(true)
  const [syncResult, setSyncResult] = useState<VendorCatalogApiSyncResponse | null>(null)
  const [syncError, setSyncError] = useState<string | null>(null)
  const vendorOptions = useMemo<PickerOption[]>(() => toPartyPickerOptions(vendors), [vendors])
  const selectedVendor = useMemo(
    () => vendors.find((vendor) => vendor.partyId === selectedVendorId) ?? null,
    [selectedVendorId, vendors],
  )
  const selectedVendorOption = useMemo<PickerOption | undefined>(
    () =>
      vendorOptions.find((option) => option.value === selectedVendorId) ??
      (selectedVendor
        ? {
            value: selectedVendor.partyId,
            label: `${selectedVendor.partyKey} — ${selectedVendor.displayName}`,
          }
        : undefined),
    [selectedVendor, selectedVendorId, vendorOptions],
  )

  useEffect(() => {
    if (!selectedVendorId && vendors[0]) {
      setSelectedVendorId(vendors[0].partyId)
    }
  }, [selectedVendorId, vendors])

  const currentEntries = useMemo<VendorCatalogEntryRow[]>(
    () =>
      parts
        .flatMap((part) =>
          part.vendorLinks.map((link) => ({
            linkId: link.linkId,
            partId: part.partId,
            partKey: part.partKey,
            partDisplayName: part.displayName,
            vendorPartyId: link.partyId,
            vendorPartyKey: link.partyKey,
            vendorDisplayName: link.partyDisplayName,
            vendorPartNumber: link.vendorPartNumber,
            isPreferred: link.isPreferred,
            catalogUnitPrice: link.catalogUnitPrice,
            catalogCurrencyCode: link.catalogCurrencyCode,
            catalogMinimumOrderQuantity: link.catalogMinimumOrderQuantity,
            catalogLeadTimeDays: link.catalogLeadTimeDays,
            catalogQuantityAvailable: link.catalogQuantityAvailable,
            catalogAvailabilityStatus: link.catalogAvailabilityStatus,
          })),
        )
        .filter((entry) => !selectedVendorId || entry.vendorPartyId === selectedVendorId)
        .sort((left, right) => {
          if (left.vendorDisplayName !== right.vendorDisplayName) {
            return left.vendorDisplayName.localeCompare(right.vendorDisplayName)
          }
          return left.partDisplayName.localeCompare(right.partDisplayName)
        }),
    [parts, selectedVendorId],
  )

  const syncMutation = useMutation({
    mutationFn: (vars: { vendorPartyKey: string; items: VendorCatalogApiSyncItem[] }) =>
      syncVendorCatalogApi(accessToken, {
        vendorPartyKey: vars.vendorPartyKey,
        dryRun,
        items: vars.items,
      }),
    onSuccess: async (result) => {
      setSyncResult(result)
      setSyncError(null)
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-parts'] })
      await queryClient.invalidateQueries({ queryKey: ['supplyarr-vendors'] })
    },
    onError: (error: unknown) => {
      setSyncError(error instanceof Error ? error.message : 'Failed to sync vendor catalog feed.')
    },
  })

  const handleSync = () => {
    if (!selectedVendor) {
      setSyncError('Select a vendor before syncing vendor catalog data.')
      return
    }

    try {
      const parsed = JSON.parse(payloadJson) as VendorCatalogApiSyncItem[]
      if (!Array.isArray(parsed)) {
        setSyncError('Payload must be a JSON array.')
        return
      }

      const normalizedItems = parsed.map((item) => ({
        partKey: typeof item.partKey === 'string' ? item.partKey.trim() : '',
        vendorPartNumber: typeof item.vendorPartNumber === 'string' ? item.vendorPartNumber.trim() : '',
        isPreferred: Boolean(item.isPreferred),
        catalogUnitPrice: typeof item.catalogUnitPrice === 'number' ? item.catalogUnitPrice : null,
        catalogCurrencyCode:
          typeof item.catalogCurrencyCode === 'string' ? item.catalogCurrencyCode : null,
        catalogMinimumOrderQuantity:
          typeof item.catalogMinimumOrderQuantity === 'number' ? item.catalogMinimumOrderQuantity : null,
        catalogLeadTimeDays: typeof item.catalogLeadTimeDays === 'number' ? item.catalogLeadTimeDays : null,
        catalogQuantityAvailable:
          typeof item.catalogQuantityAvailable === 'number' ? item.catalogQuantityAvailable : null,
        catalogAvailabilityStatus:
          typeof item.catalogAvailabilityStatus === 'string' ? item.catalogAvailabilityStatus : null,
      }))

      if (normalizedItems.length === 0) {
        setSyncError('Payload must include at least one vendor catalog row.')
        return
      }

      setSyncError(null)
      syncMutation.mutate({
        vendorPartyKey: selectedVendor.partyKey,
        items: normalizedItems,
      })
    } catch (error) {
      setSyncError(error instanceof Error ? error.message : 'Payload must be valid JSON.')
    }
  }

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-5" data-testid="vendor-catalog-api-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-medium text-white">Vendor catalog APIs</h2>
          <p className="mt-1 text-sm text-slate-400">
            Sync catalog facts from external vendor APIs while keeping the canonical catalog under SupplyArr control.
          </p>
        </div>
        <span className="rounded-full border border-slate-700 px-3 py-1 text-xs uppercase tracking-wide text-slate-400">
          {currentEntries.length} current link{currentEntries.length === 1 ? '' : 's'}
        </span>
      </div>

      <div className="mt-4 grid gap-4 lg:grid-cols-[minmax(0,320px)_minmax(0,1fr)]">
        <div className="space-y-4">
          <StaticSearchPicker
            id="vendor-catalog-api-vendor"
            label="Vendor"
            value={selectedVendorId}
            onChange={(value) => {
              setSelectedVendorId(value)
              setSyncError(null)
            }}
            options={vendorOptions}
            selectedOption={selectedVendorOption}
            placeholder="Select vendor"
            testId="vendor-catalog-api-vendor-picker"
          />

          <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <div className="text-xs uppercase tracking-wide text-slate-500">Current feed status</div>
            <div className="mt-2 space-y-1 text-sm text-slate-300">
              <div>Selected vendor: {selectedVendor?.displayName ?? 'Not selected'}</div>
              <div>Linked parts in view: {currentEntries.length}</div>
              <div>Sync mode: JSON API payload</div>
            </div>
          </div>
        </div>

        <div className="space-y-3">
          <label htmlFor="vendor-catalog-api-payload" className="block text-sm text-slate-400">
            Vendor catalog JSON payload
            <textarea
              id="vendor-catalog-api-payload"
              className="mt-1 min-h-[14rem] w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 font-mono text-xs text-slate-100"
              value={payloadJson}
              onChange={(event) => {
                setPayloadJson(event.target.value)
                setSyncError(null)
              }}
            />
          </label>

          <div className="flex flex-wrap gap-3">
            <label className="flex items-center gap-2 text-sm text-slate-400">
              <input
                type="checkbox"
                checked={dryRun}
                onChange={(event) => setDryRun(event.target.checked)}
              />
              Dry run
            </label>
            <button
              type="button"
              className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={!canManage || !selectedVendor || syncMutation.isPending}
              onClick={handleSync}
            >
              {syncMutation.isPending ? 'Syncing…' : 'Sync feed'}
            </button>
          </div>

          {!canManage ? (
            <p className="text-sm text-slate-500">Read-only access: vendor catalog API sync requires manage permission.</p>
          ) : null}

          {syncError ? (
            <div className="rounded-lg border border-rose-500/40 bg-rose-500/10 p-3 text-sm text-rose-200" role="alert">
              {syncError}
            </div>
          ) : null}

          {syncResult ? (
            <div className="rounded-lg border border-emerald-500/30 bg-emerald-500/10 p-3 text-sm text-emerald-200">
              <div className="font-medium">
                {syncResult.success ? 'Sync succeeded' : 'Sync completed with validation issues'}
              </div>
              <div className="mt-1 text-emerald-100/90">
                {syncResult.itemsRead} rows read · {syncResult.itemsAccepted} accepted · {syncResult.itemsApplied}{' '}
                applied{syncResult.dryRun ? ' · dry run' : ''}
              </div>
              {syncResult.issues.length > 0 ? (
                <ul className="mt-2 list-disc space-y-1 pl-5 text-emerald-50/90">
                  {syncResult.issues.map((issue) => (
                    <li key={`${issue.itemNumber}-${issue.code}`}>
                      Row {issue.itemNumber}: {issue.code} - {issue.message}
                    </li>
                  ))}
                </ul>
              ) : null}
            </div>
          ) : null}
        </div>
      </div>

      <div className="mt-6">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <h3 className="text-sm font-medium text-slate-300">Current vendor catalog links</h3>
          <span className="text-xs uppercase tracking-wide text-slate-500">
            Derived from the live part catalog and vendor links
          </span>
        </div>

        {currentEntries.length === 0 ? (
          <p className="mt-3 text-sm text-slate-500">No vendor catalog links found for this filter.</p>
        ) : (
          <div className="mt-3 overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="text-xs uppercase tracking-wide text-slate-500">
                <tr>
                  <th className="border-b border-slate-800 px-3 py-2">Part</th>
                  <th className="border-b border-slate-800 px-3 py-2">Vendor</th>
                  <th className="border-b border-slate-800 px-3 py-2">Vendor part #</th>
                  <th className="border-b border-slate-800 px-3 py-2">Price</th>
                  <th className="border-b border-slate-800 px-3 py-2">Lead time</th>
                  <th className="border-b border-slate-800 px-3 py-2">Availability</th>
                </tr>
              </thead>
              <tbody>
                {currentEntries.map((entry) => (
                  <tr key={entry.linkId} className="border-b border-slate-800/70">
                    <td className="px-3 py-2 text-slate-200">
                      <div className="font-medium">{entry.partDisplayName}</div>
                      <div className="text-xs text-slate-500">{entry.partKey}</div>
                    </td>
                    <td className="px-3 py-2 text-slate-300">{entry.vendorDisplayName}</td>
                    <td className="px-3 py-2 text-slate-300">
                      {entry.vendorPartNumber}
                      {entry.isPreferred ? (
                        <span className="ml-2 rounded-full bg-cyan-500/15 px-2 py-0.5 text-[10px] uppercase tracking-wide text-cyan-300">
                          preferred
                        </span>
                      ) : null}
                    </td>
                    <td className="px-3 py-2 text-slate-300">
                      {formatMoney(entry.catalogUnitPrice)}
                      {entry.catalogCurrencyCode ? ` ${entry.catalogCurrencyCode}` : ''}
                    </td>
                    <td className="px-3 py-2 text-slate-300">
                      {entry.catalogLeadTimeDays != null ? `${entry.catalogLeadTimeDays} days` : '—'}
                    </td>
                    <td className="px-3 py-2 text-slate-300">
                      {entry.catalogQuantityAvailable != null ? `${entry.catalogQuantityAvailable} qty` : '—'}
                      {entry.catalogAvailabilityStatus ? ` · ${entry.catalogAvailabilityStatus}` : ''}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  )
}
