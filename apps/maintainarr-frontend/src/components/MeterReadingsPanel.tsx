import { useMemo } from 'react'
import { buildSemanticKey, DetailBadge as Badge, StaticSearchPicker, type PickerOption } from '@stl/shared-ui'

import type { AssetMeterResponse, AssetResponse, MeterPmForecastResponse, MeterReadingResponse } from '../api/types'

interface MeterReadingsPanelProps {
  mode: 'drawer' | 'details' | 'create'
  canManageMeters: boolean
  canRecordReadings: boolean
  assets: AssetResponse[]
  meters: AssetMeterResponse[]
  readings: MeterReadingResponse[]
  forecast: MeterPmForecastResponse | null
  selectedAssetId: string
  selectedMeterId: string
  meterName: string
  confirmedMeterKey: string | null
  meterUnit: string
  baselineReading: string
  readingValue: string
  readingNotes: string
  isLoading: boolean
  isCreatingMeter: boolean
  isRecording: boolean
  onSelectedAssetIdChange: (assetId: string) => void
  onSelectedMeterIdChange: (meterId: string) => void
  onMeterNameChange: (value: string) => void
  onMeterUnitChange: (value: string) => void
  onBaselineReadingChange: (value: string) => void
  onReadingValueChange: (value: string) => void
  onReadingNotesChange: (value: string) => void
  onCreateMeter: () => void
  onRecordReading: () => void
}

export function MeterReadingsPanel({
  mode,
  canManageMeters,
  canRecordReadings,
  assets,
  meters,
  readings,
  forecast,
  selectedAssetId,
  selectedMeterId,
  meterName,
  confirmedMeterKey,
  meterUnit,
  baselineReading,
  readingValue,
  readingNotes,
  isLoading,
  isCreatingMeter,
  isRecording,
  onSelectedAssetIdChange,
  onSelectedMeterIdChange,
  onMeterNameChange,
  onMeterUnitChange,
  onBaselineReadingChange,
  onReadingValueChange,
  onReadingNotesChange,
  onCreateMeter,
  onRecordReading,
}: MeterReadingsPanelProps) {
  const selectedMeter = meters.find((m) => m.assetMeterId === selectedMeterId)
  const assetOptions = useMemo<PickerOption[]>(
    () =>
      assets.map((asset) => ({
        value: asset.assetId,
        label: `${asset.assetTag} — ${asset.name}`,
      })),
    [assets],
  )
  const meterOptions = useMemo<PickerOption[]>(
    () =>
      meters.map((meter) => ({
        value: meter.assetMeterId,
        label: `${meter.name} (${meter.currentReading} ${meter.unit})`,
      })),
    [meters],
  )
  const selectedAssetOption = useMemo<PickerOption | undefined>(
    () =>
      assetOptions.find((option) => option.value === selectedAssetId) ??
      (selectedAssetId ? { value: selectedAssetId, label: selectedAssetId } : undefined),
    [assetOptions, selectedAssetId],
  )
  const selectedMeterOption = useMemo<PickerOption | undefined>(
    () =>
      meterOptions.find((option) => option.value === selectedMeterId) ??
      (selectedMeterId ? { value: selectedMeterId, label: selectedMeterId } : undefined),
    [meterOptions, selectedMeterId],
  )
  const forecastVelocityLabel = forecast?.usageVelocityPerDay != null
    ? `${forecast.usageVelocityPerDay.toFixed(1)} ${forecast.unit}/day`
    : 'No trend yet'
  const forecastConfidenceLabel = `${Math.round(forecast?.confidenceScore ?? 0)}%`
  const forecastDueSoon = Boolean(forecast?.isDueSoon)
  const predictedDueAtLabel = forecast?.predictedDueAt ? new Date(forecast.predictedDueAt).toLocaleDateString() : 'Not predicted'
  const generatedMeterKey = buildSemanticKey({
    domain: 'asset',
    kind: 'meter',
    title: meterName,
    existingKeys: meters.map((meter) => meter.meterKey),
    maxLength: 128,
  })
  void confirmedMeterKey
  void generatedMeterKey

  return (
    <section className="rounded-xl border border-slate-700 bg-slate-900/60 p-6">
      <header className="mb-4">
        <h2 className="text-lg font-semibold text-white">Meter readings</h2>
        <p className="mt-1 text-sm text-slate-400">
          Capture usage readings per asset and forecast meter-linked preventive maintenance.
        </p>
      </header>

      {isLoading ? (
        <p className="text-sm text-slate-400">Loading meters…</p>
      ) : (
        <>
          <div className="mb-6 grid gap-4 md:grid-cols-2">
            <StaticSearchPicker
              id="meterreadings-asset"
              label="Asset"
              value={selectedAssetId}
              onChange={onSelectedAssetIdChange}
              options={assetOptions}
              selectedOption={selectedAssetOption}
              placeholder="Search assets…"
              testId="meterreadings-asset"
            />

            <StaticSearchPicker
              id="meterreadings-meter"
              label="Meter"
              value={selectedMeterId}
              onChange={onSelectedMeterIdChange}
              options={meterOptions}
              selectedOption={selectedMeterOption}
              placeholder="Search meters…"
              disabled={!selectedAssetId}
              testId="meterreadings-meter"
            />
          </div>

          {selectedMeter ? (
            <p className="mb-4 text-sm text-slate-300">
              Current reading:{' '}
              <span className="font-medium text-white">
                {selectedMeter.currentReading} {selectedMeter.unit}
              </span>
              {selectedMeter.lastReadingAt ? (
                <span className="text-slate-400">
                  {' '}
                  · last read {new Date(selectedMeter.lastReadingAt).toLocaleString()}
                </span>
              ) : null}
            </p>
          ) : null}

          {mode === 'create' && canManageMeters ? (
            <div className="mb-6 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
              <h3 className="text-sm font-medium text-slate-200">Define meter</h3>
              <div className="mt-3 grid gap-3 md:grid-cols-2">
                <div className="text-xs text-slate-400 md:col-span-2">Reference is auto-generated from meter name.</div>
                <input id="meterreadings-input-field-5"
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Display name"
                  value={meterName}
                  onChange={(e) => onMeterNameChange(e.target.value)}
                />
                <input id="meterreadings-input-field-4"
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Unit (hours, miles)"
                  value={meterUnit}
                  onChange={(e) => onMeterUnitChange(e.target.value)}
                />
                <input id="meterreadings-input-field-3"
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Baseline reading"
                  value={baselineReading}
                  onChange={(e) => onBaselineReadingChange(e.target.value)}
                />
              </div>
              <button
                type="button"
                className="mt-3 rounded-lg bg-sky-700 px-4 py-2 text-sm font-medium text-white hover:bg-sky-600 disabled:opacity-50"
                disabled={!selectedAssetId || isCreatingMeter}
                onClick={onCreateMeter}
              >
                {isCreatingMeter ? 'Creating…' : 'Create meter'}
              </button>
            </div>
          ) : null}

          {mode === 'create' && canRecordReadings ? (
            <div className="mb-6 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
              <h3 className="text-sm font-medium text-slate-200">Record reading</h3>
              <div className="mt-3 grid gap-3 md:grid-cols-2">
                <input id="meterreadings-input-field-2"
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Reading value"
                  value={readingValue}
                  onChange={(e) => onReadingValueChange(e.target.value)}
                />
                <input id="meterreadings-input-field"
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Notes (optional)"
                  value={readingNotes}
                  onChange={(e) => onReadingNotesChange(e.target.value)}
                />
              </div>
              <button
                type="button"
                className="mt-3 rounded-lg bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-600 disabled:opacity-50"
                disabled={!selectedMeterId || isRecording}
                onClick={onRecordReading}
              >
                {isRecording ? 'Recording…' : 'Record reading'}
              </button>
            </div>
          ) : null}

          {mode !== 'drawer' && forecast && forecast.linkedSchedules.length > 0 ? (
            <div className="mb-6 overflow-x-auto">
              <h3 className="mb-2 text-sm font-medium text-slate-200">PM usage forecast</h3>
              <div className="mb-3 grid gap-3 md:grid-cols-4">
                <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="text-xs text-slate-500">Usage velocity</p>
                  <p className="mt-1 text-sm font-medium text-slate-100">{forecastVelocityLabel}</p>
                </div>
                <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="text-xs text-slate-500">Predicted due</p>
                  <p className="mt-1 text-sm font-medium text-slate-100">{predictedDueAtLabel}</p>
                </div>
                <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="text-xs text-slate-500">Confidence</p>
                  <p className="mt-1 text-sm font-medium text-slate-100">{forecastConfidenceLabel}</p>
                </div>
                <div className="rounded-lg border border-slate-700 bg-slate-950/60 p-3">
                  <p className="text-xs text-slate-500">Due soon</p>
                  <div className="mt-1">
                    <Badge
                      label={forecastDueSoon ? 'Yes' : 'No'}
                      tone={forecastDueSoon ? 'warn' : 'good'}
                    />
                  </div>
                </div>
              </div>
              <table className="min-w-full text-left text-sm">
                <thead className="border-b border-slate-700 text-slate-400">
                  <tr>
                    <th className="px-3 py-2 font-medium">Schedule</th>
                    <th className="px-3 py-2 font-medium">Due status</th>
                    <th className="px-3 py-2 font-medium">Next due at usage</th>
                    <th className="px-3 py-2 font-medium">Until due</th>
                  </tr>
                </thead>
                <tbody>
                  {forecast.linkedSchedules.map((item) => (
                    <tr key={item.pmScheduleId} className="border-b border-slate-800 text-slate-200">
                      <td className="px-3 py-2">{item.name}</td>
                      <td className="px-3 py-2">
                        <span
                          className={
                            item.isDueFromUsage
                              ? 'rounded-full bg-amber-950 px-2 py-0.5 text-xs text-amber-200'
                              : 'text-slate-300'
                          }
                        >
                          {item.dueStatus}
                        </span>
                      </td>
                      <td className="px-3 py-2">
                        {item.nextDueAtUsage ?? '—'} {forecast.unit}
                      </td>
                      <td className="px-3 py-2">
                        {item.usageUntilDue ?? '—'} {forecast.unit}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}

          <div className="overflow-x-auto">
            <h3 className="mb-2 text-sm font-medium text-slate-200">Reading history</h3>
            {readings.length === 0 ? (
              <p className="text-sm text-slate-400">No readings recorded for this meter yet.</p>
            ) : (
              <table className="min-w-full text-left text-sm">
                <thead className="border-b border-slate-700 text-slate-400">
                  <tr>
                    <th className="px-3 py-2 font-medium">Read at</th>
                    <th className="px-3 py-2 font-medium">Value</th>
                    <th className="px-3 py-2 font-medium">Delta</th>
                    <th className="px-3 py-2 font-medium">Notes</th>
                  </tr>
                </thead>
                <tbody>
                  {readings.map((reading) => (
                    <tr key={reading.meterReadingId} className="border-b border-slate-800 text-slate-200">
                      <td className="px-3 py-2">{new Date(reading.readAt).toLocaleString()}</td>
                      <td className="px-3 py-2">{reading.readingValue}</td>
                      <td className="px-3 py-2">{reading.deltaFromPrevious}</td>
                      <td className="px-3 py-2 text-slate-400">{reading.notes || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </>
      )}
    </section>
  )
}
