import { MeterReadingsPanel } from '../../components/MeterReadingsPanel'
import { useLocation } from 'react-router-dom'
import { MeterProfile } from './MaintenanceDetailProfiles'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }
type MetersViewMode = 'drawer' | 'details' | 'create'

export function MetersSection({ state }: Props) {
  const s = state
  const location = useLocation()
  const mode: MetersViewMode = location.pathname.startsWith('/meters/create')
    ? 'create'
      : location.pathname.startsWith('/meters/details')
      ? 'details'
      : 'drawer'
  if (mode === 'details') {
    return <MeterProfile state={s} />
  }

  return (
    <div className="mb-8">
      {mode === 'create' ? (
        <div className="mb-4 rounded-xl border border-amber-700/50 bg-amber-950/20 p-4 text-sm text-amber-100">
          <ol className="list-decimal space-y-1 pl-5">
            <li>Step 1: Select the asset and define meter identity with unit and baseline reading.</li>
            <li>Step 2: Record the first operational reading to initialize trend and PM forecast context.</li>
            <li>Step 3: Continue recording readings so PM thresholds and due states stay accurate.</li>
          </ol>
        </div>
      ) : null}
      <MeterReadingsPanel
        mode={mode}
        canManageMeters={s.canManage}
        canRecordReadings={s.canExecuteInspections}
        assets={s.assetsQuery.data ?? []}
        meters={s.assetMetersQuery.data ?? []}
        readings={s.meterReadingsQuery.data ?? []}
        forecast={s.meterForecastQuery.data ?? null}
        selectedAssetId={s.meterAssetId}
        selectedMeterId={s.selectedMeterId}
        meterName={s.meterName}
        confirmedMeterKey={s.confirmedMeterKey}
        meterUnit={s.meterUnit}
        baselineReading={s.baselineReading}
        readingValue={s.readingValue}
        readingNotes={s.readingNotes}
        isLoading={s.assetMetersQuery.isLoading || s.meterReadingsQuery.isLoading}
        isCreatingMeter={s.createMeterMutation.isPending}
        isRecording={s.recordMeterReadingMutation.isPending}
        onSelectedAssetIdChange={(assetId) => {
          s.setMeterAssetId(assetId)
          s.setSelectedMeterId('')
        }}
        onSelectedMeterIdChange={s.setSelectedMeterId}
        onMeterNameChange={s.setMeterName}
        onMeterUnitChange={s.setMeterUnit}
        onBaselineReadingChange={s.setBaselineReading}
        onReadingValueChange={s.setReadingValue}
        onReadingNotesChange={s.setReadingNotes}
        onCreateMeter={() => s.createMeterMutation.mutate()}
        onRecordReading={() => s.recordMeterReadingMutation.mutate()}
      />
    </div>
  )
}
