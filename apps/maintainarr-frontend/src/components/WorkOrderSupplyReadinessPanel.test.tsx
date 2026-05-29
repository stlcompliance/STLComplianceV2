import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { WorkOrderSupplyReadinessPanel } from './WorkOrderSupplyReadinessPanel'

describe('WorkOrderSupplyReadinessPanel', () => {
  it('shows loading state', () => {
    render(<WorkOrderSupplyReadinessPanel readiness={null} isLoading />)
    expect(screen.getByTestId('work-order-supply-readiness-loading')).toBeInTheDocument()
  })

  it('renders overall and line readiness with blockers', () => {
    render(
      <WorkOrderSupplyReadinessPanel
        isLoading={false}
        readiness={{
          workOrderId: 'wo-1',
          workOrderNumber: 'WO-100',
          generatedAt: '2026-05-29T00:00:00Z',
          overallReadinessStatus: 'not_ready',
          totalDemandLines: 2,
          linesChecked: 1,
          linesReady: 0,
          linesBlocked: 1,
          linesSkipped: 1,
          lines: [
            {
              demandLineId: 'line-1',
              lineNumber: 1,
              supplyarrPartId: 'part-1',
              partNumber: 'BRK-001',
              quantityRequested: 2,
              lineStatus: 'pending',
              readinessStatus: 'not_ready',
              readinessBasis: 'availability',
              skipReason: null,
              quantityAvailable: 0,
              calculatedAt: '2026-05-29T00:00:00Z',
              blockers: [
                {
                  reasonCode: 'part_stockout',
                  message: 'Insufficient available quantity.',
                  sourceEntityType: 'part_stock',
                  sourceEntityId: 'part-1',
                  relatedEntityId: null,
                },
              ],
            },
            {
              demandLineId: 'line-2',
              lineNumber: 2,
              supplyarrPartId: null,
              partNumber: 'FREE-TEXT',
              quantityRequested: 1,
              lineStatus: 'pending',
              readinessStatus: null,
              readinessBasis: null,
              skipReason: 'missing_supplyarr_part_id',
              quantityAvailable: null,
              calculatedAt: null,
              blockers: [],
            },
          ],
        }}
      />,
    )

    expect(screen.getByTestId('work-order-supply-readiness-overall')).toHaveTextContent('Supply blocked')
    expect(screen.getByTestId('work-order-supply-readiness-line-line-1')).toHaveTextContent('part_stockout')
    expect(screen.getByTestId('work-order-supply-readiness-line-line-2')).toHaveTextContent('skipped')
  })
})
