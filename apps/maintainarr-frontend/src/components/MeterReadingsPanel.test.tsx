import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { MeterReadingsPanel } from './MeterReadingsPanel'

const sampleAsset = {
  assetId: '11111111-1111-1111-1111-111111111111',
  assetTypeId: '22222222-2222-2222-2222-222222222222',
  typeKey: 'forklift',
  typeName: 'Forklift',
  classKey: 'vehicles',
  className: 'Vehicles',
  assetTag: 'FL-100',
  name: 'Forklift 100',
  description: '',
  lifecycleStatus: 'active',
  siteRef: null,
  createdAt: '2026-05-27T00:00:00Z',
  updatedAt: '2026-05-27T00:00:00Z',
}

describe('MeterReadingsPanel', () => {
  it('renders meter capture and reading history', () => {
    render(
      <MeterReadingsPanel
        canManageMeters
        canRecordReadings
        assets={[sampleAsset]}
        meters={[
          {
            assetMeterId: '33333333-3333-3333-3333-333333333333',
            assetId: sampleAsset.assetId,
            assetTag: 'FL-100',
            assetName: 'Forklift 100',
            meterKey: 'engine-hours',
            name: 'Engine hours',
            description: '',
            unit: 'hours',
            baselineReading: 1000,
            currentReading: 1050,
            lastReadingAt: '2026-05-27T12:00:00Z',
            status: 'active',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T12:00:00Z',
          },
        ]}
        readings={[
          {
            meterReadingId: '44444444-4444-4444-4444-444444444444',
            assetMeterId: '33333333-3333-3333-3333-333333333333',
            assetId: sampleAsset.assetId,
            readingValue: 1050,
            deltaFromPrevious: 50,
            readAt: '2026-05-27T12:00:00Z',
            recordedByUserId: '55555555-5555-5555-5555-555555555555',
            notes: 'Monthly',
            isCorrection: false,
            createdAt: '2026-05-27T12:00:00Z',
          },
        ]}
        forecast={{
          assetMeterId: '33333333-3333-3333-3333-333333333333',
          meterKey: 'engine-hours',
          unit: 'hours',
          currentReading: 1050,
          linkedSchedules: [
            {
              pmScheduleId: '66666666-6666-6666-6666-666666666666',
              scheduleKey: 'oil-change',
              name: 'Oil change',
              scheduleMode: 'meter',
              dueStatus: 'due',
              nextDueAtUsage: 1100,
              intervalUsage: 100,
              currentMeterReading: 1050,
              usageUntilDue: 50,
              isDueFromUsage: false,
            },
          ],
        }}
        selectedAssetId={sampleAsset.assetId}
        selectedMeterId="33333333-3333-3333-3333-333333333333"
        meterName=""
        meterKeyManualOverride=""
        confirmedMeterKey={null}
        meterUnit="hours"
        baselineReading="1000"
        readingValue="1100"
        readingNotes=""
        isLoading={false}
        isCreatingMeter={false}
        isRecording={false}
        onSelectedAssetIdChange={vi.fn()}
        onSelectedMeterIdChange={vi.fn()}
        onMeterNameChange={vi.fn()}
        onMeterKeyManualOverrideChange={vi.fn()}
        onMeterUnitChange={vi.fn()}
        onBaselineReadingChange={vi.fn()}
        onReadingValueChange={vi.fn()}
        onReadingNotesChange={vi.fn()}
        onCreateMeter={vi.fn()}
        onRecordReading={vi.fn()}
      />,
    )

    expect(screen.getByText('Meter readings')).toBeInTheDocument()
    expect(screen.getByText('1050')).toBeInTheDocument()
    expect(screen.getByText('PM usage forecast')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Record reading' })).toBeInTheDocument()
  })

  it('shows empty reading history', () => {
    render(
      <MeterReadingsPanel
        canManageMeters={false}
        canRecordReadings
        assets={[sampleAsset]}
        meters={[]}
        readings={[]}
        forecast={null}
        selectedAssetId=""
        selectedMeterId=""
        meterName=""
        meterKeyManualOverride=""
        confirmedMeterKey={null}
        meterUnit=""
        baselineReading=""
        readingValue=""
        readingNotes=""
        isLoading={false}
        isCreatingMeter={false}
        isRecording={false}
        onSelectedAssetIdChange={vi.fn()}
        onSelectedMeterIdChange={vi.fn()}
        onMeterNameChange={vi.fn()}
        onMeterKeyManualOverrideChange={vi.fn()}
        onMeterUnitChange={vi.fn()}
        onBaselineReadingChange={vi.fn()}
        onReadingValueChange={vi.fn()}
        onReadingNotesChange={vi.fn()}
        onCreateMeter={vi.fn()}
        onRecordReading={vi.fn()}
      />,
    )

    expect(screen.getByText('No readings recorded for this meter yet.')).toBeInTheDocument()
  })
})
