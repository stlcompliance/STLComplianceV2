import { MeterReadingsPanel } from '../../components/MeterReadingsPanel'
import type { MaintainArrWorkspaceState } from '../useMaintainArrWorkspaceState'

type Props = { state: MaintainArrWorkspaceState }

export function MetersSection({ state }: Props) {
  const s = state
  return (
    <div className="mb-8">
      <MeterReadingsPanel
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
