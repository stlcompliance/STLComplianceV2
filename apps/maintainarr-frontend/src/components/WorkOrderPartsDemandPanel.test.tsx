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
})
