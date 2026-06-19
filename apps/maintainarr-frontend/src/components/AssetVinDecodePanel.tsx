import { ChevronRight, Loader2, ScanSearch } from 'lucide-react'

import type { ExternalVinDecodeResponse } from '../api/types'

interface AssetVinDecodePanelProps {
  vin: string
  modelYear: number | null
  result: ExternalVinDecodeResponse | null
  isLoading: boolean
  error: string | null
}

function formatFieldLabel(key: string): string {
  return key
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
    .replace(/\b\w/g, (char) => char.toUpperCase())
}

function formatConfidence(value: number): string {
  return `${Math.round(value * 100)}%`
}

export function AssetVinDecodePanel({
  vin,
  modelYear,
  result,
  isLoading,
  error,
}: AssetVinDecodePanelProps) {
  const hasVin = vin.trim().length > 0

  return (
    <section className="rounded-xl border border-slate-800 bg-slate-900/60 p-5" data-testid="asset-vin-decode-panel">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="text-lg font-semibold text-white">VIN intelligence preview</h2>
          <p className="mt-1 text-sm text-slate-400">
            Live NHTSA decode for the value currently entered on the create form.
          </p>
        </div>
        <span className="inline-flex items-center gap-2 rounded-full border border-slate-700 px-3 py-1 text-xs text-slate-300">
          <ScanSearch className="h-4 w-4" />
          {modelYear ? `${modelYear}` : 'Any model year'}
        </span>
      </div>

      {!hasVin ? (
        <p className="mt-4 text-sm text-slate-400">Enter a VIN to see a decode preview.</p>
      ) : isLoading ? (
        <div className="mt-4 flex items-center gap-3 text-sm text-slate-300">
          <Loader2 className="h-4 w-4 animate-spin" />
          Decoding {vin}…
        </div>
      ) : error ? (
        <p className="mt-4 rounded-lg border border-rose-500/30 bg-rose-500/10 p-3 text-sm text-rose-100">
          {error}
        </p>
      ) : result ? (
        <div className="mt-4 space-y-4">
          <div className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
            <div className="flex flex-wrap items-center gap-2 text-xs text-slate-400">
              <span className="font-mono text-sky-100">{result.normalizedVin}</span>
              {result.isPartial ? <span className="rounded-full border border-amber-500/30 bg-amber-500/10 px-2 py-0.5 text-amber-100">Partial decode</span> : null}
            </div>
            <p className="mt-2 text-sm text-slate-300">{result.message ?? 'VIN decoded successfully.'}</p>
            <p className="mt-1 text-xs text-[var(--color-text-muted)]">
              Search criteria: {result.searchCriteria ?? 'Not returned'}
            </p>
            {result.errorText ? (
              <p className="mt-2 text-xs text-rose-200">
                {result.errorCode ? `${result.errorCode} · ` : ''}
                {result.errorText}
              </p>
            ) : null}
          </div>

          <div className="grid gap-2 md:grid-cols-2 xl:grid-cols-3">
            {[
              ['Make', result.decodedFields.Make],
              ['Model', result.decodedFields.Model],
              ['Model year', result.decodedFields.ModelYear],
              ['Manufacturer', result.decodedFields.Manufacturer],
              ['Body class', result.decodedFields.BodyClass],
              ['Vehicle type', result.decodedFields.VehicleType],
              ['Plant', result.decodedFields.PlantCompanyName],
              ['Fuel', result.decodedFields.FuelTypePrimary ?? result.decodedFields.FuelTypeSecondary],
            ]
              .filter(([, value]) => Boolean(value))
              .slice(0, 8)
              .map(([label, value]) => (
                <div key={label} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                  <div className="text-xs uppercase tracking-wide text-[var(--color-text-muted)]">{label}</div>
                  <div className="mt-1 text-sm font-medium text-white">{value}</div>
                </div>
              ))}
          </div>

          {result.suggestions.length > 0 ? (
            <div className="space-y-2">
              <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                <ChevronRight className="h-4 w-4" />
                Suggested field values
              </div>
              <ul className="space-y-2">
                {result.suggestions.slice(0, 4).map((suggestion) => (
                  <li key={suggestion.suggestionId} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                    <div className="flex flex-wrap items-start justify-between gap-3">
                      <div>
                        <div className="text-sm font-medium text-white">{suggestion.fieldLabel}</div>
                        <div className="mt-1 text-xs text-[var(--color-text-muted)]">
                          {formatFieldLabel(suggestion.fieldKey)} · {formatConfidence(suggestion.confidence)}
                        </div>
                      </div>
                      <span className="rounded-full border border-slate-700 px-2 py-0.5 text-[11px] uppercase tracking-wide text-slate-200">
                        {suggestion.status}
                      </span>
                    </div>
                    <div className="mt-2 text-xs text-slate-300">
                      <span className="text-[var(--color-text-muted)]">Current:</span> {suggestion.currentValue ?? 'Not recorded'}
                      <span className="mx-2 text-[var(--color-text-muted)]">·</span>
                      <span className="text-[var(--color-text-muted)]">Proposed:</span> {suggestion.proposedValue ?? 'Not proposed'}
                    </div>
                    <p className="mt-2 text-xs leading-5 text-slate-400">{suggestion.reason}</p>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

          {result.identifiers.length > 0 ? (
            <div className="space-y-2">
              <div className="flex items-center gap-2 text-xs uppercase tracking-wide text-[var(--color-text-muted)]">
                <ChevronRight className="h-4 w-4" />
                Identifiers
              </div>
              <ul className="space-y-2">
                {result.identifiers.slice(0, 4).map((identifier) => (
                  <li key={identifier.identifierId} className="rounded-lg border border-slate-800 bg-slate-950/60 p-3">
                    <div className="text-xs text-[var(--color-text-muted)]">
                      {identifier.sourceSystem} · {identifier.identifierType}
                    </div>
                    <div className="mt-1 break-all font-mono text-sm text-sky-100">{identifier.identifierValue}</div>
                  </li>
                ))}
              </ul>
            </div>
          ) : null}

          {result.snapshotId ? (
            <p className="text-xs text-[var(--color-text-muted)]">Snapshot ID: {result.snapshotId}</p>
          ) : null}
        </div>
      ) : (
        <p className="mt-4 text-sm text-slate-400">No decode result available yet.</p>
      )}
    </section>
  )
}
