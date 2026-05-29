import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import type { WorkOrderDetailResponse } from '../api/types'
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

describe('WorkOrderPartsDemandPanel', () => {
  it('renders empty state when no demand lines', () => {
    render(
      <WorkOrderPartsDemandPanel
        workOrder={workOrder}
        demandLines={[]}
        statusEvents={[]}
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
    expect(screen.getByTestId('parts-demand-status-timeline')).toBeInTheDocument()
    expect(screen.getAllByText(/PR submitted for approval/i).length).toBeGreaterThan(0)
  })
})
