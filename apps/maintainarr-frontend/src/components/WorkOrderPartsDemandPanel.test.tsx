import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import type { WorkOrderDetailResponse, WorkOrderSupplyReadinessResponse } from '../api/types'
import { WorkOrderPartsDemandPanel } from './WorkOrderPartsDemandPanel'

const workOrder: WorkOrderDetailResponse = {
  workOrderId: 'wo-1',
  workOrderNumber: 'WO-1001',
  assetId: 'asset-1',
  assetTag: 'TRK-01',
  assetName: 'Truck 01',
  defectId: null,
  defectTitle: null,
  pmScheduleId: null,
  pmScheduleName: null,
  title: 'Replace brake pads',
  description: 'Front axle service',
  priority: 'medium',
  status: 'open',
  source: 'manual',
  assignedTechnicianPersonId: null,
  createdByUserId: 'user-1',
  createdAt: '2026-05-27T00:00:00Z',
  updatedAt: '2026-05-27T00:00:00Z',
  startedAt: null,
  completedAt: null,
  cancelledAt: null,
}

const supplyReadiness: WorkOrderSupplyReadinessResponse = {
  workOrderId: 'wo-1',
  workOrderNumber: 'WO-1001',
  generatedAt: '2026-05-27T13:00:00Z',
  overallReadinessStatus: 'not_ready',
  totalDemandLines: 1,
  linesChecked: 1,
  linesReady: 0,
  linesBlocked: 1,
  linesSkipped: 0,
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
      calculatedAt: '2026-05-27T13:00:00Z',
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
}

describe('WorkOrderPartsDemandPanel', () => {
  it('renders empty state when no demand lines', () => {
    render(
      <WorkOrderPartsDemandPanel
        workOrder={workOrder}
        demandLines={[]}
        statusEvents={[]}
        supplyReadiness={null}
        canPerform
        partNumber=""
        supplyarrPartId=""
        quantityRequested=""
        unitOfMeasure="each"
        notes=""
        createPurchaseRequestDraft={false}
        onPartNumberChange={vi.fn()}
        onSupplyarrPartIdChange={vi.fn()}
        onQuantityRequestedChange={vi.fn()}
        onUnitOfMeasureChange={vi.fn()}
        onNotesChange={vi.fn()}
        onCreatePurchaseRequestDraftChange={vi.fn()}
        onAddDemandLine={vi.fn()}
        onPublishDemand={vi.fn()}
        isAdding={false}
        isPublishing={false}
      />,
    )

    expect(screen.getByText(/No parts demand lines yet/i)).toBeInTheDocument()
  })

  it('renders published line procurement badge and status timeline', () => {
    render(
      <WorkOrderPartsDemandPanel
        workOrder={workOrder}
        demandLines={[
          {
            demandLineId: 'line-1',
            lineNumber: 1,
            supplyarrPartId: 'part-1',
            partNumber: 'BRK-001',
            description: 'Brake pads',
            quantityRequested: 2,
            unitOfMeasure: 'each',
            notes: '',
            status: 'published',
            maintainarrPublicationId: 'pub-1',
            supplyarrDemandRefId: 'ref-1',
            publishedAt: '2026-05-27T12:00:00Z',
            procurementStatus: 'pr_submitted',
            supplyarrPurchaseRequestId: 'pr-1',
            supplyarrPurchaseOrderId: null,
            quantityReceived: 0,
            procurementStatusMessage: 'PR submitted for approval',
            lastProcurementStatusAt: '2026-05-27T13:00:00Z',
            createdAt: '2026-05-27T11:00:00Z',
          },
        ]}
        statusEvents={[
          {
            statusEventId: 'evt-1',
            maintainarrPublicationId: 'pub-1',
            supplyarrDemandRefId: 'ref-1',
            eventType: 'pr_submitted',
            procurementStatus: 'pr_submitted',
            supplyarrPurchaseRequestId: 'pr-1',
            supplyarrPurchaseOrderId: null,
            supplyarrReceivingReceiptId: null,
            message: 'PR submitted for approval',
            occurredAt: '2026-05-27T13:00:00Z',
            createdAt: '2026-05-27T13:00:00Z',
          },
        ]}
        supplyReadiness={supplyReadiness}
        canPerform
        partNumber=""
        supplyarrPartId=""
        quantityRequested=""
        unitOfMeasure="each"
        notes=""
        createPurchaseRequestDraft={false}
        onPartNumberChange={vi.fn()}
        onSupplyarrPartIdChange={vi.fn()}
        onQuantityRequestedChange={vi.fn()}
        onUnitOfMeasureChange={vi.fn()}
        onNotesChange={vi.fn()}
        onCreatePurchaseRequestDraftChange={vi.fn()}
        onAddDemandLine={vi.fn()}
        onPublishDemand={vi.fn()}
        isAdding={false}
        isPublishing={false}
      />,
    )

    expect(screen.getByTestId('procurement-status-line-1')).toHaveTextContent('pr_submitted')
    expect(screen.getByTestId('work-order-parts-demand-summary')).toHaveTextContent('Shortage procurement')
    expect(screen.getByTestId('work-order-parts-demand-summary')).toHaveTextContent('Supply readiness reports 1 blocked line')
    expect(screen.getByTestId('parts-demand-status-timeline')).toBeInTheDocument()
    expect(screen.getAllByText(/PR submitted for approval/i).length).toBeGreaterThan(0)
  })
})
