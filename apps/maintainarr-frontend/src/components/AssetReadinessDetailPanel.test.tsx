import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { AssetReadinessDetailPanel } from './AssetReadinessDetailPanel'

describe('AssetReadinessDetailPanel', () => {
  it('prompts to select an asset when none selected', () => {
    render(
      <AssetReadinessDetailPanel readiness={null} isLoading={false} selectedAssetLabel={null} />,
    )

    expect(screen.getByTestId('asset-readiness-detail-empty')).toBeInTheDocument()
  })

  it('renders blockers and signal counts for selected asset', () => {
    render(
      <AssetReadinessDetailPanel
        isLoading={false}
        selectedAssetLabel="EX-1001"
        readiness={{
          assetId: '22222222-2222-2222-2222-222222222222',
          assetTag: 'EX-1001',
          assetName: 'Excavator 1001',
          lifecycleStatus: 'active',
          readinessStatus: 'not_ready',
          readinessBasis: 'maintenance_blockers',
          calculatedAt: '2026-05-29T12:00:00Z',
          blockers: [
            {
              blockerType: 'critical_defect',
              message: 'Critical defect open: Hydraulic leak',
              sourceEntityType: 'defect',
              sourceEntityId: '33333333-3333-3333-3333-333333333333',
              relatedEntityId: null,
            },
          ],
          signals: {
            openCriticalDefectCount: 1,
            openHighDefectCount: 0,
            activeWorkOrderCount: 1,
            pmDueCount: 0,
            pmOverdueCount: 0,
            failedInspectionCount: 0,
          },
        }}
      />,
    )

    expect(screen.getByTestId('asset-readiness-detail-content')).toBeInTheDocument()
    expect(screen.getByTestId('asset-readiness-detail-status')).toHaveTextContent(
      /not ready for dispatch/i,
    )
    expect(screen.getByText('Critical defect open: Hydraulic leak')).toBeInTheDocument()
    expect(screen.getByText('Active work orders')).toBeInTheDocument()
  })

  it('highlights AssurArr quality holds as the blocking reason', () => {
    render(
      <AssetReadinessDetailPanel
        isLoading={false}
        selectedAssetLabel="EX-1001"
        readiness={{
          assetId: '22222222-2222-2222-2222-222222222222',
          assetTag: 'EX-1001',
          assetName: 'Excavator 1001',
          lifecycleStatus: 'active',
          readinessStatus: 'not_ready',
          readinessBasis: 'maintenance_blockers',
          calculatedAt: '2026-05-29T12:00:00Z',
          blockers: [
            {
              blockerType: 'quality_hold',
              message: 'AssurArr hold requires a release review before return-to-service.',
              sourceEntityType: 'assurarr_hold',
              sourceEntityId: '44444444-4444-4444-4444-444444444444',
              relatedEntityId: '55555555-5555-5555-5555-555555555555',
            },
          ],
          signals: {
            openCriticalDefectCount: 0,
            openHighDefectCount: 0,
            activeWorkOrderCount: 0,
            pmDueCount: 0,
            pmOverdueCount: 0,
            failedInspectionCount: 0,
          },
        }}
      />,
    )

    expect(screen.getByTestId('asset-readiness-hold-summary')).toBeInTheDocument()
    expect(screen.getByTestId('asset-readiness-hold-summary')).toHaveTextContent('Quality hold from AssurArr')
    expect(screen.getByTestId('asset-readiness-hold-summary')).toHaveTextContent(/active quality hold/i)
    expect(screen.getByTestId('asset-readiness-hold-summary')).toHaveTextContent(/AssurArr hold requires a release review/i)
    expect(screen.getByTestId('asset-readiness-hold-summary')).toHaveTextContent(/Hold scope: assurarr_hold · 55555555…/i)
  })
})
