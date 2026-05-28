import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import type { TrainingAssignmentDetailResponse } from '../api/types'
import { AssignmentMaterialDemandPanel } from './AssignmentMaterialDemandPanel'

const assignment: TrainingAssignmentDetailResponse = {
  assignmentId: 'asg-1',
  staffarrPersonId: 'person-1',
  trainingDefinitionId: 'def-1',
  trainingDefinitionName: 'Forklift refresher',
  trainingDefinitionKey: 'forklift_refresher',
  qualificationKey: 'forklift_ops',
  qualificationName: 'Forklift operator',
  staffarrIncidentRemediationId: null,
  sourceQualificationIssueId: null,
  assignmentReason: 'manual',
  status: 'assigned',
  dueAt: null,
  createdAt: '2026-05-27T00:00:00Z',
  assignedByUserId: 'user-1',
  blockerPublicationId: null,
  staffarrAcknowledgementRequestId: null,
  staffarrAcknowledgementStatus: null,
  staffarrAcknowledgementAt: null,
  staffarrAcknowledgementRequired: false,
  completedAt: null,
  completedByUserId: null,
  updatedAt: '2026-05-27T00:00:00Z',
  evidenceCount: 0,
  evaluation: null,
  signoffs: [],
  completionRequirementsMet: false,
  qualificationIssue: null,
}

describe('AssignmentMaterialDemandPanel', () => {
  it('renders empty state when no demand lines', () => {
    render(
      <AssignmentMaterialDemandPanel
        assignment={assignment}
        demandLines={[]}
        statusEvents={[]}
        canManage
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

    expect(screen.getByText(/No material demand lines yet/i)).toBeTruthy()
  })

  it('renders procurement badge and status timeline for published lines', () => {
    render(
      <AssignmentMaterialDemandPanel
        assignment={assignment}
        demandLines={[
          {
            demandLineId: 'line-1',
            lineNumber: 1,
            supplyarrPartId: null,
            partNumber: 'KIT-01',
            description: 'Training kit',
            quantityRequested: 2,
            unitOfMeasure: 'each',
            notes: '',
            status: 'published',
            trainarrPublicationId: 'pub-1',
            supplyarrDemandRefId: 'ref-11111111-1111-1111-1111-111111111111',
            publishedAt: '2026-05-27T01:00:00Z',
            procurementStatus: 'pr_submitted',
            supplyarrPurchaseRequestId: 'pr-1',
            supplyarrPurchaseOrderId: null,
            quantityReceived: 0,
            procurementStatusMessage: 'PR submitted',
            lastProcurementStatusAt: '2026-05-27T02:00:00Z',
            createdAt: '2026-05-27T00:00:00Z',
            updatedAt: '2026-05-27T02:00:00Z',
          },
        ]}
        statusEvents={[
          {
            statusEventId: 'evt-1',
            trainarrPublicationId: 'pub-1',
            supplyarrDemandRefId: 'ref-11111111-1111-1111-1111-111111111111',
            eventType: 'pr_submitted',
            procurementStatus: 'pr_submitted',
            supplyarrPurchaseRequestId: 'pr-1',
            supplyarrPurchaseOrderId: null,
            supplyarrReceivingReceiptId: null,
            message: 'PR submitted',
            occurredAt: '2026-05-27T02:00:00Z',
            createdAt: '2026-05-27T02:00:00Z',
          },
        ]}
        canManage={false}
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

    expect(screen.getByTestId('procurement-status-line-1').textContent).toContain('pr_submitted')
    expect(screen.getByTestId('material-demand-status-timeline')).toBeTruthy()
    expect(screen.getByTestId('material-demand-status-timeline').textContent).toContain('PR submitted')
  })
})
