import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { MaintenanceHistoryPanel } from './MaintenanceHistoryPanel'

describe('MaintenanceHistoryPanel', () => {
  const assets = [
    {
      assetId: '11111111-1111-1111-1111-111111111111',
      assetTypeId: '22222222-2222-2222-2222-222222222222',
      typeKey: 'forklift',
      typeName: 'Forklift',
      classKey: 'vehicles',
      className: 'Vehicles',
      assetTag: 'FL-001',
      name: 'Yard Forklift',
      description: '',
      lifecycleStatus: 'active',
      siteRef: null,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    },
  ]

  it('renders timeline entries with category and event labels', () => {
    render(
      <MaintenanceHistoryPanel
        assets={assets}
        selectedAssetId="11111111-1111-1111-1111-111111111111"
        entries={[
          {
            entryId: 'inspection:1:completed',
            assetId: '11111111-1111-1111-1111-111111111111',
            category: 'inspection',
            eventType: 'inspection_completed',
            title: 'Inspection completed: Pre-Trip',
            detail: 'passed · completed',
            occurredAt: '2026-05-27T12:30:00Z',
            actorUserId: '44444444-4444-4444-4444-444444444444',
            sourceEntityType: 'inspection_run',
            sourceEntityId: '55555555-5555-5555-5555-555555555555',
            relatedEntityId: null,
          },
          {
            entryId: 'defect:2:reported',
            assetId: '11111111-1111-1111-1111-111111111111',
            category: 'defect',
            eventType: 'defect_reported',
            title: 'Defect reported: Hydraulic leak',
            detail: 'high · manual · open',
            occurredAt: '2026-05-27T13:00:00Z',
            actorUserId: '44444444-4444-4444-4444-444444444444',
            sourceEntityType: 'defect',
            sourceEntityId: '66666666-6666-6666-6666-666666666666',
            relatedEntityId: null,
          },
        ]}
        totalCount={2}
        isLoading={false}
        onSelectedAssetIdChange={() => undefined}
      />,
    )

    expect(screen.getByText('Maintenance history')).toBeTruthy()
    expect(screen.getByText('Inspection completed: Pre-Trip')).toBeTruthy()
    expect(screen.getByText('Inspection completed')).toBeTruthy()
    expect(screen.getByText('Defect reported: Hydraulic leak')).toBeTruthy()
    expect(screen.getByText('2 events total for FL-001')).toBeTruthy()
  })

  it('shows empty prompt when no asset is selected', () => {
    render(
      <MaintenanceHistoryPanel
        assets={assets}
        selectedAssetId=""
        entries={[]}
        totalCount={0}
        isLoading={false}
        onSelectedAssetIdChange={() => undefined}
      />,
    )

    expect(screen.getByText('Choose an asset to view its maintenance history.')).toBeTruthy()
  })

  it('shows empty state when asset has no events', () => {
    render(
      <MaintenanceHistoryPanel
        assets={assets}
        selectedAssetId="11111111-1111-1111-1111-111111111111"
        entries={[]}
        totalCount={0}
        isLoading={false}
        onSelectedAssetIdChange={() => undefined}
      />,
    )

    expect(screen.getByText('No maintenance events recorded yet for Yard Forklift.')).toBeTruthy()
  })
})
