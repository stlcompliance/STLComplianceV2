import { GeneratedKeyField, slugifyKey } from '@stl/shared-ui'

import type { AssetMeterResponse, AssetResponse, MeterPmForecastResponse, MeterReadingResponse } from '../api/types'

interface MeterReadingsPanelProps {
  canManageMeters: boolean
  canRecordReadings: boolean
  assets: AssetResponse[]
  meters: AssetMeterResponse[]
  readings: MeterReadingResponse[]
  forecast: MeterPmForecastResponse | null
  selectedAssetId: string
  selectedMeterId: string
  meterName: string
  meterKeyManualOverride: string
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
  onMeterKeyManualOverrideChange: (value: string) => void
  onMeterUnitChange: (value: string) => void
  onBaselineReadingChange: (value: string) => void
  onReadingValueChange: (value: string) => void
  onReadingNotesChange: (value: string) => void
  onCreateMeter: () => void
  onRecordReading: () => void
}

export function MeterReadingsPanel({
  canManageMeters,
  canRecordReadings,
  assets,
  meters,
  readings,
  forecast,
  selectedAssetId,
  selectedMeterId,
  meterName,
  meterKeyManualOverride,
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
  onMeterKeyManualOverrideChange,
  onMeterUnitChange,
  onBaselineReadingChange,
  onReadingValueChange,
  onReadingNotesChange,
  onCreateMeter,
  onRecordReading,
}: MeterReadingsPanelProps) {
  const selectedMeter = meters.find((m) => m.assetMeterId === selectedMeterId)

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
            <label className="block text-sm">
              <span className="text-slate-300">Asset</span>
              <select
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
                value={selectedAssetId}
                onChange={(e) => onSelectedAssetIdChange(e.target.value)}
              >
                <option value="">Select asset…</option>
                {assets.map((asset) => (
                  <option key={asset.assetId} value={asset.assetId}>
                    {asset.assetTag} — {asset.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="block text-sm">
              <span className="text-slate-300">Meter</span>
              <select
                className="mt-1 w-full rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-white"
                value={selectedMeterId}
                onChange={(e) => onSelectedMeterIdChange(e.target.value)}
                disabled={!selectedAssetId}
              >
                <option value="">Select meter…</option>
                {meters.map((meter) => (
                  <option key={meter.assetMeterId} value={meter.assetMeterId}>
                    {meter.meterKey} ({meter.currentReading} {meter.unit})
                  </option>
                ))}
              </select>
            </label>
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

          {canManageMeters ? (
            <div className="mb-6 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
              <h3 className="text-sm font-medium text-slate-200">Define meter</h3>
              <div className="mt-3 grid gap-3 md:grid-cols-2">
                <GeneratedKeyField
                  sourceLabel={meterName}
                  generatedKey={slugifyKey(meterName)}
                  confirmedKey={confirmedMeterKey}
                  manualOverride={meterKeyManualOverride}
                  onManualOverrideChange={onMeterKeyManualOverrideChange}
                  showAdvancedKey
                  label="Meter key"
                />
                <input
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Display name"
                  value={meterName}
                  onChange={(e) => onMeterNameChange(e.target.value)}
                />
                <input
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Unit (hours, miles)"
                  value={meterUnit}
                  onChange={(e) => onMeterUnitChange(e.target.value)}
                />
                <input
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

          {canRecordReadings ? (
            <div className="mb-6 rounded-lg border border-slate-700 bg-slate-950/50 p-4">
              <h3 className="text-sm font-medium text-slate-200">Record reading</h3>
              <div className="mt-3 grid gap-3 md:grid-cols-2">
                <input
                  className="rounded-lg border border-slate-600 bg-slate-950 px-3 py-2 text-sm text-white"
                  placeholder="Reading value"
                  value={readingValue}
                  onChange={(e) => onReadingValueChange(e.target.value)}
                />
                <input
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

          {forecast && forecast.linkedSchedules.length > 0 ? (
            <div className="mb-6 overflow-x-auto">
              <h3 className="mb-2 text-sm font-medium text-slate-200">PM usage forecast</h3>
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
