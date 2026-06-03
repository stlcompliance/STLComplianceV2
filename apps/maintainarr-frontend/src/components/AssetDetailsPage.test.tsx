import { MemoryRouter } from 'react-router-dom'
import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { AssetDetailsPage } from './AssetDetailsPage'

describe('AssetDetailsPage', () => {
  it('renders the readiness history rail', () => {
    render(
      <MemoryRouter>
        <AssetDetailsPage
          asset={{
            assetId: '11111111-1111-1111-1111-111111111111',
            assetTypeId: '22222222-2222-2222-2222-222222222222',
            typeKey: 'truck',
            typeName: 'Truck',
            classKey: 'vehicles',
            className: 'Vehicles',
            assetTag: 'TRK-01',
            name: 'Truck 01',
            description: 'Primary route truck',
            lifecycleStatus: 'active',
            siteRef: 'Yard A',
            createdAt: '2026-06-01T00:00:00Z',
            updatedAt: '2026-06-02T00:00:00Z',
          }}
          readiness={{
            assetId: '11111111-1111-1111-1111-111111111111',
            assetTag: 'TRK-01',
            assetName: 'Truck 01',
            lifecycleStatus: 'active',
            readinessStatus: 'ready',
            readinessBasis: 'maintenance_clear',
            calculatedAt: '2026-06-03T12:00:00Z',
            blockers: [],
            signals: {
              openCriticalDefectCount: 0,
              openHighDefectCount: 0,
              activeWorkOrderCount: 0,
              pmDueCount: 0,
              pmOverdueCount: 0,
              failedInspectionCount: 0,
            },
          }}
          isReadinessLoading={false}
          readinessHistory={{
            assetId: '11111111-1111-1111-1111-111111111111',
            assetTag: 'TRK-01',
            assetName: 'Truck 01',
            totalCount: 1,
            limit: 15,
            items: [
              {
                entryId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                statusFieldKey: 'readinessStatus',
                statusValueKey: 'ready',
                notes: 'Cleared after PM completion.',
                changedByPersonId: 'person-1',
                changedAt: '2026-06-03T10:00:00Z',
                createdAt: '2026-06-03T10:00:00Z',
              },
            ],
          }}
          isReadinessHistoryLoading={false}
          fieldContext={{
            assetId: '11111111-1111-1111-1111-111111111111',
            fields: [],
          }}
        />
      </MemoryRouter>,
    )

    expect(screen.getByText('Readiness history')).toBeInTheDocument()
    expect(screen.getByText('Readiness Status')).toBeInTheDocument()
    expect(screen.getByText('ready')).toBeInTheDocument()
    expect(screen.getByText('Cleared after PM completion.')).toBeInTheDocument()
  })
})
