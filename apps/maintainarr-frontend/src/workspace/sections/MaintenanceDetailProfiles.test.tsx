import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'

import { WorkOrderProfile } from './MaintenanceDetailProfiles'

describe('WorkOrderProfile', () => {
  it('renders supply readiness inside the work order detail profile', () => {
    const state = {
      selectedWorkOrderId: 'wo-1',
      workOrdersQuery: {
        data: [
          {
            workOrderId: 'wo-1',
            workOrderNumber: 'WO-100',
            assetTag: 'FLT-01',
            assetName: 'Forklift 01',
            title: 'Replace brake pads',
            priority: 'high',
            status: 'open',
            source: 'defect',
            assignedTechnicianPersonId: 'person-1',
            defectId: 'def-1',
            pmScheduleId: null,
            createdAt: '2026-05-29T00:00:00Z',
          },
        ],
      },
      workOrderDetailQuery: {
        data: null,
      },
      workOrderTasksQuery: {
        data: [
          {
            taskLineId: 'task-1',
            title: 'Inspect pads',
            status: 'completed',
          },
        ],
      },
      workOrderLaborQuery: {
        data: [],
      },
      workOrderEvidenceQuery: {
        data: [],
      },
      workOrderPartsDemandQuery: {
        data: [
          {
            demandLineId: 'demand-1',
          },
        ],
      },
      workOrderSupplyReadinessQuery: {
        isLoading: false,
        data: {
          workOrderId: 'wo-1',
          workOrderNumber: 'WO-100',
          generatedAt: '2026-05-29T00:00:00Z',
          overallReadinessStatus: 'not_ready',
          totalDemandLines: 1,
          linesChecked: 1,
          linesReady: 0,
          linesBlocked: 1,
          linesSkipped: 0,
          lines: [
            {
              demandLineId: 'demand-1',
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
          ],
        },
      },
    } as any

    render(
      <MemoryRouter>
        <WorkOrderProfile state={state} />
      </MemoryRouter>,
    )

    expect(screen.getByText('Supply readiness')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-supply-readiness-panel')).toBeInTheDocument()
    expect(screen.getByTestId('work-order-supply-readiness-overall')).toHaveTextContent('Supply blocked')
  })
})
