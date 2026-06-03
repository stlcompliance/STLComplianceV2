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
      partId: part.partId,
      partKey: part.partKey,
      partDisplayName: part.displayName,
      vendorPartyId: link.partyId,
      vendorPartyKey: link.partyKey,
      vendorDisplayName: link.partyDisplayName,
      vendorPartNumber: link.vendorPartNumber,
      isPreferred: link.isPreferred,
      catalogUnitPrice: link.catalogUnitPrice,
      catalogLeadTimeDays: link.catalogLeadTimeDays,
      label: `${part.partKey} · ${link.partyKey} · ${link.vendorPartNumber}`,
    })),
  )
  const selectedLink = vendorLinks.find((link) => link.linkId === selectedVendorLinkId)
  const selectedPart = selectedLink ? parts.find((part) => part.partId === selectedLink.partId) : null
  const pricingKeySource = selectedLink ? `${selectedLink.label}-price` : ''
  const leadTimeKeySource = selectedLink ? `${selectedLink.label}-lead` : ''
  const existingPricingKeys = pricingSnapshots.map((row) => row.snapshotKey)
  const existingLeadTimeKeys = leadTimeSnapshots.map((row) => row.snapshotKey)
  const selectedPartLinks = selectedPart
    ? vendorLinks.filter((link) => link.partId === selectedPart.partId)
    : []
  const linkMetrics = selectedPartLinks.map((link) => {
    const currentPricingSnapshot =
      pricingSnapshots.find((snapshot) => snapshot.partVendorLinkId === link.linkId && snapshot.isCurrent) ??
      pricingSnapshots.find((snapshot) => snapshot.partVendorLinkId === link.linkId)
    const currentLeadTimeSnapshot =
      leadTimeSnapshots.find((snapshot) => snapshot.partVendorLinkId === link.linkId && snapshot.isCurrent) ??
      leadTimeSnapshots.find((snapshot) => snapshot.partVendorLinkId === link.linkId)
    const unitPrice = currentPricingSnapshot?.unitPrice ?? link.catalogUnitPrice
    const leadTimeDays = currentLeadTimeSnapshot?.leadTimeDays ?? link.catalogLeadTimeDays
    const priceScore = unitPrice == null ? 1000 : unitPrice
    const leadTimeScore = leadTimeDays == null ? 1000 : leadTimeDays * 0.25
    const preferredBonus = link.isPreferred ? -5 : 0
    return {
      ...link,
      unitPrice,
      leadTimeDays,
      currentPricingSnapshot,
      currentLeadTimeSnapshot,
      combinedScore: priceScore + leadTimeScore + preferredBonus,
    }
  })
  const recommendedLink = linkMetrics.length
    ? [...linkMetrics].sort((a, b) => a.combinedScore - b.combinedScore)[0]
    : null
  const bestPriceLink =
    [...linkMetrics].filter((link) => link.unitPrice != null).sort((a, b) => (a.unitPrice! - b.unitPrice!))[0] ??
    null
  const bestLeadTimeLink =
    [...linkMetrics].filter((link) => link.leadTimeDays != null).sort((a, b) => (a.leadTimeDays! - b.leadTimeDays!))[0] ??
    null

  const filteredPricing = currentOnlyFilter
    ? pricingSnapshots.filter((row) => row.isCurrent)
    : pricingSnapshots
  const filteredLeadTime = currentOnlyFilter
    ? leadTimeSnapshots.filter((row) => row.isCurrent)
    : leadTimeSnapshots

  return (
    <section
      data-testid="pricing-lead-time-panel"
      className="rounded-xl border border-slate-800 bg-slate-900/60 p-5 shadow-lg lg:col-span-2"
    >
      <h2 className="text-lg font-medium text-white">Pricing &amp; lead time</h2>
      <p className="mt-1 text-sm text-slate-400">
        Record vendor part link unit price and lead-time history with effective dates.
      </p>

      <label htmlFor="pricing-lead-time-current-only-filter" className="mt-4 flex items-center gap-2 text-sm text-slate-400">
        <input
          id="pricing-lead-time-current-only-filter"
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

      {selectedPart && recommendedLink ? (
        <div className="mt-6 rounded-lg border border-sky-800/60 bg-sky-950/30 p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 className="text-sm font-semibold uppercase tracking-wide text-sky-200">
                Source recommendations
              </h3>
              <p className="mt-1 text-sm text-slate-200">
                Recommended source for {selectedPart.partKey} · {selectedPart.displayName}
              </p>
              <p className="mt-1 text-xs text-slate-400">
                Based on current price, current lead time, and preferred-vendor status.
              </p>
            </div>
            <span className="rounded-full bg-sky-500/20 px-3 py-1 text-xs font-semibold uppercase tracking-wide text-sky-200">
              Recommended
            </span>
          </div>

          <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            <Metric
              label="Recommended vendor"
              value={`${recommendedLink.vendorPartyKey} · ${recommendedLink.vendorDisplayName}`}
            />
            <Metric label="Unit price" value={formatMoney(recommendedLink.unitPrice)} />
            <Metric label="Lead time" value={formatDays(recommendedLink.leadTimeDays)} />
            <Metric
              label="Reason"
              value={
                recommendedLink.linkId === bestPriceLink?.linkId && recommendedLink.linkId === bestLeadTimeLink?.linkId
                  ? 'Best price and lead time'
                  : recommendedLink.linkId === bestPriceLink?.linkId
                    ? 'Lowest current price'
                    : recommendedLink.linkId === bestLeadTimeLink?.linkId
                      ? 'Shortest current lead time'
                      : recommendedLink.isPreferred
                        ? 'Preferred vendor with balanced current metrics'
                        : 'Balanced current source score'
              }
            />
            <Metric label="Part source" value={recommendedLink.vendorPartNumber} />
            <Metric
              label="Current snapshots"
              value={`${recommendedLink.currentPricingSnapshot ? 'price' : 'no price'} · ${recommendedLink.currentLeadTimeSnapshot ? 'lead time' : 'no lead time'}`}
            />
          </div>

          {selectedPartLinks.length > 1 ? (
            <div className="mt-4">
              <h4 className="text-xs font-semibold uppercase tracking-wide text-slate-400">
                Comparison
              </h4>
              <ul className="mt-2 space-y-2 text-sm">
                {linkMetrics.map((link) => (
                  <li
                    key={link.linkId}
                    className={`rounded-md border px-3 py-2 ${
                      link.linkId === recommendedLink.linkId
                        ? 'border-sky-500/50 bg-sky-950/40'
                        : 'border-slate-800 bg-slate-950/50'
                    }`}
                  >
                    <div className="flex flex-wrap items-start justify-between gap-2">
                      <div>
                        <div className="font-medium text-slate-100">
                          {link.vendorPartyKey} · {link.vendorDisplayName}
                        </div>
                        <div className="mt-1 text-xs text-slate-500">
                          {link.vendorPartNumber}
                          {link.isPreferred ? ' · preferred' : ''}
                        </div>
                      </div>
                      {link.linkId === recommendedLink.linkId ? (
                        <span className="text-xs font-semibold uppercase tracking-wide text-sky-300">
                          Recommended
                        </span>
                      ) : null}
                    </div>
                    <p className="mt-2 text-xs text-slate-400">
                      Price {formatMoney(link.unitPrice)} · Lead time {formatDays(link.leadTimeDays)}
                    </p>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}
        </div>
      ) : null}

      {canManage ? (
        <div className="mt-6 space-y-4 rounded-lg border border-slate-800 bg-slate-950/40 p-4">
          <label htmlFor="pricing-lead-time-vendor-link" className="block text-sm text-slate-400">
            Vendor part link
            <select
              id="pricing-lead-time-vendor-link"
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
              domain="purchase"
              kind="pricing"
              label="Pricing snapshot key"
            />
            <GeneratedKeyFieldGroup
              sourceLabel={leadTimeKeySource}
              existingKeys={existingLeadTimeKeys}
              onKeyChange={onLeadTimeSnapshotKeyChange}
              domain="purchase"
              kind="leadtime"
              label="Lead-time snapshot key"
            />
            <label htmlFor="pricing-lead-time-unit-price" className="block text-sm text-slate-400">
              Unit price
              <input
                id="pricing-lead-time-unit-price"
                type="number"
                min="0"
                step="0.01"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={unitPrice}
                onChange={(e) => onUnitPriceChange(e.target.value)}
              />
            </label>
            <ControlledSelect
              id="pricing-lead-time-currency"
              label="Currency"
              value={currencyCode}
              onChange={onCurrencyCodeChange}
              options={CURRENCY_OPTIONS}
            />
            <label htmlFor="pricing-lead-time-min-order-qty" className="block text-sm text-slate-400">
              Minimum order qty (optional)
              <input
                id="pricing-lead-time-min-order-qty"
                type="number"
                min="0"
                step="0.01"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={minimumOrderQuantity}
                onChange={(e) => onMinimumOrderQuantityChange(e.target.value)}
              />
            </label>
            <label htmlFor="pricing-lead-time-days" className="block text-sm text-slate-400">
              Lead time (days)
              <input
                id="pricing-lead-time-days"
                type="number"
                min="0"
                step="1"
                className="mt-1 block w-full rounded-lg border border-slate-700 bg-slate-950 px-3 py-2 text-slate-200"
                value={leadTimeDays}
                onChange={(e) => onLeadTimeDaysChange(e.target.value)}
              />
            </label>
          </div>

          <label htmlFor="pricing-lead-time-snapshot-notes" className="block text-sm text-slate-400">
            Snapshot notes
            <textarea
              id="pricing-lead-time-snapshot-notes"
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

function formatMoney(value: number | null): string {
  return value == null ? 'n/a' : value.toFixed(2)
}

function formatDays(value: number | null): string {
  return value == null ? 'n/a' : `${value} day${value === 1 ? '' : 's'}`
}

function Metric({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-800 bg-slate-950/60 px-3 py-2">
      <div className="text-[11px] uppercase tracking-wide text-slate-500">{label}</div>
      <div className="mt-1 text-sm font-semibold text-slate-100">{value}</div>
    </div>
  )
}
