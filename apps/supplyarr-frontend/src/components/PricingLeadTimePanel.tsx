import { ControlledSelect } from '@stl/shared-ui'

import type {
  LeadTimeSnapshotResponse,
  PartResponse,
  PricingSnapshotResponse,
} from '../api/types'
import { CURRENCY_OPTIONS } from '../forms/controlledFormHelpers'
import { GeneratedKeyFieldGroup } from '../forms/GeneratedKeyFieldGroup'

interface PricingLeadTimePanelProps {
  parts: PartResponse[]
  pricingSnapshots: PricingSnapshotResponse[]
  leadTimeSnapshots: LeadTimeSnapshotResponse[]
  canManage: boolean
  isLoading: boolean
  pricingSnapshotKey: string
  leadTimeSnapshotKey: string
  selectedVendorLinkId: string
  unitPrice: string
  currencyCode: string
  minimumOrderQuantity: string
  leadTimeDays: string
  snapshotNotes: string
  currentOnlyFilter: boolean
  onPricingSnapshotKeyChange: (value: string) => void
  onLeadTimeSnapshotKeyChange: (value: string) => void
  onSelectedVendorLinkIdChange: (value: string) => void
  onUnitPriceChange: (value: string) => void
  onCurrencyCodeChange: (value: string) => void
  onMinimumOrderQuantityChange: (value: string) => void
  onLeadTimeDaysChange: (value: string) => void
  onSnapshotNotesChange: (value: string) => void
  onCurrentOnlyFilterChange: (value: boolean) => void
  onCreatePricingSnapshot: () => void
  onCreateLeadTimeSnapshot: () => void
  isCreatingPricing: boolean
  isCreatingLeadTime: boolean
}

export function PricingLeadTimePanel({
  parts,
  pricingSnapshots,
  leadTimeSnapshots,
  canManage,
  isLoading,
  pricingSnapshotKey,
  leadTimeSnapshotKey,
  selectedVendorLinkId,
  unitPrice,
  currencyCode,
  minimumOrderQuantity,
  leadTimeDays,
  snapshotNotes,
  currentOnlyFilter,
  onPricingSnapshotKeyChange,
  onLeadTimeSnapshotKeyChange,
  onSelectedVendorLinkIdChange,
  onUnitPriceChange,
  onCurrencyCodeChange,
  onMinimumOrderQuantityChange,
  onLeadTimeDaysChange,
  onSnapshotNotesChange,
  onCurrentOnlyFilterChange,
  onCreatePricingSnapshot,
  onCreateLeadTimeSnapshot,
  isCreatingPricing,
  isCreatingLeadTime,
}: PricingLeadTimePanelProps) {
  const vendorLinks = parts.flatMap((part) =>
    part.vendorLinks.map((link) => ({
      linkId: link.linkId,
      label: `${part.partKey} · ${link.partyKey} · ${link.vendorPartNumber}`,
    })),
  )
  const selectedLink = vendorLinks.find((link) => link.linkId === selectedVendorLinkId)
  const pricingKeySource = selectedLink ? `${selectedLink.label}-price` : ''
  const leadTimeKeySource = selectedLink ? `${selectedLink.label}-lead` : ''
  const existingPricingKeys = pricingSnapshots.map((row) => row.snapshotKey)
  const existingLeadTimeKeys = leadTimeSnapshots.map((row) => row.snapshotKey)

  const filteredPricing = currentOnlyFilter
    ? pricingSnapshots.filter((row) => row.isCurrent)
    : pricingSnapshots
  const filteredLeadTime = currentOnlyFilter
    ? leadTimeSnapshots.filter((row) => row.isCurrent)
    : leadTimeSnapshots

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg lg:col-span-2">
      <h2 className="text-lg font-medium text-white">Pricing &amp; lead time</h2>
      <p className="mt-1 text-sm text-slate-400">
        Record vendor part link unit price and lead-time history with effective dates.
      </p>

      <label className="mt-4 flex items-center gap-2 text-sm text-slate-400">
        <input
          type="checkbox"
          className="rounded border-slate-600"
          checked={currentOnlyFilter}
          onChange={(e) => onCurrentOnlyFilterChange(e.target.checked)}
        />
        Show current snapshots only
      </label>

      {isLoading ? <p className="mt-4 text-sm text-slate-500">Loading snapshots…</p> : null}

      <div className="mt-4 grid gap-6 lg:grid-cols-2">
        <div>
          <h3 className="text-sm font-medium text-slate-300">Pricing history</h3>
          <ul className="mt-2 max-h-48 space-y-2 overflow-y-auto">
            {filteredPricing.map((row) => (
              <li
                key={row.pricingSnapshotId}
                className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2 text-sm"
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="font-medium text-slate-200">{row.snapshotKey}</span>
                  {row.isCurrent ? (
                    <span className="rounded bg-emerald-500/20 px-2 py-0.5 text-xs text-emerald-300">
                      current
                    </span>
                  ) : null}
                </div>
                <p className="mt-1 text-slate-400">
                  {row.partKey} · {row.vendorPartyKey} · {row.unitPrice} {row.currencyCode}
                </p>
                <p className="text-xs text-slate-500">
                  effective {new Date(row.effectiveFrom).toLocaleDateString()}
                  {row.effectiveTo
                    ? ` – ${new Date(row.effectiveTo).toLocaleDateString()}`
                    : ' – open'}
                </p>
              </li>
            ))}
            {filteredPricing.length === 0 ? (
              <li className="text-sm text-slate-500">No pricing snapshots yet.</li>
            ) : null}
          </ul>
        </div>

        <div>
          <h3 className="text-sm font-medium text-slate-300">Lead-time history</h3>
          <ul className="mt-2 max-h-48 space-y-2 overflow-y-auto">
            {filteredLeadTime.map((row) => (
              <li
                key={row.leadTimeSnapshotId}
                className="rounded-lg border border-slate-800 bg-slate-950/60 px-3 py-2 text-sm"
              >
                <div className="flex items-center justify-between gap-2">
                  <span className="font-medium text-slate-200">{row.snapshotKey}</span>
                  {row.isCurrent ? (
                    <span className="rounded bg-emerald-500/20 px-2 py-0.5 text-xs text-emerald-300">
                      current
                    </span>
                  ) : null}
                </div>
                <p className="mt-1 text-slate-400">
                  {row.partKey} · {row.vendorPartyKey} · {row.leadTimeDays} days
                </p>
                <p className="text-xs text-slate-500">
                  effective {new Date(row.effectiveFrom).toLocaleDateString()}
                  {row.effectiveTo
                    ? ` – ${new Date(row.effectiveTo).toLocaleDateString()}`
                    : ' – open'}
                </p>
              </li>
            ))}
            {filteredLeadTime.length === 0 ? (
              <li className="text-sm text-slate-500">No lead-time snapshots yet.</li>
            ) : null}
          </ul>
        </div>
      </div>

      {canManage ? (
        <div className="mt-6 space-y-4 rounded-lg border border-slate-800 bg-slate-950/40 p-4">
          <label className="block text-sm text-slate-400">
            Vendor part link
            <select
              className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
              value={selectedVendorLinkId}
              onChange={(e) => onSelectedVendorLinkIdChange(e.target.value)}
            >
              <option value="">Select link…</option>
              {vendorLinks.map((link) => (
                <option key={link.linkId} value={link.linkId}>
                  {link.label}
                </option>
              ))}
            </select>
          </label>

          <div className="grid gap-3 sm:grid-cols-2">
            <GeneratedKeyFieldGroup
              sourceLabel={pricingKeySource}
              existingKeys={existingPricingKeys}
              onKeyChange={onPricingSnapshotKeyChange}
              label="Pricing snapshot key"
            />
            <GeneratedKeyFieldGroup
              sourceLabel={leadTimeKeySource}
              existingKeys={existingLeadTimeKeys}
              onKeyChange={onLeadTimeSnapshotKeyChange}
              label="Lead-time snapshot key"
            />
            <label className="block text-sm text-slate-400">
              Unit price
              <input
                type="number"
                min="0"
                step="0.01"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={unitPrice}
                onChange={(e) => onUnitPriceChange(e.target.value)}
              />
            </label>
            <ControlledSelect
              label="Currency"
              value={currencyCode}
              onChange={onCurrencyCodeChange}
              options={CURRENCY_OPTIONS}
            />
            <label className="block text-sm text-slate-400">
              Minimum order qty (optional)
              <input
                type="number"
                min="0"
                step="0.01"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={minimumOrderQuantity}
                onChange={(e) => onMinimumOrderQuantityChange(e.target.value)}
              />
            </label>
            <label className="block text-sm text-slate-400">
              Lead time (days)
              <input
                type="number"
                min="0"
                step="1"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={leadTimeDays}
                onChange={(e) => onLeadTimeDaysChange(e.target.value)}
              />
            </label>
          </div>

          <label className="block text-sm text-slate-400">
            Notes
            <textarea
              className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
              rows={2}
              value={snapshotNotes}
              onChange={(e) => onSnapshotNotesChange(e.target.value)}
            />
          </label>

          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              className="rounded-lg bg-sky-600 px-4 py-2 text-sm font-medium text-white hover:bg-sky-500 disabled:opacity-50"
              disabled={isCreatingPricing || !selectedVendorLinkId || !pricingSnapshotKey || !unitPrice}
              onClick={onCreatePricingSnapshot}
            >
              {isCreatingPricing ? 'Saving price…' : 'Record pricing'}
            </button>
            <button
              type="button"
              className="rounded-lg bg-violet-600 px-4 py-2 text-sm font-medium text-white hover:bg-violet-500 disabled:opacity-50"
              disabled={
                isCreatingLeadTime || !selectedVendorLinkId || !leadTimeSnapshotKey || !leadTimeDays
              }
              onClick={onCreateLeadTimeSnapshot}
            >
              {isCreatingLeadTime ? 'Saving lead time…' : 'Record lead time'}
            </button>
          </div>
        </div>
      ) : null}
    </section>
  )
}
